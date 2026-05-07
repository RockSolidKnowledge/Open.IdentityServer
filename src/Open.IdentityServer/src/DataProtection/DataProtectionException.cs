#nullable enable
using System;

namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Exception thrown when an error occurs with data protection
/// </summary>
/// <param name="ex">The inner exception that caused the data protection failure.</param>
/// <param name="msg">A message describing the data protection error.</param>
public class DataProtectionException(Exception ex, string msg): Exception(msg, ex);