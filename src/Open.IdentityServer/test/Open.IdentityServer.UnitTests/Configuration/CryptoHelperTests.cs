using AwesomeAssertions;
using Open.IdentityServer.Configuration;
using System;
using Xunit;

namespace Open.IdentityServer.UnitTests.Configuration;

public class CryptoHelperTests
{
    [Theory]
    [InlineData("RS256")]
    [InlineData("RS384")]
    [InlineData("RS512")]
    [InlineData("PS256")]
    [InlineData("PS384")]
    [InlineData("PS512")]
    public void IsRsaAlgorithm_ShouldReturnTrueForRsaAlgorithms(string algorithm)
    {
        algorithm.IsRsaAlgorithm().Should().BeTrue();
    }

    [Theory]
    [InlineData("ES256")]
    [InlineData("ES384")]
    [InlineData("ES512")]
    [InlineData("HS256")]
    [InlineData("AES256")]
    public void IsRsaAlgorithm_ShouldReturnFalseForNonRsaAlgorithms(string algorithm)
    {
        algorithm.IsRsaAlgorithm().Should().BeFalse();
    }

    [Theory]
    [InlineData("ES256")]
    [InlineData("ES384")]
    [InlineData("ES512")]
    public void IsEcAlgorithm_ShouldReturnTrueForEcAlgorithms(string algorithm)
    {
        algorithm.IsEcAlgorithm().Should().BeTrue();
    }

    [Theory]
    [InlineData("RS256")]
    [InlineData("RS384")]
    [InlineData("RS512")]
    [InlineData("PS256")]
    [InlineData("PS384")]
    [InlineData("PS512")]
    [InlineData("HS256")]
    [InlineData("AES256")]
    public void IsEcAlgorithm_ShouldReturnFalseForNonEcAlgorithms(string algorithm)
    {
        algorithm.IsEcAlgorithm().Should().BeFalse();
    }

    [Theory]
    [InlineData("ES256", "P-256")]
    [InlineData("ES384", "P-384")]
    [InlineData("ES512", "P-521")]
    public void GetCurveNameForAlgorithm_ShouldReturnCorrectCurveNameForEcAlgorithms(string algorithm, string expectedCurveName)
    {
        algorithm.GetCurveNameForAlgorithm().Should().Be(expectedCurveName);
    }

    [Theory]
    [InlineData("RS256")]
    [InlineData("RS384")]
    [InlineData("RS512")]
    [InlineData("PS256")]
    [InlineData("PS384")]
    [InlineData("PS512")]
    [InlineData("HS256")]
    [InlineData("AES256")]
    public void GetCurveNameForAlgorithm_ShouldThrowArgumentOutOfRangeExceptionForNonEcAlgorithms(string algorithm)
    {
        Action act = () => algorithm.GetCurveNameForAlgorithm();
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
