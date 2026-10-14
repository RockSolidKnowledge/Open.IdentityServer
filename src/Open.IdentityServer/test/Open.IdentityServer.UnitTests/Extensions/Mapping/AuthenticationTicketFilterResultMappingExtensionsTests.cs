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

        actualSession.SubjectId.Should().Be(sut.Session.SubjectId);
        actualSession.SessionId.Should().Be(sut.Session.SessionId);
        actualSession.DisplayName.Should().Be(sut.Session.DisplayName);
        actualSession.Created.Should().Be(sut.Session.Created);
        actualSession.Renewed.Should().Be(sut.Session.Renewed);
        actualSession.Expires.Should().Be(sut.Session.Expires);
        actualSession.AuthenticationTicket.Should().BeEquivalentTo(sut.AuthTicket);
        actualSession.Issuer.Should().BeNullOrWhiteSpace();
        actualSession.ClientIds.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public void ToUserSession_WhenClientIdsSet_ShouldExtractClientIdsInResult()
    {
        var sut = ServerSessionTestGenerators.GenerateAuthenticationTicketFilterResult("sess1", "SchemeA", "bob", "session-0001",
            "Robert", clientIds: ["clientA"]);

        var actualSession = sut.ToUserSession();

        actualSession.SubjectId.Should().Be(sut.Session.SubjectId);
        actualSession.SessionId.Should().Be(sut.Session.SessionId);
        actualSession.DisplayName.Should().Be(sut.Session.DisplayName);
        actualSession.Created.Should().Be(sut.Session.Created);
        actualSession.Renewed.Should().Be(sut.Session.Renewed);
        actualSession.Expires.Should().Be(sut.Session.Expires);
        actualSession.Issuer.Should().BeNullOrWhiteSpace();
        actualSession.AuthenticationTicket.Should().BeEquivalentTo(sut.AuthTicket);
        actualSession.ClientIds.Should().BeEquivalentTo(sut.AuthTicket!.Properties.GetClientList());
    }
    
    [Fact]
    public void ToUserSession_WhenIssuerSet_ShouldExtractIssuerInResult()
    {
        string fakeIssuer = "https://fakeissuer.com";
        
        var sut = ServerSessionTestGenerators.GenerateAuthenticationTicketFilterResult("sess1", "SchemeA", "bob", "session-0001",
            "Robert", clientIds: ["clientA"], issuer: fakeIssuer);

        var actualSession = sut.ToUserSession();

        actualSession.SubjectId.Should().Be(sut.Session.SubjectId);
        actualSession.SessionId.Should().Be(sut.Session.SessionId);
        actualSession.DisplayName.Should().Be(sut.Session.DisplayName);
        actualSession.Created.Should().Be(sut.Session.Created);
        actualSession.Renewed.Should().Be(sut.Session.Renewed);
        actualSession.Expires.Should().Be(sut.Session.Expires);
        actualSession.Issuer.Should().Be(fakeIssuer);
        actualSession.AuthenticationTicket.Should().BeEquivalentTo(sut.AuthTicket);
        actualSession.ClientIds.Should().BeEquivalentTo(sut.AuthTicket!.Properties.GetClientList());
    }
}