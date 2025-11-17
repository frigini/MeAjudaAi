using System.Text.Json;
using System.Web;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API Nominatim (OpenStreetMap).
/// Endpoint: https://nominatim.openstreetmap.org/search?q={query}&format=json
/// 
/// IMPORTANTE: Respeitar política de uso justo:
/// - Máximo 1 requisição por segundo
/// - Incluir User-Agent identificando a aplicação
/// - Usar caching para reduzir chamadas
/// 
/// Documentação: https://nominatim.org/release-docs/latest/api/Search/
/// </summary>
public sealed class NominatimClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimClient> _logger;
    private readonly SemaphoreSlim _rateLimiter = new(1, 1); // 1 req/sec
    private DateTime _lastRequestTime = DateTime.MinValue;

    public NominatimClient(HttpClient httpClient, ILogger<NominatimClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configurar User-Agent conforme política de uso
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MeAjudaAi/1.0 (https://github.com/frigini/MeAjudaAi)");
    }

    public async Task<GeoPoint?> GetCoordinatesAsync(string address, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        try
        {
            // Rate limiting: esperar pelo menos 1 segundo entre requisições
            await _rateLimiter.WaitAsync(cancellationToken);
            try
            {
                var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
                if (timeSinceLastRequest < TimeSpan.FromSeconds(1))
                {
                    var delay = TimeSpan.FromSeconds(1) - timeSinceLastRequest;
                    await Task.Delay(delay, cancellationToken);
                }

                _lastRequestTime = DateTime.UtcNow;
            }
            finally
            {
                _rateLimiter.Release();
            }

            var encodedAddress = HttpUtility.UrlEncode(address);
            var url = $"https://nominatim.openstreetmap.org/search?q={encodedAddress}&format=json&limit=1&countrycodes=br";

            _logger.LogInformation("Consultando Nominatim para endereço: {Address}", address);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim retornou status {StatusCode} para endereço {Address}", 
                    response.StatusCode, address);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var results = JsonSerializer.Deserialize<NominatimResponse[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (results is null || results.Length == 0)
            {
                _logger.LogInformation("Nenhuma coordenada encontrada no Nominatim para endereço {Address}", address);
                return null;
            }

            var firstResult = results[0];

            if (string.IsNullOrWhiteSpace(firstResult.Lat) || string.IsNullOrWhiteSpace(firstResult.Lon))
            {
                _logger.LogWarning("Resultado do Nominatim sem coordenadas para endereço {Address}", address);
                return null;
            }

            if (!double.TryParse(firstResult.Lat, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out var latitude))
            {
                _logger.LogWarning("Latitude inválida retornada pelo Nominatim: {Lat}", firstResult.Lat);
                return null;
            }

            if (!double.TryParse(firstResult.Lon, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out var longitude))
            {
                _logger.LogWarning("Longitude inválida retornada pelo Nominatim: {Lon}", firstResult.Lon);
                return null;
            }

            GeoPoint coordinates;
            try
            {
                coordinates = new GeoPoint(latitude, longitude);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, "Coordenadas fora dos limites válidos: Lat={Lat}, Lon={Lon}", latitude, longitude);
                return null;
            }

            _logger.LogInformation("Coordenadas obtidas do Nominatim para {Address}: {Coordinates}", 
                address, coordinates);

            return coordinates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar Nominatim para endereço {Address}", address);
            return null;
        }
    }

    public void Dispose()
    {
        _rateLimiter.Dispose();
    }
}
