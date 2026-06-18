// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AwesomeAssertions;
using Open.IdentityServer;
using Open.IdentityServer.Services;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultTelemetryServiceTests : IDisposable
{
    private DefaultTelemetryService _subject;
    private MeterProvider _meterProvider;
    private ICollection<Metric> _exportedMetrics;

    public DefaultTelemetryServiceTests()
    {
        _exportedMetrics = new List<Metric>();

        _subject = new DefaultTelemetryService();
        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(TelemetryConstants.MetricsConstants.MeterName)
            .AddInMemoryExporter(_exportedMetrics)
            .Build();
    }

    public void Dispose()
    {
        _meterProvider?.Dispose();
    }

    [Fact]
    public void CountOperationError_WhenCalledWithNoTags_ShouldIncrementCounterWithSuccessTag()
    {
        const string error = "something goes bang";

        var expectedTags = new TagList()
        {
            { TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.InternalError },
            { TelemetryConstants.TagConstants.Error, error }
        };

        _subject.CountInternalError(error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single();

        metric.Name.Should().Be(TelemetryConstants.MetricsConstants.OperationCounterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Fact]
    public void BeginActiveRequest_WhenCalled_ShouldIncrementCounterWithExpectedTags()
    {
        const string endpoint = "expectedEndpoint";
        const string path = "expectedPath";

        var expectedTags = new TagList
        {
            { TelemetryConstants.TagConstants.Endpoint, endpoint },
            { TelemetryConstants.TagConstants.Path, path }
        };

        _subject.BeginActiveRequest(endpoint, path);
        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single();

        metric.Name.Should().Be(TelemetryConstants.MetricsConstants.ActiveRequestsCounterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Fact]
    public void BeginActiveRequest_WhenDisposed_ShouldDecrementCounterWithExpectedTags()
    {
        const string endpoint = "expectedEndpoint";
        const string path = "expectedPath";

        var expectedTags = new TagList
        {
            { TelemetryConstants.TagConstants.Endpoint, endpoint },
            { TelemetryConstants.TagConstants.Path, path }
        };

        var disposable = _subject.BeginActiveRequest(endpoint, path);
        disposable.Dispose();
        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single();

        metric.Name.Should().Be(TelemetryConstants.MetricsConstants.ActiveRequestsCounterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(0);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Fact]
    public void CountApiSecretValidation_WhenCalledWithClientAndAuthMethod_ShouldIncrementCounterWithExpectedTags()
    {
        const string client = "expectedClient";
        const string authMethod = "expectedAuthMethod";
        const string expectedCounterName = TelemetryConstants.MetricsConstants.ApiSecretValidationCounterName;

        var expectedTags = new TagList
        {
            { TelemetryConstants.TagConstants.Api, client },
            { TelemetryConstants.TagConstants.AuthMethod, authMethod }
        };

        _subject.CountApiSecretValidation(client, authMethod);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == expectedCounterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Fact]
    public void CountApiSecretValidation_WhenCalledWithClientAnError_ShouldIncrementCounterWithExpectedTags()
    {
        const string client = "expectedClient";
        const string error = "expectedError";
        const string expectedCounterName = TelemetryConstants.MetricsConstants.ApiSecretValidationCounterName;

        var expectedTags = new TagList
        {
            { TelemetryConstants.TagConstants.Api, client },
            { TelemetryConstants.TagConstants.Error, error }
        };

        _subject.CountApiSecretValidation(client, error: error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == expectedCounterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Fact]
    public void
        CountApiSecretValidation_WhenCalledWithClientAndAuthMethod_ShouldIncrementOperationCounterWithExpectedTags()
    {
        const string client = "expectedClient";
        const string authMethod = "expectedAuthMethod";
        const string expectedCounterName = TelemetryConstants.MetricsConstants.OperationCounterName;

        var expectedTags = new TagList
        {
            { TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Success },
            { TelemetryConstants.TagConstants.Client, client },
        };

        _subject.CountApiSecretValidation(client, authMethod);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == expectedCounterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Fact]
    public void CountApiSecretValidation_WhenCalledWithClientAnError_ShouldIncrementOperationCounterWithExpectedTags()
    {
        const string client = "expectedClient";
        const string error = "expectedError";
        const string expectedCounterName = TelemetryConstants.MetricsConstants.OperationCounterName;

        var expectedTags = new TagList
        {
            { TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Error },
            { TelemetryConstants.TagConstants.Client, client },
            { TelemetryConstants.TagConstants.Error, error }
        };

        _subject.CountApiSecretValidation(client, error: error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == expectedCounterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData(TelemetryConstants.MetricsConstants.BackChannelAuthenticationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.BackChannelAuthenticationCounterName, "clientid", "some error")]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", "some error")]
    public void CountBackchannelAuthentication_WhenCalled_ShouldIncrementCorrectCountersWithExpectedTags(
        string counterName,
        string client,
        string error)
    {
        var expectedTags = new TagList();
        if (counterName == TelemetryConstants.MetricsConstants.OperationCounterName)
        {
            expectedTags.Add(TelemetryConstants.TagConstants.Result,
                error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        }

        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountBackchannelAuthentication(client, error: error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData(TelemetryConstants.MetricsConstants.ClientConfigValidationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.ClientConfigValidationCounterName, "clientid", "some error")]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", "some error")]
    public void CountClientConfigValidation_WhenCalled_ShouldIncrementCorrectCountersWithExpectedTags(
        string counterName,
        string client,
        string error)
    {
        var expectedTags = new TagList();
        if (counterName == TelemetryConstants.MetricsConstants.OperationCounterName)
        {
            expectedTags.Add(TelemetryConstants.TagConstants.Result,
                error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        }

        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountClientConfigValidation(client, error: error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData("clientid", "authMethod", null)]
    [InlineData("clientid", null, "some error")]
    public void CountClientSecretValidation_WhenCalled_ShouldIncrementCountersWithExpectedTags(
        string client,
        string authMethod,
        string error)
    {
        const string counterName = TelemetryConstants.MetricsConstants.ClientSecretValidationCounterName;

        var expectedTags = new TagList();
        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (authMethod != null) expectedTags.Add(TelemetryConstants.TagConstants.AuthMethod, authMethod);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountClientSecretValidation(client, authMethod, error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData("clientid", "authMethod", null)]
    [InlineData("clientid", null, "some error")]
    public void CountClientSecretValidation_WhenCalled_ShouldIncrementOperationCountersWithExpectedTags(
        string client,
        string authMethod,
        string error)
    {
        const string counterName = TelemetryConstants.MetricsConstants.OperationCounterName;

        var expectedTags = new TagList();
        expectedTags.Add(TelemetryConstants.TagConstants.Result,
            error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountClientSecretValidation(client, authMethod, error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData(TelemetryConstants.MetricsConstants.ResourceOwnerAuthenticationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.ResourceOwnerAuthenticationCounterName, "clientid", "some error")]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", "some error")]
    public void CountResourceOwnerAuthentication_WhenCalled_ShouldIncrementCorrectCountersWithExpectedTags(
        string counterName,
        string client,
        string error)
    {
        var expectedTags = new TagList();
        if (counterName == TelemetryConstants.MetricsConstants.OperationCounterName)
        {
            expectedTags.Add(TelemetryConstants.TagConstants.Result,
                error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        }

        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountResourceOwnerAuthentication(client, error: error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData(TelemetryConstants.MetricsConstants.DeviceAuthenticationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.DeviceAuthenticationCounterName, "clientid", "some error")]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", "some error")]
    public void CountDeviceAuthentication_WhenCalled_ShouldIncrementCorrectCountersWithExpectedTags(
        string counterName,
        string client,
        string error)
    {
        var expectedTags = new TagList();
        if (counterName == TelemetryConstants.MetricsConstants.OperationCounterName)
        {
            expectedTags.Add(TelemetryConstants.TagConstants.Result,
                error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        }

        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountDeviceAuthentication(client, error: error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData("clientid", true, null)]
    [InlineData("clientid", null, "some error")]
    public void CountTokenIntrospection_WhenCalled_ShouldIncrementCountersWithExpectedTags(
        string client,
        bool? active,
        string error)
    {
        const string counterName = TelemetryConstants.MetricsConstants.IntrospectionCounterName;

        var expectedTags = new TagList();
        expectedTags.Add(TelemetryConstants.TagConstants.Caller, client);
        if (active != null) expectedTags.Add(TelemetryConstants.TagConstants.Active, active.Value);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountTokenIntrospection(client, active, error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData("clientid", false, null)]
    [InlineData("clientid", null, "some error")]
    public void CountTokenIntrospection_WhenCalled_ShouldIncrementOperationCountersWithExpectedTags(
        string client,
        bool? active,
        string error)
    {
        const string counterName = TelemetryConstants.MetricsConstants.OperationCounterName;

        var expectedTags = new TagList();
        expectedTags.Add(TelemetryConstants.TagConstants.Result,
            error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountTokenIntrospection(client, active, error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData(TelemetryConstants.MetricsConstants.PushedAuthorizationRequestCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.PushedAuthorizationRequestCounterName, "clientid", "some error")]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", "some error")]
    public void CountPushedAuthentication_WhenCalled_ShouldIncrementCorrectCountersWithExpectedTags(
        string counterName,
        string client,
        string error)
    {
        var expectedTags = new TagList();
        if (counterName == TelemetryConstants.MetricsConstants.OperationCounterName)
        {
            expectedTags.Add(TelemetryConstants.TagConstants.Result,
                error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        }

        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountPushedAuthorizationRequest(client, error: error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData(TelemetryConstants.MetricsConstants.RevocationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.RevocationCounterName, "clientid", "some error")]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", null)]
    [InlineData(TelemetryConstants.MetricsConstants.OperationCounterName, "clientid", "some error")]
    public void CountTokenRevocation_WhenCalled_ShouldIncrementCorrectCountersWithExpectedTags(
        string counterName,
        string client,
        string error)
    {
        var expectedTags = new TagList();
        if (counterName == TelemetryConstants.MetricsConstants.OperationCounterName)
        {
            expectedTags.Add(TelemetryConstants.TagConstants.Result,
                error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        }

        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountTokenRevocation(client, error: error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData("clientid", "grant", null)]
    [InlineData("clientid", "grant", "some error")]
    public void CountTokenIssued_WhenCalled_ShouldIncrementCountersWithExpectedTags(
        string client,
        string grantType,
        string error)
    {
        const string counterName = TelemetryConstants.MetricsConstants.TokenIssuedCounterName;

        var expectedTags = new TagList();
        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        expectedTags.Add(TelemetryConstants.TagConstants.GrantType, grantType);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountTokenIssued(client, grantType, error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData("clientid", "grant", null)]
    [InlineData("clientid", "grant", "some error")]
    public void CountTokenIssued_WhenCalled_ShouldIncrementOperationCountersWithExpectedTags(
        string client,
        string grantType,
        string error)
    {
        const string counterName = TelemetryConstants.MetricsConstants.OperationCounterName;

        var expectedTags = new TagList();
        expectedTags.Add(TelemetryConstants.TagConstants.Result,
            error == null ? TelemetryConstants.TagConstants.Success : TelemetryConstants.TagConstants.Error);
        expectedTags.Add(TelemetryConstants.TagConstants.Client, client);
        if (error != null) expectedTags.Add(TelemetryConstants.TagConstants.Error, error);


        _subject.CountTokenIssued(client, grantType, error);

        _meterProvider.ForceFlush();

        Metric metric = _exportedMetrics.Single(m => m.Name == counterName);

        var accessor = metric.GetMetricPoints();
        foreach (MetricPoint point in accessor)
        {
            point.GetSumLong().Should().Be(1);

            point.Tags.ShouldBeEquivalentTo(expectedTags);
        }
    }

    [Theory]
    [InlineData(TelemetryConstants.TraceCategories.Basic)]
    [InlineData(TelemetryConstants.TraceCategories.Cache)]
    [InlineData(TelemetryConstants.TraceCategories.Services)]
    [InlineData(TelemetryConstants.TraceCategories.Stores)]
    [InlineData(TelemetryConstants.TraceCategories.Validation)]
    public void Trace_WithKnownCategory_ShouldStartAndStopActivity(string traceCategoryName)
    {
        var started = new List<Activity>();
        var stopped = new List<Activity>();

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == traceCategoryName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => started.Add(activity),
            ActivityStopped = activity => stopped.Add(activity)
        };

        ActivitySource.AddActivityListener(listener);

        using (var trace = _subject.Trace(traceCategoryName, "test-activity"))
        {
            started.Should().HaveCount(1);
            started[0].DisplayName.Should().Be("test-activity");
            started[0].Source.Name.Should().Be(traceCategoryName);
            
            stopped.Should().BeEmpty();
        }

        stopped.Should().HaveCount(1);
    }

    [Fact]
    public void Trace_WithUnknownCategory_ShouldReturnNull()
    {
        var started = new List<Activity>();

        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => started.Add(activity),
        };

        ActivitySource.AddActivityListener(listener);

        using var trace = _subject.Trace("unknown-category", "test-activity");

        trace.Should().BeNull();
        started.Should().BeEmpty();
    }
}

public static class OpenTelemetryTestExtensions
{
    extension(OpenTelemetry.ReadOnlyTagCollection tags)
    {
        public void ShouldBeEquivalentTo(TagList expected)
        {
            tags.Count.Should().Be(expected.Count, "The collections should have the same number of elements");

            foreach (KeyValuePair<string, object> tag in tags)
            {
                expected.Should().Contain(tag, $"The tag '{tag.Key}' with value '{tag.Value}' should be present in the expected collection");
            }
        }
    }
}