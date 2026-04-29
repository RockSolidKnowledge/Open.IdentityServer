// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Open.IdentityServer.Models.ManageViewModels;

public class ExternalLoginsViewModel
{
    public IList<UserLoginInfo> CurrentLogins { get; set; }

    public IList<AuthenticationScheme> OtherLogins { get; set; }

    public bool ShowRemoveButton { get; set; }

    public string StatusMessage { get; set; }
}