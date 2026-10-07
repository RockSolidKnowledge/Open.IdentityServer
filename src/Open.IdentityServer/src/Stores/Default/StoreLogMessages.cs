// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Stores.Default;

internal static partial class Log
{
    [LoggerMessage(
        LogLevel.Debug, 
         EventName = nameof(RetrieveAuthenticationTicket), 
         Message = "Retrieving authentication ticket for key {key}")]
    internal static partial void RetrieveAuthenticationTicket(this ILogger logger, string key);
    
    [LoggerMessage(
        LogLevel.Debug,
        EventName = nameof(NoTicketFoundInStore),
        Message = "No ticket found in store for key {key}")]
    internal static partial void NoTicketFoundInStore(this ILogger logger, string key);
    
    [LoggerMessage(
        LogLevel.Error,
        EventName = nameof(FailedToRetrieveTicketFromStore),
        Message = "Failed to retrieve ticket from store for key {key}")]
    internal static partial void FailedToRetrieveTicketFromStore(this ILogger logger, string key);
    
    [LoggerMessage(
        LogLevel.Debug, 
        EventName = nameof(TicketFoundInStore), 
        Message = "Ticket found in store for key {key} with expiration {expiration}")]
    internal static partial void TicketFoundInStore(this ILogger logger, string key, DateTimeOffset? expiration);

    [LoggerMessage(
        LogLevel.Debug,
        EventName = nameof(RemovingTicketFromStore), 
        Message = "Removing ticket from store for key {key}")]
    internal static partial void RemovingTicketFromStore(this ILogger logger, string key);
    
    [LoggerMessage(
        LogLevel.Debug,
        EventName = nameof(RenewingTicketInStore),
        Message = "Renewing ticket in store for key {key} with expiration {expiration}")]
    internal static partial void RenewingTicketInStore(this ILogger logger, string key, DateTimeOffset? expiration);
    
    [LoggerMessage(
        LogLevel.Debug,
        EventName = nameof(CreatingNewSessionFromTicket),
        Message = "Creating new session from ticket for key {key} with expiration {expiration}")]
    internal static partial void CreatingNewSessionFromTicket(this ILogger logger, string key, DateTimeOffset? expiration);
    
    [LoggerMessage(
        LogLevel.Debug,
        EventName = nameof(SessionOverwriteRevokingGrants),
        Message = "Session overwrite detected for key {key}, subject {subjectId} and session {sessionId}, revoking grants")]
    internal static partial void SessionOverwriteRevokingGrants(this ILogger logger, string key, string subjectId, string sessionId);
}