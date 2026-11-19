// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Text.Json;

#nullable enable
namespace Open.IdentityServer.Models;

/// <summary>
/// Provides a strongly typed abstraction onto a software statement associated with the client
/// </summary>
/// <param name="client">The client associated with this software statement</param>
public readonly struct ClientSoftwareStatementAccessor(Client client)
{
    private static readonly string InternalPropertyPrefix = "software_statement:";
    private static readonly string PolicyUriProperty = InternalPropertyPrefix + "policy_uri";
    private static readonly string SoftwareIdProperty = InternalPropertyPrefix + "software_id";
    private static readonly string SoftwareVersionProperty = InternalPropertyPrefix + "software_version";
    private static readonly string TermsOfServiceProperty = InternalPropertyPrefix + "terms_of_service";
    private static readonly string ContactsProperty = InternalPropertyPrefix + "contacts";
    private static readonly string AdditionalMetadataPropertyPrefix = InternalPropertyPrefix + "metadata:";
    
    /// <summary>
    /// The Policy Uri
    /// </summary>
    public string PolicyUri
    {
        get => GetStringPropertyOrEmpty(PolicyUriProperty);
        set => client.Properties[PolicyUriProperty] = value;
    }
    
    /// <summary>
    /// The Identifier of the software
    /// </summary>
    public string SoftwareId
    {
        get => GetStringPropertyOrEmpty(SoftwareIdProperty);
        set => client.Properties[SoftwareIdProperty] = value;
    }
    
    /// <summary>
    /// The version of the software
    /// </summary>
    public string SoftwareVersion
    {
        get => GetStringPropertyOrEmpty(SoftwareVersionProperty);
        set => client.Properties[SoftwareVersionProperty] = value;
    }
    
    /// <summary>
    /// The terms of service
    /// </summary>
    public string TermsOfService
    {
        get => GetStringPropertyOrEmpty(TermsOfServiceProperty);
        set => client.Properties[TermsOfServiceProperty] = value;
    }

    /// <summary>
    /// Email addresses of whom to contact regarding this client
    /// </summary>
    public string[] Contacts
    {
        get
        {
            if (!client.Properties.TryGetValue(ContactsProperty, out var contactsAsJsonArray) || String.IsNullOrEmpty(contactsAsJsonArray))
            {
                return [];
            }

            return  JsonSerializer.Deserialize<string[]>(contactsAsJsonArray) ?? [];
        }

        set => client.Properties[ContactsProperty] = JsonSerializer.Serialize(value);
    }

    /// <summary>
    /// Sets additional metadata not defined in the base schema
    /// </summary>
    /// <param name="key">name of the item</param>
    /// <param name="value">value of the item</param>
    public void SetAdditionalMetadata(string key, string value)
    {
        client.Properties[AdditionalMetadataPropertyPrefix + key] = value;
    }

    /// <summary>
    /// Gets an additional piece of metadata
    /// </summary>
    /// <param name="key">name of the item</param>
    /// <returns>value associated with the key, empty string if no value</returns>
    public string GetAdditionalMetadata(string key) => GetStringPropertyOrEmpty(AdditionalMetadataPropertyPrefix + key);
    
    private string GetStringPropertyOrEmpty(string propertyName)
    {
        if (!client.Properties.TryGetValue(propertyName, out var value) || value == null)
        {
            return String.Empty;
        }

        return value;
    }
}
