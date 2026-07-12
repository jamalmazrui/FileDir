# FileDir Modernization — Transition Brief

Purpose: carry the learnings from the EdSharp 5.0 modernization into a new
project that modernizes **FileDir** the same way. Upload this file as the first
message of the new FileDir project so the assistant starts with full context.
Each Claude Project has its own separate memory, so nothing from the EdSharp
project transfers automatically — this brief is the bridge.

---

## 1. How to use this brief

- Start the new project, attach the FileDir source materials listed in section 8,
  and paste/upload this brief.
- The assistant cannot compile or test on Windows (see section 3), so the working
  loop is: assistant edits source and packages a zip → you build/run on Windows →
  you upload the build log and report behavior → repeat.
- Treat EdSharp 5.0 and DbDo as the two reference implementations. Where FileDir
  shares their architecture (it largely does), reuse the proven solution rather
  than reinventing it.

---

## 2. Developer profile and working preferences

- The developer (Jamal Mazrui) is blind and uses screen readers: JAWS and NVDA on
  Windows, VoiceOver on iOS. Explanations belong inline in the chat text.
- Response conventions the assistant should follow every turn:
  - Begin each reply with a line that simply says `Claude:` (acts as a heading
    landmark for screen-reader navigation).
  - End each reply with a Markdown level-2 `## Summary` heading.
  - Default output is Pandoc-flavored Markdown; offer a copy button and a
    downloadable Markdown file when producing documents.
  - Keep responses reasonably concise; minimize heavy formatting.
- Coding style is **Camel Type** (see section 9). Use it by default for all code.
- Avoid bringing up unrelated personal details; keep things professional and
  focused on the work.

---

## 3. Environment and workflow constraints

- The assistant's sandbox has **no Windows, no .NET, no C# compiler, and no
  network**. It therefore **cannot compile, run, ngen, or test** any of this
  software, and cannot compile JAWS scripts. It can run `pandoc`, Python
  (with Pillow, markdown), `zip`, and standard Unix tools.
- Working loop (this is exactly how EdSharp proceeded over ~40 revisions):
  1. Assistant edits source in a working copy.
  2. Assistant stages files and produces a single deliverable zip
     (e.g., `FileDir.zip`) plus any standalone docs.
  3. You unzip into the program folder on Windows, run the build script, and
     upload the resulting build log.
  4. Assistant reads the log, fixes, and repeats. You also report runtime/voicing
     behavior, since the assistant can't observe it.
- Because the assistant can't test, **static verification is essential**: keep a
  brace-balance invariant for the monolithic source file (EdSharp's
  open-minus-close brace delta had to stay at a fixed number after every edit;
  establish FileDir's baseline number early and check it after every change),
  confirm each edited routine is individually brace-balanced, and confirm every
  referenced symbol resolves.

---

## 4. The program family (shared "Homer" lineage)

All of these are by the same author and share the "Homer editor interface"
(common menu system, Alternate Menu = Alt+F10, Hotkey Summary, Key Describer,
Web Client Utilities, JAWS scripts, similar build/installer patterns):

- **EdSharp** — accessible text/source editor. Modernized to **5.0** (first
  64-bit release). The primary reference for this effort.
- **FileDir** — accessible file/directory manager. **The target of the new
  project.** Last known era ~version 3.8.
- **DbDo** — accessible database manager. Already modernized; its
  `DbDo.cs` (Elevate Version) and `DbDo_setup.iss` (launch-hotkey shortcut) are
  clean reference models that EdSharp deliberately followed.
- **2htm** — `2htm.exe`, an MIT-licensed multi-format document-to-text/HTML
  converter by the same author (github.com/JamalMazrui/2htm). Now bundled in
  EdSharp; reuse it in FileDir.
- Homer/EmpowermentZone toolkit — shared scripts and utilities.

---

## 5. What FileDir is (from available materials)

FileDir is a keyboard-driven, screen-reader-efficient file and directory manager.
Observed characteristics (confirm against the actual source at project start):

- Launch hotkey **Alt+Control+F** (Windows desktop shortcut key) — the same
  shortcut-key mechanism EdSharp uses for Alt+Control+E.
- List-centric UI: navigates folders, tags items (Space), Tag All (Control+A),
  opens subfolders/zip archives/files (Enter / Shift+Enter), parent folder
  navigation (Backspace), Windows Properties (Alt+Enter).
- Text operations that almost certainly use the same converter machinery EdSharp
  did: **Append Tagged** (append textual content of files to the clipboard) and
  **Extract with Regular Expression** (Control+Shift+E). These are the FileDir
  features most likely to depend on the broken `GetText.exe` (see section 6).
- **Web Client Utilities**, Alt+Shift+Space — a collection of ~35 "web 2.0"
  utilities (unit/currency conversion, URL shorten/expand, captcha submit, etc.),
  several backed by a Python helper (`InPy.py` / `InPyC.exe`). Many of these
  services are likely dead in 2026 and will need auditing.

---

## 6. The EdSharp modernization playbook (reuse this)

These are the concrete patterns EdSharp adopted. FileDir shares the architecture,
so most apply directly.

### Build and platform
- Single monolithic C# WinForms source file in one namespace, plus portable
  Homer helper files, compiled by a direct `csc` command (not an SDK project).
- Target: **.NET Framework 4.8, x64** (`/platform:x64`), Roslyn `csc.exe`
  preferred (fallback to framework `csc`), with `/win32manifest` and
  `/win32icon`. A `BuildFileDir.cmd` should mirror `BuildEdSharp.cmd`: locate the
  compiler, compile, then best-effort fetch any missing third-party tools.
- Add an `AssemblyVersion` and a single `VersionString` constant used by the
  updater and the About box.

### Portable Homer helper files (reuse as-is from EdSharp 5.0)
These compile in `namespace Homer` and were written BCL-only and screen-reader
aware. They should drop into FileDir with little change:
- **Say.cs** — speech with a fallback chain: JAWS → NVDA → UIA → SAPI.
- **Inix.cs** — `InixCodec` for the `.inix` config format (order-preserving,
  multi-line verbatim values, written UTF-8-with-BOM + CRLF).
- **KeyMap.cs** — Hotkey Summary, Key Describer, Alternate Menu support.
- **Web.cs** — BCL-only web client: TLS 1.2/1.3, Chrome user-agent,
  `getPage(url, out finalUrl)` and `download(url, dir, name)`, both returning ""
  on failure (used by the updater).
- **Lbc.cs** — line/buffer helper used by EdSharp; include if FileDir needs it.

### Configuration (.inix over .ini)
- Two-file model: a **default** config in the program folder (factory settings,
  shipped) and a **per-user** config in the data folder (holds user state and the
  ACTIVE settings the app reads at runtime).
- The `.inix` layer is read **in preference to** the Win32 `.ini` for the same
  key, so it is a non-destructive override mechanism. Important consequence: a zip
  or default-config change does NOT alter an existing user's active settings in
  the data folder. Deliver changes to converter/command definitions BOTH ways —
  update the default config (for new installs) AND provide a ready `.inix`
  override block (for existing installs).

### Encoding autodetection
- Use the UDE library (`Ude.dll`, fetched at build time) behind a compile flag:
  BOM check → detect → fall back to UTF-8. Reuse EdSharp's `GetFileEncoding`.

### Converters — replace GetText.exe with 2htm (high priority for FileDir)
- The old generic extractor **`GetText.exe` hangs on modern Windows.** EdSharp
  and FileDir both call it. EdSharp replaced all text-extraction paths with
  **`2htm.exe`** (bundled in `Convert\2htm\`) run in plain-text mode (`-p`),
  through a thin wrapper `Convert\any2txt.cmd` that renames 2htm's
  `<basename>.txt` output to the exact target file the app expects.
- 2htm handles docx/doc/rtf/odt/pdf/xlsx/xls/pptx/ppt/csv/html/htm/md/json/txt.
- Keep Pandoc for lightweight-markup conversions and Xpdf `pdftotext` for PDF
  (no Office dependency). Office COM converters (WdVert/XlVert/PpVert) and
  `htm2txt.exe` were retired for text extraction.
- FileDir's **Append Tagged** and **Extract with Regular Expression** almost
  certainly route through the same converter table — point them at `any2txt.cmd`
  / 2htm. This is likely the single biggest correctness win for FileDir.

### Elevate Version / self-update (the DbDo model)
- Replace any old "AppStamp.ini over HTTP" mechanism with **GitHub Releases**:
  - `FetchLatestReleaseTag("JamalMazrui/FileDir")` — try the REST API
    (`api.github.com/repos/.../releases/latest`, parse `tag_name`), then fall back
    to the `releases/latest` page and read the tag from the post-redirect URL.
    Both via `Homer.Web.getPage` (supplies the required User-Agent and TLS).
  - `CompareVersions` — dotted-numeric ("5.0" == "5.0.0").
  - On confirmation, download `releases/latest/download/FileDir_Setup.exe` via
    `Homer.Web.download`, then launch it with `ProcessStartInfo.UseShellExecute =
    true` (lets the installer request UAC). **Do not** exit the app — the Inno
    installer detects the running instance and offers to close it.
  - GitHub asset download URLs are **case-sensitive**: the requested asset name
    must exactly match the installer's `OutputBaseFilename`.

### Installer (Inno Setup) — follow DbDo_setup.iss
- Launch shortcut with the hotkey, DbDo-style: a single `{autodesktop}\FileDir`
  shortcut with `HotKey: Alt+Ctrl+F` (`{autodesktop}` adapts to per-user vs
  all-users installs). Do NOT also put the hotkey on Start Menu items — exactly
  one shortcut should own the hotkey.
- FileDir, like EdSharp, has a **legacy installer** that placed an
  Alt+Control+F shortcut on the user's desktop pointing at the old exe. Add an
  `[InstallDelete]` that removes any pre-existing `FileDir.lnk` from BOTH
  `{userdesktop}` and `{commondesktop}` before `[Icons]` recreates the single
  shortcut, so the new one is the sole owner of the hotkey. (This was the actual
  root cause of "the hotkey still launches the old version" in EdSharp.)
- No `-activate` parameter is needed if FileDir is single-instance and its
  `OnStartupNextInstance` foregrounds the running copy (verify this; EdSharp does
  it natively, DbDo used `-activate` only because of its dual GUI/CLI shortcut).
- **Native pre-JIT (ngen) belongs in the installer, not the build script.** Use
  a `{code:NgenExe}` helper pointing at
  `{win}\Microsoft.NET\Framework64\v4.0.30319\ngen.exe` with a `HasNgen` check;
  in `[Run]` do `ngen uninstall` then `ngen install "{app}\FileDir.exe"
  /AppBase:"{app}" /nologo /silent` (runs elevated, silent), and `ngen uninstall`
  in `[UninstallRun]`. This gives the installed copy a cached native image
  (faster startup). A build-time ngen in the build folder does nothing useful.
- Ship a `FileDir.exe.config` next to the exe (flag `ignoreversion`) with startup
  tuning: `<generatePublisherEvidence enabled="false"/>` (avoids an
  Authenticode/CRL stall at launch) and `<gcConcurrent enabled="true"/>`.
- Install JAWS scripts via a `--install-jaws-settings` post-install step and offer
  the NVDA add-on. Ship the default config with `onlyifdoesntexist` so existing
  user configs are not clobbered.

### JAWS scripts (critical, and the assistant can only partly help)
- **The assistant cannot recompile `.jsb`** (that needs JAWS on Windows). It CAN
  edit the keymap `.jkm`, which JAWS reads live. Prefer keymap-only fixes; if a
  `.jss` change is truly required, you must recompile in JAWS and the assistant
  should not silently let the shipped `.jss` and `.jsb` drift apart.
- JAWS resolves a script/keystroke in the app context first, then climbs to
  `Default.jss`/`Default.jkm`. A key like Control+DownArrow has a **default JAWS
  implementation** that runs unless the app keymap provides an effective override.
- The single most useful pattern: when JAWS's default behavior for a key is worse
  than the application's native behavior, **force pass-through** by binding the
  key directly to the built-in `TypeCurrentScriptKey` in the app keymap
  (e.g., `Control+UpArrow=TypeCurrentScriptKey`). This is robust and needs no
  recompile. EdSharp's earlier, fragile approach routed through scripts that only
  passed through when `UIIsEditorWindow()` returned true — and that function used
  an **exact** window-class match (`== "WindowsForms10.RichEdit"`) that does NOT
  match the real WinForms RichEdit class (`WindowsForms10.RICHEDIT50W.app.0.…`),
  so it fell back to the JAWS default. FileDir's editor/list detection will have
  the same fragility: prefer `StringContains(class, "RICHEDIT")` /
  appropriate list-class substring, case-insensitively, or sidestep it with
  direct keymap pass-through.

### Documents
- Convert Markdown to HTML with bundled Pandoc (`-f gfm -t html5 -s`); modern
  flags only (the removed `-S`/`--smart` and old `markdown_github` will fail on
  Pandoc 3.x — use `gfm`).
- Generated `.htm` should keep heading IDs aligned with any in-document anchors;
  watch for stray/unbalanced code fences that swallow later headings (an
  accessibility problem). Keep a clear title/version/date/copyright block and
  update it to the new version.

### Repository hygiene
- `.gitignore`: ignore the built `FileDir.exe`/`.dll`, the setup exe, the build
  log, fetched tools (`Ude.dll`, the `Convert\<tool>` folders), and lock files;
  KEEP committed binaries like the icon, bundled `2htm`, and any required DLLs.
- `.gitattributes`: `* text=auto eol=crlf` plus `*.exe`/`*.dll`/`*.ico` `binary`.

---

## 7. Hard-won lessons and gotchas

- **Line endings/encoding:** shipped text files are CRLF; configs the apps write
  are UTF-8-with-BOM + CRLF. When editing with Python, read/write with
  `newline=""` to preserve CRLF, and verify endings with `tr -cd '\r' | wc -c`
  rather than a `grep $'\r'` test (which gave false readings).
- **Monolithic handler editing:** the giant `menuItem_Click`-style handler has
  per-`if` block scope; block-local variables are safe, but method-level
  variables must be assigned (not re-declared), and avoid early `return;` inside
  it (wrap success paths in `if`).
- **Don't collide names:** never rename a Homer type to something that clashes
  with an app member or BCL type; qualify (`Homer.Web`, `InixCodec.Section`).
- **Active config lives in the data folder:** see section 6 (Configuration). Plan
  delivery accordingly.
- **Past assistance is not a license:** keep the child-safety/weapons/etc. norms;
  not relevant here, but the assistant should stay in normal helpful mode.

---

## 8. What to gather at the start of the new project

The assistant does NOT have FileDir's source. Provide as many of these as
possible (the more complete, the faster it can move):

- FileDir source: the main `FileDir.cs` (or equivalent) and any helper sources.
- The current build script and the Inno Setup script (`*.iss`).
- JAWS scripts: `FileDir.jss`, `FileDir.jkm`, headers, and any `.jsb` (note the
  recompile constraint).
- NVDA add-on, if any.
- The current `FileDir.ini` (especially its `[Import]`/converter command table)
  and the Hotkeys file.
- Documentation (`FileDir.md` / `FileDir.txt`) and the current version/date.
- Web Client Utilities materials: `InPy.py` / `InPyC.exe` and the list of the
  ~35 utilities (for the audit in section 5).
- A list of third-party dependencies FileDir currently ships or calls.

Reuse directly from EdSharp 5.0 / DbDo (ask the assistant to pull these in):
- The portable Homer files (`Say.cs`, `Inix.cs`, `KeyMap.cs`, `Web.cs`, `Lbc.cs`).
- `2htm.exe` + `any2txt.cmd` and the converter approach.
- `DbDo.cs` (Elevate Version) and `DbDo_setup.iss` (hotkey shortcut) as models.
- The Camel Type guide.

---

## 9. Camel Type coding style (apply by default)

Summarized from the developer's spec; ship/keep a `Camel_Type_CSharp.md` in the
project for the authoritative version.

- Hungarian prefixes on variables/arguments: `a` array, `b` bool, `bin` byte
  buffer, `dt` datetime, `f` file object, `h` handle, `i` int, `ls` List (use
  `ls`, not `l`), `n` real, `s` string, `d` Dictionary, `hs` HashSet, `o`
  COM-only object; managed instances use a lower-camel class-name prefix (or a
  common abbreviation such as `sb`, `ex`, `en` for Encoding, `match`); `v`
  variant.
- `lowerCamel` for methods, variables, locals; `PascalCase` for class names;
  public API may be PascalCase.
- Constants are named like variables (NO `c_` prefix); declared on their own
  lines above the variables, grouped and alphabetized by type. Third-party
  constant names are used verbatim.
- Declarations grouped at the top of scope, alphabetized by type then name where
  practical; prefer explicit types over `var`.
- Prefer `foreach` over indexed `for`; single-line `if`-then; double-quoted
  strings; no subprocedures (use functions even when no value is returned).
- Files the app writes: UTF-8-with-BOM + CRLF.

---

## 10. Suggested roadmap for FileDir (mirrors EdSharp's sequence)

1. Stand up the x64 / .NET 4.8 / Roslyn build (`BuildFileDir.cmd`); record the
   brace-balance baseline; get a clean compile log from you.
2. Adopt the portable Homer files (Say/Inix/KeyMap/Web/Lbc).
3. Encoding autodetection (Ude).
4. Converters → 2htm + `any2txt.cmd`; wire FileDir's Append/Extract-text and any
   "view/append file content" paths to it; retire `GetText.exe`.
5. Elevate Version via GitHub Releases (DbDo model), with `FileDir_Setup.exe`.
6. Installer: Alt+Control+F hotkey shortcut (DbDo model) + legacy `[InstallDelete]`
   cleanup; ngen install/uninstall; `FileDir.exe.config` startup tuning; JAWS and
   NVDA settings install.
7. JAWS scripts: keymap pass-through where JAWS overrides good native behavior;
   robust list/editor detection.
8. Web Client Utilities: audit the ~35 utilities, drop or replace dead services,
   and decide the fate of the `InPy.py` Python dependency.
9. Documentation refreshed to the new version/date/copyright and 64-bit / .NET 4.8
   requirements; regenerate the `.htm`.
10. Highest-risk item, done on its own with you launch-testing: if FileDir relies
    on `Microsoft.VisualBasic`'s `WindowsFormsApplicationBase` for single-instance,
    plan the eventual move to a Mutex + IPC (this was deferred in EdSharp too).

---

## 11. Note on a future native/.NET 8 port (informational)

Modern .NET (Core/5–8) does support Windows Forms with high API compatibility, so
a future port is conceivable. Caveats learned during EdSharp:

- **True NativeAOT is not supported for WinForms** (reflection/COM/data-binding),
  so a genuine single native binary is not currently achievable for these apps.
- A .NET 8 port could use ReadyToRun + self-contained single-file publishing
  (fast startup, one exe, no separate runtime install), but it is NOT a
  no-significant-changes move: it needs an SDK-style project, compatibility
  testing, and — critically — replacement of any **JScript .NET** component
  (compiled with `jsc.exe`), which exists only on .NET Framework. Check whether
  FileDir has such a component before estimating effort.
- The installer's `ngen` already gives cached native-code startup on .NET
  Framework 4.8, which is most of the practical launch-speed benefit. Treat a
  .NET 8 port as a separate, deliberate modernization project, not a quick win.

---

## Summary

This brief transfers the EdSharp 5.0 modernization playbook to a new FileDir
project. It records the developer's working preferences and the assistant's
build-only-via-you workflow (no compile/test in the sandbox); the shared Homer
architecture across EdSharp, FileDir, DbDo, and 2htm; and the concrete, reusable
solutions — x64/.NET 4.8 Roslyn build, the portable Say/Inix/KeyMap/Web/Lbc
helpers, the `.inix` override-over-`.ini` config model, encoding autodetection,
the `GetText.exe` → `2htm` converter replacement (especially relevant to
FileDir's Append/Extract-text features), the GitHub-Releases Elevate Version
updater, the Inno installer patterns (Alt+Control+F hotkey shortcut with legacy
`[InstallDelete]` cleanup, installer-time `ngen`, `.exe.config` startup tuning,
JAWS/NVDA settings install), the JAWS keymap pass-through technique and the
fragile window-class-detection pitfall, Pandoc document generation, and repo
hygiene. It lists exactly what FileDir materials to gather at kickoff, the Camel
Type style to apply by default, a staged roadmap mirroring EdSharp's revisions,
and an honest note that a true native/.NET 8 port is a separate effort (WinForms
is not NativeAOT-compatible, and any JScript .NET component is a blocker).
