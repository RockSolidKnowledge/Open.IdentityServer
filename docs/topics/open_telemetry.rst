.. _refOpenTelemetry:

OpenTelemetry
=============

`OpenTelemetry <https://opentelemetry.io/>`_ is an open-source, vendor-neutral observability framework for generating, collecting, and exporting telemetry data (metrics, traces, and logs). It provides a standardised way to instrument applications, making it possible to monitor and troubleshoot distributed systems without being locked in to a particular observability vendor.

Open.IdentityServer provides built-in OpenTelemetry support through its ``ITelemetryService`` interface and ``DefaultTelemetryService`` implementation. Telemetry data is emitted automatically for key operations such as token issuance, client authentication, and request handling — with no additional configuration required to start collecting signals.

Overview
--------
OpenTelemetry defines three primary signal types:

- **Metrics** — Numerical measurements that describe the behaviour of your system over time (e.g. request counts, error rates, active connections).
- **Traces** — Records of the path a request takes through your system, broken into spans that represent individual units of work.
- **Logs** — Timestamped text records, enriched with structured data, that capture discrete events.

Open.IdentityServer emits metrics and traces natively using the .NET ``System.Diagnostics`` APIs (``Meter``, ``Counter``, ``ActivitySource``). Logging is handled through the standard ASP.NET Core ``ILogger`` infrastructure, which can be exported via OpenTelemetry's logging bridge.

Getting Started
^^^^^^^^^^^^^^^
OpenTelemetry signals are emitted by Open.IdentityServer automatically. To collect and export them, you need to configure the OpenTelemetry SDK in your host application.

Install the required NuGet packages:

.. code-block:: bash

    dotnet add package OpenTelemetry.Extensions.Hosting
    dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
    dotnet add package OpenTelemetry.Instrumentation.AspNetCore

Then configure the OpenTelemetry SDK in your ``Program.cs`` or ``Startup.cs``:

.. code-block:: csharp

    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;
    using OpenTelemetry.Logs;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddIdentityServer()
        // ... your IdentityServer configuration
        ;

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource =>
        {
            resource.AddService("<Your IdentityServer application name>");
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddMeter(TelemetryConstants.MetricsConstants.MeterName)
                .AddOtlpExporter();
        })
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddSource(TelemetryConstants.TraceCategories.Basic)
                .AddSource(TelemetryConstants.TraceCategories.Cache)
                .AddSource(TelemetryConstants.TraceCategories.Services)
                .AddSource(TelemetryConstants.TraceCategories.Stores)
                .AddSource(TelemetryConstants.TraceCategories.Validation)
                .AddOtlpExporter();
        });

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.AddOtlpExporter();
    });

.. note::
    The ``DefaultTelemetryService`` is registered as a singleton automatically when you call ``AddIdentityServer()``. No additional registration is needed to enable telemetry.

Metrics
-------
Open.IdentityServer exposes metrics through a .NET ``Meter`` named ``Open.IdentityServer``. All metric instruments are counters that track the volume and outcome of protocol operations.

Meter Name
^^^^^^^^^^
``Open.IdentityServer``

Available Instruments
^^^^^^^^^^^^^^^^^^^^^

.. list-table::
   :header-rows: 1
   :widths: 40 15 45

   * - Instrument Name
     - Type
     - Description
   * - ``tokenservice.operation``
     - Counter
     - Counts all operations, tagged with ``result`` (``success``, ``error``, or ``internal_error``), ``client``, and optionally ``error``.
   * - ``tokenservice.active_requests``
     - UpDownCounter
     - Tracks the number of currently active IdentityServer protocol requests. Tagged with ``endpoint`` and ``path``.
   * - ``tokenservice.api.secret_validation``
     - Counter
     - Counts API secret validation attempts. Tagged with ``api``, ``auth_method`` (on success), and ``error`` (on failure).
   * - ``tokenservice.client.secret_validation``
     - Counter
     - Counts client secret validation attempts. Tagged with ``client``, ``auth_method`` (on success), and ``error`` (on failure).
   * - ``tokenservice.client.config_validation``
     - Counter
     - Counts client configuration validation attempts. Tagged with ``client`` and ``error`` (on failure).
   * - ``tokenservice.token_issued``
     - Counter
     - Counts tokens issued. Tagged with ``client``, ``grant_type``, ``token_type`` and ``error`` (on failure).
   * - ``tokenservice.introspection``
     - Counter
     - Counts token introspection requests. Tagged with ``caller``, ``active`` (token status), and ``error`` (on failure).
   * - ``tokenservice.revocation``
     - Counter
     - Counts token revocation requests. Tagged with ``client`` and ``error`` (on failure).
   * - ``tokenservice.device_authentication``
     - Counter
     - Counts device authentication requests. Tagged with ``client`` and ``error`` (on failure).
   * - ``tokenservice.resourceowner_authentication``
     - Counter
     - Counts resource owner password authentication attempts. Tagged with ``client`` and ``error`` (on failure).
   * - ``tokenservice.backchannel_authentication``
     - Counter
     - Counts backchannel (CIBA) authentication requests. Tagged with ``client`` and ``error`` (on failure).  **Note:** CIBA is not yet implemented in Open.IdentityServer, but the metric is reserved for future use.
   * - ``tokenservice.pushed_authorization_request``
     - Counter
     - Counts pushed authorization requests (PAR). Tagged with ``client`` and ``error`` (on failure).  **Note:** PAR is not yet implemented in Open.IdentityServer, but the metric is reserved for future use.

Metric Tags
^^^^^^^^^^^^
Metrics are enriched with the following tags to enable filtering and grouping in your observability platform:

.. list-table::
   :header-rows: 1
   :widths: 20 80

   * - Tag
     - Description
   * - ``result``
     - The outcome of the operation: ``success``, ``error``, or ``internal_error``.
   * - ``client``
     - The client identifier associated with the operation.
   * - ``error``
     - A description of the error, present only when the operation failed.
   * - ``endpoint``
     - The logical endpoint handling the request (e.g. ``TokenEndpoint``).
   * - ``path``
     - The request path.
   * - ``api``
     - The API resource name (used in API secret validation).
   * - ``auth_method``
     - The authentication method used (e.g. ``client_secret_post``, ``private_key_jwt``).
   * - ``caller``
     - The caller of the introspection endpoint.
   * - ``active``
     - Whether the introspected token is active (``true``/``false``).
   * - ``grant_type``
     - The OAuth grant type used for token issuance.

Subscribing to Metrics
^^^^^^^^^^^^^^^^^^^^^^
To collect Open.IdentityServer metrics, subscribe to the ``Open.IdentityServer`` meter in your OpenTelemetry configuration:

.. code-block:: csharp

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddMeter(TelemetryConstants.MetricsConstants.MeterName);
        });

Traces
------
Open.IdentityServer uses .NET ``ActivitySource`` instances to create distributed trace spans. Traces provide visibility into the internal processing of each request, helping you identify performance bottlenecks and understand the flow of operations.

Activity Sources
^^^^^^^^^^^^^^^^
Open.IdentityServer defines several activity sources, each representing a category of operations:

.. list-table::
   :header-rows: 1
   :widths: 40 60

   * - Activity Source Name
     - Description
   * - ``Open.IdentityServer``
     - Top-level protocol request processing (e.g. the overall handling of a token or authorize request).
   * - ``Open.IdentityServer.Cache``
     - Caching operations (cache reads, writes, and invalidations).
   * - ``Open.IdentityServer.Services``
     - Internal service operations (token creation, consent, key material, etc.).
   * - ``Open.IdentityServer.Stores``
     - Data store operations (reading/writing grants, clients, resources).
   * - ``Open.IdentityServer.Validation``
     - Validation operations (token validation, request validation, secret validation).

Subscribing to Traces
^^^^^^^^^^^^^^^^^^^^^
To collect traces, add the relevant activity sources to your OpenTelemetry tracing configuration:

.. code-block:: csharp

    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            // Subscribe to all Open.IdentityServer activity sources
            tracing
                .AddAspNetCoreInstrumentation()
                .AddSource(TelemetryConstants.TraceCategories.Basic)
                .AddSource(TelemetryConstants.TraceCategories.Cache)
                .AddSource(TelemetryConstants.TraceCategories.Services)
                .AddSource(TelemetryConstants.TraceCategories.Stores)
                .AddSource(TelemetryConstants.TraceCategories.Validation);
        });

You can subscribe to a subset of sources if you only need visibility into specific categories. For example, to trace only store operations:

.. code-block:: csharp

    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing
              .AddAspNetCoreInstrumentation()
              .AddSource(TelemetryConstants.TraceCategories.Stores);
        });

Trace Granularity
^^^^^^^^^^^^^^^^^
Each trace activity is named after the class and method performing the operation, providing fine-grained visibility. For example, a token request might produce a trace hierarchy such as:

::

    Open.IdentityServer: IdentityServerProtocolRequest
    └─ Open.IdentityServer: TokenEndpoint.ProcessAsync
      └─ Open.IdentityServer.Validation: ClientSecretValidator.ValidateAsync
      └─ Open.IdentityServer.Validation: TokenRequestValidator.ValidateRequestAsync
      └─ Open.IdentityServer.Stores: DefaultRefreshTokenStore.GetRefreshTokenAsync
      └─ Open.IdentityServer.Services: DefaultTokenService.CreateAccessTokenAsync

Tracing in ASP.NET Core
^^^^^^^^^^^^^^^^^^^^^^^
Additional trace data is automatically captured by the OpenTelemetry ASP.NET Core instrumentation package (``OpenTelemetry.Instrumentation.AspNetCore``).

The ``AddAspNetCoreInstrumentation()`` method automatically creates traces for HTTP requests, including:

- Request duration
- HTTP method, route, and status code
- Network information
- User agent

These traces are collected without requiring any additional code in your controllers or middleware.  For more details on the ASP.NET Core instrumentation, see the `OpenTelemetry ASP.NET Core Instrumentation <https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/README.md>`_ documentation.

Logging
-------
Open.IdentityServer uses the standard ASP.NET Core ``ILogger`` infrastructure for structured logging. Log messages are emitted at various levels throughout the processing pipeline, providing detailed diagnostic information.

To export logs via OpenTelemetry, configure the logging bridge:

.. code-block:: csharp

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });

For more details on log levels and filtering, see the :ref:`Logging <refLogging>` documentation page.

.. note::
    If using Serilog, you will need to use the `OpenTelemetry Serilog Sink <https://github.com/serilog/serilog-sinks-opentelemetry>`_ to forward logs to OpenTelemetry. The built-in logging bridge does not support Serilog directly.

Exporters
---------
OpenTelemetry supports a wide variety of exporters for sending telemetry data to your chosen backend. Common choices include:

- **OTLP (OpenTelemetry Protocol)** — The standard protocol, supported by most observability platforms (e.g. Jaeger, Grafana, Datadog, Azure Monitor, AWS X-Ray).
- **Prometheus** — For metrics scraping (use ``OpenTelemetry.Exporter.Prometheus.AspNetCore``).
- **Console** — Useful during development (use ``OpenTelemetry.Exporter.Console``).

Example using the Console exporter for development:

.. code-block:: csharp

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics
                .AddMeter("Open.IdentityServer")
                .AddConsoleExporter();
        })
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddSource("Open.IdentityServer")
                .AddSource("Open.IdentityServer.Cache")
                .AddSource("Open.IdentityServer.Services")
                .AddSource("Open.IdentityServer.Stores")
                .AddSource("Open.IdentityServer.Validation")
                .AddConsoleExporter();
        });

Customisation
-------------
The ``ITelemetryService`` interface can be replaced with a custom implementation if you need to modify or extend the telemetry behaviour. The default implementation is registered with ``TryAddSingleton``, so you can override it by registering your own implementation before or after calling ``AddIdentityServer()``.

.. note::
    In most cases, the default implementation combined with OpenTelemetry SDK configuration provides sufficient flexibility without needing a custom ``ITelemetryService``.
