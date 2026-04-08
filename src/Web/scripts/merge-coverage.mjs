#!/usr/bin/env node
/**
 * Merge coverage reports from all frontend projects into a single global report.
 * Usage: node scripts/merge-coverage.mjs
 *
 * Prerequisites: Each project must have run `vitest run --coverage` first.
 * Input:  coverage-final.json from each project's ./coverage/ directory.
 * Output: Combined report in ./coverage-global/ (html + json-summary + lcov).
 */
import { existsSync, mkdirSync, readFileSync, writeFileSync, rmSync } from 'fs';
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
    console.warn(`[WARN] Coverage file not found for ${project}. Skipping...`);
  }
}

if (map.files().length === 0) {
  console.error('[ERROR] No coverage files found to merge.');
  process.exit(1);
}

try {
  const context = createContext({
    dir: OUTPUT_DIR,
    defaultSummarizer: 'nested',
    coverageMap: map,
  });

  reports.create('json-summary').execute(context);
  reports.create('cobertura').execute(context);

  const coberturaPath = join(OUTPUT_DIR, 'cobertura-coverage.xml');
  if (existsSync(coberturaPath)) {
    let xml = readFileSync(coberturaPath, 'utf8');
    let packages = xml.split('<package ');
    for (let i = 1; i < packages.length; i++) {
      const match = packages[i].match(/filename="([^/\\"]+)[/\\]/);
      if (match) {
        const appName = match[1].replace('MeAjudaAi.Web.', '');
        packages[i] = packages[i].replace(/name="([^"]+)"/, `name="${appName}/$1"`);
      }
    }
    writeFileSync(coberturaPath, packages.join('<package '), 'utf8');
  }

  const summaryPath = join(OUTPUT_DIR, 'coverage-summary.json');
  if (existsSync(summaryPath)) {
    const summary = JSON.parse(readFileSync(summaryPath, 'utf-8'));
    const totals = summary.total;
    
    // -------------------------------------------------------------
    // Custom Markdown Report Generation
    // -------------------------------------------------------------
    const projectsData = {};
    for (const [file, stats] of Object.entries(summary)) {
      if (file === 'total') continue;
      
      // Group by project
      const parts = file.split(/[/\\]/);
      const projName = parts[0]; 
      
      const dirName = parts.slice(0, -1).join('/');
      
      if (!projectsData[projName]) {
        projectsData[projName] = { packages: {}, totals: { lines: { c: 0, t: 0 }, branches: { c: 0, t: 0 } } };
      }
      
      if (!projectsData[projName].packages[dirName]) {
        projectsData[projName].packages[dirName] = { lines: { c: 0, t: 0 }, branches: { c: 0, t: 0 } };
      }
      
      const pGroup = projectsData[projName].packages[dirName];
      pGroup.lines.t += stats.lines.total;
      pGroup.lines.c += stats.lines.covered;
      pGroup.branches.t += stats.branches.total;
      pGroup.branches.c += stats.branches.covered;
      
      projectsData[projName].totals.lines.t += stats.lines.total;
      projectsData[projName].totals.lines.c += stats.lines.covered;
      projectsData[projName].totals.branches.t += stats.branches.total;
      projectsData[projName].totals.branches.c += stats.branches.covered;
    }
    
    const fmt = (c, t) => t === 0 ? '100%' : `${Math.floor((c/t)*100)}%`;
    const getHealth = (pct) => pct >= 80 ? '✔' : pct >= 60 ? '➖' : '❌';
    
    // Calculate global line percentage for the badge
    const gLTotal = totals.lines.total;
    const gLCov = totals.lines.covered;
    const gLRateRaw = gLTotal === 0 ? 100 : Math.floor((gLCov / gLTotal) * 100);
    const badgeColor = gLRateRaw >= 80 ? 'brightgreen' : gLRateRaw >= 60 ? 'yellow' : 'red';
    const badgeUrl = `https://img.shields.io/badge/Code%20Coverage-${gLRateRaw}%25-${badgeColor}?style=flat`;
    
    let md = `### Code Coverage Report\n\n`;
    md += `![Code Coverage](${badgeUrl})\n\n`;
    md += `| Project | Package | Line Rate | Branch Rate | Health |\n`;
    md += `|---|---|---|---|---|\n`;
    
    for (const [proj, data] of Object.entries(projectsData)) {
      const pkgs = Object.keys(data.packages).sort();
      for (const pkg of pkgs) {
        const stats = data.packages[pkg];
        if (stats.lines.t === 0) continue;
        
        const linePctRaw = (stats.lines.c / stats.lines.t) * 100;
        const linePct = fmt(stats.lines.c, stats.lines.t);
        const branchPct = fmt(stats.branches.c, stats.branches.t);
        const health = getHealth(linePctRaw);
        
        let displayPkg = pkg.startsWith(`${proj}/`) ? pkg.replace(`${proj}/`, '') : (pkg === proj ? 'root' : pkg);
        displayPkg = displayPkg
          .replace('app/(admin)/', '')
          .replace('app/(customer)/', '')
          .replace('app/(provider)/', '')
          .replace('app/', '')
          .replace('src/', '')
          .replace('libs/', 'lib/');
          
        md += `| ${proj} | ${displayPkg} | ${linePct} | ${branchPct} | ${health} |\n`;
      }
      
      const lTotal = data.totals.lines.t;
      const lCov = data.totals.lines.c;
      const bTotal = data.totals.branches.t;
      const bCov = data.totals.branches.c;
      const pLinePct = fmt(lCov, lTotal);
      const pBranchPct = fmt(bCov, bTotal);
      
      md += `| **${proj}** | **Summary** | **${pLinePct} (${lCov} / ${lTotal})** | **${pBranchPct} (${bCov} / ${bTotal})** | - |\n`;
    }
    
    const gBTotal = totals.branches.total;
    const gBCov = totals.branches.covered;
    
    md += `| **Overall** | **Summary** | **${fmt(gLCov, gLTotal)} (${gLCov} / ${gLTotal})** | **${fmt(gBCov, gBTotal)} (${gBCov} / ${gBTotal})** | - |\n`;
    
    writeFileSync(join(OUTPUT_DIR, 'custom-coverage-results.md'), md, 'utf-8');
    console.log('[INFO] Custom Markdown report generated successfully at coverage-global/custom-coverage-results.md');

    // -------------------------------------------------------------
    
    const failures = [];
    if (totals.lines.pct < GLOBAL_THRESHOLDS.lines) failures.push(`lines: ${totals.lines.pct}%`);
    if (totals.functions.pct < GLOBAL_THRESHOLDS.functions) failures.push(`functions: ${totals.functions.pct}%`);
    if (totals.branches.pct < GLOBAL_THRESHOLDS.branches) failures.push(`branches: ${totals.branches.pct}%`);
    if (totals.statements.pct < GLOBAL_THRESHOLDS.statements) failures.push(`statements: ${totals.statements.pct}%`);
    
    if (failures.length > 0) {
      console.error(`[ERROR] Threshold failures: \n${failures.join('\n')}`);
      process.exit(1); 
    }
  } else {
    console.error(`[ERROR] coverage-summary.json missing at ${summaryPath}`);
    process.exit(1);
  }
} catch (err) {
  console.error('[ERROR] Unhandled exception during coverage merge:', err);
  process.exit(1);
}
