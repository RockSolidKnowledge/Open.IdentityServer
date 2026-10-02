using Microsoft.AspNetCore.DataProtection;
using Moq;

namespace Open.IdentityServer.UnitTests.DataProtection;

public class AuthenticationTicketSerializerTests
{
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly MockDataProtector dataProtector = new();
    
}