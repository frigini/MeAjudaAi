# Authentication Documentation

## Overview
This directory contains comprehensive documentation about the authentication system in MeAjudaAi.

## Contents

- [Test Authentication Handler](../testing/test_authentication_handler.md) - Documentation for testing authentication

## Authentication System

The MeAjudaAi platform uses a configurable authentication system designed to support multiple authentication providers and testing scenarios.

### Key Components

1. **Authentication Services** - Main authentication logic
2. **Test Authentication Handler** - Configurable handler for testing scenarios
3. **Authentication Middleware** - Request processing and validation

### Configuration

Authentication is configured through the application settings and can be adapted for different environments:

- **Development**: Simplified authentication for local development
- **Testing**: Configurable test authentication handler
- **Production**: Full authentication with external providers

### Testing Authentication

For testing scenarios, the platform includes a configurable authentication handler that allows:

- Custom user creation for test scenarios
- Flexible authentication outcomes
- Integration with test containers and databases

See the [Test Authentication Handler documentation](../testing/test_authentication_handler.md) for detailed usage instructions.

## Related Documentation

- [Development Guidelines](../development-guidelines.md)
- [Testing Guide](../testing/test_authentication_handler.md)