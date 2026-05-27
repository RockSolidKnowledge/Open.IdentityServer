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
`samples <https://github.com/RockSolidKnowledge/Open.IdentityServer/tree/main/samples/Quickstarts>`_ folder.

Preparation
^^^^^^^^^^^
The first thing you should do is clone the repository and pick the quickstart you want to work with. Each quickstart has a corresponding sample solution in the samples folder.

.. note:: If you are using private NuGet sources do not forget to add the --nuget-source parameter: --nuget-source https://api.nuget.org/v3/index.json

OK - let's get started!

.. note:: The quickstarts target the Open.IdentityServer 1.x and .NET 10.0 and use Asp.NET MVC.
