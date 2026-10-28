#nullable enable

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Open.IdentityServer.Services;
using Xunit;

namespace Open.IdentityServer.IntegrationTests;

public class SessionCleanupServerSideSessionTests : ServerSideSessionTests
{
    private const string Category = nameof(SessionCleanupServerSideSessionTests);

    [Fact]
    [Trait("Category", Category)]
    public async Task RemoveExpiredServerSideSessions_ShouldSendBackChannelLogoutWithSessionIssuer()
    {
        _mockPipeline.Clients.Single(x => x.ClientId == "client1").BackChannelLogoutUri = "https://client1/backchannel";

        // assert outside OnInvoke: BackChannelLogoutHttpClient swallows exceptions thrown by the handler
        string? form = null;
        _mockPipeline.BackChannelMessageHandler.OnInvoke = async req => form = await req.Content!.ReadAsStringAsync();

        await _mockPipeline.LoginAsync("bob");
        await AuthCodeRequest("client1", "openid", "https://client1/callback");

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(7));

        // no HttpContext here, as in the session cleanup hosted service
        using (var scope = _mockPipeline.Server!.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ISessionCleanupService>().RemoveExpiredServerSideSessionsAsync();
        }

        form.Should().StartWith(OidcConstants.BackChannelLogoutRequest.LogoutToken + "=");
        var token = new JwtSecurityTokenHandler().ReadJwtToken(form!.Split('=')[1]);
        token.Issuer.Should().Be(IdentityServerPipeline.BaseUrl);
    }
}
