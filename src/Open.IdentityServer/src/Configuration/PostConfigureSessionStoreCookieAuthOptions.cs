// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// IPostConfigureOptions implementation for <see cref="CookieAuthenticationOptions"/>. Registers the <see cref="ITicketStore"/>
/// implementation to use for storing auth tickets.
/// </summary>
/// <param name="services">The service provider</param>
/// <param name="idsOptions">Open.IdentityServer options</param>
/// <param name="authOptions">Authentication options</param>
public class PostConfigureSessionStoreCookieAuthOptions(
    IServiceProvider services,
    IdentityServerOptions idsOptions,
    IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> authOptions): IPostConfigureOptions<CookieAuthenticationOptions>
{
    /// <summary>
    /// Implementation of post configure setting <see cref="CookieAuthenticationOptions"/> SessionStore parameter if
    /// name provided matches scheme
    /// </summary>
    /// <param name="name">name of the scheme</param>
    /// <param name="options">cookie authentication options</param>
    public void PostConfigure(string name, CookieAuthenticationOptions options)
    {
        var scheme = idsOptions.Authentication.CookieAuthenticationScheme ??
            authOptions.Value.DefaultAuthenticateScheme ??
            authOptions.Value.DefaultScheme;

        if (scheme == name)
        {
            options.SessionStore = new ScopedTicketStoreAdapter(services);
        }
    }
}

/// <summary>
/// Allows access to scoped dependencies from a singleton service. Wraps the <see cref="IServerSessionTicketStore"/> in a scoped service provider.
/// </summary>
internal class ScopedTicketStoreAdapter : ITicketStore
{
    private readonly IServiceProvider _services;

    public ScopedTicketStoreAdapter(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket) => await WithScopeAsync(store => store.StoreAsync(ticket));

    public async Task RenewAsync(string key, AuthenticationTicket ticket) => await WithScopeAsync(store => store.RenewAsync(key, ticket));

    public async Task<AuthenticationTicket> RetrieveAsync(string key) => await WithScopeAsync(store => store.RetrieveAsync(key));   

    public async Task RemoveAsync(string key) => await WithScopeAsync(store => store.RemoveAsync(key));
    
    private async Task WithScopeAsync(Func<ITicketStore, Task> action) => await WithScopeAsync(async t => { await action(t); return true; });
    
    private async Task<T> WithScopeAsync<T>(Func<ITicketStore, Task<T>> action)
    {
        using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IServerSessionTicketStore>();
        return await action(service);
    }
}