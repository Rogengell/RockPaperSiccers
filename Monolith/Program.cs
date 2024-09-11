// See https://aka.ms/new-console-template for more information

using Monolith;
using Monitoring;
using Serilog;
using OpenTelemetry.Trace;

public class Program
{
    public static void Main()
    {
        using var activity = MonitoringService.activitySource.StartActivity("Start Man");
        var game = new Game();
        for (int i = 0; i < 1000; i++)
        {
            MonitoringService.log.Information("Starting game number: {i}", i);
            game.Start();
        }
        MonitoringService.log.Information("Finished all games");

        Console.WriteLine("Finished");
        
        Log.CloseAndFlush();
        MonitoringService.tracerProvider.ForceFlush();
    }
}