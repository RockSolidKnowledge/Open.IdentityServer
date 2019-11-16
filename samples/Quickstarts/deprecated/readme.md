# IdentityServer 4 Quickstart Samples

## Quickstart #1: Securing an API using Client Credentials

The first quickstart sets up a minimal identityserver that shows how to protect an API using the OAuth 2.0 client credentials grant.
This approach is typically used for server to server communication.

## Quickstart #2: Securing an API using the Resource Owner Password Grant

This quickstart adds support for the OAuth 2.0 resource owner password grant. 
This allows a client to send a user's name and password to identityserver to request a token representing that user.

> **note** The resource owner password grant is only recommended for so called "trusted clients" - in many cases you are better off with an OpenID Connect based flow for user authentication.
Nevertheless, this sample allows for an easy way to introduce users in identityserver - that's why we included it.

## Quickstart #3: User Authentication using OpenID Connect Implicit Flow

This quickstart adds support for interactive user authentication using the OpenID Connect implicit flow.

## Quickstart #4: Adding external Authentication

This quickstart adds support for Google authentication.

## Quickstart #5: OpenID Connect Hybrid Flow Authentication and API Access Tokens

This quickstart adds support for hybrid flow authentication.

## Quickstart #6: JavaScript clients

This quickstart shows how to build a JavaScript client for IdentityServer.

## Quickstart #7: EntityFramework configuration

This quickstart shows how to use EntityFramework for the configuration data.

## Quickstart #8: IdentityServer and ASP.NET Identity

This quickstart uses ASP.NET Identity for identity management.

## Quickstart #9 IdentityServer with ASP.NET Identity and EF Core

This quickstart shows how you could put both quickstart #8 and #9 together into a single IdentityServer host.


