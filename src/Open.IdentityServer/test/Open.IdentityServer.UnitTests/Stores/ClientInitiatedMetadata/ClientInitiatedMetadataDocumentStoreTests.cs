using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.ClientInitiatedMetadata;

public class ClientInitiatedMetadataDocumentStoreTests
{
    private readonly Mock<ILogger<ClientInitiatedMetadataDocumentStore>> logger = new();
    private readonly Mock<ITelemetryService> telemetry = new();
    private readonly Mock<IClientMetadataReferenceValidator> validator = new();
    private readonly Mock<IClientStore> configurationStore = new();
    private readonly Mock<IClientInitiatedMetadataDocumentLoader> loader = new();
    
    [Fact]
    public async Task FindByClientIdAsync_when_called_should_invoke_telemetry_trace()
    {
        string expectedClientId = "23-244-ABC";
        AddNonValidClientReference(expectedClientId);
       
        var sut = CreateSut();
        Mock<ITrace> trace = new Mock<ITrace>();
        telemetry.Setup(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores,
                sut,
                nameof(ClientInitiatedMetadataDocumentStore.FindClientByIdAsync)))
            .Returns(trace.Object);
        
        Client _ = await sut.FindClientByIdAsync(expectedClientId);
        
        trace.Verify(t=>t.AddTag(
           TelemetryConstants.TagConstants.Client , expectedClientId), 
            Times.Once);
    }

    [Fact]
    public async Task FindByClientIdAsync_when_called_with_a_non_cimd_id_should_forward_to_configured_store()
    {
        string expectedClientId = "23-244-ABC";
        Client expectedClient = new Client();

        AddClientToStore(expectedClientId, expectedClient);
        
        var sut = CreateSut();
        Client result = await sut.FindClientByIdAsync(expectedClientId);

        result.Should().Be(expectedClient);
    }

    [Fact] 
    public async Task FindByClientIdAsync_when_called_with_a_non_valid_cimd_id_should_return_null()
    {
        string expectedClientId = "https://open.identityserver.com/clients/123";

        AddNonValidClientReference(expectedClientId);
        
        var sut = CreateSut();
        Client result = await sut.FindClientByIdAsync(expectedClientId);

        result.Should().BeNull();
    }
    
    [Fact] 
    public async Task FindByClientIdAsync_when_called_with_a_valid_cimd_id_should_return_loaded_client()
    {
        string expectedClientId = "https://open.identityserver.com/clients/123";
        Client client = new Client();
        
        AddValidClientReference(expectedClientId , client);
        
        var sut = CreateSut();
        Client result = await sut.FindClientByIdAsync(expectedClientId);

        result.Should().Be(client);
    }

    private void AddValidClientReference(string expectedClientId, Client client)
    {
        AddClientReference(expectedClientId, true);
        loader
            .Setup(l => l.Load(expectedClientId))
            .ReturnsAsync(client);
    }

    private void AddNonValidClientReference(string expectedClientId)
    {
        AddClientReference(expectedClientId, false);
    }

    private void AddClientReference(string expectedClientId, bool isValid)
    {
        validator.Setup(v => v.IsMetadataReference(expectedClientId))
            .Returns(true);

        validator.Setup(v => v.IsValidMetadataReference(expectedClientId))
            .ReturnsAsync(isValid);
    }

    private void AddClientToStore(string expectedClientId, Client expectedClient)
    {
        configurationStore.Setup(cs => cs.FindClientByIdAsync(expectedClientId))
            .ReturnsAsync(expectedClient);
        
        ClientIsConfiguredInStore(expectedClientId);
    }

    private void ClientIsConfiguredInStore(string expectedClientId)
    {
        validator.Setup(v => v.IsMetadataReference(expectedClientId))
            .Returns(false);
    }
    
    ClientInitiatedMetadataDocumentStore CreateSut()
    {
        return new ClientInitiatedMetadataDocumentStore(
            configurationStore.Object,
            validator.Object,
            loader.Object,
            telemetry.Object,
            logger.Object
            );
    }
}