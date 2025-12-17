using System.Text.Json;
using System.Web;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API Nominatim (OpenStreetMap).
/// IMPORTANTE: Respeitar política de uso justo:
/// - Máximo 1 requisição por segundo
/// - Incluir User-Agent identificando a aplicação
/// - Usar caching para reduzir chamadas
/// Documentação: https://nominatim.org/release-docs/latest/api/Search/
/// </summary>
public sealed class NominatimClient(HttpClient httpClient, ILogger<NominatimClient> logger, IDateTimeProvider dateTimeProvider) : IDisposable
{
    private readonly SemaphoreSlim _rateLimiter = new(1, 1); // 1 req/sec
    private DateTime _lastRequestTime = DateTime.MinValue;

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
                var timeSinceLastRequest = dateTimeProvider.CurrentDate() - _lastRequestTime;
                if (timeSinceLastRequest < TimeSpan.FromSeconds(1))
                {
                    var delay = TimeSpan.FromSeconds(1) - timeSinceLastRequest;
                    await Task.Delay(delay, cancellationToken);
                }

                var encodedAddress = HttpUtility.UrlEncode(address);
                var url = $"search?q={encodedAddress}&format=json&limit=1&countrycodes=br";

                logger.LogInformation("Querying Nominatim for address: {Address}", address);

                _lastRequestTime = dateTimeProvider.CurrentDate();
                var response = await httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Nominatim returned status {StatusCode} for address {Address}",
                        response.StatusCode, address);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var results = JsonSerializer.Deserialize<NominatimResponse[]>(content, SerializationDefaults.Default);

                if (results is null || results.Length == 0)
                {
                    logger.LogInformation("No coordinates found in Nominatim for address {Address}", address);
                    return null;
                }

                var firstResult = results[0];

                if (string.IsNullOrWhiteSpace(firstResult.Lat) || string.IsNullOrWhiteSpace(firstResult.Lon))
                {
                    logger.LogWarning("Nominatim result without coordinates for address {Address}", address);
                    return null;
                }

                if (!double.TryParse(firstResult.Lat, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var latitude))
                {
                    logger.LogWarning("Invalid latitude returned by Nominatim: {Lat}", firstResult.Lat);
                    return null;
                }

                if (!double.TryParse(firstResult.Lon, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var longitude))
                {
                    logger.LogWarning("Invalid longitude returned by Nominatim: {Lon}", firstResult.Lon);
                    return null;
                }

                GeoPoint coordinates;
                try
                {
                    coordinates = new GeoPoint(latitude, longitude);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    logger.LogWarning(ex, "Coordinates outside valid bounds: Lat={Lat}, Lon={Lon}", latitude, longitude);
                    return null;
                }

                logger.LogInformation("Coordinates obtained from Nominatim for {Address}: {Coordinates}",
                    address, coordinates);

                return coordinates;
            }
            finally
            {
                _rateLimiter.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error querying Nominatim for address {Address}", address);
            return null;
        }
    }

    public void Dispose()
    {
        _rateLimiter.Dispose();
    }
}
