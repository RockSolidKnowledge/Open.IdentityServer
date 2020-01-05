using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace IdentityServer4.Extensions
{
    /// <summary>
    /// Extensions methods for X509Certificate2
    /// </summary>
    public static class X509CertificateExtensions
    {
        /// <summary>
        /// Create the value of a thumbprint-based cnf claim
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static string CreateThumbprintCnf(this X509Certificate2 certificate)
        {
            var values = new Dictionary<string, string>
            {
                { "x5t#S256", certificate.Thumbprint.ToLowerInvariant() }
            };
            var cnf = JsonConvert.SerializeObject(values);

            return cnf;
        }
    }
}