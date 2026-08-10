namespace GoldFieldsHR.Application.Common;

public class AuthResult<T>
{
    public bool Succeeded { get; }
    public T? Value { get; }
    public IReadOnlyList<string> Errors { get; }

    private AuthResult(bool succeeded, T? value, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        Errors = errors;
    }

    public static AuthResult<T> Success(T value) => new(true, value, Array.Empty<string>());

    public static AuthResult<T> Failure(params string[] errors) => new(false, default, errors);
}
