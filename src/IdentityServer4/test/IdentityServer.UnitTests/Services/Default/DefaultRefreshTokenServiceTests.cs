using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using IdentityServer4.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default
{
    public class DefaultRefreshTokenServiceTests
    {
        private DefaultRefreshTokenService _subject;
        private DefaultRefreshTokenStore _store;

        private ClaimsPrincipal _user = new IdentityServerUser("123").CreatePrincipal();
        private StubClock _clock = new StubClock();

        public DefaultRefreshTokenServiceTests()
        {
            _store = new DefaultRefreshTokenStore(
                new InMemoryPersistedGrantStore(),
                new PersistentGrantSerializer(),
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultRefreshTokenStore>());

            _subject = new DefaultRefreshTokenService(
                _clock,
                _store,
                TestLogger.Create<DefaultRefreshTokenService>());
        }

        [Fact]
        public async Task CreateRefreshToken_token_exists_in_store()
        {
            var client = new Client();
            var accessToken = new Token();

            var handle = await _subject.CreateRefreshTokenAsync(_user, accessToken, client);

            (await _store.GetRefreshTokenAsync(handle)).Should().NotBeNull();
        }

        [Fact]
        public async Task CreateRefreshToken_should_match_absolute_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                AbsoluteRefreshTokenLifetime = 10
            };

            var handle = await _subject.CreateRefreshTokenAsync(_user, new Token(), client);

            var refreshToken = (await _store.GetRefreshTokenAsync(handle));

            refreshToken.Should().NotBeNull();
            refreshToken.Lifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
        }

        [Fact]
        public async Task CreateRefreshToken_should_match_sliding_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10
            };

            var handle = await _subject.CreateRefreshTokenAsync(_user, new Token(), client);

            var refreshToken = (await _store.GetRefreshTokenAsync(handle));

            refreshToken.Should().NotBeNull();
            refreshToken.Lifetime.Should().Be(client.SlidingRefreshTokenLifetime);
        }

        [Fact]
        public async Task UpdateRefreshToken_one_time_use_should_create_new_token()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.OneTimeOnly
            };

            var refreshToken = new RefreshToken
            {
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            };

            var handle = await _store.StoreRefreshTokenAsync(refreshToken);

            (await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client))
                .Should().NotBeNull()
                .And
                .NotBe(handle);
        }

        [Fact]
        public async Task UpdateRefreshToken_sliding_with_non_zero_absolute_should_update_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10,
                AbsoluteRefreshTokenLifetime = 100
            };

            var now = DateTime.UtcNow;
            _clock.UtcNowFunc = () => now;

            var handle = await _store.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = now.AddSeconds(-10),
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            });

            var refreshToken = await _store.GetRefreshTokenAsync(handle);
            var newHandle = await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client);

            newHandle.Should().NotBeNull().And.Be(handle);

            var newRefreshToken = await _store.GetRefreshTokenAsync(newHandle);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken.Lifetime.Should().Be((int)(now - newRefreshToken.CreationTime).TotalSeconds + client.SlidingRefreshTokenLifetime);
        }

        [Fact]
        public async Task UpdateRefreshToken_lifetime_exceeds_absolute_should_be_absolute_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10,
                AbsoluteRefreshTokenLifetime = 1000
            };

            var now = DateTime.UtcNow;
            _clock.UtcNowFunc = () => now;

            var handle = await _store.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = now.AddSeconds(-1000),
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            });

            var refreshToken = await _store.GetRefreshTokenAsync(handle);
            var newHandle = await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client);

            newHandle.Should().NotBeNull().And.Be(handle);

            var newRefreshToken = await _store.GetRefreshTokenAsync(newHandle);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken.Lifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
        }

        [Fact]
        public async Task UpdateRefreshToken_sliding_with_zero_absolute_should_update_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10,
                AbsoluteRefreshTokenLifetime = 0
            };

            var now = DateTime.UtcNow;
            _clock.UtcNowFunc = () => now;

            var handle = await _store.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = now.AddSeconds(-1000),
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            });

            var refreshToken = await _store.GetRefreshTokenAsync(handle);
            var newHandle = await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client);

            newHandle.Should().NotBeNull().And.Be(handle);

            var newRefreshToken = await _store.GetRefreshTokenAsync(newHandle);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken.Lifetime.Should().Be((int)(now - newRefreshToken.CreationTime).TotalSeconds + client.SlidingRefreshTokenLifetime);
        }

        [Fact]
        public async Task UpdateRefreshToken_for_onetime_and_sliding_with_zero_absolute_should_update_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10,
                AbsoluteRefreshTokenLifetime = 0
            };

            var now = DateTime.UtcNow;
            _clock.UtcNowFunc = () => now;

            var handle = await _store.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = now.AddSeconds(-1000),
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            });

            var refreshToken = await _store.GetRefreshTokenAsync(handle);
            var newHandle = await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client);

            newHandle.Should().NotBeNull().And.NotBe(handle);

            var newRefreshToken = await _store.GetRefreshTokenAsync(newHandle);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken.Lifetime.Should().Be((int)(now - newRefreshToken.CreationTime).TotalSeconds + client.SlidingRefreshTokenLifetime);
        }
    }
}
