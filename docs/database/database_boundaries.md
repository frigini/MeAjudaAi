# 🗄️ Database Boundaries Strategy - MeAjudaAi Platform

Following [Milan Jovanović's approach](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith) for maintaining data boundaries in Modular Monoliths.

## 🎯 Core Principles

### Enforced Boundaries at Database Level
- ✅ **One schema per module** with dedicated database role
- ✅ **Role-based permissions** restrict access to module's own schema only
- ✅ **One DbContext per module** with default schema configuration
- ✅ **Separate connection strings** using module-specific credentials
- ✅ **Cross-module access** only through explicit views or APIs

## 📁 File Structure

```text
infrastructure/database/
├── 📂 shared/                          # Base platform scripts
│   ├── 00-create-base-roles.sql        # Shared roles
│   └── 01-create-base-schemas.sql      # Shared schemas
│
├── 📂 modules/                         # Module-specific scripts
│   ├── 📂 users/                       # Users Module (IMPLEMENTED)
│   │   ├── 00-create-roles.sql         # Module roles
│   │   ├── 01-create-schemas.sql       # Module schemas
│   │   └── 02-grant-permissions.sql    # Module permissions
│   │
│   ├── 📂 providers/                   # Providers Module (FUTURE)
│   │   ├── 00-create-roles.sql
│   │   ├── 01-create-schemas.sql
│   │   └── 02-grant-permissions.sql
│   │
│   └── 📂 services/                    # Services Module (FUTURE)
│       ├── 00-create-roles.sql
│       ├── 01-create-schemas.sql
│       └── 02-grant-permissions.sql
│
├── 📂 views/                          # Cross-cutting queries
│   └── cross-module-views.sql         # Controlled cross-module access
│
├── 📂 orchestrator/                   # Coordination and control
│   └── module-registry.sql            # Registry of installed modules
│
└── README.md                          # Documentation
```csharp
## 🏗️ Schema Organization

### Database Schema Structure
```sql
-- Database: meajudaai
├── users (schema)         - User management data
├── providers (schema)     - Service provider data  
├── services (schema)      - Service catalog data
├── bookings (schema)      - Appointments and reservations
├── notifications (schema) - Messaging system
└── public (schema)        - Cross-cutting views and shared data
```text
## 🔐 Database Roles

| Role | Schema | Purpose |
|------|--------|---------|
| `users_role` | `users` | User profiles, authentication data |
| `providers_role` | `providers` | Service provider information |
| `services_role` | `services` | Service catalog and pricing |
| `bookings_role` | `bookings` | Appointments and reservations |
| `notifications_role` | `notifications` | Messaging and alerts |
| `meajudaai_app_role` | `public` | Cross-module access via views |

## 🔧 Current Implementation

### Users Module (Active)
- **Schema**: `users`
- **Role**: `users_role` 
- **Search Path**: `users, public`
- **Permissions**: Full CRUD on users schema, limited access to public for EF migrations

### Connection String Configuration
```json
{
  "ConnectionStrings": {
    "Users": "Host=localhost;Database=meajudaai;Username=users_role;Password=${USERS_ROLE_PASSWORD}",
    "Providers": "Host=localhost;Database=meajudaai;Username=providers_role;Password=${PROVIDERS_ROLE_PASSWORD}",
    "DefaultConnection": "Host=localhost;Database=meajudaai;Username=meajudaai_app_role;Password=${APP_ROLE_PASSWORD}"
  }
}
```csharp
### DbContext Configuration
```csharp
public class UsersDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all entities
        modelBuilder.HasDefaultSchema("users");
        base.OnModelCreating(modelBuilder);
    }
}

// Registration with schema-specific migrations
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString, 
        o => o.MigrationsHistoryTable("__EFMigrationsHistory", "users")));
```yaml
## 🚀 Benefits of This Strategy

### Enforceable Boundaries
- Each module operates in its own security context
- Cross-module data access must be explicit (views or APIs)
- Dependencies become visible and maintainable
- Easy to spot boundary violations

### Future Microservice Extraction
- Clean boundaries make module extraction straightforward
- Database can be split along existing schema lines
- Minimal refactoring required for service separation

### Key Advantages
1. **🔒 Database-Level Isolation**: Prevents accidental cross-module access
2. **🎯 Clear Ownership**: Each module owns its schema and data
3. **📈 Independent Scaling**: Modules can be extracted to separate databases later
4. **🛡️ Security**: Role-based access control at database level
5. **🔄 Migration Safety**: Separate migration history per module

## 🚀 Adding New Modules

### Step 1: Copy Module Template
```bash
# Copy template for new module
cp -r infrastructure/database/modules/users infrastructure/database/modules/providers
```
### Step 2: Update SQL Scripts
Replace `users` with new module name in:
- `00-create-roles.sql`
- `01-create-schemas.sql` 
- `02-grant-permissions.sql`

### Step 3: Create DbContext
```csharp
public class ProvidersDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("providers");
        base.OnModelCreating(modelBuilder);
    }
}
```

### Step 4: Register in DI
```csharp
builder.Services.AddDbContext<ProvidersDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Providers"), 
        o => o.MigrationsHistoryTable("__EFMigrationsHistory", "providers")));
```
## 🔄 Migration Commands

### Generate Migrations
```bash
# Generate migration for Users module
dotnet ef migrations add AddUserProfile --context UsersDbContext --output-dir Infrastructure/Persistence/Migrations

# Generate migration for Providers module (future)
dotnet ef migrations add InitialProviders --context ProvidersDbContext --output-dir Infrastructure/Persistence/Migrations
```yaml
### Apply Migrations
```bash
# Apply all migrations for Users module
dotnet ef database update --context UsersDbContext

# Apply specific migration
dotnet ef database update AddUserProfile --context UsersDbContext
```

### Remove Migrations
```bash
# Remove last migration for Users module
dotnet ef migrations remove --context UsersDbContext
```
## 🌐 Cross-Module Access Strategies

### Option 1: Database Views (Current)
```sql
CREATE VIEW public.user_bookings_summary AS
SELECT u.id, u.email, b.booking_date, s.service_name
FROM users.users u
JOIN bookings.bookings b ON b.user_id = u.id
JOIN services.services s ON s.id = b.service_id;

GRANT SELECT ON public.user_bookings_summary TO meajudaai_app_role;
```yaml
### Option 2: Module APIs (Recommended)
```csharp
// Each module exposes a clean API
public interface IUsersModuleApi
{
    Task<UserSummaryDto?> GetUserSummaryAsync(Guid userId);
    Task<bool> UserExistsAsync(Guid userId);
}

// Implementation uses internal DbContext
public class UsersModuleApi : IUsersModuleApi
{
    private readonly UsersDbContext _context;
    
    public async Task<UserSummaryDto?> GetUserSummaryAsync(Guid userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserSummaryDto(u.Id, u.Email, u.FullName))
            .FirstOrDefaultAsync();
    }
}

// Usage in other modules
public class BookingService
{
    private readonly IUsersModuleApi _usersApi;
    
    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
    {
        // Validate user exists via API
        var userExists = await _usersApi.UserExistsAsync(request.UserId);
        if (!userExists)
            throw new UserNotFoundException();
            
        // Create booking...
    }
}
```csharp
### Option 3: Event-Driven Read Models (Future)
```csharp
// Users module publishes events
public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
}

// Other modules subscribe and build read models
public class NotificationEventHandler : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Build notification-specific read model
        await _notificationContext.UserNotificationPreferences.AddAsync(
            new UserNotificationPreference 
            { 
                UserId = notification.UserId, 
                EmailEnabled = true 
            });
    }
}
```text
## ⚡ Development Setup

### Local Development
1. **Aspire**: Automatically creates database and runs initialization scripts
2. **Docker**: PostgreSQL container with volume mounts for schema scripts
3. **Migrations**: Each module maintains separate migration history

### Production Considerations
- Use Azure PostgreSQL with separate schemas
- Consider read replicas for cross-module views
- Monitor cross-schema queries for performance
- Plan for eventual database splitting if modules need to scale independently

## ✅ Compliance Checklist

- [x] Each module has its own schema
- [x] Each module has its own database role
- [x] Role permissions restricted to module schema only
- [x] DbContext configured with default schema
- [x] Migrations history table in module schema
- [x] Connection strings use module-specific credentials
- [x] Search path set to module schema
- [x] Cross-module access controlled via views/APIs
- [ ] Additional modules follow the same pattern
- [ ] Cross-cutting views created as needed

## 🎓 References

Based on Milan Jovanović's excellent articles:
- [How to Keep Your Data Boundaries Intact in a Modular Monolith](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith)
- [Modular Monolith Data Isolation](https://www.milanjovanovic.tech/blog/modular-monolith-data-isolation)
- [Internal vs Public APIs in Modular Monoliths](https://www.milanjovanovic.tech/blog/internal-vs-public-apis-in-modular-monoliths)

---

Esta estratégia garante boundaries enforceáveis enquanto mantém a simplicidade operacional de um modular monolith.