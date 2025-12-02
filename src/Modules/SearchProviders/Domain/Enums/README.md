# SearchProviders Domain Enums

## ESubscriptionTier

### Ownership Decision (Current: SearchProviders Module)

**Location:** `SearchProviders.Domain.Enums.ESubscriptionTier`

**Rationale:**
- ✅ **Current Consumer:** Only the SearchProviders module uses this enum for provider ranking
- ⚠️ **Future Consideration:** When Payment/Billing module is created, this enum should be:
  1. Moved to `Shared.Contracts` as a shared enum, OR
  2. Kept in Payment/Billing domain with SearchProviders module using it via module API

**Why Not Move Now:**
1. No Payment/Billing module exists yet
2. Only SearchProviders module needs it
3. YAGNI principle - don't add abstraction until needed
4. When Payment module is created, we'll have better understanding of cross-module dependencies

**Decision Documented:** 2024-12-XX
**Review When:** Payment/Billing module implementation begins

### Values

```csharp
public enum ESubscriptionTier
{
    Free = 0,      // Basic provider tier (lowest ranking)
    Standard = 1,  // Paid tier 1
    Gold = 2,      // Paid tier 2
    Platinum = 3   // Premium tier (highest ranking)
}
```

### Usage in Search

The subscription tier is used for **search result ranking**:
1. **Primary sort:** Platinum > Gold > Standard > Free
2. **Secondary sort:** Average rating (descending)
3. **Tertiary sort:** Distance from search location (ascending)

This ensures premium subscribers appear first in search results.
