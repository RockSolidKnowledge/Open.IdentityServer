// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for UserClaim classes.
/// </summary>
public static class UserClaimMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for List of Type
    /// </summary>
    /// <param name="userClaims">The list of claim entities to operate on; may be <see langword="null"/>.</param>
    /// <typeparam name="TClaim">Class that implements <see cref="Entities.UserClaim"/></typeparam>
    extension<TClaim>(List<TClaim>? userClaims)
        where TClaim: Entities.UserClaim
    {
        /// <summary>
        /// Maps to collection of strings using the <see cref="Entities.UserClaim.Type"/> value
        /// </summary>
        /// <returns>A collection of claim type strings mapped from <paramref name="userClaims"/>, or an empty collection when <paramref name="userClaims"/> is <see langword="null"/>.</returns>
        public ICollection<string> ToModel() => userClaims?
            .Select(scope => scope.Type).ToList() ?? [];
    }

    /// <summary>
    /// Mapping extension methods for Collection of string
    /// </summary>
    /// <param name="userClaims">The collection of claim type strings to operate on; may be <see langword="null"/>.</param>
    extension(ICollection<string>? userClaims)
    {
        /// <summary>
        /// Maps to list of Type using the assigning string value to <see cref="Entities.UserClaim.Type"/> property
        /// </summary>
        /// <typeparam name="TClaim">class that implements <see cref="Entities.UserClaim"/></typeparam>
        /// <returns>A list of <typeparamref name="TClaim"/> mapped from <paramref name="userClaims"/>, or an empty list when <paramref name="userClaims"/> is <see langword="null"/>.</returns>
        public List<TClaim> ToEntity<TClaim>()
            where TClaim: Entities.UserClaim, new() => userClaims?
            .Select(claim => new TClaim
            {
                Type = claim,
            }).ToList() ?? [];
    }
}