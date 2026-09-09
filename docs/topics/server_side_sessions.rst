.. _refServerSideSessions:

Server-Side Sessions
====================

Overview
--------
When users authenticate with Open.IdentityServer, a session is created to track the logged in user. By default this will be done
storing this state in cookies in the user browser. This approach of storing session state in a cookie can work well for many scenarios
but does have some drawbacks.

* **No Tracking Active Sessions**       - There is no way to track active sessions, and how many users are currently logged in.
* **No Immidiate Revokation**           - There will be no process of immidiate session revokation on the serevr ad a session cookie will be valid till it expires, or they log out.
* **No Sign-Out Coordination**          - Coordinating sign-outs from Open.IdentityServer with connected clients is less relable without server tracking of active sessions.

Open.IdentityServer Server-Side Sessions solves these issues by storing the contents of this cookie in a server side data store. This enables
in Open.IdentityServer the ability to:

* Provide APIs for manging and querying active user sessions
* Support for explicit session revokation regardless of the cookie state in the browser
* Storing session data server side, so browser cookie only contains an ID for the session nothing more

Getting Started
^^^^^^^^^^^^^^^

1. **Database schema**

   Ensure your database is in the correct state. If you are coming from Duende
   IdentityServer, there is nothing further to do - the schema is already compatible.
   If you are migrating from IdentityServer4 and have not yet updated your schema to
   match the Open.IdentityServer schema, you will need to do this first. See
   :ref:`migration from IdentityServer4 <refMigrateFromIdS4>` for details.

2. **Enable server-side sessions**

   Call ``.AddServerSideSessions()`` when configuring Open.IdentityServer:

   .. code-block:: csharp

       builder.Services.AddIdentityServer()
           .AddServerSideSessions();

3. **Configure a session store**

   ``AddServerSideSessions`` requires an implementation of ``IServerSideSessionStore``
   to persist session data. If you are using Entity Framework Core, this is provided
   automatically when you configure the operational store:

   .. code-block:: csharp

       builder.Services.AddIdentityServer()
           .AddServerSideSessions()
           .AddOperationalStore(options =>
           {
               options.ConfigureDbContext = b =>
                   b.UseSqlServer("ConnectionString");
           });

   If you are not using the built-in EF Core store, you will need to provide your own
   ``IServerSideSessionStore`` implementation.

5. **(Optional) Configure additional session options**

   You can customize behavior via ``ServerSideSessionOptions``, such as how often
   sessions are checked for expiration in the background, or coordinating this with
   your sign-in cookie expiration:

   .. code-block:: csharp

       builder.Services.AddIdentityServer(options =>
       {
           // Other options...
           options.ServerSideSessions.ExpiredSessionsTriggerBackchannelLogout = true;
           options.ServerSideSessions.RemoveExpiredSessions = true;
           options.ServerSideSessions.RemoveExpiredSessionsFrequency = TimeSpan.FromSeconds(10);
           options.ServerSideSessions.FuzzExpiredSessionsFrequency = true;
           options.ServerSideSessions.RemoveExpiredSessionsBatchSize = 100;
       });

Managemet Interface
^^^^^^^^^^^^^^^^^^^

TODO: not yet implemented