.. _refTemplatesDescriptions:
Descriptions
=========

This section highlights each Open.IdentityServer template and why you choose it as a starting point for your application. All templates serve as a starting point for development and customization.

Open.IdentityServer Empty
^^^^^^^^^^^^^^^^^^^^^^^^^

To create a .NET project using this template run the following command:

.. code-block:: console

    dotnet new oids-empty

This will create a barebones, no UI, Open.IdentityServer project. It contains 3 files: ``Config.cs``, ``Program.cs``, and ``HostingExtensions.cs``.

``Config.cs`` can be modified to add clients, scopes, and claims as these configurations will be loaded from in-memory collections.