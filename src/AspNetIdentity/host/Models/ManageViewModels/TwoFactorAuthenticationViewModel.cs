// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Models.ManageViewModels;

public class TwoFactorAuthenticationViewModel
{
    public bool HasAuthenticator { get; set; }

    public int RecoveryCodesLeft { get; set; }

    public bool Is2faEnabled { get; set; }
}