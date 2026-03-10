using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Interfaces;

public interface IPythonSandboxService
{
    /// <summary>
    /// Executes the provided Python code within a Docker sandbox.
    /// </summary>
    /// <param name="code">The Python source code to execute.</param>
    /// <param name="jsonData">The JSON data payload to pass to the script.</param>
    /// <returns>The captured standard output from the execution.</returns>
    Task<string> ExecuteAsync(string code, string? jsonData);
}
