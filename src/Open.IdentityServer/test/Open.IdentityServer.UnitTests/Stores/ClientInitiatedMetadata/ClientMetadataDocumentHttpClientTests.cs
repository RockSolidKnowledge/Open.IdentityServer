// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Moq.Protected;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.ClientInitiatedMetadata;

public class ClientMetadataDocumentHttpClientTests
{
    [Fact]
    public async Task GetDocument_when_called_should_load_the_requested_document()
    {
        Uri metadataUrl = new("https://identityserver.com/clients/hrApp.json");
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct = cts.Token;
        const string expectedDocument = """{"client_name":"test"}""";

        var handler = SetupHandler(metadataUrl, expectedDocument);

        var httpClient = new HttpClient(handler.Object);
        var sut = new ClientMetadataDocumentHttpClient(httpClient);

        string document = await sut.GetDocument(metadataUrl, ct);

        document.Should().Be(expectedDocument);
    }
    
    [Fact]
    public async Task GetDocument_when_called_and_operation_cancelled_should_throw_operation_cancelled_exception()
    {
        Uri metadataUrl = new("https://identityserver.com/clients/hrApp.json");
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct = cts.Token;
        cts.Cancel();
        const string expectedDocument = "{}";

        var handler = SetupHandler(metadataUrl, expectedDocument);

        var httpClient = new HttpClient(handler.Object);
        var sut = new ClientMetadataDocumentHttpClient(httpClient);

        Func<Task<string>> act = () => sut.GetDocument(metadataUrl, ct);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
    
    
    [Fact]
    public async Task GetDocument_when_called_and_load_fails_should_throw_ClientMetadataDocumentGetException()
    {
        Uri metadataUrl = new("https://identityserver.com/clients/hrApp.json");
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct = cts.Token;
        
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>( (_,__) => throw new HttpRequestException())
            ;
        
        var httpClient = new HttpClient(handler.Object);
        var sut = new ClientMetadataDocumentHttpClient(httpClient);

        Func<Task<string>> act = () => sut.GetDocument(metadataUrl, ct);

        await act.Should().ThrowAsync<ClientMetadataDocumentGetException>();
    }

    private static Mock<HttpMessageHandler> SetupHandler(Uri metadataUrl, string expectedDocument)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get &&
                    request.RequestUri == metadataUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedDocument)
            });
        return handler;
    }
}

