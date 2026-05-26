.. _refInternalisedIdentityModel:
Internalised IdentityModel Resources
====================================

With Open.IdentityServer we have internalised constants and helpers needed, meaning that references to an IdentityModel package are no longer needed.

Internalised Content from IdentityModel package
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

.. list-table::
       :header-rows: 1

       * - Original Full Class Name
         - New Full Class Name

       * - IdentityModel.OidcConstants
         - Open.IdentityServer.OidcConstants
       * - IdentityModel.JwtClaimTypes
         - Open.IdentityServer.JwtClaimTypes
         
       * - IdentityModel.X509
         - Open.IdentityServer.Utility.X509.X509
       * - IdentityModel.X509CertificatesFinder
         - Open.IdentityServer.Utility.X509.X509CertificatesFinder
       * - IdentityModel.X509CertificatesLocation
         - Open.IdentityServer.Utility.X509.X509CertificatesLocation
       * - IdentityModel.X509CertificatesName
         - Open.IdentityServer.Utility.X509.X509CertificatesName

       * - IdentityModel.Base64Url
         - Open.IdentityServer.Utility.Base64Url
       * - IdentityModel.ClaimComparer
         - Open.IdentityServer.ClaimComparer
       * - IdentityModel.CryptoRandom
         - Open.IdentityServer.Utility.CryptoRandom
       * - IdentityModel.Identity
         - Open.IdentityServer.Utility.Identity
       * - IdentityModel.Principal
         - Open.IdentityServer.Utility.Principal
       * - IdentityModel.StringExtensions
         - Open.IdentityServer.Utility.StringExtensions
       * - IdentityModel.TimeConstantComparer
         - Open.IdentityServer.Utility.TimeConstantComparer

Recommendations for Client Side
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

For client-side applications, there are external 3rd party libraries available, including the 
Duende.IdentityModel free open-source package, which is the latest version of the package
that IdentityServer4 used.