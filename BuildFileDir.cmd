@echo off
rem BuildFileDir.cmd -- build FileDir.exe and FileDir_setup.exe.
rem
rem   BuildFileDir             full build, taking the next version number
rem   BuildFileDir nobump      recompile without taking a new number
rem   BuildFileDir noinstall   build the program but not the installer
rem   BuildFileDir audit       run the checks only, and compile nothing
rem
rem THE LOG ALWAYS EXISTS. This wrapper opens BuildFileDir.log and writes the
rem first lines BEFORE PowerShell is started, then captures everything PowerShell
rem prints and appends it at the end. That matters for one failure in
rem particular: a PowerShell script that will not PARSE never runs a line of
rem itself, including the line that opens its own log. Three builds have failed
rem that way, each leaving no log at all and a wall of parser errors on a console
rem that scrolls. Now the parser errors land in the log with everything else.
rem
rem So: BuildFileDir.log is the file to send, whatever went wrong.
setlocal
pushd "%~dp0"

set "log=%~dp0BuildFileDir.log"
set "capture=%TEMP%\BuildFileDir_console_%RANDOM%.txt"

rem A fresh log, opened here so it exists no matter what happens next.
> "%log%" echo FileDir build log
>> "%log%" echo [wrapper] Started %DATE% %TIME%
>> "%log%" echo [wrapper] Wrapper: %~f0
>> "%log%" echo [wrapper] Command line: %0 %*
>> "%log%" echo [wrapper] Computer: %COMPUTERNAME%, user: %USERNAME%
>> "%log%" echo [wrapper] Working directory: %CD%
for %%f in ("%~dp0BuildFileDir.ps1") do >> "%log%" echo [wrapper] Script: %%~ff, %%~zf bytes, written %%~tf
>> "%log%" echo.

if not exist "%~dp0BuildFileDir.ps1" (
  >> "%log%" echo [wrapper] ERROR: BuildFileDir.ps1 is not here. Nothing can be built.
  echo ERROR: BuildFileDir.ps1 is not here. See "%log%".
  popd & endlocal & exit /b 1
)

rem PowerShell writes its own detail into the log as it goes. Its CONSOLE output
rem is captured separately and appended below, because that is the only place a
rem parse error appears.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0BuildFileDir.ps1" %* > "%capture%" 2>&1
set "rc=%ERRORLEVEL%"

rem Show it, so the person running the build sees what happened.
if exist "%capture%" type "%capture%"

>> "%log%" echo.
>> "%log%" echo [wrapper] ---- Console output from PowerShell ----
if exist "%capture%" type "%capture%" >> "%log%"
>> "%log%" echo [wrapper] PowerShell exit code: %rc%
>> "%log%" echo [wrapper] Finished %DATE% %TIME%
if exist "%capture%" del /f /q "%capture%" >nul 2>&1

if not "%rc%"=="0" (
  echo.
  echo BUILD FAILED. Send "%log%" -- it holds the whole story, including any
  echo parser errors, which are the one thing the script cannot log itself.
)
popd
endlocal & exit /b %rc%
