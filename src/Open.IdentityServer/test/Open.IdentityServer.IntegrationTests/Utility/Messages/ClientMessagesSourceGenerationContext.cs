#nullable enable

using System.Text.Json.Serialization;

namespace IdentityServer.IntegrationTests.Utility;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(DynamicClientRegistrationDocument))]
internal partial class ClientMessagesSourceGenerationContext : JsonSerializerContext
{
}

