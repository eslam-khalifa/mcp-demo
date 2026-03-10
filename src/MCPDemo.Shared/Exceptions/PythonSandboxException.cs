namespace MCPDemo.Shared.Exceptions;

/// <summary>
/// Exception thrown when a Python sandbox execution fails.
/// </summary>
public class PythonSandboxException : Exception
{
    public PythonSandboxException(string message) : base(message)
    {
    }

    public PythonSandboxException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
