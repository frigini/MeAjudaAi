using MeAjudaAi.Modules.Users.Domain.Enums;
using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.Entities;

public class ServiceProvider : AggregateRoot<UserId>
{
    private int _version = 0;

    public UserId UserId { get; private set; }
    public string CompanyName { get; private set; }
    public string? TaxId { get; private set; }
    public EServiceProviderTier Tier { get; private set; }
    public ESubscriptionStatus SubscriptionStatus { get; private set; }
    public DateTime? SubscriptionExpiresAt { get; private set; }
    public string? SubscriptionId { get; private set; }
    public List<string> ServiceCategories { get; private set; } = [];
    public string? Description { get; private set; }
    public decimal Rating { get; private set; }
    public int TotalReviews { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    
    // Business constraints based on tier
    public int MaxActiveServices => Tier switch
    {
        EServiceProviderTier.Standard => 5,
        EServiceProviderTier.Silver => 15,
        EServiceProviderTier.Gold => 50,
        EServiceProviderTier.Platinum => int.MaxValue,
        _ => 5
    };

    public bool CanAccessPremiumFeatures => Tier is EServiceProviderTier.Gold or EServiceProviderTier.Platinum;
    public bool CanCustomizeBranding => Tier == EServiceProviderTier.Platinum;

    private ServiceProvider() { } // EF Constructor

    public ServiceProvider(
        UserId id,
        UserId userId,
        string companyName,
        string? taxId = null,
        EServiceProviderTier tier = EServiceProviderTier.Standard)
    {
        Id = id;
        UserId = userId;
        CompanyName = companyName;
        TaxId = taxId;
        Tier = tier;
        SubscriptionStatus = ESubscriptionStatus.Active;
        Rating = 0;
        TotalReviews = 0;
        _version++;
        
        MarkAsUpdated();
    }

    public void UpdateTier(EServiceProviderTier newTier, string changedBy)
    {
        if (Tier == newTier) return;

        var previousTier = Tier.ToString();
        Tier = newTier;
        _version++;
        MarkAsUpdated();

        AddDomainEvent(new UserTierChangedDomainEvent(
            UserId.Value,
            _version,
            previousTier,
            newTier.ToString(),
            changedBy
        ));
    }

    public void UpdateSubscription(string subscriptionId, ESubscriptionStatus status, DateTime? expiresAt = null)
    {
        SubscriptionId = subscriptionId;
        SubscriptionStatus = status;
        SubscriptionExpiresAt = expiresAt;
        _version++;
        MarkAsUpdated();

        AddDomainEvent(new UserSubscriptionUpdatedDomainEvent(
            UserId.Value,
            _version,
            subscriptionId,
            status.ToString(),
            expiresAt
        ));
    }

    public void AddServiceCategory(string category)
    {
        if (!ServiceCategories.Contains(category))
        {
            ServiceCategories.Add(category);
            MarkAsUpdated();
        }
    }

    public void RemoveServiceCategory(string category)
    {
        if (ServiceCategories.Remove(category))
        {
            MarkAsUpdated();
        }
    }

    public void UpdateRating(decimal newRating, int totalReviews)
    {
        Rating = newRating;
        TotalReviews = totalReviews;
        MarkAsUpdated();
    }

    public void Verify()
    {
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void UpdateProfile(string companyName, string? description, string? taxId = null)
    {
        CompanyName = companyName;
        Description = description;
        TaxId = taxId;
        MarkAsUpdated();
    }
}