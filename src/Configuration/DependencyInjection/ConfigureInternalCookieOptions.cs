using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServer4.Configuration
{
    internal class ConfigureInternalCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IdentityServerOptions _idsrv;

        public ConfigureInternalCookieOptions(IdentityServerOptions idsrv)
        {
            _idsrv = idsrv;
        }

        public void Configure(CookieAuthenticationOptions options)
        {
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name == IdentityServerConstants.DefaultCookieAuthenticationScheme)
            {
                options.SlidingExpiration = _idsrv.Authentication.CookieSlidingExpiration;
                options.ExpireTimeSpan = _idsrv.Authentication.CookieLifetime;
                options.Cookie.Name = IdentityServerConstants.DefaultCookieAuthenticationScheme;
                options.Cookie.SameSite = SameSiteMode.None;

                options.LoginPath = ExtractLocalUrl(_idsrv.UserInteraction.LoginUrl);
                options.LogoutPath = ExtractLocalUrl(_idsrv.UserInteraction.LogoutUrl);
                if (_idsrv.UserInteraction.LoginReturnUrlParameter != null)
                {
                    options.ReturnUrlParameter = _idsrv.UserInteraction.LoginReturnUrlParameter;
                }
            }

            if (name == IdentityServerConstants.ExternalCookieAuthenticationScheme)
            {
                options.Cookie.Name = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            }
        }

        private static string ExtractLocalUrl(string url)
        {
            if (url.IsLocalUrl())
            {
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(1);
                }

                return url;
            }

            return null;
        }
    }

    internal class PostConfigureInternalCookieOptions : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly IdentityServerOptions _idsrv;
        private readonly IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> _authOptions;
        private readonly ILogger _logger;

        public PostConfigureInternalCookieOptions(
            IdentityServerOptions idsrv,
            IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> authOptions,
            ILoggerFactory loggerFactory)
        {
            _idsrv = idsrv;
            _authOptions = authOptions;
            _logger = loggerFactory.CreateLogger("IdentityServer4.Startup");

        }

        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            var scheme = _idsrv.Authentication.CookieAuthenticationScheme ??
                _authOptions.Value.DefaultAuthenticateScheme ??
                _authOptions.Value.DefaultScheme;

            if (name == scheme)
            {
                _idsrv.UserInteraction.LoginUrl = _idsrv.UserInteraction.LoginUrl ?? options.LoginPath;
                _idsrv.UserInteraction.LoginReturnUrlParameter = _idsrv.UserInteraction.LoginReturnUrlParameter ?? options.ReturnUrlParameter;
                _idsrv.UserInteraction.LogoutUrl = _idsrv.UserInteraction.LogoutUrl ?? options.LogoutPath;

                _logger.LogDebug("Login Url: {url}", _idsrv.UserInteraction.LoginUrl);
                _logger.LogDebug("Login Return Url Parameter: {param}", _idsrv.UserInteraction.LoginReturnUrlParameter);
                _logger.LogDebug("Logout Url: {url}", _idsrv.UserInteraction.LogoutUrl);

                _logger.LogDebug("ConsentUrl Url: {url}", _idsrv.UserInteraction.ConsentUrl);
                _logger.LogDebug("Consent Return Url Parameter: {param}", _idsrv.UserInteraction.ConsentReturnUrlParameter);

                _logger.LogDebug("Error Url: {url}", _idsrv.UserInteraction.ErrorUrl);
                _logger.LogDebug("Error Id Parameter: {param}", _idsrv.UserInteraction.ErrorIdParameter);
            }
        }
    }
}