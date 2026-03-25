#!/usr/bin/env node
/**
 * Merge coverage reports from all frontend projects into a single global report.
 * Usage: node scripts/merge-coverage.mjs
 *
 * Prerequisites: Each project must have run `vitest run --coverage` first.
 * Input:  coverage-final.json from each project's ./coverage/ directory.
 * Output: Combined report in ./coverage-global/ (html + json-summary + lcov).
 */
import { execSync } from 'child_process';
import { existsSync, mkdirSync, copyFileSync } from 'fs';
import { resolve, join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = resolve(__dirname, '..');

const PROJECTS = [
  'MeAjudaAi.Web.Customer',
  'MeAjudaAi.Web.Admin',
  'MeAjudaAi.Web.Provider',
];

const TEMP_DIR = join(ROOT, '.nyc_output');
const OUTPUT_DIR = join(ROOT, 'coverage-global');

// 1. Clean previous output
if (existsSync(TEMP_DIR)) execSync(`npx rimraf "${TEMP_DIR}"`);
if (existsSync(OUTPUT_DIR)) execSync(`npx rimraf "${OUTPUT_DIR}"`);
mkdirSync(TEMP_DIR, { recursive: true });
mkdirSync(OUTPUT_DIR, { recursive: true });

// 2. Copy coverage-final.json from each project into .nyc_output/
let found = 0;
for (const project of PROJECTS) {
  const coverageFile = join(ROOT, project, 'coverage', 'coverage-final.json');
  if (existsSync(coverageFile)) {
    copyFileSync(coverageFile, join(TEMP_DIR, `${project}.json`));
    console.log(`✅ ${project}: coverage found`);
    found++;
  } else {
    console.warn(`⚠️  ${project}: no coverage-final.json (run tests with --coverage first)`);
  }
}

if (found === 0) {
  console.error('❌ No coverage files found. Run: npm run test:coverage:all');
  process.exit(1);
}

// 3. Merge and generate combined report
console.log(`\n📊 Merging ${found}/${PROJECTS.length} project(s)...`);
execSync(
  `npx nyc merge "${TEMP_DIR}" "${join(OUTPUT_DIR, 'merged.json')}"`,
  { stdio: 'inherit' }
);
execSync(
  `npx nyc report --temp-dir "${OUTPUT_DIR}" --reporter=html --reporter=json-summary --reporter=lcov --report-dir "${OUTPUT_DIR}"`,
  { stdio: 'inherit' }
);

console.log(`\n✅ Global coverage report: ${OUTPUT_DIR}/index.html`);
