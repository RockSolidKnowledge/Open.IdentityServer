.. _refQuickstartOverview:
Overview
========
The quickstarts provide step by step instructions for various common IdentityServer scenarios.
They start with the absolute basics and become more complex - 
it is recommended you do them in order.

* adding IdentityServer to an ASP.NET Core application
* configuring IdentityServer
* issuing tokens for various clients
* securing web applications and APIs
* adding support for EntityFramework based configuration
* adding support for ASP.NET Identity

Every quickstart has a reference solution - you can find the code in the 
`samples <https://github.com/IdentityServer/IdentityServer4/tree/master/samples/Quickstarts>`_ folder.

Preparation
^^^^^^^^^^^
The first thing you should do is install our templates::

    dotnet new -i IdentityServer4.Templates

They will be used as a starting point for the various tutorials.

OK - let's get started!

.. note:: The quickstarts target the latest version of IdentityServer and ASP.NET Core (3.0) - there are also quickstarts for `ASP.NET Core 2 <http://docs.identityserver.io/en/aspnetcore2/quickstarts/0_overview.html>`_ and `ASP.NET Core 1 <http://docs.identityserver.io/en/aspnetcore1/quickstarts/0_overview.html>`_.