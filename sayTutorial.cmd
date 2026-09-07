@echo off
rem sayTutorial.cmd -- speak the walkthrough into Tutorial.mp3.
rem
rem   sayTutorial                  speak Tutorial.inix into Tutorial.mp3
rem   sayTutorial Tutorial_Tagging speak that script into its own .mp3
rem   sayTutorial Tutorial -live   perform it now, through JAWS, writing no file
rem
rem Two voices: one for the narrator, one standing in for the screen reader,
rem which is Eloquence when Windows reports such a voice. It says at the start
rem that it is a simulation. The log is sayTutorial.log, beside the script.
setlocal
pushd "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0sayTutorial.ps1" %*
set iResult=%ERRORLEVEL%
popd
endlocal & exit /b %iResult%
