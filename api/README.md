# API Specifications

This directory contains API specifications and related documentation for the MeAjudaAi project.

## Files Overview

### OpenAPI Specifications
- **`api-spec.json`** - Generated OpenAPI 3.x specification for the entire API surface

## File Descriptions

### API Specification (`api-spec.json`)
Complete OpenAPI specification containing:
- **All endpoints** across all modules (Users, Organizations, etc.)
- **Request/response schemas** with detailed examples
- **Authentication requirements** for each endpoint
- **Health check endpoints** (health, ready, live)
- **Error response formats** with proper HTTP status codes

## Generation

The API specification is automatically generated using the export script:

```bash
# Generate current API specification
./scripts/export-openapi.ps1

# Generate to custom location
./scripts/export-openapi.ps1 -OutputPath "api/my-api-spec.json"
```

## Features

### Offline Generation
- No need to run the application
- Works from compiled assemblies
- Always reflects current codebase

### Client Integration
Compatible with popular API clients:
- **APIDog** - Import for advanced testing
- **Postman** - Generate collections automatically
- **Insomnia** - REST client integration
- **Bruno** - Open-source API client
- **Thunder Client** - VS Code extension

### Development Benefits
- **Realistic examples** in request/response schemas
- **Complete type information** for all DTOs
- **Authentication schemes** clearly documented
- **Error handling patterns** standardized

## Usage Patterns

### For Frontend Development
```bash
# Generate spec for frontend team
./scripts/export-openapi.ps1 -OutputPath "api/frontend-api.json"
# Frontend team imports into their preferred client
```

### For API Testing
```bash
# Generate spec for QA testing
./scripts/export-openapi.ps1 -OutputPath "api/test-api.json"
# Import into Postman/APIDog for comprehensive testing
```

### For Documentation
```bash
# Generate spec for documentation site
./scripts/export-openapi.ps1 -OutputPath "docs/api-reference.json"
# Use with Swagger UI or similar documentation tools
```

## Version Control

API specification files are **not version controlled** (included in .gitignore) because:
- They are generated artifacts
- Always reflect current codebase state
- Avoid merge conflicts
- Regenerated on demand

## Structure Purpose

This directory provides a dedicated location for API-related artifacts, making it clear where to find and generate API specifications for different use cases.