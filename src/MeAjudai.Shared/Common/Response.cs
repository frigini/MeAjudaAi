using System.Text.Json.Serialization;

namespace MeAjudai.Shared.Common;

public record Response<TData>
{
    private readonly int _code;

    [JsonConstructor]
    public Response() => _code = 200;

    public Response(
        TData? data,
        int statusCode = 200,
        string? message = null)
    {
        Data = data;
        _code = statusCode;
        Message = message;
    }

    [JsonIgnore]
    public bool IsSuccess => _code is >= 200 and <= 299;

    [JsonIgnore]
    public int StatusCode => _code; // Para acessar o código externamente

    public string? Message { get; init; }
    public TData? Data { get; init; }
}