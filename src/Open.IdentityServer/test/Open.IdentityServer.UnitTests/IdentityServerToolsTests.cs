using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Xunit;

namespace Open.IdentityServer.UnitTests;

public class IdentityServerToolsTests
{
    private readonly Mock<ITokenCreationService> _tokenCreationService = new();

    private IdentityServerTools CreateSut()
    {
        return new IdentityServerTools(new MockHttpContextAccessor(), _tokenCreationService.Object, new FakeTimeProvider());
    }

    [Fact]
    public async Task IssueJwtAsync_WhenCalledWithClaimsContainingAudience_ShouldSetAudienceOnToken()
    {
        var audience = "audience";

        var sut = CreateSut();

        await sut.IssueJwtAsync(3600, "https://localhost:5000",
            new List<Claim>
            {
                new(JwtClaimTypes.Audience, audience),
                new(JwtClaimTypes.GivenName, "Joe")
            });

        _tokenCreationService.Verify(x => x.CreateTokenAsync(It.Is<Token>(t => 
                t.Audiences.Count == 1 &&
                t.Audiences.Contains(audience))),
            Times.Once);
    }

    [Fact]
    public async Task IssueJwtAsync_WhenCalledWithMultipleClaimsContainingAudience_ShouldSetAllAudiencesOnToken()
    {
        var audience1 = "audience1";
        var audience2 = "audience2";
        var audience3 = "audience3";

        var sut = CreateSut();

        await sut.IssueJwtAsync(3600, "https://localhost:5000", new List<Claim>
        {
            new(JwtClaimTypes.Audience, audience1),
            new(JwtClaimTypes.Audience, audience2),
            new(JwtClaimTypes.Audience, audience3),
            new(JwtClaimTypes.GivenName, "Joe")
        });

        _tokenCreationService.Verify(x => x.CreateTokenAsync(It.Is<Token>(t => 
                    t.Audiences.Count == 3 &&
                    t.Audiences.Contains(audience1) &&
                    t.Audiences.Contains(audience2) && 
                    t.Audiences.Contains(audience3))),
            Times.Once);
    }
}