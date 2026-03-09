namespace MCPDemo.Shared.Models;

/// <summary>
/// A generic wrapper for operation outcomes.
/// </summary>
/// <typeparam name="T">The type of the success payload.</typeparam>
public class Result<T>
{
    private readonly T? _value;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    /// <summary>
    /// Gets the success payload.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Value on a failed result.</exception>
    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Cannot access Value of a failed result.");

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with a non-null payload.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
    public static Result<T> Success(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Success result must carry a non-null payload.");
        }

        return new Result<T>(true, value, null);
    }

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error);
    }
}
