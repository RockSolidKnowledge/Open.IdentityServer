Windows Authentication
======================

On supported platforms, you can use IdentityServer to authenticate users using Windows authentication (e.g. against Active Directory).
Currently Windows authentication is available when you host IdentityServer using:

* `Kestrel <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel>`_ on Windows using IIS and the IIS integration package
* `HTTP.sys <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/httpsys>`_ server on Windows
* The Negotiate authentication handler from Microsoft (requires .NET Core 3.x or higher)

In all cases, Windows authentication is triggered by using the ``ChallengeAsync`` API on the ``HttpContext``.

The ``ExternalController`` in our `quickstart UI <https://github.com/IdentityServer/IdentityServer4.Quickstart.UI>`_ implements the necessary logic.

Using Kestrel
^^^^^^^^^^^^^
When using Kestrel, you must run "behind" IIS and use the IIS integration::

    var host = new WebHostBuilder()
        .UseKestrel()
        .UseUrls("http://localhost:5000")
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>()
        .Build();


Kestrel is automatically configured when using the ``WebHost.CreateDefaultBuilder`` approach for setting up the ``WebHostBuilder``.

Also, if you are hosting in IIS/IIS Express, the virtual directory in must have Windows and anonymous authentication enabled.

The IIS integration layer will configure a Windows authentication handler into DI that can be invoked via the authentication service.
Typically in IdentityServer it is advisable to disable this automatic behavior. 

This is done in ``ConfigureServices``::

    // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
    services.Configure<IISOptions>(iis =>
    {
        iis.AuthenticationDisplayName = "Windows";
        iis.AutomaticAuthentication = false;
    });

    // ..or configures IIS in-proc settings
    services.Configure<IISServerOptions>(iis =>
    {
        iis.AuthenticationDisplayName = "Windows";
        iis.AutomaticAuthentication = false;
    });

.. Note:: By default, the display name is empty, and the Windows authentication button will not show up in the quickstart UI. You need to set a display name if you rely on automatic discovery of external providers.
