#nullable enable

using System;

#pragma warning disable 1591

namespace Open.IdentityServer.EntityFramework.Entities;

public class IdentityServerIdentityProvider
{
    public int Id { get; set; }
    public string Scheme { get; set; } //200, non null
    public string? DisplayName { get; set; }    //200
    public bool Enabled { get; set; } = true;      
    public string Type { get; set; }   //20, non null
    public string? Properties { get; set; }     //max
    public DateTime Created { get; set; } //non null
    public DateTime? LastAccessed { get; set; }
    public bool NonEditable { get; set; }
    public DateTime? Updated { get; set; }
}