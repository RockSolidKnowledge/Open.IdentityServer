// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Models;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Mvc;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Test;

namespace IdentityServer.Controllers;

public class Sessions(TestUserStore userManager , 
    ISessionManagementService serverSessions) : Controller
{
    [HttpGet("/sessions")]
    public async Task<IActionResult> List()
    {
        QueryResult<UserSession> allSessions = await serverSessions.QuerySessionsAsync(null);
        var model = BuildViewModel(allSessions.Results, string.Empty);
        return View("~/Views/UserSessions/List.cshtml", model);
    }
    
    [HttpGet("/sessions/{username}")]
    public async Task<IActionResult> List(string username)
    {
        var user = userManager.FindByUsername(username);

        if (user == null)
        {
            return NotFound();
        }
        
        QueryResult<UserSession> userSessions = await serverSessions.QuerySessionsAsync(
            new SessionQuery()
            {
                SubjectId = user.SubjectId
            });
        
        var model = BuildViewModel(userSessions.Results, username);
        return View("~/Views/UserSessions/List.cshtml", model);
    }

    [Route("/sessions/{username}")]
    [HttpDelete]
    public async Task<IActionResult> EndSession(string username)
    {
        var user = userManager
            .FindByUsername(username);

        if (user == null)
        {
            return NotFound();
        }
        
        RemoveSessionsContext ctx = new RemoveSessionsContext()
        {
           SubjectId = user.SubjectId,
           SendBackchannelLogoutNotification = true,
           RevokeConsents = false,
           RevokeTokens = true
        };

        await serverSessions.RemoveSessionsAsync(ctx);

        return Ok();
    }

    private static SessionsListViewModel BuildViewModel(IEnumerable<UserSession> sessions, string selectedUsername)
    {
        var usernames = TestUsers.Users
            .Select(user => user.Username)
            .OrderBy(username => username)
            .ToList();

        return new SessionsListViewModel
        {
            Sessions = sessions.ToList(),
            Usernames = usernames,
            SelectedUsername = selectedUsername
        };
    }
}