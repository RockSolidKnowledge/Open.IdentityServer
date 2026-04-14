// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.UnitTests.Common;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Stores.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default
{
    public class DefaultPersistedGrantServiceTests
    {
        private DefaultPersistedGrantService _subject;
        private InMemoryPersistedGrantStore _store = new InMemoryPersistedGrantStore();
        private IAuthorizationCodeStore _codes;
        private IRefreshTokenStore _refreshTokens;
        private IReferenceTokenStore _referenceTokens;
        private IUserConsentStore _userConsent;

        private IPersistentGrantSerializer _persistentGrantSerializer = new PersistentGrantSerializer();
        private ILogger<DefaultPersistedGrantService> _logger = Mock.Of<ILogger<DefaultPersistedGrantService>>();
        
        private ClaimsPrincipal _user = new IdentityServerUser("123").CreatePrincipal();

        public DefaultPersistedGrantServiceTests()
        {
            _subject = new DefaultPersistedGrantService(_store, _persistentGrantSerializer, _logger);
            _codes = new DefaultAuthorizationCodeStore(_store,
                _persistentGrantSerializer, 
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultAuthorizationCodeStore>());
            _refreshTokens = new DefaultRefreshTokenStore(_store,
                _persistentGrantSerializer, 
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultRefreshTokenStore>());
            _referenceTokens = new DefaultReferenceTokenStore(_store,
                _persistentGrantSerializer, 
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultReferenceTokenStore>());
            _userConsent = new DefaultUserConsentStore(_store,
                _persistentGrantSerializer, 
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultUserConsentStore>());
        }

        [Fact]
        public async Task GetAllGrantsAsync_should_return_all_grants()
        {
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                CreationTime = DateTime.UtcNow,
                ClientId = "client1",
                SubjectId = "123",
                Scopes = new string[] { "foo1", "foo2" }
            });
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                CreationTime = DateTime.UtcNow,
                ClientId = "client2",
                SubjectId = "123",
                Scopes = new string[] { "foo3" }
            });
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                CreationTime = DateTime.UtcNow,
                ClientId = "client1",
                SubjectId = "456",
                Scopes = new string[] { "foo3" }
            });

            var handle1 = await _referenceTokens.StoreReferenceTokenAsync(new Token
            {
                ClientId = "client1",
                Audiences = { "aud" },
                CreationTime = DateTime.UtcNow,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar1"),
                    new Claim("scope", "bar2")
                }
            });

            var handle2 = await _referenceTokens.StoreReferenceTokenAsync(new Token
            {
                ClientId = "client2",
                Audiences = { "aud" },
                CreationTime = DateTime.UtcNow,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar3")
                }
            });

            var handle3 = await _referenceTokens.StoreReferenceTokenAsync(new Token
            {
                ClientId = "client1",
                Audiences = { "aud" },
                CreationTime = DateTime.UtcNow,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "456"),
                    new Claim("scope", "bar3")
                }
            });

            var handle4 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                ClientId = "client1",
                AuthorizedScopes = ["baz1", "baz2"],
                Version = 5
            });
            var handle5 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("456").CreatePrincipal(),
                ClientId = "client1",
                AuthorizedScopes = ["baz3"],
                Version = 5
            });
            var handle6 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                ClientId = "client2",
                AuthorizedScopes = ["baz3"],
                Version = 5
            });

            var handle7 = await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "client1",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux1", "quux2" }
            });

            var handle8 = await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "client2",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });

            var handle9 = await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "client1",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("456").CreatePrincipal(),
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });

            var grants = await _subject.GetAllGrantsAsync("123");

            grants.Count().Should().Be(2);
            var grant1 = grants.First(x => x.ClientId == "client1");
            grant1.SubjectId.Should().Be("123");
            grant1.ClientId.Should().Be("client1");
            grant1.Scopes.Should().BeEquivalentTo(new string[] { "foo1", "foo2", "bar1", "bar2", "baz1", "baz2", "quux1", "quux2" });

            var grant2 = grants.First(x => x.ClientId == "client2");
            grant2.SubjectId.Should().Be("123");
            grant2.ClientId.Should().Be("client2");
            grant2.Scopes.Should().BeEquivalentTo(new string[] { "foo3", "bar3", "baz3", "quux3" });
        }

        [Fact]
        public async Task RemoveAllGrantsAsync_should_remove_all_grants()
        {
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                ClientId = "client1",
                SubjectId = "123",
                Scopes = new string[] { "foo1", "foo2" }
            });
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                ClientId = "client2",
                SubjectId = "123",
                Scopes = new string[] { "foo3" }
            });
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                ClientId = "client1",
                SubjectId = "456",
                Scopes = new string[] { "foo3" }
            });

            var handle1 = await _referenceTokens.StoreReferenceTokenAsync(new Token
            {
                ClientId = "client1",
                Audiences = { "aud" },
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar1"),
                    new Claim("scope", "bar2")
                }
            });

            var handle2 = await _referenceTokens.StoreReferenceTokenAsync(new Token
            {
                ClientId = "client2",
                Audiences = { "aud" },
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar3")
                }
            });

            var handle3 = await _referenceTokens.StoreReferenceTokenAsync(new Token
            {
                ClientId = "client1",
                Audiences = { "aud" },
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "456"),
                    new Claim("scope", "bar3")
                }
            });

            var handle4 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                ClientId = "client1",
                AuthorizedScopes = ["baz1", "baz2"],
                Version = 5
            });
            var handle5 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("456").CreatePrincipal(),
                ClientId = "client1",
                AuthorizedScopes = ["baz3"],
                Version = 5
            });
            var handle6 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                ClientId = "client2",
                AuthorizedScopes = ["baz3"],
                Version = 5
            });

            var handle7 = await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "client1",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux1", "quux2" }
            });

            var handle8 = await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "client2",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });

            var handle9 = await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "client1",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("456").CreatePrincipal(),
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });

            await _subject.RemoveAllGrantsAsync("123", "client1");

            (await _referenceTokens.GetReferenceTokenAsync(handle1)).Should().BeNull();
            (await _referenceTokens.GetReferenceTokenAsync(handle2)).Should().NotBeNull();
            (await _referenceTokens.GetReferenceTokenAsync(handle3)).Should().NotBeNull();
            (await _refreshTokens.GetRefreshTokenAsync(handle4)).Should().BeNull();
            (await _refreshTokens.GetRefreshTokenAsync(handle5)).Should().NotBeNull();
            (await _refreshTokens.GetRefreshTokenAsync(handle6)).Should().NotBeNull();
            (await _codes.GetAuthorizationCodeAsync(handle7)).Should().BeNull();
            (await _codes.GetAuthorizationCodeAsync(handle8)).Should().NotBeNull();
            (await _codes.GetAuthorizationCodeAsync(handle9)).Should().NotBeNull();
        }
        [Fact]
        public async Task RemoveAllGrantsAsync_should_filter_on_session_id()
        {
            {
                var handle1 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client1",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle2 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client2",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle3 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client3",
                    SessionId = "session3",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });

                await _subject.RemoveAllGrantsAsync("123");

                (await _refreshTokens.GetRefreshTokenAsync(handle1)).Should().BeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle2)).Should().BeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle3)).Should().BeNull();
                await _refreshTokens.RemoveRefreshTokenAsync(handle1);
                await _refreshTokens.RemoveRefreshTokenAsync(handle2);
                await _refreshTokens.RemoveRefreshTokenAsync(handle3);
            }
            {
                var handle1 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client1",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle2 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client2",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle3 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client3",
                    SessionId = "session3",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });

                await _subject.RemoveAllGrantsAsync("123", "client1");

                (await _refreshTokens.GetRefreshTokenAsync(handle1)).Should().BeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle2)).Should().NotBeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle3)).Should().NotBeNull();
                await _refreshTokens.RemoveRefreshTokenAsync(handle1);
                await _refreshTokens.RemoveRefreshTokenAsync(handle2);
                await _refreshTokens.RemoveRefreshTokenAsync(handle3);
            }
            {
                var handle1 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client1",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle2 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client2",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle3 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client3",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle4 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client1",
                    SessionId = "session2",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                await _subject.RemoveAllGrantsAsync("123", "client1", "session1");

                (await _refreshTokens.GetRefreshTokenAsync(handle1)).Should().BeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle2)).Should().NotBeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle3)).Should().NotBeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle4)).Should().NotBeNull();
                await _refreshTokens.RemoveRefreshTokenAsync(handle1);
                await _refreshTokens.RemoveRefreshTokenAsync(handle2);
                await _refreshTokens.RemoveRefreshTokenAsync(handle3);
                await _refreshTokens.RemoveRefreshTokenAsync(handle4);
            }
            {
                var handle1 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client1",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle2 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client2",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle3 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client3",
                    SessionId = "session1",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                var handle4 = await _refreshTokens.StoreRefreshTokenAsync(new RefreshToken
                {
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 10,
                    Subject = new IdentityServerUser("123").CreatePrincipal(),
                    ClientId = "client1",
                    SessionId = "session2",
                    AuthorizedScopes = ["baz"],
                    Version = 5
                });
                await _subject.RemoveAllGrantsAsync("123", sessionId:"session1");

                (await _refreshTokens.GetRefreshTokenAsync(handle1)).Should().BeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle2)).Should().BeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle3)).Should().BeNull();
                (await _refreshTokens.GetRefreshTokenAsync(handle4)).Should().NotBeNull();
                await _refreshTokens.RemoveRefreshTokenAsync(handle1);
                await _refreshTokens.RemoveRefreshTokenAsync(handle2);
                await _refreshTokens.RemoveRefreshTokenAsync(handle3);
                await _refreshTokens.RemoveRefreshTokenAsync(handle4);
            }
        }

        [Fact]
        public async Task GetAllGrantsAsync_should_aggregate_correctly()
        {
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                ClientId = "client1",
                SubjectId = "123",
                Scopes = new string[] { "foo1", "foo2" }
            });

            var grants = await _subject.GetAllGrantsAsync("123");

            grants.Count().Should().Be(1);
            grants.First().Scopes.Should().Contain(new string[] { "foo1", "foo2" });

            var handle9 = await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "client1",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });

            grants = await _subject.GetAllGrantsAsync("123");

            grants.Count().Should().Be(1);
            grants.First().Scopes.Should().Contain(new string[] { "foo1", "foo2", "quux3" });
        }

        [Fact]
        public async Task GetAllGrantsAsync_WhenSomeGrantsFailToDeserialize_ShouldReturnGrantsAbleToDeserialize_AndLogErrors()
        {
            IPersistentGrantSerializer mockedSerializer = Mock.Of<IPersistentGrantSerializer>();
            
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                ClientId = "client1",
                SubjectId = "123",
                Scopes = ["foo1", "foo2"]
            });
            
            await _userConsent.StoreUserConsentAsync(new Consent
            {
                ClientId = "client2",
                SubjectId = "123",
                Scopes = ["foo1", "foo2"]
            });

            await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "client3",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = ["quux3"]
            });

            await _codes.StoreAuthorizationCodeAsync(new AuthorizationCode
            {
                ClientId = "clientB",
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                CodeChallenge = "challenge",
                RedirectUri = "http://some/uri",
                Nonce = "nonce",
                RequestedScopes = ["api1"]
            });

            int consentCallCount = 0;
            Mock.Get(mockedSerializer)
                .Setup(x => x.Deserialize<Consent>(It.IsAny<string>()))
                .Returns<string>(json =>
                {
                    consentCallCount++;
                    
                    return consentCallCount == 1 ? 
                        throw new Exception() : 
                        _persistentGrantSerializer.Deserialize<Consent>(json);
                });

            int codeCallCount = 0;
            Mock.Get(mockedSerializer)
                .Setup(x => x.Deserialize<AuthorizationCode>(It.IsAny<string>()))
                .Returns<string>(json =>
                {
                    codeCallCount++;
                    
                    return codeCallCount == 2 ? 
                        throw new Exception() : 
                        _persistentGrantSerializer.Deserialize<AuthorizationCode>(json);
                });

            _subject = new DefaultPersistedGrantService(_store, mockedSerializer, _logger);
                
            var grants = (await _subject.GetAllGrantsAsync("123")).ToList();

            grants.Should().NotBeNullOrEmpty();
            grants.Count.Should().Be(2);

            Mock.Get(_logger)
                .Verify(logger => logger.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Exactly(2));
        }
    }
}
