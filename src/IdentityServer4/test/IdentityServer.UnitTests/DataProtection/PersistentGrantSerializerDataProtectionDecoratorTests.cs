using System;
using System.Text.Json;
using AwesomeAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.DataProtection;
using IdentityServer4.Storage.Stores.DataProtection;
using IdentityServer4.Stores.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IdentityServer.UnitTests.DataProtection;

public class PersistentGrantSerializerDataProtectionDecoratorTests
{
    private readonly IPersistentGrantSerializer decoratedSerializer = Mock.Of<IPersistentGrantSerializer>();
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly IDataProtector dataProtector = Mock.Of<IDataProtector>();
    private readonly IOptions<IdentityServerOptions> options = Mock.Of<IOptions<IdentityServerOptions>>();

    public PersistentGrantSerializerDataProtectionDecoratorTests()
    {
        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector(It.IsAny<string>()))
            .Returns(dataProtector);
    }
    
    private PersistentGrantSerializerDataProtectionDecorator CreateSut(bool enabled)
    {
        Mock.Get(options)
            .Setup(x => x.Value)
            .Returns(new IdentityServerOptions
            {
                PersistentGrants = new PersistentGrantsOptions
                {
                    DataProtectData = enabled,
                },
            });
        
        return new PersistentGrantSerializerDataProtectionDecorator(decoratedSerializer, dataProtectionProvider, options);
    }

    [Fact]
    public void Serialize_WhenProtectionEnabled_ShouldProtectDataAndWrapInDataProtectedDataObject()
    {
        var input = new { value1 = "someVal" };
        var inputSerialised = "{ \"value1\" = \"someVal\" }";
        var inputProtected = "PROTECTED_INPUT";
        
        SetupSerialise(input, inputSerialised, inputProtected);

        var sut = CreateSut(true);
        var actual = sut.Serialize(input);

        actual.Should().NotBeNullOrWhiteSpace();
        JsonElement element = JsonElement.Parse(actual);

        element.GetProperty("PersistentGrantDataContainerVersion").GetInt32().Should().Be(1);
        element.GetProperty("DataProtected").GetBoolean().Should().BeTrue();
        element.GetProperty("Payload").GetString().Should().BeEquivalentTo(inputProtected);
    }

    [Fact]
    public void Serialize_WhenProtectionDisabled_ShouldWrapInDataProtectedDataObjectUnprotected()
    {
        var input = new { value1 = "someVal" };
        var inputSerialised = "{ \"value1\" = \"someVal\" }";
        
        SetupSerialise(input, inputSerialised, string.Empty);

        var sut = CreateSut(false);
        var actual = sut.Serialize(input);

        actual.Should().NotBeNullOrWhiteSpace();
        JsonElement element = JsonElement.Parse(actual);

        element.GetProperty("PersistentGrantDataContainerVersion").GetInt32().Should().Be(1);
        element.GetProperty("DataProtected").GetBoolean().Should().BeFalse();
        element.GetProperty("Payload").GetString().Should().BeEquivalentTo(inputSerialised);
    }

    [Fact]
    public void Deserialize_WhenProtectedDataProvided_ShouldUnprotectAndReturn()
    {
        object payload = new { value1 = "someVal" };
        var serialisedPayload = "{ \"value1\" = \"someVal\" }";
        var protectedPayload = "PROTECTED_INPUT";

        var input = $$"""
                    {
                        "PersistentGrantDataContainerVersion": 1,
                        "DataProtected": true,
                        "Payload": {{protectedPayload}},
                    }
                    """;
        
        SetupDeserialise(payload, serialisedPayload, protectedPayload);

        var sut = CreateSut(true);
        var actual = sut.Deserialize<object>(input);

        actual.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public void Deserialize_WhenUnProtectedDataProvided_ShouldReturnData()
    {
        object payload = new { value1 = "someVal" };
        var serialisedPayload = "{ \"value1\" = \"someVal\" }";

        var input = $$"""
                      {
                          "PersistentGrantDataContainerVersion": 1,
                          "DataProtected": false,
                          "Payload": {{serialisedPayload}},
                      }
                      """;
        
        SetupDeserialise(payload, serialisedPayload, string.Empty);

        var sut = CreateSut(true);
        var actual = sut.Deserialize<object>(input);

        actual.Should().BeEquivalentTo(payload);
    }
    
    private void SetupSerialise(object input, string inputSerialised, string inputProtected)
    {
        Mock.Get(decoratedSerializer)
            .Setup(x => x.Serialize(input))
            .Returns<object>(_ => inputSerialised);

        Mock.Get(dataProtector)
            .Setup(x => x.Protect(It.Is<byte[]>(
                b => System.Text.Encoding.UTF8.GetString(b) == inputSerialised)))
            .Returns<byte[]>(_ => Convert.FromBase64String(inputProtected));
    }
    
    private void SetupDeserialise(object input, string inputSerialised, string inputProtected)
    {
        Mock.Get(decoratedSerializer)
            .Setup(x => x.Deserialize<object>(inputSerialised))
            .Returns<object>(_ => input);

        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == inputProtected)))
            .Returns<byte[]>(_ => System.Text.Encoding.UTF8.GetBytes(inputSerialised));
    }
}