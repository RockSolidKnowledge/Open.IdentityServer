.. _refTemplatesOverview:
Overview
========
Open.IdentityServer includes a set of project templates, created to help you get started quickly.
The templates are available for ASP.NET Core 10.0 and can be installed using the .NET CLI.
They cover various scenarios ranging from a barebones Open.IdentityServer application to a more fleshed out application highlighting some of the various features of Open.IdentityServer.
These templates can be used in conjunction with our :ref:`Quickstarts <refQuickstartOverview>` explaining how to build an application using Open.IdentityServer.

Templates
^^^^^^^^^^^
You can install the templates using the .NET CLI by running the following command:

.. code-block:: console

    dotnet new install Open.IdentityServer.Templates

Then check the templates installed correctly by running the following command:

.. code-block:: console

    dotnet new list Open.IdentityServer

You should see a list of the following templates:

.. list-table::
   :header-rows: 1
   :widths: 60 40

   * - Template Name
     - Short Name
     - Language
     - Tags
   * - Open.IdentityServer Empty
     - open-identityserver-empty
     - C#
     - Web/IdentityServer

Once you are happy the templates have installed correctly, move on to the next section to learn how to create a new project using one of the templates.