; FileDir_Setup.iss -- Inno Setup script for the AnyCPU FileDir 5.0 beta (x64 and ARM64).
;
; Compile with ISCC.exe (Inno Setup 5.6+ or 6.x). Run BuildFileDir.cmd first
; so FileDir.exe exists. Produces FileDir_setup.exe in C:\FileDir.
;
; OutputBaseFilename is FileDir_Setup so the Elevate Version (F11) command can
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
;  - JAWS scripts still install via Scripts\FileDir_Scripts_setup.exe; moving to
;    a "FileDir.exe --install-jaws-settings" model is the JAWS step.
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
AppVerName=FileDir {#AppVersion} beta
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
Source: "ICSharpCode.SharpZipLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "FileAssociation.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source and build inputs (shipped so users can recompile, EdSharp-style).
Source: "FileDir.cs";         DestDir: "{app}"; Flags: ignoreversion
Source: "Web.cs";             DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Say.cs";             DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Inix.cs";             DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Util.cs";            DestDir: "{app}"; Flags: ignoreversion
Source: "Dialogs.cs";         DestDir: "{app}"; Flags: ignoreversion
Source: "FileDirScript.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.js";         DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.manifest";   DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "BuildFileDir.cmd";   DestDir: "{app}"; Flags: ignoreversion
Source: "FileDir_Setup.iss";  DestDir: "{app}"; Flags: ignoreversion
; Helper tools shipped alongside the app.
Source: "7z.*";               DestDir: "{app}"; Flags: ignoreversion
Source: "chimes.wav";         DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Burn2CD.exe";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Burn2CD.dll";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "WebGet.exe";         DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "AssocOn.exe";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "AssocOff.exe";       DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; Text-extraction engine: 2htm (plain-text mode) replaces gettext.exe + filters\.
Source: "2htm.exe";           DestDir: "{app}"; Flags: ignoreversion
; Extra-speech bridges (32-bit; dormant in the 64-bit process -- see header note).
Source: "nvdaControllerClient.dll";   DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; JAWS settings family (installed by Scripts\FileDir_Scripts_setup.exe in [Run]).
Source: "Scripts\*";          DestDir: "{app}\Scripts"; Flags: recursesubdirs ignoreversion skipifsourcedoesntexist
; Configuration: do NOT clobber a user's existing settings on upgrade.
Source: "FileDir.ini";        DestDir: "{app}"; Flags: onlyifdoesntexist
Source: "Hotkeys.ini";        DestDir: "{app}"; Flags: onlyifdoesntexist
; Documentation and licenses.
Source: "FileDir.md";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "History.md";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "FileDir.htm";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "History.htm";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "hotkeys.txt";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Convert.txt";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "Quick.txt";          DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "history.txt";        DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "gpl.txt";            DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
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
Type: files; Name: "{app}\lbc.cs"

[Icons]
Name: "{group}\Launch FileDir";   Filename: "{app}\FileDir.exe"; WorkingDir: "{app}"
Name: "{group}\FileDir Manual";   Filename: "{app}\FileDir.htm"
Name: "{group}\Set Extensions to Open with FileDir"; Filename: "{app}\AssocOn.exe"; WorkingDir: "{app}"
Name: "{group}\Turn off Association between Extensions and FileDir"; Filename: "{app}\AssocOff.exe"; WorkingDir: "{app}"
Name: "{group}\View License for FileDir"; Filename: "{app}\gpl.txt"
Name: "{group}\Uninstall FileDir"; Filename: "{uninstallexe}"
; Single hot-key shortcut (DbDo/EdSharp model): the one shortcut that owns
; Alt+Ctrl+F is created with {autodesktop} (user desktop for a per-user install,
; common desktop for an all-users install) and HotKey. No Start Menu item carries
; a hot key, so Alt+Ctrl+F has exactly one owner. FileDir is single-instance:
; OnStartupNextInstance brings the running copy to the foreground, so a plain
; relaunch activates rather than starting a second copy.
Name: "{autodesktop}\FileDir"; Filename: "{app}\FileDir.exe"; WorkingDir: "{app}"; IconFilename: "{app}\FileDir.ico"; HotKey: Alt+Ctrl+F; Comment: "Launch or activate FileDir 5.0 (Alt+Control+F)"

[Run]
; Optional JAWS scripts (Finish-page checkbox). Interim: delegates to the existing
; Scripts installer; later this becomes "FileDir.exe --install-jaws-settings".
Filename: "{app}\Scripts\FileDir_Scripts_setup.exe"; WorkingDir: "{app}"; Description: "Install optional JAWS scripts to fine-tune FileDir speech"; Flags: postinstall skipifsilent unchecked skipifdoesntexist
; Pre-generate native images for faster startup (64-bit ngen).
Filename: "{code:NgenExe}"; Parameters: "uninstall FileDir /nologo /silent"; Flags: runhidden; Check: HasNgen
Filename: "{code:NgenExe}"; Parameters: "install ""{app}\FileDir.exe"" /AppBase:""{app}"" /nologo /silent"; Flags: runhidden; Check: HasNgen
; Offer the documentation on the Finish page.
Filename: "{app}\FileDir.htm"; Description: "Read FileDir documentation"; Flags: postinstall shellexec skipifsilent skipifdoesntexist

[UninstallRun]
Filename: "{code:NgenExe}"; Parameters: "uninstall FileDir /nologo /silent"; Flags: runhidden; Check: HasNgen

[UninstallDelete]
Type: files; Name: "{app}\FileDir.exe"
Type: files; Name: "{app}\BuildFileDir.log"

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\FileDir.exe"; ValueType: string; ValueName: ""; ValueData: "{app}\FileDir.exe"; Flags: uninsdeletekey

[Code]
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




