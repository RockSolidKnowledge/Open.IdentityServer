// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Diagnostics;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models the common data of API and identity resources.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class Resource
    {
        private string DebuggerDisplay => Name ?? $"{{{typeof(Resource)}}}";

        // todo: brock add this ?
        ///// <summary>
        ///// Ctor for Resource.
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="displayName"></param>
        ///// <param name="claimTypes"></param>
        //protected Resource(string name, string displayName, IEnumerable<string> claimTypes)
        //{
        //    if (name.IsMissing()) throw new ArgumentNullException(nameof(name));

        //    Name = name;
        //    DisplayName = displayName;

        //    foreach (var type in claimTypes)
        //    {
        //        UserClaims.Add(type);
        //    }
        //}

        /// <summary>
        /// Indicates if this resource is enabled. Defaults to true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The unique name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name of the resource.
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Description of the resource.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether this scope is shown in the discovery document. Defaults to true.
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// List of accociated user claims that should be included when this resource is requested.
        /// </summary>
        public ICollection<string> UserClaims { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the custom properties for the resource.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}