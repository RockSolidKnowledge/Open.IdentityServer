#nullable enable

using System;

#pragma warning disable 1591

namespace Open.IdentityServer.EntityFramework.Entities;

public class IdentityServerServerSideSessions
{
    public int Id { get; set; }
    public string Key { get; set; }             //100, non null
    public string Scheme { get; set; }          //100, non null
    public string SubjectId { get; set; }       //100, non null
    public string? SessionId { get; set; }      //100
    public string? DisplayName { get; set; }    //100
    public DateTime Created { get; set; }       
    public DateTime Renewed { get; set; }
    public DateTime? Expires { get; set; }
    public string Data { get; set; }            //max
}