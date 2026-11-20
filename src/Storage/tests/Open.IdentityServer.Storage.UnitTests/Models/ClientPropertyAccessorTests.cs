// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.Storage.UnitTests.Models;

public class ClientPropertyAccessorTests
{
    [Fact]
    public void PolicyUri_GetSet_ShouldRoundTripValue()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.PolicyUri = "https://example.com/policy";

        sut.PolicyUri.Should().Be("https://example.com/policy");
        client.Properties["dynamic_client:policy_uri"].Should().Be("https://example.com/policy");
    }

    private static ClientPropertyAccessor CreateSut(Client client)
    {
        var sut= new ClientPropertyAccessor(client);
        return sut;
    }

    [Fact]
    public void SoftwareId_GetSet_ShouldRoundTripValue()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.SoftwareId = "software-id";

        sut.SoftwareId.Should().Be("software-id");
        client.Properties["software_statement:software_id"].Should().Be("software-id");
    }

    [Fact]
    public void SoftwareVersion_GetSet_ShouldRoundTripValue()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.SoftwareVersion = "1.2.3";

        sut.SoftwareVersion.Should().Be("1.2.3");
        client.Properties["software_statement:software_version"].Should().Be("1.2.3");
    }

    [Fact]
    public void TermsOfService_GetSet_ShouldRoundTripValue()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.TermsOfService = "https://example.com/tos";

        sut.TermsOfService.Should().Be("https://example.com/tos");
        client.Properties["dynamic_client:terms_of_service"].Should().Be("https://example.com/tos");
    }

    [Fact]
    public void Contacts_Set_ShouldStoreJsonOnContactsProperty()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.Contacts = ["ops@example.com", "security@example.com"];

        client.Properties.Should().ContainKey("dynamic_client:contacts");
        client.Properties["dynamic_client:contacts"].Should().Be("[\"ops@example.com\",\"security@example.com\"]");
        client.Properties.Should().NotContainKey("dynamic_client:terms_of_service");
    }

    [Fact]
    public void Contacts_Get_ShouldDeserializeStoredJson()
    {
        Client client = new();
        var sut = CreateSut(client);
        client.Properties["dynamic_client:contacts"] = """["ops@example.com","security@example.com"]""";

        string[] contacts = sut.Contacts;

        contacts.Should().Equal("ops@example.com", "security@example.com");
    }

    [Fact]
    public void Contacts_Get_WhenPropertyMissing_ShouldReturnEmptyArray()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.Contacts.Should().BeEmpty();
    }

    [Fact]
    public void Getters_WhenPropertyMissing_ShouldReturnEmptyString()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.PolicyUri.Should().BeEmpty();
        sut.SoftwareId.Should().BeEmpty();
        sut.SoftwareVersion.Should().BeEmpty();
        sut.TermsOfService.Should().BeEmpty();
        sut.Contacts.Should().BeEmpty();
    }

    [Fact]
    public void SetAdditionalMetadata_ShouldStoreValueWithMetadataPrefix()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.SetAdditionalMetadata("logo_uri", "https://example.com/logo.png");

        client.Properties["dynamic_client:metadata:logo_uri"].Should().Be("https://example.com/logo.png");
    }

    [Fact]
    public void GetAdditionalMetadata_ShouldReturnStoredValue()
    {
        Client client = new();
        var sut = CreateSut(client);
        client.Properties["dynamic_client:metadata:owner"] = "identity-team";

        string value = sut.GetAdditionalMetadata("owner");

        value.Should().Be("identity-team");
    }

    [Fact]
    public void GetAdditionalMetadata_WhenMissing_ShouldReturnEmptyString()
    {
        Client client = new();
        var sut = CreateSut(client);

        sut.GetAdditionalMetadata("missing-item").Should().BeEmpty();
    }
}
