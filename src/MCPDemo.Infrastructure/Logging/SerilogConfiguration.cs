using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace MCPDemo.Infrastructure.Logging;

/// <summary>
/// Helper for Serilog logger configuration.
/// </summary>
public static class SerilogConfiguration
{
    private const string LogPath = "logs/mcp-tool-.log";

    public static LoggerConfiguration CreateConfiguration()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: LogPath,
                rollingInterval: RollingInterval.Day,
                formatter: new CompactJsonFormatter(),
                restrictedToMinimumLevel: LogEventLevel.Information
            );
    }
}
