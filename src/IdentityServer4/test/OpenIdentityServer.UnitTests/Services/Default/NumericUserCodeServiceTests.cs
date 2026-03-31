using System.Threading.Tasks;
using AwesomeAssertions;
using OpenIdentityServer.Services;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default;

public class NumericUserCodeGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_should_return_expected_code()
    {
        var sut = new NumericUserCodeGenerator();

        var userCode = await sut.GenerateAsync();
        var userCodeInt = int.Parse(userCode);

        userCodeInt.Should().BeGreaterThanOrEqualTo(100000000);
        userCodeInt.Should().BeLessThanOrEqualTo(999999999);
    }
}