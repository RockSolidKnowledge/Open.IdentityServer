Welcome to Open IdentityServer
=============================================

.. image:: images/OpenIdentityServer.png
   :align: center

Open IdentityServer is an OpenID Connect and OAuth 2.0 framework for ASP.NET.

.. note:: 
   Open IdentityServer is an independent open‑source project led by Rock Solid Knowledge, based on the Apache 2.0 licensed IdentityServer4 codebase. It is not affiliated with or endorsed by Duende Software or the original IdentityServer4 authors.

   At Rock Solid Knowledge, we are committed to keeping Open IdentityServer free open source indefinitely. Additional products and services are available on `IdentityServer.com <https://www.identityserver.com/>`_, which contribute to the long-term sustainability of the project.

It enables the following features in your applications:


| **Authentication as a Service** 
| Centralized login logic and workflow for all of your applications (web, native, mobile, services). Open IdentityServer is an implementation of OpenID Connect.

| **Single Sign-on / Sign-out** 
| Single sign-on (and out) over multiple application types.

| **Access Control for APIs** 
| Issue access tokens for APIs for various types of clients, e.g. server to server, web applications, SPAs and native/mobile apps.

| **Federation Gateway**
| Support for external identity providers like Azure Active Directory, Google, Facebook etc. This shields your applications from the details of how to connect to these external providers.

| **Focus on Customization**
| The most important part - many aspects of Open IdentityServer can be customized to fit **your** needs. Since Open IdentityServer is a framework and not a boxed product or a SaaS, you can write code to adapt the system the way it makes sense for your scenarios.

| **Mature Open Source**
| Open IdentityServer uses the permissive `Apache 2 <https://www.apache.org/licenses/LICENSE-2.0>`_ license that allows building commercial products on top of it.

| **Free and Commercial Support**
| If you need help building or running your identity platform, :ref:`let us know <refSupport>`. There are several ways we can help you out.

.. note:: These docs cover the latest version on the main branch.

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Introduction

   intro/big_picture
   intro/architecture
   intro/terminology
   intro/specs
   intro/packaging
   intro/support
   intro/test
   intro/contributing

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Quickstarts

   quickstarts/0_overview
   quickstarts/1_client_credentials
   quickstarts/2_interactive_aspnetcore
   quickstarts/3_aspnetcore_and_apis
   quickstarts/4_javascript_client
   quickstarts/5_entityframework
   quickstarts/6_aspnet_identity

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Migrating

   migrating/duendeidentityserver
   migrating/identityserver4
   
.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Configuration

   configuration/startup
   configuration/resources
   configuration/clients
   configuration/mvc
   configuration/apis

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Topics

   topics/startup
   topics/compatibility
   topics/resources
   topics/clients
   topics/signin
   topics/signin_external_providers
   topics/windows
   topics/signout
   topics/signout_external_providers
   topics/signout_federated
   topics/federation_gateway
   topics/consent
   topics/apis
   topics/deployment
   topics/logging
   topics/events
   topics/crypto
   topics/grant_types
   topics/client_authentication
   topics/extension_grants
   topics/resource_owner
   topics/refresh_tokens
   topics/reference_tokens
   topics/persisted_grants
   topics/resource_indicators
   topics/pop
   topics/mtls
   topics/request_object
   topics/custom_token_request_validation
   topics/cors
   topics/discovery
   topics/add_apis
   topics/add_protocols
   topics/tools

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Endpoints

   endpoints/discovery
   endpoints/authorize
   endpoints/token
   endpoints/userinfo
   endpoints/device_authorization
   endpoints/introspection
   endpoints/revocation
   endpoints/endsession

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Reference

   reference/options
   reference/identity_resource
   reference/api_scope
   reference/api_resource
   reference/client
   reference/grant_validation_result
   reference/profileservice
   reference/interactionservice
   reference/deviceflow_interactionservice
   reference/ef
   reference/aspnet_identity

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Misc

   misc/blogs
   misc/videos
