using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

namespace Host.Extensions
{
    public class HostProfileService : DefaultProfileService
    {
        public HostProfileService(ILogger<DefaultProfileService> logger) : base(logger)
        {
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            return base.GetProfileDataAsync(context);
        }

        public override Task IsActiveAsync(IsActiveContext context)
        {
            return base.IsActiveAsync(context);
        }
    }
}