// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using IdentityServer4.Stores;

namespace IdentityServer4.UnitTests.Validation
{
    public class ScopeValidation
    {
        private const string Category = "Scope Validation";

        private List<IdentityResource> _identityResources = new List<IdentityResource>
        {
            new IdentityResource
            {
                Name = "openid",
                Required = true
            },
            new IdentityResource
            {
                Name = "email"
            }
        };

        private List<ApiResource> _apiResources = new List<ApiResource>
        {
            new ApiResource
            {
                Name = "api",
                Scopes =
                {
                    new Scope
                    {
                        Name = "resource1",
                        Required = true
                    },
                    new Scope
                    {
                        Name = "resource2"
                    }
                }
            },
            new ApiResource
            {
                Name = "disabled_api",
                Enabled = false,
                Scopes =
                {
                    new Scope
                    {
                        Name = "disabled"
                    }
                }
            }
        };

        private Client _restrictedClient = new Client
        {
            ClientId = "restricted",

            AllowedScopes = new List<string>
                {
                    "openid",
                    "resource1",
                    "disabled"
                }
        };

        private IResourceStore _store;

        public ScopeValidation()
        {
            _store = new InMemoryResourcesStore(_identityResources, _apiResources);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Empty_Scope_List()
        {
            var scopes = string.Empty.ParseScopesString();

            scopes.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Sorting()
        {
            var scopes = "scope3 scope2 scope1".ParseScopesString();

            scopes.Count.Should().Be(3);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
            scopes[2].Should().Be("scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Extra_Spaces()
        {
            var scopes = "   scope3     scope2     scope1   ".ParseScopesString();

            scopes.Count.Should().Be(3);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
            scopes[2].Should().Be("scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Duplicate_Scope()
        {
            var scopes = "scope2 scope1 scope2".ParseScopesString();

            scopes.Count.Should().Be(2);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task All_Scopes_Valid()
        {
            var scopes = "openid email resource1 resource2".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Scope()
        {
            var scopes = "openid email resource1 resource2 unknown".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Disabled_Scope()
        {
            var scopes = "openid email resource1 resource2 disabled".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task All_Scopes_Allowed_For_Restricted_Client()
        {
            var scopes = "openid resource1".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesAllowedAsync(_restrictedClient, scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Scopes()
        {
            var scopes = "openid email resource1 resource2".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesAllowedAsync(_restrictedClient, scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Resource_and_Identity_Scopes()
        {
            var scopes = "openid email resource1 resource2".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeTrue();
            validator.ContainsApiResourceScopes.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Resource_Scopes_Only()
        {
            var scopes = "resource1 resource2".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeFalse();
            validator.ContainsApiResourceScopes.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Identity_Scopes_Only()
        {
            var scopes = "openid email".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeTrue();
            validator.ContainsApiResourceScopes.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void ValidateRequiredScopes_required_scopes_present_should_succeed()
        {
            var validator = Factory.CreateScopeValidator(_store);
            validator.RequestedResources = new Resources(_identityResources, _apiResources);
            validator.ValidateRequiredScopes(new string[] { "openid", "email", "resource1", "resource2" }).Should().BeTrue();
            validator.ValidateRequiredScopes(new string[] { "openid", "email", "resource1" }).Should().BeTrue();
            validator.ValidateRequiredScopes(new string[] { "openid", "resource1", "resource2" }).Should().BeTrue();
            validator.ValidateRequiredScopes(new string[] { "openid", "resource1" }).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void ValidateRequiredScopes_required_scopes_absent_should_fail()
        {
            var validator = Factory.CreateScopeValidator(_store);
            validator.RequestedResources = new Resources(_identityResources, _apiResources);
            validator.ValidateRequiredScopes(new string[] { "email", "resource2" }).Should().BeFalse();
            validator.ValidateRequiredScopes(new string[] { "email", "resource1", "resource2" }).Should().BeFalse();
            validator.ValidateRequiredScopes(new string[] { "openid", "resource2" }).Should().BeFalse();
        }
    }
}