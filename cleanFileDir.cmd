@echo off
rem cleanFileDir.cmd -- move everything the project does not claim into notes\.
rem
rem   cleanFileDir            do it
rem   cleanFileDir --survey   list what would move, and change nothing
rem
rem THE LOG ALWAYS EXISTS, for the same reason it does in BuildFileDir.cmd: a
rem script that will not parse never reaches its own logging. This wrapper opens
rem cleanFileDir.log first and appends everything Python printed at the end, so a
rem syntax error lands in the log rather than only on a console that scrolls.
setlocal
pushd "%~dp0"

set "log=%~dp0cleanFileDir.log"
set "capture=%TEMP%\cleanFileDir_console_%RANDOM%.txt"

> "%log%" echo cleanFileDir log
>> "%log%" echo [wrapper] Started %DATE% %TIME%
>> "%log%" echo [wrapper] Command line: %0 %*
for %%f in ("%~dp0cleanFileDir.py") do >> "%log%" echo [wrapper] Script: %%~ff, %%~zf bytes, written %%~tf
>> "%log%" echo.

python "%~dp0cleanFileDir.py" %* > "%capture%" 2>&1
set "rc=%ERRORLEVEL%"
if exist "%capture%" type "%capture%"

>> "%log%" echo.
>> "%log%" echo [wrapper] ---- Console output from Python ----
if exist "%capture%" type "%capture%" >> "%log%"
>> "%log%" echo [wrapper] Python exit code: %rc%
>> "%log%" echo [wrapper] Finished %DATE% %TIME%
if exist "%capture%" del /f /q "%capture%" >nul 2>&1

popd
endlocal & exit /b %rc%
