using System.Diagnostics;
using System.Text;

namespace MCPDemo.Infrastructure.PythonSandbox;

/// <summary>
/// Result of a process execution.
/// </summary>
public record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

/// <summary>
/// Abstraction for running Docker CLI processes.
/// </summary>
public interface IDockerProcessRunner
{
    /// <summary>
    /// Runs a Docker process with the specified arguments and stdin input.
    /// </summary>
    /// <param name="arguments">The CLI arguments to pass to docker.</param>
    /// <param name="stdinInput">The string to write to the process standard input.</param>
    /// <param name="cancellationToken">Token to cancel the execution.</param>
    /// <returns>The result containing exit code and captured output strings.</returns>
    Task<ProcessResult> RunAsync(string arguments, string stdinInput, CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of IDockerProcessRunner using System.Diagnostics.Process.
/// </summary>
public class DockerProcessRunner : IDockerProcessRunner
{
    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(string arguments, string stdinInput, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        process.OutputDataReceived += (s, e) => { if (e.Data != null) stdoutBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderrBuilder.AppendLine(e.Data); };

        if (!process.Start())
        {
            throw new Exception("Failed to start docker process.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (!string.IsNullOrEmpty(stdinInput))
        {
            await using (var sw = process.StandardInput)
            {
                await sw.WriteAsync(stdinInput);
            }
        }

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(true);
            }
            throw;
        }

        return new ProcessResult(
            process.ExitCode,
            stdoutBuilder.ToString().Trim(),
            stderrBuilder.ToString().Trim()
        );
    }
}
