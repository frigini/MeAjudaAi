# 🗄️ Database Boundaries Strategy - MeAjudaAi Platform# 🗄️ Database Structure - MeAjudaAi Platform



Following [Milan Jovanović's approach](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith) for maintaining data boundaries in Modular Monoliths.## 📁 Organização Modular



## 🎯 Core Principles```

infrastructure/database/

### **Enforced Boundaries at Database Level**├── 📂 shared/                          # Scripts base da plataforma

- ✅ **One schema per module** with dedicated database role│   ├── 00-create-base-roles.sql        # Roles compartilhadas

- ✅ **Role-based permissions** restrict access to module's own schema only│   └── 01-create-base-schemas.sql      # Schemas compartilhados

- ✅ **One DbContext per module** with default schema configuration│

- ✅ **Separate connection strings** using module-specific credentials├── 📂 modules/                         # Scripts específicos por módulo

- ✅ **Cross-module access** only through explicit views or APIs│   ├── 📂 users/                       # Módulo de Usuários (IMPLEMENTADO)

│   │   ├── 00-create-roles.sql         # Roles específicas do módulo

## 📁 Structure│   │   ├── 01-create-schemas.sql       # Schemas do módulo

│   │   └── 02-grant-permissions.sql    # Permissões do módulo

```│   │

infrastructure/database/│   ├── 📂 providers/                   # Módulo de Prestadores (FUTURO)

├── 📂 setup/                          # Module setup scripts│   │   ├── 00-create-roles.sql

│   ├── users-module-setup.sql         # ✅ Users module (IMPLEMENTED)│   │   ├── 01-create-schemas.sql

│   ├── providers-module-setup.sql.template  # 🔄 Template for Providers│   │   └── 02-grant-permissions.sql

│   └── services-module-setup.sql.template   # 🔄 Template for Services│   │

││   └── 📂 services/                    # Módulo de Serviços (FUTURO)

├── 📂 views/                          # Cross-cutting queries│       ├── 00-create-roles.sql

│   └── cross-module-views.sql         # Controlled cross-module access│       ├── 01-create-schemas.sql

││       └── 02-grant-permissions.sql

└── README.md                          # This documentation│

```├── 📂 orchestrator/                    # Coordenação e controle

│   └── module-registry.sql             # Registro de módulos instalados

## 🔧 Current Implementation│

└── 📂 schemas/                         # DEPRECATED - Scripts antigos

### **Users Module (Active)**    ├── 00-create-roles-users-only.sql  # ⚠️ Manter para referência

- **Schema**: `users`    ├── 01-create-schemas-users-only.sql

- **Role**: `users_role` (password: `users_secret`)    └── 02-grant-permissions-users-only.sql

- **Search Path**: `users, public````

- **Permissions**: Full CRUD on users schema, limited access to public for EF migrations

---

### **Connection String Example**

```json# Database Boundaries Strategy (LEGACY)

{

  "ConnectionStrings": {Esta documentação descreve a estratégia de boundaries de dados implementada no MeAjudaAi, baseada nas melhores práticas de Milan Jovanovic para Modular Monoliths.

    "Users": "Host=localhost;Database=meajudaai;Username=users_role;Password=users_secret"

  }## 🎯 Estratégia Adotada

}

```### **Abordagem Híbrida:**

- **Scripts SQL Centralizados**: Para criação de schemas, roles e permissões

### **DbContext Configuration**- **Configuração nos Módulos**: DbContexts individuais com schema dedicado

```csharp- **Connection Strings Separadas**: Cada módulo usa credenciais específicas

public class UsersDbContext : DbContext

{## 🏗️ Estrutura de Schemas

    protected override void OnModelCreating(ModelBuilder modelBuilder)

    {```sql

        // Set default schema for all entities-- Database: meajudaai

        modelBuilder.HasDefaultSchema("users");├── users (schema)       - Users module data

        base.OnModelCreating(modelBuilder);├── providers (schema)   - Service providers data  

    }├── services (schema)    - Service catalog data

}├── bookings (schema)    - Appointments and reservations

├── notifications (schema) - Notification system

// Registration with schema-specific migrations└── public (schema)      - Cross-cutting views

builder.Services.AddDbContext<UsersDbContext>(options =>```

    options.UseNpgsql(connectionString, 

        o => o.MigrationsHistoryTable("__EFMigrationsHistory", "users")));## 🔐 Database Roles

```

| Role | Schema | Purpose |

## 🚀 Adding New Modules|------|--------|---------|

| `users_role` | `users` | User profiles, authentication data |

### 1. **Copy Template**| `providers_role` | `providers` | Service provider information |

```bash| `services_role` | `services` | Service catalog and pricing |

cp setup/providers-module-setup.sql.template setup/providers-module-setup.sql| `bookings_role` | `bookings` | Appointments and reservations |

```| `notifications_role` | `notifications` | Messaging and alerts |



### 2. **Uncomment and Customize**## 📂 Files Structure

- Replace `providers` with your module name

- Set appropriate password```

- Adjust permissions if neededinfrastructure/

└── database/

### 3. **Execute Script**    ├── schemas/

```bash    │   ├── 00-create-roles.sql     # Database roles creation

psql -d meajudaai -f setup/providers-module-setup.sql    │   ├── 01-create-schemas.sql   # Schemas creation

```    │   └── 02-grant-permissions.sql # Permissions setup

    └── views/

### 4. **Configure DbContext**        └── cross-module-views.sql  # Cross-cutting queries

- Create module-specific DbContext

- Set `HasDefaultSchema("[module]")`src/Modules/

- Configure migrations history table└── Users/

- Add connection string with module credentials    └── Infrastructure/

        ├── UsersDbContext.cs       # Schema: "users"

### 5. **Generate Migrations**        └── Extensions.cs           # Connection: "Users"

```bash```

dotnet ef migrations add Initial --context ProvidersDbContext --output-dir Data/Migrations/Providers

```## 🔧 Module Configuration



## 🛡️ Security Benefits### UsersDbContext Example:

```csharp

### **Enforced Isolation**protected override void OnModelCreating(ModelBuilder modelBuilder)

- Users module **cannot** query providers tables directly{

- Database-level security prevents accidental cross-module access    modelBuilder.HasDefaultSchema("users");

- Each module operates in its own security context    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

}

### **Clear Dependencies**```

- Cross-module data access must be explicit (views or APIs)

- Dependencies become visible and maintainable### Connection String Setup:

- Easy to spot boundary violations```json

{

### **Future Microservice Extraction**  "ConnectionStrings": {

- Clean boundaries make module extraction straightforward    "Users": "Host=localhost;Database=meajudaai;Username=users_role;Password=users_secret;Search Path=users"

- Database can be split along existing schema lines  }

- Minimal refactoring required for service separation}

```

## 🔍 Cross-Module Queries

## 🚀 Benefits

When you need data from multiple modules:

1. **🔒 Enforceable Boundaries**: Database-level isolation prevents accidental cross-module access

### **Option 1: Database Views (Recommended for shared database)**2. **🎯 Clear Ownership**: Each module owns its schema and data

```sql3. **📈 Independent Scaling**: Modules can be extracted to separate databases later

CREATE VIEW public.user_summary AS4. **🛡️ Security**: Role-based access control at database level

SELECT id, username, email, created_at5. **🔄 Migration Safety**: Separate migration history per module

FROM users.users

WHERE is_active = true;## 📋 Migration Commands



GRANT SELECT ON public.user_summary TO providers_role;```bash

```# Generate migration for Users module

dotnet ef migrations add InitialUsers --context UsersDbContext --output-dir Persistence/Migrations

### **Option 2: Module APIs (Recommended for future microservices)**

```csharp# Apply migrations for specific module

// Providers module queries Users module via APIdotnet ef database update --context UsersDbContext

var userInfo = await _usersApi.GetUserSummaryAsync(userId);```

```

## 🌐 Cross-Module Queries

### **Option 3: Event-Driven Read Models**

```csharpFor queries spanning multiple modules, use:

// Users module publishes events, other modules build read models

public class UserRegisteredEvent1. **Integration Events**: Async communication between modules

{2. **Database Views**: Read-only views in public schema with controlled access

    public Guid UserId { get; set; }3. **Dedicated APIs**: Module exposes public APIs for data access

    public string Username { get; set; }

    public string Email { get; set; }### Example Cross-Module View:

}```sql

```CREATE VIEW public.user_bookings_summary AS

SELECT u.id, u.email, b.booking_date, s.service_name

## ✅ Compliance ChecklistFROM users.users u

JOIN bookings.bookings b ON b.user_id = u.id

- [x] Each module has its own schemaJOIN services.services s ON s.id = b.service_id;

- [x] Each module has its own database role

- [x] Role permissions restricted to module schema onlyGRANT SELECT ON public.user_bookings_summary TO meajudaai_app_role;

- [x] DbContext configured with default schema```

- [x] Migrations history table in module schema

- [x] Connection strings use module-specific credentials## ⚡ Local Development Setup

- [x] Search path set to module schema

- [x] Cross-module access controlled via views/APIs1. **Aspire**: Automatically creates database and runs initialization scripts

- [ ] Additional modules follow the same pattern2. **Docker**: PostgreSQL container with volume mounts for schema scripts

- [ ] Cross-cutting views created as needed3. **Migrations**: Each module maintains separate migration history



## 🎓 References## 🎪 Production Considerations



Based on Milan Jovanović's excellent article:- Use Azure PostgreSQL with separate schemas

- [How to Keep Your Data Boundaries Intact in a Modular Monolith](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith)- Consider read replicas for cross-module views

- Monitor cross-schema queries for performance

Additional resources:- Plan for eventual database splitting if modules need to scale independently

- [Modular Monolith Data Isolation](https://www.milanjovanovic.tech/blog/modular-monolith-data-isolation)

- [Internal vs Public APIs in Modular Monoliths](https://www.milanjovanovic.tech/blog/internal-vs-public-apis-in-modular-monoliths)---

Esta estratégia garante boundaries enforceáveis enquanto mantém a simplicidade operacional de um modular monolith.