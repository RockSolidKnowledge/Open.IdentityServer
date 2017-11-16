.. _refProfileService:
Profile Service
===============

Often IdentityServer requires identity information about users when creating tokens or when handling requests to the userinfo or introspection endpoints.
By default, IdentityServer only has the claims in the authentication cookie to draw upon for this identity data.

It is impractical to put all of the possible claims needed for users into the cookie, so IdentityServer defines an extensibility point for allowing claims to be dynamically loaded as needed for a user.
This extensibility point is the ``IProfileService`` and it is common for a developer to implement this interface to access a custom database or API that contains the identity data for users.

IProfileService APIs
^^^^^^^^^^^^^^^^^^^^

``GetProfileDataAsync``
    The API that is expected to load claims for a user. It is passed an instance of ``ProfileDataRequestContext``.

``IsActiveAsync``
    The API that is expected to indicate if a user is currently allowed to obtain tokens. It is passed an instance of ``IsActiveContext``.

ProfileDataRequestContext
^^^^^^^^^^^^^^^^^^^^^^^^^

Models the request for user claims and is the vehicle to return those claims. It contains these properties:

``Subject``
    The ``ClaimsPrincipal`` modeling the user. If the request The claims from the user's cookie will be in the ``ClaimsPrincipal``.
``Client``
    The ``Client`` for which the claims are being requested.
``RequestedClaimTypes``
    The collection of claim types being requested.
``Caller``
    An identifier for the context in which the claims are being requested (e.g. an identity token, an access token, or the user info endpoint). The constant ``IdentityServerConstants.ProfileDataCallers`` contains the different constant values.
``IssuedClaims``
    The list of ``Claim`` s that will be returned. This is expected to be populated by the custom ``IProfileService`` implementation.
``AddRequestedClaims``
    Extension method on the ``ProfileDataRequestContext`` to populate the ``IssuedClaims``, but first filters the claims based on ``RequestedClaimTypes``.

IsActiveContext
^^^^^^^^^^^^^^^

Models the request to determine is the user is currently allowed to obtain tokens. It contains these properties:

``Subject``
    The ``ClaimsPrincipal`` modeling the user. If the request The claims from the user's cookie will be in the ``ClaimsPrincipal``.
``Client``
    The ``Client`` for which the claims are being requested.
``Caller``
    An identifier for the context in which the claims are being requested (e.g. an identity token, an access token, or the user info endpoint). The constant ``IdentityServerConstants.ProfileDataCallers`` contains the different constant values.
``IsActive``
    The flag indicating if the user is allowed to obtain tokens. This is expected to be assigned by the custom ``IProfileService`` implementation.
