@echo off
rem ===================================================================
rem buildUrlFido.cmd  --  Build urlFido.exe from urlFido.cs
rem
rem urlFido uses modern C# language features (string interpolation is
rem avoided, but object/collection initializers, lambdas, and the
rem async websocket APIs are used), so it requires the ROSLYN compiler
rem that ships with Visual Studio 2017 or later -- NOT the pre-Roslyn
rem csc.exe under %WINDIR%\Microsoft.NET\Framework64, which only
rem supports C# 5. This script locates the highest-level Roslyn csc.exe
rem available and uses it. (The search prefers newer Visual Studio
rem versions first: 2022, then 2019, then 2017.)
rem
rem Target framework: .NET Framework 4.8 (ships with Windows 10 1903+
rem and Windows 11; no runtime install needed by end users).
rem
rem Requires ONE of:
rem   - Visual Studio 2022/2019/2017 (any edition, incl. free
rem     Community), OR
rem   - Visual Studio Build Tools 2022/2019 (free, smaller; select the
rem     ".NET desktop build tools" workload during install).
rem   Download: https://visualstudio.microsoft.com/downloads/
rem
rem References are all .NET Framework 4.8 assemblies:
rem   System, System.Core, System.Drawing, System.Windows.Forms,
rem   System.Web.Extensions (JavaScriptSerializer),
rem   System.Net.Http, and System.Web (transitive). No NuGet packages,
rem   no bundled Chromium, no Playwright driver -- urlFido.exe is a
rem   true single-file executable.
rem
rem Output (current directory):
rem   urlFido.exe  -- the built executable, with embedded icon.
rem
rem To produce the installer (urlFido_setup.exe): open
rem urlFido_setup.iss in Inno Setup and click Compile.
rem ===================================================================

setlocal enableextensions
cd /d "%~dp0"

rem ---- Locate the highest-level Roslyn csc.exe -----------------------
set "sCsc="

rem Visual Studio 2022 editions (Community, Professional, Enterprise)
for %%E in (Community Professional Enterprise) do (
    if exist "C:\Program Files\Microsoft Visual Studio\2022\%%E\MSBuild\Current\Bin\Roslyn\csc.exe" (
        set "sCsc=C:\Program Files\Microsoft Visual Studio\2022\%%E\MSBuild\Current\Bin\Roslyn\csc.exe"
        goto :cscFound
    )
    if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\%%E\MSBuild\Current\Bin\Roslyn\csc.exe" (
        set "sCsc=C:\Program Files (x86)\Microsoft Visual Studio\2022\%%E\MSBuild\Current\Bin\Roslyn\csc.exe"
        goto :cscFound
    )
)

rem Visual Studio Build Tools 2022
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe" (
    set "sCsc=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
    goto :cscFound
)
if exist "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe" (
    set "sCsc=C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
    goto :cscFound
)

rem Visual Studio 2019 editions and Build Tools
for %%E in (Community Professional Enterprise BuildTools) do (
    if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\%%E\MSBuild\Current\Bin\Roslyn\csc.exe" (
        set "sCsc=C:\Program Files (x86)\Microsoft Visual Studio\2019\%%E\MSBuild\Current\Bin\Roslyn\csc.exe"
        goto :cscFound
    )
)

rem Visual Studio 2017 editions and Build Tools
for %%E in (Community Professional Enterprise BuildTools) do (
    if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\%%E\MSBuild\15.0\Bin\Roslyn\csc.exe" (
        set "sCsc=C:\Program Files (x86)\Microsoft Visual Studio\2017\%%E\MSBuild\15.0\Bin\Roslyn\csc.exe"
        goto :cscFound
    )
)

rem Roslyn standalone in PATH (reject the pre-Roslyn Framework csc,
rem whose path contains Microsoft.NET\Framework).
where csc.exe >nul 2>&1
if not errorlevel 1 (
    for /f "delims=" %%P in ('where csc.exe') do (
        echo %%P | findstr /i "Microsoft.NET\\Framework" >nul
        if errorlevel 1 (
            set "sCsc=%%P"
            goto :cscFound
        )
    )
)

echo [ERROR] Could not find a Roslyn C# compiler.
echo.
echo         urlFido requires the Roslyn compiler that ships with
echo         Visual Studio 2017 or later. Install either:
echo           - Visual Studio 2022 Community (free), OR
echo           - Visual Studio Build Tools 2022 (smaller, free;
echo             select ".NET desktop build tools").
echo         https://visualstudio.microsoft.com/downloads/
echo.
echo         NOTE: The csc.exe under
echo         %%WINDIR%%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
echo         is the older pre-Roslyn compiler and will NOT work.
exit /b 2

:cscFound
echo [INFO] Using compiler: %sCsc%

rem ---- Icon check ---------------------------------------------------
if not exist "urlFido.ico" (
    echo [ERROR] urlFido.ico not found in %CD%.
    echo         The icon file is required to embed into the exe.
    exit /b 2
)

rem ---- Locate the UI Automation assemblies ---------------------------
rem Say.cs raises Narrator notification events, which needs:
rem   UIAutomationProvider.dll -- IRawElementProviderSimple, ProviderOptions
rem   UIAutomationTypes.dll    -- AutomationNotificationKind / ...Processing
rem
rem These are WPF-side assemblies. Unlike System.dll and friends, they
rem are NOT on the Roslyn compiler's default reference path, so passing
rem the bare file name fails with CS0006. They must be referenced by
rem full path. Preferred source is the .NET Framework reference
rem assemblies folder; the runtime WPF folder is the fallback.

rem PARSE-TIME PITFALL: the variable NAME ProgramFiles(x86) contains
rem parentheses, and cmd.exe scans a parenthesized block for its closing
rem paren BEFORE expanding variables. Referencing %ProgramFiles(x86)%
rem directly inside the for/if blocks below therefore terminates the
rem block early. Copy both roots into paren-free names out here first,
rem then use only those inside the blocks.
set "sProgFiles86=%ProgramFiles(x86)%"
set "sProgFiles=%ProgramFiles%"
set "sUiaRefBase=Reference Assemblies\Microsoft\Framework\.NETFramework"

set "sUiaDir="

for %%V in (v4.8 v4.7.2 v4.7.1 v4.7 v4.6.2 v4.6.1 v4.6 v4.5.2) do (
    if not defined sUiaDir (
        if exist "%sProgFiles86%\%sUiaRefBase%\%%V\UIAutomationProvider.dll" (
            set "sUiaDir=%sProgFiles86%\%sUiaRefBase%\%%V"
        )
    )
)

for %%V in (v4.8 v4.7.2 v4.7.1 v4.7 v4.6.2 v4.6.1 v4.6 v4.5.2) do (
    if not defined sUiaDir (
        if exist "%sProgFiles%\%sUiaRefBase%\%%V\UIAutomationProvider.dll" (
            set "sUiaDir=%sProgFiles%\%sUiaRefBase%\%%V"
        )
    )
)

rem Fallback: the runtime WPF folder that ships with the .NET Framework.
if not defined sUiaDir (
    if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\WPF\UIAutomationProvider.dll" (
        set "sUiaDir=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\WPF"
    )
)
if not defined sUiaDir (
    if exist "%WINDIR%\Microsoft.NET\Framework\v4.0.30319\WPF\UIAutomationProvider.dll" (
        set "sUiaDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\WPF"
    )
)

if not defined sUiaDir (
    echo [ERROR] Could not find UIAutomationProvider.dll / UIAutomationTypes.dll.
    echo(
    echo         These ship with the .NET Framework targeting pack. Install
    echo         the ".NET Framework 4.8 targeting pack" via the Visual
    echo         Studio Installer ^(Individual components^), or install the
    echo         .NET Framework 4.8 Developer Pack:
    echo         https://dotnet.microsoft.com/download/dotnet-framework/net48
    exit /b 2
)

echo [INFO] UI Automation assemblies: %sUiaDir%

rem ---- Compile ------------------------------------------------------
rem urlFido.exe is built from SIX sources compiled together into one
rem assembly, so the result is still a single self-contained .exe:
rem
rem   urlFido.cs  -- the program itself
rem   Lbc.cs      -- Homer Layout-By-Code dialog classes (LbcTextBox,
rem                  LbcForm, LbcDialog, HelpDialog)
rem   Say.cs      -- Homer screen-reader announcement layer
rem   Inix.cs     -- Homer .ini/.inix order-preserving codec
rem   Util.cs     -- Homer general utilities
rem   Web.cs      -- Homer web-download helpers
rem
rem Extra references beyond the base set are required by the Homer
rem modules: Microsoft.VisualBasic (Util), System.Data, and the two
rem UI Automation assemblies used by Say.cs to raise Narrator
rem notification events.
rem
rem NVDA NOTE: Say.cs P/Invokes nvdaControllerClient.dll for NVDA
rem speech. That DLL is an OPTIONAL runtime companion -- when it is
rem absent the DllNotFoundException is caught and speech falls back to
rem JAWS, Narrator, or SAPI, so urlFido.exe still runs standalone. To
rem enable NVDA support, place nvdaControllerClient.dll next to
rem urlFido.exe (the installer ships it when it is present here).
rem
rem /platform:x64 -- 64-bit, matching the project convention.

rem ---- Embed the NVDA controller client, when present ---------------
rem nvdaControllerClient.dll is a NATIVE DLL, so it cannot be loaded from
rem memory; urlFido embeds it as a managed resource and extracts it to
rem %LOCALAPPDATA%\urlFido on first use, but ONLY when NVDA is actually
rem running. Embedding keeps the distributable a single .exe.
rem
rem BITNESS: this is an x64 build, so the 64-bit build of the DLL is
rem required. A 32-bit DLL embeds without complaint but fails at
rem LoadLibrary, silently dropping urlFido back to JAWS/Narrator/SAPI.
rem
rem When the DLL is absent the switch is empty and the build still
rem succeeds -- urlFido simply has no NVDA path.

set "sNvdaRes="
if exist "nvdaControllerClient.dll" set "sNvdaRes=/resource:nvdaControllerClient.dll,nvdaControllerClient.dll"

set "sLog=%CD%\buildUrlFido.log"
echo [INFO] Build log: %sLog%
echo urlFido build started %DATE% %TIME%> "%sLog%"
echo Compiler: %sCsc%>> "%sLog%"
echo UI Automation: %sUiaDir%>> "%sLog%"
if defined sNvdaRes (echo NVDA client: embedded as a resource>> "%sLog%") else (echo NVDA client: not embedded ^(DLL not found^)>> "%sLog%")
echo(>> "%sLog%"

"%sCsc%" /nologo /target:exe /platform:x64 /optimize+ ^
    /reference:System.dll ^
    /reference:System.Core.dll ^
    /reference:System.Data.dll ^
    /reference:System.Drawing.dll ^
    /reference:System.Windows.Forms.dll ^
    /reference:System.Web.dll ^
    /reference:System.Web.Extensions.dll ^
    /reference:System.Net.Http.dll ^
    /reference:Microsoft.VisualBasic.dll ^
    /reference:"%sUiaDir%\UIAutomationProvider.dll" ^
    /reference:"%sUiaDir%\UIAutomationTypes.dll" ^
    %sNvdaRes% ^
    /win32icon:urlFido.ico ^
    /out:urlFido.exe ^
    urlFido.cs Lbc.cs Say.cs Inix.cs Util.cs Web.cs >> "%sLog%" 2>&1

set iBuildResult=%ERRORLEVEL%

rem Echo the compiler output to the console as well as the log, so a
rem failed build is visible without opening the file.
type "%sLog%"

if not "%iBuildResult%"=="0" (
    echo(
    echo [ERROR] Build failed. Details above and in %sLog%.
    exit /b 1
)

echo Build succeeded %DATE% %TIME%>> "%sLog%"
echo(
echo [INFO] Build complete. urlFido.exe is in %CD% ^(icon embedded^).
echo [INFO] Compiler output was logged to %sLog%.

if defined sNvdaRes (
    echo [INFO] nvdaControllerClient.dll was embedded into urlFido.exe.
    echo        urlFido.exe remains a single self-contained file.
) else (
    echo(
    echo [NOTE] nvdaControllerClient.dll is not present in %CD%, so it was
    echo        NOT embedded. urlFido still runs and speaks through JAWS,
    echo        Narrator, or SAPI. For NVDA speech, put the 64-bit DLL here
    echo        and rebuild.
)

echo(
echo To produce the installer (urlFido_setup.exe):
echo   Open urlFido_setup.iss in Inno Setup and click Compile.
echo   Inno Setup writes urlFido_setup.exe to %CD%.
endlocal
