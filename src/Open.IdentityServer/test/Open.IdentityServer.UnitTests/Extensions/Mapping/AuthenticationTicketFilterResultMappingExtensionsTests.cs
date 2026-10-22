using AwesomeAssertions;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Extensions.Mapping;
using Open.IdentityServer.UnitTests.Utilities.Generators;
using Xunit;

namespace Open.IdentityServer.UnitTests.Extensions.Mapping;

public class AuthenticationTicketFilterResultMappingExtensionsTests
{
    [Fact]
    public void ToUserSession_ShouldMapAuthTicketResultToUserSession()
    {
        var sut = ServerSessionTestGenerators.GenerateAuthenticationTicketFilterResult("sess1", "SchemeA", "bob", "session-0001",
            "Robert");

        var actualSession = sut.ToUserSession();

        actualSession.SubjectId.Should().Be(sut.SubjectId);
        actualSession.SessionId.Should().Be(sut.SessionId);
        actualSession.DisplayName.Should().Be(sut.DisplayName);
        actualSession.Created.Should().Be(sut.Created);
        actualSession.Renewed.Should().Be(sut.Renewed);
        actualSession.Expires.Should().Be(sut.Expires);
        actualSession.AuthenticationTicket.Should().BeEquivalentTo(sut.AuthenticationTicket);
        actualSession.Issuer.Should().BeNullOrWhiteSpace();
        actualSession.ClientIds.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public void ToUserSession_WhenClientIdsSet_ShouldExtractClientIdsInResult()
    {
        var sut = ServerSessionTestGenerators.GenerateAuthenticationTicketFilterResult("sess1", "SchemeA", "bob", "session-0001",
            "Robert", clientIds: ["clientA"]);

        var actualSession = sut.ToUserSession();

        actualSession.SubjectId.Should().Be(sut.SubjectId);
        actualSession.SessionId.Should().Be(sut.SessionId);
        actualSession.DisplayName.Should().Be(sut.DisplayName);
        actualSession.Created.Should().Be(sut.Created);
        actualSession.Renewed.Should().Be(sut.Renewed);
        actualSession.Expires.Should().Be(sut.Expires);
        actualSession.Issuer.Should().BeNullOrWhiteSpace();
        actualSession.AuthenticationTicket.Should().BeEquivalentTo(sut.AuthenticationTicket);
        actualSession.ClientIds.Should().BeEquivalentTo(sut.AuthenticationTicket!.Properties.GetClientList());
    }
    
    [Fact]
    public void ToUserSession_WhenIssuerSet_ShouldExtractIssuerInResult()
    {
        string fakeIssuer = "https://fakeissuer.com";
        
        var sut = ServerSessionTestGenerators.GenerateAuthenticationTicketFilterResult("sess1", "SchemeA", "bob", "session-0001",
            "Robert", clientIds: ["clientA"], issuer: fakeIssuer);

        var actualSession = sut.ToUserSession();

        actualSession.SubjectId.Should().Be(sut.SubjectId);
        actualSession.SessionId.Should().Be(sut.SessionId);
        actualSession.DisplayName.Should().Be(sut.DisplayName);
        actualSession.Created.Should().Be(sut.Created);
        actualSession.Renewed.Should().Be(sut.Renewed);
        actualSession.Expires.Should().Be(sut.Expires);
        actualSession.Issuer.Should().Be(fakeIssuer);
        actualSession.AuthenticationTicket.Should().BeEquivalentTo(sut.AuthenticationTicket);
        actualSession.ClientIds.Should().BeEquivalentTo(sut.AuthenticationTicket!.Properties.GetClientList());
    }
}