Packaging and Builds
====================

IdentityServer consists of a number of nuget packages.

Open.IdentityServer main repo
^^^^^^^^^^^^^^^
`github <https://github.com/RockSolidKnowledge/Open.IdentityServer>`_

Contains the core IdentityServer object model, services and middleware as well as the EntityFramework and ASP.NET Identity integration.

nugets:

* `Open.IdentityServer <https://www.nuget.org/packages/Open.IdentityServer/>`_
* `Open.IdentityServer.EntityFramework <https://www.nuget.org/packages/Open.IdentityServer.EntityFramework>`_
* `Open.IdentityServer.AspNetIdentity <https://www.nuget.org/packages/Open.IdentityServer.AspNetIdentity>`_

Quickstart UI
^^^^^^^^^^^^^
`github <https://github.com/IdentityServer/IdentityServer4.Quickstart.UI>`_
Contains a simple starter UI including login, logout and consent pages.

Templates
^^^^^^^^^
`nuget <https://www.nuget.org/packages/IdentityServer4.Templates>`_

Contains templates for the original IdentityServer4, can be used for Open.IdenityServer with little work to replce the refrences to old packages and
update framework used to net10.0. In the future we will create some update templates.