@echo off
rem retireRepair.cmd -- remove the repair script of 6 October 2026, whose text
rem the release check reads as two lines on the Roaming tree, now that the
rem repair is done; then remove this script itself. Logged to
rem logs\FileDir-retire-<stamp>.log.
setlocal enabledelayedexpansion
set "sHere=%~dp0"
pushd "%sHere%.."
if not exist logs mkdir logs
for /f %%I in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "stamp=%%I"
set "log=logs\FileDir-retire-!stamp!.log"
echo retireRepair start %date% %time% > "!log!"
echo Script: %~f0 >> "!log!"
echo Folder: %cd% >> "!log!"
if exist scripts\repairFileDir.cmd (
  del /q scripts\repairFileDir.cmd
  echo Removed scripts\repairFileDir.cmd, its work done >> "!log!"
) else (
  echo scripts\repairFileDir.cmd was already gone >> "!log!"
)
echo Done. The log is !log!
echo retireRepair end %date% %time% >> "!log!"
popd
endlocal
(goto) 2>nul & del "%~f0"
