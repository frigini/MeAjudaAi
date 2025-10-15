# Configuration Files

This directory contains configuration files for various tools and services used in the MeAjudaAi project.

## Files Overview

### Security & Quality Scanning
- **`.gitleaks.toml`** - Configuration for GitLeaks secret detection
- **`.lycheeignore`** - Files/patterns to ignore in link checking
- **`lychee.toml`** - Link checker configuration for documentation

### Code Quality & Formatting
- **`.yamllint.yml`** - YAML linting rules for GitHub Actions and config files

### Test Coverage
- **`coverlet.json`** - Code coverage collection settings for unit tests

## Tool Descriptions

### GitLeaks Security Scanning
GitLeaks scans for secrets and sensitive information in the codebase:
- Prevents accidental commit of API keys, passwords, tokens
- Configured to scan development configuration files
- Excludes legitimate configuration patterns

### Lychee Link Checking
Lychee validates links in documentation files:
- Ensures documentation links are not broken
- Supports markdown files across the project
- Configured for reliable CI/CD execution

### YAML Linting
Ensures consistent formatting and quality of YAML files:
- GitHub Actions workflows
- Configuration files
- Docker Compose files

### Code Coverage
Coverlet configuration for test coverage reporting:
- Excludes generated files and third-party code
- Supports multiple output formats
- Integrates with CI/CD pipelines

## Usage

These configuration files are automatically used by their respective tools during development and CI/CD processes. No manual intervention is typically required.

## Structure Purpose

This directory consolidates configuration files that were previously scattered in the project root, making it easier to:
- Find and modify tool configurations
- Maintain consistent settings across environments
- Understand what tools are configured for the project