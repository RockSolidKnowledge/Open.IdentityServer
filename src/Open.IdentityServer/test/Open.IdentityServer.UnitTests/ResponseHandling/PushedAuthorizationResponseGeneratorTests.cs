// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling;
#nullable enable

public class PushedAuthorizationResponseGeneratorTests
{
    private readonly Mock<IPushedAuthorizationRequestService> service = new();
    private Mock<ILogger<PushedAuthorizationResponseGenerator>> _logger = new();
    
    private ValidatedAuthorizeRequest _request = new ValidatedAuthorizeRequest() { Raw = new NameValueCollection() };

    public PushedAuthorizationResponseGeneratorTests()
    {
      
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldMapRequestCorrectlyAndSendToStore()
    {
        Uri expectedKey = new Uri($"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}232444234");
        
        _request.Client = new Client();
        
        var sut = CreateSut();
        
        PushedAuthorizationMemento? storedInfo = null;

        service.Setup(s => s.CreateAsync(_request.Client, _request.Raw))
            .ReturnsAsync(new PushedAuthorization(expectedKey, TimeSpan.FromSeconds(20)));
        
        var response = await sut.CreateResponseAsync(_request);

        response!.Lifetime.Should().Be(20);
        response.Uri.Should().Be(expectedKey.ToString());
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalledAndServiceThrowsException_ShouldReturnNull()
    {
        service.Setup(s => s.CreateAsync(It.IsAny<Client>(),It.IsAny<NameValueCollection>()))
            .ThrowsAsync(new Exception());

        var sut = CreateSut();

        PushedAuthorizationResponse? response = await sut.CreateResponseAsync(_request);

        response.Should().BeNull();
    }
    
    private PushedAuthorizationResponseGenerator CreateSut()
    {
        return new PushedAuthorizationResponseGenerator(service.Object);
    }
}