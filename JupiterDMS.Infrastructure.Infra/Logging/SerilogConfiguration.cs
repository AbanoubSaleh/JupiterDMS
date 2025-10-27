using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Enrichers;

namespace JupiterDMS.Infrastructure.Infra.Logging;

/// <summary>
/// Configuration for Serilog logging.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog with console and file sinks.
    /// </summary>
    /// <returns>The configured logger.</returns>
    public static Logger ConfigureLogger()
    {
        var logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        if (!Directory.Exists(logsPath))
        {
            Directory.CreateDirectory(logsPath);
        }

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "JupiterDMS")
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logsPath, "jupiterdms-.txt"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 30)
            .CreateLogger();
    }
}

