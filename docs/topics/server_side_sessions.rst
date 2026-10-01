.. _refServerSideSessions:

Server-Side Sessions
====================

Overview
--------
When users authenticate with Open.IdentityServer, a session is created to track the logged in user. By default this will be done by storing this state in cookies in the user browser. This approach of storing session state in a cookie can work well for many scenarios but does have some drawbacks.

* **No Tracking Active Sessions**       - There is no way to track active sessions, and how many users are currently logged in.
* **No Immediate Revocation**           - There will be no process of Immediate session revocation on the server side-session cookie will be valid till it expires, or they log out.
* **No Sign-Out Coordination**          - Coordinating sign-outs from Open.IdentityServer with connected clients is less relable without server tracking of active sessions.

Open.IdentityServer Server-Side Sessions solves these issues by storing the contents of this cookie in a server side data store. This gives Open.IdentityServer the ability to:

* Provide APIs for managing and querying active user sessions
* Support for explicit session revocation regardless of the cookie state in the browser
* Storing session data server side, so browser cookie only contains an ID for the session nothing more

Getting Started
^^^^^^^^^^^^^^^

1. **Database schema**

   Ensure your database is in the correct state. If you are coming from Duende IdentityServer, there is nothing further to do - the schema is already compatible. If you are migrating from IdentityServer4 and have not yet updated your schema to match the Open.IdentityServer schema, you will need to do this first. See :ref:`migration from IdentityServer4 <refMigrateFromIdS4>` for details.

2. **Enable server-side sessions**

   Call ``.AddServerSideSessions()`` when configuring Open.IdentityServer:

   .. code-block:: csharp

       builder.Services.AddIdentityServer()
           .AddServerSideSessions();

3. **Configure a session store**

   ``AddServerSideSessions`` requires an implementation of ``IServerSideSessionStore`` to persist session data. If you are using Entity Framework Core, this is provided automatically when you configure the operational store:

   .. code-block:: csharp

       builder.Services.AddIdentityServer()
           .AddServerSideSessions()
           .AddOperationalStore(options =>
           {
               options.ConfigureDbContext = b =>
                   b.UseSqlServer("ConnectionString");
           });

   If you are not using the built-in EF Core store, you will need to provide your own ``IServerSideSessionStore`` implementation.

5. **(Optional) Configure additional session options**

   You can customize behavior via ``ServerSideSessionOptions``, such as how often sessions are checked for expiration in the background, or coordinating this with your sign-in cookie expiration:

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

Management Interface
^^^^^^^^^^^^^^^^^^^^

Open.IdentityServer has a built-in session management interface, ``ISessionManagementService``, that allows you to query existing sessions and remove sessions. When you configure Open.IdentityServer to use server-side sessions, a default implementation is registered for this interface.

The interface provides two methods:

- ``Task<QueryResult<UserSession>> ISessionManagementService.QuerySessionsAsync(SessionQuery? filter, CancellationToken ct = default)``

  Filters sessions using the ``SessionQuery`` object. The filter is optional; default values are used when it is not provided.

- ``Task ISessionManagementService.RemoveSessionsAsync(RemoveSessionsContext context, CancellationToken ct = default)``

  Removes sessions using the ``RemoveSessionsContext`` object. The context allows you to control which sessions are removed and what actions are taken as part of the removal.

Data Types
^^^^^^^^^^

The following types are used by ``ISessionManagementService`` to query and manage sessions.

SessionQuery
""""""""""""

``SessionQuery`` is used to filter results when calling ``QuerySessionsAsync``.

.. list-table::
   :header-rows: 1
   :widths: 30 20 50

   * - Property
     - Type
     - Description
   * - ``ResultsToken``
     - ``string?``
     - Optional selector for current page location, contains identifier for the first element and last element in page of results. e.g. '0,24'
   * - ``RequestPriorResults``
     - ``bool``
     - Specifies if instead of getting next page should get the previous page that ResultsToken identifies. Only valid if ResultsToken is specified.
   * - ``CountRequested``
     - ``int``
     - Specifies the count requested per page, defaults to 25
   * - ``SubjectId``
     - ``string?``
     - Filters sessions with a specific subject id
   * - ``SessionId``
     - ``string?``
     - Filters sessions with a specific session id
   * - ``DisplayName``
     - ``string?``
     - Filters sessions with a specific display name

QueryResult<T>
""""""""""""""

``QueryResult<T>`` wraps paged results returned from ``QuerySessionsAsync``.

.. list-table::
   :header-rows: 1
   :widths: 30 20 50

   * - Property
     - Type
     - Description
   * - ``ResultsToken``
     - ``string?``
     - Token identifying the current page of results. Contains the first element and last element in page of results. e.g. '0,24'
   * - ``HasPrevResults``
     - ``bool``
     - Specifies if there is a page of results before this page
   * - ``HasNextResults``
     - ``bool``
     - Specifies if there is a page of results after this page
   * - ``TotalCount``
     - ``int?``
     - Total sessions accross all pages of results
   * - ``TotalPages``
     - ``int?``
     - Total number of pages for query result
   * - ``CurrentPage``
     - ``int?``
     - Page numer of the current result collection
   * - ``Results``
     - ``IReadOnlyCollection<T>``
     - The collection of results for the current page

UserSession
"""""""""""

``UserSession`` represents a single server-side session record.

.. list-table::
   :header-rows: 1
   :widths: 30 20 50

   * - Property
     - Type
     - Description
   * - ``SubjectId``
     - ``string``
     - The subject (user) identifier associated with the session.
   * - ``SessionId``
     - ``string``
     - The unique identifier for the session
   * - ``DisplayName``
     - ``string?``
     - An optional display name for the user
   * - ``Created``
     - ``DateTime``
     - The date and time the session was created.
   * - ``Renewed``
     - ``DateTime``
     - The date and time the session was last renewed
   * - ``Expires``
     - ``DateTime?``
     - The date and time the session expires, if applicable
   * - ``Issuer``
     - ``string?``
     - The issuer of the authentication ticket
   * - ``Clients``
     - ``IEnumerable<string>``
     - The clients associated with the session
   * - ``AuthenticationTicket``
     - ``AuthenticationTicket?``
     - The user sessions suthentication ticket object

RemoveSessionsContext
""""""""""""""""""""""

``RemoveSessionsContext`` controls which sessions are removed and what side effects occur when calling ``RemoveSessionsAsync``.

.. list-table::
   :header-rows: 1
   :widths: 30 20 50

   * - Property
     - Type
     - Description
   * - ``SubjectId``
     - ``string?``
     - Optional if SessionId has value, limits removal to a specific subject id
   * - ``SessionId``
     - ``string?``
     - Optional if SubjectId has value, limits removal to a specific session id
   * - ``ClientIds``
     - ``IEnumerable<string>?``
     - Optionally limits removal to sessions for specific clients
   * - ``RemoveServerSideSession``
     - ``bool``
     - Whether to trigger session entity removal from the database
   * - ``SendBackchannelLogoutNotification``
     - ``bool``
     - Whether to trigger backchannel logout notifications for removed sessions
   * - ``RevokeTokens``
     - ``bool``
     - Whether to trigger session token removal
   * - ``RevokeConsents``
     - ``bool``
     - Whether to trigger session consent removal