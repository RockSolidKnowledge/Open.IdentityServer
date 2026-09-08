using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// Service to clean up expired server-side sessions
/// </summary>
public interface ISessionCleanupService
{
    /// <summary>
    /// Method to clear expired server-side sessions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all expired grants and device codes have been removed.</returns>
    public Task RemoveExpiredServerSideSessionsAsync();
}