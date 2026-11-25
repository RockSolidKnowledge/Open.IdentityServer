#nullable enable

using System;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Stores.Serialization;
using Open.IdentityServer.Test.Utilities;
using Open.IdentityServer.UnitTests.Utilities.Generators;
using Xunit;

namespace Open.IdentityServer.UnitTests.DataProtection;

public class AuthenticationTicketSerializerTests
{
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly MockDataProtector dataProtector = new();
    private readonly MockLogger<AuthenticationTicketSerializer> logger = new();

    public AuthenticationTicketSerializerTests()
    {
        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector(DataProtectionConstants.ServerSideTicketStorePurpose))
            .Returns(dataProtector);
    }

    private AuthenticationTicketSerializer CreateSut() => new(dataProtectionProvider, logger);
    
    [Fact]
    public async Task Serialize_ShouldSerializeAuthTicketAndDataProtectItWrappingInProtectedDataContainer()
    {
        const string authScheme = "FakeAuthScheme";
        string subjectId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();
        const string displayName = "Fake User";
        DateTime issuedUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);
        DateTime expiresUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);
        string fakeIssuer = "https://fake.issuer.com";
        
        AuthenticationTicket authenticationTicket =
            ServerSessionTestGenerators.GenerateAuthenticationTicket(authScheme, subjectId, sessionId, displayName, issuedUtc, expiresUtc, issuer: fakeIssuer);
        
        AuthenticationTicketSerializer sut = CreateSut();

        var actualData = sut.Serialize(authenticationTicket);
        
        var jsonElement = JsonElement.Parse(actualData);

        jsonElement.GetProperty("Version").GetInt32().Should().Be(1);
        var actualPayload = jsonElement.GetProperty("Payload").GetString();
        actualPayload.Should().NotBeNull();

        var expectedAuthTicket = authenticationTicket.Clone();
        expectedAuthTicket.Properties.Items[JwtClaimTypes.Issuer] = fakeIssuer;
        
        string expectedJson = JsonSerializer.Serialize(expectedAuthTicket.ToSerializableObj(),
            AuthenticationTicketSerializer.JsonSettings);
        dataProtector.ValidateProtectedData(actualPayload, expectedJson);
    }

    [Theory]
    [InlineData("{invalid.json}")]
    [InlineData("{\"Version\": 2, Payload: \"SOMEDATA\"}")]
    public async Task Deserialize_WhenSessionDataDeserialisationFails_ShouldReturnNull(string data)
    {
        AuthenticationTicketSerializer sut = CreateSut();
        
        AuthenticationTicket? actual = sut.Deserialize(data);

        actual.Should().BeNull();
    }

    [Fact]
    public async Task Deserialize_WhenUnprotectPayloadFails_ShouldReturnNull()
    {
        const string authScheme = "FakeAuthScheme";
        string subjectId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();
        const string displayName = "Fake User";
        DateTime issuedUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);
        DateTime expiresUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);
        string fakeIssuer = "https://fake.issuer.com";
        
        SerializedAuthenticationTicket authenticationTicket = ServerSessionTestGenerators.GenerateSerializedAuthenticationTicket(authScheme, subjectId, sessionId, displayName, issuedUtc, expiresUtc, issuer: fakeIssuer);

        Mock.Get(dataProtector.dataProtector)
            .Setup(x => x.Unprotect(It.IsAny<byte[]>()))
            .Throws(new Exception("Fake Error"));
        AuthenticationTicketSerializer sut = CreateSut();

        string fakeProtectedData = GenerateFakeProtectedData(authenticationTicket);
        AuthenticationTicket? actual = sut.Deserialize(fakeProtectedData);

        actual.Should().BeNull();
    }

    [Theory]
    [InlineData("{invalid.json}")]
    public async Task Deserialize_WhenUnprotectedPayloadIsInvalidJson_ShouldReturnNull(string invalidJson)
    {
        Mock.Get(dataProtector.dataProtector)
            .Setup(x => x.Unprotect(It.IsAny<byte[]>()))
            .Throws(new Exception("Fake Error"));
        AuthenticationTicketSerializer sut = CreateSut();

        string fakeProtectedData = GenerateFakeProtectedData(invalidJson);
        AuthenticationTicket? actual = sut.Deserialize(fakeProtectedData);

        actual.Should().BeNull();
    }

    [Fact]
    public async Task Deserialize_WhenValidProtectedData_ShouldSuccessfullyUnprotectAndDeserialize()
    {
        const string authScheme = "FakeAuthScheme";
        string subjectId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();
        const string displayName = "Fake User";
        DateTime issuedUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);
        DateTime expiresUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);
        string fakeIssuer = "https://fake.issuer.com";
        
        SerializedAuthenticationTicket authenticationTicket = ServerSessionTestGenerators.GenerateSerializedAuthenticationTicket(authScheme, subjectId, sessionId, displayName, issuedUtc, expiresUtc, issuer: fakeIssuer);
        
        AuthenticationTicketSerializer sut = CreateSut();

        string fakeProtectedData = GenerateFakeProtectedData(authenticationTicket);
        AuthenticationTicket? actual = sut.Deserialize(fakeProtectedData);
        
        actual.Should().BeOfType<AuthenticationTicket>();
        actual.AuthenticationScheme.Should().Be(authScheme);
        actual.Principal.Identity.Should().NotBeNull();
        actual.Principal.Identity?.AuthenticationType.Should()
            .BeEquivalentTo(authenticationTicket.User.AuthenticationType);
        actual.Principal.Identity?.Name.Should().BeEquivalentTo(displayName);
        actual.Principal.Identities.Should().Contain(x => 
            x.NameClaimType == JwtClaimTypes.Name && 
            x.RoleClaimType == JwtClaimTypes.Role);
        actual.Properties.Items.Should().BeEquivalentTo(authenticationTicket.Items);
    }

    private string GenerateFakeProtectedData(SerializedAuthenticationTicket serializedAuthenticationTicket)
    {
        return GenerateFakeProtectedData(JsonSerializer.Serialize(serializedAuthenticationTicket,
            AuthenticationTicketSerializer.JsonSettings));
    }

    private string GenerateFakeProtectedData(string serializedAuthenticationTicket)
    {
        DataProtectedSessionData sessionData = new DataProtectedSessionData
        {
            Payload = dataProtector.GenerateFakeProtectedData(serializedAuthenticationTicket)
        };

        return JsonSerializer.Serialize(sessionData, AuthenticationTicketSerializer.JsonSettings);
    }
}