// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// The service responsible for performing back-channel signout requests.
    /// </summary>
    public interface IBackChannelLogoutService
    {
        /// <summary>
        /// Performs the http back-channel logout request notifications for the collection of clients.
        /// </summary>
        /// <param name="clients">The collection of clients that require back channel signout notifications for this session.</param>
        Task SendLogoutNotificationsAsync(IEnumerable<BackChannelLogoutModel> clients);
    }
}
