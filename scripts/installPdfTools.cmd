@echo off
rem installPDF tools.cmd -- install or update PDF tools, which FileDir uses to read PDF files as text.
rem
rem Run from the installer's final page, or on its own at any time. The
rem common half -- the log under %LOCALAPPDATA%\FileDir\logs, the quiet
rem flag when the installer is driving -- is the kit's installCommon.cmd, so a
rem fix there reaches every install script in every Homer app.
rem
rem MACHINE WIDE, to its own default folder, never under FileDir's tree: an
rem upgrade of FileDir replaces that tree and would take the tool with it.
setlocal enabledelayedexpansion
set "sScript=%~n0"
if not exist "%~dp0installCommon.cmd" (
  echo(
  echo installCommon.cmd is missing from %~dp0
  echo That file is part of this program. Reinstall, or copy it from the
  echo program's zip into this folder, and run this again.
  echo(
  if not defined noPause pause
  exit /b 1
)
call "%~dp0installCommon.cmd" setup "%~f0" %*

set "bReinstall="
for %%A in (%*) do if /i "%%A"=="reinstall" set "bReinstall=1"

where python >nul 2>&1
if not errorlevel 1 if not defined bReinstall goto :update
echo Python is installed. Making sure the PDF reader is current.
python -m pip install --upgrade pymupdf4llm >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0installCommon.cmd" log "pip install pymupdf4llm exit code %iCode%"
if "%iCode%"=="0" echo The PDF reader is current.
if not "%iCode%"=="0" echo The PDF reader could not be updated; the log says why.
goto :done

:updateWinget

echo Installing PDF tools. Nothing is asked of you while it runs.
winget install --id Python.Python.3.13 --exact --silent --accept-source-agreements --accept-package-agreements --disable-interactivity >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0installCommon.cmd" log "winget install Python.Python.3.13 exit code %iCode%"
if not "%iCode%"=="0" goto :failed
where python >nul 2>&1 || (echo Python is installed but not on this window's path yet; open a new window and run this again.& goto :done)
python -m pip install --upgrade pymupdf4llm >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0installCommon.cmd" log "pip install pymupdf4llm exit code %iCode%"
if not "%iCode%"=="0" goto :failed
echo The PDF tools are installed.
goto :done

:update
echo Python is installed. Making sure the PDF reader is current.
python -m pip install --upgrade pymupdf4llm >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0installCommon.cmd" log "pip install pymupdf4llm exit code %iCode%"
if "%iCode%"=="0" echo The PDF reader is current.
if not "%iCode%"=="0" echo The PDF reader could not be updated; the log says why.
goto :done

:updateWinget
echo Updating PDF tools to the newest version. Nothing is asked of you while it runs.
winget upgrade --id Python.Python.3.13 --exact --silent --accept-source-agreements --accept-package-agreements --disable-interactivity >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0installCommon.cmd" log "winget upgrade Python.Python.3.13 exit code %iCode%"
if "%iCode%"=="0" echo PDF tools was updated.
if not "%iCode%"=="0" echo PDF tools was NOT updated; the log has winget's answer.
goto :done

:failed
echo(
echo PDF tools could not be installed automatically. The log says why:
echo   %log%
echo(
if not defined noPause pause
endlocal
exit /b 1

:done
echo(
if not defined noPause pause
endlocal
exit /b 0
