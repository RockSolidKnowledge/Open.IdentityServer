using System;
using Microsoft.IdentityModel.Tokens;

namespace Open.IdentityServer.Models;

/// <summary>
/// 
/// </summary>
public class SigningKey
{
    /// <summary>
    /// Get or set unique identifier
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Get or set created date time
    /// </summary>
    public DateTime Created { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public SigningCredentials Credentials { get; set; }
}