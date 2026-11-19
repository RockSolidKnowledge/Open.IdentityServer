// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.Storage.UnitTests.Models;

public class ClientSoftwareStatementTests
{
    [Fact]
    public void PolicyUri_GetSet_ShouldRoundTripValue()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.PolicyUri = "https://example.com/policy";

        softwareStatement.PolicyUri.Should().Be("https://example.com/policy");
        client.Properties["software_statement:policy_uri"].Should().Be("https://example.com/policy");
    }

    [Fact]
    public void SoftwareId_GetSet_ShouldRoundTripValue()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.SoftwareId = "software-id";

        softwareStatement.SoftwareId.Should().Be("software-id");
        client.Properties["software_statement:software_id"].Should().Be("software-id");
    }

    [Fact]
    public void SoftwareVersion_GetSet_ShouldRoundTripValue()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.SoftwareVersion = "1.2.3";

        softwareStatement.SoftwareVersion.Should().Be("1.2.3");
        client.Properties["software_statement:software_version"].Should().Be("1.2.3");
    }

    [Fact]
    public void TermsOfService_GetSet_ShouldRoundTripValue()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.TermsOfService = "https://example.com/tos";

        softwareStatement.TermsOfService.Should().Be("https://example.com/tos");
        client.Properties["software_statement:terms_of_service"].Should().Be("https://example.com/tos");
    }

    [Fact]
    public void Contacts_Set_ShouldStoreJsonOnContactsProperty()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.Contacts = ["ops@example.com", "security@example.com"];

        client.Properties.Should().ContainKey("software_statement:contacts");
        client.Properties["software_statement:contacts"].Should().Be("[\"ops@example.com\",\"security@example.com\"]");
        client.Properties.Should().NotContainKey("software_statement:terms_of_service");
    }

    [Fact]
    public void Contacts_Get_ShouldDeserializeStoredJson()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);
        client.Properties["software_statement:contacts"] = """["ops@example.com","security@example.com"]""";

        string[] contacts = softwareStatement.Contacts;

        contacts.Should().Equal("ops@example.com", "security@example.com");
    }

    [Fact]
    public void Contacts_Get_WhenPropertyMissing_ShouldReturnEmptyArray()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.Contacts.Should().BeEmpty();
    }

    [Fact]
    public void Getters_WhenPropertyMissing_ShouldReturnEmptyString()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.PolicyUri.Should().BeEmpty();
        softwareStatement.SoftwareId.Should().BeEmpty();
        softwareStatement.SoftwareVersion.Should().BeEmpty();
        softwareStatement.TermsOfService.Should().BeEmpty();
        softwareStatement.Contacts.Should().BeEmpty();
    }

    [Fact]
    public void SetAdditionalMetadata_ShouldStoreValueWithMetadataPrefix()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.SetAdditionalMetadata("logo_uri", "https://example.com/logo.png");

        client.Properties["software_statement:metadata:logo_uri"].Should().Be("https://example.com/logo.png");
    }

    [Fact]
    public void GetAdditionalMetadata_ShouldReturnStoredValue()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);
        client.Properties["software_statement:metadata:owner"] = "identity-team";

        string value = softwareStatement.GetAdditionalMetadata("owner");

        value.Should().Be("identity-team");
    }

    [Fact]
    public void GetAdditionalMetadata_WhenMissing_ShouldReturnEmptyString()
    {
        Client client = new();
        ClientSoftwareStatementAccessor softwareStatement = new(client);

        softwareStatement.GetAdditionalMetadata("missing-item").Should().BeEmpty();
    }
}
