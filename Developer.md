# FileDir — Developer Guide

**Version 5.0.76**  
August 2026  
Copyright 2006-2026 by Jamal Mazrui  
MIT License

This document is for developers. It is as technical as it needs to be, and it
assumes familiarity with Windows, C#, and the command line.

## Contents

- [Requirements](#requirements)
- [The Sources](#the-sources)
- [Building](#building)
- [The Audit](#the-audit)
- [Single Sources of Truth](#single-sources-of-truth)
- [Releasing](#releasing)
- [Keeping the Folder Clean](#keeping-the-folder-clean)
- [Conventions](#conventions)
- [Traps Worth Knowing](#traps-worth-knowing)

## Requirements

- Windows 10 or 11, x64 or ARM64.
- .NET Framework 4.8 and its Developer Pack. The Developer Pack supplies the
  reference assemblies; the build falls back to the GAC copies if it is absent.
- A C# compiler. The build prefers the Roslyn `csc.exe` shipped with Visual
  Studio Build Tools 2022 and falls back to the Framework `csc.exe`.
- `jsc.exe`, the JScript .NET compiler, which ships with the Framework. It
  compiles `FileDir.js` into `FileDirScript.dll`, the expression evaluator
  behind the Evaluate command.
- Inno Setup 6 (or 5.6 and later) for `ISCC.exe`, which compiles the installer.
- `git` and the GitHub CLI `gh`, used by `tagRelease`.

Nothing is fetched from NuGet and there is no MSBuild project. The build is a
direct compiler invocation, which keeps it inspectable and fast.

## The Sources

FileDir itself:

- `FileDir.cs` — the program. One monolithic WinForms source, about 8,170 lines.
- `Dialogs.cs` — FileDir's own dialogs.
- `MediaPlayer.cs` — the Homer Player dialog behind Play Queue: the queue, its
  ordering, the transport, the clip export, and the per-play-list settings kept
  in `HomerPlayer.inix`. It owns the mpv process for exactly as long as the
  dialog is open.
- `FileDir.js` — the JScript .NET expression evaluator.

Shared Homer files, common to FileDir, EdSharp, and DbDo, compiled in
`namespace Homer`:

- `Say.cs` — speech, trying JAWS through its automation interface, then NVDA
  through its controller library, then a native UIA notification that Narrator
  reads, stopping at the first that answers. It also carries
  `Homer.JawsSettingsInstaller`, which the installer calls through
  `FileDir.exe --install-jaws-settings`.
- `Inix.cs` — the `.inix` configuration layer read in preference to the classic
  `.ini` for the same key, giving a non-destructive override.
- `Web.cs` — a base-class-library-only web client with TLS 1.2 and 1.3 and a
  browser user agent, used by Elevate Version.
- `Util.cs` — shared helpers.
- `KeyMap.cs` — **generated**; see below.
- `Lbc.cs` — the Layout by Code dialog toolkit.
- `Mpv.cs` — drives the mpv media player over its documented JSON IPC pipe:
  mpv runs with no window, no keys and no focus of its own, and every command
  travels as one line of JSON. Three rules in it are worth keeping. Never hold a
  lock across a write — the reader thread must never stop draining the pipe, or
  both ends deadlock, which happened. Never write to the pipe from the interface
  thread; commands go on a queue and a writer thread sends them. And mpv's own
  `stop` command clears the play list as well as stopping playback, which is why
  `stop()` here is a pause and the real thing is called `clearAndStop`.
- `Ollama.cs` — the client for the local model server, used by Translate File.
  Base class library only: the exchange is one string in and one string out,
  which does not justify a reference to `System.Web.Extensions` or
  `System.Runtime.Serialization` on a build that has to stay on .NET Framework
  4.8. Two rules in it are worth keeping. Never run the `ollama` command to ask
  a question — it starts the server in a console of its own that looks like a
  fault; ask over `http://localhost:11434` instead, which opens nothing. And
  choose the model from what is installed rather than making anyone configure
  it.

Referenced assemblies that must sit beside the sources: `FileAssociation.dll`,
`Tektosyne.dll`, `ICSharpCode.SharpZipLib.dll`, and optionally `Ude.dll`.

`Ude.dll` is a port of the Mozilla universal character set detector. The base
class library cannot detect a text file's encoding; Ude can, and EdSharp uses
the same library so both programs detect identically. The build copies it from
`..\EdSharp\Ude.dll` if it is missing. Without it the `HAVEUDE` symbol is not
defined and detection falls back to the byte order mark, which was the old
behaviour. Its absence can never fail a build.

## Building

```
cd C:\FileDir
BuildFileDir.cmd
```

**There are two commands to run.** `BuildFileDir` builds; `cleanFileDir` tidies.
Each is a `.cmd` wrapper, so neither PowerShell's execution-policy parameters nor
a Python path ever has to be typed, and each writes its own log beside itself.
`makeKeyMap.py`, `auditFileDir.py` and `homerPolicy.py` are called by those two
rather than run directly; to run the checks alone, use `BuildFileDir audit`.

The build is PowerShell and the checks and the sweep are Python, which matches
EdSharp: `BuildEdSharp.ps1` with `auditEdSharp.py` and `repoPolicy.py` beside
it. PowerShell suits the compiler and installer work; Python suits the text and
policy work, and is what lets the two projects share code rather than share only
an idea.

`BuildFileDir.ps1` does six things, in order, and appends every step to
`BuildFileDir.log` as it happens, so a build that dies still leaves a log:

1. **Key map.** `makeKeyMap.py` regenerates `KeyMap.cs` and `Hotkeys.md` from
   `Hotkeys.ini`. This runs first because the audit checks what it produces.
2. **Audit.** `auditFileDir.py` runs, and the build stops if anything fails.
   Nothing is compiled until the checks a compiler cannot make have passed.
   Warnings are printed and do not stop it.
3. **Version.** The last dotted part of `version.txt` is incremented and
   `Version.cs` is generated from the result.
4. **Compile.** `jsc.exe` builds `FileDirScript.dll`; `csc.exe` builds
   `FileDir.exe` with `/platform:anycpu`, the manifest, and the icon.
5. **Documents.** Each document's `**Version ...**` line is stamped from
   `version.txt`, then the `.htm` is regenerated for every Markdown document.
   `Hotkeys.md` is stamped by `makeKeyMap.py` instead, since the build does not
   edit a generated file.

   **Two converters.** `2htm.exe` is tried first, being the house tool, and
   Pandoc is the fallback. That is not belt and braces for its own sake: 2htm
   failed on all nine documents in one build with *Could not load file or
   assembly System.Memory* — the Span trap, needing `System.Memory.dll` beside
   the executable — and every page silently kept its previous HTML while the
   installer, which ships each `.htm` only if it exists, would have carried
   stale documents into a release. A page that comes out zero bytes is deleted
   rather than kept, and the build **stops** if any document ends with no HTML.

   That check is deliberately in the build and not in the audit. The audit runs
   before the conversion, so "no HTML yet" is the ordinary state of a tree just
   unarchived; putting a failure there made the build refuse to run the step
   that fixes it. The audit still reports the state as advice. The failure lives
   where it is actionable, one line after the conversion.
6. **Installer.** `ISCC.exe` compiles `FileDir_setup.iss` into
   `FileDir_setup.exe`.

Three optional words:

- `BuildFileDir nobump` recompiles without taking a new version number.
- `BuildFileDir noinstall` builds the program but skips the installer.
- `BuildFileDir audit` runs the checks and compiles nothing.

If `ISCC.exe` is not found the build still succeeds and says so; only the
installer is skipped.

Every external program is run with `runProgram`, which passes the program and
its arguments separately, quotes any argument holding a space, gives the command
an **empty standard input**, logs everything, captures the output into the build
log, and records the exit code.

The quoting and the empty standard input are both there because of one hang.
PowerShell joins an argument list with spaces and quotes nothing, so
`--metadata title=FileDir - ReadMe` reached Pandoc as four arguments, and the
bare `-` among them means *read standard input*. Pandoc waited for ever and the
console said nothing. Quoting fixes that case; the empty standard input fixes
the whole class, since any tool that reads standard input would hang the same
way. Passing them apart matters: the command interpreter's
quote stripping after `/c` defeats a quoted program with a quoted argument, and
PowerShell joins an argument list with spaces and quotes nothing. Kept apart, no
quoting rule applies at all.

**Check the script stamps first when a fault looks familiar.** Both
`BuildFileDir.log` and `auditFileDir.log` open with the date and size of every
script in the build. A failure that was supposedly fixed is far more often a
stale copy — an archive not unarchived, or unarchived after the build ran — than
a fix that did not work, and the stamps tell the two apart at a glance.

When a build fails, `BuildFileDir.log` is the file to send. The script opens
that log before doing anything that could fail and then traps unexpected errors.

**A log always exists, including for a parse error.** PowerShell parses a whole
script before running a line of it, so a script that will not parse never
reaches its own logging. That happened three times, each leaving no log at all.
Both wrappers now open the log and write the first lines *before* starting the
interpreter, capture everything it prints, and append that at the end; the
scripts append rather than truncate. So `BuildFileDir.log` is the file to send
whatever went wrong, and `python auditFileDir.py` is no longer the only way to
see a parse error.

Two PowerShell traps have caused those parse errors, and the audit now refuses
both:

- **`\"` to escape a quote.** PowerShell escapes with a backtick. This came from
  generating C# source, which is why that generator moved to `makeKeyMap.py`.
- **A continuation line beginning with `+`.** PowerShell wants the operator at
  the *end* of the line, or a backtick. Build a long message a line at a time
  into a variable instead.

The audit now refuses any `.ps1` containing a backslash-escaped quote, so that
particular cause is caught at the next build. Generating one language's source
from another is fiddly enough that it moved to `makeKeyMap.py`, where the
quoting is simple.

## The Audit

`auditFileDir.py` checks what a compiler cannot. Every check exists because
something broke, here or in EdSharp:

1. Every source file the build needs is present.
2. `FileDir.cs` still has its baseline brace delta. The open-minus-close count
   is a fixed number for a correct file — currently **15**, the surplus being
   braces inside strings and comments. An edit that moves it has unbalanced a
   block. This matters because whoever edits this file may not be able to
   compile it.
3. Every live menu command has a description.
4. No key is bound to two commands.
5. `Hotkeys.ini` describes no command that no longer exists.
6. `KeyMap.cs` is not older than `Hotkeys.ini`.
7. Every file named in the installer's `[Files]` section exists. An entry
   carrying `skipifsourcedoesntexist` fails silently at compile time, so a file
   can quietly stop shipping; that case is reported as a warning.
8. The installer script contains no literal version number.
9. The documentation set is complete.
10. `ReadMe.md` and `FileDir.md` name the current version.
11. No document still claims the GNU licence.
12. No document points at a retired installer name, a retired download address,
    Internet Explorer, GetText, or the removed Web Client Utilities.
13. No Markdown file in the folder belongs to another project. `ReadMe.md` and
    `Announce.md` were urlFido's for a while, and would have shipped as
    FileDir's if the installer had named them.

Checks 14 through 19 were added later: that every document the program opens is
one the installer ships; that the About box carries no typed version or licence;
that every delivered script writes its own log and acts without a confirmation
word; that Elevate Version asks GitHub for exactly the file name the installer
produces, since GitHub addresses are case sensitive; that nothing sits at the
root which the setup script and `RepoFiles.txt` do not claim; and that no Markdown file in the folder
belongs to another project.

Run the checks alone with `BuildFileDir audit`, which compiles nothing. The
audit prints PASS or FAIL for each check with a plain sentence, warns separately
about things that are wrong but not fatal, and returns 1 when anything fails so
the build can stop on it.

Add a check whenever something breaks. That habit is the point of it.

**Where a check belongs.** Anything the build regenerates — the HTML twins, the
version lines, the key map — is a NOTE before the build and a FAILURE only
after. The audit runs first, so before the build it is looking at what the last
one left, which just after unarchiving is not a fault at all. This was learned
three times: a missing `ReadMe.htm` once stopped the build that writes
`ReadMe.htm`, and eight warnings about unstamped documents once appeared on a
build that stamped all nine. A check that fires before the step that fixes it
teaches the reader to skim, and a reader who skims misses the one that
mattered.

## Single Sources of Truth

Two facts live in exactly one place each, and everything else is generated:

Everything that names a version reads it from `version.txt`: `Version.cs`, the
installer, the release tag, and the header line of every document. Nothing types
one.

**The version lives in `version.txt`**, one line and nothing else, as plain
UTF-8 with **no byte order mark**. Four things read it — the audit, the build,
`FileDir_setup.iss` and `tagRelease` — and only PowerShell is forgiving about a
mark. It both writes one, with `Set-Content -Encoding UTF8`, and silently strips
it again on reading, so the fault is invisible from that side; Inno Setup reads
it as part of the number and refuses to compile. The build writes the file with
`[System.IO.File]::WriteAllText` and a `UTF8Encoding($false)`, and reads it with
a `TrimStart` on the mark, so a file that already has one is repaired.
`BuildFileDir.ps1` increments it and generates `Version.cs`;
`FileDir_setup.iss` reads the same file with `FileOpen`/`FileRead`, so no
version literal appears in the installer script; `tagRelease` tags with it. The
program, the installer, and the tag therefore always agree, which is what
Elevate Version compares. This arrangement exists because a stale `.iss`
carrying its own `AppVersion` once rewound the number and the next build
re-minted a version already published.

**Keys and descriptions live in `Hotkeys.ini`.** The build generates from it
`KeyMap.cs`, a compiled-in table, and `Hotkeys.md`, the reference document.
`Version.cs` and `KeyMap.cs` are generated output: do not edit them, and they
are in `.gitignore`.

`KeyMap.cs` exists for a specific reason. The installer used to ship
`Hotkeys.ini` with the `onlyifdoesntexist` flag, so a machine that already had
FileDir never received an updated copy, and a description added in a new version
was never heard — Key Describer answered "no description available" for every
new command. The table is compiled in now. `Hotkeys.ini` is still read first, as
a user override, and the installer deletes the stale shipped copy on upgrade.

## Releasing

```
cd C:\FileDir
BuildFileDir.cmd
git add -A
git commit -m "..."
git push
tagRelease
```

`tagRelease` reads the version from the built installer, tags the commit, and
publishes the release with `FileDir_setup.exe` as its asset. GitHub asset URLs
are case sensitive, so the installer's `OutputBaseFilename` and the name Elevate
Version requests must stay identical.

Commit a new Markdown document before tagging, or its `.htm` will not exist in
the release.

The network check for an already-published version belongs in `tagRelease`, not
in the build. An earlier build script asked GitHub whether a number was taken,
and because `gh` has no timeout a slow network hung the build with no message
and no way to interrupt it. A build script must never wait on the network.

## Keeping the Folder Clean

**Nothing in this project keeps a second list of what belongs.** A file belongs
only if it is *named*, and there are exactly two places to name one:

- **`FileDir_setup.iss`** names it, which means FileDir installs it. A file on
  that list is part of the project by definition.
- **`RepoFiles.txt`** names it, under `Tracked:` if the build needs it, or under
  `Local:` if it lives here without being tracked, like build output and logs.

Doing neither is how a file is kept out. There is no third place to look, and no
pattern admits a file by the look of its name.

That rule comes from EdSharp, where a tidy script carried a line meant to spare
the documentation set:

```
if sPath.endswith((".md", ".htm")) and "/" not in sPath:
    return True
```

What it actually said was that any file at the top of the folder ending in `.md`
or `.htm` is part of the project. Every saved Stack Overflow page and every old
draft ends that way and sits at the top, so the survey declared 38 of them
needed and printed a clean report. `.gitignore` could not help either, because
`.gitignore` has no effect on a file that is already tracked.

`cleanFileDir` looks at every entry at the top level and finds it is one of two
things:

- **It belongs.** The setup script ships it, or `RepoFiles.txt` names it.
- **Nothing claims it.** It moves into `notes\`, whatever kind of file it is --
  a saved web page, a draft, a retired program, a downloaded library, an old
  copy, a test file.

One folder rather than two, on purpose. Sorting the unclaimed material into
kinds is work that only pays if something reads the kinds, and nothing does:
none of it goes into the repository, and going through it is a job for a person
with an afternoon, not for a script. A second ignored folder would be one more
name to remember for no gain.

The folder sits inside the project and is named in `.gitignore`, which the
script adds if it is missing. Nothing is deleted, and subfolders are never
searched: what is inside a folder is that folder's business.

`cleanFileDir` then **untracks** anything git is carrying that the repository
should not hold. Moving a file stages its own removal, because the path it was
tracked under no longer exists, but a file that *stays* and should not be
tracked is untouched by that — build output, a generated source, a log, a
working document. `.gitignore` does nothing for those, because it has no effect
on a file that is already tracked; adding a name to it after the fact changes
nothing at all. Only `git rm --cached` takes one out, and the file stays on
disk. On the working folder that was 10 files.

A working document you want left at the root, but that is neither shipped nor
needed by the build, goes under `Local:` in `RepoFiles.txt` — the same list that
keeps build output and logs in place. There is no separate keep-at-root list,
deliberately: there was one for a day, in two scripts and in neither
`RepoFiles.txt` nor `.gitignore`, which is why git went on tracking two of the
documents it named.

Sorting by what a file is *for* rather than by what its name looks like is the
point. An extension says what a file is made of, and the valid documents share
`.md` and `.htm` with the saved pages, so no pattern on the name can separate
them. A folder can.

Run `cleanFileDir` with no parameters and it does the job. `cleanFileDir
--survey` lists what would move and changes nothing, for when you want to look
first. Both spellings match EdSharp's `moveNotes`, so one habit works in either
project. If a build breaks afterwards, the file it wants is in `notes\` and its
name belongs in `RepoFiles.txt`.

The sweep only runs when you run it, so the audit catches whatever lands at the
root afterwards and does not belong, and warns at the next build. It also warns
about anything git is *tracking* that nothing claims, which is the case
`.gitignore` cannot see.

## Shared With the Other Homer Tools

`homerPolicy.py` is the same file in every Homer Tools project. Nothing in it
names FileDir, so a fix made here can be copied to EdSharp without reading it
first. It finds the project's name from the single `<App>_setup.iss` in the
folder, and everything project-specific comes from that script and from
`RepoFiles.txt`.

`auditFileDir.py` follows the shape of `auditEdSharp.py`: the same `say`,
`report` and `startLog`, the same exit code, the same log beside the script, and
the same handler that puts an unexpected traceback into the log rather than only
on a console that scrolls away. A check written for one project can be moved to
the other by copying the function.

`cleanFileDir.py` does the work that EdSharp splits between `moveNotes.py` and
`tidyRepo.py`. FileDir needed the wider sweep because its clutter is wider: of
296 files at the root, 134 were documents and 162 were programs, libraries and
old copies, so a documents-only mover would have left most of it behind.

## Conventions

**Camel Type** throughout: Hungarian prefixes on variables, lower camel case for
methods and locals, functions rather than subprocedures, one-line simple
conditions, declarations grouped at the top of scope. The authoritative
description is `Camel_Type_C#.md`, which ships with the source.

**Encoding.** Shipped text files are CRLF. Files the program writes are UTF-8
with a byte order mark and CRLF. When editing with Python, read and write with
`newline=""` so CRLF survives.

**Speech supplements the screen reader and never repeats it.** Window titles and
the name, role, state, and value of the focused control are announced by JAWS,
NVDA, and Narrator on their own. FileDir speaks only what it alone knows, and
never sends one message by two mechanisms.

**Documents.** Every Markdown file has a matching `.htm` generated by the build.
Prose in the user-facing documents is written plainly. Lists are preferred to
tables, because a list reads better aloud.

**Logs.** Every script writes a detailed log beside itself, line by line as
things happen, never buffered: the environment, every effective setting, every
command with its exit code, and any error. Each PowerShell script opens its log
before doing anything that could fail and then installs a `trap` that records
the message, the exception type, the line, and the stack trace, so a failure
never produces a console traceback and an empty log.

**Scripts act.** A delivered script does its job when run with no parameters.
Requiring a confirmation word to make it work is a manual step in disguise. Where
a preview is genuinely useful it is an option, not the default, and safety comes
from the design instead: `cleanFileDir` moves rather than deletes, so there is
nothing to guard against.

## The Installer's Optional Components

The finish-page checkbox pattern is EdSharp's, arrived at over many iterations,
and is deliberately not reinvented. The rules it encodes:

- **No Tasks or Components page.** Every optional install is a checkbox on the
  finish page, running a probe-first script that reuses whatever is already
  installed.
- **Each tool appears three times**, with a `Check` function so only one entry
  shows: install, update, or reinstall. They are grouped in that order, so the
  boxes that do something come first, and reinstall is never ticked because
  there is nothing to gain.
- **Every label says what the box will do**, with the version and the size:
  "Install Ollama 0.x", "Update Ollama from A to B", "Reinstall Ollama A
  (current version)". Never "a stronger model" — name it, so a person knows
  what they are getting and what to look for afterwards.
- **Probe behind the progress bar.** `warmComponentProbes` asks every question
  at `ssPostInstall`, where the status line can say what is being checked. Asked
  when the finish page is built, the same queries add up to a silent minute.
- **Nothing pauses.** One Results box at the very end, started from
  `DeinitializeSetup` so it runs after every checkbox, reporting each item by
  name with its version or the exact command to add it later.
- **`runascurrentuser` for the optional installs**: setup is elevated, and
  winget installs into the profile of whoever is signed in.

Two traps specific to the `.iss`, both hit while writing this:

- A Pascal **brace comment ends at the first closing brace**, so a comment
  mentioning an Inno constant ends mid-sentence and hands the rest of its prose
  to the compiler. Comments naming a constant use `//`.
- The line asking Ollama for its model list has **four levels of quoting** on
  it. It is assembled with `Chr(39)` rather than written with doubled
  apostrophes, because writing them literally corrupted it once already.

The audit checks both, plus the `begin`/`end` balance and that every routine
named by a `Check:` or `{code:...}` exists.

## The Media Tools, and a Question Left Open

`exiftool.exe`, `ffmpeg.exe`, `ffprobe.exe` and `yt-dlp.exe` sit in the program
folder, copied from HomerScribe, and `Media.cs` finds them: beside the program
first, then the PATH. Every candidate is run to learn its version and the newest
wins, which is HomerScribe's answer to a question that looking cannot settle --
a winget install alongside an older copy in the build folder. Version parts are
compared as whole numbers, because ExifTool released 13.11 after 13.8.

They are in `RepoFiles.txt` under `Local:` and in `.gitignore`, so they live on
disk without being tracked. Over 100 MB of third-party binaries is not something
a git repository should carry, and the installer ships whichever are present at
build time.

**The open question.** Pandoc moved to `C:\Program Files\Pandoc` precisely so
three programs would not each carry 100 MB of it. ffmpeg is larger than Pandoc,
and HomerScribe carries its own copy, so the same argument applies with more
force. Against that: HomerScribe already works this way, both programs find the
tools identically, and a tool beside the program is the one meant to be used.
Nothing is decided here. If it is decided, the change is small -- `Media.cs`
already searches Program Files for ExifTool, and winget has `Gyan.FFmpeg` and
`OliverBetz.ExifTool`.

## Two Places Define a Key

`menu_Helper("Chat with AI", "F12", handler)` sets the key **shown** beside a
command: in the menu, in Key Describer, in the Alternate Menu, and in the
`Hotkeys.md` the build generates. It does **not** bind anything.

The keystroke is dispatched by the switch in `ProcessCmdKey`, and nothing
connects the two. Change one without the other and the program is confidently
wrong: every place a person looks reads the same argument, so they all agree
with each other and none of them agrees with the keyboard. That is exactly what
happened when the AI commands were added — F12 kept starting the timer while
four separate sources said otherwise.

**Both must be edited together.** The audit checks it, and Enter and
Shift+Enter are the named exceptions, since they branch on what the item is and
call `item_Helper` rather than a menu.

## The Conversion Chain, and How It Is Checked

`Homer.Convert.toPlainText` is the one path all four text commands take -- Say
Contents, Append to Clipboard, Translate File and Chat about File. The order is
deliberate: nothing fundamental depends on a commercial product.

| Source | Read by | Needs |
| --- | --- | --- |
| .txt .md .cs and other text | read directly | nothing |
| .docx .odt .epub .html .rtf .rst .tex .csv | Pandoc | Pandoc |
| .pptx .xlsx | FileDir, via SharpZipLib | nothing |
| .pdf | PyMuPDF4LLM through Python | Python and the package |
| .doc .ppt .xls | 2htm | Office, and System.Memory.dll |

`categoryOf` decides what Output Type offers, and every category must have an
engine that can serve its targets. Two tables that must agree is where the bugs
live: three separate lists once claimed to say what Pandoc reads, and they
disagreed three ways.

**The audit traces this on every build.** It refuses a format routed to an
engine that cannot read it, a document source nothing can read, a category with
no branch, and any claim that Pandoc reads PDF or the Office formats. Add to
that check whenever a format or an engine is added.

## Finding an Installed Tool

`Homer.Media.findInstalled` is the one way to locate an outside program, and it
looks in more places than seems necessary because each was learned from a
failure:

- **Beside FileDir.exe**, so a developer copy always wins.
- **Every folder on PATH** — but PATH alone is never enough. A process inherits
  the environment it was born with, so a program installed after Explorer
  started is invisible to anything Explorer launches.
- **Program Files, and any folder there whose name CONTAINS the tool name.** mpv
  installs into "MPV Player".
- **The user's Programs folder, winget's Links folder, and the Packages folder**
  winget unpacks into, which no PATH mentions.

**Every runnable extension**, not just `.exe`: `.com`, `.cmd` and `.bat` are all
legitimate. A `.cmd` wrapper cannot be started by the process object, so
`needsShell` says when to route through `cmd /c`.

`Homer.Media.searchLog()` returns the whole search, and every failure message
shows it. A "not found" that cannot be diagnosed from its own text costs more
rounds than the bug.

## Loading a Folder

`fillTableFromDir` is the live loader. Three things about it are deliberate and
easy to undo by accident:

- **Attributes, times and lengths come from the enumeration.** Windows returns
  them with each directory entry and `FileSystemInfo` keeps them, so reading
  `fs.Attributes` is free. Calling `File.GetAttributes` per item instead is
  three extra trips to the disk for data already in hand — which is what the
  dead `fillTable` does, and why it is dead.
- **`DisplayFields` is a plain column, not an expression column.** It was an
  expression, evaluated by the DataTable interpreter for every row and again on
  every tag change. `watchDisplayFields` hooks the table once to keep it right;
  do not add the value at each `Rows.Add`, and do not turn it back into an
  expression.
- **The fill is bracketed by `BeginLoadData`/`EndLoadData`**, which suppresses
  row events — hence the single `refreshDisplayFields` pass afterwards.

## The .htm Convention

A file any Homer Tools app **creates** gets `.htm`. Both spellings are read, and
a `.html` file already on disk keeps its name; this is only about what is
written. The audit refuses a conversion target named `.html` and a build that
would generate `.html` documents.

## Namespaces

Two namespaces are in play and they are easy to confuse:

- **`Homer`** — `Util`, `Web`, `Inix` (`InixCodec`), `Say`, `Lbc`, `Ollama`,
  `Convert`, `Media`, `Log`, `Table`. The portable toolkit, shared with EdSharp
  and HomerScribe.
- **`FileDir`** — `FileDir.cs` and `Dialogs.cs` only. The application itself.

A new file in `Homer` refers to its neighbours **unqualified**. Writing
`FileDir.InixCodec` from inside `Table.cs` cost a build; the class is
`Homer.InixCodec` and no prefix was needed.

## Finding an Installed Tool: the Order

`Homer.Media.findInstalled` looks in this order, and the order is the point:

1. **Beside FileDir.exe** — a developer's copy wins.
2. **The named folder the installer uses** — `c_aOfficialFolders`. mpv is "MPV
   Media Player", Pandoc is "Pandoc". A command's name is not its folder's name.
3. **Program Files under the command's own name**, and its `bin`.
4. **The user's Programs folder and winget's Links folder.**
5. **Winget's Packages folder**, searched.
6. **The PATH, last.**

The PATH is last because everything above is a real location on disk, true the
moment an installer finishes, whereas a running process's PATH is only as
current as the moment that process started. FileDir launched from the
installer's finish page proves the point: it starts before winget finishes.

`WM_SETTINGCHANGE` does not fix this — it tells *other* processes to re-read
their environment and cannot help one already running. Add a folder to
`c_aOfficialFolders` when adding a component.

## Component Packages

Every component installs **machine wide, as administrator**. The installer sets
`PrivilegesRequired=admin` and the component scripts inherit that. There is no
per-user fallback: a package that cannot install machine wide reports the
failure, because a silent per-user install is both against the policy and
impossible to find afterwards.

Winget treats `--scope machine` as a requirement, not a preference, so this
works by construction.

**Verify a package id before trusting it.** `winget show --id <id>` — an id that
does not exist returns `-1978335212`, which reads as an error about scope and is
not. `mpv.mpv` was wrong for three releases; the correct id is `shinchiro.mpv`.

**Do not guess an install folder.** mpv creates `MPV Player`, not
`MPV Media Player` or `mpv`. Add the real name to `c_aOfficialFolders` in
`Media.cs`, to `mpvPresent` in the `.iss`, and to `$dOfficial` in
`summarizeSetup.ps1` — all three ask the same question and all three must know.

## A JAWS Script Set Needs Four Files, Not Three

`.jss`, `.jkm`, `.jsb` — **and `.jcf`**. Without the configuration file, JAWS
reports the right application and loads the default settings, so the scripts
never run. Every scripted application in a JAWS settings folder has one; that is
the quickest way to check the claim.

The `.jcf` may be a section header with nothing under it. It exists to be found.

## JAWS Scripts Do Not Take Effect Until JAWS Looks Again

JAWS reads its compiled scripts at startup. A newer `.jsb` in the settings
folder changes nothing for an application JAWS has already seen in that session.

`App.reloadJaws_Helper()` asks a running JAWS to re-read everything, through
`freedomsci.jawsapi` and `RunFunction("ReloadAllConfigs")`. It is called after
every script install. Late binding through reflection, because the type library
exists only where JAWS is installed and FileDir must run without it.

`RunFunction` returns that the call was *scheduled*, not that it finished, so
never treat its answer as proof. Restarting JAWS is the certain cure.

## Traps Worth Knowing

- **Modern packages on .NET Framework 4.8.** Anything whose members are declared
  with `Span` fails the build demanding `System.Memory`, in a way the error
  message does not explain. Check a package's target framework before pinning
  it, and reach it by reflection if in doubt.
- **PowerShell argument lists** are joined with spaces and nothing is quoted, so
  `python -c "import x"` arrives as three words.
- **The command interpreter's quote stripping after `/c`** defeats a quoted
  program with a quoted argument. Do not fight it: Inno's `Exec` and .NET's
  process object both take the program and its arguments separately, and then no
  quoting rule applies.
- **A file shipped with `onlyifdoesntexist` never reaches a machine that already
  has the program.** This is what `KeyMap.cs` exists to work around.
- **JAWS scripts.** The `.jsb` files can only be recompiled by JAWS on Windows,
  so the shipped `.jss` and `.jsb` can drift apart. Prefer keymap-only fixes.
  `FileDir.jkm` binds about 270 keys directly to `TypeCurrentScriptKey`, which
  forces JAWS to pass the key through to the application. That is robust and
  needs no recompile, and it sidesteps the fragile window-class detection that
  cost EdSharp several rounds.
- **The monolithic click handler.** Block-local variables are safe; method-level
  variables must be assigned rather than re-declared, and an early `return;`
  inside it is a trap. Wrap success paths in `if`.
- **Verify an edit by reading the file back.** An edit script that hits an
  assertion and exits leaves the file untouched while the transcript says
  otherwise.

## Known Open Items

None of these blocks a release.

- Single-instance behaviour still uses `WindowsFormsApplicationBase` from
  `Microsoft.VisualBasic`. Moving to a mutex with inter-process communication is
  deferred, as it was in EdSharp.
- Two compiler warnings, CS0219, for `sUserName` and `sPassword` assigned but
  never used. They are the remains of a credential path worth a look.
- The version history for 5.0.1 through 5.0.14 is not recorded in `History.md`.
- A true .NET 8 port is a separate project, not a quick win. WinForms is not
  compatible with NativeAOT, so a single native binary is not achievable, and
  the JScript .NET evaluator would have to be replaced because `jsc.exe` exists
  only on .NET Framework. The installer's `ngen` step already gives most of the
  practical startup benefit.

End of Document
