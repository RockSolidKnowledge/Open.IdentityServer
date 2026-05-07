using System.Text.Json;
using AwesomeAssertions;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Stores.Serialization;
using Xunit;

namespace IdentityServer.UnitTests.DataProtection;

public class DataProtectedGrantDataConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new DataProtectedGrantDataConverter() }
    };

    [Fact]
    public void Deserialize_WhenAllPropertiesPresent_ShouldReturnFullyPopulatedObject()
    {
        var json = """
            {
                "PersistentGrantDataContainerVersion": 2,
                "DataProtected": true,
                "Payload": "encrypted-data"
            }
            """;

        var result = JsonSerializer.Deserialize<DataProtectedGrantData>(json, _options);

        result.Should().NotBeNull();
        result!.PersistentGrantDataContainerVersion.Should().Be(2);
        result.DataProtected.Should().BeTrue();
        result.Payload.Should().Be("encrypted-data");
    }

    [Fact]
    public void Deserialize_WhenPersistentGrantDataContainerVersionNotPresent_ShouldReturnNull()
    {
        var json = """
                   {
                       "DataProtected": true,
                       "Payload": "encrypted-data"
                   }
                   """;

        var result = JsonSerializer.Deserialize<DataProtectedGrantData>(json, _options);

        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WhenUnexpectObject_ShouldReturnNull()
    {
        var json = """
                   {
                       "Property": "Value"
                   }
                   """;

        var result = JsonSerializer.Deserialize<DataProtectedGrantData>(json, _options);

        result.Should().BeNull();
    }

    [Fact]
    public void Serialize_WhenValidObject_ShouldProduceValidJson()
    {
        var data = new DataProtectedGrantData
        {
            PersistentGrantDataContainerVersion = 2,
            DataProtected = true,
            Payload = "encrypted-data"
        };

        var json = JsonSerializer.Serialize(data, _options);

        json.Should().Contain("\"PersistentGrantDataContainerVersion\":2");
        json.Should().Contain("\"DataProtected\":true");
        json.Should().Contain("\"Payload\":\"encrypted-data\"");
    }
}