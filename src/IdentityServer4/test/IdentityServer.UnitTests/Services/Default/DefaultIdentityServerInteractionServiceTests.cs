// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.UnitTests.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using IdentityServer.UnitTests.Common;

namespace IdentityServer4.UnitTests.Services.Default
{
    public class DefaultIdentityServerInteractionServiceTests
    {
        private DefaultIdentityServerInteractionService _subject;

        private IdentityServerOptions _options = new IdentityServerOptions();
        private MockHttpContextAccessor _mockMockHttpContextAccessor;
        private MockMessageStore<EndSession> _mockEndSessionStore = new MockMessageStore<EndSession>();
        private MockMessageStore<LogoutMessage> _mockLogoutMessageStore = new MockMessageStore<LogoutMessage>();
        private MockMessageStore<ErrorMessage> _mockErrorMessageStore = new MockMessageStore<ErrorMessage>();
        private MockConsentMessageStore _mockConsentStore = new MockConsentMessageStore();
        private MockPersistedGrantService _mockPersistedGrantService = new MockPersistedGrantService();
        private MockUserSession _mockUserSession = new MockUserSession();
        private MockReturnUrlParser _mockReturnUrlParser = new MockReturnUrlParser();

        public DefaultIdentityServerInteractionServiceTests()
        {
            _mockMockHttpContextAccessor = new MockHttpContextAccessor(_options, _mockUserSession, _mockEndSessionStore);

            _subject = new DefaultIdentityServerInteractionService(new StubClock(), 
                _mockMockHttpContextAccessor,
                _mockLogoutMessageStore,
                _mockErrorMessageStore,
                _mockConsentStore,
                _mockPersistedGrantService,
                _mockUserSession,
                _mockReturnUrlParser,
                TestLogger.Create<DefaultIdentityServerInteractionService>()
            );
        }
        
        [Fact]
        public async Task GetLogoutContextAsync_valid_session_and_logout_id_should_not_provide_signout_iframe()
        {
            // for this, we're just confirming that since the session has changed, there's not use in doing the iframe and thsu SLO
            _mockUserSession.SessionId = null;
            _mockLogoutMessageStore.Messages.Add("id", new Message<LogoutMessage>(new LogoutMessage() { SessionId = "session" }));

            var context = await _subject.GetLogoutContextAsync("id");

            context.SignOutIFrameUrl.Should().BeNull();
        }

        [Fact]
        public async Task GetLogoutContextAsync_valid_session_no_logout_id_should_provide_iframe()
        {
            _mockUserSession.Clients.Add("foo");
            _mockUserSession.SessionId = "session";
            _mockUserSession.User = new IdentityServerUser("123").CreatePrincipal();

            var context = await _subject.GetLogoutContextAsync(null);

            context.SignOutIFrameUrl.Should().NotBeNull();
        }

        [Fact]
        public async Task GetLogoutContextAsync_without_session_should_not_provide_iframe()
        {
            _mockUserSession.SessionId = null;
            _mockLogoutMessageStore.Messages.Add("id", new Message<LogoutMessage>(new LogoutMessage()));

            var context = await _subject.GetLogoutContextAsync("id");

            context.SignOutIFrameUrl.Should().BeNull();
        }

        [Fact]
        public async Task CreateLogoutContextAsync_without_session_should_not_create_session()
        {
            var context = await _subject.CreateLogoutContextAsync();

            context.Should().BeNull();
            _mockLogoutMessageStore.Messages.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateLogoutContextAsync_with_session_should_create_session()
        {
            _mockUserSession.Clients.Add("foo");
            _mockUserSession.User = new IdentityServerUser("123").CreatePrincipal();
            _mockUserSession.SessionId = "session";

            var context = await _subject.CreateLogoutContextAsync();

            context.Should().NotBeNull();
            _mockLogoutMessageStore.Messages.Should().NotBeEmpty();
        }

        [Fact]
        public void GrantConsentAsync_should_throw_if_granted_and_no_subject()
        {
            Func<Task> act = () => _subject.GrantConsentAsync(
                new AuthorizationRequest(), 
                new ConsentResponse() { ScopesConsented = new[] { "openid" } }, 
                null);

            act.Should().Throw<ArgumentNullException>()
                .And.Message.Should().Contain("subject");
        }

        [Fact]
        public async Task GrantConsentAsync_should_allow_deny_for_anonymous_users()
        {
            await _subject.GrantConsentAsync(new AuthorizationRequest(), ConsentResponse.Denied, null);
        }

        [Fact]
        public async Task GrantConsentAsync_should_use_current_subject_and_create_message()
        {
            _mockUserSession.User = new IdentityServerUser("bob").CreatePrincipal();

            var req = new AuthorizationRequest() { ClientId = "client" };
            await _subject.GrantConsentAsync(req, new ConsentResponse(), null);

            _mockConsentStore.Messages.Should().NotBeEmpty();
            var consentRequest = new ConsentRequest(req, "bob");
            _mockConsentStore.Messages.First().Key.Should().Be(consentRequest.Id);
        }
    }
}
