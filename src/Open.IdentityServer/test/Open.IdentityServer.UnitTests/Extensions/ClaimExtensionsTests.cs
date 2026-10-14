// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using AwesomeAssertions;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Stores.Serialization;
using Xunit;

namespace Open.IdentityServer.UnitTests.Extensions;

public class ClaimExtensionsTests
{
    [Fact]
    public void ToClaimsDictionary_WhenDuplicates_ShouldHandleAndProduceCorrectDictionary()
    {
        var jsonObj = new { Data = "SOMEDATA", Veriosn = 1 };
        JsonElement jsonElement = JsonSerializer.SerializeToElement(jsonObj);
        
        IEnumerable<Claim> testClaims = [
            new("Integer", "1", ClaimValueTypes.Integer, "FakeIssuer1"),
            new("Integer", "1", ClaimValueTypes.Integer, "FakeIssuer1"),
            new("Integer32", "150", ClaimValueTypes.Integer32, "FakeIssuer2"),
            new("Integer32", "150", ClaimValueTypes.Integer32, "FakeIssuer2"),
            new("Integer64", "522337203485477580", ClaimValueTypes.Integer64, "FakeIssuer3"),
            new("Integer64", "522337203485477580", ClaimValueTypes.Integer64, "FakeIssuer3"),
            new("Boolean", "true", ClaimValueTypes.Boolean, "FakeIssuer4"),
            new("Boolean", "true", ClaimValueTypes.Boolean, "FakeIssuer4"),
            new("Json", JsonSerializer.Serialize(jsonObj), IdentityServerConstants.ClaimValueTypes.Json, "FakeIssuer4"),
            new("Json", JsonSerializer.Serialize(jsonObj), IdentityServerConstants.ClaimValueTypes.Json, "FakeIssuer4"),
        ];

        var actual = testClaims.ToClaimsDictionary();

        IDictionary<string, object> expected = new Dictionary<string, object>
        {
            ["Integer"] = (int) 1,
            ["Integer32"] = (int) 150,
            ["Integer64"] = (long) 522337203485477580,
            ["Boolean"] = (bool) true,
            ["Json"] = (JsonElement) jsonElement,
        };

        actual.Keys.Should().HaveCount(5);
        actual["Integer"].Should().BeEquivalentTo(expected["Integer"]);
        actual["Integer32"].Should().BeEquivalentTo(expected["Integer32"]);
        actual["Integer64"].Should().BeEquivalentTo(expected["Integer64"]);
        actual["Boolean"].Should().BeEquivalentTo(expected["Boolean"]);
        actual["Json"].ToString().Should().BeEquivalentTo(expected["Json"].ToString());
    }
    
    [Fact]
    public void ToClaimsDictionary_WhenAllClaimValueTypesProvided_ShouldAllBeConvertedToCorrectObjectInDictionary()
    {
        var jsonObj = new { Data = "SOMEDATA", Veriosn = 1 };
        JsonElement jsonElement = JsonSerializer.SerializeToElement(jsonObj);
        
        IEnumerable<Claim> testClaims = [
            new("Integer", "1", ClaimValueTypes.Integer, "FakeIssuer1"),
            new("Integer32", "150", ClaimValueTypes.Integer32, "FakeIssuer2"),
            new("Integer64", "522337203485477580", ClaimValueTypes.Integer64, "FakeIssuer3"),
            new("Boolean", "true", ClaimValueTypes.Boolean, "FakeIssuer4"),
            new("Json", JsonSerializer.Serialize(jsonObj), IdentityServerConstants.ClaimValueTypes.Json, "FakeIssuer4"),
        ];

        var actual = testClaims.ToClaimsDictionary();

        IDictionary<string, object> expected = new Dictionary<string, object>
        {
            ["Integer"] = (int) 1,
            ["Integer32"] = (int) 150,
            ["Integer64"] = (long) 522337203485477580,
            ["Boolean"] = (bool) true,
            ["Json"] = (JsonElement) jsonElement,
        };

        actual["Integer"].Should().BeEquivalentTo(expected["Integer"]);
        actual["Integer32"].Should().BeEquivalentTo(expected["Integer32"]);
        actual["Integer64"].Should().BeEquivalentTo(expected["Integer64"]);
        actual["Boolean"].Should().BeEquivalentTo(expected["Boolean"]);
        actual["Json"].ToString().Should().BeEquivalentTo(expected["Json"].ToString());
    }
    
    [Fact]
    public void ToSerializableObj_WhenEnumerableOfClaimProvided_ShouldReturnArrayOfClaimLite()
    {
        IEnumerable<Claim> testClaims =
        [
            new("Type1", "Value1", ClaimValueTypes.String, "FakeIssuer1"),
            new("Type2", "Value2", ClaimValueTypes.String, "FakeIssuer2"),
            new("Type3", "Value3", ClaimValueTypes.String, "FakeIssuer3"),
            new("Type4", "Value4", ClaimValueTypes.String, "FakeIssuer4"),
        ];

        var actual = testClaims.ToSerializableObj();
        
        ClaimLite[] expected =
        [
            new() { Type = "Type1", Value = "Value1", ValueType = ClaimValueTypes.String, Issuer = "FakeIssuer1" },
            new() { Type = "Type2", Value = "Value2", ValueType = ClaimValueTypes.String, Issuer = "FakeIssuer2" },
            new() { Type = "Type3", Value = "Value3", ValueType = ClaimValueTypes.String, Issuer = "FakeIssuer3" },
            new() { Type = "Type4", Value = "Value4", ValueType = ClaimValueTypes.String, Issuer = "FakeIssuer4" },
        ];

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ToClaims_WhenArrayOfClaimLiteProvided_ShouldReturnEnumerableOfClaim()
    {
        ClaimLite[] testClaims =
        [
            new() { Type = "Type1", Value = "Value1", ValueType = ClaimValueTypes.String, Issuer = "FakeIssuer1" },
            new() { Type = "Type2", Value = "Value2", ValueType = ClaimValueTypes.String, Issuer = "FakeIssuer2" },
            new() { Type = "Type3", Value = "Value3", ValueType = ClaimValueTypes.String, Issuer = "FakeIssuer3" },
            new() { Type = "Type4", Value = "Value4", ValueType = ClaimValueTypes.String, Issuer = "FakeIssuer4" },
        ];

        var actual = testClaims.ToClaims();
        
        IEnumerable<Claim> expected =
        [
            new("Type1", "Value1", ClaimValueTypes.String, "FakeIssuer1"),
            new("Type2", "Value2", ClaimValueTypes.String, "FakeIssuer2"),
            new("Type3", "Value3", ClaimValueTypes.String, "FakeIssuer3"),
            new("Type4", "Value4", ClaimValueTypes.String, "FakeIssuer4"),
        ];

        actual.Should().BeEquivalentTo(expected);
    }
}