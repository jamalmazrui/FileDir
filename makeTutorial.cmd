@echo off
rem makeTutorial.cmd -- write the walkthrough section of Tutorials.md.
rem
rem Reads Tutorial.inix and puts the walkthrough into Tutorials.md, between the
rem markers. Everything outside them is left alone. The log is makeTutorial.log,
rem beside the script.
setlocal
pushd "%~dp0"
python makeTutorial.py %*
set iResult=%ERRORLEVEL%
popd
endlocal & exit /b %iResult%
