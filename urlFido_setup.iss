; ============================================================================
; urlFido_setup.iss  --  Inno Setup script for urlFido
;
; Compile with the Inno Setup IDE (or ISCC.exe) to produce
; urlFido_setup.exe. The resulting installer:
;   - Targets 64-bit Windows 10 (and later) only.
;   - Requires administrator privileges (writes to Program Files).
;   - Prompts for the installation directory; default C:\Program Files\urlFido.
;   - Shows a brief MIT license summary on the welcome page. The full
;     license text installs alongside the program as License.htm.
;   - Adds a Start Menu group with shortcuts to urlFido, the README,
;     and the uninstaller.
;   - Adds a desktop shortcut whose hotkey is Alt+Control+U. The shortcut
;     launches urlFido in GUI mode (-g) with saved-configuration
;     loading (-u). WorkingDir is the user's Documents folder so output
;     files and the optional log land somewhere writable.
;   - Adds no right-click Explorer verbs and no file associations.
;   - On uninstall, removes the program files but leaves
;     %LOCALAPPDATA%\urlFido\urlFido.inix intact (the user's saved
;     settings -- their filesystem, their call).
;
; NOTE on the hotkey: urlFido uses Alt+Control+U, which urlCheck currently
; uses. The plan is for urlCheck to move to Alt+Control+Shift+U in a
; future version so the more frequently used urlFido keeps the shorter
; chord. Until urlCheck is updated, installing both leaves two desktop
; shortcuts contending for Alt+Control+U; Windows binds the hotkey to
; whichever shortcut was created most recently. If needed, change one
; shortcut's hotkey via Alt+Enter (properties) on the desktop icon.
;
; This installer ships only the runtime distribution (the .exe, the
; HTML documentation, and the license). The Markdown sources, the C#
; source, the build script, and this .iss script live in the GitHub
; repository.
; ============================================================================

#define AppName       "urlFido"
#define AppVer        "1.0.0"
#define Publisher     "Jamal Mazrui"
#define AppUrl        "https://github.com/JamalMazrui/urlFido"
#define AppExeName    "urlFido.exe"
#define Copyright     "Copyright (c) 2026 Jamal Mazrui. MIT License."
; Inno's HotKey: directive requires "Ctrl" syntax; prose and
; user-visible text use "Control" per the project convention.
#define HotKeySyntax  "Alt+Ctrl+U"
#define HotKeyDisplay "Alt+Control+U"

[Setup]
; A fresh, unique AppId GUID (distinct from extCheck, 2htm, urlCheck).
AppId={{7F3A9C1E-6D24-4B57-A1E9-3B8F5C2D4A60}
AppName={#AppName}
AppVersion={#AppVer}
AppVerName={#AppName} {#AppVer}
AppPublisher={#Publisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}
AppUpdatesURL={#AppUrl}/releases
AppCopyright={#Copyright}
; tagRelease reads the version from THIS installer's version resource and
; tags v<that>. VersionInfoTextVersion is set explicitly so the string that
; .NET's FileVersionInfo.FileVersion reports is exactly "1.0.0" and the tag
; is a predictable v1.0.0 rather than a padded v1.0.0.0.
VersionInfoVersion={#AppVer}
VersionInfoTextVersion={#AppVer}

DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
UsePreviousAppDir=yes

DisableDirPage=no
UsePreviousGroup=yes

OutputDir=.
OutputBaseFilename={#AppName}_setup
Compression=lzma2
SolidCompression=yes
SetupIconFile={#AppName}.ico
WizardStyle=modern

PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

Uninstallable=yes
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName} {#AppVer}

MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
; Fold a brief MIT license notice into the welcome page rather than
; adding a dedicated license page. The full text installs as License.htm.
WelcomeLabel2=This will install [name/ver] on your computer.%n%n[name] downloads files of the types you choose (default: PDF) from web pages, driving your installed Microsoft Edge so pages behave as they do for a signed-in person.%n%n[name] is licensed under the MIT License: free to use, copy, modify, and distribute; provided "as is" with no warranty. The full license text will be installed as License.htm in the program folder.%n%nIt is recommended that you close all other applications before continuing.

[Files]
; Runtime distribution only. The icon is embedded in urlFido.exe at
; build time (csc /win32icon), so the .ico does not ship here.
Source: "{#AppName}.exe";  DestDir: "{app}"; Flags: ignoreversion
; NOTE: nvdaControllerClient.dll is NOT shipped here. It is embedded
; inside urlFido.exe as a resource at build time and extracted to
; %LOCALAPPDATA%\urlFido on first use, and then only when NVDA is
; actually running. urlFido.exe is therefore self-contained.
Source: "ReadMe.htm";       DestDir: "{app}"; Flags: ignoreversion
Source: "Announce.htm";     DestDir: "{app}"; Flags: ignoreversion
Source: "License.htm";      DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Start Menu group. WorkingDir is the user's Documents folder so
; downloaded files and the optional urlFido.log land somewhere
; writable (Program Files is not writable for non-admins).
Name: "{group}\{#AppName}"; \
  Filename: "{app}\{#AppExeName}"; \
  Parameters: "-g -u"; \
  WorkingDir: "{userdocs}"; \
  Comment: "Download files from web pages by extension"

Name: "{group}\{#AppName} ReadMe"; \
  Filename: "{app}\ReadMe.htm"; \
  WorkingDir: "{app}"; \
  Comment: "Documentation for {#AppName}"

Name: "{group}\Uninstall {#AppName}"; \
  Filename: "{uninstallexe}"; \
  Comment: "Remove {#AppName} from this computer"

; Desktop shortcut with the Alt+Ctrl+U hotkey. Launches urlFido in GUI
; mode (-g) with saved-configuration loading (-u). WorkingDir is the
; user's Documents folder for the same writability reason as above.
Name: "{userdesktop}\{#AppName}"; \
  Filename: "{app}\{#AppExeName}"; \
  WorkingDir: "{userdocs}"; \
  Parameters: "-g -u"; \
  HotKey: {#HotKeySyntax}; \
  Comment: "Download files from web pages ({#HotKeyDisplay})"

[Run]
; Post-install checkboxes on the final wizard page, both checked by
; default. The launch label reminds the user of the desktop hotkey.
FileName: "{app}\{#AppExeName}"; \
  Parameters: "-g"; \
  WorkingDir: "{userdocs}"; \
  Description: "Launch {#AppName} now (desktop hotkey: {#HotKeyDisplay})"; \
  Flags: nowait postinstall skipifsilent

FileName: "{app}\ReadMe.htm"; \
  Description: "Read documentation for {#AppName}"; \
  Flags: nowait postinstall skipifsilent shellexec

; NOTE: no [UninstallDelete] entry targets %LOCALAPPDATA%\urlFido, so
; the user's urlFido.inix is intentionally preserved on uninstall.
