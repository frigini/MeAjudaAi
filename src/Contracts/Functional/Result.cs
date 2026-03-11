using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Functional;

public class Result<T>
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public T Value { get; }
    public Error Error { get; }

    [JsonConstructor]
    public Result(bool isSuccess, T value, Error error)
    {
        if (isSuccess)
        {
            if (EqualityComparer<T>.Default.Equals(value, default!))
                throw new ArgumentNullException(nameof(value), "Success result must have a non-default value.");
                
            if (error != null) 
                throw new ArgumentException("Success result cannot have an error.", nameof(error));
        }
        else
        {
            ArgumentNullException.ThrowIfNull(error);
            
            if (!EqualityComparer<T>.Default.Equals(value, default!))
                throw new ArgumentException("Failure result must have a default value.", nameof(value));
        }

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    private Result(T value) 
    {
        if (EqualityComparer<T>.Default.Equals(value, default!))
            throw new ArgumentNullException(nameof(value), "Success result must have a non-default value.");

        IsSuccess = true;
        Value = value;
        Error = null!;
    }

    private Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        IsSuccess = false;
        Value = default!;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
    public static Result<T> Failure(string message) => new(Error.BadRequest(message));

    public static implicit operator Result<T>(T value) => value is null ? throw new ArgumentNullException(nameof(value)) : Success(value);
    public static implicit operator Result<T>(Error error) => error is null ? throw new ArgumentNullException(nameof(error)) : Failure(error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Error);
}

public class Result
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    [JsonConstructor]
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != null) throw new ArgumentException("Success result cannot have an error.", nameof(error));
        if (!isSuccess && error == null) throw new ArgumentNullException(nameof(error), "Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null!);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string message) => new(false, Error.BadRequest(message));

    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Executa ação de sucesso ou falha conforme o resultado.
    /// </summary>
    /// <param name="onSuccess">Ação a executar em caso de sucesso. Não pode ser nulo.</param>
    /// <param name="onFailure">Ação a executar em caso de falha. Não pode ser nulo.</param>
    /// <exception cref="ArgumentNullException">Lançada quando onSuccess ou onFailure é nulo.</exception>
    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error!);
    }

    /// <summary>
    /// Executa função de sucesso ou falha conforme o resultado.
    /// </summary>
    /// <typeparam name="TResult">O tipo do resultado</typeparam>
    /// <param name="onSuccess">Função a executar em caso de sucesso. Não pode ser nulo.</param>
    /// <param name="onFailure">Função a executar em caso de falha. Não pode ser nulo.</param>
    /// <returns>O resultado da função executada</returns>
    /// <exception cref="ArgumentNullException">Lançada quando onSuccess ou onFailure é nulo.</exception>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess() : onFailure(Error);
    }
}

