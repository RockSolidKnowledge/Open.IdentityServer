using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AnotherMvcClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<BackChannelSessionStore>();
            builder.Services.AddSingleton(new BackChannelLogoutTokenValidator("https://localhost:5001", "anotherMvc"));

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies", o =>
                {
                    o.Cookie.Name = "AnotherMvcClient";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5001";

                    options.ClientId = "anotherMvc";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.Scope.Add("api1");

                    options.SaveTokens = true;
                });

            builder.Services.AddOptions<CookieAuthenticationOptions>("Cookies")
                .Configure<BackChannelSessionStore>((o, store) =>
                {
                    o.SessionStore = store;
                });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.Use(async (context, next) =>
            {
                if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path.Equals("/signout-callback-oidc"))
                {
                    var form = await context.Request.ReadFormAsync();
                    var logoutToken = form["logout_token"].ToString();

                    if (string.IsNullOrWhiteSpace(logoutToken))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    var validator = context.RequestServices.GetRequiredService<BackChannelLogoutTokenValidator>();
                    var result = await validator.ValidateAsync(logoutToken);
                    if (!result.IsValid)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    var store = context.RequestServices.GetRequiredService<BackChannelSessionStore>();
                    if (!string.IsNullOrWhiteSpace(result.SessionId))
                    {
                        await store.RemoveBySessionIdAsync(result.SessionId);
                    }
                    else if (!string.IsNullOrWhiteSpace(result.SubjectId))
                    {
                        await store.RemoveBySubjectIdAsync(result.SubjectId);
                    }

                    context.Response.StatusCode = StatusCodes.Status200OK;
                    return;
                }

                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapDefaultControllerRoute()
                .RequireAuthorization();

            app.Run();
        }
    }
}
