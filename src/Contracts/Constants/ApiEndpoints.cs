namespace MeAjudaAi.Contracts.Constants;

[ExcludeFromCodeCoverage]
public static class ApiEndpoints
{
    public const string VersionPrefix = "/api/v1";

    public static class Users
    {
        public const string Base = "users";
        public const string Create = "/";
        public const string GetAll = "/";
        public const string GetById = "/{id:guid}";
        public const string Delete = "/{id:guid}";
        public const string GetByEmail = "/by-email/{email}";
        public const string UpdateProfile = "/{id:guid}/profile";
        public const string UpdateDeviceToken = "/{id:guid}/device-token";
        public const string GetAuthProviders = "/auth-providers";

        public static class Names
        {
            public const string Create = "CreateUser";
            public const string GetAll = "GetUsers";
            public const string GetById = "GetUserById";
            public const string Delete = "DeleteUser";
            public const string GetByEmail = "GetUserByEmail";
            public const string UpdateProfile = "UpdateUserProfile";
            public const string UpdateDeviceToken = "UpdateUserDeviceToken";
            public const string GetAuthProviders = "GetAuthProviders";
        }
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
        public const string Become = "/become";
        public const string AddService = "/{id:guid}/services/{serviceId:guid}";
        public const string RemoveService = "/{id:guid}/services/{serviceId:guid}";
        public const string GetMyProfile = "/me";
        public const string GetMyStatus = "/me/status";
        public const string UpdateMyProfile = "/me";
        public const string ActivateMyProfile = "/me/activate";
        public const string DeactivateMyProfile = "/me/deactivate";
        public const string DeleteMyProfile = "/me";
        public const string UploadMyDocument = "/me/documents";
        public const string UpdateDeviceToken = "/{id:guid}/device-token";
        public const string VerificationEvents = "/{id:guid}/verification-events";

        public static class Names
        {
            public const string Create = "CreateProvider";
            public const string GetAll = "GetProviders";
            public const string GetById = "GetProviderById";
            public const string Delete = "DeleteProvider";
            public const string GetByUserId = "GetProviderByUserId";
            public const string GetByCity = "GetProvidersByCity";
            public const string GetByState = "GetProvidersByState";
            public const string GetByType = "GetProvidersByType";
            public const string GetByVerificationStatus = "GetProvidersByVerificationStatus";
            public const string UpdateProfile = "UpdateProviderProfile";
            public const string UpdateVerificationStatus = "UpdateVerificationStatus";
            public const string AddDocument = "AddDocument";
            public const string RemoveDocument = "RemoveDocument";
            public const string RequireBasicInfoCorrection = "RequireBasicInfoCorrection";
            public const string GetPublicByIdOrSlug = "GetPublicProviderByIdOrSlug";
            public const string Become = "BecomeProvider";
            public const string AddService = "AddServiceToProvider";
            public const string RemoveService = "RemoveServiceFromProvider";
            public const string GetMyProfile = "GetMyProviderProfile";
            public const string GetMyStatus = "GetMyProviderStatus";
            public const string UpdateMyProfile = "UpdateMyProviderProfile";
            public const string ActivateMyProfile = "ActivateMyProviderProfile";
            public const string DeactivateMyProfile = "DeactivateMyProviderProfile";
            public const string DeleteMyProfile = "DeleteMyProviderProfile";
            public const string UploadMyDocument = "UploadMyDocument";
            public const string UpdateDeviceToken = "UpdateProviderDeviceToken";
            public const string VerificationEvents = "GetProviderVerificationEvents";
        }
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
        public const string GetEvents = "/{id:guid}/events";

        public static class Names
        {
            public const string Create = "CreateBooking";
            public const string Confirm = "ConfirmBooking";
            public const string Cancel = "CancelBooking";
            public const string Reject = "RejectBooking";
            public const string Complete = "CompleteBooking";
            public const string GetById = "GetBookingById";
            public const string GetMy = "GetMyBookings";
            public const string GetProviderBookings = "GetProviderBookings";
            public const string GetProviderAvailability = "GetProviderAvailability";
            public const string SetProviderSchedule = "SetProviderSchedule";
            public const string GetEvents = "GetBookingEvents";
        }
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

        public static class Names
        {
            public const string GetLogs = "GetCommunicationLogs";
            public const string GetTemplates = "GetEmailTemplates";
            public const string CreateTemplate = "CreateEmailTemplate";
            public const string UpdateTemplate = "UpdateEmailTemplate";
            public const string ActivateTemplate = "ActivateEmailTemplate";
            public const string DeactivateTemplate = "DeactivateEmailTemplate";
        }
    }

    public static class Documents
    {
        public const string Base = "documents";

        public static class Names
        {
            public const string GetById = "GetDocumentById";
            public const string GetByProvider = "GetProviderDocuments";
            public const string Upload = "UploadDocument";
            public const string Verify = "VerifyDocument";
            public const string RequestVerification = "RequestVerification";
            public const string Delete = "DeleteDocument";
        }
    }

    public static class Locations
    {
        public const string Base = "locations";
        public const string AdminAllowedCities = "admin/allowed-cities";
        public const string Search = "/search";

        public static class Names
        {
            public const string GetAll = "GetAllAllowedCities";
            public const string GetById = "GetAllowedCityById";
            public const string GetByState = "GetAllowedCitiesByState";
            public const string Create = "CreateAllowedCity";
            public const string Update = "UpdateAllowedCity";
            public const string Patch = "PatchAllowedCity";
            public const string Delete = "DeleteAllowedCity";
            public const string Search = "SearchLocations";
        }
    }

    public static class Payments
    {
        public const string Base = "payments";
        public const string CreateSubscription = "/subscriptions";
        public const string GetBillingPortal = "/subscriptions/billing-portal";
        public const string StripeWebhook = "/stripe";

        public static class Names
        {
            public const string CreateSubscription = "CreateSubscription";
            public const string GetBillingPortal = "GetBillingPortal";
            public const string StripeWebhook = "StripeWebhook";
        }
    }

    public static class Ratings
    {
        public const string Base = "ratings";
        public const string Create = "/";
        public const string GetById = "/{id:guid}";
        public const string GetByProvider = "/provider/{providerId:guid}";
        public const string GetStatus = "/{id:guid}/status";

        public static class Names
        {
            public const string Create = "CreateReview";
            public const string GetById = "GetReviewById";
            public const string GetByProvider = "GetProviderReviews";
            public const string GetStatus = "GetReviewStatus";
        }
    }

    public static class SearchProviders
    {
        public const string Base = "search";
        public const string ProvidersSearch = "search/providers";

        public static class Names
        {
            public const string Search = "SearchProviders";
        }
    }

    public static class ServiceCatalogs
    {
        public const string Base = "service-catalogs";

        public static class Categories
        {
            public const string Base = $"{ServiceCatalogs.Base}/categories";
            public const string GetAll = "/";
            public const string GetById = "/{id:guid}";
            public const string Create = "/";
            public const string Update = "/{id:guid}";
            public const string Activate = "/{id:guid}/activate";
            public const string Deactivate = "/{id:guid}/deactivate";
            public const string Delete = "/{id:guid}";

            public static class Names
            {
                public const string GetAll = "GetAllServiceCategories";
                public const string GetById = "GetServiceCategoryById";
                public const string Create = "CreateServiceCategory";
                public const string Update = "UpdateServiceCategory";
                public const string Activate = "ActivateServiceCategory";
                public const string Deactivate = "DeactivateServiceCategory";
                public const string Delete = "DeleteServiceCategory";
            }
        }

        public static class Services
        {
            public const string Base = $"{ServiceCatalogs.Base}/services";
            public const string GetAll = "/";
            public const string GetById = "/{id:guid}";
            public const string GetByCategory = "/category/{categoryId:guid}";
            public const string Create = "/";
            public const string Update = "/{id:guid}";
            public const string ChangeCategory = "/{id:guid}/change-category";
            public const string Activate = "/{id:guid}/activate";
            public const string Deactivate = "/{id:guid}/deactivate";
            public const string Delete = "/{id:guid}";
            public const string Validate = "/validate";

            public static class Names
            {
                public const string GetAll = "GetAllServices";
                public const string GetById = "GetServiceById";
                public const string GetByCategory = "GetServicesByCategory";
                public const string Create = "CreateService";
                public const string Update = "UpdateService";
                public const string ChangeCategory = "ChangeServiceCategory";
                public const string Activate = "ActivateService";
                public const string Deactivate = "DeactivateService";
                public const string Delete = "DeleteService";
                public const string Validate = "ValidateServices";
            }
        }
    }

    public static class System
    {
        public const string Health = "/health";
        public const string HealthReady = "/health/ready";
        public const string HealthLive = "/health/live";
    }
}
