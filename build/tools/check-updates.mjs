// Compares every installed plugin (plugins.json) with its latest GitHub release.
// Usage: node check-updates.mjs
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const { plugins } = JSON.parse(fs.readFileSync(path.join(here, 'plugins.json'), 'utf8'));
const headers = { 'User-Agent': 'cs2-server-update-check', Accept: 'application/vnd.github+json' };

const normalize = (v) => String(v).toLowerCase().replace(/^(build-|v)/, '');

async function github(url) {
  const res = await fetch(url, { headers });
  if (res.status === 403) throw new Error('GitHub rate limit reached, try again in an hour');
  return res.ok ? res.json() : null;
}

async function check(p) {
  if (!p.repo) return { status: 'own plugin', latest: '-' };

  if (p.installed.startsWith('commit:')) {
    const sha = p.installed.slice(7);
    const [release, compare] = await Promise.all([
      github(`https://api.github.com/repos/${p.repo}/releases/latest`),
      github(`https://api.github.com/repos/${p.repo}/compare/${sha}...HEAD`),
    ]);
    const ahead = compare?.ahead_by ?? '?';
    return {
      status: ahead === 0 ? 'up to date' : `${ahead} new commit(s)`,
      latest: `release ${release?.tag_name ?? '?'}, main ${compare?.commits?.at(-1)?.sha?.slice(0, 7) ?? sha.slice(0, 7)}`,
      date: compare?.commits?.at(-1)?.commit?.author?.date?.slice(0, 10) ?? '',
    };
  }

  const release = await github(`https://api.github.com/repos/${p.repo}/releases/latest`);
  if (!release) return { status: 'no release found', latest: '?' };
  const latest = release.tag_name;
  const date = release.published_at.slice(0, 10);
  if (p.installed === 'unknown') return { status: 'unknown installed version', latest, date };
  if (normalize(latest) !== normalize(p.installed)) return { status: 'UPDATE AVAILABLE', latest, date };
  if (!p.patched) return { status: 'up to date', latest, date };

  // Self-built from a release: upstream fixes often land on main long before the next release
  // (e.g. a merged PR that makes one of our patches unnecessary), so count the commits after the tag too.
  // reviewedMain = last main commit we already checked (nothing relevant), only newer ones are reported.
  const base = p.reviewedMain ?? latest;
  const compare = await github(`https://api.github.com/repos/${p.repo}/compare/${base}...HEAD`);
  const ahead = compare?.ahead_by ?? '?';
  return {
    status: ahead === 0 ? 'up to date' : `${ahead} new commit(s) on main since ${p.reviewedMain ? 'last review' : 'release'}`,
    latest: ahead === 0 ? latest : `${latest}, main ${compare?.commits?.at(-1)?.sha?.slice(0, 7) ?? '?'}`,
    date,
  };
}

const results = [];
for (const p of plugins) {
  try {
    results.push({ ...p, ...(await check(p)) });
  } catch (e) {
    results.push({ ...p, status: `error: ${e.message}`, latest: '?' });
  }
}

// Every column is as wide as its longest value plus 2 spaces, so long names never run into the next column
const rows = results.map((r) => [
  r.name,
  r.installed.startsWith('commit:') ? r.installed.slice(7, 14) : r.installed,
  r.latest,
  r.date ?? '',
  r.patched ? `${r.status} (self-built)` : r.status,
]);
const header = ['PLUGIN', 'INSTALLED', 'LATEST', 'RELEASED', 'STATUS'];
const widths = header.map((h, i) => Math.max(h.length, ...rows.map((row) => String(row[i]).length)) + 2);
const line = (row) => row.map((c, i) => (i === row.length - 1 ? String(c) : String(c).padEnd(widths[i]))).join('');
console.log('\n' + line(header));
console.log('-'.repeat(widths.reduce((a, b) => a + b, 0)));
for (const row of rows) console.log(line(row));

const outdated = results.filter((r) => r.status === 'UPDATE AVAILABLE' || /new commit/.test(r.status));
console.log('\n' + (outdated.length
  ? `${outdated.length} plugin(s) can be updated. Settings to keep when updating:`
  : 'Everything is up to date.'));
for (const r of outdated) {
  console.log(`\n  ${r.name}  (https://github.com/${r.repo}/releases)`);
  if (r.patched) console.log(`    - SELF-BUILT, do not just install the release, redo our changes with: ${r.patched}`);
  for (const k of r.keep) console.log(`    - keep: ${k}`);
}
console.log('');
