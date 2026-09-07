@echo off
rem makeTutorial.cmd -- the documents step on its own.
rem
rem buildTutorial is the command to use: it writes the documents AND speaks the
rem tutorials. This wrapper exists because makeTutorial.py is a script, and
rem every script here has a wrapper so nobody has to type the PowerShell or
rem Python invocation from memory. buildTutorial -docs does the same thing.
setlocal
pushd "%~dp0"
python makeTutorial.py %*
set iResult=%ERRORLEVEL%
popd
endlocal & exit /b %iResult%
