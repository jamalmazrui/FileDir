@echo off
rem ====================================================================
rem BuildFileDir.cmd - 64-bit build for FileDir.exe (5.0 beta).
rem
rem Compiles FileDir.cs to FileDir.exe with csc.exe, /platform:anycpu so
rem it runs as a 64-bit process on 64-bit Windows. No MSBuild/NuGet.
rem
rem Homer helpers folded in as source so far: Web.cs (Homer.Web), Say.cs (Homer.Say), and
rem Inix.cs (Homer.InixCodec, the FileDir.inix layer). Say.cs needs the UIA notification assemblies
rem (UIAutomationProvider/UIAutomationTypes) and System.Web; the UIA
rem DLLs are located via the .NET 4.8 reference-assemblies dir with a
rem GAC fallback, mirroring the EdSharp build. The existing reference
rem DLLs (lbc/LbcVB/LbcJS, Tektosyne, SharpZipLib, FileAssociation)
rem remain until their subsystems are ported. Remaining Homer files are
rem adopted in later passes.
rem ====================================================================
setlocal enableextensions enabledelayedexpansion
pushd "%~dp0"

set "log=BuildFileDir.log"
echo FileDir build log > "!log!"
echo Started %DATE% %TIME% >> "!log!"

if not exist "FileDir.cs" echo ERROR: FileDir.cs not found.& popd & exit /b 1
if not exist "Web.cs" echo ERROR: Web.cs ^(Homer^) not found.& popd & exit /b 1
if not exist "Say.cs" echo ERROR: Say.cs ^(Homer^) not found.& popd & exit /b 1
if not exist "Inix.cs" echo ERROR: Inix.cs ^(Homer^) not found.& popd & exit /b 1
if not exist "Util.cs" echo ERROR: Util.cs ^(Homer^) not found.& popd & exit /b 1
if not exist "FileDir.js" echo ERROR: FileDir.js not found.& popd & exit /b 1

rem ---- required reference DLLs (must sit beside FileDir.cs) ----
for %%d in (FileAssociation.dll Tektosyne.dll ICSharpCode.SharpZipLib.dll) do (
  if not exist "%%d" echo ERROR: reference assembly %%d not found.& popd & exit /b 1
)

rem ---- locate csc.exe: prefer Roslyn (latest C#), fall back to Framework64 ----
set "csc="
for %%p in (
  "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
  "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
  "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
  "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
  "C:\Program Files (x86)\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe"
  "C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe"
  "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
  "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
  "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
) do (if not defined csc if exist %%p set "csc=%%~p")
if not defined csc echo ERROR: No csc.exe found. Install VS Build Tools or repair .NET Framework.& popd & exit /b 1
echo C# compiler: !csc! >> "!log!"

rem ---- locate jsc.exe (Framework only; JScript .NET for the evaluator) ----
set "jsc="
if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\jsc.exe" set "jsc=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\jsc.exe"
if not defined jsc if exist "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\jsc.exe" set "jsc=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\jsc.exe"
if not defined jsc echo ERROR: No jsc.exe found ^(repair .NET Framework^).& popd & exit /b 1
echo JScript compiler: !jsc! >> "!log!"

rem ---- locate UIA notification assemblies for Homer.Say ----
rem Prefer the .NET 4.8 reference assemblies; fall back to the GAC, which
rem is present on any machine with the .NET 4.x runtime installed.
set "refDir=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
set "uiaProv="
set "uiaTypes="
if exist "!refDir!\UIAutomationProvider.dll" set "uiaProv=!refDir!\UIAutomationProvider.dll"
if exist "!refDir!\UIAutomationTypes.dll"    set "uiaTypes=!refDir!\UIAutomationTypes.dll"
if not defined uiaProv  if exist "%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\UIAutomationProvider\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationProvider.dll" set "uiaProv=%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\UIAutomationProvider\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationProvider.dll"
if not defined uiaTypes if exist "%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll" set "uiaTypes=%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll"
if not defined uiaProv  echo ERROR: UIAutomationProvider.dll not found; install the .NET 4.8 Developer Pack.& popd & exit /b 1
if not defined uiaTypes echo ERROR: UIAutomationTypes.dll not found; install the .NET 4.8 Developer Pack.& popd & exit /b 1
echo UIAutomationProvider: !uiaProv! >> "!log!"
echo UIAutomationTypes: !uiaTypes! >> "!log!"

rem ---- optional manifest and icon (used only if present) ----
set "manifest="
if exist "FileDir.manifest" set "manifest=/win32manifest:FileDir.manifest"
if defined manifest (echo Manifest: FileDir.manifest >> "!log!") else (echo Manifest: none ^(optional^) >> "!log!")
set "icon="
if exist "FileDir.ico" set "icon=/win32icon:FileDir.ico"
if defined icon (echo Icon: FileDir.ico >> "!log!") else (echo Icon: none ^(optional^) >> "!log!")

rem ---- compile FileDir.cs -> FileDir.exe (64-bit on 64-bit Windows) ----
rem ---- compile FileDir.js -^> FileDirScript.dll (JScript .NET host) ----
echo Compiling FileDir.js -^> FileDirScript.dll ...
if exist FileDirScript.dll del /f /q FileDirScript.dll
"!jsc!" /nologo /target:library /out:FileDirScript.dll FileDir.js >> "!log!" 2>&1
if errorlevel 1 goto failed

echo Compiling FileDir.cs -^> FileDir.exe ...
if exist FileDir.exe del /f /q FileDir.exe
"!csc!" /nologo /target:winexe /platform:anycpu /optimize+ /nowarn:0162 %manifest% %icon% /reference:FileAssociation.dll /reference:Microsoft.CSharp.dll /reference:Tektosyne.dll /reference:ICSharpCode.SharpZipLib.dll /reference:Microsoft.VisualBasic.dll /reference:System.Security.dll /reference:System.Web.dll /reference:"!uiaProv!" /reference:"!uiaTypes!" /out:FileDir.exe FileDir.cs Web.cs Say.cs Inix.cs Util.cs Dialogs.cs >> "!log!" 2>&1
if errorlevel 1 goto failed

rem ---- optional: build accessible HTML docs from Markdown via 2htm ----
if exist "2htm.exe" (
  if exist "FileDir.md" "2htm.exe" -f "FileDir.md" >> "!log!" 2>&1
  if exist "History.md" "2htm.exe" -f "History.md" >> "!log!" 2>&1
  echo Docs: generated FileDir.htm / History.htm via 2htm. >> "!log!"
) else (
  echo Docs: 2htm.exe not present; HTML docs not regenerated. >> "!log!"
)

echo.
echo Build complete:
echo   FileDir.exe  -- the application (64-bit on 64-bit Windows)
echo. >> "!log!"
echo BUILD COMPLETE: FileDir.exe built successfully. >> "!log!"
echo Finished %DATE% %TIME% >> "!log!"
popd & endlocal & exit /b 0

:failed
echo. >> "!log!"
echo BUILD FAILED - compile errors are listed above in this log. >> "!log!"
echo.
echo BUILD FAILED. Errors from %log%:
type "!log!" | findstr /C:": error" /C:"error CS"
popd & endlocal & exit /b 1
