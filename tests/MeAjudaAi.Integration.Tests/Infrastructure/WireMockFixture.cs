using WireMock.Admin.Requests;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture for WireMock HTTP server used to mock external APIs in integration tests.
/// Provides stubs for ViaCep, BrasilApi, OpenCep, Nominatim, and IBGE APIs.
/// </summary>
public class WireMockFixture : IAsyncDisposable
{
    private WireMockServer? _server;

    /// <summary>
    /// Gets the WireMock server instance.
    /// </summary>
    public WireMockServer Server => _server ?? throw new InvalidOperationException("WireMock server not started. Call StartAsync() first.");

    /// <summary>
    /// Gets the base URL for the mock server.
    /// </summary>
    public string BaseUrl => Server.Url!;

    /// <summary>
    /// Starts the WireMock server and configures all API stubs.
    /// </summary>
    public Task StartAsync()
    {
        _server = WireMockServer.Start(new WireMockServerSettings
        {
            Port = 0, // Use dynamic port to avoid conflicts in parallel test execution
            StartAdminInterface = true,
            ReadStaticMappings = false,
            WatchStaticMappings = false,
            Logger = new WireMockConsoleLogger()
        });

        // Configure all API stubs
        ConfigureIbgeStubs();
        ConfigureViaCepStubs();
        ConfigureBrasilApiStubs();
        ConfigureOpenCepStubs();
        ConfigureNominatimStubs();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Configures IBGE Localidades API stubs.
    /// </summary>
    private void ConfigureIbgeStubs()
    {
        // Muriaé/MG - IBGE code: 3143906
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "muriaé")
                .WithParam("orderBy", "nome")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [{
                        "id": 3143906,
                        "nome": "Muriaé",
                        "microrregiao": {
                            "id": 31054,
                            "nome": "Muriaé",
                            "mesorregiao": {
                                "id": 3107,
                                "nome": "Zona da Mata",
                                "UF": {
                                    "id": 31,
                                    "sigla": "MG",
                                    "nome": "Minas Gerais",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        },
                        "regiao-imediata": {
                            "id": 310027,
                            "nome": "Muriaé",
                            "regiao-intermediaria": {
                                "id": 3106,
                                "nome": "Juiz de Fora",
                                "UF": {
                                    "id": 31,
                                    "sigla": "MG",
                                    "nome": "Minas Gerais",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        }
                    }]
                    """));

        // Itaperuna/RJ - IBGE code: 3302205
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "itaperuna")
                .WithParam("orderBy", "nome")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [{
                        "id": 3302205,
                        "nome": "Itaperuna",
                        "microrregiao": {
                            "id": 33005,
                            "nome": "Itaperuna",
                            "mesorregiao": {
                                "id": 3301,
                                "nome": "Noroeste Fluminense",
                                "UF": {
                                    "id": 33,
                                    "sigla": "RJ",
                                    "nome": "Rio de Janeiro",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        },
                        "regiao-imediata": {
                            "id": 330007,
                            "nome": "Itaperuna",
                            "regiao-intermediaria": {
                                "id": 3301,
                                "nome": "Rio de Janeiro",
                                "UF": {
                                    "id": 33,
                                    "sigla": "RJ",
                                    "nome": "Rio de Janeiro",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        }
                    }]
                    """));

        // Linhares/ES - IBGE code: 3203205
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "linhares")
                .WithParam("orderBy", "nome")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [{
                        "id": 3203205,
                        "nome": "Linhares",
                        "microrregiao": {
                            "id": 32009,
                            "nome": "Linhares",
                            "mesorregiao": {
                                "id": 3202,
                                "nome": "Litoral Norte Espírito-Santense",
                                "UF": {
                                    "id": 32,
                                    "sigla": "ES",
                                    "nome": "Espírito Santo",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        },
                        "regiao-imediata": {
                            "id": 320002,
                            "nome": "Linhares",
                            "regiao-intermediaria": {
                                "id": 3201,
                                "nome": "Vitória",
                                "UF": {
                                    "id": 32,
                                    "sigla": "ES",
                                    "nome": "Espírito Santo",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        }
                    }]
                    """));

        // Unknown city - empty array response
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "CidadeInexistente")
                .WithParam("orderBy", "nome")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("[]"));

        // Service unavailability simulation - 500 error
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios/unavailable")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500)
                .WithHeader("Content-Type", "text/plain")
                .WithBody("Internal Server Error"));

        // Timeout simulation - delay 30 seconds (exceeds typical HTTP client timeout)
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios/timeout")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("[]")
                .WithDelay(TimeSpan.FromSeconds(30)));

        // Malformed response simulation - invalid JSON
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios/malformed")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("{invalid json"));
    }

    /// <summary>
    /// Configures ViaCep API stubs.
    /// </summary>
    private void ConfigureViaCepStubs()
    {
        // CEP 01310-100 - Avenida Paulista, São Paulo/SP
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/01310100/json/")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    {
                        "cep": "01310-100",
                        "logradouro": "Avenida Paulista",
                        "complemento": "lado ímpar",
                        "bairro": "Bela Vista",
                        "localidade": "São Paulo",
                        "uf": "SP",
                        "erro": false
                    }
                    """));

        // CEP inválido - erro
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/00000000/json/")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    {
                        "erro": true
                    }
                    """));
    }

    /// <summary>
    /// Configures BrasilApi CEP stubs.
    /// </summary>
    private void ConfigureBrasilApiStubs()
    {
        // CEP 01310-100 - Avenida Paulista, São Paulo/SP
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v1/01310100")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    {
                        "cep": "01310100",
                        "state": "SP",
                        "city": "São Paulo",
                        "neighborhood": "Bela Vista",
                        "street": "Avenida Paulista",
                        "service": "viacep"
                    }
                    """));
    }

    /// <summary>
    /// Configures OpenCep API stubs.
    /// </summary>
    private void ConfigureOpenCepStubs()
    {
        // CEP 01310-100 - Avenida Paulista, São Paulo/SP
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/01310100.json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    {
                        "cep": "01310-100",
                        "logradouro": "Avenida Paulista",
                        "complemento": "lado ímpar",
                        "bairro": "Bela Vista",
                        "localidade": "São Paulo",
                        "uf": "SP",
                        "ibge": "3550308"
                    }
                    """));
    }

    /// <summary>
    /// Configures Nominatim geocoding API stubs.
    /// </summary>
    private void ConfigureNominatimStubs()
    {
        // São Paulo coordinates (example: -23.5505, -46.6333)
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/reverse")
                .WithParam("lat", "-23.5505")
                .WithParam("lon", "-46.6333")
                .WithParam("format", "json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    {
                        "address": {
                            "city": "São Paulo",
                            "state": "São Paulo",
                            "country": "Brasil",
                            "country_code": "br"
                        },
                        "display_name": "São Paulo, Brasil"
                    }
                    """));
    }

    /// <summary>
    /// Resets all configured stubs and request logs.
    /// </summary>
    public void Reset()
    {
        Server.Reset();
        ConfigureIbgeStubs();
        ConfigureViaCepStubs();
        ConfigureBrasilApiStubs();
        ConfigureOpenCepStubs();
        ConfigureNominatimStubs();
    }

    /// <summary>
    /// Disposes the WireMock server.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        _server?.Stop();
        _server?.Dispose();
        _server = null;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Console logger for WireMock server.
/// </summary>
internal class WireMockConsoleLogger : IWireMockLogger
{
    public void Debug(string formatString, params object[] args)
    {
        // Suppress debug logs to reduce noise
    }

    public void Info(string formatString, params object[] args)
    {
        Console.WriteLine($"[WireMock INFO] {string.Format(formatString, args)}");
    }

    public void Warn(string formatString, params object[] args)
    {
        Console.WriteLine($"[WireMock WARN] {string.Format(formatString, args)}");
    }

    public void Error(string formatString, params object[] args)
    {
        Console.WriteLine($"[WireMock ERROR] {string.Format(formatString, args)}");
    }

    public void Error(string formatString, Exception exception)
    {
        Console.WriteLine($"[WireMock ERROR] {formatString} - {exception}");
    }

    public void DebugRequestResponse(LogEntryModel logEntryModel, bool isAdminRequest)
    {
        // Suppress request/response logs to reduce noise
    }
}
