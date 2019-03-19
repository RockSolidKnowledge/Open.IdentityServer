// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Xunit;

namespace IdentityServer.UnitTests.Extensions
{
    public class IResourceStoreExtensionsTests
    {
        [Fact]
        public void GetAllEnabledResourcesAsync_on_duplicate_identity_scopes_should_fail()
        {
            var store = new MockResourceStore()
            {
                IdentityResources = {
                    new IdentityResource { Name = "A" },
                    new IdentityResource { Name = "A" } }
            };

            Func<Task> a = () => store.GetAllEnabledResourcesAsync();
            a.Should().Throw<Exception>().And.Message.ToLowerInvariant().Should().Contain("duplicate").And.Contain("identity scopes");
        }

        [Fact]
        public async Task GetAllEnabledResourcesAsync_without_duplicate_identity_scopes_should_succeed()
        {
            var store = new MockResourceStore()
            {
                IdentityResources = {
                    new IdentityResource { Name = "A" },
                    new IdentityResource { Name = "B" } }
            };

            await store.GetAllEnabledResourcesAsync();
        }

        [Fact]
        public void GetAllEnabledResourcesAsync_on_duplicate_api_scopes_should_fail()
        {
            var store = new MockResourceStore()
            {
                ApiResources = { new ApiResource("A"), new ApiResource("A") }
            };

            Func<Task> a = () => store.GetAllEnabledResourcesAsync();
            a.Should().Throw<Exception>().And.Message.ToLowerInvariant().Should().Contain("duplicate").And.Contain("api scopes");
        }

        [Fact]
        public async Task GetAllEnabledResourcesAsync_without_duplicate_api_scopes_should_succeed()
        {
            var store = new MockResourceStore()
            {
                ApiResources = { new ApiResource("A"), new ApiResource("B") }
            };

            await store.GetAllEnabledResourcesAsync();
        }

        [Fact]
        public void FindResourcesByScopeAsync_on_duplicate_identity_scopes_should_fail()
        {
            var store = new MockResourceStore()
            {
                IdentityResources = {
                    new IdentityResource { Name = "A" },
                    new IdentityResource { Name = "A" } }
            };

            Func<Task> a = () => store.FindResourcesByScopeAsync(new string[] { "A" });
            a.Should().Throw<Exception>().And.Message.ToLowerInvariant().Should().Contain("duplicate").And.Contain("identity scopes");
        }

        [Fact]
        public async Task FindResourcesByScopeAsync_without_duplicate_identity_scopes_should_succeed()
        {
            var store = new MockResourceStore()
            {
                IdentityResources = {
                    new IdentityResource { Name = "A" },
                    new IdentityResource { Name = "B" } }
            };

            await store.FindResourcesByScopeAsync(new string[] { "A" });
        }

        [Fact]
        public void FindResourcesByScopeAsync_on_duplicate_api_scopes_should_fail()
        {
            var store = new MockResourceStore()
            {
                ApiResources = { new ApiResource("A"), new ApiResource("A") }
            };

            Func<Task> a = () => store.FindResourcesByScopeAsync(new string[] { "A" });
            a.Should().Throw<Exception>().And.Message.ToLowerInvariant().Should().Contain("duplicate").And.Contain("api scopes");
        }

        [Fact]
        public async Task FindResourcesByScopeAsync_without_duplicate_api_scopes_should_succeed()
        {
            var store = new MockResourceStore()
            {
                ApiResources = { new ApiResource("A"), new ApiResource("B") }
            };

            await store.FindResourcesByScopeAsync(new string[] { "A" });
        }

        public class MockResourceStore : IResourceStore
        {
            public List<IdentityResource> IdentityResources { get; set; } = new List<IdentityResource>();
            public List<ApiResource> ApiResources { get; set; } = new List<ApiResource>();

            public Task<ApiResource> FindApiResourceAsync(string name)
            {
                var api = from a in ApiResources
                          where a.Name == name
                          select a;
                return Task.FromResult(api.FirstOrDefault());
            }

            public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> names)
            {
                if (names == null) throw new ArgumentNullException(nameof(names));

                var api = from a in ApiResources
                          let scopes = (from s in a.Scopes where names.Contains(s.Name) select s)
                          where scopes.Any()
                          select a;

                return Task.FromResult(api);
            }

            public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> names)
            {
                if (names == null) throw new ArgumentNullException(nameof(names));

                var identity = from i in IdentityResources
                               where names.Contains(i.Name)
                               select i;

                return Task.FromResult(identity);
            }

            public Task<Resources> GetAllResourcesAsync()
            {
                var result = new Resources(IdentityResources, ApiResources);
                return Task.FromResult(result);
            }
        }
    }
}
