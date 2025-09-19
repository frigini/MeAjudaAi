# Authentication and Authorization

This documentation covers the authentication and authorization system used in MeAjudaAi, including Keycloak integration and JWT token handling.

## Overview

The MeAjudaAi platform uses a dual authentication approach:
- **Production**: Keycloak-based authentication with JWT tokens
- **Development/Testing**: TestAuthenticationHandler for simplified development

## Table of Contents

1. [Keycloak Setup](#keycloak-setup)
2. [JWT Token Configuration](#jwt-token-configuration)
3. [Testing Authentication](#testing-authentication)
4. [Production Deployment](#production-deployment)
5. [Troubleshooting](#troubleshooting)

## Keycloak Setup

### Local Development

For local development, Keycloak is automatically configured using Docker Compose:

```bash
docker-compose -f infrastructure/docker-compose.keycloak.yml up -d
```

### Configuration

The Keycloak realm configuration is located at:
- `infrastructure/keycloak/realms/meajudaai-realm.json`

Key configuration includes:
- **Realm**: `meajudaai`
- **Client ID**: `meajudaai-client`
- **Allowed redirect URIs**: `http://localhost:*`
- **Token settings**: Access token lifespan, refresh token settings

### Users and Roles

Default test users are configured in the realm:
- **Admin User**: `admin@meajudaai.com` / `admin123`
- **Regular User**: `user@meajudaai.com` / `user123`

## JWT Token Configuration

### Token Validation

JWT tokens are validated using the following configuration in `appsettings.json`:

```json
{
  "Authentication": {
    "Keycloak": {
      "Authority": "http://localhost:8080/realms/meajudaai",
      "Audience": "account",
      "MetadataAddress": "http://localhost:8080/realms/meajudaai/.well-known/openid_configuration",
      "RequireHttpsMetadata": false
    }
  }
}
```

### Claims Mapping

The system maps Keycloak claims to application claims:
- `sub` → User ID
- `email` → Email address
- `preferred_username` → Username
- `realm_access.roles` → User roles

### Token Refresh

Refresh tokens are automatically handled by the frontend application. The backend validates both access and refresh tokens.

## Testing Authentication

For development and testing purposes, the system includes a `TestAuthenticationHandler` that bypasses Keycloak authentication.

See the complete testing documentation:
- [Test Authentication Handler](../testing/test-authentication-handler.md)
- [Test Configuration](../testing/test-auth-configuration.md)
- [Test Examples](../testing/test-auth-examples.md)

## Production Deployment

### Environment Configuration

In production, ensure the following environment variables are set:

```bash
Authentication__Keycloak__Authority=https://your-keycloak-domain/realms/meajudaai
Authentication__Keycloak__RequireHttpsMetadata=true
Authentication__Keycloak__Audience=account
```

### Security Considerations

1. **HTTPS Required**: Always use HTTPS in production
2. **Token Validation**: Ensure proper token signature validation
3. **Audience Validation**: Validate the token audience claim
4. **Issuer Validation**: Validate the token issuer claim

### SSL/TLS Configuration

For production deployments, configure SSL certificates:
- Use valid SSL certificates for Keycloak
- Configure proper trust store if using custom certificates
- Ensure certificate chain validation

## Troubleshooting

### Common Issues

1. **Token Validation Errors**
   - Check authority URL configuration
   - Verify metadata endpoint accessibility
   - Ensure proper audience configuration

2. **CORS Issues**
   - Configure allowed origins in Keycloak client
   - Set proper CORS headers in application

3. **Certificate Issues**
   - Verify SSL certificate validity
   - Check certificate trust chain
   - Configure proper certificate validation

### Debug Logging

Enable authentication debug logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug"
    }
  }
}
```

### Health Checks

The application includes authentication health checks:
- Keycloak connectivity
- Token validation endpoint
- Metadata endpoint accessibility

## API Documentation

The Swagger UI includes authentication support:
1. Click "Authorize" button
2. Enter JWT token in format: `Bearer <token>`
3. Test authenticated endpoints

For obtaining tokens during development, see the [testing documentation](../testing/test-auth-examples.md).