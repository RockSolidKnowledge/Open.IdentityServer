.. _refResources:
Defining Resources
==================
The ultimate job of an OpenID Connect/OAuth token service is to control access to resources.

The two fundamental resource types in IdentityServer are:

* identity resources: these are claims about a user like user ID, display name, email address etc…
* API resources: this is functionality a client wants to access. Typically, they are HTTP-based endpoints (aka APIs), but could be also message queuing endpoint or similar.

.. note:: You can define resources using a C# object model - or load them from a data store. An implementation of ``IResourceStore`` deals with these low-level details. For this document we are using the in-memory implementation.

Identity Resources
------------------
An identity resource is a named logical grouping of claims.

The OpenID Connect specification `suggests <https://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims>`_ a couple of standard 
scope name to claim type mappings that might be useful to you for inspiration, but you can freely design them yourself.

One of them is actually mandatory, the *openid* scope, which tells the provider to return the *sub* (subject id) claim in the identity token.

This is how you could define the openid scope in code::

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResource(
                name: "openid",
                claimTypes: new[] { "sub" },
                displayName: "Your user identifier")
        };
    }

But since this is one of the standard scopes from the spec you can shorten that to::

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId()
        };
    }

.. note:: see the reference section for more information on ``IdentityResource``.

The following example shows custom identity resource called *profile* that represents the display name, email address and website claim::

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResource(
                name: "profile",
                claimTypes: new[] { "name", "email", "website" },
                displayName: "Your profile data")
        };
    }

Once the resource is defined, you can give access to it to a client via the ``AllowedScopes`` option (other properties omitted)::

    var client = new Client
    {
        ClientId = "client",
        
        AllowedScopes = { "openid", "profile" }
    };


The client can then request the resource using the scope parameter (other parameters omitted)::

    https://demo.identityserver.io/connect/authorize?client_id=client&scope=openid profile

IdentityServer will then use the scope names to create a list of requested claim types, 
and present that to your implementation of the :ref:`profile service <refProfileService>`.

APIs
----
Designing your API surface can be a complicated task. IdentityServer provides a couple of primitives to help you with that.

The original OAuth 2.0 specification has the concept of scopes, which is just defined as *the scope of access* that the client requests.
Technically speaking, the *scope* parameter is a list of space delimited values - you need to provide the structure and semantics of it.

In more complex systems, often the notion of a *resource* is introduced. This might be e.g. a physical or logical API. 
In turn each API can potentially have scopes as well. Some scope might be exclusive to that resource, and some scope might be shared.

Let's start with simple scopes first, and then we'll have a look how resources can help structure scopes.

Scopes
^^^^^^
Let's model something very simple - a system that has three logical operations *read*, *write*, and *delete*.

You can define them using the ``ApiScope`` class::

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope(name: "read",   displayName: "Read your data."),
            new ApiScope(name: "write",  displayName: "Write your data."),
            new ApiScope(name: "delete", displayName: "Delete your data.")
        };
    }

You can then assign the scope to various clients, e.g.::

    var webViewer = new Client
    {
        ClientId = "web_viewer",
        
        AllowedScopes = { "openid", "profile", "read" }
    };

    var mobileApp = new Client
    {
        ClientId = "mobile_app",
        
        AllowedScopes = { "openid", "profile", "read", "write", "delete" }
    }

Authorization based on Scopes
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
When a client asks for a scope (and that scope is allowed via configuration and not denied via consent), 
the value of that scope will be included in the resulting access token as a claim of type *scope* (for both JWTs and introspection), e.g.::

    {
        "typ": "at+jwt"
    }
    {
        "client_id": "mobile_app",
        "sub": "123",

        "scope": "read write delete"
    }

The consumer of the access token can use that data to make sure that the client is actually allowed to invoke the corresponding functionality.

.. note:: Be aware, that scopes are purely for authorizing clients - not users. IOW - the *write* scope allows the client to invoke the functionality associated with that. Still that client can most probably only write the data the belongs to the current user.

You can add more identity information about the user by deriving additional claims from the scope request. The following scope definition tells the configuration system,
that when a *write* scope gets granted, the *user_level* claim should be added to the access token::

    var writeScope = new ApiScope(
        name: "write",
        displayName: "Write your data.",
        claimTypes: new[] { "user_level" });

This will pass the *user_level* claim as a requested claim type to the profile service, 
so that the consumer of the access token can use this data as input for authorization decisions or business logic.

Parameterized Scopes
^^^^^^^^^^^^^^^^^^^^
Sometimes scopes have a certain structure, e.g. a scope name with an additional parameter: *transaction:id* or *read_patient:patientid*.

In this case you would create a scope without the parameter part and assign that name to a client, but in addition provide some logic to parse the structure
of the scope at runtime using the ``IResourceValidator`` interace, e.g.::

    public class ParameterizedScopeValidator : ResourceValidator
    {
        public ParameterizedScopeValidator(IResourceStore store, ILogger<ResourceValidator> logger) : base(store, logger)
        {
        }

        public override Task<ParsedScopeValue> ParseScopeValue(string scopeValue)
        {
            const string transactionScopeName = "transaction";
            const string separator = ":";
            const string transactionScopePrefix = transactionScopeName + separator;

            if (scopeValue.StartsWith(transactionScopePrefix))
            {
                var parts = scopeValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                return Task.FromResult(new ParsedScopeValue(transactionScopeName, scopeValue, parts[1]));
            }

            return base.ParseScopeValue(scopeValue);
        }
    }

You then have access to the parsed value throughout the pipeline, e.g. in the profile service::

    public class HostProfileService : IProfileService
    {
        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var transaction = context.RequestedResources.ParsedScopes.FirstOrDefault(x => x.Name == "transaction");
            if (transaction?.ParameterValue != null)
            {
                context.IssuedClaims.Add(new Claim("transaction_id", transaction.ParameterValue));
            }
        }
    }

API Resources
^^^^^^^^^^^^^
When the API surface gets larger, a flat list of scopes like the one used above might not be feasible.

You typically need to introduce some sort of namespacing to organize the scope names, and maybe you also want to group them together and 
get some higher-level constructs like an *audience* claim in access tokens.
You might also have scenarios, where multiple resources should support the same scope names, whereas sometime you explicitly want to isolate a scope to a certain resource.

In IdentityServer, the ``ApiResource`` class allows some additional organization. Let's use the following scope definition::

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope(name: "invoice.read",   displayName: "Reads your invoices."),
            new ApiScope(name: "invoice.pay",    displayName: "Pays your invoices."),

            new ApiScope(name: "customer.read",    displayName: "Reads you customers information."),
            new ApiScope(name: "customer.contact", displayName: "Allows contacting one of your customers.")

            new ApiScope(name: "manage", displayName: "Provides administrative access to invoice and customer data.")
        };
    }

With ``ApiResource`` you can now create two logical APIs and their correponding scopes::

    public static readonly IEnumerable<ApiResource> GetApiResources()
    { 
        return new List<ApiResource>
        {
            new ApiResource("invoices", "Invoice API")
            {
                Scopes = { "invoice.read", "invoice.pay", "manage" }
            },
            
            new ApiResource("customers", "Customer API")
            {
                Scopes = { "customer.read", "customer.contact", "manage" }
            }
        };
    }

Using the API resource grouping gives you the following additional features

* support for the JWT *aud* claim. The value(s) of the audience claim will be the name of the API resource
* support for adding common user claims across all contained scopes
* support for introspection by assigning a API secret to the resource
* support for configuring the access token signing algorithm for the resource
