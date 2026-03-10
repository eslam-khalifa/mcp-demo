using System.Text.Json;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.Interfaces;
using MCPDemo.Shared.Exceptions;

namespace MCPDemo.Infrastructure.PythonSandbox;

/// <summary>
/// Implementation of IPythonSandboxService that runs Python code inside a Docker container.
/// </summary>
public class PythonSandboxService : IPythonSandboxService
{
    private readonly IDockerProcessRunner _processRunner;
    private readonly ILogger<PythonSandboxService> _logger;
    private readonly IMetricsCollector _metrics;
    private const string ImageName = "mcp-python-sandbox";
    private const int TimeoutSeconds = 30;

    public PythonSandboxService(
        IDockerProcessRunner processRunner, 
        ILogger<PythonSandboxService> logger,
        IMetricsCollector metrics)
    {
        _processRunner = processRunner;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<string> ExecuteAsync(string code, string? jsonData)
    {
        _logger.LogInformation("Launching Python sandbox container for code execution.");

        // Build the payload for main.py
        var payload = JsonSerializer.Serialize(new
        {
            code,
            data = jsonData ?? "null"
        });

        // Docker flags as defined in the implementation plan:
        // --rm: Automatically remove the container when it exits
        // -i: Keep STDIN open even if not attached
        // --memory=256m: Limit memory to 256MB
        // --cpus=0.5: Limit CPU to 0.5 cores
        // --network=none: Disable all networking
        // --read-only: Mount the container's root filesystem as read-only
        // --security-opt=no-new-privileges: Prevent privilege escalation
        var dockerArgs = $"run --rm -i --memory=256m --cpus=0.5 --network=none --read-only --security-opt=no-new-privileges {ImageName}";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await _processRunner.RunAsync(dockerArgs, payload, cts.Token);

            if (result.ExitCode != 0)
            {
                var combinedOutput = $"Stdout: {result.StandardOutput}\nStderr: {result.StandardError}";
                
                _logger.LogError("Python sandbox execution failed with exit code {ExitCode}. Output: {Output}", 
                    result.ExitCode, combinedOutput);
                
                _metrics.RecordExecution("__PythonSandbox__", sw.ElapsedMilliseconds, false, "ExecutionError");
                throw new PythonSandboxException($"Python execution failed (Code {result.ExitCode}): {combinedOutput}");
            }

            _metrics.RecordExecution("__PythonSandbox__", sw.ElapsedMilliseconds, true);
            _logger.LogInformation("Python sandbox execution completed successfully in {Elapsed}ms.", sw.ElapsedMilliseconds);
            return result.StandardOutput;
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            _logger.LogError("Python sandbox execution timed out after {Timeout} seconds.", TimeoutSeconds);
            _metrics.RecordExecution("__PythonSandbox__", sw.ElapsedMilliseconds, false, "Timeout");
            throw new PythonSandboxException($"Python execution timed out after {TimeoutSeconds} seconds.");
        }
        catch (Exception ex) when (ex is not PythonSandboxException)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _logger.LogError(ex, "Unexpected error during Python sandbox execution.");
            _metrics.RecordExecution("__PythonSandbox__", sw.ElapsedMilliseconds, false, errorType);
            throw new PythonSandboxException($"Unexpected error during Python execution: {ex.Message}", ex);
        }
    }
}
