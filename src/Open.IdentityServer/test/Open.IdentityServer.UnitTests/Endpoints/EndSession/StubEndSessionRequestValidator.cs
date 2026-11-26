// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.UnitTests.Endpoints.EndSession;

class StubEndSessionRequestValidator : IEndSessionRequestValidator
{
    public EndSessionValidationResult EndSessionValidationResult { get; set; } = new EndSessionValidationResult();
    public EndSessionCallbackValidationResult EndSessionCallbackValidationResult { get; set; } = new EndSessionCallbackValidationResult();

    public Task<EndSessionValidationResult> ValidateAsync(EndSessionRequestValidationContext validationContext)
    {
        return Task.FromResult(EndSessionValidationResult);
    }

    public Task<EndSessionCallbackValidationResult> ValidateCallbackAsync(EndSessionRequestValidationContext validationContext)
    {
        return Task.FromResult(EndSessionCallbackValidationResult);
    }
}