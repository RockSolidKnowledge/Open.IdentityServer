// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Stores.Serialization;
using IdentityServer.UnitTests.Common;
using IdentityServer.UnitTests.Validation.Setup;
using IdentityServer4.Services.Default;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer4.UnitTests.Validation
{
    internal static class Factory
    {
        public static IClientStore CreateClientStore()
        {
            return new InMemoryClientStore(TestClients.Get());
        }

        public static ScopeValidator CreateScopeValidator(IResourceStore store)
        {
            return new ScopeValidator(store, TestLogger.Create<ScopeValidator>());
        }

        public static TokenRequestValidator CreateTokenRequestValidator(
            IdentityServerOptions options = null,
            IResourceStore resourceStore = null,
            IAuthorizationCodeStore authorizationCodeStore = null,
            IRefreshTokenStore refreshTokenStore = null,
            IResourceOwnerPasswordValidator resourceOwnerValidator = null,
            IProfileService profile = null,
            IDeviceCodeValidator deviceCodeValidator = null,
            IEnumerable<IExtensionGrantValidator> extensionGrantValidators = null,
            ICustomTokenRequestValidator customRequestValidator = null,
            ITokenValidator tokenValidator = null,
            ScopeValidator scopeValidator = null)
        {
            if (options == null)
            {
                options = TestIdentityServerOptions.Create();
            }

            if (resourceStore == null)
            {
                resourceStore = new InMemoryResourcesStore(TestScopes.GetIdentity(), TestScopes.GetApis());
            }

            if (resourceOwnerValidator == null)
            {
                resourceOwnerValidator = new TestResourceOwnerPasswordValidator();
            }

            if (profile == null)
            {
                profile = new TestProfileService();
            }
            
            if (deviceCodeValidator == null)
            {
                deviceCodeValidator = new TestDeviceCodeValidator();
            }

            if (customRequestValidator == null)
            {
                customRequestValidator = new DefaultCustomTokenRequestValidator();
            }

            ExtensionGrantValidator aggregateExtensionGrantValidator;
            if (extensionGrantValidators == null)
            {
                aggregateExtensionGrantValidator = new ExtensionGrantValidator(new[] { new TestGrantValidator() }, TestLogger.Create<ExtensionGrantValidator>());
            }
            else
            {
                aggregateExtensionGrantValidator = new ExtensionGrantValidator(extensionGrantValidators, TestLogger.Create<ExtensionGrantValidator>());
            }

            if (authorizationCodeStore == null)
            {
                authorizationCodeStore = CreateAuthorizationCodeStore();
            }

            if (refreshTokenStore == null)
            {
                refreshTokenStore = CreateRefreshTokenStore();
            }

            if (scopeValidator == null)
            {
                scopeValidator = new ScopeValidator(resourceStore, new LoggerFactory().CreateLogger<ScopeValidator>());
            }

            if (tokenValidator == null)
            {
                tokenValidator = CreateTokenValidator(refreshTokenStore: refreshTokenStore, profile: profile);
            }

            return new TokenRequestValidator(
                options,
                authorizationCodeStore,
                resourceOwnerValidator,
                profile,
                deviceCodeValidator,
                aggregateExtensionGrantValidator,
                customRequestValidator,
                scopeValidator,
                tokenValidator,
                new TestEventService(), new StubClock(), TestLogger.Create<TokenRequestValidator>());
        }

        internal static ITokenCreationService CreateDefaultTokenCreator()
        {
            return new DefaultTokenCreationService(
                new StubClock(),
                new DefaultKeyMaterialService(new IValidationKeysStore[] { }, new DefaultSigningCredentialsStore(TestCert.LoadSigningCredentials())), TestLogger.Create<DefaultTokenCreationService>());
        }

        public static DeviceAuthorizationRequestValidator CreateDeviceAuthorizationRequestValidator(
            IdentityServerOptions options = null,
            IResourceStore resourceStore = null,
            ScopeValidator scopeValidator = null)
        {
            if (options == null)
            {
                options = TestIdentityServerOptions.Create();
            }
            
            if (resourceStore == null)
            {
                resourceStore = new InMemoryResourcesStore(TestScopes.GetIdentity(), TestScopes.GetApis());
            }

            if (scopeValidator == null)
            {
                scopeValidator = new ScopeValidator(resourceStore, new LoggerFactory().CreateLogger<ScopeValidator>());
            }

            return new DeviceAuthorizationRequestValidator(
                options,
                scopeValidator,
                TestLogger.Create<DeviceAuthorizationRequestValidator>());
        }

        public static AuthorizeRequestValidator CreateAuthorizeRequestValidator(
            IdentityServerOptions options = null,
            IResourceStore resourceStore = null,
            IClientStore clients = null,
            IProfileService profile = null,
            ICustomAuthorizeRequestValidator customValidator = null,
            IRedirectUriValidator uriValidator = null,
            ScopeValidator scopeValidator = null)
        {
            if (options == null)
            {
                options = TestIdentityServerOptions.Create();
            }

            if (resourceStore == null)
            {
                resourceStore = new InMemoryResourcesStore(TestScopes.GetIdentity(), TestScopes.GetApis());
            }

            if (clients == null)
            {
                clients = new InMemoryClientStore(TestClients.Get());
            }

            if (customValidator == null)
            {
                customValidator = new DefaultCustomAuthorizeRequestValidator();
            }

            if (uriValidator == null)
            {
                uriValidator = new StrictRedirectUriValidator();
            }

            if (scopeValidator == null)
            {
                scopeValidator = new ScopeValidator(resourceStore, new LoggerFactory().CreateLogger<ScopeValidator>());
            }

            var userSession = new MockUserSession();

            return new AuthorizeRequestValidator(
                options,
                clients,
                customValidator,
                uriValidator,
                scopeValidator,
                userSession,
                TestLogger.Create<AuthorizeRequestValidator>());
        }

        public static TokenValidator CreateTokenValidator(
            IReferenceTokenStore store = null, 
            IRefreshTokenStore refreshTokenStore = null,
            IProfileService profile = null, 
            IdentityServerOptions options = null, ISystemClock clock = null)
        {
            if (options == null)
            {
                options = TestIdentityServerOptions.Create();
            }

            if (profile == null)
            {
                profile = new TestProfileService();
            }

            if (store == null)
            {
                store = CreateReferenceTokenStore();
            }

            clock = clock ?? new StubClock();

            if (refreshTokenStore == null)
            {
                refreshTokenStore = CreateRefreshTokenStore();
            }

            var clients = CreateClientStore();
            var context = new MockHttpContextAccessor(options);
            var logger = TestLogger.Create<TokenValidator>();

            var validator = new TokenValidator(
                clients: clients,
                clock: clock,
                profile: profile,
                referenceTokenStore: store,
                refreshTokenStore: refreshTokenStore,
                customValidator: new DefaultCustomTokenValidator(),
                    keys: new DefaultKeyMaterialService(new[] { new DefaultValidationKeysStore(new[] { TestCert.LoadSigningCredentials().Key }) }),
                logger: logger,
                options: options,
                context: context);

            return validator;
        }

        public static IDeviceCodeValidator CreateDeviceCodeValidator(
            IDeviceFlowCodeService service,
            IProfileService profile = null,
            IDeviceFlowThrottlingService throttlingService = null,
            ISystemClock clock = null)
        {
            profile = profile ?? new TestProfileService();
            throttlingService = throttlingService ?? new TestDeviceFlowThrottlingService();
            clock = clock ?? new StubClock();
            
            var validator = new DeviceCodeValidator(service, profile, throttlingService, clock, TestLogger.Create<DeviceCodeValidator>());

            return validator;
        }

        public static IClientSecretValidator CreateClientSecretValidator(IClientStore clients = null, SecretParser parser = null, SecretValidator validator = null, IdentityServerOptions options = null)
        {
            options = options ?? TestIdentityServerOptions.Create();

            if (clients == null) clients = new InMemoryClientStore(TestClients.Get());

            if (parser == null)
            {
                var parsers = new List<ISecretParser>
                {
                    new BasicAuthenticationSecretParser(options, TestLogger.Create<BasicAuthenticationSecretParser>()),
                    new PostBodySecretParser(options, TestLogger.Create<PostBodySecretParser>())
                };

                parser = new SecretParser(parsers, TestLogger.Create<SecretParser>());
            }

            if (validator == null)
            {
                var validators = new List<ISecretValidator>
                {
                    new HashedSharedSecretValidator(TestLogger.Create<HashedSharedSecretValidator>()),
                    new PlainTextSharedSecretValidator(TestLogger.Create<PlainTextSharedSecretValidator>())
                };

                validator = new SecretValidator(new StubClock(), validators, TestLogger.Create<SecretValidator>());
            }

            return new ClientSecretValidator(clients, parser, validator, new TestEventService(), TestLogger.Create<ClientSecretValidator>());
        }

        public static IAuthorizationCodeStore CreateAuthorizationCodeStore()
        {
            return new DefaultAuthorizationCodeStore(new InMemoryPersistedGrantStore(),
                new PersistentGrantSerializer(),
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultAuthorizationCodeStore>());
        }
        
        public static IRefreshTokenStore CreateRefreshTokenStore()
        {
            return new DefaultRefreshTokenStore(new InMemoryPersistedGrantStore(),
                new PersistentGrantSerializer(),
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultRefreshTokenStore>());
        }
        
        public static IReferenceTokenStore CreateReferenceTokenStore()
        {
            return new DefaultReferenceTokenStore(new InMemoryPersistedGrantStore(),
                new PersistentGrantSerializer(),
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultReferenceTokenStore>());
        }

        public static IDeviceFlowCodeService CreateDeviceCodeService()
        {
            return new DefaultDeviceFlowCodeService(new InMemoryDeviceFlowStore(), new DefaultHandleGenerationService());
        }
        
        public static IUserConsentStore CreateUserConsentStore()
        {
            return new DefaultUserConsentStore(new InMemoryPersistedGrantStore(),
                new PersistentGrantSerializer(),
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultUserConsentStore>());
        }
    }
}