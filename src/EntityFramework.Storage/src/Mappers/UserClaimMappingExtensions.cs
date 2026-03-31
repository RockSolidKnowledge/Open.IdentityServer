#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace OpenIdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for UserClaim classes.
/// </summary>
public static class UserClaimMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for List of Type
    /// </summary>
    /// <param name="userClaims">list of Type src</param>
    /// <typeparam name="TClaim">class that implements <see cref="Entities.UserClaim"/></typeparam>
    extension<TClaim>(List<TClaim>? userClaims)
        where TClaim: Entities.UserClaim
    {
        /// <summary>
        /// Maps to collection of strings using the <see cref="Entities.UserClaim.Type"/> value
        /// </summary>
        /// <returns>a collections of string</returns>
        public ICollection<string> ToModel() => userClaims?
            .Select(scope => scope.Type).ToList() ?? [];
    }

    /// <summary>
    /// Mapping extension methods for Collection of string
    /// </summary>
    /// <param name="userClaims"></param>
    extension(ICollection<string>? userClaims)
    {
        /// <summary>
        /// Maps to list of Type using the assigning string value to <see cref="Entities.UserClaim.Type"/> property
        /// </summary>
        /// <typeparam name="TClaim">class that implements <see cref="Entities.UserClaim"/></typeparam>
        /// <returns>a list of Type</returns>
        public List<TClaim> ToEntity<TClaim>()
            where TClaim: Entities.UserClaim, new() => userClaims?
            .Select(claim => new TClaim
            {
                Type = claim,
            }).ToList() ?? [];
    }
}