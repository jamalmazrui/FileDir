@echo off
rem installPdfTools.cmd -- install the free PDF reader FileDir uses in place of
rem Microsoft Word: PyMuPDF4LLM, which turns a PDF's own structure into Markdown
rem with headings, lists and tables. Free, no Word, no account, about 25 MB.
rem
rem This is EdSharp's arrangement, kept deliberately identical so one habit and
rem one set of packages serve both programs. EdSharp's version also installs
rem WordNet for its thesaurus; FileDir has no thesaurus, so that part is left
rem out and nothing else is changed.
rem
rem Probe first, install or upgrade, verify, log milestones.
rem NOTHING PAUSES: a console waiting for a keypress interrupts the
rem installation. Failures are logged, and the summary shown at the very end
rem reports the outcome of every checkbox.
setlocal
set "logFile=%LOCALAPPDATA%\FileDir\logs\FileDir_setup.log"
if not exist "%LOCALAPPDATA%\FileDir\logs" mkdir "%LOCALAPPDATA%\FileDir\logs" >nul 2>&1
echo [installPdfTools] started %date% %time% >> "%logFile%"
echo.

call :findPython
if not defined pythonExe goto no_python
echo [installPdfTools] python: %pythonExe% >> "%logFile%"

"%pythonExe%" -c "import pymupdf4llm" >nul 2>&1
if errorlevel 1 goto install_reader
echo Updating the PDF reader
echo [installPdfTools] pip install --upgrade pymupdf4llm >> "%logFile%"
"%pythonExe%" -m pip install --upgrade pymupdf4llm >> "%logFile%" 2>&1
echo [installPdfTools] upgrade exit %errorlevel% >> "%logFile%"
goto verify

:install_reader
echo Installing the PDF reader, about 25 MB
echo [installPdfTools] pip install pymupdf4llm >> "%logFile%"
"%pythonExe%" -m pip install pymupdf4llm >> "%logFile%" 2>&1
echo [installPdfTools] install exit %errorlevel% >> "%logFile%"
if errorlevel 1 goto failed

:verify
rem Prove it rather than assume it. pip returning 0 means pip ran, not that the
rem package can be imported: a wheel can install and still fail to load. The
rem import is attempted with the SAME interpreter that did the installing, and
rem whatever it says goes into the log, so a disagreement with the summary can
rem never be a mystery.
"%pythonExe%" -c "import pymupdf4llm; print('pymupdf4llm ready')" >> "%logFile%" 2>&1
if errorlevel 1 goto failed
echo [installPdfTools] verified pymupdf4llm with %pythonExe% >> "%logFile%"
rem Which interpreter has the package, recorded where FileDir will look. A
rem machine may carry several Pythons, and the one that installed it is the one
rem that can import it.
echo %pythonExe%> "%LOCALAPPDATA%\FileDir\logs\FileDir_python.txt"
echo PDF reader ready.
echo [installPdfTools] done >> "%logFile%"
exit /b 0

:no_python
echo Python was not found, so the PDF reader cannot be installed.
echo Install Python from python.org, then run this script again.
echo [installPdfTools] FAILED: no python >> "%logFile%"
exit /b 7

:failed
echo The PDF reader did not install. The log is:
echo %logFile%
echo [installPdfTools] FAILED >> "%logFile%"
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
