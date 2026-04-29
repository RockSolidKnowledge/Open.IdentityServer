using System;
using Microsoft.EntityFrameworkCore;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.EntityFramework.Interfaces;

public interface IIdentityServerKeyDbContext: IDisposable
{
    DbSet<IdentityServerKeyMaterial> Keys { get; set; }
}