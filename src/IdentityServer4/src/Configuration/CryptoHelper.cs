using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Crypto helper
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// Creates a new RSA security key.
        /// </summary>
        /// <returns></returns>
        public static RsaSecurityKey CreateRsaSecurityKey(int keySize = 2048)
        {
            return new RsaSecurityKey(RSA.Create(keySize))
            {
                KeyId = CryptoRandom.CreateUniqueId(16)
            };
        }

        /// <summary>
        /// Creates an RSA security key.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static RsaSecurityKey CreateRsaSecurityKey(RSAParameters parameters, string id)
        {
            var key = new RsaSecurityKey(parameters)
            {
                KeyId = id
            };

            return key;
        }

        /// <summary>
        /// Creates the hash for the various hash claims (e.g. c_hash, at_hash or s_hash).
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="tokenSigningAlgorithm">The token signing algorithm</param>
        /// <returns></returns>
        public static string CreateHashClaimValue(string value, string tokenSigningAlgorithm)
        {
            using (var sha = GetHashAlgorithmForSigningAlgorithm(tokenSigningAlgorithm))
            {
                var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(value));
                var size = (sha.HashSize / 8) / 2;

                var leftPart = new byte[size];
                Array.Copy(hash, leftPart, size);

                return Base64Url.Encode(leftPart);
            }
        }

        /// <summary>
        /// Returns the matching hashing algorithm for a token signing algorithm
        /// </summary>
        /// <param name="signingAlgorithm">The signing algorithm</param>
        /// <returns></returns>
        public static HashAlgorithm GetHashAlgorithmForSigningAlgorithm(string signingAlgorithm)
        {
            var signingAlgorithmBits = int.Parse(signingAlgorithm.Substring(signingAlgorithm.Length - 3));

            switch (signingAlgorithmBits)
            {
                case 256:
                    return SHA256.Create();
                case 384:
                    return SHA384.Create();
                case 512:
                    return SHA512.Create();
                default:
                    throw new InvalidOperationException($"Invalid signing algorithm: {signingAlgorithm}");
            }
        }

        // used for serialization to temporary RSA key
        internal class TemporaryRsaKey
        {
            public string KeyId { get; set; }
            public RSAParameters Parameters { get; set; }
        }

        internal class RsaKeyContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                property.Ignored = false;

                return property;
            }
        }
    }
}