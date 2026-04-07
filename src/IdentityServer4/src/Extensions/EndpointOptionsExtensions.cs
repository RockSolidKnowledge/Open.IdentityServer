// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Configuration;
using Open.IdentityServer.Hosting;
using static Open.IdentityServer.Constants;

namespace Open.IdentityServer.Extensions
{
    internal static class EndpointOptionsExtensions
    {
        public static bool IsEndpointEnabled(this EndpointsOptions options, Endpoint endpoint)
        {
            return endpoint?.Name switch
            {
                EndpointNames.Authorize => options.EnableAuthorizeEndpoint,
                EndpointNames.CheckSession => options.EnableCheckSessionEndpoint,
                EndpointNames.DeviceAuthorization => options.EnableDeviceAuthorizationEndpoint,
                EndpointNames.Discovery => options.EnableDiscoveryEndpoint,
                EndpointNames.EndSession => options.EnableEndSessionEndpoint,
                EndpointNames.Introspection => options.EnableIntrospectionEndpoint,
                EndpointNames.Revocation => options.EnableTokenRevocationEndpoint,
                EndpointNames.Token => options.EnableTokenEndpoint,
                EndpointNames.UserInfo => options.EnableUserInfoEndpoint,
                _ => true
            };
        }
    }
}