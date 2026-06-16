using MeAjudaAi.Modules.Locations.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Locations;

[ExcludeFromCodeCoverage]
public class AllowedCityBuilder : BaseBuilder<AllowedCity>
{
    private string? _cityName;
    private string? _stateSigla;
    private string? _createdBy;
    private int? _ibgeCode;
    private double _latitude;
    private double _longitude;
    private double _serviceRadiusKm;
    private bool _isActive = true;

    public AllowedCityBuilder()
    {
        Faker = new Faker<AllowedCity>()
            .CustomInstantiator(f =>
            {
                var cityName = _cityName ?? "Muriaé";
                var state = _stateSigla ?? "MG";
                var createdBy = _createdBy ?? "admin@test.com";

                return new AllowedCity(
                    cityName,
                    state,
                    createdBy,
                    _ibgeCode,
                    _latitude,
                    _longitude,
                    _serviceRadiusKm,
                    _isActive);
            });
    }

    public AllowedCityBuilder WithCityName(string cityName)
    {
        _cityName = cityName;
        return this;
    }

    public AllowedCityBuilder WithState(string stateSigla)
    {
        _stateSigla = stateSigla.ToUpperInvariant();
        return this;
    }

    public AllowedCityBuilder WithCreatedBy(string createdBy)
    {
        _createdBy = createdBy;
        return this;
    }

    public AllowedCityBuilder WithIbgeCode(int ibgeCode)
    {
        _ibgeCode = ibgeCode;
        return this;
    }

    public AllowedCityBuilder WithCoordinates(double latitude, double longitude)
    {
        _latitude = latitude;
        _longitude = longitude;
        return this;
    }

    public AllowedCityBuilder WithServiceRadius(double radiusKm)
    {
        _serviceRadiusKm = radiusKm;
        return this;
    }

    public AllowedCityBuilder AsActive()
    {
        _isActive = true;
        return this;
    }

    public AllowedCityBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    public static AllowedCityBuilder AsTestCity(string cityName, string state) => new AllowedCityBuilder()
        .WithCityName(cityName)
        .WithState(state);
}
