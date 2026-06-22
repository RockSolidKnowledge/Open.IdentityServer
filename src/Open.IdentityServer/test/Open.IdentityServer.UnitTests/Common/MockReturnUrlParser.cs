// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.UnitTests.Common;

public class MockReturnUrlParser : ReturnUrlParser
{
    public AuthorizationRequest AuthorizationRequestResult { get; set; }
    public bool IsValidReturnUrlResult { get; set; }

    public MockReturnUrlParser() : base(Enumerable.Empty<IReturnUrlParser>(), Mock.Of<ITelemetryService>())
    {
    }

    public override Task<AuthorizationRequest> ParseAsync(string returnUrl)
    {
        return Task.FromResult(AuthorizationRequestResult);
    }

    public override bool IsValidReturnUrl(string returnUrl)
    {
        return IsValidReturnUrlResult;
    }
}