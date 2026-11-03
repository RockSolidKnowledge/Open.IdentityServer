using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcClient
{
    public class BackChannelSessionStore : ITicketStore
    {
        private readonly ConcurrentDictionary<string, AuthenticationTicket> _tickets = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _sidIndex = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _subjectIndex = new();

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = Guid.NewGuid().ToString("N");
            _tickets[key] = ticket;
            IndexTicket(key, ticket);
            return Task.FromResult(key);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            if (_tickets.TryGetValue(key, out var existing))
            {
                RemoveFromIndex(key, existing);
            }

            _tickets[key] = ticket;
            IndexTicket(key, ticket);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            _tickets.TryGetValue(key, out var ticket);
            return Task.FromResult(ticket);
        }

        public Task RemoveAsync(string key)
        {
            if (_tickets.TryRemove(key, out var ticket))
            {
                RemoveFromIndex(key, ticket);
            }

            return Task.CompletedTask;
        }

        public async Task RemoveBySessionIdAsync(string sid)
        {
            await RemoveByIndexAsync(_sidIndex, sid);
        }

        public async Task RemoveBySubjectIdAsync(string subjectId)
        {
            await RemoveByIndexAsync(_subjectIndex, subjectId);
        }

        private async Task RemoveByIndexAsync(ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> index, string subjectOrSessionId)
        {
            if (!index.TryGetValue(subjectOrSessionId, out var keys))
            {
                return;
            }

            var keySnapshot = keys.Keys.ToArray();
            foreach (var key in keySnapshot)
            {
                await RemoveAsync(key);
            }
        }

        private void IndexTicket(string key, AuthenticationTicket ticket)
        {
            var sid = ticket.Principal?.FindFirst("sid")?.Value;
            if (!string.IsNullOrWhiteSpace(sid))
            {
                _sidIndex.GetOrAdd(sid, _ => new ConcurrentDictionary<string, byte>())[key] = 0;
            }

            var sub = ticket.Principal?.FindFirst("sub")?.Value;
            if (!string.IsNullOrWhiteSpace(sub))
            {
                _subjectIndex.GetOrAdd(sub, _ => new ConcurrentDictionary<string, byte>())[key] = 0;
            }
        }

        private void RemoveFromIndex(string key, AuthenticationTicket ticket)
        {
            var sid = ticket.Principal?.FindFirst("sid")?.Value;
            if (!string.IsNullOrWhiteSpace(sid))
            {
                RemoveIndexEntry(_sidIndex, sid, key);
            }

            var sub = ticket.Principal?.FindFirst("sub")?.Value;
            if (!string.IsNullOrWhiteSpace(sub))
            {
                RemoveIndexEntry(_subjectIndex, sub, key);
            }
        }

        private static void RemoveIndexEntry(ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> index, string indexKey, string ticketKey)
        {
            if (!index.TryGetValue(indexKey, out var keySet))
            {
                return;
            }

            keySet.TryRemove(ticketKey, out _);
            if (keySet.IsEmpty)
            {
                index.TryRemove(indexKey, out _);
            }
        }
    }
}
