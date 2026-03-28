#!/usr/bin/env node
/**
 * Merge coverage reports from all frontend projects into a single global report.
 * Usage: node scripts/merge-coverage.mjs
 *
 * Prerequisites: Each project must have run `vitest run --coverage` first.
 * Input:  coverage-final.json from each project's ./coverage/ directory.
 * Output: Combined report in ./coverage-global/ (html + json-summary + lcov).
 */
import { existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'fs';
import { resolve, join, dirname } from 'path';
import { fileURLToPath } from 'url';
import libCoverage from 'istanbul-lib-coverage';
import libReport from 'istanbul-lib-report';
import reports from 'istanbul-reports';

const { createCoverageMap } = libCoverage;
const { createContext } = libReport;

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = resolve(__dirname, '..');

const PROJECTS = [
  'MeAjudaAi.Web.Customer',
  'MeAjudaAi.Web.Admin',
  'MeAjudaAi.Web.Provider',
];

const OUTPUT_DIR = join(ROOT, 'coverage-global');

// Global coverage thresholds
const GLOBAL_THRESHOLDS = {
  lines: 70,
  functions: 70,
  branches: 70,
  statements: 70,
};

// 1. Clean previous output
console.log('🧹 Cleaning previous coverage data...');
if (existsSync(OUTPUT_DIR)) rmSync(OUTPUT_DIR, { recursive: true, force: true });
mkdirSync(OUTPUT_DIR, { recursive: true });

// 2. Aggregate coverage from each project
const map = createCoverageMap();
const missing = [];
const projectStats = {};

for (const project of PROJECTS) {
  projectStats[project] = { files: 0, lines: 0, branches: 0, functions: 0, statements: 0 };
  const coveragePath = join(ROOT, project, 'coverage', 'coverage-final.json');
  if (existsSync(coveragePath)) {
    console.log(`✅ Adicionando cobertura: ${project}`);
    const coverage = JSON.parse(readFileSync(coveragePath, 'utf8'));
    
    // Prefix each file path with the project name to distinguish between apps
    const prefixedCoverage = {};
    for (const file of Object.keys(coverage)) {
      const data = coverage[file];
      const relativePath = file.split(project)[1] || file;
      const newPath = join(project, relativePath).replace(/\\/g, '/');
      data.path = newPath;
      prefixedCoverage[newPath] = data;
      projectStats[project].files++;
    }
    
    map.merge(prefixedCoverage);
  } else {
    missing.push(project);
    console.error(`❌ ${project}: missing coverage-final.json (run tests with --coverage first)`);
  }
}

// 3. Fail if any project is missing (PR Review Requirement)
if (missing.length > 0) {
  console.error('\n❌ Global coverage failed: Missing reports for the following projects:');
  missing.forEach(p => console.error(`   - ${p}`));
  console.error('\nAll projects in the PROJECTS array must have a coverage-final.json file.');
  console.error('Ensure all tests ran with the --coverage flag before merging.');
  process.exit(1);
}

if (map.files().length === 0) {
  console.error('❌ No coverage data found. Ensure projects are listed correctly and tests ran.');
  process.exit(1);
}

// 4. Generate combined reports
try {
  console.log(`\n📊 Generating global reports for ${map.files().length} files...`);
  
  const context = createContext({
    dir: OUTPUT_DIR,
    defaultSummarizer: 'nested',
    coverageMap: map,
  });

  // Execute report generation
  reports.create('html').execute(context);
  reports.create('lcov').execute(context);
  reports.create('json-summary').execute(context);
  reports.create('text').execute(context);
  reports.create('cobertura').execute(context);

  // 5. Generate detailed per-project breakdown
  const summaryPath = join(OUTPUT_DIR, 'coverage-summary.json');
  if (existsSync(summaryPath)) {
    const summary = JSON.parse(readFileSync(summaryPath, 'utf-8'));
    const totals = summary.total;
    
    console.log('\n📈 Global Coverage Summary:');
    console.log(`   Lines:      ${totals.lines.pct}%`);
    console.log(`   Functions:  ${totals.functions.pct}%`);
    console.log(`   Branches:   ${totals.branches.pct}%`);
    console.log(`   Statements: ${totals.statements.pct}%`);
    
    // Generate detailed table with project identification
    console.log('\n');
    console.log('-------------------|---------|----------|---------|---------|-------------------');
    console.log('File               | % Stmts | % Branch | % Funcs | % Lines | Uncovered Line #s ');
    console.log('-------------------|---------|----------|---------|---------|-------------------');
    
    // Group files by project
    const projectGroups = {};
    for (const project of PROJECTS) {
      projectGroups[project] = [];
    }
    
    // Get detailed coverage for each file
    const fileDetails = [];
    for (const file of map.files()) {
      const fileData = map.fileCoverage(file);
      let projectName = 'Unknown';
      for (const project of PROJECTS) {
        if (file.includes(project + '/')) {
          projectName = project.replace('MeAjudaAi.Web.', '');
          break;
        }
      }
      
      if (fileData.s) {
        const stmtPct = fileData.s.total > 0 ? Math.round((fileData.s.covered / fileData.s.total) * 100) : 0;
        const branchPct = fileData.b && fileData.b.total > 0 ? Math.round((fileData.b.covered / fileData.b.total) * 100) : 0;
        const funcPct = fileData.f && fileData.f.total > 0 ? Math.round((fileData.f.covered / fileData.f.total) * 100) : 0;
        
        const lines = fileData.lineMap ? Object.keys(fileData.lineMap).filter(line => !fileData.statementMap[line] || !fileData.statementMap[line].covered) : [];
        const uncoveredLines = lines.slice(0, 3).join(',') + (lines.length > 3 ? '...' : '');
        
        const shortPath = file.split('/').slice(2).join('/').substring(0, 19);
        
        console.log(`${shortPath.padEnd(19)}|${projectName.padStart(8)}|    ${stmtPct.toString().padStart(3)}   |    ${branchPct.toString().padStart(3)}   |    ${funcPct.toString().padStart(3)}   | ${uncoveredLines}`);
        
        projectGroups[project.replace('MeAjudaAi.Web.', 'MeAjudaAi.Web.') || project].push({ stmtPct, branchPct, funcPct });
      }
    }
    
    console.log('-------------------|---------|----------|---------|---------|-------------------');
    
    // Print per-project summary
    console.log('\n📊 Coverage by Project:');
    for (const project of PROJECTS) {
      const shortName = project.replace('MeAjudaAi.Web.', '');
      console.log(`   ${shortName}: branches ${projectStats[project].branches}%`);
    }
    
    // Check thresholds
    const failures = [];
    if (totals.lines.pct < GLOBAL_THRESHOLDS.lines) failures.push(`lines: ${totals.lines.pct}% < ${GLOBAL_THRESHOLDS.lines}%`);
    if (totals.functions.pct < GLOBAL_THRESHOLDS.functions) failures.push(`functions: ${totals.functions.pct}% < ${GLOBAL_THRESHOLDS.functions}%`);
    if (totals.branches.pct < GLOBAL_THRESHOLDS.branches) failures.push(`branches: ${totals.branches.pct}% < ${GLOBAL_THRESHOLDS.branches}%`);
    if (totals.statements.pct < GLOBAL_THRESHOLDS.statements) failures.push(`statements: ${totals.statements.pct}% < ${GLOBAL_THRESHOLDS.statements}%`);
    
    if (failures.length > 0) {
      console.error('\n⚠️  Global coverage thresholds not met:');
      failures.forEach(f => console.error(`   - ${f}`));
      process.exit(1); 
    } else {
      console.log(`\n✅ All global coverage thresholds met (>= ${GLOBAL_THRESHOLDS.lines}%)`);
    }
  } else {
    console.error('❌ Failed to find coverage-summary.json after generation.');
    process.exit(1);
  }
} catch (error) {
  console.error('\n❌ Failed to generate global coverage report:');
  console.error(error.stack || error.message);
  process.exit(1);
}

console.log(`\n✅ Global coverage report available at: ${OUTPUT_DIR}/index.html`);
