// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Open.IdentityServer.Models;

namespace IdentityServer.Models;

public class SessionsListViewModel
{
    public IReadOnlyCollection<UserSession> Sessions { get; init; } = [];
    public IReadOnlyCollection<string> Usernames { get; init; } = [];
    public string SelectedUsername { get; init; } = string.Empty;
}
