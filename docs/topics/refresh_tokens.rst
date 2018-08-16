Refresh Tokens
==============
Since access tokens have finite lifetimes, refresh tokens allow requesting new access tokens without user interaction.

Refresh tokens are supported for the following flows: authorization code, hybrid and resource owner password credential flow.
The clients needs to be explicitly authorized to request refresh tokens by setting ``AllowOfflineAccess`` to ``true``.

Additional client settings
^^^^^^^^^^^^^^^^^^^^^^^^^^
``AbsoluteRefreshTokenLifetime``
    Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds / 30 days. Zero allows refresh tokens that, when used with ``RefreshTokenExpiration = Sliding`` only expire after the SlidingRefreshTokenLifetime is passed.
``SlidingRefreshTokenLifetime``
    Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds / 15 days
``RefreshTokenUsage``
    ``ReUse`` the refresh token handle will stay the same when refreshing tokens
    
    ``OneTime`` the refresh token handle will be updated when refreshing tokens
``RefreshTokenExpiration``
    ``Absolute`` the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
    
    ``Sliding`` when refreshing the token, the lifetime of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime). The lifetime will not exceed `AbsoluteRefreshTokenLifetime`.
``UpdateAccessTokenClaimsOnRefresh``
    Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request.
