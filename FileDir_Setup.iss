; FileDir_setup.iss -- Inno Setup script for AnyCPU FileDir 5.0 (x64 and ARM64).
;
; Compile with ISCC.exe (Inno Setup 5.6+ or 6.x). Run BuildFileDir.cmd first
; so FileDir.exe exists. Produces FileDir_setup.exe in C:\FileDir.
;
; OutputBaseFilename is FileDir_setup so the Elevate Version (F11) command can
; fetch releases/latest/download/FileDir_setup.exe -- GitHub asset URLs are
; case-sensitive, so this name and the F11 asset name must stay identical.
;
; This is a slimmed, 64-bit replacement for the old FileDir_setup.iss. Removed:
; the Java / JRE detection block (Java Access Bridge), the .NET 2.0/4.0 probing,
; the dead 2015 filter-pack / calibre / .NET download links, the dotnet.exe
; report, PSetup/unicows, and the fragile PostHotkey/CurStepChanged shortcut
; hack. The single Alt+Control+F hot-key shortcut now follows the DbDo/EdSharp
; model, with an [InstallDelete] that clears the legacy desktop shortcut first.
; Native code generation via ngen is kept (installer-time, elevated).
;
; Interim notes (resolved by later modernization steps):
;  - Text extraction uses 2htm.exe (plain-text mode). The old gettext.exe and
;    the filters\ DLLs it drove are retired and no longer shipped.
;  - JAWS scripts install via "FileDir.exe --install-jaws-settings" (shared
;    Homer.JawsSettingsInstaller), the same way DbDo and EdSharp do it.  The old
;    Scripts\FileDir_Scripts_setup.exe is retired.
;  - Speech goes through Homer.Say (JAWS, NVDA, then a UIA notification that
;    Narrator announces).  The old 32-bit saapi32/nvdaControllerClient32 DLLs
;    and the Web Client Utilities tree are no longer shipped, and are deleted
;    from existing installs.

; ---- Version -----------------------------------------------------------------
; The version number is NOT stored in this script.  It lives in version.txt, one
; line, which Build<App>.cmd increments on every build.  Inno reads it here, and
; Build<App>.cmd also generates Version.cs from it, so the program, the installer,
; and the release tag always report the same number -- which is what Elevate
; Version (F11) compares.  Because no version literal appears in this file, a
; stale copy of it can never rewind the version.
#define VerFile FileOpen(AddBackslash(SourcePath) + "version.txt")
#define AppVersion Trim(FileRead(VerFile))
#expr FileClose(VerFile)
#undef VerFile

[Setup]
AppName=FileDir
AppVersion={#AppVersion}
AppVerName=FileDir {#AppVersion}
VersionInfoVersion={#AppVersion}
AppPublisher=NonvisualDevelopment.org
AppPublisherURL=https://github.com/JamalMazrui/FileDir
AppContact=Jamal Mazrui
AppCopyright=Copyright 2006-2026 by Jamal Mazrui
UninstallDisplayIcon={app}\FileDir.exe
SetupIconFile=FileDir.ico
DefaultDirName={autopf}\FileDir
DefaultGroupName=FileDir
; x64compatible matches both x64 and ARM64 (Inno Setup 6.3+), so the AnyCPU
; FileDir.exe installs and runs natively on both. MinVersion 10.0 matches the
; .NET Framework 4.8 / Windows 10+ requirement.
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
Compression=lzma2/max
SolidCompression=yes
OutputBaseFilename=FileDir_setup
OutputDir=C:\FileDir
SourceDir=C:\FileDir
PrivilegesRequired=admin
ChangesAssociations=yes
ChangesEnvironment=yes
DisableProgramGroupPage=yes
DisableStartupPrompt=yes
Uninstallable=yes
SetupLogging=yes

[Files]
; Built artifact (present after BuildFileDir.cmd).
Source: "FileDir.exe";        DestDir: "{app}"; Flags: ignoreversion
; Runtime configuration: startup tuning (disables Authenticode publisher-evidence
; / CRL check at launch, enables concurrent GC). Must sit next to FileDir.exe;
; ignoreversion keeps it in sync with the executable.
Source: "FileDir.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "FileDir.ico";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; Referenced assemblies still loaded at run time (until the Homer port retires them).
Source: "Tektosyne.dll";      DestDir: "{app}"; Flags: ignoreversion
; Ude.dll: character-encoding autodetection (a port of the Mozilla universal
; detector).  The .NET base class library cannot detect an encoding, and this is
; what the retired Encoding.exe did.  EdSharp ships the same library.
Source: "Ude.dll";            DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "ICSharpCode.SharpZipLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "FileAssociation.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source and build inputs (shipped so users can recompile, EdSharp-style).
Source: "FileDir.cs";         DestDir: "{app}"; Flags: ignoreversion
Source: "Web.cs";             DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Say.cs";             DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Inix.cs";             DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Util.cs";            DestDir: "{app}"; Flags: ignoreversion
Source: "KeyMap.cs";          DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Dialogs.cs";         DestDir: "{app}"; Flags: ignoreversion
; Lbc.cs is compiled into FileDir.exe alongside the others and was the one
; source the installer never shipped, so the source that came with the program
; could not be rebuilt from what was there.
Source: "Lbc.cs";             DestDir: "{app}"; Flags: ignoreversion
; The Ollama client, used by Translate File. Shared in shape with the other
; Homer Tools, which talk to the same local server and the same models.
Source: "Ollama.cs";          DestDir: "{app}"; Flags: ignoreversion
; The Pandoc conversion class, shared in shape with EdSharp and HomerScribe,
; which drive the same machine-wide Pandoc.
Source: "Convert.cs";         DestDir: "{app}"; Flags: ignoreversion
; Finding and running ExifTool, ffmpeg and ffprobe. Adapted from HomerScribe,
; which solved the finding; what each program does with the tools differs.
Source: "Media.cs";           DestDir: "{app}"; Flags: ignoreversion
; The session log, in the same place and naming as EdSharp's, beside the setup
; log this installer writes.
Source: "Log.cs";             DestDir: "{app}"; Flags: ignoreversion
Source: "FileDirScript.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.js";         DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.manifest";   DestDir: "{app}"; Flags: ignoreversion

; JAWS script family, installed into each JAWS version by
; "FileDir.exe --install-jaws-settings" (the [Run] checkbox below) and
; removed by the [UninstallRun] entry. FileDir.jss says Use "Homer.jsb",
; so Homer.jss ships too and compiles first; Homer.jsh and MSAA.jsh are
; include headers its compile needs beside it. The .jsd files document
; the scripts inside JAWS; FileDir.jcf carries configuration defaults.
Source: "FileDir.jss";        DestDir: "{app}"; Flags: ignoreversion
Source: "FileDir.jkm";        DestDir: "{app}"; Flags: ignoreversion
Source: "FileDir.jsd";        DestDir: "{app}"; Flags: ignoreversion
Source: "FileDir.jcf";        DestDir: "{app}"; Flags: ignoreversion
Source: "Homer.jss";          DestDir: "{app}"; Flags: ignoreversion
Source: "Homer.jsd";          DestDir: "{app}"; Flags: ignoreversion
Source: "Homer.jsh";          DestDir: "{app}"; Flags: ignoreversion
Source: "MSAA.jsh";           DestDir: "{app}"; Flags: ignoreversion
Source: "BuildFileDir.cmd";   DestDir: "{app}"; Flags: ignoreversion
Source: "BuildFileDir.ps1";   DestDir: "{app}"; Flags: ignoreversion
Source: "cleanFileDir.cmd";   DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "cleanFileDir.py";    DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "auditFileDir.py";    DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; Optional local AI. One Ollama installation and one set of models serve every
; Homer Tools program on the machine, so these scripts probe before they fetch.
; Pandoc, machine-wide in C:\Program Files\Pandoc. About 100 MB, and one copy
; serves FileDir, EdSharp and HomerScribe -- which is why it is installed there
; rather than inside each program's own folder.
Source: "installPandoc.cmd";  DestDir: "{app}"; Flags: ignoreversion
; The PDF reader, EdSharp's arrangement kept identical: PyMuPDF4LLM through
; Python, which reads a PDF's own structure into Markdown with headings, lists
; and tables. No Microsoft Word anywhere in it.
Source: "installPdfTools.cmd"; DestDir: "{app}"; Flags: ignoreversion
; mpv, the player Play List hands its list to. Not ticked: it carries its own
; copy of ffmpeg, which FileDir already has, so much of the download is a
; second copy of something already installed. It buys playback and nothing
; else -- conversion is ffmpeg's job and stays ffmpeg's job.
Source: "installMpv.cmd";     DestDir: "{app}"; Flags: ignoreversion
Source: "pdfRich.py";         DestDir: "{app}"; Flags: ignoreversion
Source: "installOllama.cmd";  DestDir: "{app}"; Flags: ignoreversion
Source: "installTranslateModel.cmd"; DestDir: "{app}"; Flags: ignoreversion
; The single Results box, shown after every finish-page checkbox has run.
Source: "summarizeSetup.cmd"; DestDir: "{app}"; Flags: ignoreversion
Source: "summarizeSetup.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "makeKeyMap.py";      DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; homerPolicy.py is the same file in every Homer Tools project. Both the audit
; and the sweep read it, so they cannot disagree about what belongs.
Source: "homerPolicy.py";     DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "RepoFiles.txt";      DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir_setup.iss";  DestDir: "{app}"; Flags: ignoreversion
; Helper tools shipped alongside the app.
Source: "7z.*";               DestDir: "{app}"; Flags: ignoreversion
Source: "chimes.wav";         DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Burn2CD.exe";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Burn2CD.dll";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "AssocOn.exe";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "AssocOff.exe";       DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; Text-extraction engine: 2htm (plain-text mode) replaces gettext.exe + filters\.
Source: "2htm.exe";           DestDir: "{app}"; Flags: ignoreversion
; 2htm needs System.Memory beside it on .NET Framework 4.8 -- the Span trap.
; Without it, it fails on EVERY file and still exits with code 0, so callers
; that trusted the exit code got silence. Shipped when present.
Source: "System.Memory.dll";  DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; ---- Media tools ----
;
; ExifTool, ffmpeg with ffprobe, and yt-dlp are NOT shipped here. Together they
; are well over 100 MB, and EdSharp, HomerScribe and FileDir all want them --
; exactly the argument that moved Pandoc to Program Files. They are offered as
; one finish-page checkbox instead, installed machine wide by winget.
;
; Media.cs still looks in the program folder first, so a copy dropped into
; C:\FileDir during development is used in preference to anything installed.
Source: "installMediaTools.cmd"; DestDir: "{app}"; Flags: ignoreversion
; NVDA direct speech.  Homer.Say P/Invokes nvdaControllerClient.dll -- the
; 64-bit build, with no architecture suffix.  The 32-bit nvdaControllerClient32.dll
; that older releases shipped cannot load in this process at all.  When the DLL is
; absent, speech falls back to the UIA notification, which NVDA does read, so this
; is a quality improvement rather than a requirement.
Source: "nvdaControllerClient.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; The 32-bit client is useless in a 64-bit process; remove it from an upgraded install.

; JAWS settings family.  FileDir installs these itself via --install-jaws-settings;
; the old FileDir_Scripts_setup.exe is no longer used and is deleted on upgrade.
; Only the compiled JAWS scripts.  This used to be Scripts\*, the whole folder,
; which shipped the retired FileDir_Scripts_setup.exe and its .iss -- and the
; [InstallDelete] below then removed the .exe, so the installer put a file in
; place and took it out again in the same run.
Source: "Scripts\*.jsb";      DestDir: "{app}\Scripts"; Flags: ignoreversion skipifsourcedoesntexist
; Configuration: do NOT clobber a user's existing settings on upgrade.
Source: "FileDir.ini";        DestDir: "{app}"; Flags: onlyifdoesntexist
; Hotkeys.ini is deliberately NOT shipped.  The key and description of every
; command are compiled into the program (KeyMap.cs, generated from Hotkeys.ini at
; build time).  Shipping the file with onlyifdoesntexist meant an existing
; installation kept its old copy for ever and never saw a new description; the
; [InstallDelete] below removes that stale copy.  A user who wants to override an
; entry can still create Hotkeys.ini in the program folder, and it is read first.
; Documentation.  The standard Homer Tools set, each Markdown file with the
; matching HTML the build generates from it.  gpl.txt is gone: FileDir is MIT
; licensed, and License.md/.htm say so.  The old plain-text hotkeys.txt and
; history.txt are replaced by Hotkeys.md and History.md.
Source: "ReadMe.md";          DestDir: "{app}"; Flags: ignoreversion
Source: "ReadMe.htm";         DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.md";         DestDir: "{app}"; Flags: ignoreversion
Source: "FileDir.htm";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Developer.md";       DestDir: "{app}"; Flags: ignoreversion
Source: "Developer.htm";      DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "License.md";         DestDir: "{app}"; Flags: ignoreversion
Source: "License.htm";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "History.md";         DestDir: "{app}"; Flags: ignoreversion
Source: "History.htm";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Hotkeys.md";         DestDir: "{app}"; Flags: ignoreversion
Source: "Hotkeys.htm";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Announce.md";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Announce.htm";       DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FAQ.md";             DestDir: "{app}"; Flags: ignoreversion
Source: "FAQ.htm";            DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Tutorials.md";       DestDir: "{app}"; Flags: ignoreversion
Source: "Tutorials.htm";      DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; Data files the program reads.
Source: "Convert.txt";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Quick.txt";          DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; Coding style, shipped with the source so a modification follows it.
Source: "Camel_Type_C#.md";       DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "CamelType_CSharp.md";    DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "CamelType_JAWSScript.md"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Dirs]
Name: "{userappdata}\FileDir";
Name: "{userappdata}\FileDir\Temp";

[InstallDelete]
; Clear any pre-existing FileDir desktop shortcut before [Icons] recreates the
; single hot-key shortcut below. The legacy installer placed an Alt+Ctrl+F
; shortcut on the USER's desktop pointing at the old exe (via a FileCopy hack);
; removing it from both the user and common desktops leaves the {autodesktop}
; shortcut as the sole owner of Alt+Ctrl+F. (InstallDelete runs before [Icons],
; so the recreate still wins.)
Type: files; Name: "{userdesktop}\FileDir.lnk"
Type: files; Name: "{commondesktop}\FileDir.lnk"

; Remove components retired in 5.0 so upgrading over a 4.x install does not
; leave orphans: the LbcVB / LbcJS helper assemblies and their sources, the
; GetProps.js shell script, the old gettext text engine (now 2htm), and the
; LayoutByCode dialog library (lbc.dll / lbc.cs), now folded into Homer and
; the FileDir-local Dialogs.cs.
Type: files; Name: "{app}\LbcVB.dll"
Type: files; Name: "{app}\LbcVB.VB"
Type: files; Name: "{app}\LbcJS.dll"
Type: files; Name: "{app}\LbcJS.js"
Type: files; Name: "{app}\GetProps.js"
Type: files; Name: "{app}\gettext.exe"
Type: files; Name: "{app}\Scripts\FileDir_Scripts_setup.exe"
; WebGet.exe scraped Internet Explorer's address bar for the Quick URL and Web
; Download commands.  Internet Explorer is gone; those commands now take the
; address from the clipboard, and downloading is done by FileDir itself.
Type: files; Name: "{app}\WebGet.exe"
Type: files; Name: "{app}\WebGet.tmp"
; Encoding.exe was the external character-encoding tool; the Ude library does the
; detection now, and the base class library does the conversion.
Type: files; Name: "{app}\Encoding.exe"
; Web Client Utilities: the ~35 Python "web 2.0" scripts and the InPy interpreter
; they ran under.  The services they called are long gone, so the feature has been
; removed from FileDir; delete the whole tree from an existing install.
Type: filesandordirs; Name: "{app}\WebClient"
Type: files; Name: "{app}\InPy.exe"
Type: files; Name: "{app}\InPyC.exe"
; 32-bit screen-reader client DLLs.  System Access is gone, and these cannot load
; in the 64-bit process anyway; speech now goes through JAWS/NVDA/UIA (Homer.Say).
Type: files; Name: "{app}\saapi32.dll"
Type: files; Name: "{app}\nvdaControllerClient32.dll"
Type: files; Name: "{app}\lbc.dll"
; The lbc.cs line that used to sit here removed a file the installer now ships.
; Windows file names are case insensitive, so "{app}\lbc.cs" and the shipped
; Lbc.cs are the same file: the installer deleted it and put it straight back.
; Hotkeys.ini used to ship with onlyifdoesntexist, so an existing installation
; kept a copy that could never be updated and Key Describer read stale text from
; it.  The table is compiled in now, so the stale file is removed.  A user who
; wants to override an entry can create the file again.
Type: files; Name: "{app}\Hotkeys.ini"
; The build was three scripts for a while; it is two now.
Type: files; Name: "{app}\auditFileDir.ps1"
Type: files; Name: "{app}\auditFileDir.cmd"
Type: files; Name: "{app}\makeKeyMap.ps1"
Type: files; Name: "{app}\makeKeyMap.cmd"
; Documents replaced by the Markdown set.  gpl.txt goes with the licence change
; to MIT; hotkeys.txt and history.txt are now Hotkeys.md and History.md.
Type: files; Name: "{app}\gpl.txt"
Type: files; Name: "{app}\hotkeys.txt"
Type: files; Name: "{app}\history.txt"
Type: files; Name: "{app}\FileDir.txt"
; The text-extraction filter DLLs that gettext.exe drove.  2htm replaced the
; whole pipeline, so the folder is orphaned on an upgraded install.
Type: filesandordirs; Name: "{app}\filters"

[Icons]
Name: "{group}\Launch FileDir";   Filename: "{app}\FileDir.exe"; WorkingDir: "{app}"
Name: "{group}\FileDir Manual";   Filename: "{app}\FileDir.htm"
Name: "{group}\Set Extensions to Open with FileDir"; Filename: "{app}\AssocOn.exe"; WorkingDir: "{app}"
Name: "{group}\Turn off Association between Extensions and FileDir"; Filename: "{app}\AssocOff.exe"; WorkingDir: "{app}"
Name: "{group}\View License for FileDir"; Filename: "{app}\License.htm"
Name: "{group}\FileDir Hotkeys"; Filename: "{app}\Hotkeys.htm"
Name: "{group}\FileDir Tutorials"; Filename: "{app}\Tutorials.htm"
Name: "{group}\FileDir Questions and Answers"; Filename: "{app}\FAQ.htm"
Name: "{group}\Uninstall FileDir"; Filename: "{uninstallexe}"
; Single hot-key shortcut (DbDo/EdSharp model): the one shortcut that owns
; Alt+Ctrl+F is created with {autodesktop} (user desktop for a per-user install,
; common desktop for an all-users install) and HotKey. No Start Menu item carries
; a hot key, so Alt+Ctrl+F has exactly one owner. FileDir is single-instance:
; OnStartupNextInstance brings the running copy to the foreground, so a plain
; relaunch activates rather than starting a second copy.
Name: "{autodesktop}\FileDir"; Filename: "{app}\FileDir.exe"; WorkingDir: "{app}"; IconFilename: "{app}\FileDir.ico"; HotKey: Alt+Ctrl+F; Comment: "Launch or activate FileDir 5.0 (Alt+Control+F)"

[Run]
; ---- The Finish-page checkbox list, in the Homer Tools pattern ----
;
; No Tasks page and no Components page: every optional install is a checkbox in
; this list at the end, each running a probe-first script that reuses whatever
; is already installed, logs to the consolidated setup log, and never pauses.
; runascurrentuser matters for the optional installs: winget installs per user,
; into the profile of whoever is signed in, while this installer runs elevated.
;
; WHICH BOXES ARE TICKED BY DEFAULT. The question is what a FileDir user gets
; for the download:
;   The screen reader support and the program itself are TICKED. They are what
;     FileDir is, they cost nothing to fetch, and they are what the person came
;     for.
;   Ollama with its chat model (about 2 GB) and the larger translation model
;     (about 5 GB) are NOT ticked. Each serves a real feature -- translating
;     files without sending them anywhere -- but each serves some users and not
;     others, and together they cost several gigabytes. Nobody should download
;     that by not noticing a checkbox.
;
; WHY EACH TOOL APPEARS THREE TIMES BELOW. The label says what the box will do,
; and the boxes are grouped so the ones that do something come first:
; everything to be installed, then everything to be updated, then anything
; already current, which is offered last and never ticked because there is
; nothing to gain. Only one entry per tool is ever shown; the other two are
; skipped by their Check function.
;
; The two screen reader checkboxes come first, both ticked. Launching FileDir
; and opening the guide come LAST, after every component, because they are not
; installations -- the same order EdSharp uses.
;
; 1. JAWS scripts.  "FileDir.exe --install-jaws-settings" copies the script family into
;    every installed version of JAWS and compiles it there.  The implementation is the
;    shared Homer.JawsSettingsInstaller (in Say.cs), so EdSharp, FileDir, and DbDo all
;    install scripts by the same code, and the command can be re-run later.
FileName: "{app}\FileDir.exe"; \
  Parameters: "--install-jaws-settings --quiet"; \
  WorkingDir: "{app}"; \
  Description: "Install scripts for improving use with the JAWS screen reader"; \
  Flags: postinstall waituntilterminated runhidden skipifsilent

; 2. NVDA add-on.  Shell-executing the .nvda-addon hands it to NVDA's own file
;    association, so NVDA shows its native add-on install dialog.  skipifdoesntexist
;    means the checkbox simply does not appear if the app ships no add-on yet.
FileName: "{app}\FileDir.nvda-addon"; \
  WorkingDir: "{app}"; \
  Description: "Install add-on for improving use with the NVDA screen reader"; \
  Flags: postinstall shellexec waituntilterminated skipifsilent skipifdoesntexist

; ---- Install: not on this computer yet ----

; Pandoc is the one optional component that IS ticked. It is what gives FileDir
; its conversions: Convert Format, and reading the formats Say Contents cannot
; reach on its own. Without it FileDir still works, but a third of what the
; Transfer and Query commands can do quietly disappears. About 100 MB, machine
; wide, shared with EdSharp and HomerScribe.
FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installPandoc.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descPandoc}"; \
  Flags: postinstall skipifsilent runascurrentuser; Check: pandocNeedsInstall

; The media tools, ticked. Three commands need them: Type Extended reads the
; metadata inside a file with ExifTool, Output As converts audio, video and
; pictures with ffmpeg, and Web Download fetches media with yt-dlp. Ticked for
; the same reason Pandoc is: without them those commands quietly do less.
FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installMediaTools.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descMediaTools}"; \
  Flags: postinstall skipifsilent runascurrentuser; Check: mediaToolsNeedInstall

; The PDF reader. Ticked: without it, Say Contents and the conversions do
; nothing useful with a PDF, and PDF is the format people most often have.
FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installPdfTools.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descPdfTools}"; \
  Flags: postinstall skipifsilent runascurrentuser; Check: pdfToolsNeedInstall

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installOllama.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descOllama}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: ollamaNeedsInstall

; The larger translation model, offered next to Ollama itself because it is
; useless without it. Unticked: five gigabytes is a real decision, and the small
; chat model translates well enough to try the feature first.
FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installTranslateModel.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descTranslateModel}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: translateModelNeedsInstall

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installMpv.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descMpv}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: mpvNeedsInstall

; ---- Update: installed, but a newer version is available ----

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installPandoc.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descPandoc}"; \
  Flags: postinstall skipifsilent runascurrentuser; Check: pandocNeedsUpdate

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installOllama.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descOllama}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: ollamaNeedsUpdate

; ---- Reinstall: already current, offered only for repair ----

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installMpv.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descMpv}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: mpvIsCurrent

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installPandoc.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descPandoc}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: pandocIsCurrent

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installOllama.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descOllama}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: ollamaIsCurrent

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installMediaTools.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descMediaTools}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: mediaToolsAreCurrent

FileName: "{cmd}"; \
  Parameters: "/c """"{app}\installTranslateModel.cmd""""";  \
  WorkingDir: "{app}"; \
  Description: "{code:descTranslateModel}"; \
  Flags: postinstall skipifsilent runascurrentuser unchecked; Check: translateModelIsCurrent

; ---- After the components: what to do now ----
;
; Two ordinary things a person may want the moment setup ends, offered last
; because they are not installations. Both unticked: somebody reinstalling to
; fix one component does not want a file manager opening over their work. The
; label names the key as a friendly reminder rather than an instruction.
;
; runasoriginaluser matters -- setup is elevated, and a program started from
; here would otherwise run as the administrator, writing its settings into the
; wrong profile.
FileName: "{app}\FileDir.exe"; \
  WorkingDir: "{app}"; \
  Description: "Launch FileDir (Alt+Control+F starts it any time)"; \
  Flags: postinstall skipifsilent nowait runasoriginaluser unchecked

FileName: "{app}\FileDir.htm"; \
  Description: "Open the user guide (F1 opens it inside FileDir)"; \
  Flags: postinstall skipifsilent shellexec nowait runasoriginaluser unchecked

; The results summary is NOT listed here. It is not an option -- it always runs,
; and it must run last of all -- so it is started from code at the very end,
; once every entry above has finished. See DeinitializeSetup.

; Native image generation.  Not checkboxes: these run automatically and elevated, so
; the installed copy starts from a cached native image instead of JIT-compiling.
; Identical in all three apps.  HasNgen skips them if ngen.exe is absent.
FileName: "{code:NgenExe}"; Parameters: "uninstall FileDir /nologo /silent"; Flags: runhidden; Check: HasNgen
FileName: "{code:NgenExe}"; Parameters: "install ""{app}\FileDir.exe"" /AppBase:""{app}"" /nologo /silent"; Flags: runhidden; Check: HasNgen

[UninstallRun]
; Symmetric to the JAWS-install [Run] entry above. Removes only the
; files FileDir placed in the JAWS settings folders, tracked via the
; install-time log at %APPDATA%\FileDir\jawsSettings.log. runhidden so
; no console window flashes; skipped if FileDir.exe is already gone.
FileName: "{app}\FileDir.exe"; \
  Parameters: "--uninstall-jaws-settings"; \
  WorkingDir: "{app}"; \
  Flags: runhidden waituntilterminated skipifdoesntexist

Filename: "{code:NgenExe}"; Parameters: "uninstall FileDir /nologo /silent"; Flags: runhidden; Check: HasNgen

[UninstallDelete]
Type: files; Name: "{app}\FileDir.exe"
Type: files; Name: "{app}\BuildFileDir.log"

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\FileDir.exe"; ValueType: string; ValueName: ""; ValueData: "{app}\FileDir.exe"; Flags: uninsdeletekey

[Code]

{ ---- Probing, in the Homer Tools pattern -------------------------------------

  Every optional component is probed before it is offered, so an existing
  installation is reused rather than duplicated. Each probe is asked once and
  cached: winget takes a second or two per question, and the finish page asks
  several.

  All of this is lifted from the EdSharp installer, which reached this shape
  over many iterations. It is deliberately not reinvented here. }

var
  gbInstalled: boolean;

procedure logLine(sFolder, sText: string);
var
  lsLine: TArrayOfString;
begin
  SetArrayLength(lsLine, 1);
  lsLine[0] := '[' + GetDateTimeString('yyyy/mm/dd hh:nn:ss', '-', ':') + '] ' + sText;
  SaveStringsToFile(AddBackslash(sFolder) + 'FileDir_setup.log', lsLine, True);
end;

function probeLines(sCommand: string; var lsLines: TArrayOfString): boolean;
{ Run a command and read what it printed, recording both in the log. }
var
  iResult, i: integer;
  sLogDir, sCaptureFile: string;
begin
  sLogDir := ExpandConstant('{localappdata}\FileDir\logs');
  ForceDirectories(sLogDir);
  sCaptureFile := sLogDir + '\FileDir_probe.tmp';
  // The probes run 64-bit like everything else here: the installer itself is
  // marked x64, so the cmd constant gives the 64-bit shell and winget reports
  // the machine's real 64-bit packages rather than a WOW64 view.
  //
  // Note the comment style. A brace comment ends at the FIRST closing brace,
  // so one that mentions an Inno constant ends in the middle of its own
  // sentence and hands the rest of the prose to the compiler as code. Any
  // comment naming a constant uses double slashes instead.
  result := Exec(ExpandConstant('{cmd}'), '/c ' + sCommand + ' > "' + sCaptureFile + '" 2>&1', '', SW_HIDE, ewWaitUntilTerminated, iResult);
  if result then
    result := LoadStringsFromFile(sCaptureFile, lsLines);
  logLine(sLogDir, '[probe] ' + sCommand);
  if result then
    for i := 0 to GetArrayLength(lsLines) - 1 do
      if Trim(lsLines[i]) <> '' then
        logLine(sLogDir, '[probe]   ' + lsLines[i])
  else
    logLine(sLogDir, '[probe]   the command could not be run');
  if FileExists(sCaptureFile) then
    DeleteFile(sCaptureFile);
end;

var
  gModelList: string;
  gModelListKnown: boolean;

function ollamaModelList(): string;
{ Ollama's model list, read once. Two labels ask about it, and each reading
  costs a second or more.

  Asked over Ollama's web interface rather than by running its command line
  client, which starts the server in a console of its own when it is not
  already running -- a window on screen during setup that looks like a fault.
  This opens nothing. }
var
  lsLines: TArrayOfString;
  i: integer;
  sQuote, sCommand: string;
begin
  if gModelListKnown then
  begin
    result := gModelList;
    exit;
  end;
  gModelListKnown := True;
  gModelList := '';
  { The command is assembled with Chr(39) rather than written with doubled
    apostrophes. Four levels of quoting meet on this one line: Pascal, the
    command interpreter, the double quotes PowerShell needs, and the single
    quotes inside them. Writing them literally is how this line was corrupted
    once already. Built this way, each apostrophe is visibly one apostrophe,
    and no apostrophe appears in this comment to confuse a reader either. }
  sQuote := Chr(39);
  sCommand := 'powershell -NoProfile -Command "try { (Invoke-RestMethod'
    + ' -Uri http://localhost:11434/api/tags -TimeoutSec 10).models.name'
    + ' -join ' + sQuote + ' ' + sQuote
    + ' } catch { ' + sQuote + sQuote + ' }"';
  if probeLines(sCommand, lsLines) then
    for i := 0 to GetArrayLength(lsLines) - 1 do
      gModelList := gModelList + lsLines[i] + Chr(10);
  result := gModelList;
end;

function wingetInfo(sId: string; var sInstalled, sAvailable: string): boolean;
{ True when winget lists the package as installed; fills the installed version
  and, when an update exists, the available version. Columns are located by the
  header line, since names can contain spaces. }
var
  lsLines: TArrayOfString;
  i, iVer, iAvail, iSrc: integer;
  sLine: string;
begin
  result := false;
  sInstalled := '';
  sAvailable := '';
  iVer := 0;
  iAvail := 0;
  iSrc := 0;
  if not probeLines('winget list --id ' + sId + ' --exact --disable-interactivity', lsLines) then
    exit;
  for i := 0 to GetArrayLength(lsLines) - 1 do
  begin
    sLine := lsLines[i];
    if (iVer = 0) and (Pos('Name', sLine) > 0) and (Pos('Version', sLine) > 0) then
    begin
      iVer := Pos('Version', sLine);
      iAvail := Pos('Available', sLine);
      iSrc := Pos('Source', sLine);
      continue;
    end;
    if (iVer > 0) and (Pos(sId, sLine) > 0) then
    begin
      result := true;
      if iAvail > 0 then
      begin
        sInstalled := Trim(Copy(sLine, iVer, iAvail - iVer));
        if iSrc > iAvail then
          sAvailable := Trim(Copy(sLine, iAvail, iSrc - iAvail))
        else
          sAvailable := Trim(Copy(sLine, iAvail, 200));
      end
      else if iSrc > iVer then
        sInstalled := Trim(Copy(sLine, iVer, iSrc - iVer))
      else
        sInstalled := Trim(Copy(sLine, iVer, 200));
      exit;
    end;
  end;
end;

function exeVersion(sExe: string): string;
{ The tool's own version line, for an install winget does not know about. }
var
  lsLines: TArrayOfString;
  i: integer;
begin
  result := '';
  if not probeLines(sExe + ' --version', lsLines) then
    exit;
  for i := 0 to GetArrayLength(lsLines) - 1 do
    if Trim(lsLines[i]) <> '' then
    begin
      result := Trim(lsLines[i]);
      exit;
    end;
end;

function wingetLatest(sId: string): string;
{ The newest version winget offers for a package, installed or not, so the
  Install label can carry a number parallel to the Update one. }
var
  lsLines: TArrayOfString;
  i: integer;
  sLine: string;
begin
  result := '';
  if not probeLines('winget show --id ' + sId + ' --exact --disable-interactivity', lsLines) then
    exit;
  for i := 0 to GetArrayLength(lsLines) - 1 do
  begin
    sLine := Trim(lsLines[i]);
    if Pos('Version:', sLine) = 1 then
    begin
      result := Trim(Copy(sLine, 9, 100));
      exit;
    end;
  end;
end;

var
  gDescCache: array[0..3] of string;
  gStateCache: array[0..3] of integer;
  gStateKnown: array[0..3] of boolean;

function devToolDesc(iIndex: integer; sIdList, sExe, sTool, sInstallLabel: string): string;
{ All three labels start with the action the checkbox performs and carry version
  numbers in parallel: "Install <tool> <latest>", "Update <tool> from <old> to
  <new>", and "Reinstall <tool> <version> (current version)". When winget cannot
  say which version is current, the plain label still works. }
var
  sInstalled, sAvailable, sId, sRest, sVersion, sFirstId: string;
  iSplit: integer;
begin
  if gDescCache[iIndex] <> '' then
  begin
    result := gDescCache[iIndex];
    exit;
  end;
  result := '';
  sFirstId := sIdList;
  if Pos(';', sFirstId) > 0 then
    sFirstId := Copy(sFirstId, 1, Pos(';', sFirstId) - 1);
  sRest := sIdList;
  while (result = '') and (sRest <> '') do
  begin
    iSplit := Pos(';', sRest);
    if iSplit > 0 then
    begin
      sId := Copy(sRest, 1, iSplit - 1);
      sRest := Copy(sRest, iSplit + 1, 500);
    end
    else
    begin
      sId := sRest;
      sRest := '';
    end;
    if wingetInfo(sId, sInstalled, sAvailable) then
    begin
      if sAvailable <> '' then
        result := 'Update ' + sTool + ' from ' + sInstalled + ' to ' + sAvailable
      else if sInstalled <> '' then
        result := 'Reinstall ' + sTool + ' ' + sInstalled + ' (current version)';
    end;
  end;
  if result = '' then
  begin
    sVersion := exeVersion(sExe);
    if sVersion <> '' then
      result := 'Reinstall ' + sTool + ' ' + sVersion + ' (installed version)'
    else
    begin
      sVersion := wingetLatest(sFirstId);
      if sVersion <> '' then
        result := 'Install ' + sTool + ' ' + sVersion
      else
        result := sInstallLabel;
    end;
  end;
  gDescCache[iIndex] := result;
end;

function devToolState(iIndex: integer; sIdList, sExe: string): integer;
{ Which of the three things a box would do: 0 install, 1 update, 2 reinstall.
  Computed from the same probes the labels use, cached so winget is asked once,
  and consulted by the Check functions that decide which of a tool's three
  entries is shown. }
var
  sInstalled, sAvailable, sId, sRest: string;
  iSplit: integer;
begin
  if gStateKnown[iIndex] then
  begin
    result := gStateCache[iIndex];
    exit;
  end;
  result := 0;
  sRest := sIdList;
  while sRest <> '' do
  begin
    iSplit := Pos(';', sRest);
    if iSplit > 0 then
    begin
      sId := Copy(sRest, 1, iSplit - 1);
      sRest := Copy(sRest, iSplit + 1, 500);
    end
    else
    begin
      sId := sRest;
      sRest := '';
    end;
    if wingetInfo(sId, sInstalled, sAvailable) then
    begin
      if sAvailable <> '' then result := 1
      else result := 2;
      break;
    end;
  end;
  { Installed outside winget's knowledge still counts as installed: the person
    should be offered a reinstall, not a second copy. }
  if (result = 0) and (exeVersion(sExe) <> '') then result := 2;
  gStateCache[iIndex] := result;
  gStateKnown[iIndex] := True;
end;

function ollamaNeedsInstall(): boolean;
begin
  result := devToolState(0, 'Ollama.Ollama', 'ollama') = 0;
end;

function ollamaNeedsUpdate(): boolean;
begin
  result := devToolState(0, 'Ollama.Ollama', 'ollama') = 1;
end;

function ollamaIsCurrent(): boolean;
begin
  result := devToolState(0, 'Ollama.Ollama', 'ollama') = 2;
end;

function pandocNeedsInstall(): boolean;
begin
  result := devToolState(1, 'JohnMacFarlane.Pandoc', 'pandoc') = 0;
end;

function pandocNeedsUpdate(): boolean;
begin
  result := devToolState(1, 'JohnMacFarlane.Pandoc', 'pandoc') = 1;
end;

function pandocIsCurrent(): boolean;
begin
  result := devToolState(1, 'JohnMacFarlane.Pandoc', 'pandoc') = 2;
end;

function descPandoc(sParam: string): string;
var
  sMachineCopy: string;
begin
  // Pandoc installs machine-wide, so its own folder answers for a version even
  // when it is not yet on this process PATH -- which it will not be, moments
  // after winget put it there.
  sMachineCopy := ExpandConstant('{pf}\Pandoc\pandoc.exe');
  if FileExists(sMachineCopy) then
    result := devToolDesc(1, 'JohnMacFarlane.Pandoc', '"' + sMachineCopy + '"', 'Pandoc', 'Install Pandoc for document conversion: Word, ODT, EPUB, RTF, LaTeX, HTML and more (about 100 MB, shared with other apps)')
  else
    result := devToolDesc(1, 'JohnMacFarlane.Pandoc', 'pandoc', 'Pandoc', 'Install Pandoc for document conversion: Word, ODT, EPUB, RTF, LaTeX, HTML and more (about 100 MB, shared with other apps)');
end;

function mediaToolsPresent(): boolean;
// The three are treated as one, the way EdSharp treats its document tools: they
// are used together, and a half set leaves a command half working. Found by
// running each, so a copy installed outside winget counts too.
var
  lsLines: TArrayOfString;
begin
  if gStateKnown[2] then
  begin
    result := (gStateCache[2] = 2);
    exit;
  end;
  result := probeLines('where exiftool', lsLines) and (GetArrayLength(lsLines) > 0)
            and (Pos('exiftool', Lowercase(lsLines[0])) > 0);
  if result then
    result := probeLines('where ffmpeg', lsLines) and (GetArrayLength(lsLines) > 0)
              and (Pos('ffmpeg', Lowercase(lsLines[0])) > 0);
  if result then
    result := probeLines('where yt-dlp', lsLines) and (GetArrayLength(lsLines) > 0)
              and (Pos('yt-dlp', Lowercase(lsLines[0])) > 0);
  if result then gStateCache[2] := 2 else gStateCache[2] := 0;
  gStateKnown[2] := True;
end;

function mpvPresent(): boolean;
// Whether mpv answers on this machine. Found by running it, so a copy
// installed outside winget counts too.
var
  lsLines: TArrayOfString;
begin
  result := probeLines('where mpv', lsLines) and (GetArrayLength(lsLines) > 0)
            and (Pos('mpv', Lowercase(lsLines[0])) > 0);
end;

function mpvNeedsInstall(): boolean;
begin
  result := not mpvPresent();
end;

function mpvIsCurrent(): boolean;
begin
  result := mpvPresent();
end;

function descMpv(sParam: string): string;
begin
  if mpvPresent() then
    result := 'Reinstall mpv, the media player Play List uses (installed)'
  else
    result := 'Install mpv so Play List can play what it makes, sound and picture or sound alone (about 60 MB; conversion does not need it)';
end;

function pdfToolsPresent(): boolean;
// Whether a Python on this machine can import the PDF reader. Asked of the
// interpreter installPdfTools recorded, when there is one, because a machine
// may carry several Pythons and only one of them will have the package.
var
  lsLines: TArrayOfString;
  sRecord, sPython: string;
begin
  if gStateKnown[3] then
  begin
    result := (gStateCache[3] = 2);
    exit;
  end;
  sPython := 'python';
  sRecord := ExpandConstant('{localappdata}\FileDir\logs\FileDir_python.txt');
  if FileExists(sRecord) then
    if LoadStringsFromFile(sRecord, lsLines) then
      if GetArrayLength(lsLines) > 0 then
        if Trim(lsLines[0]) <> '' then sPython := '"' + Trim(lsLines[0]) + '"';
  result := probeLines(sPython + ' -c "import pymupdf4llm; print(1)"', lsLines)
            and (GetArrayLength(lsLines) > 0) and (Pos('1', lsLines[0]) > 0);
  if result then gStateCache[3] := 2 else gStateCache[3] := 0;
  gStateKnown[3] := True;
end;

function pdfToolsNeedInstall(): boolean;
begin
  result := not pdfToolsPresent();
end;

function descPdfTools(sParam: string): string;
begin
  if pdfToolsPresent() then
    result := 'Reinstall the PDF reader, PyMuPDF4LLM (installed)'
  else
    result := 'Install the PDF reader, PyMuPDF4LLM, so PDFs can be read with their headings, lists and tables (about 25 MB; needs Python)';
end;

function mediaToolsNeedInstall(): boolean;
begin
  result := not mediaToolsPresent();
end;

function mediaToolsAreCurrent(): boolean;
begin
  result := mediaToolsPresent();
end;

function descMediaTools(sParam: string): string;
begin
  if mediaToolsPresent() then
    result := 'Reinstall the media tools: ExifTool, ffmpeg and yt-dlp (installed)'
  else
    result := 'Install the media tools: ExifTool for file metadata, ffmpeg for audio and video, yt-dlp for web media (about 200 MB, shared with other apps)';
end;

function translateModelIsCurrent(): boolean;
{ The translation model is an Ollama model rather than a winget package, so
  "installed" means its name appears in Ollama's own list. Without this test it
  would be offered as an install however often it was installed. }
begin
  result := Pos('qwen2.5:7b', ollamaModelList()) > 0;
end;

function translateModelNeedsInstall(): boolean;
begin
  result := not translateModelIsCurrent();
end;

function descTranslateModel(sParam: string): string;
begin
  { Name the model. "A stronger model" tells nobody what they are getting or
    what to look for in Ollama afterwards. }
  if translateModelIsCurrent() then
    result := 'Reinstall qwen2.5:7b, the translation model (installed)'
  else
    result := 'Install qwen2.5:7b for translation, better than the chat model (about 5 GB; needs Ollama)';
end;

function descOllama(sParam: string): string;
var
  sUserCopy: string;
begin
  { Ollama installs per user; when it is absent from PATH, its own exe in the
    profile still answers for a version. }
  sUserCopy := ExpandConstant('{localappdata}\Programs\Ollama\ollama.exe');
  if FileExists(sUserCopy) then
    result := devToolDesc(0, 'Ollama.Ollama', '"' + sUserCopy + '"', 'Ollama', 'Install Ollama with the llama3.2 chat model, for Translate File (about 2 GB, shared with other apps)')
  else
    result := devToolDesc(0, 'Ollama.Ollama', 'ollama', 'Ollama', 'Install Ollama with the llama3.2 chat model, for Translate File (about 2 GB, shared with other apps)');
end;

function NgenExe(sParam: string): string;
begin
  // ngen ships with the 64-bit .NET Framework runtime; on an ARM64 system the
  // Framework64 path is the ARM64 framework. HasNgen guards a missing file.
  result := ExpandConstant('{win}\Microsoft.NET\Framework64\v4.0.30319\ngen.exe');
end;

function HasNgen(): boolean;
begin
  result := FileExists(ExpandConstant('{code:NgenExe}'));
end;

procedure warmComponentProbes();
{ Ask every question the finish page will ask, while the progress bar is still
  on screen and can say what is happening. Each winget or Ollama query takes a
  second or two; asked when the finish page is being built, they add up to a
  silent wait with nothing to read. Asked here, the answers are cached, the page
  appears at once, and no extra screen is added -- the existing status line does
  the talking. }
begin
  try
    WizardForm.StatusLabel.Caption := 'Checking which components are installed ...';
    WizardForm.ProgressGauge.Style := npbstMarquee;
    WizardForm.Refresh();

    WizardForm.StatusLabel.Caption := 'Checking the media tools ...';
    mediaToolsPresent();
    WizardForm.StatusLabel.Caption := 'Checking Pandoc ...';
    devToolState(1, 'JohnMacFarlane.Pandoc', 'pandoc');
    WizardForm.StatusLabel.Caption := 'Checking Ollama ...';
    devToolState(0, 'Ollama.Ollama', 'ollama');
    WizardForm.StatusLabel.Caption := 'Checking the AI models ...';
    ollamaModelList();
    { The labels themselves, so the page has nothing left to compute. }
    WizardForm.StatusLabel.Caption := 'Checking the PDF reader ...';
    pdfToolsPresent();
    WizardForm.StatusLabel.Caption := 'Checking mpv ...';
    mpvPresent();
    descMediaTools(''); descPandoc(''); descPdfTools(''); descMpv(''); descOllama(''); descTranslateModel('');

    WizardForm.ProgressGauge.Style := npbstNormal;
    WizardForm.StatusLabel.Caption := '';
  except
  end;
end;

procedure saveResultsForSummary(sFolder, sText: string);
{ Hand what the installer already knows to the summary, so ONE box tells the
  whole story instead of two telling halves. }
var
  lsLines: TArrayOfString;
begin
  SetArrayLength(lsLines, 1);
  lsLines[0] := sText;
  SaveStringsToFile(AddBackslash(sFolder) + 'FileDir_setup_results.txt', lsLines, False);
end;

procedure showResultsSummary();
{ The single Results box: always shown, always last. Inno reaches this point
  after the finish page's entries have run, which is the only moment at which
  the disposition of every checkbox is actually known. The script is hidden and
  shows one message box; setup does not wait for it, so closing the box is the
  last thing that happens. }
var
  iResult: integer;
begin
  try
    Exec(ExpandConstant('{cmd}'), '/c ""' + ExpandConstant('{app}\summarizeSetup.cmd') + '""', ExpandConstant('{app}'), SW_HIDE, ewNoWait, iResult);
  except
  end;
end;

procedure CurStepChanged(iCurStep: TSetupStep);
begin
  if iCurStep = ssPostInstall then
  begin
    gbInstalled := True;
    warmComponentProbes();
  end;
end;

function haveJaws(): boolean;
{ Whether JAWS is on this computer at all, so the Results box can say "not
  offered" rather than "not installed" -- which would read as a failure to
  somebody who does not use JAWS. }
begin
  result := DirExists(ExpandConstant('{userappdata}\Freedom Scientific\JAWS'))
            or DirExists(ExpandConstant('{commonappdata}\Freedom Scientific\JAWS'));
end;

procedure DeinitializeSetup();
var
  sBreak, sLogDir, sMessage: string;
begin
  { DeinitializeSetup runs whenever Setup exits, INCLUDING WHEN THE USER
    CANCELS. Announcing success to somebody who has just backed out would be a
    plain lie, so the summary is shown only if the files were copied. And there
    is nobody to read a box in a silent installation, where it would wait for
    ever for a click a script cannot give. }
  if (not gbInstalled) or WizardSilent then
    exit;

  sBreak := Chr(13) + Chr(10);
  sLogDir := ExpandConstant('{localappdata}\FileDir\logs');
  ForceDirectories(sLogDir);

  sMessage := 'FileDir is installed.' + sBreak + sBreak
    + 'Program files:' + sBreak + '  ' + ExpandConstant('{app}') + sBreak
    + 'Logs:' + sBreak + '  ' + sLogDir + sBreak + sBreak
    + 'Results' + sBreak;

  { The JAWS scripts, which run before this point and so can be reported here.
    The shared installer records what it copied in a log under the user profile;
    its presence is what says the step ran. }
  if FileExists(ExpandConstant('{userappdata}\FileDir\jawsSettings.log')) then
    sMessage := sMessage + '  JAWS scripts: installed.' + sBreak
  else if haveJaws() then
    sMessage := sMessage + '  JAWS scripts: NOT installed. Reinstall and leave that box ticked, or run FileDir.exe --install-jaws-settings from the program folder.' + sBreak
  else
    sMessage := sMessage + '  JAWS scripts: not offered, because JAWS was not found on this computer.' + sBreak;

  { The optional installs run from the finish page AFTER this text is handed
    over, so their outcome cannot be reported here. The summary that runs last
    says how each one fared. }
  saveResultsForSummary(sLogDir, sMessage);
  showResultsSummary();
end;
