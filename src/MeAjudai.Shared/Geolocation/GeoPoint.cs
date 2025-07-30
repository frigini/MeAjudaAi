namespace MeAjudaAi.Shared.Geolocation;

public record GeoPoint(double Latitude, double Longitude)
{
    public double DistanceTo(GeoPoint other)
    {
        // Haversine formula implementation
        var R = 6371; // Earth's radius in km
        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                Math.Sin(dLon/2) * Math.Sin(dLon/2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}