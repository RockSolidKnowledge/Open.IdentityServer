using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Stores;
using Xunit;

namespace IdentityServer.UnitTests.Stores.Default;

public class ServerSessionTicketStoreTests
{
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly ILogger<ServerSessionTicketStore> logger;
    
    private ServerSessionTicketStore CreateSut() => new(dataProtectionProvider, logger);

    [Fact]
    public void _When_Should()
    {
        
    }
}