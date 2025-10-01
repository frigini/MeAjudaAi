# Database Documentation

Esta pasta contém toda a documentação relacionada ao banco de dados do projeto MeAjudaAi.

## 📚 Índice de Documentação

### 🗂️ **Organização de Scripts**
- [`scripts_organization.md`](./scripts_organization.md) - Como organizar e criar scripts de banco para novos módulos

### 🔒 **Isolamento de Schema**
- [`schema_isolation.md`](./schema_isolation.md) - Implementação de isolamento de schema por módulo

### 🔧 **Arquivos Relacionados**
- [`../technical/database_boundaries.md`](../technical/database_boundaries.md) - Boundaries e limites entre módulos
- [`../infrastructure.md`](../infrastructure.md) - Visão geral da infraestrutura

## 🎯 **Scripts de Banco**

Os scripts SQL estão localizados em:
```
infrastructure/database/
├── modules/
│   └── users/
│       ├── 00-roles.sql
│       └── 01-permissions.sql
├── views/
│   └── cross-module-views.sql
└── create-module.ps1
```

## 📝 **Convenções**

- **Nomenclatura**: `kebab-case.md` (exceto `README.md`)
- **Localização**: Documentação específica em `docs/database/`
- **Scripts**: Organizados por módulo em `infrastructure/database/modules/`