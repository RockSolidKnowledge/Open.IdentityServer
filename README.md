## About IdentityServer4
[<img align="right" width="100px" src="https://dotnetfoundation.org/images/logo_big.svg" />](https://dotnetfoundation.org/projects?q=identityserver&type=project)

IdentityServer is a free, open source [OpenID Connect](http://openid.net/connect/) and [OAuth 2.0](https://tools.ietf.org/html/rfc6749) framework for ASP.NET Core.
Founded and maintained by [Dominick Baier](https://twitter.com/leastprivilege) and [Brock Allen](https://twitter.com/brocklallen), IdentityServer4 incorporates all the protocol implementations and extensibility points needed to integrate token-based authentication, single-sign-on and API access control in your applications.
IdentityServer4 is officially [certified](https://openid.net/certification/) by the [OpenID Foundation](https://openid.net) and thus spec-compliant and interoperable.
It is part of the [.NET Foundation](https://www.dotnetfoundation.org/), and operates under their [code of conduct](https://www.dotnetfoundation.org/code-of-conduct). It is licensed under [Apache 2](https://opensource.org/licenses/Apache-2.0) (an OSI approved license).

For project documentation, please visit [readthedocs](https://identityserver4.readthedocs.io).

## Overview
IdentityServer4 consists of multiple repositories (in addition to this repository):

* [Samples](https://github.com/IdentityServer/IdentityServer4.Samples)
* [Access token validation handler for ASP.NET Core](https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation)
* [Quickstart UI](https://github.com/IdentityServer/IdentityServer4.Quickstart.UI)
* [ASP.NET Identity integration](https://github.com/IdentityServer/IdentityServer4.AspNetIdentity)
* [EntityFramework integration](https://github.com/IdentityServer/IdentityServer4.EntityFramework)
* [Templates](https://github.com/IdentityServer/IdentityServer4.Templates)

If you encounter issues or find bugs, please open an issue in this repo here first.

## How to build
IdentityServer is built against the latest ASP.NET Core 2.

* [Install](https://www.microsoft.com/net/download/core#/current) the latest .NET Core 2.x SDK
* Run `build.ps1` (Powershell) or `build.sh` (bash)

## ASP.NET Core 1.x
For using IdentityServer with ASP.NET Core 1.x, you can use the 1.x packages of IdentityServer and [this branch](https://github.com/IdentityServer/IdentityServer4/tree/aspnetcore1). Documentation for 1.x can be found [here](http://docs.identityserver.io/en/aspnetcore1/). The 1.x version is not maintained anymore.

## Commercial and Community Support
If you need help with implementing IdentityServer4 or your security architecture in general, there are both free and commercial support options.
See [here](https://identityserver4.readthedocs.io/en/release/intro/support.html) for more details.

## Sponsorship
If you are a fan of the project or a company that relies on IdentityServer, you might want to consider sponsoring.
This will help us devote more time to answering questions and doing feature development. If you are interested please head to our [Patreon](https://www.patreon.com/identityserver) page which has further details.

### Platinum Sponsors

**Auth0 <span><img src="https://user-images.githubusercontent.com/1801923/31792116-d4fca9ec-b512-11e7-92eb-56e8d3df8e70.png" height="28" align="top"></span>**

Auth0 is an easy to implement authentication and identity management SaaS based on OpenID Connect/OAuth 2.0. Check out their quickstarts [here](https://auth0.com/docs?utm_source=GHsponsor&utm_medium=GHsponsor&utm_campaign=identityserver).

### Corporate Sponsors

[Thinktecture AG](https://www.thinktecture.com)  
[Ritter Insurance Marketing](https://www.ritterim.com)  
[Alcidion Corporation](https://www.alcidion.com)  
[Intuit](https://www.intuit.com)  

You can see a list of our current sponsors [here](https://github.com/IdentityServer/IdentityServer4/blob/release/SPONSORS.md) - and for companies we have some nice advertisement options as well.

## Acknowledgements
IdentityServer4 is built using the following great open source projects

* [ASP.NET Core](https://github.com/aspnet)
* [Json.Net](http://www.newtonsoft.com/json)
* [Cake](http://cakebuild.net/)
* [XUnit](https://xunit.github.io/)
* [Fluent Assertions](http://www.fluentassertions.com/)

..and last but not least a big thanks to all our [contributors](https://github.com/IdentityServer/IdentityServer4/graphs/contributors)!
