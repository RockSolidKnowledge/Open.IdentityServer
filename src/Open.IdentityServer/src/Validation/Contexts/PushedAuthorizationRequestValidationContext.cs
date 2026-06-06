// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Encapsulates the context for the Pushed Authorization Request, used for validation
/// </summary>
public record PushedAuthorizationRequestValidationContext(NameValueCollection RequestParameters) { }
