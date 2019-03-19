using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Configuration;
using IdentityServer4.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityServer.IntegrationTests.Pipeline
{
    public class ConfigurationTests
    {
        private const string Category = "Configuration Tests";

        private IdentityServerPipeline _pipeline = new IdentityServerPipeline();

        [Fact]
        public async Task empty_origin_should_be_ignored_in_discovery_document()
        {
            _pipeline.OnPostConfigureServices += s=>
            {
                s.Configure<IdentityServerOptions>(opts=>
                {
                    opts.PublicOrigin = string.Empty;
                });
            };
            _pipeline.Initialize();

            var client = new HttpClient(_pipeline.Handler);
            var result = await client.GetDiscoveryDocumentAsync(IdentityServerPipeline.BaseUrl);

            result.Issuer.Should().Be(IdentityServerPipeline.BaseUrl);
        }

        [Fact]
        public void invalid_origin_should_throw_at_load_time()
        {
            _pipeline.OnPostConfigureServices += s =>
            {
                s.Configure<IdentityServerOptions>(opts =>
                {
                    opts.PublicOrigin = "invalid";
                });
            };

            Action a = () => _pipeline.Initialize();
            a.Should().Throw<InvalidOperationException>();
        }
    }
}
