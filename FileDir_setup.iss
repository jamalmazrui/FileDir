; FileDir_setup.iss -- installer for FileDir, built from the HomerDev template.
;
; WHAT IS FILEDIR'S OWN HERE, against the template: the file list, which follows
; the Homer layout FileDir keeps; seven components on the finish page in place
; of the template's one; the file-association helpers; and the AppId, which is
; not a GUID because it never was -- see the note at AppId.
;
; ---- What this template settles, so no app decides again ---------------------
;
; MACHINE WIDE, ADMINISTRATOR. Program Files, no per-user fallback, no "who is
; this for" page. Somebody who wants a portable copy takes the zip.
;
; THE VERSION COMES FROM version.txt, read at compile time. No version literal
; appears here, so a stale copy of this file cannot rewind the number.
;
; IT KNOWS WHAT IS ALREADY INSTALLED. The [Code] section reads the version of
; any previous install from this app's own uninstall key and compares it with
; the version being installed. The checkboxes are worded from that comparison --
; "Install" when nothing is there, "Update" when something older is -- by
; pairing two [Run] lines with Check: functions so only one of each pair
; appears. Nothing says "Install" over the top of a copy that is already there.
;
; CHECKBOX ORDER AND DEFAULTS, in the order the user meets them (HomerDev
; rule, 25 September 2026; the [Run] section says how it is done):
;   1. Install entries, TICKED: screen reader scripts first -- a blind user
;      installing a Homer tool wants its JAWS scripts and its NVDA add-on --
;      then components in alphabetical order.
;   2. Update entries, TICKED, alphabetical.
;   3. Reinstall entries, UNTICKED, alphabetical.
;   4. Launch, TICKED, after the Results box has been read.
;   5. Open the user guide, UNTICKED: there when wanted, out of the way when not.
;
; THE RESULTS BOX COMES BEFORE THE LAUNCH. The Launch entry only leaves a
; marker; CurStepChanged(ssDone) shows one Results box -- a past-tense line per
; box that was ticked, probed after its script ran, and nothing about the rest
; -- and starts the program only after that box is dismissed.

#define AppName       "FileDir"

#define VerFile FileOpen(AddBackslash(SourcePath) + "version.txt")
#define AppVersion Trim(FileRead(VerFile))
#expr FileClose(VerFile)
#undef VerFile

#define AppPublisher  "Jamal Mazrui"
#define AppUrl        "https://github.com/JamalMazrui/FileDir"
#define AppExeName    "FileDir.exe"
#define AppCopyright  "Copyright (c) 2006-2026 Jamal Mazrui. MIT License."

; The desktop shortcut's hotkey. HotKey is the Inno Setup directive
; value, which requires Ctrl syntax; HotKeyDisplay is the same key in the
; notation a person reads -- Control rather than Ctrl, modifiers in alphabetical
; order. Alt+Control+key space belongs to desktop shortcuts, and a shortcut's own
; hotkey is the sanctioned use of it, so it is the right space to take here.
; Alt+Control+F has been FileDir's since 2006.
#define HotKey        "Alt+Ctrl+F"
#define HotKeyDisplay "Alt+Control+F"

[Setup]
; NOT A GUID, AND NOT TO BE CHANGED. FileDir has shipped since 2006 with no
; AppId line, so Inno used the AppName, and every installed copy is registered
; under the uninstall key FileDir_is1. Giving it a GUID now would make the next
; installer fail to recognise the copy already there -- no Update wording, no
; upgrade, a second entry in Programs and Features. The name stays.
AppId=FileDir

AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}
AppUpdatesURL={#AppUrl}/releases
AppCopyright={#AppCopyright}

; The version resource of the built setup. tagRelease reads the FileVersion
; STRING from it and tags v<that>, so the text form is set explicitly: the tag
; wanted is v1.0.0, not v1.0.0.0.
VersionInfoVersion={#AppVersion}
VersionInfoTextVersion={#AppVersion}
VersionInfoProductVersion={#AppVersion}
VersionInfoProductTextVersion={#AppVersion}
VersionInfoCompany={#AppPublisher}
VersionInfoCopyright={#AppCopyright}
VersionInfoDescription={#AppName} Setup

DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
UsePreviousAppDir=yes
; Hide the destination page when a previous install of the same AppId is found:
; a reinstall then asks nothing at all and goes where the last one went. A first
; install still chooses the folder.
DisableDirPage=auto
UsePreviousGroup=yes

; THE INSTALLER KEEPS A LOG, always, and puts it where the program's own logs
; go. SetupLogging makes Inno write a detailed log of every file, registry key
; and run entry into the temporary folder; the [Code] section at the foot of
; this file copies it to
;     %LOCALAPPDATA%\{#AppName}\logs\{#AppName}-setup-<yyyymmdd-hhmmss>.log
; when setup finishes, so an install can be explained a week later. Writing it
; costs nothing; not having it costs an evening.
SetupLogging=yes

; THE INSTALLER IS WRITTEN TO THE TOP OF THE PROJECT, as in every Homer app:
; scripts\release looks for it there, and LocalFiles.txt names it, so tidy
; leaves it in place and git never takes it. Written into exec until
; 26 September 2026, when the release stopped with "FileDir_setup.exe not
; found".
OutputDir=.
OutputBaseFilename={#AppName}_setup
SolidCompression=yes
LicenseFile=
WizardStyle=modern
Compression=lzma2/max
MinVersion=10.0
AppComments=A file manager worked by typed command rather than mouse, for keyboard and screen reader users.

SetupIconFile=FileDir.ico

PrivilegesRequired=admin
; THE PER-USER AREAS ARE USED ON PURPOSE, so the warning about them is off. The
; setup log and the launch marker go to the profile of whoever answered the
; elevation prompt, which on a machine one person uses is that person -- and the
; template says so where it uses them.
UsedUserAreasWarning=no
PrivilegesRequiredOverridesAllowed=

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

Uninstallable=yes
UninstallDisplayIcon={app}\exec\{#AppExeName}
UninstallDisplayName={#AppName} {#AppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%n[name] is licensed under the MIT License: free to use, copy, modify, and distribute; provided "as is" with no warranty. The full license is installed as License.htm in the program folder.%n%nIt is recommended that you close all other applications before continuing.

[Dirs]
; THE HOMER FOLDER LAYOUT. Every folder starts with a different letter, so a
; screen reader user reaches any of them with one keystroke:
;   configs data exec help scripts templates
; temp and logs are not here: they belong to the per-user tree, which the
; program makes for itself. A temp folder under Program Files could not be
; written to anyway.
Name: "{app}\configs"
Name: "{app}\data"
Name: "{app}\help"
Name: "{app}\exec"
Name: "{app}\scripts"
Name: "{app}\templates"

[Files]
; THE PROGRAM AND WHAT RUNS WITH IT go in exec: the executable, the libraries it
; links, the tools it starts, and the icon, manifest and config that are compiled
; in or read beside it. Only the executable is required; every other line says
; skipifsourcedoesntexist so a missing optional piece does not abort the build.
Source: "exec\{#AppExeName}"; DestDir: "{app}\exec"; Flags: ignoreversion
Source: "FileDir.exe.config"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.ico"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.manifest"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "exec\FileDirScript.dll"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "exec\*.dll"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "exec\7z.*"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "exec\2htm.exe"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "exec\AssocOn.exe"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "exec\AssocOff.exe"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist
Source: "exec\Burn2CD.exe"; DestDir: "{app}\exec"; Flags: ignoreversion skipifsourcedoesntexist

; THE DOCUMENTS. ReadMe and License at the root, where a person looking for them
; expects them; everything else in help, which is where F1 and the Help menu read
; from. Markdown for an editor or a braille display, HTML for a browser.
Source: "ReadMe.md"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "ReadMe.htm"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "License.md"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "License.htm"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "help\*.md"; DestDir: "{app}\help"; Flags: ignoreversion skipifsourcedoesntexist
Source: "help\*.htm"; DestDir: "{app}\help"; Flags: ignoreversion skipifsourcedoesntexist
Source: "help\Tutorial_*.inix"; DestDir: "{app}\help"; Flags: ignoreversion skipifsourcedoesntexist
Source: "help\TutorialFeed.xml"; DestDir: "{app}\help"; Flags: ignoreversion skipifsourcedoesntexist
Source: "help\tutorials\*.mp3"; DestDir: "{app}\help\tutorials"; Flags: ignoreversion skipifsourcedoesntexist
Source: "help\tutorials\Tutorials.m3u"; DestDir: "{app}\help\tutorials"; Flags: ignoreversion skipifsourcedoesntexist

; THE SOURCES, because FileDir has always shipped them: anyone can rebuild what
; they installed. The shared classes are not here; they are the kit's.
Source: "FileDir.cs"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Convert.cs"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Dialogs.cs"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Media.cs"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "MediaPlayer.cs"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Mpv.cs"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Table.cs"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.js"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "buildFileDir.cmd"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir_setup.iss"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "RepoFiles.txt"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "LocalFiles.txt"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "accept.inix"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

; SETTINGS AND DATA. FileDir.ini seeds the per-user settings and is never
; overwritten; the program writes its own copy under AppData from the first run.
Source: "configs\FileDir.ini"; DestDir: "{app}\configs"; Flags: onlyifdoesntexist skipifsourcedoesntexist
Source: "configs\Convert.txt"; DestDir: "{app}\configs"; Flags: ignoreversion skipifsourcedoesntexist
Source: "configs\Quick.txt"; DestDir: "{app}\configs"; Flags: ignoreversion skipifsourcedoesntexist
Source: "configs\Hotkeys.inix"; DestDir: "{app}\configs"; Flags: ignoreversion skipifsourcedoesntexist
Source: "data\*"; DestDir: "{app}\data"; Flags: ignoreversion skipifsourcedoesntexist

; THE FINISH HELPER, always shipped: the common half of every install script.
Source: "scripts\installCommon.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion
; ONE SCRIPT PER COMPONENT on the finish page, each calling installCommon.cmd for
; its log and then doing one thing. Every component appears three times in
; [Run] -- install, update, reinstall -- and only one is ever shown.
Source: "scripts\installExifTool.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\installFfmpeg.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\installImageMagick.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\installModels.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\installMpv.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\installOllama.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\installPandoc.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\installPdfTools.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\installYtDlp.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\pdfRich.py"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
; SCREEN READER support: the JAWS scripts zipped by the build from scripts\jaws,
; and the NVDA add-on when there is one, plus the kit's script that puts each
; where its reader looks.
Source: "scripts\installScreenReaderSupport.cmd"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "exec\FileDir_JAWS.zip"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.nvda-addon"; DestDir: "{app}\scripts"; Flags: ignoreversion skipifsourcedoesntexist
Source: "scripts\jaws\*"; DestDir: "{app}\scripts\jaws"; Flags: ignoreversion skipifsourcedoesntexist

[Icons]
; The documents stay at the root of the installed tree, where somebody looking
; for the ReadMe expects them. Everything that runs is one folder down.
Name: "{group}\{#AppName}"; Filename: "{app}\exec\{#AppExeName}"; WorkingDir: "{userdocs}"
Name: "{group}\{#AppName} documentation"; Filename: "{app}\ReadMe.htm"; Flags: createonlyiffileexists
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\exec\{#AppExeName}"; WorkingDir: "{userdocs}"; IconFilename: "{app}\exec\FileDir.ico"; HotKey: "{#HotKey}"; Comment: "Launch or activate FileDir ({#HotKeyDisplay})"

[Run]
; FINISH-PAGE ORDER, a HomerDev rule (25 September 2026):
;   1. Install entries, ticked -- screen reader scripts first, then components
;      in alphabetical order.
;   2. Update entries, ticked, alphabetical.
;   3. Reinstall entries, UNTICKED, alphabetical.
;   4. Launch, ticked.
;   5. Open the user guide, unticked.
; Inno shows [Run] entries in script order and Check: hides the ones that do not
; apply, so three entries per component -- one per verb, each with its own
; Check: from Templates\HomerComponents.iss -- group the page by themselves.
; The label function words each one: "Install X 1.2 (what it is for)",
; "Update X from 1.1 to 1.2 (...)", "Reinstall X 1.2 (...)".
;
; AI NOTE FOR CUSTOMIZING: register each component once in InitializeSetup
; (see homerAdd below), then copy its three entries here into the three groups,
; keeping each group in alphabetical order. A model has Install and Reinstall
; only: ollama pull always fetches the current one.
;
; Scripts run DIRECTLY, with "noPause" as their argument -- never through a cmd
; wrapper with a "set X=1 &&" prefix, which cmd /s cannot quote correctly.

; ---- 1. Install ---------------------------------------------------------------
; JAWS AND NVDA HAVE A BOX EACH, JAWS first, worded alike (1.43.20).
FileName: "{app}\scripts\installScreenReaderSupport.cmd"; \
  Parameters: "noPause jaws"; \
  WorkingDir: "{app}\scripts"; \
  Description: "Install JAWS scripts"; \
  Check: isFreshInstall; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installScreenReaderSupport.cmd"; \
  Parameters: "noPause nvda"; \
  WorkingDir: "{app}\scripts"; \
  Description: "Install NVDA add-on"; \
  Check: isFreshInstall; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installExifTool.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelExifTool}"; \
  Check: isInstallExifTool; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installFfmpeg.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelffmpeg}"; \
  Check: isInstallffmpeg; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installImageMagick.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelImageMagick}"; \
  Check: isInstallImageMagick; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installMpv.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelmpv}"; \
  Check: isInstallmpv; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installOllama.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelOllama}"; \
  Check: isInstallOllama; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installPandoc.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelPandoc}"; \
  Check: isInstallPandoc; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installPdfTools.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelPdfTools}"; \
  Check: isInstallPdfTools; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installYtDlp.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelYtDlp}"; \
  Check: isInstallYtDlp; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installModels.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelModel}"; \
  Check: isModelInstall; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

; ---- 2. Update ----------------------------------------------------------------
FileName: "{app}\scripts\installScreenReaderSupport.cmd"; \
  Parameters: "noPause jaws"; \
  WorkingDir: "{app}\scripts"; \
  Description: "Update JAWS scripts"; \
  Check: isUpgradeOrSame; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installScreenReaderSupport.cmd"; \
  Parameters: "noPause nvda"; \
  WorkingDir: "{app}\scripts"; \
  Description: "Update NVDA add-on"; \
  Check: isUpgradeOrSame; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installExifTool.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelExifTool}"; \
  Check: isUpdateExifTool; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installFfmpeg.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelffmpeg}"; \
  Check: isUpdateffmpeg; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installImageMagick.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelImageMagick}"; \
  Check: isUpdateImageMagick; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installMpv.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelmpv}"; \
  Check: isUpdatempv; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installOllama.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelOllama}"; \
  Check: isUpdateOllama; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installPandoc.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelPandoc}"; \
  Check: isUpdatePandoc; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installPdfTools.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelPdfTools}"; \
  Check: isUpdatePdfTools; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

FileName: "{app}\scripts\installYtDlp.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelYtDlp}"; \
  Check: isUpdateYtDlp; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated skipifdoesntexist

; ---- 3. Reinstall, unticked ---------------------------------------------------
FileName: "{app}\scripts\installExifTool.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelExifTool}"; \
  Check: isReinstallExifTool; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

FileName: "{app}\scripts\installFfmpeg.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelffmpeg}"; \
  Check: isReinstallffmpeg; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

FileName: "{app}\scripts\installImageMagick.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelImageMagick}"; \
  Check: isReinstallImageMagick; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

FileName: "{app}\scripts\installMpv.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelmpv}"; \
  Check: isReinstallmpv; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

FileName: "{app}\scripts\installOllama.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelOllama}"; \
  Check: isReinstallOllama; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

FileName: "{app}\scripts\installPandoc.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelPandoc}"; \
  Check: isReinstallPandoc; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

FileName: "{app}\scripts\installPdfTools.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelPdfTools}"; \
  Check: isReinstallPdfTools; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

FileName: "{app}\scripts\installYtDlp.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelYtDlp}"; \
  Check: isReinstallYtDlp; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

FileName: "{app}\scripts\installModels.cmd"; \
  Parameters: "noPause"; \
  WorkingDir: "{app}\scripts"; \
  Description: "{code:labelModel}"; \
  Check: isModelReinstall; \
  Flags: postinstall skipifsilent runascurrentuser waituntilterminated unchecked skipifdoesntexist

; ---- 4. Launch, ticked --------------------------------------------------------
; The entry only leaves a marker. The program starts from CurStepChanged(ssDone),
; AFTER the Results box has been read and closed -- so the box is not hidden
; behind the program's own window. Inno runs postinstall entries before ssDone.
; TWO pairs of quotes: this Parameters value starts with a quote, which is the
; one case where cmd /s strips the outer pair correctly.
FileName: "{cmd}"; \
  Parameters: "/c echo launch > ""{localappdata}\{#AppName}\logs\{#AppName}_launch.flag"""; \
  Description: "Launch {#AppName} (desktop hotkey {#HotKeyDisplay})"; \
  Flags: postinstall skipifsilent runhidden runasoriginaluser

; ---- 5. Open the user guide, unticked -----------------------------------------
FileName: "{app}\help\FileDir.htm"; \
  Description: "Open the user guide (F1 in {#AppName})"; \
  Flags: postinstall shellexec nowait skipifsilent skipifdoesntexist runasoriginaluser unchecked

[InstallDelete]
; UPGRADING FROM THE FLAT LAYOUT. Every FileDir before 5.0.106 put the program,
; its libraries, its tools, its documents and its scripts at the root of the
; program folder. They are all one level down now, and an old copy left at the
; root would be found first by anything looking there. Each is named rather than
; wildcarded where a wildcard could take something a person put there.
Type: files; Name: "{app}\FileDir.exe"
Type: files; Name: "{app}\FileDirScript.dll"
Type: files; Name: "{app}\*.dll"
Type: files; Name: "{app}\7z.*"
Type: files; Name: "{app}\2htm.exe"
Type: files; Name: "{app}\AssocOn.exe"
Type: files; Name: "{app}\AssocOff.exe"
Type: files; Name: "{app}\Burn2CD.exe"
Type: files; Name: "{app}\FileDir.ico"
Type: files; Name: "{app}\FileDir.manifest"
Type: files; Name: "{app}\FileDir.exe.config"
Type: files; Name: "{app}\FileDir.htm"
Type: files; Name: "{app}\FileDir.md"
Type: files; Name: "{app}\Developer.htm"
Type: files; Name: "{app}\Developer.md"
Type: files; Name: "{app}\History.htm"
Type: files; Name: "{app}\History.md"
Type: files; Name: "{app}\Hotkeys.htm"
Type: files; Name: "{app}\Hotkeys.md"
Type: files; Name: "{app}\Announce.htm"
Type: files; Name: "{app}\Announce.md"
Type: files; Name: "{app}\FAQ.htm"
Type: files; Name: "{app}\FAQ.md"
Type: files; Name: "{app}\Tutorials.htm"
Type: files; Name: "{app}\Tutorials.md"
Type: files; Name: "{app}\Hotkeys.inix"
Type: files; Name: "{app}\Convert.txt"
Type: files; Name: "{app}\chimes.wav"
Type: files; Name: "{app}\install*.cmd"
Type: files; Name: "{app}\summarizeSetup.*"
Type: files; Name: "{app}\BuildFileDir.*"
Type: files; Name: "{app}\cleanFileDir.*"
Type: files; Name: "{app}\auditFileDir.py"
Type: files; Name: "{app}\homerPolicy.py"
Type: files; Name: "{app}\makeKeyMap.py"
Type: files; Name: "{app}\pdfRich.py"
Type: files; Name: "{app}\*.jss"
Type: files; Name: "{app}\*.jsd"
Type: files; Name: "{app}\*.jsh"
Type: files; Name: "{app}\*.jkm"
Type: files; Name: "{app}\*.jcf"
Type: files; Name: "{app}\Web.cs"
Type: files; Name: "{app}\Say.cs"
Type: files; Name: "{app}\Inix.cs"
Type: files; Name: "{app}\Util.cs"
Type: files; Name: "{app}\KeyMap.cs"
Type: files; Name: "{app}\Lbc.cs"
Type: files; Name: "{app}\Ollama.cs"
Type: files; Name: "{app}\Log.cs"
Type: filesandordirs; Name: "{app}\Scripts"

[UninstallDelete]
; ONLY WHAT THIS PROGRAM WROTE. Never the whole {localappdata}\{#AppName}
; folder: every upgrade runs the uninstaller first, and a folder that may hold
; something a user installed or made is never removed wholesale. On
; 24 September 2026 a wholesale line here deleted Whisper on every HomerScribe
; upgrade.
Type: filesandordirs; Name: "{localappdata}\FileDir\logs"

[Code]
//  WHAT THE CODE SECTION DOES, in one screen:
//    - registers the components this app needs, in the shared table from
//      Templates\HomerComponents.iss, which probes each once (winget, then a
//      file, then the exe, then the registry) and words every checkbox;
//    - reads the version of any previous install, so the screen reader
//      script entries say Install or Update truthfully;
//    - records which boxes were ticked when Finish is pressed, and after
//      the scripts have run, reports what happened to each of THOSE -- one
//      past-tense line per ticked box, nothing about the rest;
//    - keeps the setup log with the program's own logs;
//    - starts the program only after the Results box has been closed.
//
//  The include goes INSIDE [Code], and HomerComponents.iss carries no [Code]
//  header of its own. Comments inside [Code] use // or (* *), never ;.
// The kit comes from the build as /DHomerDev=; compiled by hand, C:\HomerDev.
// (Inside [Code] the language is Pascal: a comment starts with //, never ;.)
#ifndef HomerDev
  #define HomerDev "C:\HomerDev"
#endif
#include HomerDev + "\Templates\HomerComponents.iss"

var
  iExifTool, iFfmpeg, iImageMagick, iMpv, iOllama, iPandoc, iPdfTools, iYtDlp: Integer;
  sActions: String;
  sPriorVersion: String;

//  AI NOTE FOR CUSTOMIZING: one homerAdd per component. Arguments: name,
//  winget ids (semicolon separated, or ''), an exe that answers --version,
//  a file that proves it (Inno constants allowed), three or four words of
//  use, and the uninstall registry key name (or ''). Add a var above for each.
function InitializeSetup(): Boolean;
begin
  //  Each line: name, winget id, an exe that answers --version, a file that
  //  proves it, what it is for in a few words, and the uninstall key name.
  //  Machine-wide tools go to their own default folders; an app upgrade must
  //  never remove them, so none is under {app}.
  iExifTool := homerAdd('ExifTool', 'OliverBetz.ExifTool', 'exiftool',
    '', 'reads what a recording or photo knows about itself', 'ExifTool');
  iFfmpeg := homerAdd('ffmpeg', 'Gyan.FFmpeg', 'ffmpeg',
    '', 'converts audio and video, with ffprobe', '');
  iImageMagick := homerAdd('ImageMagick', 'ImageMagick.ImageMagick', 'magick',
    '', 'converts and resizes images', 'ImageMagick');
  iMpv := homerAdd('mpv', 'shinchiro.mpv', 'mpv',
    '{pf}\MPV Player\mpv.exe', 'plays audio and video for the Player', 'mpv');
  iOllama := homerAdd('Ollama', 'Ollama.Ollama', 'ollama',
    '{localappdata}\Programs\Ollama\ollama.exe', 'runs the local AI model', 'Ollama');
  iPandoc := homerAdd('Pandoc', 'JohnMacFarlane.Pandoc', 'pandoc',
    '{pf}\Pandoc\pandoc.exe', 'converts documents between formats', 'Pandoc');
  iPdfTools := homerAdd('PDF tools', 'Python.Python.3.13', 'python',
    '', 'reads PDF files as text, through Python', 'Python 3.13');
  iYtDlp := homerAdd('yt-dlp', 'yt-dlp.yt-dlp', 'yt-dlp',
    '', 'fetches audio and video from web pages', '');
  sPriorVersion := '';
  Result := True;
end;

//  ---- the previous install, for the screen reader script entries ----------
function priorVersion(): String;
var
  sKey, sFound: String;
begin
  Result := '';
  sKey := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1';
  if RegQueryStringValue(HKLM, sKey, 'DisplayVersion', sFound) then Result := sFound
  else if RegQueryStringValue(HKCU, sKey, 'DisplayVersion', sFound) then Result := sFound;
end;

function isFreshInstall(): Boolean;
begin
  if sPriorVersion = '' then sPriorVersion := priorVersion();
  Result := (sPriorVersion = '');
end;

function isUpgradeOrSame(): Boolean;
begin
  Result := not isFreshInstall();
end;

//  versionPart: the Nth dotted number of a version, or 0 where there is none.
//  Written out rather than using PackVersionString, which this Inno Setup does
//  not have: the version check must not depend on the compiler's own version.
function versionPart(sVersion: String; iWanted: Integer): Integer;
var
  iAt, iPart: Integer;
  sNumber: String;
begin
  Result := 0;
  iPart := 1;
  sNumber := '';
  for iAt := 1 to Length(sVersion) do
  begin
    if sVersion[iAt] = '.' then
    begin
      if iPart = iWanted then begin Result := StrToIntDef(sNumber, 0); exit; end;
      iPart := iPart + 1;
      sNumber := '';
    end
    else if (sVersion[iAt] >= '0') and (sVersion[iAt] <= '9') then
      sNumber := sNumber + sVersion[iAt];
  end;
  if iPart = iWanted then Result := StrToIntDef(sNumber, 0);
end;

function versionIsOlder(sHave, sWant: String): Boolean;
var
  iPart, iHave, iWant: Integer;
begin
  Result := False;
  for iPart := 1 to 4 do
  begin
    iHave := versionPart(sHave, iPart);
    iWant := versionPart(sWant, iPart);
    if iHave < iWant then begin Result := True; exit; end;
    if iHave > iWant then exit;
  end;
end;

function isOlderInstalled(): Boolean;
begin
  Result := False;
  if isFreshInstall() then exit;
  Result := versionIsOlder(sPriorVersion, '{#AppVersion}');
end;

//  ---- checkbox wording and visibility, one line each ----------------------
//  AI NOTE FOR CUSTOMIZING: three functions per component, one per verb, and
//  a label function; two per model. Name the model as installModels.cmd does.
function labelExifTool(sParam: String): String;  begin Result := homerLabel(iExifTool); end;
function isInstallExifTool(): Boolean;           begin Result := homerIs(iExifTool, 0); end;
function isUpdateExifTool(): Boolean;            begin Result := homerIs(iExifTool, 1); end;
function isReinstallExifTool(): Boolean;         begin Result := homerIs(iExifTool, 2); end;
function labelffmpeg(sParam: String): String;  begin Result := homerLabel(iffmpeg); end;
function isInstallffmpeg(): Boolean;           begin Result := homerIs(iffmpeg, 0); end;
function isUpdateffmpeg(): Boolean;            begin Result := homerIs(iffmpeg, 1); end;
function isReinstallffmpeg(): Boolean;         begin Result := homerIs(iffmpeg, 2); end;
function labelImageMagick(sParam: String): String;  begin Result := homerLabel(iImageMagick); end;
function isInstallImageMagick(): Boolean;           begin Result := homerIs(iImageMagick, 0); end;
function isUpdateImageMagick(): Boolean;            begin Result := homerIs(iImageMagick, 1); end;
function isReinstallImageMagick(): Boolean;         begin Result := homerIs(iImageMagick, 2); end;
function labelmpv(sParam: String): String;  begin Result := homerLabel(impv); end;
function isInstallmpv(): Boolean;           begin Result := homerIs(impv, 0); end;
function isUpdatempv(): Boolean;            begin Result := homerIs(impv, 1); end;
function isReinstallmpv(): Boolean;         begin Result := homerIs(impv, 2); end;
function labelOllama(sParam: String): String;  begin Result := homerLabel(iOllama); end;
function isInstallOllama(): Boolean;           begin Result := homerIs(iOllama, 0); end;
function isUpdateOllama(): Boolean;            begin Result := homerIs(iOllama, 1); end;
function isReinstallOllama(): Boolean;         begin Result := homerIs(iOllama, 2); end;
function labelPandoc(sParam: String): String;  begin Result := homerLabel(iPandoc); end;
function isInstallPandoc(): Boolean;           begin Result := homerIs(iPandoc, 0); end;
function isUpdatePandoc(): Boolean;            begin Result := homerIs(iPandoc, 1); end;
function isReinstallPandoc(): Boolean;         begin Result := homerIs(iPandoc, 2); end;
function labelPdfTools(sParam: String): String;  begin Result := homerLabel(iPdfTools); end;
function isInstallPdfTools(): Boolean;           begin Result := homerIs(iPdfTools, 0); end;
function isUpdatePdfTools(): Boolean;            begin Result := homerIs(iPdfTools, 1); end;
function isReinstallPdfTools(): Boolean;         begin Result := homerIs(iPdfTools, 2); end;
function labelYtDlp(sParam: String): String;  begin Result := homerLabel(iYtDlp); end;
function isInstallYtDlp(): Boolean;           begin Result := homerIs(iYtDlp, 0); end;
function isUpdateYtDlp(): Boolean;            begin Result := homerIs(iYtDlp, 1); end;
function isReinstallYtDlp(): Boolean;         begin Result := homerIs(iYtDlp, 2); end;
function labelModel(sParam: String): String;   begin Result := homerModelLabel('qwen2.5:7b', 'translates text', 'about 4.7 GB'); end;
function isModelInstall(): Boolean;            begin Result := homerModelIs('qwen2.5:7b', False); end;
function isModelReinstall(): Boolean;          begin Result := homerModelIs('qwen2.5:7b', True); end;

//  ---- the Results box: one line per ticked box, probed after the scripts ran
procedure addAction(sText: String);
begin
  if sText = '' then exit;
  if sActions <> '' then sActions := sActions + #13#10;
  sActions := sActions + '  ' + sText;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
//  Finish pressed: the boxes are settled, the scripts have not yet run.
begin
  Result := True;
  if CurPageID = wpFinished then homerNoteTicked();
end;

procedure startIfAsked();
//  Starts the program if the Launch box left its marker, and removes the marker.
//  Started from cmd so it runs as the person, not as the elevated installer.
var
  sFlag: String;
  iResult: Integer;
begin
  sFlag := ExpandConstant('{localappdata}\{#AppName}\logs\{#AppName}_launch.flag');
  if not FileExists(sFlag) then exit;
  DeleteFile(sFlag);
  Exec(ExpandConstant('{cmd}'),
       '/s /c ""' + ExpandConstant('{app}\exec\{#AppExeName}') + '""',
       ExpandConstant('{userdocs}'), SW_SHOW, ewNoWait, iResult);
end;

procedure reportWhatHappened();
var
  sBody: String;
begin
  //  AI NOTE FOR CUSTOMIZING: one addAction per component and per model, in
  //  the same alphabetical order as the [Run] section.
  addAction(homerScreenReaderOutcome());
  addAction(homerOutcomeLine(iExifTool));
  addAction(homerOutcomeLine(iffmpeg));
  addAction(homerOutcomeLine(iImageMagick));
  addAction(homerOutcomeLine(impv));
  addAction(homerOutcomeLine(iOllama));
  addAction(homerOutcomeLine(iPandoc));
  addAction(homerOutcomeLine(iPdfTools));
  addAction(homerOutcomeLine(iYtDlp));
  addAction(homerModelOutcomeLine('qwen2.5:7b', 'translates text', 'about 4.7 GB'));
  sBody := '{#AppName} {#AppVersion} is installed.';
  if sActions <> '' then sBody := sBody + #13#10 + #13#10 + sActions;
  sBody := sBody + #13#10 + #13#10
         + 'Logs are kept in ' + ExpandConstant('{localappdata}\{#AppName}\logs') + '.';
  homerResultsBox(sBody);
  startIfAsked();
end;

//  ---- keep the setup log with the program's own logs ------------------------
//  Inno writes its log to the temporary folder, where nobody finds it. One
//  caveat: the installer runs elevated, so {localappdata} is the profile of
//  whoever answered the elevation prompt.
procedure keepSetupLog();
var
  sFolder, sTarget: String;
begin
  sFolder := ExpandConstant('{localappdata}\{#AppName}\logs');
  if not DirExists(sFolder) then
    if not ForceDirectories(sFolder) then exit;
  sTarget := sFolder + '\{#AppName}-setup-' + GetDateTimeString('yyyymmdd-hhnnss', #0, #0) + '.log';
  CopyFile(ExpandConstant('{log}'), sTarget, False);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssDone then
  begin
    keepSetupLog();
    reportWhatHappened();
  end;
end;

procedure CurPageChanged(CurPageID: Integer);
//  Say on the welcome page what is about to happen, because a user reading by
//  ear should not have to work it out from a version number in a caption.
begin
  if CurPageID = wpWelcome then
  begin
    if isFreshInstall() then
      WizardForm.WelcomeLabel1.Caption := 'Install {#AppName} {#AppVersion}'
    else if isOlderInstalled() then
      WizardForm.WelcomeLabel1.Caption := 'Update {#AppName} from ' + sPriorVersion + ' to {#AppVersion}'
    else
      WizardForm.WelcomeLabel1.Caption := 'Reinstall {#AppName} {#AppVersion}';
  end;
end;
