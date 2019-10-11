using Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace SampleApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors();
            services.AddDistributedMemoryCache();

            services.AddAuthentication("token")
                .AddIdentityServerAuthentication("token", options =>
                {
                    options.Authority = Constants.Authority;
                    options.RequireHttpsMetadata = false;

                    options.ApiName = "api1";
                    options.ApiSecret = "secret";

                    options.JwtBearerEvents = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnTokenValidated = e =>
                        {
                            var jwt = e.SecurityToken as JwtSecurityToken;
                            var type = jwt.Header.Typ;

                            if (!string.Equals(type, "at+jwt", StringComparison.Ordinal))
                            {
                                e.Fail("JWT is not an access token");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
                //.AddCertificate("x509", options =>
                //{
                //    options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

                //    options.Events = new CertificateAuthenticationEvents
                //    {
                //        OnValidateCertificate = context =>
                //        {
                //            context.Principal = Principal.CreateFromCertificate(context.ClientCertificate, includeAllClaims: true);
                //            context.Success();

                //            return Task.CompletedTask;
                //        }
                //    };
                //});
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "http://localhost:28895",
                    "http://localhost:7017");

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseMiddleware<ConfirmationValidationMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}