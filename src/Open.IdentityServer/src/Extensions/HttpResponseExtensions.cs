// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Diagnostics.CodeAnalysis;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Open.IdentityServer.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpResponse"/>.
/// </summary>
[SuppressMessage(
    "Usage", 
    "ASP0019:Suggest using IHeaderDictionary.Append or the indexer", 
    Justification = "Maintain throwing ArgumentException if the header is already set.")]
public static class HttpResponseExtensions
{
    /// <summary>
    /// Serializes the specified object to JSON and writes it to the response body.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="o">The object to serialize and write.</param>
    /// <param name="contentType">The content type header value. Defaults to <c>application/json; charset=UTF-8</c>.</param>
    public static async Task WriteJsonAsync(this HttpResponse response, object o, string contentType = null)
    {
        var json = ObjectSerializer.ToString(o);
        await response.WriteJsonAsync(json, contentType);
        await response.Body.FlushAsync();
    }

    /// <summary>
    /// Writes a JSON string to the response body.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="json">The JSON string to write.</param>
    /// <param name="contentType">The content type header value. Defaults to <c>application/json; charset=UTF-8</c>.</param>
    public static async Task WriteJsonAsync(this HttpResponse response, string json, string contentType = null)
    {
        response.ContentType = contentType ?? "application/json; charset=UTF-8";
        await response.WriteAsync(json);
        await response.Body.FlushAsync();
    }

    /// <summary>
    /// Sets cache control headers on the response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="maxAge">The maximum age in seconds. A value of 0 sets no-cache headers.</param>
    /// <param name="varyBy">Optional request header names to include in the Vary header.</param>
    public static void SetCache(this HttpResponse response, int maxAge, params string[] varyBy)
    {
        if (maxAge == 0)
        {
            SetNoCache(response);
        }
        else if (maxAge > 0)
        {
            if (!response.Headers.ContainsKey("Cache-Control"))
            {
                response.Headers.Add("Cache-Control", $"max-age={maxAge}");
            }

            if (varyBy?.Any() == true)
            {
                var vary = varyBy.Aggregate((x, y) => x + "," + y);
                if (response.Headers.ContainsKey("Vary"))
                {
                    vary = response.Headers["Vary"].ToString() + "," + vary;
                }
                response.Headers["Vary"] = vary;
            }
        }
    }

    /// <summary>
    /// Sets no-cache headers on the response to prevent caching.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    public static void SetNoCache(this HttpResponse response)
    {
        if (!response.Headers.ContainsKey("Cache-Control"))
        {
            response.Headers.Add("Cache-Control", "no-store, no-cache, max-age=0");
        }
        else
        {
            response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        }

        if (!response.Headers.ContainsKey("Pragma"))
        {
            response.Headers.Add("Pragma", "no-cache");
        }
    }

    /// <summary>
    /// Writes an HTML string to the response body with UTF-8 encoding.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="html">The HTML string to write.</param>
    public static async Task WriteHtmlAsync(this HttpResponse response, string html)
    {
        response.ContentType = "text/html; charset=UTF-8";
        await response.WriteAsync(html, Encoding.UTF8);
        await response.Body.FlushAsync();
    }

    /// <summary>
    /// Redirects the response to the specified URL, resolving relative URLs against the IdentityServer base URL.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="url">The URL to redirect to.</param>
    public static void RedirectToAbsoluteUrl(this HttpResponse response, string url)
    {
        if (url.IsLocalUrl())
        {
            if (url.StartsWith("~/")) url = url.Substring(1);
            url = response.HttpContext.GetIdentityServerBaseUrl().EnsureTrailingSlash() + url.RemoveLeadingSlash();
        }
        response.Redirect(url);
    }

    /// <summary>
    /// Adds Content Security Policy headers for inline scripts to the response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="options">The CSP options controlling the policy level and deprecated header behaviour.</param>
    /// <param name="hash">The hash of the inline script to allow.</param>
    public static void AddScriptCspHeaders(this HttpResponse response, CspOptions options, string hash)
    {
        var csp1part = options.Level == CspLevel.One ? "'unsafe-inline' " : string.Empty;
        var cspHeader = $"default-src 'none'; script-src {csp1part}'{hash}'";

        AddCspHeaders(response.Headers, options, cspHeader);
    }

    /// <summary>
    /// Adds Content Security Policy headers for inline styles to the response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="options">The CSP options controlling the policy level and deprecated header behaviour.</param>
    /// <param name="hash">The hash of the inline style to allow.</param>
    /// <param name="frameSources">Optional frame sources to include in the CSP frame-src directive.</param>
    public static void AddStyleCspHeaders(this HttpResponse response, CspOptions options, string hash, string frameSources)
    {
        var csp1part = options.Level == CspLevel.One ? "'unsafe-inline' " : string.Empty;
        var cspHeader = $"default-src 'none'; style-src {csp1part}'{hash}'";

        if (!string.IsNullOrEmpty(frameSources))
        {
            cspHeader += $"; frame-src {frameSources}";
        }

        AddCspHeaders(response.Headers, options, cspHeader);
    }

    /// <summary>
    /// Adds Content Security Policy headers to the specified header dictionary.
    /// </summary>
    /// <param name="headers">The response header dictionary to add CSP headers to.</param>
    /// <param name="options">The CSP options controlling whether the deprecated header is also added.</param>
    /// <param name="cspHeader">The CSP header value to set.</param>
    public static void AddCspHeaders(IHeaderDictionary headers, CspOptions options, string cspHeader)
    {
        if (!headers.ContainsKey("Content-Security-Policy"))
        {
            headers.Add("Content-Security-Policy", cspHeader);
        }
        if (options.AddDeprecatedHeader && !headers.ContainsKey("X-Content-Security-Policy"))
        {
            headers.Add("X-Content-Security-Policy", cspHeader);
        }
    }
}