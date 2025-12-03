# Validate Coverage Action

A reusable GitHub Action that validates code coverage against a minimum threshold with intelligent multi-stage fallback analysis.

## Features

- **Multi-Stage Fallback**: Tries multiple methods to extract coverage data
  1. Step outputs from CodeCoverageSummary actions (OpenCover → Cobertura → Fallback)
  2. Direct XML file analysis when step outputs unavailable
  3. Regex-based extraction from line-rate/sequenceCoverage attributes
  
- **Flexible Configuration**: 
  - Customizable coverage directory
  - Adjustable threshold percentage
  - Strict/lenient mode for CI/CD flexibility
  
- **Comprehensive Debugging**:
  - Detailed logs showing data sources
  - Coverage file discovery information
  - Clear validation results and recommendations

## Usage

### Basic Example

```yaml
- name: Validate Coverage
  uses: ./.github/actions/validate-coverage
  with:
    coverage-directory: './coverage'
    threshold: '70'
    strict-mode: 'true'
```

### With CodeCoverageSummary Integration

```yaml
- name: Generate Coverage Summary (OpenCover)
  id: coverage_opencover
  uses: irongut/CodeCoverageSummary@v1.3.0
  continue-on-error: true
  with:
    filename: coverage/**/*.opencover.xml
    format: markdown

- name: Validate Coverage
  uses: ./.github/actions/validate-coverage
  with:
    threshold: '70'
    strict-mode: 'false'
    opencover-output: ${{ steps.coverage_opencover.outputs.line-rate }}
    opencover-outcome: ${{ steps.coverage_opencover.outcome }}
```

### Multiple Format Fallback

```yaml
- name: Generate Coverage Summary (OpenCover)
  id: coverage_opencover
  uses: irongut/CodeCoverageSummary@v1.3.0
  continue-on-error: true
  with:
    filename: coverage/**/*.opencover.xml

- name: Generate Coverage Summary (Cobertura)
  id: coverage_cobertura
  if: steps.coverage_opencover.outcome != 'success'
  uses: irongut/CodeCoverageSummary@v1.3.0
  continue-on-error: true
  with:
    filename: coverage/**/*.cobertura.xml

- name: Validate Coverage
  uses: ./.github/actions/validate-coverage
  with:
    threshold: '70'
    strict-mode: ${{ github.event_name == 'push' }}
    opencover-output: ${{ steps.coverage_opencover.outputs.line-rate }}
    opencover-outcome: ${{ steps.coverage_opencover.outcome }}
    cobertura-output: ${{ steps.coverage_cobertura.outputs.line-rate }}
    cobertura-outcome: ${{ steps.coverage_cobertura.outcome }}
```

## Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `coverage-directory` | Directory containing coverage XML files | No | `./coverage` |
| `threshold` | Minimum coverage percentage required (0-100) | No | `70` |
| `strict-mode` | Whether to fail workflow when coverage is below threshold | No | `true` |
| `opencover-output` | Line coverage from OpenCover CodeCoverageSummary step | No | `''` |
| `opencover-outcome` | Outcome from OpenCover step (success/failure/skipped) | No | `''` |
| `cobertura-output` | Line coverage from Cobertura CodeCoverageSummary step | No | `''` |
| `cobertura-outcome` | Outcome from Cobertura step (success/failure/skipped) | No | `''` |
| `fallback-output` | Line coverage from fallback CodeCoverageSummary step | No | `''` |
| `fallback-outcome` | Outcome from fallback step (success/failure/skipped) | No | `''` |

## Outputs

| Output | Description |
|--------|-------------|
| `coverage-percentage` | Final coverage percentage determined by the action |
| `coverage-source` | Source of the coverage data (OpenCover, Cobertura, Direct Analysis, etc.) |
| `threshold-met` | Whether the coverage threshold was met (true/false) |

## Coverage Data Sources

The action tries to extract coverage data in the following order:

1. **OpenCover Step Output**: From `irongut/CodeCoverageSummary` analyzing `*.opencover.xml`
2. **Cobertura Step Output**: From `irongut/CodeCoverageSummary` analyzing `*.cobertura.xml`
3. **Fallback Step Output**: From `irongut/CodeCoverageSummary` analyzing any `*.xml`
4. **Direct OpenCover Analysis**: Regex extraction from `*.opencover.xml` files
5. **Direct Cobertura Analysis**: Regex extraction from `*.cobertura.xml` files
6. **Direct XML Analysis**: Regex extraction from any `*.xml` files

## Strict vs Lenient Mode

### Strict Mode (`strict-mode: 'true'`)
- Fails the workflow if coverage is below threshold
- Fails if coverage data cannot be extracted
- Recommended for: main branch, release workflows, production deployments

### Lenient Mode (`strict-mode: 'false'`)
- Logs warnings but continues workflow execution
- Useful during development when coverage is being improved
- Recommended for: feature branches, draft PRs, development workflows

## Examples

### Development Workflow (Lenient)

```yaml
name: Development CI

on:
  pull_request:
    types: [opened, synchronize]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6
      
      - name: Run Tests
        run: dotnet test --collect:"XPlat Code Coverage"
      
      - name: Validate Coverage
        uses: ./.github/actions/validate-coverage
        with:
          threshold: '70'
          strict-mode: 'false'  # Warn but don't fail
```

### Release Workflow (Strict)

```yaml
name: Release

on:
  push:
    branches: [main]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6
      
      - name: Run Tests
        run: dotnet test --collect:"XPlat Code Coverage"
      
      - name: Validate Coverage
        uses: ./.github/actions/validate-coverage
        with:
          threshold: '80'
          strict-mode: 'true'  # Fail if coverage insufficient
```

### Nightly Build with Dynamic Threshold

```yaml
name: Nightly Coverage Check

on:
  schedule:
    - cron: '0 2 * * *'

jobs:
  coverage:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6
      
      - name: Run Tests
        run: dotnet test --collect:"XPlat Code Coverage"
      
      - name: Validate Coverage
        uses: ./.github/actions/validate-coverage
        with:
          threshold: ${{ vars.NIGHTLY_COVERAGE_THRESHOLD || '75' }}
          strict-mode: 'true'
```

## Troubleshooting

### Coverage file not found
**Problem**: Action logs show "Coverage directory not found" or "No coverage files found"

**Solutions**:
1. Verify coverage files are generated: `find . -name "*.xml" -type f`
2. Check `coverage-directory` input matches actual location
3. Ensure test command includes coverage collection:
   ```yaml
   dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
   ```

### Percentage extraction failed
**Problem**: "Coverage files found but percentage extraction failed"

**Solutions**:
1. Verify XML format is OpenCover or Cobertura
2. Check for `line-rate` or `sequenceCoverage` attributes in XML
3. Use CodeCoverageSummary step outputs instead of direct analysis

### Threshold not met
**Problem**: Coverage below required threshold

**Solutions**:
1. Review detailed coverage report from CodeCoverageSummary
2. Temporarily set `strict-mode: 'false'` to allow development
3. Lower threshold during coverage improvement phase
4. Add more unit tests to increase coverage

## Contributing

When modifying this action:

1. **Test locally** using `act` or similar tools
2. **Update README** if inputs/outputs change
3. **Maintain backward compatibility** when possible
4. **Add comments** explaining complex logic
5. **Test all fallback paths** to ensure robustness

## Design Decisions

### Why Multi-Stage Fallback?
Different CI environments and coverage tools produce varying XML formats. The fallback strategy ensures reliability across:
- Different .NET versions (coverlet output format changes)
- Different coverage collectors (OpenCover, Cobertura, etc.)
- CI environment differences (GitHub Actions, Azure DevOps, etc.)

### Why Composite Action?
Using `composite` instead of Docker or JavaScript allows:
- Faster execution (no container build/pull)
- Better compatibility across runners (Linux, macOS, Windows)
- Easier debugging (shell scripts visible in logs)
- No additional dependencies

### Why Bash Instead of PowerShell?
While the main project uses PowerShell, bash provides:
- Better portability across GitHub-hosted runners
- Simpler string manipulation for XML parsing
- Consistent behavior between Ubuntu/macOS runners
- Fallback compatibility with Windows Git Bash

## License

Same as parent repository (see root LICENSE file).
