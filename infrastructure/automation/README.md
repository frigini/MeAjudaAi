# Automation Scripts

This directory contains automation scripts for CI/CD setup and deployment workflows.

## Files Overview

### CI/CD Setup Scripts
- **`setup-ci-only.ps1`** - Configure CI (Continuous Integration) only
- **`setup-cicd.ps1`** - Configure full CI/CD (Continuous Integration/Deployment)

## Script Descriptions

### CI-Only Setup (`setup-ci-only.ps1`)
Configures basic continuous integration:
- Sets up GitHub Actions workflows
- Configures automated testing
- Establishes code quality checks
- Does not include deployment automation

**Use Case**: Development environments where you want automated testing and validation but manual deployment control.

### Full CI/CD Setup (`setup-cicd.ps1`)
Configures complete continuous integration and deployment:
- Everything from CI-only setup
- Automated deployment to production
- Infrastructure provisioning
- Release management workflows

**Use Case**: Production environments with automated deployment pipelines.

## Usage

### Prerequisites
- PowerShell 5.1 or later
- Azure CLI (if using Azure deployment)
- Appropriate permissions for the target environment

### Running Scripts

```powershell
# For CI-only setup
.\automation\setup-ci-only.ps1

# For full CI/CD setup
.\automation\setup-cicd.ps1
```

### Parameters
Both scripts support various parameters for customization. Use the `-Help` parameter to see available options:

```powershell
# View help for CI-only setup
.\automation\setup-ci-only.ps1 -Help

# View help for full CI/CD setup
.\automation\setup-cicd.ps1 -Help
```

## Best Practices

1. **Test in Development First**: Always test automation scripts in development environments before applying to production
2. **Review Generated Configurations**: Inspect the generated workflow files before committing
3. **Backup Existing Configurations**: Keep backups of existing CI/CD configurations before running setup scripts
4. **Environment-Specific Settings**: Ensure environment-specific settings are properly configured

## Structure Purpose

This directory consolidates automation setup scripts, making it clear what automation tools are available and how to configure them for different environments.