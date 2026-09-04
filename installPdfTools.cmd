@echo off
rem installPdfTools.cmd -- install the free PDF reader FileDir uses in place of
rem Microsoft Word: PyMuPDF4LLM, which turns a PDF's own structure into Markdown
rem with headings, lists and tables. Free, no Word, no account, about 25 MB.
rem
rem This is EdSharp's arrangement, kept deliberately identical so one habit and
rem one set of packages serve both programs.
rem
rem CONSOLE VERSUS LOG. The console gets short plain sentences. Commands, exit
rem codes and pip output go to the log.
rem
rem WHAT IT DID. Anything actually done is written to
rem FileDir_setup_actions.txt, which is what the Results box reports. A reader
rem already installed that needed nothing writes no action.
rem
rem REINSTALL. Pass "reinstall" as an argument, or set FILEDIR_REINSTALL=1,
rem which is what the installer's Reinstall checkbox does. pip is then given
rem --force-reinstall, since an upgrade of a current package does nothing and a
rem repair is the whole point of that checkbox.
rem
rem NOTHING PAUSES.
setlocal
set "logDir=%LOCALAPPDATA%\FileDir\logs"
if not exist "%logDir%" mkdir "%logDir%" >nul 2>&1
set "logFile=%logDir%\FileDir_setup.log"
set "actionFile=%logDir%\FileDir_setup_actions.txt"
set "didWhat="
set "bReinstall="
set "sArgs=%*"
if not "%sArgs%"=="" echo %sArgs% | findstr /i /c:"reinstall" >nul && set "bReinstall=1"
if /i "%FILEDIR_REINSTALL%"=="1" set "bReinstall=1"

call :log "[installPdfTools] started %date% %time%"
call :log "[installPdfTools] arguments: %sArgs%"
call :log "[installPdfTools] reinstall requested: %bReinstall%"

call :findPython
if not defined pythonExe call :getPython
if not defined pythonExe goto no_python
call :log "[installPdfTools] python: %pythonExe%"

"%pythonExe%" -c "import pymupdf4llm" >nul 2>&1
if errorlevel 1 goto install_reader
if defined bReinstall goto reinstall_reader

call :say "Updating the PDF reader."
call :log "[installPdfTools] pip install --upgrade pymupdf4llm"
"%pythonExe%" -m pip install --upgrade pymupdf4llm >> "%logFile%" 2>&1
call :log "[installPdfTools] upgrade exit %errorlevel%"
rem pip reports success whether or not there was anything to upgrade, so this
rem is not counted as an action.
goto verify

:reinstall_reader
call :say "Reinstalling the PDF reader."
call :log "[installPdfTools] pip install --force-reinstall pymupdf4llm"
"%pythonExe%" -m pip install --force-reinstall pymupdf4llm >> "%logFile%" 2>&1
call :log "[installPdfTools] reinstall exit %errorlevel%"
set "didWhat=reinstalled"
goto verify

:install_reader
call :say "Installing the PDF reader, about 25 MB."
call :log "[installPdfTools] pip install pymupdf4llm"
"%pythonExe%" -m pip install pymupdf4llm >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installPdfTools] install exit %code%"
set "didWhat=installed"
if not "%code%"=="0" goto failed

:verify
rem Prove it rather than assume it. pip returning 0 means pip ran, not that the
rem package can be imported: a wheel can install and still fail to load. The
rem import is attempted with the SAME interpreter that did the installing.
"%pythonExe%" -c "import pymupdf4llm; print('pymupdf4llm ready')" >> "%logFile%" 2>&1
if errorlevel 1 goto failed
call :log "[installPdfTools] verified pymupdf4llm with %pythonExe%"
rem Which interpreter has the package, recorded where FileDir will look. A
rem machine may carry several Pythons, and the one that installed it is the one
rem that can import it.
echo %pythonExe%> "%logDir%\FileDir_python.txt"
if not defined didWhat goto quietDone
call :action "PDF reader (PyMuPDF4LLM) %didWhat%."
call :say "The PDF reader is ready."
call :log "[installPdfTools] done"
exit /b 0

:quietDone
call :log "[installPdfTools] nothing needed doing"
exit /b 0

:no_python
call :say "Python could not be installed, so the PDF reader could not be either."
call :action "The PDF reader could not be installed, because Python is missing."
call :log "[installPdfTools] FAILED: no python, and winget could not install one"
exit /b 7

:getPython
rem INSTALL PYTHON RATHER THAN ASK FOR IT. The PDF reader box is TICKED, so
rem anyone without Python would otherwise get a failure and an errand on their
rem first run. Machine wide, like every other component. About 30 MB.
call :say "Installing Python, about 30 MB."
call :log "[installPdfTools] winget install Python.Python.3.13"
winget install --id Python.Python.3.13 -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installPdfTools] python install exit %code%"
if "%code%"=="0" goto pythonInstalled
call :log "[installPdfTools] retrying Python without a scope"
winget install --id Python.Python.3.13 -e --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
call :log "[installPdfTools] python retry exit %errorlevel%"

:pythonInstalled
rem A Python installed a moment ago is not on this console's path, so the fixed
rem locations are searched again rather than trusting "where".
call :findPython
if not defined pythonExe goto :eof
call :log "[installPdfTools] python now at %pythonExe%"
call :action "Python installed."
goto :eof

:failed
call :say "The PDF reader could not be installed. The log has the details."
call :action "The PDF reader could not be installed."
call :log "[installPdfTools] FAILED"
exit /b 3

:findPython
rem Windows answers "where python" with an app execution alias under
rem WindowsApps that is NOT Python: it advertises the Microsoft Store and exits.
rem Every candidate is checked for that path and made to answer --version.
set "pythonExe="
for /f "delims=" %%p in ('where python 2^>nul') do (
  echo %%p | find /i "WindowsApps" >nul
  if errorlevel 1 (
    if not defined pythonExe (
      "%%p" --version >nul 2>&1
      if not errorlevel 1 set "pythonExe=%%p"
    )
  )
)
if defined pythonExe exit /b 0
for %%d in (
  "%LOCALAPPDATA%\Programs\Python\Python313\python.exe"
  "%LOCALAPPDATA%\Programs\Python\Python312\python.exe"
  "%LOCALAPPDATA%\Programs\Python\Python311\python.exe"
  "%ProgramFiles%\Python313\python.exe"
  "%ProgramFiles%\Python312\python.exe"
  "%ProgramFiles%\Python311\python.exe"
) do (
  if not defined pythonExe if exist %%d set "pythonExe=%%~d"
)
exit /b 0

:log
>>"%logFile%" echo %~1
goto :eof

:say
echo %~1
>>"%logFile%" echo [console] %~1
goto :eof

:action
>>"%actionFile%" echo %~1
>>"%logFile%" echo [action] %~1
goto :eof
