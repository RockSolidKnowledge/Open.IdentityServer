#nullable enable

using System;

#pragma warning disable 1591

namespace Open.IdentityServer.EntityFramework.Entities;

public class IdentityServerPushedAuthorizationRequests
{
    public int Id { get; set; }
    public string ReferenceHashValue { get; set; }  //64
    public DateTime Created { get; set; }
    public string Parameters { get; set; }          //max
}