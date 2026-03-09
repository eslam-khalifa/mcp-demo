using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Interfaces;

public interface IPythonSandboxService
{
    Task<Result<string>> ExecuteAsync(string toolName, string jsonInput);
}
