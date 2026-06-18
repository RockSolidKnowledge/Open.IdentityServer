// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class TelemetryConstants
{
    public static class MetricsConstants
    {
        public const string MeterName = "Open.IdentityServer";
        public const string OperationCounterName = "tokenservice.operation";
        public const string ActiveRequestsCounterName = "tokenservice.active_requests";

        public const string ApiSecretValidationCounterName = "tokenservice.api.secret_validation";
        public const string ClientConfigValidationCounterName =  "tokenservice.client.config_validation";
        public const string ClientSecretValidationCounterName = "tokenservice.client.secret_validation";
        public const string DeviceAuthenticationCounterName =  "tokenservice.device_authentication";
        public const string IntrospectionCounterName = "tokenservice.introspection";
        public const string ResourceOwnerAuthenticationCounterName = "tokenservice.resourceowner_authentication";
        public const string TokenIssuedCounterName = "tokenservice.token_issued";
        public const string RevocationCounterName = "tokenservice.revocation";
        
        // not-implemented, on roadmap, included
        public const string PushedAuthorizationRequestCounterName = "tokenservice.pushed_authorization_request";
        public const string BackChannelAuthenticationCounterName =  "tokenservice.backchannel_authentication";
        
        // components (excluded, todo: note in docs & component ticket)
        // Implement equivalents in component packages
        public const string DynamicIdentityProviderCounterName = "tokenservice.dynamic_identityprovider.validation";
        public const string SamlSsoCounterName = "tokenservice.saml.sso";
        public const string SamlSloCounterName = "tokenservice.saml.slo";
    }

    public static class TagConstants
    {
        public const string Error = "error";
        public const string Result = "result";
        public const string Client = "client";
        
        public const string Success = "success";
        public const string InternalError = "internal_error";
        public const string Endpoint = "endpoint";
        public const string Path = "path";
        public const string Api = "api";
        public const string AuthMethod = "auth_method";
        public const string Scheme = "scheme";
        public const string Caller = "caller";
        public const string Active = "active";
        public const string GrantType = "grant_type";
    }
    
    public static class TraceCategories
    {
        public const string Basic = "Open.IdentityServer";
        public const string Cache = "Open.IdentityServer.Cache";
        public const string Services = "Open.IdentityServer.Services";
        public const string Stores = "Open.IdentityServer.Stores";
        public const string Validation = "Open.IdentityServer.Validation";
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member