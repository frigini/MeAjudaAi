namespace MeAjudai.Shared.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string Error { get; }

    private Result(T value) => (IsSuccess, Value, Error) = (true, value, string.Empty);
    private Result(string error) => (IsSuccess, Value, Error) = (false, default!, error);

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);

    // Adicione apenas se precisar
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Error);
}

// Versão sem valor (para Commands)
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    private Result(bool isSuccess, string error) => (IsSuccess, Error) = (isSuccess, error);

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}