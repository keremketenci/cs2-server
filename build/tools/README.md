# Plugin list and update check

`check-plugin-updates.bat` (repo root) runs `check-updates.mjs`, which compares every entry in `plugins.json`
with GitHub. It never looks at the plugin files: **`plugins.json` is the only source of what is installed**.
After every manual plugin change, update it as described below, or the check reports wrong results.

## How the check decides

| Entry | Compared with | "up to date" when |
|---|---|---|
| no `repo` (our own plugin) | nothing | always shown as "own plugin" |
| `installed` is a tag, e.g. `v2.0.1` | latest GitHub release | release tag equals `installed` (a leading `v` / `build-` is ignored) |
| same, plus `patched` (self-built from a release) | latest release, then the commits on main after it | release is the same and no commit on main after the tag (or after `reviewedMain`) |
| `installed` is `commit:<sha>` (self-built from main) | commits on main after that commit | no commit on main after it (or after `reviewedMain`) |

The check only says *something new exists upstream*. Whether it fixes what we patched is decided by hand
(step 0 in the plugin's guide, `build/patches/<Plugin>.md`).

## Fields

| Field | Meaning |
|---|---|
| `name` | Shown in the table |
| `folder` | Where the plugin lives under `addons/counterstrikesharp/` (info only) |
| `repo` | `owner/name` on GitHub, `""` for our own plugins |
| `installed` | Release tag we installed, or `commit:<full sha>` when self-built from main |
| `keep` | Files and settings that must survive an update, printed when an update is available |
| `patched` | Only for plugins we self-build with local changes: path of its guide `build/patches/<Plugin>.md` |
| `reviewedMain` | Optional, only with `patched`: full sha of the last main commit we already reviewed (nothing relevant for us). Commits up to it are not reported again |

## After a manual update: what to change

### 1. Normal plugin (installed the release zip)

Set `installed` to the new release tag as GitHub shows it:

```json
"installed": "v2.0.1",
```

Add a line to `keep` if the update has new files or settings to keep.

### 2. Self-built plugin (`patched`)

First follow the plugin's guide (`build/patches/<Plugin>.md`), including its "Afterwards" steps. Then:

- **Built from a release tag:** `"installed": "<tag>"` and delete the `reviewedMain` line.
- **Built from a main commit:** `"installed": "commit:<full sha>"` and delete the `reviewedMain` line.
  Use this form also when a plugin that was built from a tag is now built from main.

Full sha of the current main (first column of the output):

```
git ls-remote https://github.com/<repo> refs/heads/main
```

### 3. Reviewed new upstream commits, nothing relevant (nothing installed)

Self-built plugins only. Set `reviewedMain` to the full sha of the newest main commit you looked at
(add the line after `patched` if it is missing):

```json
"patched": "build/patches/MenuManagerAPI.md",
"reviewedMain": "ccab9728edb3bc480ec9236fd1286e79a9e1792e"
```

Do not change `installed` for this: it must still say what is really installed.

### 4. Upstream fixed our bug, the patch is dropped

Install the release as a normal plugin, delete `patched` and `reviewedMain`, set `installed` to the release tag.
Also remove the plugin from `build/patches/README.md` and delete its guide and `.patch` file.

### 5. Plugin added or removed

- Added: new entry with `name`, `folder`, `repo`, `installed`, `keep` (`[]` if nothing), plus `patched` if self-built.
- Removed from the server: delete its entry.

### 6. Check

- JSON syntax: a missing or extra comma stops the script. Quick test: `node -e "require('./build/tools/plugins.json')"`
  from the repo root (prints nothing when it is fine).
- Run `check-plugin-updates.bat`: the plugin you changed shows "up to date".
  "commit ... not found" means a wrong sha in `installed` or `reviewedMain`.

## Limits

GitHub allows 60 API requests per hour without a login. One run uses about 35, so do not run it twice within an hour,
otherwise some rows show "GitHub rate limit reached".
