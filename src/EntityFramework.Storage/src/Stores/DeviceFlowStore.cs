// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.EntityFramework.Stores
{
    /// <summary>
    /// Implementation of IDeviceFlowStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IDeviceFlowStore" />
    public class DeviceFlowStore : IDeviceFlowStore
    {
        private readonly IPersistedGrantDbContext _context;
        private readonly IPersistentGrantSerializer _serializer;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceFlowStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="serializer">The serializer</param>
        /// <param name="logger">The logger.</param>
        public DeviceFlowStore(
            IPersistedGrantDbContext context, 
            IPersistentGrantSerializer serializer, 
            ILogger<DeviceFlowStore> logger)
        {
            _context = context;
            _serializer = serializer;
            _logger = logger;
        }

        /// <summary>
        /// Stores the device authorization request.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceCode data)
        {
            _context.DeviceFlowCodes.Add(ToEntity(data, deviceCode, userCode));

            _context.SaveChanges();

            return Task.FromResult(0);
        }

        /// <summary>
        /// Finds device authorization by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <returns></returns>
        public Task<DeviceCode> FindByUserCodeAsync(string userCode)
        {
            var deviceFlowCodes = _context.DeviceFlowCodes.AsNoTracking().FirstOrDefault(x => x.UserCode == userCode);
            var model = ToModel(deviceFlowCodes?.Data);

            _logger.LogDebug("{userCode} found in database: {userCodeFound}", userCode, model != null);

            return Task.FromResult(model);
        }

        /// <summary>
        /// Finds device authorization by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        public Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
        {
            var deviceFlowCodes = _context.DeviceFlowCodes.AsNoTracking().FirstOrDefault(x => x.DeviceCode == deviceCode);
            var model = ToModel(deviceFlowCodes?.Data);

            _logger.LogDebug("{deviceCode} found in database: {deviceCodeFound}", deviceCode, model != null);

            return Task.FromResult(model);
        }

        /// <summary>
        /// Updates device authorization, searching by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
        {
            var existing = _context.DeviceFlowCodes.SingleOrDefault(x => x.UserCode == userCode);
            if (existing == null)
            {
                _logger.LogError("{userCode} not found in database", userCode);
                throw new InvalidOperationException("Could not update device code");
            }

            var entity = ToEntity(data, existing.DeviceCode, userCode);
            _logger.LogDebug("{userCode} found in database", userCode);

            existing.SubjectId = data.Subject?.FindFirst(JwtClaimTypes.Subject).Value;
            existing.Data = entity.Data;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning("exception updating {userCode} user code in database: {error}", userCode, ex.Message);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Removes the device authorization, searching by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        public Task RemoveByDeviceCodeAsync(string deviceCode)
        {
            var deviceFlowCodes = _context.DeviceFlowCodes.FirstOrDefault(x => x.DeviceCode == deviceCode);

            if(deviceFlowCodes != null)
            {
                _logger.LogDebug("removing {deviceCode} device code from database", deviceCode);

                _context.DeviceFlowCodes.Remove(deviceFlowCodes);

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogInformation("exception removing {deviceCode} device code from database: {error}", deviceCode, ex.Message);
                }
            }
            else
            {
                _logger.LogDebug("no {deviceCode} device code found in database", deviceCode);
            }

            return Task.FromResult(0);
        }

        private DeviceFlowCodes ToEntity(DeviceCode model, string deviceCode, string userCode)
        {
            if (model == null || deviceCode == null || userCode == null) return null;

            return new DeviceFlowCodes
            {
                DeviceCode = deviceCode,
                UserCode = userCode,
                ClientId = model.ClientId,
                SubjectId = model.Subject?.FindFirst(JwtClaimTypes.Subject).Value,
                CreationTime = model.CreationTime,
                Expiration = model.CreationTime.AddSeconds(model.Lifetime),
                Data = _serializer.Serialize(model)
            };
        }

        private DeviceCode ToModel(string entity)
        {
            if (entity == null) return null;

            return _serializer.Deserialize<DeviceCode>(entity);
        }
    }
}