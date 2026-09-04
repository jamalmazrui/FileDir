@echo off
rem installOllama.cmd -- smart install of Ollama plus the llama3.2 chat model,
rem which FileDir uses for the Translate File command.
rem
rem 64-bit by rule: the winget calls ask for the x64 build. Ollama installs per
rem user by its own design, into %LOCALAPPDATA%\Programs\Ollama with its models
rem under the profile -- that IS its default Windows location, and one
rem installation serves every program on the machine through its local service,
rem so it is left exactly there. EdSharp, DbDo and FileDir therefore share one
rem Ollama and one set of models: several gigabytes downloaded once, not once
rem per program.
rem
rem CONSOLE VERSUS LOG. The console gets short plain sentences. Commands, exit
rem codes and tool output go to the log.
rem
rem WHAT IT DID. Anything actually done is written to
rem FileDir_setup_actions.txt, which is what the Results box reports. A
rem component already present that needed nothing writes no action.
rem
rem REINSTALL. Pass "reinstall" as an argument, or set FILEDIR_REINSTALL=1,
rem which is what the installer's Reinstall checkbox does. Ollama itself is
rem installed again with --force; a model already downloaded is left alone,
rem since reinstalling the program is the repair being asked for and the model
rem is gigabytes that have not gone anywhere.
rem
rem NOTHING PAUSES.
setlocal
set "logDir=%LOCALAPPDATA%\FileDir\logs"
if not exist "%logDir%" mkdir "%logDir%" >nul 2>&1
set "logFile=%logDir%\FileDir_setup.log"
set "actionFile=%logDir%\FileDir_setup_actions.txt"
set "modelName=llama3.2"
set "bReinstall="
set "sArgs=%*"
if not "%sArgs%"=="" echo %sArgs% | findstr /i /c:"reinstall" >nul && set "bReinstall=1"
if /i "%FILEDIR_REINSTALL%"=="1" set "bReinstall=1"

call :log "[installOllama] started %date% %time%"
call :log "[installOllama] arguments: %sArgs%"
call :log "[installOllama] reinstall requested: %bReinstall%"

rem A just-installed Ollama is not on this console's PATH yet, so the install
rem location is probed as well as the name.
if exist "%LOCALAPPDATA%\Programs\Ollama\ollama.exe" set "PATH=%LOCALAPPDATA%\Programs\Ollama;%PATH%"
where ollama >nul 2>&1
if errorlevel 1 goto install_ollama
if defined bReinstall goto reinstall_ollama

call :say "Updating Ollama."
call :log "[installOllama] winget upgrade Ollama.Ollama"
winget upgrade --id Ollama.Ollama -e --architecture x64 --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installOllama] winget upgrade exit %code%"
if "%code%"=="0" call :action "Ollama updated."
if not "%code%"=="0" call :say "Ollama is already current."
goto pull_model

:reinstall_ollama
call :say "Reinstalling Ollama."
call :log "[installOllama] winget install Ollama.Ollama --force"
winget install --id Ollama.Ollama -e --force --architecture x64 --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
call :log "[installOllama] winget install exit %errorlevel%"
call :action "Ollama reinstalled."
goto pull_model

:install_ollama
call :say "Installing Ollama."
call :log "[installOllama] winget install Ollama.Ollama"
winget install --id Ollama.Ollama -e --architecture x64 --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
call :log "[installOllama] winget install exit %errorlevel%"
set "PATH=%LOCALAPPDATA%\Programs\Ollama;%PATH%"
where ollama >nul 2>&1
if errorlevel 1 goto fail_ollama
call :action "Ollama installed."

:pull_model
call :ollamaModels
echo %modelList% | find /i "%modelName%" >nul 2>&1
if not errorlevel 1 goto model_present
call :say "Fetching the %modelName% model, about 2 GB."
call :log "[installOllama] ollama pull %modelName%"
call :ollamaPullHidden %modelName%
set "code=%errorlevel%"
call :log "[installOllama] ollama pull exit %code%"
if not "%code%"=="0" goto fail_model
call :action "The %modelName% chat model was downloaded."
goto done_model

:model_present
call :log "[installOllama] the %modelName% model was already present"

:done_model
call :say "Done."
call :log "[installOllama] done"
exit /b 0

:fail_ollama
call :say "Ollama could not be installed. The log has the details."
call :action "Ollama could not be installed."
call :log "[installOllama] FAILED: ollama not found after install"
exit /b 3

:fail_model
call :say "The %modelName% model did not download. Run installOllama.cmd again later."
call :action "The %modelName% chat model did not download."
call :log "[installOllama] model pull failed"
exit /b 4

rem ---- Talking to Ollama without opening a window ----------------------
rem The ollama command starts its server in a console of its own when the
rem server is not already running, and that window stays on screen looking
rem like something has gone wrong. Ollama also answers over a local web
rem interface, which opens nothing, so presence and model lists are asked
rem that way; only a download needs the command, and that runs hidden.

:ollamaModels
rem Sets modelList to the names Ollama reports, or leaves it empty.
set "modelList="
for /f "delims=" %%m in ('powershell -NoProfile -Command "try { (Invoke-RestMethod -Uri http://localhost:11434/api/tags -TimeoutSec 10).models.name -join \" \" } catch { \"\" }" 2^>nul') do set "modelList=%%m"
exit /b 0

:ollamaPullHidden
rem Downloads %1 with no window of any kind. The command writes its progress
rem to the log rather than to a console nobody should have to look at.
powershell -NoProfile -Command "$p = Start-Process -FilePath 'ollama' -ArgumentList 'pull','%~1' -WindowStyle Hidden -PassThru -Wait; exit $p.ExitCode" >> "%logFile%" 2>&1
exit /b %errorlevel%

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
