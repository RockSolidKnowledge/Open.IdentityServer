using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace SampleApi
{
    // this middleware validate the cnf claim (if present) against the thumbprint of the X.509 client certificate for the current client
    public class ConfirmationValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ConfirmationValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext ctx)
        {
            if (ctx.User.Identity.IsAuthenticated)
            {
                var cnfJson = ctx.User.FindFirst("cnf")?.Value;
                if (!String.IsNullOrWhiteSpace(cnfJson))
                {
                    var certResult = await ctx.AuthenticateAsync("x509");
                    if (!certResult.Succeeded)
                    {
                        await ctx.ChallengeAsync("x509");
                        return;
                    }

                    var cert = ctx.Connection.ClientCertificate;
                    if (cert == null)
                    {
                        await ctx.ChallengeAsync("x509");
                        return;
                    }

                    var thumbprint = cert.Thumbprint;

                    var cnf = JObject.Parse(cnfJson);
                    var sha256 = cnf.Value<string>("x5t#S256");

                    if (String.IsNullOrWhiteSpace(sha256) ||
                        !thumbprint.Equals(sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        await ctx.ChallengeAsync("token");
                        return;
                    }
                }
            }

            await _next(ctx);
        }
    }
}