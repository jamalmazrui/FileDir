@echo off
REM postPage.cmd (generic)
REM Thin wrapper that invokes postPage.ps1 with execution-policy bypass.
REM Place this and postPage.ps1 in any project folder that contains a
REM Pandoc-Markdown source file, then run "postPage" from that folder.
REM
REM The PowerShell script discovers the first *.md file (alphabetically,
REM excluding README.md) and treats the parent folder name as the repo
REM name. Full log is written to postPage.log next to this script.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0postPage.ps1"
set iExitCode=%ERRORLEVEL%

echo.
echo postPage finished with exit code %iExitCode%.
echo Read the full log at %~dp0postPage.log
exit /b %iExitCode%
