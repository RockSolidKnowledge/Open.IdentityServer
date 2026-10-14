using System;
using System.Collections.Specialized;

namespace Open.IdentityServer.Storage.Models;

/// <summary>
/// Used to serialize the data necessary to store a pushed authorization request.
/// </summary>
/// 
public record PushedAuthorizationMemento(string Key , DateTimeOffset ValidUntil , NameValueCollection Parameters ) { }

