// // Copyright (c) 2026, Rock Solid Knowledge Ltd
// // Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using AwesomeAssertions;
// using Open.IdentityServer;
// using Open.IdentityServer.Services;
// using OpenTelemetry;
// using OpenTelemetry.Metrics;
// using Xunit;
//
// namespace Open.IdentityServer.UnitTests.Services.Default;
//
// public class DefaultTelemetryServiceTests : IDisposable
// {
//     private DefaultTelemetryService _subject;
//     private MeterProvider _meterProvider;
//     private ICollection<Metric> _exportedMetrics;
//
//     public DefaultTelemetryServiceTests()
//     {
//         _exportedMetrics = new List<Metric>();
//         
//         _subject = new DefaultTelemetryService();
//         _meterProvider = Sdk.CreateMeterProviderBuilder()
//             .AddMeter(TelemetryConstants.MetricsConstants.MeterName)
//             .AddInMemoryExporter(_exportedMetrics)
//             .Build();
//     }
//     
//     public void Dispose()
//     {
//         _meterProvider?.Dispose();
//     }
//     
//     private void VerifyTags(ReadOnlyTagCollection pointTags, List<TelemetryTag> expectedTags)
//     {
//         var actualTags = new List<TelemetryTag>();
//
//         foreach (var tag in pointTags)
//         {
//             actualTags.Add(new TelemetryTag(tag.Key, tag.Value));
//         }
//         
//         actualTags.Should().BeEquivalentTo(expectedTags);
//     }
//
//     [Fact]
//     public void CountOperationSucceeded_WhenCalledWithNoTags_ShouldIncrementCounterWithSuccessTag()
//     {
//         var expectedTags = new List<TelemetryTag>
//         {
//             new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Success)
//         };
//         
//         _subject.CountOperationSucceeded();
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(TelemetryConstants.MetricsConstants.OperationCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountOperationFailed_WhenCalledWithNoTags_ShouldIncrementCounterWithSuccessTag()
//     {
//         var expectedTags = new List<TelemetryTag>
//         {
//             new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Error)
//         };
//         
//         _subject.CountOperationFailed();
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(TelemetryConstants.MetricsConstants.OperationCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//     
//     [Fact]
//     public void CountOperationError_WhenCalledWithNoTags_ShouldIncrementCounterWithSuccessTag()
//     {
//         var expectedTags = new List<TelemetryTag>
//         {
//             new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.InternalError)
//         };
//         
//         _subject.CountInternalError();
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(TelemetryConstants.MetricsConstants.OperationCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountOperationSucceeded_WhenCalledWithAnyTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         
//         var expectedTags = new List<TelemetryTag>
//         {
//             new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Success),
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountOperationSucceeded(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(TelemetryConstants.MetricsConstants.OperationCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountOperationFailed_WhenCalledWithAnyTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         
//         var expectedTags = new List<TelemetryTag>
//         {
//             new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Error),
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountOperationFailed(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(TelemetryConstants.MetricsConstants.OperationCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountOperationError_WhenCalledWithAnyTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         var expectedTags = new List<TelemetryTag>
//         {
//             new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.InternalError),
//             arbitraryTag1,
//             arbitraryTag2
//         };
//
//         _subject.CountInternalError(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();
//
//         Metric metric = _exportedMetrics.Single();
//
//         metric.Name.Should().Be(TelemetryConstants.MetricsConstants.OperationCounterName);
//
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void BeginActiveRequest_WhenCalled_ShouldIncrementCounterWithExpectedTags()
//     {
//         const string endpoint = "expectedEndpoint";
//         const string path = "expectedPath";
//         
//         var expectedTags = new List<TelemetryTag>
//         {
//             new TelemetryTag(TelemetryConstants.TagConstants.Endpoint, endpoint),
//             new TelemetryTag(TelemetryConstants.TagConstants.Path, path)
//         };
//         
//         _subject.BeginActiveRequest(endpoint, path);
//         _meterProvider.ForceFlush();
//         
//         Metric metric = _exportedMetrics.Single();
//
//         metric.Name.Should().Be(TelemetryConstants.MetricsConstants.ActiveRequestsCounterName);
//
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void BeginActiveRequest_WhenDisposed_ShouldDecrementCounterWithExpectedTags()
//     {
//         const string endpoint = "expectedEndpoint";
//         const string path = "expectedPath";
//         
//         var expectedTags = new List<TelemetryTag>
//         {
//             new TelemetryTag(TelemetryConstants.TagConstants.Endpoint, endpoint),
//             new TelemetryTag(TelemetryConstants.TagConstants.Path, path)
//         };
//         
//         var disposable = _subject.BeginActiveRequest(endpoint, path);
//         disposable.Dispose();
//         _meterProvider.ForceFlush();
//         
//         Metric metric = _exportedMetrics.Single();
//
//         metric.Name.Should().Be(TelemetryConstants.MetricsConstants.ActiveRequestsCounterName);
//
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(0);
//
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountApiSecretValidation_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.ApiSecretValidationCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountApiSecretValidation(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountBackchannelAuthentication_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.BackChannelAuthenticationCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountBackchannelAuthentication(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountClientConfigValidation_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.ClientConfigValidationCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountClientConfigValidation(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountClientSecretValidation_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.ClientSecretValidationCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountClientSecretValidation(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountDeviceAuthentication_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.DeviceAuthenticationCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountDeviceAuthentication(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountTokenIntrospection_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.IntrospectionCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountTokenIntrospection(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountPushedAuthorizationRequest_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.PushedAuthorizationRequestCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountPushedAuthorizationRequest(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountResourceOwnerAuthentication_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.ResourceOwnerAuthenticationCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountResourceOwnerAuthentication(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountTokenRevocation_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.RevocationCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountTokenRevocation(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
//
//     [Fact]
//     public void CountTokenIssued_WhenCalledWithArbitraryTags_ShouldIncrementCounterWithExpectedTags()
//     {
//         var arbitraryTag1 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//         var arbitraryTag2 = new TelemetryTag(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
//
//         const string expectedCounterName = TelemetryConstants.MetricsConstants.TokenIssuedCounterName;
//         var expectedTags = new List<TelemetryTag>
//         {
//             arbitraryTag1,
//             arbitraryTag2
//         };
//         
//         _subject.CountTokenIssued(arbitraryTag1, arbitraryTag2);
//
//         _meterProvider.ForceFlush();        
//         
//         Metric metric = _exportedMetrics.Single();
//         
//         metric.Name.Should().Be(expectedCounterName);
//         
//         var accessor = metric.GetMetricPoints();
//         foreach (MetricPoint point in accessor)
//         {
//             point.GetSumLong().Should().Be(1);
//             
//             VerifyTags(point.Tags, expectedTags);
//         }
//     }
// }