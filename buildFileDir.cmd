@echo off
rem ===================================================================
rem buildFileDir.cmd -- build FileDir.exe from FileDir.cs and the Homer
rem Development Kit modules in C:\HomerDev.
rem
rem This is the HomerDev TEMPLATE. newHomerApp.cmd writes a copy of it
rem with FileDir replaced by a real app name. If you are reading the copy,
rem the app name is already in place and you can edit freely.
rem
rem KIT: the shared C# modules are NOT copied into the app folder. They
rem are compiled straight out of the kit, so there is one copy of Lbc.cs
rem on the machine and every app gets a fix the moment the kit gets it.
rem
rem WHERE THE KIT IS LOOKED FOR, in order, first hit wins:
rem   1. %HomerDev%        the environment variable, when it is set
rem   2. C:\HomerDev        the usual place
rem   3. the current directory, for a folder that carries its own copy
rem
rem The third is what lets a sample, a demonstration, or a machine with no
rem kit installed still build: drop the CSharp folder beside the source.
rem
rem VERSION: version.txt is the SINGLE source of truth. It holds one
rem line, nothing else. This script increments it on every build --
rem stepping over any number already released, which it learns from the
rem repository's own tags -- then generates Version.cs from it, so the
rem running program reports the same number. FileDir_setup.iss reads
rem version.txt directly, so the installer reports it too, and
rem tagRelease reads it back out of the built setup's version resource
rem to form the tag. No version literal appears anywhere else, so a
rem stale file cannot rewind it.
rem
rem   buildFileDir.cmd          increments the version, then builds
rem   buildFileDir.cmd nobump   keeps the current number
rem
rem COMPILER: Roslyn is preferred, from Visual Studio or the free Build
rem Tools. The pre-Roslyn csc.exe under Microsoft.NET\Framework64 is
rem accepted as a fallback, but the Homer modules use language features
rem beyond C# 5, so if that fallback is taken and Lbc.cs or Inix.cs
rem fails to compile, install Build Tools:
rem https://visualstudio.microsoft.com/downloads/
rem
rem REFERENCES: three assemblies are NOT on the compiler's default
rem reference path and must be given by full path, or the build fails
rem with CS0006:
rem   System.Speech.dll        -- the Windows voices
rem   UIAutomationProvider.dll -- Say.cs, Narrator notification events
rem   UIAutomationTypes.dll    -- Say.cs
rem Inix.cs additionally needs System.IO.Compression and System.Xml,
rem which are part of the Framework and are referenced below.
rem
rem PARSE-TIME PITFALL: the variable NAME ProgramFiles(x86) contains
rem parentheses, and cmd.exe scans a parenthesised block for its closing
rem paren BEFORE expanding variables. Every search below is therefore a
rem single-line "if not defined X if exist ... set" chain, never a block.
rem
rem Output in this folder: FileDir.exe, and the installer if Inno Setup is
rem present. Everything is logged to logs\FileDir-build-<date>-<time>.log.
rem ===================================================================

setlocal enabledelayedexpansion
cd /d "%~dp0"

set "app=FileDir"
rem EVERY SESSION ITS OWN LOG, IN logs\, named as the program names its own:
rem <App>-build-yyyyMMdd-HHmmss.log. An alphabetical sort is then a
rem chronological one, and zipping logs\ gathers everything.
for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "sStamp=%%i"
if not exist "%~dp0logs" mkdir "%~dp0logs"
set "log=%~dp0logs\%app%-build-%sStamp%.log"
rem THE START AND END LINES CARRY AN ISO 8601 TIME (HomerDev 1.43.21), with
rem the UTC offset, from PowerShell rather than %DATE% %TIME%, whose form
rem follows the regional settings; and they name the event and its result as
rem every Homer log does.
for /f "usebackq delims=" %%i in (`powershell -NoProfile -Command "Get-Date -Format 'yyyy-MM-ddTHH:mm:ss.fffzzz'"`) do set "sIso=%%i"
> "%log%" echo %sIso% INFO  build start app=%app%
echo Script: %~f0>> "%log%"
echo Folder: %CD%>> "%log%"
echo Command line: %0 %*>> "%log%"
echo Build log: %log%

rem ---- the Homer Development Kit -------------------------------------
set "homerDev="
if defined HomerDev if exist "%HomerDev%\exec\CSharp\Lbc.cs" set "homerDev=%HomerDev%"
if not defined homerDev if exist "C:\HomerDev\exec\CSharp\Lbc.cs" set "homerDev=C:\HomerDev"
if not defined homerDev if exist "%CD%\exec\CSharp\Lbc.cs" set "homerDev=%CD%"
if not defined homerDev (
  echo ERROR: the Homer Development Kit was not found.
  echo         Looked in %%HomerDev%%, C:\HomerDev, and this folder.
  echo         Unpack HomerDev.zip into C:\HomerDev, or set HomerDev to where it is.
  echo ERROR: no kit found.>> "%log%"
  goto :failed
)
set "homerVer=unknown"
if exist "!homerDev!\version.txt" set /p homerVer=<"!homerDev!\version.txt"
rem ---- is the kit new enough? ----------------------------------------
rem WHAT version.txt ACTUALLY CONTAINS is not always a bare number. It is read
rem here with "set /p", which hands back whatever is on the first line: a byte
rem order mark shows up as three characters in front, and a trailing space or
rem tab comes through as well. Neither is a version any comparison can parse.
rem
rem So the number is cleaned before it is used, and -- this is the part that
rem cost a build to learn -- a version that CANNOT BE PARSED is reported as
rem exactly that, with the raw text in the log. The first attempt treated any
rem failure as "too old", which produced "kit 1.40.1 is older than 1.40.1": a
rem message that sent the reader looking for the wrong problem.
for /f "delims=0123456789." %%C in ("!homerVer!") do set "homerVer=!homerVer:%%C=!"
set "homerVer=!homerVer: =!"

rem THE KIT VERSION THIS APP NEEDS. Raised whenever FileDir starts depending on
rem something new in the kit, so an old kit stops here with a sentence rather
rem than somewhere inside the compiler. 1.41.2 is the release that took in
rem FileDir's work on Lbc -- the slider, the list searching, the status line,
rem the command-key hooks and the accessible-name clean-out -- with the two
rem corrections that followed it.
set "kitNeeded=1.43.29"
rem COMPARED IN CMD, WITH NO POWERSHELL AT ALL. Three attempts had PowerShell
rem parse the two numbers, and every one reported a perfectly good version as
rem unreadable -- the quoting between cmd and PowerShell was never right, and
rem nothing in the logs could show which character was the trouble. Two dotted
rem numbers do not need another language: split each on the dots and compare
rem the parts as numbers, which cmd does natively with lss and gtr.
set "iKitCheck=0"
set "kA=0" & set "kB=0" & set "kC=0"
set "nA=0" & set "nB=0" & set "nC=0"
for /f "tokens=1-3 delims=." %%a in ("!homerVer!") do (
  if not "%%a"=="" set "kA=%%a"
  if not "%%b"=="" set "kB=%%b"
  if not "%%c"=="" set "kC=%%c"
)
for /f "tokens=1-3 delims=." %%a in ("!kitNeeded!") do (
  if not "%%a"=="" set "nA=%%a"
  if not "%%b"=="" set "nB=%%b"
  if not "%%c"=="" set "nC=%%c"
)
rem A part that is not a number means the version could not be read.
for %%P in (kA kB kC) do (
  set "sPart=!%%P!"
  for /f "delims=0123456789" %%X in ("!sPart!") do set "iKitCheck=2"
)
if "!iKitCheck!"=="0" (
  if !kA! lss !nA! set "iKitCheck=1"
  if !kA! equ !nA! if !kB! lss !nB! set "iKitCheck=1"
  if !kA! equ !nA! if !kB! equ !nB! if !kC! lss !nC! set "iKitCheck=1"
)
echo Kit version read as [!homerVer!] = !kA!.!kB!.!kC!, needed !kitNeeded!, check returned !iKitCheck!.>> "%log%"

rem A CHECK THAT CANNOT READ THE VERSION DOES NOT STOP THE BUILD. It exists to
rem turn "an old kit" into a sentence somebody can act on, not to be a gate of
rem its own; if the kit really is too old the compiler says which member is
rem missing, which is the better message. Only a version that reads cleanly AND
rem is genuinely older stops the build.
if "!iKitCheck!"=="2" (
  echo Could not read the kit version from !homerDev!\version.txt. Carrying on.
  echo WARN: could not parse the kit version from [!homerVer!]; continuing.>> "%log%"
)
if "!iKitCheck!"=="1" (
  echo FileDir needs HomerDev !kitNeeded! or later, and the kit is !homerVer!.
  echo Unzip the newer HomerDev, run buildHomerDev, then build again.
  echo ERROR: kit !homerVer! is older than !kitNeeded!.>> "%log%"
  goto :failed
)
echo Kit: !homerDev! version !homerVer!
echo Kit: !homerDev! version !homerVer!>> "%log%"

rem ---- the Homer modules this app compiles in -------------------------
rem Alphabetical, as every list in Homer code is unless another order is
rem clearly more logical. Comment out the ones this app does not use; an
rem unused module costs only build time, so when in doubt leave it in.
rem A MODULE MAY NEED ANOTHER MODULE, and only two do. Mdi.cs uses KeyMap to
rem register every command as it is added, so the two are switched on together:
rem turning on Mdi without KeyMap fails to compile with "The name 'KeyMap' does
rem not exist in the current context", which is exactly how this comment came to
rem be written. Nothing else in the kit has a dependency of its own.
set "homerSources="
rem Elevate.cs: Lbc's Help box checks the web for a newer release through it.
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Elevate.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Inix.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\KeyName.cs""
rem MDI ONLY (EdSharp, FileDir, DbDo): a multiple-document app needs both of
rem these, and needs them together. Uncomment the pair.
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\KeyMap.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Mdi.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Lbc.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Log.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Paths.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Ollama.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Say.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Util.cs""
set "homerSources=!homerSources! "!homerDev!\exec\CSharp\Web.cs""
echo Homer modules: !homerSources!>> "%log%"

rem ---- component options ----------------------------------------------
rem EVERY COMPONENT ANY HOMER APP HAS EVER NEEDED IS LISTED HERE. The ones
rem used by MORE THAN ONE app are switched on, because that is the evidence
rem that the next app will want them too; the ones used by a single app are
rem left commented with the app named, so turning one on is one character.
rem
rem AI NOTE: to add a component to an app, uncomment its line here and, if it
rem needs fetching, the matching block further down. Do not invent a new
rem mechanism -- every block below follows the same shape: look for it, fetch
rem it when missing, log what happened, fail loudly if it cannot be had.
rem
rem On in the template, because more than one app uses each:
set "useConfigFile=1"
set "useDocs=1"
set "useIcon=1"
set "useInstaller=1"
set "useManifest=1"
set "useNuGet=1"
set "useScreenReaderScripts=1"
set "useVersionSteps=1"
rem
rem Off in the template, each used by one app so far. The app is named so you
rem know where to look for a working example.
rem set "useExifTool=1"        rem HomerScribe: writes descriptions into photographs
rem set "useFfmpeg=1"          rem HomerScribe: video and audio work, with yt-dlp
rem set "useMarkdig=1"         rem 2htm: Markdown to HTML, embedded as a resource
rem set "useNpoi=1"            rem DbDo: .xlsx without Excel
rem set "usePdfPig=1"          rem HomerScribe: reading a PDF with positions
rem set "useSqlite=1"          rem DbDo: System.Data.SQLite and the SQLean shell
rem set "useTesseract=1"       rem HomerScribe: reading scanned text quickly
set "useUde=1"             rem EdSharp and FileDir: detecting a text file's encoding
rem set "useWhisper=1"         rem HomerScribe: transcribing speech

rem ---- version: version.txt is the single source of truth -----------
rem A MISSING version.txt IS MADE, NOT AN ERROR. Every build needs a number --
rem the program reports it, the installer carries it, the release tag is it --
rem so a folder without one gets 1.0.0 and carries on. Stopping here would
rem leave a manual step, which no Homer build does.
if not exist "version.txt" (
  > version.txt echo 1.0.0
  echo No version.txt here, so it was created holding 1.0.0.
  echo Created version.txt holding 1.0.0>> "%log%"
)
set "ver="
set /p ver=<version.txt
set "ver=!ver: =!"
for /f "delims=0123456789." %%C in ("!ver!") do set "ver=!ver:%%C=!"
if "!ver!"=="" (
  echo ERROR: version.txt is empty.
  echo ERROR: version.txt is empty.>> "%log%"
  goto :failed
)
if /i "%~1"=="nobump" goto :keepVersion
if not defined useVersionSteps goto :keepVersion
call :takeNextVersion
goto :haveVersion

:keepVersion
echo Version: !ver! ^(nobump: keeping the current number^)
echo Version: !ver! ^(nobump^)>> "%log%"

:haveVersion

rem ---- generate Version.cs from version.txt -------------------------
rem Generated output: do not edit it, and do not commit it.
> Version.cs echo // Generated by build%app%.cmd from version.txt.  Do not edit; do not commit.
>> Version.cs echo public static class BuildVersion
>> Version.cs echo {
>> Version.cs echo     public const string Version = "!ver!";
>> Version.cs echo }

rem ---- the hotkey table and the hotkey reference ----------------------
rem configs\Hotkeys.inix is the single authored source for every command name,
rem its key and its description. KeyText.cs is compiled into the program so a
rem machine whose configs\Hotkeys.inix predates a new command still describes
rem it; help\Hotkeys.md is the reference the installer ships.
if exist "scripts\makeKeyMap.py" (
  python "scripts\makeKeyMap.py" >> "%log%" 2>&1
  if errorlevel 1 (
    echo ERROR: makeKeyMap.py failed. See the keymap log in logs\.
    echo ERROR: makeKeyMap.py failed.>> "%log%"
    goto :failed
  )
  echo Hotkey table and reference written.
)

rem ---- the JScript.NET half, FileDirScript.dll ------------------------
rem FileDir.js holds the scripting FileDir exposes to users. jsc.exe builds it
rem into a library the program loads; without it the scripting commands are the
rem only thing missing, so a failure here is reported and the build carries on.
set "jsc="
if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\jsc.exe" set "jsc=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\jsc.exe"
if defined jsc if exist "FileDir.js" (
  "!jsc!" /nologo /target:library /out:exec\FileDirScript.dll FileDir.js >> "%log%" 2>&1
  if errorlevel 1 echo WARN: FileDirScript.dll could not be built; the scripting commands will be missing.>> "%log%"
)

rem ---- locate the compiler ------------------------------------------
set "csc="
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Buildscripts\MSBuild\Current\Bin\Roslyn\csc.exe" set "csc=C:\Program Files (x86)\Microsoft Visual Studio\2022\Buildscripts\MSBuild\Current\Bin\Roslyn\csc.exe"
if not defined csc if exist "C:\Program Files\Microsoft Visual Studio\2022\Buildscripts\MSBuild\Current\Bin\Roslyn\csc.exe" set "csc=C:\Program Files\Microsoft Visual Studio\2022\Buildscripts\MSBuild\Current\Bin\Roslyn\csc.exe"
if not defined csc if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe" set "csc=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
if not defined csc if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe" set "csc=C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
if not defined csc if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe" set "csc=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe"
if not defined csc if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe" set "csc=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe"
if not defined csc if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Buildscripts\MSBuild\Current\Bin\Roslyn\csc.exe" set "csc=C:\Program Files (x86)\Microsoft Visual Studio\2019\Buildscripts\MSBuild\Current\Bin\Roslyn\csc.exe"
if not defined csc if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "csc=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not defined csc (
  echo ERROR: no C# compiler was found. Install the Visual Studio Build Tools:
  echo         https://visualstudio.microsoft.com/downloads/
  echo ERROR: no csc.exe found.>> "%log%"
  goto :failed
)
echo Compiler: !csc!
echo Compiler: !csc!>> "%log%"

rem ---- locate the reference assemblies given by full path -----------
rem Copy the paren-bearing root into a paren-free name before any block.
set "progFiles86=%ProgramFiles(x86)%"
set "progFiles=%ProgramFiles%"
set "refBase=Reference Assemblies\Microsoft\Framework\.NETFramework"

set "speech="
set "uiaProv="
set "uiaTypes="

for %%v in (v4.8 v4.7.2 v4.7.1 v4.7 v4.6.2 v4.6.1 v4.6 v4.5.2) do (
  if not defined speech if exist "!progFiles86!\!refBase!\%%v\System.Speech.dll" set "speech=!progFiles86!\!refBase!\%%v\System.Speech.dll"
  if not defined speech if exist "!progFiles!\!refBase!\%%v\System.Speech.dll" set "speech=!progFiles!\!refBase!\%%v\System.Speech.dll"
  if not defined uiaProv if exist "!progFiles86!\!refBase!\%%v\UIAutomationProvider.dll" set "uiaProv=!progFiles86!\!refBase!\%%v\UIAutomationProvider.dll"
  if not defined uiaProv if exist "!progFiles!\!refBase!\%%v\UIAutomationProvider.dll" set "uiaProv=!progFiles!\!refBase!\%%v\UIAutomationProvider.dll"
  if not defined uiaTypes if exist "!progFiles86!\!refBase!\%%v\UIAutomationTypes.dll" set "uiaTypes=!progFiles86!\!refBase!\%%v\UIAutomationTypes.dll"
  if not defined uiaTypes if exist "!progFiles!\!refBase!\%%v\UIAutomationTypes.dll" set "uiaTypes=!progFiles!\!refBase!\%%v\UIAutomationTypes.dll"
)

rem Fallbacks: the assembly cache and the runtime WPF folder.
if not defined speech if exist "%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\System.Speech\v4.0_4.0.0.0__31bf3856ad364e35\System.Speech.dll" set "speech=%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\System.Speech\v4.0_4.0.0.0__31bf3856ad364e35\System.Speech.dll"
if not defined uiaProv if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\WPF\UIAutomationProvider.dll" set "uiaProv=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\WPF\UIAutomationProvider.dll"
if not defined uiaTypes if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\WPF\UIAutomationTypes.dll" set "uiaTypes=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\WPF\UIAutomationTypes.dll"
if not defined uiaProv if exist "%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\UIAutomationProvider\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationProvider.dll" set "uiaProv=%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\UIAutomationProvider\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationProvider.dll"
if not defined uiaTypes if exist "%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll" set "uiaTypes=%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll"

if not defined speech (
  echo ERROR: System.Speech.dll was not found.
  echo         Install the .NET Framework 4.8 Developer Pack:
  echo         https://dotnet.microsoft.com/download/dotnet-framework/net48
  echo ERROR: System.Speech.dll was not found.>> "%log%"
  goto :failed
)
if not defined uiaProv (
  echo ERROR: UIAutomationProvider.dll was not found. Install the .NET Framework 4.8 Developer Pack.
  echo ERROR: UIAutomationProvider.dll was not found.>> "%log%"
  goto :failed
)
if not defined uiaTypes (
  echo ERROR: UIAutomationTypes.dll was not found. Install the .NET Framework 4.8 Developer Pack.
  echo ERROR: UIAutomationTypes.dll was not found.>> "%log%"
  goto :failed
)
echo Speech: !speech!>> "%log%"
echo UI Automation: !uiaProv!>> "%log%"

rem ---- components fetched from the web --------------------------------
rem Nothing here is committed to the repository: a build fetches what it needs,
rem so a fresh clone builds with nothing to install by hand. Every block is
rem idempotent -- a file already present is left alone.
set "extraRefs="

rem NuGet, the one mechanism all of these share. :getNuGet takes a package id
rem and an assembly name, and leaves the .dll in this folder.
rem   call :getNuGet Markdig Markdig.dll

if not defined useMarkdig goto :noMarkdig
call :getNuGet Markdig Markdig.dll
if not exist "Markdig.dll" goto :failed
rem Embedded rather than shipped beside the .exe, which is what keeps the
rem program one self-contained file; the app must resolve it in AssemblyResolve.
set "extraRefs=!extraRefs! /reference:Markdig.dll /resource:Markdig.dll,Markdig.dll"
:noMarkdig

if not defined useNpoi goto :noNpoi
call :getNuGet NPOI NPOI.dll
set "extraRefs=!extraRefs! /reference:NPOI.dll"
:noNpoi

if not defined usePdfPig goto :noPdfPig
call :getNuGet PdfPig UglyToad.PdfPig.dll
set "extraRefs=!extraRefs! /reference:UglyToad.PdfPig.dll"
:noPdfPig

if not defined useSqlite goto :noSqlite
call :getNuGet System.Data.SQLite.Core System.Data.SQLite.dll
set "extraRefs=!extraRefs! /reference:System.Data.SQLite.dll"
:noSqlite

rem ---- the four libraries FileDir links -------------------------------
rem THE COMPILE FAILED WITHOUT THESE, and the reason is worth writing down: the
rem template's compile line references only what every app uses, and FileDir
rem uses four more. They live in exec, beside the program that loads them, and
rem the build fetches any that are missing rather than leaving a manual step.
rem
rem   Ude                         detecting a text file's encoding
rem   ICSharpCode.SharpZipLib     reading and writing zip archives
rem   Tektosyne                   the geometry and collections FileDir sorts with
rem   FileAssociation             BrendanGrant's helper for file associations
rem
rem A library already in exec is left alone: these change rarely, and a build
rem that re-fetched them every time would be slower for nothing.
if not exist "exec" mkdir "exec"
call :getLib UDE.CSharp Ude.dll
call :getLib SharpZipLib ICSharpCode.SharpZipLib.dll
call :getLib Tektosyne Tektosyne.dll
call :getLib FileAssociation FileAssociation.dll
for %%L in (Ude.dll ICSharpCode.SharpZipLib.dll Tektosyne.dll FileAssociation.dll) do (
  if exist "exec\%%L" (
    set "extraRefs=!extraRefs! /reference:exec\%%L"
  ) else (
    echo ERROR: %%L is missing and could not be fetched. FileDir cannot compile without it.
    echo ERROR: %%L missing.>> "%log%"
    goto :failed
  )
)
:noUde

rem The tools below are PROGRAMS rather than assemblies, so they are not
rem referenced by the compiler. They are fetched here only when the installer
rem packages them; an app that finds them on the PATH at run time needs none of
rem this. See buildHomerScribe.cmd for worked versions of all four.
rem   ffmpeg and yt-dlp  -- winget, or a direct download of the release zip
rem   exiftool           -- a single .exe from exiftool.org
rem   tesseract          -- winget: UB-Mannheim.TesseractOCR
rem   whisper            -- pip install, into the app's own virtual environment

rem ---- optional icon ------------------------------------------------
set "icon="
if defined useIcon if exist "%app%.ico" set "icon=/win32icon:%app%.ico"

rem ---- optional application manifest ---------------------------------
rem A manifest asks Windows for a privilege level and declares the Windows
rem versions the program understands. Compile with /nowin32manifest when one is
rem supplied, or the compiler embeds its own and the file is ignored.
set "manifest="
if defined useManifest if exist "%app%.manifest" set "manifest=/nowin32manifest /win32manifest:%app%.manifest"

rem ---- is OUR program still running? ---------------------------------
rem THE ONE THAT MATTERS IS exec\FileDir.exe, THIS FOLDER'S COPY. The FileDir a
rem person works in all day is the installed one under Program Files, and it
rem shares nothing with the file the compiler is about to write, so it is left
rem exactly as it is. An earlier version of this script asked every FileDir.exe
rem to close, which shut the person's file manager mid-task; that was wrong.
rem
rem Only a process running from exec would hold the output file open. It is
rem reported, not closed: the build stops and says which one, and the person
rem decides.
if not exist "exec" mkdir "exec"
set "sOurExe=%CD%\exec\%app%.exe"
powershell -NoProfile -Command "$p = Get-Process -Name '%app%' -ErrorAction SilentlyContinue | Where-Object { $_.Path -and ($_.Path -ieq '%sOurExe%') }; if ($p) { exit 1 } else { exit 0 }" >nul 2>&1
if errorlevel 1 (
  echo ERROR: exec\%app%.exe is running from this folder. Close that copy and run this again.
  echo ERROR: exec\%app%.exe is running; the compiler cannot replace it.>> "%log%"
  goto :failed
)
echo exec\%app%.exe is not running.>> "%log%"

rem ---- compile ------------------------------------------------------
rem One assembly, so the result is a single self-contained executable:
rem   Version.cs   -- generated above from version.txt
rem   %app%.cs     -- the program
rem   and the Homer modules listed at the top, compiled from the kit.
echo Compiling>> "%log%"
echo(>> "%log%"
"!csc!" /nologo /target:winexe /platform:x64 /optimize+ ^
  /reference:System.dll ^
  /reference:System.Core.dll ^
  /reference:System.Data.dll ^
  /reference:System.Drawing.dll ^
  /reference:System.Windows.Forms.dll ^
  /reference:System.Web.dll ^
  /reference:System.Web.Extensions.dll ^
  /reference:System.Net.Http.dll ^
  /reference:System.Xml.dll ^
  /reference:System.IO.Compression.dll ^
  /reference:System.IO.Compression.FileSystem.dll ^
  /reference:Microsoft.VisualBasic.dll ^
  /reference:"!speech!" ^
  /reference:"!uiaProv!" ^
  /reference:"!uiaTypes!" ^
  !extraRefs! ^
  !icon! ^
  !manifest! ^
  /out:exec\%app%.exe ^
  Version.cs KeyText.cs ^
  FileDir.cs Convert.cs Dialogs.cs Media.cs MediaPlayer.cs Mpv.cs Table.cs ^
  !homerSources! >> "%log%" 2>&1

set iBuildResult=%ERRORLEVEL%
type "%log%"
if not "%iBuildResult%"=="0" (
  echo(
  echo ERROR: the build failed. Details above and in %log%.
  goto :failed
)
echo Built exec\%app%.exe version !ver!>> "%log%"
echo(
echo Built exec\%app%.exe version !ver!

rem A <App>.exe.config beside the program is left exactly as it is: it is
rem source, not output, and the installer ships it. It is where a runtime
rem version or an assembly binding redirect goes.
if defined useConfigFile if not exist "%app%.exe.config" echo NOTE: no %app%.exe.config here; the program will take the runtime defaults.>> "%log%"
rem The config is read from beside the program, and the program is in exec.
if exist "%app%.exe.config" copy /y "%app%.exe.config" "exec\%app%.exe.config" >nul && echo Copied %app%.exe.config beside the program.>> "%log%"

rem ---- documentation -------------------------------------------------
rem Every .md ships with a matching .htm. Pandoc writes them when it is
rem on the PATH; without it the .md files travel alone and the installer
rem lines that name .htm are skipped.
if not defined useDocs goto :docsDone
where pandoc >nul 2>&1
if errorlevel 1 (
  rem FETCH IT RATHER THAN ASK FOR IT. "Install pandoc and run me again" is a
  rem manual step, and a Homer build script does not leave one.
  echo Installing pandoc, which writes the .htm copies of the documents...
  echo Pandoc not found; installing with winget>> "%log%"
  winget install --id JohnMacFarlane.Pandoc --silent --accept-source-agreements --accept-package-agreements >> "%log%" 2>&1
)
where pandoc >nul 2>&1
if errorlevel 1 (
  echo Pandoc could not be installed, so the .htm files were not rebuilt.>> "%log%"
  echo NOTE: pandoc could not be installed, so the .htm files were not rebuilt.
) else (
  for %%m in (*.md) do (
    pandoc -f markdown -t html5 --standalone --metadata title="%%~nm" -o "%%~nm.htm" "%%m" >> "%log%" 2>&1
    if errorlevel 1 echo WARN: pandoc failed on %%m>> "%log%"
  )
  for %%m in (help\*.md) do (
    pandoc -f markdown -t html5 --standalone --metadata title="%%~nm" -o "help\%%~nm.htm" "%%m" >> "%log%" 2>&1
    if errorlevel 1 echo WARN: pandoc failed on %%m>> "%log%"
  )
  echo Documentation converted with pandoc.>> "%log%"
)
:docsDone

rem ---- screen reader scripts -----------------------------------------
rem The JAWS scripts are shipped as <App>_JAWS.zip and the NVDA add-on as
rem <App>.nvda-addon; the installer offers both, checked by default. Packing
rem them here means the installer always carries the current ones.
rem 2HTM GOES WITH FILEDIR (30 September 2026). Question Mark reads legacy Office
rem files and PDFs through 2htm, and the installer ships exec\2htm.exe only when
rem one is there -- this build never put one there, so Question Mark usually
rem found none. The current 2htm is taken from the 2htm project beside this
rem one (C:\2htm\exec), and the log says whether it was.
if exist "%~dp0..\2htm\exec\2htm.exe" (
  copy /y "%~dp0..\2htm\exec\2htm.exe" "exec\2htm.exe" >> "%log%" 2>&1
  echo Copied 2htm.exe from %~dp0..\2htm\exec into exec, exit !errorlevel!>> "%log%"
) else (
  echo WARNING: no 2htm.exe in %~dp0..\2htm\exec; build 2htm first, or Question Mark will look for an installed 2htm.>> "%log%"
)
rem Old stand-alone JAWS script installers, which nothing builds now: removed.
for %%F in ("scripts\FileDir_Scripts_setup.iss" "scripts\jaws\FileDir_Scripts_setup.iss") do (
  if exist %%F (
    del /q %%F >> "%log%" 2>&1
    echo Removed %%~F, an old stand-alone JAWS script installer.>> "%log%"
  )
)
if not defined useScreenReaderScripts goto :readersDone
rem Old compiled scripts in scripts\jaws are removed: nothing ships a .jsb.
if exist "scripts\jaws\*.jsb" (
  del /q "scripts\jaws\*.jsb" >> "%log%" 2>&1
  echo Removed the compiled .jsb files from scripts\jaws; the installer compiles its own.>> "%log%"
)
if exist "scripts\jaws\*.js*" (
  rem SOURCES ONLY, NEVER A COMPILED .jsb (HomerDev 1.43.38): a .jsb runs on
  rem the JAWS version that built it and later ones, so shipping one risks a
  rem binary from the wrong version. The installer compiles each .jss with
  rem each installed JAWS version's own compiler.
  powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "Compress-Archive -Path (Get-ChildItem -LiteralPath 'scripts\jaws' -File | Where-Object { $_.Extension -ne '.jsb' } | ForEach-Object { $_.FullName }) -DestinationPath 'exec\%app%_JAWS.zip' -Force" >> "%log%" 2>&1
  echo Packed %app%_JAWS.zip>> "%log%"
)
if exist "addon\manifest.ini" (
  powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "Compress-Archive -Path 'addon\*' -DestinationPath '%app%.nvda-addon' -Force" >> "%log%" 2>&1
  echo Packed %app%.nvda-addon>> "%log%"
)
:readersDone

rem ---- the kit's scripts the app carries, refreshed on every build ----------
rem One source of truth for the shared install and release scripts:
rem installCommon (the logging half of every install script), installOllama,
rem installScreenReaderSupport, the tutorial tools, tidy, check, push,
rem release, unpushed and finish. installModels.cmd is the app's own, since it
rem names the app's models.
if not exist "scripts" mkdir "scripts"
for %%F in (buildTutorials.cmd buildTutorials.ps1 check.cmd check.py checkTutorial.cmd checkTutorial.py finish.cmd fixEncoding.cmd fixEncoding.py installCommon.cmd installOllama.cmd installScreenReaderSupport.cmd makeTutorials.cmd makeTutorials.py push.cmd release.cmd release.ps1 tidy.cmd tidy.py unpushed.cmd unpushed.py) do (
  if exist "%homerDev%\scripts\%%F" copy /y "%homerDev%\scripts\%%F" scripts\ >nul
)
rem Retired kit scripts an app may still carry from an earlier refresh: gone.
rem Since kit 1.42 the kit's scripts have plain names -- checkHomerApp is
rem check, gitPush is push, gitUnpushed is unpushed, homerFinish is finish,
rem homerInstall is installCommon, homerTidy is tidy, tagRelease is release --
rem and the old copies go too, so an old name typed from habit fails at once
rem instead of running a stale tool.
for %%F in (checkHomerApp.cmd checkHomerApp.py gitPush.cmd gitUnpushed.cmd gitUnpushed.py homerFinish.cmd homerInstall.cmd homerTidy.cmd homerTidy.py tagRelease.cmd tagRelease.ps1 cleanDir.cmd cleanDir.py gitRelease.cmd homerPolicy.py installTools.cmd sayTutorial.cmd sayTutorial.py tidyRepo.cmd tidyRepo.py) do (
  if exist "scripts\%%F" del /q "scripts\%%F" && echo Removed retired scripts\%%F>> "%log%"
)
rem FileDir's own retired scripts: the results box is the kit's now, and the
rem three grouped tool installers became one script per component.
for %%F in (summarizeSetup.cmd summarizeSetup.ps1 installMediaTools.cmd installImageTools.cmd installTranslateModel.cmd) do (
  if exist "scripts\%%F" del /q "scripts\%%F" && echo Removed retired scripts\%%F>> "%log%"
)

rem ---- carried over to the Homer layout (September 2026) -------------------
rem THE KIT'S CLASSES ARE COMPILED FROM C:\HomerDev\exec\CSharp, so a copy at the
rem top of the project is a stale one, waiting to be read or shipped by mistake.
for %%F in (Elevate.cs Inix.cs KeyMap.cs KeyName.cs Lbc.cs Log.cs Mdi.cs Ollama.cs Paths.cs Say.cs Util.cs Web.cs) do (
  if exist "%%F" if exist "!homerDev!\exec\CSharp\%%F" del /q "%%F" && echo Removed the old top-level %%F; the kit's is compiled instead>> "%log%"
)
rem The release script's old home was the top of the project; scripts\release
rem is its home now.
rem Hotkeys.inix lives in configs; a copy left at the top from the layout before
rem the kit still named the old Alt+Control timer keys, and the kit's check read it.
if exist "Hotkeys.inix" if exist "configs\Hotkeys.inix" del /q "Hotkeys.inix" && echo Removed the old top-level Hotkeys.inix; configs\Hotkeys.inix is the one the build reads>> "%log%"
for %%F in (tagRelease.cmd tagRelease.ps1 tagRelease_README.md) do if exist "%%F" if exist "scripts\release.ps1" del /q "%%F" && echo Removed the old top-level %%F>> "%log%"
rem GIT STILL SPELLS THREE NAMES THE OLD WAY -- BuildFileDir.cmd,
rem BuildFileDir.ps1 and FileDir_Setup.iss -- because Windows' git treats a
rem change of capitals alone as no change. The file on disk is left exactly as
rem it is (renaming this script while it runs would lose cmd's place in it);
rem only the name git records is corrected.
powershell -NoProfile -Command ^
  "$lTracked = @(git ls-files 2>$null);" ^
  "foreach ($sPair in @('BuildFileDir.cmd>buildFileDir.cmd', 'BuildFileDir.ps1>buildFileDir.ps1', 'FileDir_Setup.iss>FileDir_setup.iss')) {" ^
  "  $sOld, $sNew = $sPair.Split('>');" ^
  "  if (-not ($lTracked -ccontains $sOld)) { continue }" ^
  "  git rm --cached --quiet -- $sOld 2>&1 | Out-Null; git add -- $sNew 2>&1 | Out-Null;" ^
  "  'git now records ' + $sOld + ' as ' + $sNew + ', exit code ' + $LASTEXITCODE" ^
  "}" >> "%log%" 2>&1

rem ---- the project's own files in the Homer encoding ---------------------
rem UTF-8 with a byte order mark and CRLF, except .cmd and .bat without the
rem mark. Pandoc and other tools write bare newlines with no mark; this puts
rem every file RepoFiles.txt names right, so the release check finds nothing.
if exist "scripts\fixEncoding.cmd" (
  call "scripts\fixEncoding.cmd" >> "%log%" 2>&1
  echo Encoding: fixEncoding exit code !errorlevel!>> "%log%"
)

rem ---- spoken tutorials, when the app has any ---------------------------
rem Scripts in help\Tutorial_NN_*.inix become Tutorials.md, TutorialFeed.xml,
rem and one .mp3 per walk in help\tutorials with Tutorials.m3u beside them.
rem The three tools that make them -- buildTutorials.cmd, buildTutorials.ps1,
rem makeTutorials.py -- are the kit's, refreshed into scripts\ on every build:
rem one source of truth, and the app still carries what it needs. (Calling
rem the kit's own copy in place does not work: it takes the project to be the
rem folder it sits in, which is the kit.) Speaking happens only when a walk has
rem no audio yet; delete an .mp3 to have it spoken again. The voices -- Kokoro
rem through sherpa-onnx, Apache 2.0, or piper's kristin and john, public
rem domain, when Kokoro cannot be fetched -- are fetched once by the tool.
rem Skipped silently when the app has no tutorial scripts, which most do not.
if exist "help\Tutorial_*.inix" (
  set "tutorialsMissing="
  for %%F in (help\Tutorial_*.inix) do if not exist "help\tutorials\%%~nF.mp3" set "tutorialsMissing=1"
  if defined tutorialsMissing (
    echo Speaking the tutorials that have no audio yet, with the voices in C:\HomerDev\exec.
    rem The tool's own lines go to the screen: it names each tutorial as it starts
    rem and finishes, and keeps its own log in logs\. -build is an argument of
    rem its own, because a bare call hands the tool THIS script's arguments
    rem through %* (a cmd quirk), and "nobump" is not a script.
    call "scripts\buildTutorials.cmd" -build
    if errorlevel 1 echo WARN: not every tutorial could be spoken. The tutorials log in logs\ says why.
  )
)

rem ---- every file the installer names, where the installer looks --------
rem The layout put each shipped file in a folder of its own, and a file left
rem behind stopped the installer one file per build. placeSources.py reads the
rem installer script, finds each missing source anywhere in the project and
rem moves it into place, then names whatever is missing everywhere -- all at
rem once, before ISCC gets to say it one at a time.
if exist "scripts\placeSources.py" (
  python "scripts\placeSources.py" >> "%log%" 2>&1
  if errorlevel 1 (
    echo ERROR: the installer names files that are not in the project. See the sources log in logs\.
    echo ERROR: placeSources.py reported missing sources.>> "%log%"
    goto :failed
  )
)

rem ---- installer, if Inno Setup is present --------------------------
if not defined useInstaller goto :done
set "iscc="
if exist "!progFiles86!\Inno Setup 6\ISCC.exe" set "iscc=!progFiles86!\Inno Setup 6\ISCC.exe"
if not defined iscc if exist "!progFiles!\Inno Setup 6\ISCC.exe" set "iscc=!progFiles!\Inno Setup 6\ISCC.exe"
if not defined iscc (
  rem FETCH IT RATHER THAN ASK FOR IT, as with pandoc above. The installer is
  rem part of a release, so building it is part of the build.
  echo Installing Inno Setup, which builds %app%_setup.exe...
  echo Inno Setup not found; installing with winget>> "%log%"
  winget install --id JRSoftware.InnoSetup --silent --accept-source-agreements --accept-package-agreements >> "%log%" 2>&1
  if exist "!progFiles86!\Inno Setup 6\ISCC.exe" set "iscc=!progFiles86!\Inno Setup 6\ISCC.exe"
  if not defined iscc if exist "!progFiles!\Inno Setup 6\ISCC.exe" set "iscc=!progFiles!\Inno Setup 6\ISCC.exe"
)
if not defined iscc (
  echo Inno Setup could not be installed, so no installer was built.>> "%log%"
  echo ERROR: Inno Setup could not be installed, so %app%_setup.exe was not built.
  goto :failed
)
echo Inno Setup: !iscc!>> "%log%"
rem The kit folder goes to Inno as HomerDev, so the installer's #include of
rem HomerComponents.iss follows the kit wherever it is.
rem WHICH INSTALLER SCRIPT IS COMPILED (1 October 2026): its size, date and
rem fingerprint go to the log, so a log shows whether a delivered change is the
rem one on disk.
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$f = Get-Item -LiteralPath '%app%_setup.iss'; 'Installer script: ' + $f.FullName + ' bytes=' + $f.Length + ' written=' + $f.LastWriteTime.ToString('s') + ' sha256=' + (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash" >> "%log%" 2>&1
"!iscc!" /DHomerDev="!homerDev!" "%app%_setup.iss" >> "%log%" 2>&1
if errorlevel 1 (
  echo ERROR: the installer build failed. See %log%.
  echo ERROR: the installer build failed.>> "%log%"
  goto :failed
)
rem THE INSTALLER IS WRITTEN TO THE TOP OF THE PROJECT (OutputDir=.), where
rem scripts\release looks for it; LocalFiles.txt names it, so tidy leaves it
rem and git never takes it. A copy an older build left in exec goes.
if exist "exec\%app%_setup.exe" del /q "exec\%app%_setup.exe" && echo Removed the old exec\%app%_setup.exe>> "%log%"
if not exist "%app%_setup.exe" (
  echo ERROR: Inno Setup returned 0 but wrote no %app%_setup.exe.>> "%log%"
  echo ERROR: Inno Setup returned 0 but wrote no %app%_setup.exe.
  goto :failed
)
echo Built %app%_setup.exe version !ver!>> "%log%"
echo Built %app%_setup.exe version !ver!

:done
for /f "usebackq delims=" %%i in (`powershell -NoProfile -Command "Get-Date -Format 'yyyy-MM-ddTHH:mm:ss.fffzzz'"`) do set "sIso=%%i"
>> "%log%" echo %sIso% INFO  build end result=succeeded
echo(
echo To publish: scripts\push "What changed.", then scripts\release. It reads the
echo version from the version resource of %app%_setup.exe and tags v!ver!.
endlocal
exit /b 0

:failed
rem A FAILED BUILD TAKES NO NUMBER (HomerDev 1.43.29). version.txt is stepped
rem when a build begins; when it fails, the number goes back, so the next build
rem takes it again and the release never finds an installer one version behind
rem version.txt (HomerScribe, 28 September 2026: 1.0.260 stepped, the kit not
rem found, the release refused).
if defined verOld if not "!ver!"=="!verOld!" (
  > version.txt echo !verOld!
  >> "%log%" echo Version: restored to !verOld!; a failed build takes no number
)
for /f "usebackq delims=" %%i in (`powershell -NoProfile -Command "Get-Date -Format 'yyyy-MM-ddTHH:mm:ss.fffzzz'"`) do set "sIso=%%i"
>> "%log%" echo %sIso% ERROR build end result=failed
endlocal
exit /b 1

:getLib
rem -------------------------------------------------------------------
rem One library into exec: left alone when it is already there, fetched
rem from NuGet when it is not.
rem   call :getLib <package id> <assembly file name>
rem -------------------------------------------------------------------
if exist "exec\%~2" goto :eof
call :getNuGet %~1 %~2
if exist "%~2" move /y "%~2" "exec\" >nul 2>&1
if not exist "exec\%~2" echo WARN: %~2 is not in exec and could not be fetched from %~1.>> "%log%"
goto :eof

:getNuGet
rem -------------------------------------------------------------------
rem Fetch one assembly out of one NuGet package into this folder.
rem   call :getNuGet <package id> <assembly file name>
rem
rem Straight from nuget.org over https, unzipped in the temp folder, with the
rem newest .NET Framework build preferred and any build taken when there is no
rem net4 one. Nothing is installed on the machine and nothing is left behind.
rem -------------------------------------------------------------------
if exist "%~2" goto :eof
echo Fetching %~1 from NuGet.
echo Fetching %~1 from NuGet.>> "%log%"
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ErrorActionPreference='Stop';" ^
  "[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12;" ^
  "$sTemp=Join-Path $env:TEMP ('nuget_'+[guid]::NewGuid().ToString('N'));" ^
  "New-Item -ItemType Directory -Path $sTemp -Force | Out-Null;" ^
  "$sPkg=Join-Path $sTemp 'package.nupkg';" ^
  "Invoke-WebRequest -Uri ('https://www.nuget.org/api/v2/package/%~1') -OutFile $sPkg -UseBasicParsing;" ^
  "Expand-Archive -LiteralPath $sPkg -DestinationPath $sTemp -Force;" ^
  "$oDll = Get-ChildItem -Path (Join-Path $sTemp 'lib') -Recurse -Filter '%~2' |" ^
  "  Where-Object { $_.FullName -match 'net4' } | Sort-Object FullName -Descending | Select-Object -First 1;" ^
  "if (-not $oDll) { $oDll = Get-ChildItem -Path (Join-Path $sTemp 'lib') -Recurse -Filter '%~2' |" ^
  "  Sort-Object FullName -Descending | Select-Object -First 1 }" ^
  "if (-not $oDll) { throw 'No %~2 in the %~1 package.' }" ^
  "Copy-Item -LiteralPath $oDll.FullName -Destination (Join-Path '%CD%' '%~2') -Force;" ^
  "Remove-Item -LiteralPath $sTemp -Recurse -Force -ErrorAction SilentlyContinue;" ^
  "Write-Output ('%~2 taken from ' + $oDll.FullName)" >> "%log%" 2>&1
if not exist "%~2" (
  echo ERROR: %~2 could not be fetched from the %~1 package. See %log%.
  echo ERROR: %~2 could not be fetched.>> "%log%"
)
goto :eof

:takeNextVersion
rem -------------------------------------------------------------------
rem Take the next UNUSED version. The last dotted part of !ver! is
rem incremented, and any number that already carries a release tag on
rem the origin remote is stepped over, so a version.txt that has fallen
rem behind the repository cannot mint a number that is already spent.
rem
rem One "git ls-remote" is the only network call the build makes. If it
rem fails, the plain increment is used and tagRelease remains the check
rem it has always been, so a machine with no network still builds.
rem
rem These are subroutines rather than parenthesised blocks, so each line
rem is parsed on its own.
rem -------------------------------------------------------------------
set "verOld=!ver!"
set "sTagFile=%TEMP%\%app%_tags.txt"
del "!sTagFile!" >nul 2>&1
git ls-remote --tags origin "v*" > "!sTagFile!" 2>> "%log%"
if errorlevel 1 echo WARN: the released tags could not be read, so the next number is taken blindly.>> "%log%"
if errorlevel 1 del "!sTagFile!" >nul 2>&1
rem THE TAG LIST WITH WINDOWS LINE ENDS (1 October 2026). git writes it with
rem LF alone, and findstr /e matches only before a CR LF, so no tag ever
rem matched and no spent number was stepped over: EdSharp's old releases
rem v5.0.32 to v5.0.36 were each chosen again, and each refused as already
rem released. find /v "" rewrites every line with CR LF.
if exist "!sTagFile!" type "!sTagFile!" | find /v "" > "!sTagFile!.crlf"
if exist "!sTagFile!.crlf" move /y "!sTagFile!.crlf" "!sTagFile!" >nul
if exist "!sTagFile!" for /f %%n in ('find /c "refs/tags/" ^< "!sTagFile!"') do echo Released tags on origin: %%n>> "%log%"

:nextCandidate
call :incrementVersion
if not defined new goto :eof
if not exist "!sTagFile!" goto :haveNextVersion
findstr /e /c:"refs/tags/v!ver!" "!sTagFile!" >nul 2>&1
if errorlevel 1 goto :haveNextVersion
echo Version v!ver! is already released; stepping over it.
echo Version v!ver! is already released; stepping over it.>> "%log%"
goto :nextCandidate

:haveNextVersion
del "!sTagFile!" >nul 2>&1
> version.txt echo !ver!
echo Version: !verOld! -^> !ver!
echo Version: !verOld! -^> !ver!>> "%log%"
goto :eof

:incrementVersion
rem -------------------------------------------------------------------
rem Increment the last dotted part of !ver!. Nothing is written here, so
rem the caller may call this repeatedly while stepping over numbers that
rem are already spent.
rem -------------------------------------------------------------------
set "p1=" & set "p2=" & set "p3=" & set "p4="
set "new="
for /f "tokens=1-4 delims=." %%a in ("!ver!") do (
  set "p1=%%a" & set "p2=%%b" & set "p3=%%c" & set "p4=%%d"
)
if defined p4 (
  set /a p4=p4+1
  set "new=!p1!.!p2!.!p3!.!p4!"
) else if defined p3 (
  set /a p3=p3+1
  set "new=!p1!.!p2!.!p3!"
) else if defined p2 (
  set "new=!p1!.!p2!.1"
) else (
  set "new=!p1!.0.1"
)
if not defined new (
  echo ERROR: could not work out the next version from "!ver!".
  echo ERROR: could not work out the next version from "!ver!".>> "%log%"
  goto :eof
)
set "ver=!new!"
goto :eof
