// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Open.IdentityServer.Models.ManageViewModels;

public class EnableAuthenticatorViewModel
{
    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Verification Code")]
    public string Code { get; set; }

    [ReadOnly(true)]
    public string SharedKey { get; set; }

    public string AuthenticatorUri { get; set; }
}