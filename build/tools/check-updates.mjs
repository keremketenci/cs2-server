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
  return { status: normalize(latest) === normalize(p.installed) ? 'up to date' : 'UPDATE AVAILABLE', latest, date };
}

const results = [];
for (const p of plugins) {
  try {
    results.push({ ...p, ...(await check(p)) });
  } catch (e) {
    results.push({ ...p, status: `error: ${e.message}`, latest: '?' });
  }
}

const pad = (s, n) => String(s ?? '').padEnd(n);
console.log(`\n${pad('PLUGIN', 46)}${pad('INSTALLED', 18)}${pad('LATEST', 34)}${pad('RELEASED', 12)}STATUS`);
console.log('-'.repeat(130));
for (const r of results) {
  const installed = r.installed.startsWith('commit:') ? r.installed.slice(7, 14) : r.installed;
  const status = r.patched ? `${r.status} (self-built)` : r.status;
  console.log(`${pad(r.name, 46)}${pad(installed, 18)}${pad(r.latest, 34)}${pad(r.date, 12)}${status}`);
}

const outdated = results.filter((r) => r.status === 'UPDATE AVAILABLE' || /new commit/.test(r.status));
console.log('\n' + (outdated.length
  ? `${outdated.length} plugin(s) can be updated. Settings to keep when updating:`
  : 'Everything is up to date.'));
for (const r of outdated) {
  console.log(`\n  ${r.name}  (https://github.com/${r.repo}/releases)`);
  if (r.patched) console.log(`    - SELF-BUILT, do not just install the release: ${r.patched}`);
  for (const k of r.keep) console.log(`    - keep: ${k}`);
}
console.log('');
