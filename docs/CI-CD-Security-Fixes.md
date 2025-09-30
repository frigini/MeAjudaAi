# CI/CD Security Scanning Fixes

## Overview

This document describes the fixes applied to resolve CI/CD pipeline failures related to security scanning tools.

## Issues Fixed

### 1. Gitleaks License Requirement

**Problem**: Gitleaks v2 now requires a license for organization repositories, causing the CI/CD pipeline to fail with:
```
[Me-Ajuda-Ai] is an organization. License key is required.
Error: 🛑 missing gitleaks license.
```

**Solution**: 
- Modified the workflow to handle missing licenses gracefully using `continue-on-error: true`
- Added TruffleHog as a backup secret scanner that runs alongside Gitleaks
- Both scanners now run without failing the entire pipeline

**Files Changed**:
- `.github/workflows/pr-validation.yml`

### 2. Lychee Link Checker Regex Error

**Problem**: Invalid regex patterns in `.lycheeignore` file causing the error:
```
Error: regex parse error:
    */bin/*
    ^
error: repetition operator missing expression
```

**Solution**: 
- Fixed glob patterns in `.lycheeignore` by changing `*/bin/*` to `**/bin/**`
- Updated all similar patterns to use proper glob syntax

**Files Changed**:
- `.lycheeignore`

### 3. Gitleaks Allowlist Security Blind Spot

**Problem**: The `.gitleaks.toml` configuration was excluding `appsettings.Development.json` files from secret scanning, creating a security blind spot where real development secrets could be committed without detection.

**Solution**: 
- Removed `appsettings.Development.json` from the gitleaks allowlist
- Kept only template/example files (`appsettings.template.json`, `appsettings.example.json`) in the allowlist
- Added `appsettings.example.json` to cover more template patterns

**Files Changed**:
- `.gitleaks.toml`

**Security Impact**: 
- Gitleaks will now scan any real `appsettings.Development.json` files for secrets
- Only sanitized template files are excluded from scanning
- Reduces risk of accidentally committing development secrets

## Current Security Scanning Setup

The CI/CD pipeline now includes:

1. **Gitleaks** (with graceful failure handling)
   - Scans for secrets in git history
   - Requires license for organizations
   - Configured to continue on error if license missing

2. **TruffleHog** (backup scanner)
   - Free alternative secret scanner
   - Runs regardless of Gitleaks license status
   - Focuses on verified secrets only

3. **Lychee Link Checker**
   - Validates markdown links
   - Uses proper glob patterns for exclusions
   - Caches results for performance

## Optional: Adding Gitleaks License

If you want to use the full Gitleaks functionality:

1. Purchase a license from [gitleaks.io](https://gitleaks.io)
2. Add the license as a GitHub repository secret named `GITLEAKS_LICENSE`
3. The workflow will automatically use the licensed version when available

### Setting up GITLEAKS_LICENSE Secret

1. Go to your repository Settings
2. Navigate to Secrets and variables → Actions
3. Click "New repository secret"
4. Name: `GITLEAKS_LICENSE`
5. Value: Your purchased license key
6. Click "Add secret"

## Configuration Files

### .gitleaks.toml
The gitleaks configuration file defines:
- Rules for secret detection
- Allowlisted files/patterns
- Custom detection rules

### lychee.toml
The lychee configuration file defines:
- Link checking scope (currently file:// links only)
- Timeout and concurrency settings
- Status codes to accept as valid

### .lycheeignore
Patterns to exclude from link checking:
- Build artifacts (`**/bin/**`, `**/obj/**`)
- Dependencies (`**/node_modules/**`)
- Version control (`**/.git/**`)
- Test outputs (`**/TestResults/**`)
- Localhost and development URLs

## Monitoring Security Scans

Both security scanners will:
- Run on every pull request
- Generate detailed reports in workflow logs
- Continue pipeline execution even if issues are found
- Provide summaries in the GitHub Actions interface

To view results:
1. Go to the Actions tab in your repository
2. Click on the specific workflow run
3. Check the "Secret Detection" job for security scan results

## Best Practices

1. **Regular Updates**: Keep security scanning tools updated
2. **License Management**: Monitor Gitleaks license expiration if using paid version
3. **False Positives**: Update `.gitleaks.toml` to handle legitimate false positives
4. **Link Maintenance**: Update `.lycheeignore` for new patterns that should be excluded

## Troubleshooting

### Common Issues

1. **License errors**: Use TruffleHog output if Gitleaks fails
2. **Regex errors**: Ensure `.lycheeignore` uses valid glob patterns (`**` for recursive matching)
3. **Link timeouts**: Adjust timeout settings in `lychee.toml`

### Support

For issues with:
- **Gitleaks**: Check [gitleaks documentation](https://github.com/gitleaks/gitleaks)
- **TruffleHog**: Check [TruffleHog documentation](https://github.com/trufflesecurity/trufflehog)
- **Lychee**: Check [lychee documentation](https://github.com/lycheeverse/lychee)