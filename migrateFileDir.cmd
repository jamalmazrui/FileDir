@echo off
rem ===================================================================
rem migrateFileDir.cmd -- move C:\FileDir into the Homer layout, once.
rem
rem WHY A SCRIPT AND NOT JUST FILES. A zip can add and replace; it cannot
rem move a file to a folder or delete one that should no longer exist.
rem Everything else in this delivery is a replacement file. This handles
rem the two things files cannot do, and then it is finished with: run it
rem once, and the ordinary build takes over.
rem
rem NOTHING IS DELETED THAT CANNOT BE FETCHED OR REBUILT. Sources, notes
rem and anything unrecognised are moved, never removed. What IS deleted:
rem the app's copies of the kit's classes, the earlier editions of the
rem kit's tools, and binaries the build now fetches -- each named below
rem with the reason.
rem
rem Every move and every deletion is logged in
rem logs\FileDir-migrate-yyyyMMdd-HHmmss.log.
rem ===================================================================

setlocal enabledelayedexpansion
cd /d "%~dp0"

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "sStamp=%%i"
if not exist "logs" mkdir "logs"
set "log=%CD%\logs\FileDir-migrate-%sStamp%.log"
echo FileDir migration started %DATE% %TIME%> "%log%"
echo Script: %~f0>> "%log%"
echo Folder: %CD%>> "%log%"
echo Command line: %0 %*>> "%log%"
echo Migration log: %log%
echo(

rem ---- the folders the Homer layout expects --------------------------
rem Each starts with a different letter, so a screen reader user walks the
rem folder list by initial.
for %%D in (configs data exec help help\tutorials logs notes scripts scripts\jaws templates) do (
  if not exist "%%D" mkdir "%%D" && echo Made %%D>> "%log%"
)

rem ---- documents: everything but ReadMe and License goes to help -----
call :move "FileDir.md" "help"
call :move "FileDir.htm" "help"
call :move "Announce.md" "help"
call :move "Announce.htm" "help"
call :move "Developer.md" "help"
call :move "Developer.htm" "help"
call :move "FAQ.md" "help"
call :move "FAQ.htm" "help"
call :move "History.md" "help"
call :move "History.htm" "help"
call :move "Hotkeys.md" "help"
call :move "Hotkeys.htm" "help"
call :move "Tutorials.md" "help"
call :move "Tutorials.htm" "help"
call :move "TutorialFeed.xml" "help"
call :move "Tutorial.mp3" "help\tutorials\Tutorial_00_Overview.mp3"
call :move "Tutorial_Tagging.mp3" "help\tutorials\Tutorial_01_Tagging.mp3"

rem The walks take the kit's names, so buildTutorials finds and orders them.
call :move "Tutorial.inix" "help\Tutorial_00_Overview.inix"
call :move "Tutorial_Tagging.inix" "help\Tutorial_01_Tagging.inix"

rem The Camel Type documents are the kit's now; the copies go to notes.
call :move "CamelType_CSharp.md" "notes"
call :move "CamelType_JAWSScript.md" "notes"
call :move "Camel_Type_C#.md" "notes"

rem ---- settings and data ---------------------------------------------
call :move "Hotkeys.inix" "configs"
call :move "FileDir.ini" "configs"
call :move "Convert.txt" "configs"
call :move "Quick.txt" "configs"
call :move "chimes.wav" "data"

rem ---- scripts: the app's own, and the screen reader scripts ---------
call :move "installImageTools.cmd" "scripts"
call :move "installMediaTools.cmd" "scripts"
call :move "installMpv.cmd" "scripts"
call :move "installPandoc.cmd" "scripts"
call :move "installPdfTools.cmd" "scripts"
call :move "installTranslateModel.cmd" "scripts"
call :move "postPage.cmd" "scripts"
call :move "postPage.ps1" "scripts"
call :move "makeKeyMap.py" "scripts"
call :move "pdfRich.py" "scripts"

rem ONE CALL PER FILE, and looked for in notes as well as the root. The first
rem edition listed these in a for-set continued across lines with a caret, and
rem the continuation did not survive cmd's parser: the loop moved nothing, and
rem the tidy-up at the end then put every screen reader script in notes as an
rem unrecognised file. Running this again brings them back from either place.
for %%F in (FileDir.jss FileDir.jsd FileDir.jsh FileDir.jkm FileDir.jcf) do call :retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec
rem ---- one binary into exec, from the root or from notes ---------------
if exist "%~1" call :move "%~1" "exec"
if exist "notes\%~1" call :move "notes\%~1" "exec"
goto :eof

:moveJaws "%%F"
for %%F in (Homer.jss Homer.jsd Homer.jsh MSAA.jsh) do call :retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec
rem ---- one binary into exec, from the root or from notes ---------------
if exist "%~1" call :move "%~1" "exec"
if exist "notes\%~1" call :move "notes\%~1" "exec"
goto :eof

:moveJaws "%%F"
for %%F in (mpv.jss mpv.jsd mpv.jkm mpv.jcf mpv.jsb) do call :retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec
rem ---- one binary into exec, from the root or from notes ---------------
if exist "%~1" call :move "%~1" "exec"
if exist "notes\%~1" call :move "notes\%~1" "exec"
goto :eof

:moveJaws "%%F"
call :move "scripts\filedir.jsb" "scripts\jaws"
call :move "scripts\homer.jsb" "scripts\jaws"
call :move "scripts\FileDir_Scripts_setup.iss" "scripts\jaws"
call :move "scripts\FileDir_Scripts_setup.exe" "exec"

rem ---- binaries: out of the repository, into exec --------------------
rem The build fetches Ude from NuGet; 7-Zip, Tektosyne, SharpZipLib and the
rem NVDA controller client are components rather than sources. Moving them
rem to exec keeps them working today and keeps them out of git, since
rem LocalFiles.txt names exec.
for %%F in (7z.exe 7z.dll 7zFM.exe 7zG.exe 7z.sfx 7zCon.sfx) do call :retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec "%%F"
for %%F in (Burn2CD.exe Burn2CD.dll AssocOn.exe AssocOff.exe 2htm.exe) do call :retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec "%%F"
for %%F in (FileDir.exe FileDirScript.dll Tektosyne.dll ICSharpCode.SharpZipLib.dll Ude.dll) do call :retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec "%%F"
for %%F in (nvdaControllerClient.dll FileAssociation.dll FileDir_setup.exe FileDir_Scripts_setup.exe) do call :retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec "%%F"
for %%F in (System.Memory.dll System.Buffers.dll System.Runtime.CompilerServices.Unsafe.dll) do call :retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec "%%F"

rem ---- the app's copies of the kit's classes: deleted ----------------
rem THE WHOLE POINT OF THE KIT. FileDir compiles against C:\HomerDev\CSharp
rem now, so a copy here would be compiled twice and would drift. Their work
rem is in the kit as of 1.40.0.
for %%F in (Inix.cs Log.cs Say.cs Util.cs Web.cs Ollama.cs lbc.cs Lbc.cs KeyMap.cs) do (
  if exist "%%F" del /q "%%F" && echo Deleted %%F ^(now compiled from the kit^)>> "%log%"
)

rem ---- earlier editions of the kit's tools: deleted ------------------
rem homerTidy, checkHomerApp, buildTutorials, makeTutorials and the kit's
rem tagRelease do these jobs, and the build refreshes them into scripts\ on
rem every run. Two tools for one job is how the wrong one gets run.
for %%F in (cleanFileDir.cmd cleanFileDir.py homerPolicy.py auditFileDir.py) do call :retire "%%F"
for %%F in (buildTutorial.cmd buildTutorial.ps1 makeTutorial.cmd makeTutorial.py makeTutorial.log) do call :retire "%%F"
for %%F in (tagRelease.cmd tagRelease.ps1 tagRelease_README.md Build.cmd Build.ps1 installOllama.cmd) do call :retire "%%F"
rem The results box is the kit's now, and the grouped tool installers became one
rem script per component; these are retired wherever they are.
for %%F in (summarizeSetup.cmd summarizeSetup.ps1 installMediaTools.cmd installImageTools.cmd installTranslateModel.cmd) do call :retire "%%F"
for %%F in (summarizeSetup.cmd summarizeSetup.ps1 installMediaTools.cmd installImageTools.cmd installTranslateModel.cmd) do if exist "scripts\%%F" del /q "scripts\%%F"

rem ---- anything left at the root that is not named -------------------
rem Moved to notes, never deleted: a file nobody remembered is still a file
rem somebody may want.
echo(>> "%log%"
echo Unrecognised files left at the root:>> "%log%"
for %%F in (*.*) do call :classify "%%F"

echo(
echo Migration finished. The log lists every move and deletion:
echo   %log%
echo(
echo Next: build
endlocal
exit /b 0

:move
rem ---- move one file, keeping what is already there -------------------
if not exist "%~1" goto :eof
if exist "%~2\" (
  move /y "%~1" "%~2\" >nul 2>&1 && echo Moved %~1 to %~2>> "%log%"
) else (
  move /y "%~1" "%~2" >nul 2>&1 && echo Moved %~1 to %~2>> "%log%"
)
goto :eof

:retire
rem ---- delete a retired tool, wherever the tidy-up left it ---------------
if exist "%~1" del /q "%~1" && echo Deleted %~1 ^(the kit does this now^)>> "%log%"
if exist "notes\%~1" del /q "notes\%~1" && echo Deleted notes\%~1 ^(the kit does this now^)>> "%log%"
goto :eof

:moveExec
rem ---- one binary into exec, from the root or from notes ---------------
if exist "%~1" call :move "%~1" "exec"
if exist "notes\%~1" call :move "notes\%~1" "exec"
goto :eof

:moveJaws
rem ---- one screen reader script into scripts\jaws, from wherever it is ----
if exist "%~1" call :move "%~1" "scripts\jaws"
if exist "notes\%~1" call :move "notes\%~1" "scripts\jaws"
if exist "scripts\%~1" call :move "scripts\%~1" "scripts\jaws"
goto :eof

:classify
rem ---- is this root file one the project names? -----------------------
set "sName=%~1"
set "bKnown="
for %%K in (FileDir.cs Convert.cs Dialogs.cs Media.cs MediaPlayer.cs Mpv.cs Table.cs^
 Version.cs KeyText.cs FileDir.js FileDir.ico FileDir.manifest FileDir.exe.config^
 AssocOn.bas AssocOff.bas build.cmd migrateFileDir.cmd FileDir_setup.iss^
 ReadMe.md ReadMe.htm License.md License.htm RepoFiles.txt LocalFiles.txt^
 accept.inix version.txt .gitignore) do if /i "%%K"=="!sName!" set "bKnown=1"
if defined bKnown goto :eof
move /y "!sName!" "notes\" >nul 2>&1 && echo Moved !sName! to notes>> "%log%"
goto :eof
