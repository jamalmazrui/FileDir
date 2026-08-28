# urlFido — User Guide

**Version 1.0.0**
July 2026
Copyright (c) 2026 Jamal Mazrui
MIT License — <https://github.com/JamalMazrui/urlFido>

## Contents

- [Introduction](#introduction)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [The Parameter Dialog](#the-parameter-dialog)
- [Dialog Editing Keys](#dialog-editing-keys)
- [Source Lists](#source-lists)
- [Command-Line Options](#command-line-options)
- [Extensions and Wildcards](#extensions-and-wildcards)
- [How Downloading Works](#how-downloading-works)
- [Authenticated Pages and the Main Profile](#authenticated-pages-and-the-main-profile)
- [File Names](#file-names)
- [Configuration and Logging](#configuration-and-logging)
- [Results Summary](#results-summary)
- [Hotkey Summary](#hotkey-summary)
- [Saved Settings](#saved-settings)
- [Requirements](#requirements)
- [Releasing](#releasing)
- [Development Notes](#development-notes)
  - [A note on spelling](#a-note-on-spelling)

## Introduction

urlFido downloads files of the types you choose — PDF by default — from web pages. You give it one or more source pages and a list of extensions; for each page, urlFido finds every linked file whose extension matches and saves it into your output directory.

Unlike a plain HTTP downloader, urlFido drives your installed Microsoft Edge browser to open each page, so the page behaves exactly as it would for a person sitting at the browser: scripts run, cookies apply, and links that only appear after the page finishes loading are still found. This substantially improves the odds that a file is actually reachable, especially on sites that require sign-in or that build their link lists with JavaScript.

urlFido runs in two modes from a single executable:

- **GUI mode** presents an accessible dialog with labeled fields and checkboxes. This is what the desktop shortcut and Start Menu entry launch.
- **CLI mode** takes command-line options, so a workflow you prototype in the dialog translates directly into a batch file or script.

## Installation

Run urlFido_setup.exe and follow the prompts. The installer:

- Installs to C:\Program Files\urlFido by default (you can change this).
- Adds a Start Menu group with urlFido, the ReadMe, and an uninstaller.
- Adds a desktop shortcut with the hotkey **Alt+Control+U**, which opens the parameter dialog from anywhere in Windows.
- Requires 64-bit Windows 10 or later.

urlFido needs the .NET Framework 4.8, which ships with Windows 10 version 1903 and later and with Windows 11, so no separate runtime install is normally required. It also needs Microsoft Edge, which is built into current Windows.

On uninstall, your saved settings file (urlFido.inix, described below) is left in place; delete it by hand if you want urlFido gone completely.

## Quick Start

To see urlFido work on a real page, try the Information Technology Industry Council's VPAT page. ITI publishes the Voluntary Product Accessibility Template — the template that becomes an Accessibility Conformance Report — in several editions, each a separate Word document, alongside a PDF on reporting conformance. It is a stable, widely referenced technical resource, which makes it a good first target:

```
urlFido -o "C:\Downloads\VPAT" https://www.itic.org/policy/accessibility/vpat
```

With the default extensions (`docx pdf zip`) that fetches the VPAT editions and the accompanying PDF in one pass. To take only the Word templates:

```
urlFido -e docx -o "C:\Downloads\VPAT" https://www.itic.org/policy/accessibility/vpat
```

Or, using a wildcard, only the edition covering the Web Content Accessibility Guidelines — matching on the file name the server supplies:

```
urlFido -e "*WCAG*.docx" -o "C:\Downloads\VPAT" https://www.itic.org/policy/accessibility/vpat
```

To do the same through the dialog, press **Alt+Control+U**, paste the address into Source urls, set the output directory, and press Enter.

A note on what you will see: the download links on that page end in long identifiers rather than readable names. urlFido asks the browser for the name the server recommends, so the files usually land with meaningful names — but where a server declines to supply one, you will get the identifier with the right extension. That is the same behavior described under [File Names](#file-names).

### Other pages worth trying

**The Unicode Standard.** Each version of the standard is published as a set of chapter PDFs plus a single combined volume — a genuine document collection, and a good demonstration of fetching many files at once:

```
urlFido -e pdf -o "C:\Downloads\Unicode" https://www.unicode.org/versions/Unicode15.0.0/
```

**A page you already have.** Point urlFido at any local page you have saved, and it will fetch the files that page links to:

```
urlFido -e pdf -o "C:\Downloads" "C:\Saved\research page.htm"
```

**Only what you want.** Take the newsletters and nothing else, then open the folder when finished:

```
urlFido -e "*newsletter*.pdf" -o "C:\Downloads" --view-output https://example.com/archive
```

**Behind a sign-in.** Pause so you can authenticate, then download:

```
urlFido -a -o "C:\Downloads" https://example.com/members/documents
```

### If a page yields nothing

Some pages carry no direct file links at all. A government or news home page, for instance, is mostly navigation — its documents live a level or two deeper, on the section or publication pages. urlFido reads the page you give it and does not follow links to other pages, so aim it at the page that actually lists the documents rather than at the site's front door.

urlFido tells you what it found: how many links were on the page, how many matched, and how many carried no file extension in their address. If that last number is large and the match count is zero, try `-p` — the files may be behind addresses that reveal nothing until the server is asked.

## The Parameter Dialog

The dialog is reached by launching urlFido with no arguments from a GUI shell (double-click, Start Menu, or the Alt+Control+U desktop hotkey), or explicitly with `-g`. It is built from the shared Layout-by-Code (Lbc) classes, so it behaves exactly like the dialogs in DbDo, EdSharp, and FileDir — see [Dialog Editing Keys](#dialog-editing-keys).

Fields, in tab order:

1. **Source urls** — one or more urls, local web pages, or url-list text files, separated by spaces. Put double quotes around any single item that contains a space. See [Source Lists](#source-lists). When empty, the field offers `acb.org afb.org nfb.org wbu.ngo` — the American Council of the Blind, the American Foundation for the Blind, the National Federation of the Blind, and the World Blind Union — so there is something real to run against straight away.
2. **File extensions** — what to download, separated by commas or spaces. See [Extensions and Wildcards](#extensions-and-wildcards). The default is `docx pdf zip`.
3. **Output directory** — where downloaded files are saved. Leave it empty for the current directory. If you type a folder that does not exist and press OK, urlFido offers to create it.
4. **Authenticate credentials** — pause at the first page of each site so you can sign in. See [Authenticated Pages](#authenticated-pages-and-the-main-profile).
5. **Main profile** — use your real Edge profile so existing logins apply. Requires Edge to be closed.
6. **Invisible browser** — run Edge with no visible window. Ignored when Authenticate is on, since you need to see the window to sign in.
7. **Force overwrite** — overwrite existing files instead of skipping ones that are already present.
8. **View output** — open the output directory in File Explorer when finished.
9. **Log session** — write a diagnostic urlFido.log to the output directory.
10. **Use configuration** — load settings at startup and save them when you press OK.

Every field carries a focus tip describing it; press **Shift+F1** to hear the tip for the field you are on.

Buttons: **OK** runs; **Browse source** opens a file picker for a local HTML page or url-list file; **Choose output** opens a folder picker; **Default settings** resets every field and erases the saved configuration; **Cancel** closes without running; **Help** (Alt+H or F1) describes every field with its tip plus the universal dialog keys. Each button is reachable by Alt plus its first letter.

The two picker buttons close the dialog, run the picker, and reopen the dialog with the chosen value filled in and everything else you typed preserved.

While a run is in progress, urlFido shows a small status window. Its status line reports each step — launching Edge, opening the page, matching links, and then each file as it downloads, with a running count and percentage. The line is exposed as a status bar, so a screen reader announces it as it changes and the JAWS read-status-bar command, Insert+PageDown, reads it on demand. **Cancel** stops after the current file.

As in 2htm and extCheck, the count reflects work already finished rather than the item just started, so you never see 100% while the last file is still arriving.

When the run finishes in GUI mode, a message box shows the structured results summary, and a short spoken line gives the outcome without your having to read the whole box.

### Dialog Editing Keys

Because the dialog is built from Lbc primitives, every text field offers the shared editing and review conveniences:

| Key | Action |
| --- | --- |
| Control+Enter | OK, from any control in the dialog |
| Control+C | Copy the selection, or the current line when nothing is selected |
| Alt+C | Copy Append: add the selection or line to the clipboard |
| Control+X | Cut the selection, or the current line |
| Alt+X | Cut Append |
| Control+D | Delete the current line |
| Control+A | Select All |
| Control+Shift+A | Unselect All |
| F8 / Shift+F8 | Start / Complete Selection |
| Control+F8 | Copy All |
| Alt+F8 | Read All |
| Alt+Y | Say Yield: line and character counts |
| Alt+Apostrophe | Say Clipboard |
| Shift+F1 | Say the focus tip for this field |

## Command-Line Options

```
urlFido [options] <url, local html file, or url-list text file> [...]
```

- `-e, --extensions <list>` — what to download, comma- or space-separated. Bare extensions and cmd.exe wildcard patterns both work: `pdf`, `.pdf`, `*.pdf`, `*newsletter*.pdf`. Case-insensitive. Default: `docx pdf zip`.
- `-o, --output-folder <dir>` — output directory. Default: the current directory.
- `-g, --gui-mode` — show the parameter dialog.
- `-f, --force` — overwrite existing files instead of numbering.
- `-i, --invisible` — run Edge headless (no visible window).
- `-a, --authenticate` — pause at the first url of each site to sign in. Overrides `-i`.
- `-m, --main-profile` — use your real Edge profile. Requires Edge closed.
- `-u, --use-configuration` — load and save settings in urlFido.inix. See [Saved Settings](#saved-settings).
- `-l, --log` — write urlFido.log to the output directory. Appends across runs; combine with `-f` to replace the previous log instead.
- `--view-output` — open the output directory when done.
- `-h, --help` — show help. `-v, --version` shows the version.

Command-line values take precedence over saved configuration for any option you specify explicitly.

## Source Lists

A source can be any of three things, and they can be mixed freely in one run:

- **A url**, with or without a scheme. `example.com/docs` and `https://example.com/docs` mean the same thing.
- **A local web page** — an `.htm`, `.html`, or `.xhtml` file on disk. urlFido opens it and downloads the files it links to, so a page you saved earlier can be harvested later.
- **A text file listing one url per line.** This is the form worth knowing about for repeated work: keep the pages you regularly harvest from in a text file, and hand urlFido the file instead of retyping the addresses.

A list file looks like this:

```
# Accessibility templates and standards
https://www.itic.org/policy/accessibility/vpat
https://www.unicode.org/versions/Unicode15.0.0/

; This one is on hold for now
; https://example.com/archive
example.org/reports
```

Blank lines are ignored. A line starting with `#` or `;` is a comment, so a list can be annotated and entries can be commented out rather than deleted. Each line is interpreted exactly as if you had typed it, so bare domains work, and a line naming a local `.htm` file is accepted too.

Any existing file that is not a web page is read as a list, so `.txt`, `.lst`, `.urls`, or no extension at all all behave the same — there is no required extension to remember. urlFido reports how many urls it read, and names any line it could not use.

## Extensions and Wildcards

The File extensions field accepts a list separated by commas or spaces. Each entry is a **cmd.exe-style wildcard pattern**, and the short forms you are used to are simply shorthand that escalates to a full pattern:

```
pdf     is short for   .pdf     which is short for   *.pdf
```

So the familiar `pdf, docx` still works and means exactly what it always meant. Because the entry is a pattern, you can also be more specific:

```
*newsletter*.pdf     only PDFs whose name contains "newsletter"
2026*.pdf            only PDFs whose name starts with 2026
report?.xlsx         report1.xlsx, reportA.xlsx -- one character for the ?
*.pdf, *minutes*.doc combine as many patterns as you like
```

Only the two command-prompt wildcards are supported: `*` matches any run of characters and `?` matches exactly one. Unix glob syntax such as character classes or brace expansion is deliberately not supported, to keep the field predictable.

**Matching ignores case**, so `*Newsletter*.PDF` and `*newsletter*.pdf` behave identically.

### How urlFido knows what a link is

Most links announce themselves: the address ends in a file name, and that name is all urlFido needs. Matching those costs nothing.

Some links say nothing. `example.com/download?id=42` carries no extension, and only the server knows a PDF is on the way. Working that out is the point of the program, so urlFido always does it — there is no setting to turn it on.

It is made quick rather than optional. Links that could not yield a file are ruled out by inspection first: non-web addresses, bare site addresses, paths ending in a slash, and anything ending in a page extension. Links differing only by the `#` part are one address. What remains is asked about in parallel rather than one at a time, and each question is a headers-only request, so a server replying "this is a web page" settles the matter without sending the page. Answers are remembered, so an address linked five times is asked about once.

The status line reports progress while this happens, with a count and percentage, so a slow site never leaves you wondering whether anything is happening.

Patterns are matched against the file name, not the raw url. urlFido works that name out the same way FileDir's Web Download command does: it uses the last part of the url when that is a real file name, and otherwise asks the server, so links like `example.com/download?id=42` still get a proper name and extension and can still be matched.

## How Downloading Works

For each source, urlFido launches Edge with a fresh temporary profile, and takes some care to make that profile inert: extensions, component updates, background networking, sign-in, and sync are all switched off. Edge would otherwise sign a new profile into your Windows account and begin syncing it, which is reasonable for a browser you are adopting and quite wrong for a throwaway profile that exists to fetch a file.

Only the page being scanned is opened. Files are retrieved with a direct request that replays the browser's own cookies and identity, so nothing is downloaded that the browser could not have downloaded — but no extra tab appears for each file. When a direct request will not do, urlFido falls back to asking the browser itself. (With **Main profile** that suppression is lifted, since the point of that option is to reproduce your real browsing environment.) It opens the page in Edge, waits for it to finish loading and for network activity to settle, and then reads every link on the page (anchors, image-map areas, and embedded frames or objects). It keeps the links whose file extension matches your list — comparing only the file-name part of the url, ignoring any query string or fragment — and downloads each one.

Downloads are handled by the browser itself wherever possible, so any cookies or session established while loading the page apply to the download too. PDFs are downloaded to disk rather than opened in Edge's built-in viewer. If a particular link cannot be downloaded through the browser, urlFido falls back to a direct download that replays the browser's cookies and identity, so authenticated files still come through.

If a source url is itself a matching file (for example, you pass a direct link to a PDF), urlFido downloads it directly.

## Authenticated Pages and the Main Profile

Some pages only reveal their files after you sign in, accept a cookie banner, or dismiss a prompt. Two options help:

- **Authenticate** (`-a`): the first time urlFido visits a given site in a run, it pauses after the page loads and shows a prompt. Switch to the visible Edge window (Alt+Tab), do whatever the site needs — sign in, accept cookies, complete two-factor — and then return to urlFido and confirm. urlFido then continues, and reuses that session for other pages on the same site during the same run. To give sign-in the best chance of success on sites that resist automation, urlFido briefly detaches its automation channel from Edge while you authenticate and reattaches afterward.
- **Main profile** (`-m`): instead of a fresh, empty browser session, urlFido uses your real Edge profile, so sites where you are already signed in stay signed in. Because Windows will not let two Edge processes share one profile, **all Edge windows must be closed first** — check the system tray too. urlFido checks at startup and stops with a clear message if Edge is running.

A note on Main profile: recent versions of Edge may refuse to expose their automation channel when running against the default profile for security reasons. If that happens, urlFido detects it and suggests using Authenticate instead, which signs you in within a clean temporary profile.

## File Names

urlFido names each downloaded file using the shared web-download helper that FileDir's Web Download command uses. The name comes from the last part of the url when that is a proper file name. When the url does not end in a usable file name — as with `example.com/download?id=42` — urlFido asks the server, taking the name from the Content-Disposition header or deriving the extension from the MIME type, and falls back to a sanitized name built from the url itself. Ordinary links that already end in a file name cost no extra network request.

If a file of the same name is already in the output directory, urlFido reports it as skipped and leaves it alone. Check **Force overwrite** to replace it instead.

## Configuration and Logging

With **Use configuration** (`-u`), urlFido reads its settings at startup from, and saves them on OK to:

```
%LOCALAPPDATA%\urlFido\urlFido.inix
```

The `.inix` format is the shared superset of the classic `.ini` file used across these tools: it round-trips in order and preserves any comments or extra sections you add by hand, so the file stays safe to edit yourself. urlFido uses the plain section-and-key subset, under a single `[Settings]` section, with `y`/`n` for the checkboxes.

Without that option, urlFido leaves no settings on disk. The **Default settings** button erases this file and the urlFido folder.

With **Log session** (`-l`), urlFido writes a fresh urlFido.log to the output directory. The log is deliberately detailed, because the interesting failures happen inside a browser you may not be watching. It records the resolved parameters and environment; the exact Edge command line, profile directory, and how long the automation channel took to come up; every link harvested, with the file name it was matched against and whether it matched; each download attempt, which method served it, the HTTP status and content type when the fallback is used, the byte count and elapsed time; and every status line shown in the progress window. Each run truncates any previous log.

If something goes wrong, that file is the thing to send.

## Results Summary

At the end of a run urlFido prints a structured summary, following the same convention as 2htm and extCheck. It has up to three sections — **Downloaded**, **Failed to download**, and **Skipped** — and each appears only when its count is not zero, using "file" for one and "files" for more. Failures read as `name: reason`. A closing line names the output directory.

In GUI mode the summary is what the final message box shows, and the file names are listed under each heading. In command-line mode the names have already scrolled past during the run, so the headings appear without repeating the lists.

A file is reported as **Skipped** when a file of the same name is already in the output directory; check **Force overwrite** to replace it instead.

## Hotkey Summary

| Key | Action |
| --- | --- |
| Alt+Control+U | Open the urlFido parameter dialog (desktop shortcut) |
| Enter | OK (run) |
| Control+Enter | OK, from any control |
| Esc | Cancel |
| Alt+B | Browse source |
| Alt+C | Choose output |
| Alt+D | Default settings |
| Alt+H or F1 | Help |
| Shift+F1 | Say the focus tip for the current field |

The text-field editing keys are listed under [Dialog Editing Keys](#dialog-editing-keys).

## Saved Settings

urlFido can remember the dialog's contents between runs, in `urlFido.inix` under `%LOCALAPPDATA%\urlFido`. Tick **Use configuration** and press Enter; the next time the dialog opens, it comes back the way you left it.

The behavior differs between the two modes, deliberately:

- **In the dialog**, an existing settings file is loaded automatically. You do not need to pass anything on the command line — ticking the box once is enough, and it stays that way.
- **On the command line**, `-u` is required. A script picks up no state it did not ask for, so a command means the same thing wherever it runs.

Clearing the checkbox deletes the stored settings. That matters because of the automatic loading above: if the file were left behind, the next run in the dialog would find it and load it again, and the setting could never be switched off.

Anything given on the command line wins over a saved value, so `urlFido -e zip` fetches zip files this once without disturbing what you saved.

Without this option urlFido leaves no trace of its own beyond the files you asked it to download.

## Requirements

- 64-bit Windows 10 (version 1903 or later) or Windows 11.
- .NET Framework 4.8 (included with the above).
- Microsoft Edge (included with current Windows).

urlFido.exe is self-contained: there are no accompanying DLLs to install or keep track of.

NVDA support deserves a note. urlFido speaks to NVDA through `nvdaControllerClient.dll`, a native library that NVDA does not include in its end-user installation, so it cannot be borrowed from your NVDA folder. It is embedded inside `urlFido.exe` instead. The first time you run urlFido **while NVDA is running**, that library is written once to `%LOCALAPPDATA%\urlFido` and loaded from there; afterwards the existing copy is reused. If you use JAWS, Narrator, or SAPI, the file is never written at all, and urlFido leaves no footprint of its own. Should anything about that process fail, urlFido carries on and simply uses another speech path.

## Releasing

urlFido is published with the shared `tagRelease` script, which takes the app name from the directory, finds `urlFido_setup.iss`, reads the version stamped into `urlFido_setup.exe`, tags `v<version>`, and uploads the installer as a release asset. The order matters:

1. `buildUrlFido.cmd` — produces `urlFido.exe`.
2. Compile `urlFido_setup.iss` in Inno Setup — produces `urlFido_setup.exe` in the repo root and stamps the version resource that the release is named from.
3. `git add -A`, commit, push.
4. `tagRelease.cmd`.

The repository directory must be named `urlFido`, and an `origin` remote must be configured, since the script derives both the app name and the owner/repo from them. Bump `AppVer` in `urlFido_setup.iss` and `sProgramVersion` in `urlFido.cs` together — nothing synchronizes them automatically.

The published installer is then always available at a stable address:

```
https://github.com/JamalMazrui/urlFido/releases/latest/download/urlFido_setup.exe
```

## Development Notes

### A note on spelling

Throughout urlFido — the dialog, the messages, the command-line help, and this guide — **url** is written in lower case, as an ordinary English word rather than an acronym. It is treated the way radar, laser, and lidar are treated: terms that began as initialisms and have since been absorbed into the language. Most people who use urls every day could not say what the three letters stand for, and have no need to.

The upper-case form appears only where an outside convention requires it: an Inno Setup directive such as `AppPublisherURL`, or a Camel Type identifier such as `sUrl` in the source. Please leave the lower-case spelling as it stands; it is deliberate, not an oversight.


urlFido is built to a single 64-bit executable from six C# sources compiled together, with the NVDA controller client embedded as a managed resource: `urlFido.cs` plus the shared Homer modules `Lbc.cs` (Layout-by-Code dialog classes), `Say.cs` (screen-reader announcements), `Inix.cs` (the `.ini`/`.inix` codec), `Util.cs` (general utilities), and `Web.cs` (web-download helpers). The Homer modules are copied in unmodified from the shared toolkit, so improvements made in DbDo, EdSharp, or FileDir port to urlFido by replacing the file. It drives Edge over the Chrome DevTools Protocol using only .NET Framework classes (`System.Net.WebSockets.ClientWebSocket` for the connection and `System.Web.Script.Serialization.JavaScriptSerializer` for JSON), which is the same wire protocol Playwright uses — so urlFido reproduces the browser-driving behavior proven out in its companion tool urlCheck without shipping a Node.js driver or a bundled browser.

Build it with buildUrlFido.cmd, which locates the highest-level Roslyn C# compiler installed with Visual Studio 2017 or later (the pre-Roslyn compiler that ships with the .NET Framework cannot build it) and writes the full compiler output to `buildUrlFido.log` as well as the console. Produce the installer by opening urlFido_setup.iss in Inno Setup and clicking Compile.

The icon is original artwork generated by script rather than sourced from a stock library, so there is no third-party license attached to it. `urlFido.png` in the repository is the 1024-pixel master; `urlFido.ico` carries the 16, 24, 32, 48, 64, 128, and 256 pixel sizes, and is embedded into the executable at build time.

urlFido shares identifier conventions with its companion tools urlCheck, extCheck, and 2htm: `sProgramName` and `sProgramVersion` for identity; `sConfigDirName`, `sConfigFileName`, and `sLogFileName` for persistence; `sOutputDir` for the output directory; and the `logger` surface `open`, `close`, `info`, `warn`, `error`. All names follow the project's Camel Type coding style.
