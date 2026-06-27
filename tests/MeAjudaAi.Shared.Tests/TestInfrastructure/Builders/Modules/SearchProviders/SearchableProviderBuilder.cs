using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.SearchProviders;

[ExcludeFromCodeCoverage]
public class SearchableProviderBuilder : BaseBuilder<SearchableProvider>
{
    private Guid? _providerId;
    private string? _name;
    private string? _slug;
    private GeoPoint? _location;
    private ESubscriptionTier? _subscriptionTier;
    private string? _description;
    private string? _city;
    private string? _state;
    private decimal? _averageRating;
    private int? _totalReviews;
    private Guid[]? _serviceIds;
    private bool? _isActive;

    public SearchableProviderBuilder()
    {
        Faker = new Faker<SearchableProvider>()
            .CustomInstantiator(f =>
            {
                var providerId = _providerId ?? f.Random.Guid();
                var name = _name ?? f.Company.CompanyName();
                var slug = _slug ?? SlugHelper.Generate(name);
                var location = _location ?? new GeoPoint(
                    f.Address.Latitude(-33.0, -22.0),
                    f.Address.Longitude(-50.0, -43.0));
                var subscriptionTier = _subscriptionTier ?? f.Random.Enum<ESubscriptionTier>();
                var description = _description ?? f.Lorem.Sentence();
                var city = _city ?? f.Address.City();
                var state = _state ?? f.Address.StateAbbr();

                var provider = SearchableProvider.Create(
                    providerId: providerId,
                    name: name,
                    slug: slug,
                    location: location,
                    subscriptionTier: subscriptionTier,
                    description: description,
                    city: city,
                    state: state);

                if (_averageRating.HasValue || _totalReviews.HasValue)
                {
                    provider.UpdateRating(
                        _averageRating ?? 0,
                        _totalReviews ?? 0);
                }

                if (_serviceIds is not null)
                {
                    provider.UpdateServices(_serviceIds);
                }

                if (_isActive == false)
                {
                    provider.Deactivate();
                }

                return provider;
            });
    }

    public SearchableProviderBuilder WithProviderId(Guid providerId)
    {
        _providerId = providerId;
        return this;
    }

    public SearchableProviderBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SearchableProviderBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public SearchableProviderBuilder WithLocation(GeoPoint location)
    {
        _location = location;
        return this;
    }

    public SearchableProviderBuilder WithLocation(double latitude, double longitude)
    {
        _location = new GeoPoint(latitude, longitude);
        return this;
    }

    public SearchableProviderBuilder WithSubscriptionTier(ESubscriptionTier tier)
    {
        _subscriptionTier = tier;
        return this;
    }

    public SearchableProviderBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public SearchableProviderBuilder WithCity(string? city)
    {
        _city = city;
        return this;
    }

    public SearchableProviderBuilder WithState(string? state)
    {
        _state = state;
        return this;
    }

    public SearchableProviderBuilder WithRating(decimal averageRating, int totalReviews)
    {
        _averageRating = averageRating;
        _totalReviews = totalReviews;
        return this;
    }

    public SearchableProviderBuilder WithServiceIds(Guid[] serviceIds)
    {
        _serviceIds = serviceIds;
        return this;
    }

    public SearchableProviderBuilder AsActive()
    {
        _isActive = true;
        return this;
    }

    public SearchableProviderBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }
}
