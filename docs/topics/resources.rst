.. _refResources:
Defining Resources
==================

The first thing you will typically define in your system are the resources that you want to protect.
That could be identity information of your users, like profile data or email addresses, or access to APIs.

.. note:: You can define resources using a C# object model - or load them from a data store. An implementation of ``IResourceStore`` deals with these low-level details. For this document we are using the in-memory implementation.

Defining identity resources
^^^^^^^^^^^^^^^^^^^^^^^^^^^
Identity resources are data like user ID, name, or email address of a user.
An identity resource has a unique name, and you can assign arbitrary claim types to it. These claims will then be included in the identity token for the user.
The client will use the ``scope`` parameter to request access to an identity resource.

The OpenID Connect specification specifies a couple of `standard <https://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims>`_ identity resources.
The minimum requirement is, that you provide support for emitting a unique ID for your users - also called the subject id.
This is done by exposing the standard identity resource called ``openid``::

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId()
        };
    }

The `IdentityResources` class supports all scopes defined in the specification (openid, email, profile, telephone, and address).
If you want to support them all, you can add them to your list of supported identity resources::

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(), 
            new IdentityResources.Email(),
            new IdentityResources.Profile(),
            new IdentityResources.Phone(),
            new IdentityResources.Address()
        };
    }

Defining custom identity resources
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
You can also define custom identity resources. Create a new `IdentityResource` class, give it a name and optionally a display name and description 
and define which user claims should be included in the identity token when this resource gets requested::

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        var customProfile = new IdentityResource(
            name: "custom.profile",
            displayName: "Custom profile",
            claimTypes: new[] { "name", "email", "status" });

        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            customProfile
        };
    }

See the :ref:`reference <refIdentityResource>` section for more information on identity resource settings.

Defining API resources
^^^^^^^^^^^^^^^^^^^^^^
To allow clients to request access tokens for APIs, you need to define API resources, e.g.::

To get access tokens for APIs, you also need to register them as a scope. This time the scope type is of type `Resource`::

    public static IEnumerable<ApiResource> GetApis()
    {
        return new[]
        {
            // simple API with a single scope (in this case the scope name is the same as the api name)
            new ApiResource("api1", "Some API 1"),
            
            // expanded version if more control is needed
            new ApiResource
            {
                Name = "api2",
                
                // secret for using introspection endpoint
                ApiSecrets =
                {
                    new Secret("secret".Sha256())
                },

                // include the following using claims in access token (in addition to subject id)
                UserClaims = { JwtClaimTypes.Name, JwtClaimTypes.Email },

                // this API defines two scopes
                Scopes =
                {
                    new Scope()
                    {
                        Name = "api2.full_access",
                        DisplayName = "Full access to API 2",
                    },
                    new Scope
                    {
                        Name = "api2.read_only",
                        DisplayName = "Read only access to API 2"
                    }
                }
            }
        };
    }

See the :ref:`reference <refApiResource>` section for more information on API resource settings.

.. note:: The user claims defined by resources are loaded by the :ref:`IProfileService <refProfileService>` extensibility point.
