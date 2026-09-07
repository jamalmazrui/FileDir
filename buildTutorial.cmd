@echo off
rem buildTutorial.cmd -- write the tutorials and speak them, in one command.
rem
rem   buildTutorial                    every Tutorial*.inix: documents, then audio
rem   buildTutorial Tutorial_Tagging   just that one
rem   buildTutorial -docs              documents and feed only, no speaking
rem   buildTutorial -sapi              use Windows voices; fetch nothing
rem   buildTutorial -live              perform it now through JAWS, write no file
rem
rem The narrator is Piper with a neural voice; the screen reader's stand-in is
rem eSpeak NG. Both are free and are fetched once if they are missing.
rem The log is buildTutorial.log, beside the script.
setlocal
pushd "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0buildTutorial.ps1" %*
set iResult=%ERRORLEVEL%
popd
endlocal & exit /b %iResult%
