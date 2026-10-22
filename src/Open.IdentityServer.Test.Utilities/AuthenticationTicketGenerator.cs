using System.Buffers.Text;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Open.IdentityServer.Test.Utilities;

public static class TestsClaimTypeConstants
{
    public const string Subject = "sub";
    public const string Name = "name";
    public const string IdentityProvider = "idp";
    public const string AuthenticationTime = "auth_time";
    public const string AuthenticationMethod = "amr";
    public const string Role = "role";
    public const string Issuer = "iss";
    public const string SessionId = "session_id";
    public const string ClientListKey = "client_list";
}

public static class AuthenticationTicketGenerator
{
    public static ClaimsPrincipal CreatePrincipal(
        string subjectId,
        string? displayName = null,
        string? identityProvider = null,
        DateTime? authenticationTime = null,
        string[]? authenticationMethods = null,
        Claim[]? additionalClaims = null)
    {
        if (string.IsNullOrWhiteSpace(subjectId)) throw new ArgumentException("SubjectId is mandatory", nameof(subjectId));
        var claims = new List<Claim> { new Claim(TestsClaimTypeConstants.Subject, subjectId) };

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            claims.Add(new Claim(TestsClaimTypeConstants.Name, displayName));
        }

        if (!string.IsNullOrWhiteSpace(identityProvider))
        {
            claims.Add(new Claim(TestsClaimTypeConstants.IdentityProvider, identityProvider));
        }

        if (authenticationTime.HasValue)
        {
            claims.Add(new Claim(TestsClaimTypeConstants.AuthenticationTime, new DateTimeOffset(authenticationTime.Value).ToUnixTimeSeconds().ToString()));
        }

        if (authenticationMethods != null && authenticationMethods.Length != 0)
        {
            foreach (var amr in authenticationMethods)
            {
                claims.Add(new Claim(TestsClaimTypeConstants.AuthenticationMethod, amr));
            }
        }

        if (additionalClaims != null)
        {
            claims.AddRange(additionalClaims);
        }

        var id = new ClaimsIdentity(claims, "Open.IdentityServer", TestsClaimTypeConstants.Name, TestsClaimTypeConstants.Role);
        return new ClaimsPrincipal(id);
    }
    
    public static AuthenticationTicket GenerateAuthenticationTicket(
        string authScheme, 
        string? subjectId,
        string? sessionId,
        string? displayName = null,
        DateTimeOffset? issuedUtc = null, 
        DateTimeOffset? expiresUtc = null,
        string? issuer = null,
        string[]? clientIds = null)
    {
        ClaimsPrincipal principle = CreatePrincipal(subjectId ?? string.Empty, displayName: displayName);
        AuthenticationProperties properties = new();

        properties.Items[TestsClaimTypeConstants.SessionId] = sessionId;
        properties.IssuedUtc = issuedUtc;
        properties.ExpiresUtc = expiresUtc;
        
        if (clientIds != null && clientIds.Any())
        {
            properties.Items[TestsClaimTypeConstants.ClientListKey] = GeneratePropertyList(clientIds);
        }

        if (issuer != null)
        {
            properties.Items[TestsClaimTypeConstants.Issuer] = issuer;
        }

        return new AuthenticationTicket(principle, properties, authScheme);
    }
    
    private static string GeneratePropertyList(IEnumerable<string> list)
    {
        if (list != null && list.Any())
        {
            var value = JsonSerializer.Serialize(list);
            var bytes = Encoding.UTF8.GetBytes(value);
            value = Encode(bytes);
            return value;
        }

        return null;
    }
    
    private static string Encode(byte[] arg)
    {
        var s = Convert.ToBase64String(arg); // Standard base64 encoder
            
        s = s.Split('=')[0]; // Remove any trailing '='s
        s = s.Replace('+', '-'); // 62nd char of encoding
        s = s.Replace('/', '_'); // 63rd char of encoding
            
        return s;
    }
}