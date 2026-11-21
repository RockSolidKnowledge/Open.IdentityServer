// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Storage.Services;
using Open.IdentityServer.Stores;
using Xunit;

#nullable enable
namespace Open.IdentityServer.UnitTests.Stores.ClientInitiatedMetadata;

public class ClientInitiatedMetadataDocumentLoaderTests
{
    private readonly Mock<IMapClientMetadataDocumentToClient> mapper = new ();
    private readonly Mock<IClientMetadataDocumentClient> documentClient = new ();
    private readonly Mock<ILogger<ClientInitiatedMetadataDocumentLoader>> logger = new();

    [Fact]
    public async Task
        Load_when_called_with_uri_that_references_a_client_metadata_document_should_load_and_map_to_client()
    {
        CancellationToken ct = new CancellationToken(false); 
        Uri documentUri = new Uri("https://identityserver.com/clients/hrApp.json");
        string expectedDocumentText = $$"""
                                        {
                                            "client_name":"test"
                                        }
                                        """;
        ClientMetadataDocument expectedMetadataDocument = new ClientMetadataDocument();
        Client expectedClient = new Client();

        documentClient.Setup(dc => dc.GetDocument(documentUri, ct))
            .ReturnsAsync(expectedDocumentText);

        mapper.Setup(m => m.Map(It.Is<ClientMetadataDocument>(cmd => cmd.ClientName == "test")))
            .Returns(expectedClient);
        
        var sut = CreateSut();

        Client? client = await sut.Load(documentUri,ct);
        
        client.Should().Be(expectedClient);
    }
    
    [Fact]
    public async Task
        Load_when_called_with_uri_that_references_client_metadata_document_that_can_not_be_deserialized_should_return_null()
    {
        CancellationToken ct = new CancellationToken(false); 
        Uri documentUri = new Uri("https://identityserver.com/clients/hrApp.json");
        string expectedDocumentText = $$"""
                                        {
                                            "client_name":"test"
                                        """;

        documentClient.Setup(dc => dc.GetDocument(documentUri, ct))
            .ReturnsAsync(expectedDocumentText);
        
        var sut = CreateSut();

        Client? client = await sut.Load(documentUri,ct);

        client.Should().BeNull();
    }
    
    [Fact]
    public async Task
        Load_when_called_with_uri_that_references_client_metadata_document_that_can_not_be_loaded_should_return_null()
    {
        CancellationToken ct = new CancellationToken(false); 
        Uri documentUri = new Uri("https://identityserver.com/clients/hrApp.json");

        documentClient
            .Setup(dc => dc.GetDocument(documentUri, ct))
            .Throws(new ClientMetadataDocumentGetException("Failed to load"));
        
        var sut = CreateSut();

        Client? client = await sut.Load(documentUri,ct);

        client.Should().BeNull();
    }
    
    private ClientInitiatedMetadataDocumentLoader CreateSut()
    {
        return new ClientInitiatedMetadataDocumentLoader(mapper.Object, documentClient.Object, logger.Object);
    }

}