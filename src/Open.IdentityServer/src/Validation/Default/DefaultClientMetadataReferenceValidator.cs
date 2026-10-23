// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Stores.Default;

#nullable enable
namespace Open.IdentityServer.Validation;

/// <summary>
/// The default Client Metadata Identifier Validator, will validate based on the RFC rules 
/// https://datatracker.ietf.org/doc/draft-ietf-oauth-client-id-metadata-document/
/// Finally consults a collection of metadata reference trust validators,
/// </summary>
public class DefaultClientMetadataReferenceValidator(
    IClientMetadataReferenceTrustPolicy trustPolicy,
    IdentityServerOptions options,
    ILogger<DefaultClientMetadataReferenceValidator> logger) : IClientMetadataReferenceValidator
{
    /// <summary>
    /// Determines if the client id looks like a metadata reference 
    /// </summary>
    /// <param name="clientId">the client id from the OAuth request</param>
    /// <returns>true if it believes this to be a metadata reference, false if not</returns>
    public bool IsMetadataReference(string clientId)
    {
        return clientId.StartsWith("https://");
    }

    /// <summary>
    /// Rules used to define if the metadata reference is valid
    /// </summary>
    private static readonly (string Error, Func<ClientInitiatedMetadataDocumentOptions,Uri,bool> Validator)[] ValidationSteps =
    [
        ("MUST use the https URL scheme" , MustUseHttps),
        ("MUST NOT contain a userinfo component defined by [RFC3986]",MustNotContainUserInfo),
        ("MUST contain a path component",MustContainAPathComponent),
        ("MUST NOT contain single-dot or double-dot path components",MustNotContainAPathWithDotAndDoubleDot),
        ("MUST NOT contain a fragment component",MustNotContainFragment),
        ("SHOULD NOT contain a query component",ShouldNotContainQueryArgument)
    ];
    /// <summary>
    /// Validates the metadata reference
    /// </summary>
    /// <param name="clientId">The client id from the OAuth request</param>
    /// <returns>true if the client id is a valid metadata reference, and is trusted</returns>
    public async ValueTask<bool> IsValidMetadataReference(string clientId)
    {
        if (Uri.TryCreate(clientId, UriKind.Absolute, out Uri? metadataReference) == false)
        {
            LogValidationFailure(clientId,"Can't Parse");
            return false;
        }

        bool validationResult = Validate(metadataReference, ValidationSteps);

        if (validationResult)
        {
            validationResult = await trustPolicy.IsTrusted(metadataReference);
            if (validationResult == false)
            {
              LogValidationFailure(clientId,"Not Trusted");
            }
        }

        return validationResult;
    }

    private void LogValidationFailure(string clientId, string reason)
    {
        logger.Log(LogLevel.Error,"Invalid Client Initiated Metadata Document Reference: {uri} : {reason}" , 
            clientId, reason);
    }
    
    private static bool MustUseHttps(ClientInitiatedMetadataDocumentOptions _, Uri metadataReference) => metadataReference.Scheme == "https";
    private static bool MustNotContainUserInfo(ClientInitiatedMetadataDocumentOptions _, Uri metadataReference) => String.IsNullOrEmpty(metadataReference.UserInfo);
    private static bool MustContainAPathComponent(ClientInitiatedMetadataDocumentOptions _, Uri metadataReference) => metadataReference.AbsolutePath != "/";

    private static bool MustNotContainAPathWithDotAndDoubleDot(ClientInitiatedMetadataDocumentOptions _, Uri metadataReference)
    {
        string[] originalParts = metadataReference.OriginalString.Split("/");
        return !(originalParts.Any(p => p is "." or ".."));
    }
    private static bool MustNotContainFragment(ClientInitiatedMetadataDocumentOptions _, Uri metadataReference) => String.IsNullOrEmpty(metadataReference.Fragment);
    private static bool ShouldNotContainQueryArgument(ClientInitiatedMetadataDocumentOptions cimdOptions, Uri metadataReference) => cimdOptions.AllowQueryArguments == true ||
                                                                         string.IsNullOrEmpty(metadataReference.Query);
    
    private bool Validate( Uri metadataReference ,params (string error, Func< ClientInitiatedMetadataDocumentOptions, Uri, bool> validate)[] validationSteps)
    {
        foreach (var validationStep in validationSteps)
        {
            if (!validationStep.validate(options.ClientInitiatedMetadataDocuments,metadataReference))
            {
                LogValidationFailure(metadataReference.OriginalString,
                    validationStep.error);
                
                return false;
            }
        }
        return true;
    }
}