using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Services;

/// <summary>
/// Wrapper service for IDeviceFlowStore.
/// </summary>
public interface IDeviceFlowCodeService
{
    /// <summary>
    /// Stores the device authorization request.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <param name="data">The data.</param>
    /// <returns>A task that resolves to the device code handle assigned to the stored authorization.</returns>
    Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data);

    /// <summary>
    /// Finds device authorization by user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <returns>A task that resolves to the <see cref="DeviceCode"/> associated with <paramref name="userCode"/>, or <see langword="null"/> if not found.</returns>
    Task<DeviceCode> FindByUserCodeAsync(string userCode);

    /// <summary>
    /// Finds device authorization by device code.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <returns>A task that resolves to the <see cref="DeviceCode"/> associated with <paramref name="deviceCode"/>, or <see langword="null"/> if not found.</returns>
    Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode);

    /// <summary>
    /// Updates device authorization, searching by user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <param name="data">The data.</param>
    Task UpdateByUserCodeAsync(string userCode, DeviceCode data);

    /// <summary>
    /// Removes the device authorization, searching by device code.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    Task RemoveByDeviceCodeAsync(string deviceCode);
}