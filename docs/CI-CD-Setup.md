# MeAjudaAi CI/CD Pipeline Setup

This document explains how to set up and use the CI/CD pipeline for the MeAjudaAi project.

## Overview

The project uses a **CI-only pipeline** initially to avoid cloud costs while maintaining code quality and build validation. The pipeline integrates perfectly with your .NET Aspire setup.

## Current Pipeline Features

### ✅ What's Included (CI-Only)
- **Build Validation**: Builds the entire .NET solution
- **Unit Testing**: Runs all unit tests automatically
- **Aspire Validation**: Validates your Aspire AppHost configuration
- **Code Quality**: Checks code formatting and runs security analysis
- **Container Readiness**: Validates services can be containerized
- **Cross-platform**: Runs on Ubuntu (GitHub Actions)

### 💰 What's NOT Included (No Costs)
- No cloud deployment
- No infrastructure provisioning
- No runtime costs

## Quick Setup

### 1. Run the Setup Script
```powershell
.\setup-ci-only.ps1
```

This script will:
- ✅ Verify your development environment
- ✅ Test local build
- ✅ Install Aspire workload if needed
- ✅ Validate GitHub Actions workflow

### 2. Push to GitHub
```bash
git add .
git commit -m "Add CI pipeline"
git push
```

### 3. View Results
- Go to your GitHub repository
- Click the **Actions** tab
- Watch your pipeline run! 🚀

## Pipeline Jobs

### 1. Build and Test
- Restores NuGet packages
- Builds the solution in Release mode
- Runs unit tests
- Uploads test results as artifacts

### 2. Aspire Validation
- Validates Aspire AppHost builds correctly
- Generates deployment manifest (for future use)
- Ensures Aspire configuration is valid

### 3. Code Analysis
- Checks code formatting with `dotnet format`
- Runs security analysis
- Validates C#, Docker, JSON, and YAML files

### 4. Service Build Validation
- Tests each service can be published
- Simulates container build process
- Validates deployment readiness

## Integration with Your Aspire Setup

### How It Works
Your current Aspire setup includes:
- **AppHost**: `MeAjudaAi.AppHost` - orchestrates services
- **ServiceDefaults**: Shared configuration for health checks, telemetry
- **ApiService**: Main API gateway
- **Modules**: Users module with clean architecture

The CI pipeline:
1. **Builds** your entire solution including Aspire projects
2. **Validates** that Aspire AppHost configuration is correct
3. **Tests** that services integrate properly
4. **Prepares** for future deployment without actually deploying

### Aspire Benefits in CI
- **Service Discovery**: Validates service references work correctly
- **Health Checks**: Ensures health endpoints are properly configured
- **Telemetry**: Verifies OpenTelemetry setup
- **Configuration**: Tests that all services can start with proper config

## When You're Ready to Deploy

### Option 1: Azure (Original Plan)
```powershell
# Run the full Azure setup
.\setup-cicd.ps1 -SubscriptionId "your-subscription-id"

# Then enable deployment in the workflow
# Edit .github/workflows/aspire-ci-cd.yml and change:
# if: github.ref == 'refs/heads/develop' && false
# to:
# if: github.ref == 'refs/heads/develop' && true
```

### Option 2: Alternative Cloud Providers
Consider these **free/cheaper** alternatives:
- **Railway.app**: $5/month credit (often free for small apps)
- **Render.com**: Free tier + $7/month services
- **Fly.io**: Generous free tier (3 VMs, 160GB bandwidth)
- **Docker Compose**: Deploy to any VPS

## Troubleshooting

### Build Failures
```bash
# Test locally first
dotnet restore MeAjudaAi.sln
dotnet build MeAjudaAi.sln --configuration Release

# Check Aspire specifically
cd src/Aspire/MeAjudaAi.AppHost
dotnet build --configuration Release
```

### Code Formatting Issues
```bash
# Fix formatting locally
dotnet format MeAjudaAi.sln
```

### Aspire Workload Issues
```bash
# Reinstall Aspire workload
dotnet workload install aspire --force
```

## Monitoring

### GitHub Actions
- **Actions tab**: View all pipeline runs
- **Badges**: Add build status to README
- **Notifications**: GitHub will email on failures

### Using GitHub CLI (Optional)
```bash
# Install GitHub CLI first: https://cli.github.com/

# View recent runs
gh run list

# Watch current run
gh run watch

# View logs
gh run view --log
```

## Cost Implications

### Current Setup (FREE)
- ✅ GitHub Actions: 2,000 minutes/month free
- ✅ No cloud resources created
- ✅ No deployment costs

### When Adding Deployment
- **Azure**: ~$10-50/month depending on usage
- **Railway**: Often free with $5 credit
- **Render**: $7/month per service
- **Fly.io**: Often free for development

## Best Practices

### Development Workflow
1. **Feature branches**: Create branches for new features
2. **Pull requests**: CI runs automatically on PRs
3. **Code review**: Review both code and CI results
4. **Merge**: Only merge when CI passes

### Aspire-Specific Tips
- **Local testing**: Always run Aspire locally first
- **Health checks**: Ensure all services have health endpoints
- **Service references**: Test service-to-service communication
- **Configuration**: Use Aspire's configuration patterns

## Future Enhancements

### Planned Additions
- **Integration Tests**: Test service interactions
- **Performance Tests**: Load testing with NBomber
- **Container Registry**: Push to GitHub Container Registry
- **Staging Environment**: Deploy to staging first
- **Database Migrations**: Automated EF migrations

### Advanced Aspire Features
- **Distributed Tracing**: Full OpenTelemetry integration
- **Metrics**: Application Insights integration
- **Service Mesh**: Advanced networking features
- **Multi-environment**: Development, staging, production

## Getting Help

### Resources
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [MeAjudaAi Project Wiki](../README.md)

### Common Issues
1. **Aspire workload missing**: Run `dotnet workload install aspire`
2. **Build failures**: Check local build first
3. **Test failures**: Run tests locally to debug
4. **Permission issues**: Check repository settings

---

🎉 **Your CI pipeline is ready!** Push code and watch it build automatically.
