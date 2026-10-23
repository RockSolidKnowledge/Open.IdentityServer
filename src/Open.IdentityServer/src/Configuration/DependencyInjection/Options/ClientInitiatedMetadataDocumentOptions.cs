// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Configuration;

/// <summary>
/// Options for Client Initiated Metadata Documents (CIMD)
/// </summary>
public class ClientInitiatedMetadataDocumentOptions
{
    /// <summary>
    /// True if it is allowed to have a query argument on the metadata document URL
    /// The current draft RFC says https://datatracker.ietf.org/doc/draft-ietf-oauth-client-id-metadata-document/
    /// states it SHOULD NOT
    /// </summary>
    public bool AllowQueryArguments { get; set; } = false;
}