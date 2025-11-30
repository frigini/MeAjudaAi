namespace MeAjudaAi.Shared.Geolocation;

public record GeoPoint
{
    public double Latitude { get; }
    public double Longitude { get; }

    public GeoPoint(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude deve estar entre -90 e 90");
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude deve estar entre -180 e 180");

        Latitude = latitude;
        Longitude = longitude;
    }

    public double DistanceTo(GeoPoint other)
    {
        // Implementação da fórmula de Haversine
        var R = 6371; // Raio da Terra em km
        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    /// Deconstrói o ponto geográfico em suas coordenadas componentes.
    /// Permite usar sintaxe de desconstrução: var (lat, lon) = geoPoint;
    /// </summary>
    /// <param name="latitude">Latitude em graus decimais (-90 a 90)</param>
    /// <param name="longitude">Longitude em graus decimais (-180 a 180)</param>
    public void Deconstruct(out double latitude, out double longitude)
    {
        latitude = Latitude;
        longitude = Longitude;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
