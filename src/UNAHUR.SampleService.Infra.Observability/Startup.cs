namespace UNAHUR.SampleService.Infra.Observability;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

public static class Startup
{


    /// <summary>
    /// Configura Observabilidad con OpenTelemetry para API
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder ConfigureOpenTelemetryAPI(this IHostApplicationBuilder builder, string serviceName, string serviceVersion = "", string serviceInstanceId = "")
    {
        // configura otel y agrega mtricas y traces propios de ASP.NET        
        builder.AddOpenTelemetryBase(serviceName, serviceVersion, serviceInstanceId)
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
            })
            .WithTracing(tracing =>
            {


                tracing.AddAspNetCoreInstrumentation(o =>
                    {
                        o.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("requestProtocol", httpRequest.Protocol);
                        };
                        o.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("responseLength", httpResponse.ContentLength);
                        };
                        o.EnrichWithException = (activity, exception) =>
                        {
                            activity.SetTag("exceptionType", exception.GetType());
                        };
                    });
            });
        return builder;
    }

    /// <summary>
    /// COnfiguracion de opentelemetry comun a api y a bacground workers
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static OpenTelemetryBuilder AddOpenTelemetryBase(this IHostApplicationBuilder builder,string serviceName, string serviceVersion= "", string serviceInstanceId= "" )
    {

        // Build a resource configuration action to set service information.
        // esta forma inusual de seteo sale de https://github.com/open-telemetry/opentelemetry-dotnet/blob/45cf7c3a25f768eca0a1b944120a4d28ae6ae33f/examples/AspNetCore/Program.cs#L27C1-L32C1
        void configureResource(ResourceBuilder r) => r
            .AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion ?? "unknown",
                serviceInstanceId: serviceInstanceId ?? Environment.MachineName)
            .AddEnvironmentVariableDetector();// es importante que esto este en este despues del addService para uqe en produccion podamos sobreescribir el sarvice name con la variable "OTEL_SERVICE_NAME"


        var resourceBuilder = ResourceBuilder.CreateDefault();
        configureResource(resourceBuilder);

        // agrega el destino de logueo open telemetry
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(resourceBuilder);
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        // AGREGA UN HOSTED SERVICE QUE RECOLECTA LOGS, TRACE Y METRICAS
        var otelBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(configureResource)
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // We want to view all traces in development
                    tracing.SetSampler(new AlwaysOnSampler());
                }
                tracing
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("MassTransit");
            });

        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => { logging.AddOtlpExporter();  });
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => { metrics.AddOtlpExporter(); });
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => { tracing.AddOtlpExporter(); });

        }

        // Uncomment the following lines to enable the Prometheus exporter (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
        // builder.Services.AddOpenTelemetry()
        //    .WithMetrics(metrics => metrics.AddPrometheusExporter());

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.Exporter package)
        // builder.Services.AddOpenTelemetry()
        //    .UseAzureMonitor();


        return otelBuilder;
    }

    

}
