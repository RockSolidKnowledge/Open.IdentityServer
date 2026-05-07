// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Interface for the device flow store
/// </summary>
public interface IDeviceFlowStore
{
    /// <summary>
    /// Stores the device authorization request.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <param name="userCode">The user code.</param>
    /// <param name="data">The data.</param>
    /// <returns>A <see cref="Task"/> that completes once the device authorization has been persisted.</returns>
    Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceCode data);

    /// <summary>
    /// Finds device authorization by user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <returns>The <see cref="DeviceCode"/> associated with <paramref name="userCode"/>, or <see langword="null"/> when not found.</returns>
    Task<DeviceCode> FindByUserCodeAsync(string userCode);

    /// <summary>
    /// Finds device authorization by device code.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <returns>The <see cref="DeviceCode"/> associated with <paramref name="deviceCode"/>, or <see langword="null"/> when not found.</returns>
    Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode);

    /// <summary>
    /// Updates device authorization, searching by user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <param name="data">The data.</param>
    /// <returns>A <see cref="Task"/> that completes once the device authorization has been updated.</returns>
    Task UpdateByUserCodeAsync(string userCode, DeviceCode data);

    /// <summary>
    /// Removes the device authorization, searching by device code.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <returns>A <see cref="Task"/> that completes once the device authorization has been removed.</returns>
    Task RemoveByDeviceCodeAsync(string deviceCode);
}