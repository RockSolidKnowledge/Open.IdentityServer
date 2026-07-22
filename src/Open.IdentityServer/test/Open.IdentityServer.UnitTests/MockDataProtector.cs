using System.Text;
using AwesomeAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Moq;

namespace Open.IdentityServer.UnitTests;

public class MockDataProtector: IDataProtector
{
    public IDataProtector dataProtector = Mock.Of<IDataProtector>();
    public static readonly UTF8Encoding UTF8Encoding = new(false, true);
    public const string ProtectedPrefix = "PROTECTED--";

    public MockDataProtector()
    {
        Mock.Get(dataProtector)
            .Setup(x => x.Protect(It.IsAny<byte[]>()))
            .Returns<byte[]>((plaintext) => [..UTF8Encoding.GetBytes(ProtectedPrefix), ..plaintext]);

        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.IsAny<byte[]>()))
            .Returns<byte[]>((protectedData) => UTF8Encoding.GetBytes(UTF8Encoding.GetString(protectedData).Replace(ProtectedPrefix, string.Empty)));
    }

    public IDataProtector CreateProtector(string purpose) => dataProtector;

    public byte[] Protect(byte[] plaintext) => dataProtector.Protect(plaintext);

    public byte[] Unprotect(byte[] protectedData) => dataProtector.Unprotect(protectedData);

    public void ValidateProtectedData(string protectedData, string originalString)
    {
        var unencodedProtectedData = UTF8Encoding.GetString(WebEncoders.Base64UrlDecode(protectedData));
        unencodedProtectedData = unencodedProtectedData.Replace(ProtectedPrefix, string.Empty);
        unencodedProtectedData.Should().BeEquivalentTo(originalString);
    }

    public string GenerateFakeProtectedData(string data) => 
        WebEncoders.Base64UrlEncode(UTF8Encoding.GetBytes(ProtectedPrefix + data));
}