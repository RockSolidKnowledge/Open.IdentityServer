// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.Logging.Models;

internal class EndSessionRequestValidationLog
{
    public string ClientId { get; set; }
    public string ClientName { get; set; }
    public string SubjectId { get; set; }

    public string PostLogOutUri { get; set; }
    public string State { get; set; }

    public Dictionary<string, string> Raw { get; set; }

    public EndSessionRequestValidationLog(ValidatedEndSessionRequest request)
    {
        Raw = request.Raw.ToScrubbedDictionary(OidcConstants.EndSessionRequest.IdTokenHint);

        SubjectId = "unknown";
            
        var subjectClaim = request.Subject?.FindFirst(JwtClaimTypes.Subject);
        if (subjectClaim != null)
        {
            SubjectId = subjectClaim.Value;
        }

        if (request.Client != null)
        {
            ClientId = request.Client.ClientId;
            ClientName = request.Client.ClientName;
        }

        PostLogOutUri = request.PostLogOutUri;
        State = request.State;
    }

    public override string ToString()
    {
        return LogSerializer.Serialize(this);
    }
}