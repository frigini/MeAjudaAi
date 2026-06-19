using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para endpoints da API organizados por módulo
/// </summary>
[ExcludeFromCodeCoverage]
public static class ApiEndpoints
{
    public static class Users
    {
        public const string Base = "users";
        public const string Create = "/";
        public const string GetAll = "/";
        public const string GetById = "/{id:guid}";
        public const string Delete = "/{id:guid}";
        public const string GetByEmail = "/by-email/{email}";
        public const string UpdateProfile = "/{id:guid}/profile";
    }

    public static class Providers
    {
        public const string Base = "providers";
        public const string Create = "/";
        public const string GetAll = "/";
        public const string GetById = "/{id:guid}";
        public const string Delete = "/{id:guid}";
        public const string GetByUserId = "/by-user/{userId:guid}";
        public const string GetByCity = "/by-city/{city}";
        public const string GetByState = "/by-state/{state}";
        public const string GetByType = "/by-type/{type}";
        public const string GetByVerificationStatus = "/verification-status/{status}";
        public const string UpdateProfile = "/{id:guid}/profile";
        public const string UpdateVerificationStatus = "/{id:guid}/verification-status";
        public const string AddDocument = "/{id:guid}/documents";
        public const string RemoveDocument = "/{id:guid}/documents/{documentType}";
        public const string RequireBasicInfoCorrection = "/{id:guid}/require-basic-info-correction";
        public const string GetPublicByIdOrSlug = "/public/{idOrSlug}";
    }

    public static class Bookings
    {
        public const string Base = "bookings";
        public const string Create = "/";
        public const string Confirm = "/{id:guid}/confirm";
        public const string Cancel = "/{id:guid}/cancel";
        public const string Reject = "/{id:guid}/reject";
        public const string Complete = "/{id:guid}/complete";
        public const string GetById = "/{id:guid}";
        public const string GetMy = "/my";
        public const string GetProviderBookings = "/provider/{providerId:guid}";
        public const string GetProviderAvailability = "/availability/{providerId:guid}";
        public const string SetProviderSchedule = "/schedule";
    }

    public static class Communications
    {
        public const string Base = "communications";
        public const string GetLogs = "/logs";
        public const string GetTemplates = "/templates";
        public const string CreateTemplate = "/templates";
        public const string UpdateTemplate = "/templates/{id:guid}";
        public const string ActivateTemplate = "/templates/{id:guid}/activate";
        public const string DeactivateTemplate = "/templates/{id:guid}/deactivate";
    }

    public static class Documents
    {
        public const string Base = "documents";
    }

    public static class Locations
    {
        public const string Base = "locations";
        public const string AdminAllowedCities = "admin/allowed-cities";
    }

    public static class Payments
    {
        public const string Base = "payments";
        public const string CreateSubscription = "/subscriptions";
        public const string GetBillingPortal = "/subscriptions/billing-portal";
        public const string StripeWebhook = "/stripe";
    }

    public static class Ratings
    {
        public const string Base = "ratings";
    }

    public static class SearchProviders
    {
        public const string Base = "search";
    }

    public static class ServiceCatalogs
    {
        public const string Base = "service-catalogs";
        public const string Categories = "service-catalogs/categories";
        public const string Services = "service-catalogs/services";
    }

    public static class System
    {
        public const string Health = "/health";
        public const string HealthReady = "/health/ready";
        public const string HealthLive = "/health/live";
    }
}
