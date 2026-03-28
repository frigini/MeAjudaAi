#!/usr/bin/env node
/**
 * Merge coverage reports from all frontend projects into a single global report.
 * Usage: node scripts/merge-coverage.mjs
 *
 * Prerequisites: Each project must have run `vitest run --coverage` first.
 * Input:  coverage-final.json from each project's ./coverage/ directory.
 * Output: Combined report in ./coverage-global/ (html + json-summary + lcov).
 */
import { existsSync, mkdirSync, readFileSync, rmSync } from 'fs';
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

const GLOBAL_THRESHOLDS = {
  lines: 70,
  functions: 70,
  branches: 70,
  statements: 70,
};

if (existsSync(OUTPUT_DIR)) rmSync(OUTPUT_DIR, { recursive: true, force: true });
mkdirSync(OUTPUT_DIR, { recursive: true });

const map = createCoverageMap();
const missing = [];

for (const project of PROJECTS) {
  const coveragePath = join(ROOT, project, 'coverage', 'coverage-final.json');
  if (existsSync(coveragePath)) {
    const coverage = JSON.parse(readFileSync(coveragePath, 'utf8'));
    const prefixedCoverage = {};
    for (const file of Object.keys(coverage)) {
      const data = coverage[file];
      const relativePath = file.split(project)[1] || file;
      const newPath = join(project, relativePath).replace(/\\/g, '/');
      data.path = newPath;
      prefixedCoverage[newPath] = data;
    }
    map.merge(prefixedCoverage);
  } else {
    missing.push(project);
  }
}

if (missing.length > 0 || map.files().length === 0) {
  process.exit(1);
}

try {
  const context = createContext({
    dir: OUTPUT_DIR,
    defaultSummarizer: 'nested',
    coverageMap: map,
  });

  reports.create('html').execute(context);
  reports.create('lcov').execute(context);
  reports.create('json-summary').execute(context);
  reports.create('cobertura').execute(context);

  const summaryPath = join(OUTPUT_DIR, 'coverage-summary.json');
  if (existsSync(summaryPath)) {
    const summary = JSON.parse(readFileSync(summaryPath, 'utf-8'));
    const totals = summary.total;
    
    const failures = [];
    if (totals.lines.pct < GLOBAL_THRESHOLDS.lines) failures.push(`lines: ${totals.lines.pct}%`);
    if (totals.functions.pct < GLOBAL_THRESHOLDS.functions) failures.push(`functions: ${totals.functions.pct}%`);
    if (totals.branches.pct < GLOBAL_THRESHOLDS.branches) failures.push(`branches: ${totals.branches.pct}%`);
    if (totals.statements.pct < GLOBAL_THRESHOLDS.statements) failures.push(`statements: ${totals.statements.pct}%`);
    
    if (failures.length > 0) {
      process.exit(1); 
    }
  } else {
    process.exit(1);
  }
} catch {
  process.exit(1);
}
