using System.Diagnostics;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Monitoring;

public static class MonitoringService
{
    public static readonly string ServiceName = Assembly.GetCallingAssembly().GetName().Name ?? "Unknown";
    public static TracerProvider tracerProvider;
    public static ActivitySource activitySource = new ActivitySource(ServiceName);

    public static ILogger log => Serilog.Log.Logger;

    static MonitoringService()
    {
        tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(activitySource.Name)
            .AddZipkinExporter()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
            .Build();

        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Seq("http://localhost:5341")
            .Enrich.WithSpan()
            .CreateLogger();
    }
}
