# NuGet Package Vulnerabilities

This document tracks known security vulnerabilities in transitive NuGet package dependencies.

## Current Status

Last Updated: 2025-11-20  
.NET Version: 10.0 (Preview/RC)

## Known Vulnerabilities

### 1. Microsoft.Build.Tasks.Core & Microsoft.Build.Utilities.Core

**Package**: `Microsoft.Build.Tasks.Core`, `Microsoft.Build.Utilities.Core`  
**Current Version**: 17.14.8  
**Severity**: High  
**Advisory**: [GHSA-w3q9-fxm7-j8fq](https://github.com/advisories/GHSA-w3q9-fxm7-j8fq)  
**CVE**: CVE-2024-43485

**Description**: Denial of Service vulnerability in MSBuild's task execution.

**Impact Assessment**:
- This is a transitive dependency pulled in by build-time tools
- The vulnerability affects build-time execution, not runtime
- Projects do not execute untrusted MSBuild tasks in production
- Risk is limited to developer machines and CI/CD environments with controlled access

**Mitigation Status**: ⏳ **Pending**
- **Action**: Monitor for updated versions in .NET 10 RC/RTM releases
- **Timeline**: Expected fix in .NET 10 RTM (target: Q2 2025)
- **Workaround**: All build environments are trusted and access-controlled

**Justification for Temporary Acceptance**:
- Build-time only vulnerability
- No production runtime impact
- Controlled CI/CD and development environments
- Will be resolved automatically when .NET 10 SDK updates

---

### 2. Microsoft.IdentityModel.JsonWebTokens & System.IdentityModel.Tokens.Jwt

**Package**: `Microsoft.IdentityModel.JsonWebTokens`, `System.IdentityModel.Tokens.Jwt`  
**Current Version**: 6.8.0  
**Severity**: Moderate  
**Advisory**: [GHSA-59j7-ghrg-fj52](https://github.com/advisories/GHSA-59j7-ghrg-fj52)  
**CVE**: CVE-2024-21319

**Description**: Denial of Service vulnerability in JWT token validation.

**Impact Assessment**:
- Affects only test projects (not production code)
- Projects using this: `MeAjudaAi.Providers.Tests`, `MeAjudaAi.Shared.Tests`, `MeAjudaAi.Documents.Tests`, `MeAjudaAi.SearchProviders.Tests`, `MeAjudaAi.ServiceCatalogs.Tests`
- Test JWT tokens are locally generated and controlled
- No external JWT processing in test scenarios

**Mitigation Status**: ⏳ **Pending**
- **Action**: Upgrade to `System.IdentityModel.Tokens.Jwt >= 8.0.0` when compatible test framework version is available
- **Timeline**: Monitor for updated test infrastructure packages
- **Current Blocker**: Test authentication framework depends on older version

**Justification for Temporary Acceptance**:
- Test-only dependency
- No production impact
- Controlled test environment
- Tokens are generated locally, not from external sources

---

## Remediation Plan

### Immediate Actions
1. ✅ Document all vulnerabilities with risk assessment
2. ⏳ Add targeted NuGetAudit suppressions (see below)
3. ⏳ Remove global `disableVulnerabilityWarnings` from nuget.config

### Short-term (Next Sprint)
1. Investigate upgrading `System.IdentityModel.Tokens.Jwt` to 8.14.0 (current CPM version)
2. Review test authentication framework for compatibility with newer JWT libraries
3. Monitor .NET 10 SDK updates for MSBuild.Tasks.Core fixes

### Long-term (Before Production)
1. Ensure all vulnerabilities are resolved before .NET 10 RTM
2. Implement automated vulnerability scanning in CI/CD
3. Establish policy for vulnerability response times by severity

## NuGetAudit Suppressions

The following suppressions should be added to `Directory.Build.props` after removing the global disable:

```xml
<ItemGroup>
  <!-- High severity: MSBuild build-time DoS - awaiting .NET 10 RTM fix -->
  <NuGetAuditSuppress Include="https://github.com/advisories/GHSA-w3q9-fxm7-j8fq" />
  
  <!-- Moderate severity: JWT DoS in test-only dependencies -->
  <NuGetAuditSuppress Include="https://github.com/advisories/GHSA-59j7-ghrg-fj52" />
</ItemGroup>
```

## Monitoring

- **Weekly**: Check for package updates via `dotnet list package --outdated --include-transitive`
- **Weekly**: Re-run vulnerability scan via `dotnet list package --vulnerable --include-transitive`
- **Before each release**: Full security audit
- **Subscribe to**: GitHub Security Advisories for .NET repositories

## References

- [NuGet Audit Documentation](https://learn.microsoft.com/nuget/concepts/auditing-packages)
- [GitHub Security Advisories](https://github.com/advisories)
- [.NET Security Announcements](https://github.com/dotnet/announcements/labels/security)
