using WireMock.Admin.Requests;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture para servidor HTTP WireMock usado para simular APIs externas em testes de integração.
/// Fornece stubs para as APIs ViaCep, BrasilApi, OpenCep, Nominatim e IBGE.
/// </summary>
public class WireMockFixture : IAsyncDisposable
{
    private WireMockServer? _server;

    /// <summary>
    /// Obtém a instância do servidor WireMock.
    /// </summary>
    public WireMockServer Server => _server ?? throw new InvalidOperationException("WireMock server not started. Call StartAsync() first.");

    /// <summary>
    /// Obtém a URL base para o servidor mock.
    /// </summary>
    public string BaseUrl => Server.Url!;

    /// <summary>
    /// Inicia o servidor WireMock e configura todos os stubs de API.
    /// </summary>
    public Task StartAsync()
    {
        _server = WireMockServer.Start(new WireMockServerSettings
        {
            Port = 0, // Usa porta dinâmica para evitar conflitos na execução paralela de testes
            StartAdminInterface = true,
            ReadStaticMappings = false,
            WatchStaticMappings = false,
            Logger = new WireMockConsoleLogger()
        });

        // Configura todos os stubs de API
        ConfigureIbgeStubs();
        ConfigureViaCepStubs();
        ConfigureBrasilApiStubs();
        ConfigureOpenCepStubs();
        ConfigureNominatimStubs();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Configura os stubs da API IBGE Localidades.
    /// </summary>
    private void ConfigureIbgeStubs()
    {
        // Busca por nome de cidade: Muriaé/MG - Código IBGE: 3143906
        // Usando Regex para o parâmetro 'nome' para lidar com variações de codificação de URL (ex: %C3%A9 para é)
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", new WireMock.Matchers.RegexMatcher("(?i)^muria(%C3%A9|\u00E9|e)$", true))
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
                        }
                    }]
                    """));

        // Busca por nome de cidade: Itaperuna/RJ
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", new WireMock.Matchers.RegexMatcher("(?i)^itaperuna$", true))
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
                        }
                    }]
                    """));

        // Busca por nome de cidade: Linhares/ES
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", new WireMock.Matchers.RegexMatcher("(?i)^linhares$", true))
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [{
                        "id": 3201506,
                        "nome": "Linhares",
                        "microrregiao": {
                            "id": 32004,
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
                        }
                    }]
                    """));

        // Busca cidade por ID: Muriaé/MG
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios/3143906")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    {
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
                        }
                    }
                    """));

        // Busca todas as UFs
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/estados")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [
                        {
                            "id": 31,
                            "sigla": "MG",
                            "nome": "Minas Gerais",
                            "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                        },
                        {
                            "id": 33,
                            "sigla": "RJ",
                            "nome": "Rio de Janeiro",
                            "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                        },
                        {
                            "id": 35,
                            "sigla": "SP",
                            "nome": "São Paulo",
                            "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                        }
                    ]
                    """));

        // Busca estado por ID: MG
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/estados/31")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    {
                        "id": 31,
                        "sigla": "MG",
                        "nome": "Minas Gerais",
                        "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                    }
                    """));

        // Busca estado por UF: MG
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/estados/MG")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    {
                        "id": 31,
                        "sigla": "MG",
                        "nome": "Minas Gerais",
                        "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                    }
                    """));

        // Busca por estado: SP
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/estados/SP/municipios")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [{
                        "id": 3550308,
                        "nome": "São Paulo",
                        "microrregiao": {
                            "id": 35061,
                            "nome": "São Paulo",
                            "mesorregiao": {
                                "id": 3515,
                                "nome": "Metropolitana de São Paulo",
                                "UF": {
                                    "id": 35,
                                    "sigla": "SP",
                                    "nome": "São Paulo",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        }
                    }]
                    """));

        // ID de cidade inválido - 404
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios/9999999")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("[]"));

        // ID de estado inválido - 404
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/estados/999")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("[]"));

        // Tratamento de caracteres especiais: São Paulo
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "são paulo")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [{
                        "id": 3550308,
                        "nome": "São Paulo",
                        "microrregiao": {
                            "id": 35061,
                            "nome": "São Paulo",
                            "mesorregiao": {
                                "id": 3515,
                                "nome": "Metropolitana de São Paulo",
                                "UF": {
                                    "id": 35,
                                    "sigla": "SP",
                                    "nome": "São Paulo",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        }
                    }]
                    """));

        // Catch-all para cidades desconhecidas - retorna array vazio (status 200, não 404)
        // Isso permite que o IbgeClient retorne null em vez de lançar exceção
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome")  // Corresponde a qualquer parâmetro nome
                .UsingGet())
            .AtPriority(100)  // Prioridade menor para que stubs específicos coincidam primeiro
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("[]"));

        // Simulação de indisponibilidade de serviço - erro 500
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios/unavailable")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500)
                .WithHeader("Content-Type", "text/plain")
                .WithBody("Internal Server Error"));

        // Simulação de timeout - delay de 5 segundos (dentro do timeout de 30s do HttpClient configurado nos testes)
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios/timeout")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("[]")
                .WithDelay(TimeSpan.FromSeconds(5)));

        // Simulação de resposta malformada - JSON inválido
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
    /// Configura os stubs da API ViaCep.
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
    /// Configura os stubs da API BrasilApi CEP.
    /// </summary>
    private void ConfigureBrasilApiStubs()
    {
        // CEP 01310-100 - Avenida Paulista, São Paulo/SP
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v2/01310100")
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
    /// Configura os stubs da API OpenCep.
    /// </summary>
    private void ConfigureOpenCepStubs()
    {
        // CEP 01310-100 - Avenida Paulista, São Paulo/SP
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/v1/01310100")
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
    /// Configura os stubs da API Nominatim (geocoding).
    /// </summary>
    private void ConfigureNominatimStubs()
    {
        // Busca por São Paulo
        Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/search")
                .WithParam("q", "São Paulo, Brasil")
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
    /// Reseta todos os stubs configurados e logs de requisição.
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
    /// Descarta o servidor WireMock.
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
/// Logger de console para o servidor WireMock.
/// </summary>
internal class WireMockConsoleLogger : IWireMockLogger
{
    public void Debug(string formatString, params object[] args)
    {
        // Suprime logs de debug para reduzir ruído
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
        // Suprime logs de requisição/resposta para reduzir ruído
    }
}
