Packaging and Builds
====================

IdentityServer consists of a number of nuget packages.

IdentityServer4 main repo
^^^^^^^^^^^^^^^
`github <https://github.com/identityserver/IdentityServer4>`_

Contains the core IdentityServer object model, services and middleware as well as the EntityFramework and ASP.NET Identity integration.

nugets:

* `IdentityServer4 <https://www.nuget.org/packages/IdentityServer4/>`_
* `IdentityServer4.EntityFramework <https://www.nuget.org/packages/IdentityServer4.EntityFramework>`_
* `IdentityServer4.AspNetIdentity <https://www.nuget.org/packages/IdentityServer4.AspNetIdentity>`_

Quickstart UI
^^^^^^^^^^^^^
`github <https://github.com/IdentityServer/IdentityServer4.Quickstart.UI>`_

Contains a simple starter UI including login, logout and consent pages.

Access token validation handler
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
`nuget <https://www.nuget.org/packages/IdentityServer4.AccessTokenValidation>`_ | `github <https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation>`_

ASP.NET Core authentication handler for validating tokens in APIs. The handler allows supporting both JWT and reference tokens in the same API.

Templates
^^^^^^^^^
`nuget <https://www.nuget.org/packages/IdentityServer4.Templates>`_ | `github <https://github.com/IdentityServer/IdentityServer4.Templates>`_

Contains templates for the dotnet CLI.

Dev builds
^^^^^^^^^^
In addition we publish CI builds to our package repository.
Add the following ``nuget.config`` to your project::

    <?xml version="1.0" encoding="utf-8"?>
        <configuration>
            <packageSources>
                <clear />
                <add key="CI" value="https://pkgs.dev.azure.com/netidentity/IdentityServer/_packaging/CI/nuget/v3/index.json" />
            </packageSources>
        </configuration>