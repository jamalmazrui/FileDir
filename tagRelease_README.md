# Generic tagRelease — release workflow for EdSharp, FileDir, DbDo, and siblings

## What is in this zip

| File | Where it goes | Purpose |
| --- | --- | --- |
| `tagRelease.ps1` | each repo root (`C:\FileDir`, `C:\EdSharp`, `C:\DbDo`) | The generic release script. No editing between apps or between releases. |
| `tagRelease.cmd` | beside `tagRelease.ps1` | Launcher; runs PowerShell with an execution-policy bypass for that one invocation. |
| `BuildFileDir.cmd` | `C:\FileDir` | Your build script, with a version-consistency check added. |

Add `tagRelease.ps1`, `tagRelease.cmd`, and `tagRelease.log` to each repo's
`.gitignore` — they are maintainer tools, not user-facing files.

## What it discovers by itself

Nothing is hardcoded, so the same two files work in every repo:

- **App name** — the current directory name. `C:\FileDir` means the app is `FileDir`.
- **Setup script** — the matching `<App>_setup.iss` (case-insensitive, so
  `C:\EdSharp` finds `edsharp_setup.iss`).
- **Installer asset** — taken from the `.iss` `OutputBaseFilename`, so the asset
  name always matches what Inno Setup actually emits. This matters because F11
  downloads that exact name and GitHub asset URLs are case-sensitive.
- **Version** — read from the `.iss`, in either Inno style:
  `#define AppVersion "1.0.126"` (DbDo) or `AppVersion=5.0` (EdSharp, FileDir).
- **Owner/repo** — parsed from the `origin` git remote, so the public URL is
  right even when the GitHub repo name differs in case from the local folder.

## The version problem it solves (why F11 said "up to date")

Elevate Version (F11) compares two numbers that came from **two different
places**:

- the version compiled into the running program — the
  `public const string VersionString = "..."` line in `<App>.cs`; and
- the tag of the latest GitHub release — which comes from `AppVersion` in the
  `.iss`.

Those two drift. In your own repos right now:

- `DbDo.cs` says `1.0.111` while `DbDo_setup.iss` says `1.0.126`.
- `EdSharp.cs` said `5.0.0` while `edsharp_setup.iss` said `5.0` — and
  `5.0` compares **equal** to `5.0.0`, which is exactly why re-posting a new
  build under the same number never registered as an update.

`tagRelease.ps1` removes the whole class of bug. On every run it:

1. reads `AppVersion` from the `.iss`;
2. **bumps the last number** (`5.0` → `5.0.1`, `1.0.126` → `1.0.127`), so every
   release is genuinely higher than the one before — a two-part version gains a
   third part precisely because `5.0` and `5.0.0` are equal;
3. writes that version **back into the `.iss`** (`AppVersion`, plus
   `AppVerName` / `VersionInfoVersion` when they carry a literal) **and into
   `<App>.cs`'s `VersionString`**, so the `.exe` and the release tag always
   agree;
4. commits those version files, tags, publishes, and uploads the installer.

Because the version is written into `<App>.cs`, **the app must be rebuilt and the
installer recompiled after a bump.** The script checks the installer's timestamp
and refuses to publish an installer older than the version bump, telling you to
rebuild — so a release whose program reports the wrong version cannot escape.

## Normal release, start to finish

```
cd C:\FileDir
.\tagRelease.cmd -PrepareOnly     ' bump 5.0 -> 5.0.1, sync .iss + FileDir.cs
.\BuildFileDir.cmd                ' rebuild FileDir.exe with the new version
' compile FileDir_setup.iss in Inno Setup -> FileDir_setup.exe
.\tagRelease.cmd -NoBump          ' publish 5.0.1 (do not bump again)
```

If you forget `-PrepareOnly` and just run `.\tagRelease.cmd`, no harm is done:
it bumps and syncs the version files, then stops with a clear "stale installer"
message telling you to rebuild and re-run with `-NoBump`. Two runs, same result.

## Options

| Option | Effect |
| --- | --- |
| *(none)* | Bump the last version number, sync, publish. |
| `-PrepareOnly` | Bump and sync the version files only; do not tag or publish. |
| `-NoBump` | Publish the version currently in the `.iss`, unchanged. |
| `-Version 5.1` | Use an explicit version instead of bumping. |
| `-NoCommit` | Do not commit the version files (they are still written). |
| `-SkipStaleCheck` | Publish even if the installer looks older than the bump. |

**Uncommitted changes never block a release.** The script commits the version
files it changed, warns about anything else still outstanding, and proceeds.

## The build-script change

`BuildFileDir.cmd` now compares `VersionString` in `FileDir.cs` with
`AppVersion` in `FileDir_setup.iss` before compiling, and stops with a clear
error if they disagree, pointing you at `tagRelease.cmd -PrepareOnly`. This means
a mismatched pair can never be built into an installer in the first place. (Run
it today and it will flag the existing `5.0.0` vs `5.0` mismatch — that is the
check doing its job.)

To add the same guard to `BuildEdSharp.cmd` or `buildDbDo.cmd`, copy the
`version consistency check` block and change the three file names.

## Requirements

`git` and `gh` in `PATH`, `gh` authenticated (`gh auth login`), PowerShell 5.1+.

## One-time cleanup worth doing

`DbDo.cs` (1.0.111) and `DbDo_setup.iss` (1.0.126) are out of step. Run
`tagRelease.cmd -PrepareOnly` in `C:\DbDo` once: it will set both to 1.0.127.
Rebuild, recompile the installer, then `tagRelease.cmd -NoBump`. From then on the
two stay in step by construction.
