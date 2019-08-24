// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder extension methods for registering crypto services
    /// </summary>
    public static class IdentityServerBuilderExtensionsCrypto
    {
        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="credential">The credential.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, SigningCredentials credential)
        {
            if (!(credential.Key is AsymmetricSecurityKey
                || credential.Key is JsonWebKey && ((JsonWebKey)credential.Key).HasPrivateKey))
            {
                throw new InvalidOperationException("Signing key is not asymmetric");
            }

            if (!IdentityServerConstants.SupportedSigningAlgorithms.Contains(credential.Algorithm, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Signing algorithm {credential.Algorithm} is not supported.");
            }

            if (credential.Key is ECDsaSecurityKey key && !IsValidCurveForAlgorithm(key, credential.Algorithm))
            {
                throw new InvalidOperationException("Invalid curve for signing algorithm");
            }

            builder.Services.AddSingleton<ISigningCredentialStore>(new DefaultSigningCredentialsStore(credential));
            builder.Services.AddSingleton<IValidationKeysStore>(new DefaultValidationKeysStore(new[] { credential.Key }));

            return builder;
        }

        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="signingAlgorithm">The signing algorithm (defaults to RS256)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">X509 certificate does not have a private key.</exception>
        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, X509Certificate2 certificate, string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));

            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException("X509 certificate does not have a private key.");
            }

            var credential = new SigningCredentials(new X509SecurityKey(certificate), signingAlgorithm);
            return builder.AddSigningCredential(credential);
        }

        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The name.</param>
        /// <param name="location">The location.</param>
        /// <param name="nameType">Name parameter can be either a distinguished name or a thumbprint</param>
        /// <param name="signingAlgorithm">The signing algorithm (defaults to RS256)</param>
        /// <exception cref="InvalidOperationException">certificate: '{name}'</exception>
        public static IIdentityServerBuilder AddSigningCredential(
            this IIdentityServerBuilder builder,
            string name,
            StoreLocation location = StoreLocation.LocalMachine,
            NameType nameType = NameType.SubjectDistinguishedName,
            string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            var certificate = FindCertificate(name, location, nameType);
            if (certificate == null) throw new InvalidOperationException($"certificate: '{name}' not found in certificate store");

            return builder.AddSigningCredential(certificate, signingAlgorithm);
        }

        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="key">The key.</param>
        /// <param name="signingAlgorithm">The signing algorithm</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, SecurityKey key, string signingAlgorithm)
        {
            var credential = new SigningCredentials(key, signingAlgorithm);
            return builder.AddSigningCredential(credential);
        }

        /// <summary>
        /// Sets the temporary signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="persistKey">Specifies if the temporary key should be persisted to disk.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="signingAlgorithm">The signing algorithm (defaults to RS256)</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddDeveloperSigningCredential(
            this IIdentityServerBuilder builder,
            bool persistKey = true,
            string filename = null,
            string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            if (filename == null)
            {
                filename = Path.Combine(Directory.GetCurrentDirectory(), "tempkey.rsa");
            }

            if (File.Exists(filename))
            {
                var keyFile = File.ReadAllText(filename);
                var tempKey = JsonConvert.DeserializeObject<CryptoHelper.TemporaryRsaKey>(keyFile, new JsonSerializerSettings { ContractResolver = new CryptoHelper.RsaKeyContractResolver() });

                return builder.AddSigningCredential(CryptoHelper.CreateRsaSecurityKey(tempKey.Parameters, tempKey.KeyId), signingAlgorithm);
            }
            else
            {
                var key = CryptoHelper.CreateRsaSecurityKey();

                RSAParameters parameters;

                if (key.Rsa != null)
                {
                    parameters = key.Rsa.ExportParameters(includePrivateParameters: true);
                }
                else
                {
                    parameters = key.Parameters;
                }

                var tempKey = new CryptoHelper.TemporaryRsaKey
                {
                    Parameters = parameters,
                    KeyId = key.KeyId
                };

                if (persistKey)
                {
                    File.WriteAllText(filename, JsonConvert.SerializeObject(tempKey, new JsonSerializerSettings { ContractResolver = new CryptoHelper.RsaKeyContractResolver() }));
                }
                
                return builder.AddSigningCredential(key, signingAlgorithm);
            }
        }

        /// <summary>
        /// Adds the validation keys.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="certificates">The certificates.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddValidationKeys(this IIdentityServerBuilder builder, params X509Certificate2[] certificates)
        {
            var keys = certificates.Select(certificate => new X509SecurityKey(certificate)).Cast<AsymmetricSecurityKey>();

            builder.Services.AddSingleton<IValidationKeysStore>(new DefaultValidationKeysStore(keys));

            return builder;
        }
   
        /// <summary>
        /// Adds the validation keys.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddValidationKeys(this IIdentityServerBuilder builder, params AsymmetricSecurityKey[] keys)
        {
            builder.Services.AddSingleton<IValidationKeysStore>(new DefaultValidationKeysStore(keys));

            return builder;
        }

        /// <summary>
        /// Adds the validation key.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="certificate">The certificate.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IIdentityServerBuilder AddValidationKey(this IIdentityServerBuilder builder, X509Certificate2 certificate)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));

            var key = new X509SecurityKey(certificate);
            return builder.AddValidationKeys(key);
        }

        /// <summary>
        /// Adds the validation key from the certificate store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The name.</param>
        /// <param name="location">The location.</param>
        /// <param name="nameType">Name parameter can be either a distinguished name or a thumbprint</param>
        public static IIdentityServerBuilder AddValidationKey(this IIdentityServerBuilder builder, string name, StoreLocation location = StoreLocation.LocalMachine, NameType nameType = NameType.SubjectDistinguishedName)
        {
            var certificate = FindCertificate(name, location, nameType);
            if (certificate == null) throw new InvalidOperationException($"certificate: '{name}' not found in certificate store");

            return builder.AddValidationKey(certificate);
        }

        private static X509Certificate2 FindCertificate(string name, StoreLocation location, NameType nameType)
        {
            X509Certificate2 certificate = null;

            if (location == StoreLocation.LocalMachine)
            {
                if (nameType == NameType.SubjectDistinguishedName)
                {
                    certificate = X509.LocalMachine.My.SubjectDistinguishedName.Find(name, validOnly: false).FirstOrDefault();
                }
                else if (nameType == NameType.Thumbprint)
                {
                    certificate = X509.LocalMachine.My.Thumbprint.Find(name, validOnly: false).FirstOrDefault();
                }
            }
            else
            {
                if (nameType == NameType.SubjectDistinguishedName)
                {
                    certificate = X509.CurrentUser.My.SubjectDistinguishedName.Find(name, validOnly: false).FirstOrDefault();
                }
                else if (nameType == NameType.Thumbprint)
                {
                    certificate = X509.CurrentUser.My.Thumbprint.Find(name, validOnly: false).FirstOrDefault();
                }
            }

            return certificate;
        }

        private static bool IsValidCurveForAlgorithm(ECDsaSecurityKey key, string algorithm)
        {
            var parameters = key.ECDsa.ExportParameters(false);

            if (algorithm == SecurityAlgorithms.EcdsaSha256 && parameters.Curve.Oid.Value != Constants.CurveOids.P256
                || algorithm == SecurityAlgorithms.EcdsaSha384 && parameters.Curve.Oid.Value != Constants.CurveOids.P384
                || algorithm == SecurityAlgorithms.EcdsaSha512 && parameters.Curve.Oid.Value != Constants.CurveOids.P521)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Describes the string so we know what to search for in certificate store
    /// </summary>
    public enum NameType
    {
        /// <summary>
        /// subject distinguished name
        /// </summary>
        SubjectDistinguishedName,

        /// <summary>
        /// thumbprint
        /// </summary>
        Thumbprint
    }
}
