// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultSessionManagementServiceTests
{
    private IPersistedGrantService persistedGrantService = Mock.Of<IPersistedGrantService>();
    private IBackChannelLogoutService backChannelLogoutService = Mock.Of<IBackChannelLogoutService>();
    private IServerSessionTicketStore serverSessionTicketStore = Mock.Of<IServerSessionTicketStore>();
    private ILogger<DefaultSessionManagementService> logger = Mock.Of<ILogger<DefaultSessionManagementService>>();
    
    private DefaultSessionManagementService CreateSut() => new(persistedGrantService, backChannelLogoutService, serverSessionTicketStore, logger);

    /// TODO: implement query tests, types of query to test
    /// 1. When no filter is provided, should use default values
    /// 2. When no token is provided, it should get the first page of results
    /// 3. When a token is provided, it should get the next page relative to the provided token
    /// 4. When a subjectId filter is provided, it should filter the results using it
    /// 5. When a sessionId filter is provided, it should filter results using it
    /// 6. When a display name filter provided, it should filter results using it
    /// 7. 
    /// 
    
    [Fact]
    public async Task QuerySessionsAsync_WhenFilterProvided_ShouldUseDefaultValues()
    {
        
    }
    
    [Fact]
    public async Task QuerySessionsAsync_WhenNoTokenProvided_ShouldProvideFirstPageOfResults()
    {
        
    }
    
    [Fact]
    public async Task QuerySessionsAsync_WhenTokenProvided_ShouldProvideNextPageOfResults()
    {
        
    }
    
    [Fact]
    public async Task QuerySessionsAsync_WhenSubjectIdProvided_ShouldFilterResultsUsingIt()
    {
        
    }
    
    [Fact]
    public async Task QuerySessionsAsync_WhenSessionIdProvided_ShouldFilterResultsUsingIt()
    {
        
    }
    
    [Fact]
    public async Task QuerySessionsAsync_WhenDisplayNameProvided_ShouldFilterResultsUsingIt()
    {
        
    }

    /// TODO: implement removal tests, types of query to tests
    /// 1. Remove called with sessionId specified, should remove sessions with specified sessionId
    /// 2. Remove called with subjectId specified, should remove sessions with specified subjectId
    /// 3. Remove called with clientsIds specified, should only trigger back channel notification and revocations for those clients
    /// 4. Remove called with remove sessions set to false, shouldn't remove sessions
    /// 5. Remove called with send backchannel set to false, shouldn't send backchannel
    /// 6. Remove called with revoke tokens set to false, shouldn't  revoke tokens
    /// 7. Remove called with revoke consents set to false, shouldn't  revoke consents
    ///
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenSessionIdSpecified_ShouldRemoveAllSessionsWithThatSessionId()
    {
        
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenSubjectIdSpecified_ShouldRemoveAllSessionsWithThatSubjectId()
    {
        
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenClientIdsProvided_ShouldOnlyTriggerBackchannelNotificationsAndRevocationsForThoseClients()
    {
        
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRemoveSessionsSetToFalse_ShouldNotRemoveSessions()
    {
        
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenSendBackchannelFalse_ShouldNotSendBackchannelNotification()
    {
        
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRevokeTokensFalse_ShouldNotRevokeTokens()
    {
        
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRevokeConsentsFalse_ShouldNotRevokeConsents()
    {
        
    }
}