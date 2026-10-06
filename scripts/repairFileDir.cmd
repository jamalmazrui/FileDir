@echo off
setlocal enabledelayedexpansion
rem repairFileDir.cmd -- put FileDir's build script back and move two lines
rem off the Roaming tree, after the restore of 6 October 2026 went one step
rem too far.
rem
rem WHAT HAPPENED. restoreFileDir.cmd was meant to delete the stray build
rem script a zip had added, but the kit's renameBuild had already rewritten
rem that name inside the script to build.cmd, so the script deleted your
rem build.cmd instead; the release's tidy step then renamed the stray script
rem into its place. And the release check found two lines in Dialogs.cs and
rem FileDir.cs that keep files under the Roaming tree, which git checkout had
rem put back from an older commit.
rem
rem WHAT THIS DOES, each step logged to logs\FileDir-repair-<stamp>.log:
rem   1. git checkout -- build.cmd      your committed build script returns
rem   2. two exact lines -- FileDir's data folder in FileDir.cs and its Quick
rem      folder in Dialogs.cs -- move from ApplicationData to
rem      LocalApplicationData, as the Homer layout and the release check
rem      require; the JAWS settings line, rightly on Roaming, is not touched
rem   3. says what it did
set "sHere=%~dp0"
pushd "%sHere%.."
if not exist logs mkdir logs
for /f %%I in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "stamp=%%I"
set "log=logs\FileDir-repair-!stamp!.log"
echo repairFileDir start %date% %time% > "!log!"
echo Script: %~f0 >> "!log!"
echo Folder: %cd% >> "!log!"
echo Command line: %~nx0 %* >> "!log!"
git log -1 --format=%%H -- build.cmd >> "!log!" 2>&1
git checkout -- build.cmd >> "!log!" 2>&1
echo git checkout build.cmd exit %errorlevel% >> "!log!"
if exist build.cmd (echo build.cmd is in place >> "!log!") else (echo WARNING: build.cmd is still missing; git has no committed copy >> "!log!")
powershell -NoProfile -Command ^
  "$n = 0;" ^
  "$lsEdits = @(@('Dialogs.cs', 'string sQuickDir = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)'), @('FileDir.cs', 'sDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);'));" ^
  "foreach ($e in $lsEdits) {" ^
  "  $f = $e[0]; $sOld = $e[1]; $sNew = $sOld.Replace('SpecialFolder.ApplicationData', 'SpecialFolder.LocalApplicationData');" ^
  "  if (-not (Test-Path -LiteralPath $f)) { 'missing ' + $f; continue }" ^
  "  $b = [System.IO.File]::ReadAllBytes($f); $bom = ($b.Length -ge 3 -and $b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF);" ^
  "  $t = [System.IO.File]::ReadAllText($f);" ^
  "  if (-not $t.Contains($sOld)) { 'the Roaming line is not in ' + $f + ' as expected; left alone'; continue }" ^
  "  $t = $t.Replace($sOld, $sNew);" ^
  "  [System.IO.File]::WriteAllText($f, $t, (New-Object System.Text.UTF8Encoding($bom)));" ^
  "  $n += 1; 'moved FileDir''s own folder to the Local tree in ' + $f" ^
  "}" ^
  "'' + $n + ' line(s) changed in all. The JAWS settings line in FileDir.cs stays on Roaming, where JAWS keeps them.'" >> "!log!" 2>&1
type "!log!" | findstr /C:"moved" /C:"changed" /C:"in place" /C:"WARNING"
echo The log is !log!
echo repairFileDir end %date% %time% >> "!log!"
popd
endlocal
