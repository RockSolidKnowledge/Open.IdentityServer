using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class DefaultClientMetadataReferenceValidatorTests
{
    private readonly IdentityServerOptions options = new();
    private readonly Mock<ILogger<DefaultClientMetadataReferenceValidator>> logger = new();
    private readonly Mock<IClientMetadataReferenceTrustValidator> trustPolicy = new();
    
    
    [Fact]
    public void IsMetadataReference_when_given_a_client_id_that_starts_with_https_should_return_true()
    {
        var sut = CreateSut();

        bool result = sut.IsMetadataReference("https://open.identityserver.com");

        result.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("http://open.identityserver.com")]
    [InlineData("2323-32553-32343-ABCDE")]
    public void IsMetadataReference_when_given_a_client_id_that_does_not_starts_with_http_should_return_false(string reference)
    {
        var sut = CreateSut();

        bool result = sut.IsMetadataReference(reference);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("https://client.example/oauth-client.json")]
    [InlineData("https://client.example/.well-known/oauth-client")]
    [InlineData("https://client.example:8443/oauth-client.json")]
    [InlineData("https://client.example/apps/my.client.json")]
    public async Task IsValidMetadataReference_when_given_a_valid_reference_should_return_true(string validReference)
    {
        var sut = CreateSut();

        SetupTrustPolicyResult(validReference, true);
        
        bool result = await sut.IsValidMetadataReference(validReference);

        result.Should().BeTrue();
    }
    
    // http://client.example/oauth-client.json
    // ^ not HTTPS//
    [Theory]
    [InlineData("http://client.example/oauth-client.json")]
    [InlineData("urn:client.example/oauth-client.json")]
    public async Task IsValidMetadataReference_when_given_a_reference_that_does_not_start_with_https_should_return_false(string input)
    {
        var sut = CreateSut();

        bool result = await sut.IsValidMetadataReference(input);

        result.Should().BeFalse();
    }
    
    // https://andrew@client.example/oauth-client.json
    // ^ userinfo
    [Fact]
    public async Task IsValidMetadataReference_when_given_a_reference_that_contains_userinfo_should_return_false()
    {
        var sut = CreateSut();
        bool result = await sut.IsValidMetadataReference("https://andrew@client.example/oauth-client.json");

        result.Should().BeFalse();
    }
   
   
    //     https://client.example
    // ^ no path
    [Fact]
    public async Task IsValidMetadataReference_when_given_a_reference_that_contains_no_path_should_return_false()
    {
        var sut = CreateSut();
        bool result = await sut.IsValidMetadataReference("https://client.example");

        result.Should().BeFalse();
    }
    
    // https://client.example/oauth/./client.json
    // ^ "." path component
   //
   [Fact]
   public async Task IsValidMetadataReference_when_given_a_reference_that_contains_a_path_with_dot_segment_should_return_false()
   {
       var sut = CreateSut();
       bool result = await sut.IsValidMetadataReference("https://client.example/foo/bar/./blah");

       result.Should().BeFalse();
   }
    // https://client.example/oauth/../client.json
    // ^ ".." path component
   [Fact]
    public async Task IsValidMetadataReference_when_given_a_reference_that_contains_a_path_with_double_dot_segment_should_return_false()
    {
        var sut = CreateSut();
        bool result = await sut.IsValidMetadataReference("https://client.example/foo/bar/../blah");

        result.Should().BeFalse();
    }
    
    // https://client.example/oauth-client.json#v1
    // ^ fragment
   //
   //
   [Fact]
   public async Task IsValidMetadataReference_when_given_a_reference_that_contains_a_fragment_should_return_false()
   {
       var sut = CreateSut();
       bool result = await sut.IsValidMetadataReference("https://client.example/foo/bar#blah");

       result.Should().BeFalse();
   }
    //     DISCOURAGED, BUT NOT NECESSARILY INVALID
   //
    //     https://client.example/oauth-client.json?version=1
    // ^ query
    [Fact]
    public async Task IsValidMetadataReference_when_given_a_reference_that_contains_a_query_argument_should_return_false()
    {
        var sut = CreateSut();
        bool result = await sut.IsValidMetadataReference("https://client.example/foo/bar?blah");

        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsValidMetadataReference_when_given_a_reference_that_contains_a_query_argument_and_options_allow_should_return_true()
    {
        string expectedUrl = "https://client.example/foo/bar?blah";
        options.ClientInitiatedMetadataDocuments.AllowQueryArguments = true;
        
        var sut = CreateSut();
        SetupTrustPolicyResult(expectedUrl,true);
        bool result = await sut.IsValidMetadataReference(expectedUrl);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsValidMetadataReference_when_valid_url_and_url_is_trusted_should_return_true()
    {
        string expectedUrl = "https://identityserver.com/clients/mcp.json";
        
        var sut = CreateSut();

        SetupTrustPolicyResult(expectedUrl, true);
        
        bool result = await sut.IsValidMetadataReference(expectedUrl);

        result.Should().BeTrue();
    }

    private void SetupTrustPolicyResult(string expectedUrl, bool value)
    {
        trustPolicy.Setup(tp => tp.IsTrusted(new Uri(expectedUrl, UriKind.Absolute)))
            .ReturnsAsync(value);
    }

    [Fact]
    public async Task IsValidMetadataReference_when_valid_url_and_url_is_not_trusted_should_return_false()
    {
        string expectedUrl = "https://identityserver.com/clients/mcp.json";
        
        var sut = CreateSut();

        SetupTrustPolicyResult(expectedUrl, false);
        
        bool result = await sut.IsValidMetadataReference(expectedUrl);

        result.Should().BeFalse();
    }
    
    
    private DefaultClientMetadataReferenceValidator CreateSut()
    {
        return new DefaultClientMetadataReferenceValidator( 
            trustPolicy.Object,
            options,
            logger.Object);
    }
}

