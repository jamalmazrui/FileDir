[Setup]
AppName=FileDir
AppVerName=FileDir 3.9
AppPublisher=Jamal Mazrui
AppPublisherURL=http://NonvisualDevelopment.org
;DefaultDirName=C:\Program Files\FileDir
DefaultDirName={pf}\FileDir
;CreateAppDir=no
OutputDir=c:\FileDir
OutputBaseFilename=dirsetup
;DisableDirPage=yes
DefaultGroupName=FileDir
DisableProgramGroupPage=yes
DisableReadyPage=yes
;DisableFinishedPage=yes
Compression=lzma
SolidCompression=yes

[Files]
Source: "c:\FileDir\WebClient\*.*"; DestDir: "{app}\WebClient"; Flags: RecurseSubdirs IgnoreVersion
Source: "c:\FileDir\FileDir.cs"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\refresh.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\RunFileDir.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\RunFileDirLib.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\FileDir.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\FileDir64.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\7z.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\lbc.cs"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\RunLbc.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\lbc.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\LbcVB.VB"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\RunLbcVB.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\LbcVB.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\LbcJS.js"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\RunLbcJS.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\LbcJS.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\GetProps.js"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\chimes.wav"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\Tektosyne.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\ICSharpCode.SharpZipLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\FileAssociation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\FileDir.ini"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\AssocOn.bas"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\AssocOn.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\AssocOff.bas"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\AssocOff.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\FileDir.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\Quick.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\hotkeys.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\Hotkeys.ini"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\Convert.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\history.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\gpl.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\FileDir.htm"; DestDir: "{app}"; Flags: ignoreversion
;Source: "c:\FileDir\jfwapi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\nvdaControllerClient32.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\saapi32.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\WebGet.bas"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\WebGet.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\Burn2CD.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\Burn2CD.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source: "c:\FileDir\mcdbdll.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source: "c:\FileDir\FilterPackx86.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\gettext.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\gettext.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\filters\*.*"; DestDir: "{app}\filters"; Flags: ignoreversion
;Source: "c:\FileDir\JFWAPICtrl.dll"; DestDir: "{sys}"; Flags: RegServer
;Source: "c:\FileDir\JFWAPICtrl.dll"; DestDir: "{app}"; Flags: RegServer
Source: "c:\FileDir\dirsetup.iss"; DestDir: "{app}"; Flags: ignoreversion
Source: "c:\FileDir\PSetup.exe"; DestDir: "{app}"; 
Source: "c:\FileDir\PSetup.ini"; DestDir: "{app}"; 
Source: "c:\FileDir\unicows.dll"; DestDir: "{app}"; 
Source: "c:\jsx\jsx.exe"; DestDir: "{app}"; 
;Source: "C:\Documents and Settings\Owner\Application Data\GW Micro\Window-Eyes\users\default\FileDir.wepm"; DestDir: "{app}";
Source: "C:\FileDir\FileDir.wepm"; DestDir: "{app}";
Source: "c:\FileDir\dir_scr.zip"; DestDir: "{app}"; 
Source: "c:\FileDir\jsx.ini"; DestDir: "{app}"; 

[Dirs]
;Name: "{app}\Quick"

[Run]
FileName: "cmd.exe"; Parameters: "/c if exist ""{group}\Set .zip Extension to Open with FileDir.lnk"" del ""{group}\Set .zip Extension to Open with FileDir.lnk"""; Description: "Delete previous shortcut";
FileName: "cmd.exe"; Parameters: "/c if exist ""{group}\Turn off Association between .zip Extension and FileDir.lnk"" del ""{group}\Turn off Association between .zip Extension and FileDir.lnk"""; Description: "Delete previous shortcut";
FileName:"{app}\PSetup.exe"; Parameters: "/uionlyifneeded"; WorkingDir: "{app}"; Description: "Install required Microsoft components"; 
FileName: "{code:NETFile}"; Parameters: "uninstall FileDir /nologo /silent"; Flags: runhidden; Check: FileExists(ExpandConstant('{code:NETFile}'));
FileName: "{code:NETFile}"; Parameters: "install ""{app}\FileDir.exe"" /AppBase:""{app}"" /nologo /silent"; Flags: runhidden; Check: FileExists(ExpandConstant('{code:NETFile}'));
FileName: "cmd.exe"; Parameters: "/c if exist ""{app}\jfwapi.dll"" del ""{app}\jfwapi.dll"""; Description: "Remove jfwapi.dll from Program Folder (no longer needed)"; Flags:
FileName: "cmd.exe"; Parameters: "/c copy /y ""{app}\FileDir.lnk"" ""{userdesktop}"""; Description: "Set FileDir Shortcut Key to Alt+Control+F"; Flags: postinstall
; FileName:"{app}\FilterPackx86.exe"; Parameters: ""; WorkingDir: "{app}"; Description: "Install Microsoft filter pack so FileDir can read Office 2007 files"; Flags: postinstall unchecked
FileName:"http://download.microsoft.com/download/b/e/6/be61cfa4-b59e-4f26-a641-5dbf906dee24/filterpackx86.exe"; Parameters: ""; WorkingDir: "{app}"; Description: "Download and install Microsoft filter pack so FileDir can read Office 2007 files on 32-bit Windows"; Flags: postinstall unchecked shellexec waituntilterminated
FileName:"http://download.microsoft.com/download/b/e/6/be61cfa4-b59e-4f26-a641-5dbf906dee24/filterpackx64.exe"; Parameters: ""; WorkingDir: "{app}"; Description: "Download and install Microsoft filter pack so FileDir can read Office 2007 files on 64-bit Windows"; Flags: postinstall unchecked shellexec waituntilterminated
FileName:"{app}\jsx.exe"; Parameters: "dir_scr.zip"; WorkingDir: "{app}"; Description: "Install optional scripts to fine tune JAWS speech"; Flags: postinstall unchecked
FileName:"{app}\FileDir.wepm"; WorkingDir: "{app}"; Description: "Install optional scripts to fine tune Window-Eyes speech"; Flags: PostInstall Unchecked ShellExec
FileName:"{app}\FileDir.htm"; Description: "Read Documentation for FileDir"; Flags: shellexec skipifdoesntexist postinstall skipifsilent

[UninstallRun]
FileName: "{code:NETFile}"; Parameters: "uninstall FileDir /nologo /silent"; Flags: runhidden; Check: FileExists(ExpandConstant('{code:NETFile}'));

[UninstallDelete]
Type: files; Name: "{app}\FileDir.*"

[Icons]
Name: "{group}\Launch FileDir"; Filename: "{app}\FileDir.exe"; Parameters: ""; WorkingDir: "{app}";
Name: "{group}\Read Documentation for FileDir"; Filename: "{app}\FileDir.htm"; Flags: RunMaximized
Name: "{group}\Set folders and .zip Extension to Open with FileDir"; Filename: "{app}\assocon.exe"
Name: "{group}\Turn off Association between FileDir and folders or .zip Extension"; Filename: "{app}\assocoff.exe"
Name: "{group}\Uninstall FileDir"; Filename: "{uninstallexe}"
Name: "{group}\View License for FileDir"; Filename: "{app}\gpl.txt"; Flags: RunMaximized
Name: "{app}\FileDir"; HotKey: Alt+Ctrl+F; Filename: "{app}\FileDir.exe"; Parameters: ""; WorkingDir: "{app}";
Name: "{userdesktop}\FileDir"; Filename: "{app}\FileDir.exe"; Parameters: ""; WorkingDir: "{app}";

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\FileDir.exe"; ValueType: string; ValueName: ""; ValueData: "{app}\FileDir.exe";

[Code]
Var
i: Integer;
s: String;
sNETDir: String;
sNETFile: String;

i_result: Boolean;

const
MB_ICONINFORMATION = $40;

function FrameWorkName(Param: String): String;
var
Names: TArrayOfString;
I: Integer;
FrameworkInstall: Cardinal;
begin
Result := '';
if RegGetSubkeyNames(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP', Names) then begin
for I := 0 to GetArrayLength(Names) - 1 do begin
RegQueryDwordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\'+Names[I], 'Install', FrameworkInstall);
if Copy(Names[i], 1, 4) = 'v2.0' then begin
if FrameworkInstall = 1 then begin
Result := Names[I];
end;
end;
end;
end;
end;

function MessageBox(hWnd: Integer; lpText, lpCaption: String; uType: Cardinal): Integer;
external 'MessageBoxA@user32.dll stdcall';

Procedure Message(s_message: String);
Begin
MessageBox(0, s_message, 'Message', MB_OK or MB_ICONINFORMATION);
End;

procedure CurStepChanged(CurStep: TSetupStep);
Begin
If CurStep= ssInstall Then
Begin
sNETDir := FrameWorkName('');
//Result := False;
//Result := True;
End;
End;

Function NETFile(s: String): String;
Begin
If sNETDir <> '' Then
Begin
sNETFile := ExpandConstant('{win}') + '\Microsoft.NET\Framework\' + sNETDir + '\ngen.exe';
//Message(sNETFile);
if FileExists(sNETFile) then begin
//Message('yes');
end;
Result := sNETFile;
End;
End;
