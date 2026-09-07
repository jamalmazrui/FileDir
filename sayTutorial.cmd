@echo off
rem sayTutorial.cmd -- speak the walkthrough into Tutorial.mp3.
rem
rem Two Windows voices, one for the narrator and one standing in for the screen
rem reader, joined by ffmpeg. It says at the start that it is a simulation.
rem The log is sayTutorial.log, beside the script.
setlocal
pushd "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0sayTutorial.ps1" %*
set iResult=%ERRORLEVEL%
popd
endlocal & exit /b %iResult%
