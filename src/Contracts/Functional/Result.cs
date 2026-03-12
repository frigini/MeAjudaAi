using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Functional;

/// <summary>
/// Representa o resultado de uma operação que pode retornar um valor de tipo <typeparamref name="T"/>.
/// <para>
/// <b>Nota sobre tipos nullable:</b> quando <typeparamref name="T"/> é um tipo de referência nullable
/// (ex.: <c>Result&lt;MyDto?&gt;</c>), <see cref="Value"/> pode ser <c>null</c> mesmo em caso de sucesso.
/// Nesse cenário, os atributos <c>[MemberNotNullWhen]</c> não se aplicam e o chamador deve verificar a nulidade
/// do valor separadamente. Prefira usar <c>Result&lt;MyDto&gt;</c> (não-nullable) quando o sucesso
/// garantir um valor.
/// </para>
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value { get; }
    public Error Error { get; }

    [JsonConstructor]
    public Result(bool isSuccess, T value, Error error)
    {
        if (isSuccess)
        {
            if (error != null) 
                throw new ArgumentException("Success result cannot have an error.", nameof(error));
        }
        else
        {
            ArgumentNullException.ThrowIfNull(error);
        }

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    private Result(T value) 
    {
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

    public static implicit operator Result<T>(T value) => Success(value);
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
            onFailure(Error);
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

