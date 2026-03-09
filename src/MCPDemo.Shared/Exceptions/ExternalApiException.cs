namespace MCPDemo.Shared.Exceptions;

/// <summary>
/// Thrown when the external Platzi API returns an error.
/// </summary>
public class ExternalApiException : Exception
{
    public int StatusCode { get; }
    public string ResponseBody { get; }

    public ExternalApiException(int statusCode, string responseBody, string message) : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
