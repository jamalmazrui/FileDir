@echo off
rem installTranslateModel.cmd -- install the larger AI model FileDir uses for
rem the Translate File command when it is present. The small chat model
rem translates passably; this one translates well, at about 5 gigabytes.
rem
rem CONSOLE VERSUS LOG. The console gets short plain sentences. Commands, exit
rem codes and tool output go to the log.
rem
rem WHAT IT DID. Anything actually done is written to
rem FileDir_setup_actions.txt, which is what the Results box reports. A model
rem already downloaded that needed nothing writes no action.
rem
rem REINSTALL. Pass "reinstall" as an argument, or set FILEDIR_REINSTALL=1,
rem which is what the installer's Reinstall checkbox does. The model is then
rem pulled again; Ollama re-fetches only what has changed, so a repair costs
rem far less than the first download.
rem
rem NOTHING PAUSES.
setlocal
set "logDir=%LOCALAPPDATA%\FileDir\logs"
if not exist "%logDir%" mkdir "%logDir%" >nul 2>&1
set "logFile=%logDir%\FileDir_setup.log"
set "actionFile=%logDir%\FileDir_setup_actions.txt"
set "modelName=qwen2.5:7b"
set "didWhat=downloaded"
set "bReinstall="
set "sArgs=%*"
if not "%sArgs%"=="" echo %sArgs% | findstr /i /c:"reinstall" >nul && set "bReinstall=1"
if /i "%FILEDIR_REINSTALL%"=="1" set "bReinstall=1"

call :log "[installTranslateModel] started %date% %time%"
call :log "[installTranslateModel] arguments: %sArgs%"
call :log "[installTranslateModel] reinstall requested: %bReinstall%"

if exist "%LOCALAPPDATA%\Programs\Ollama" set "PATH=%LOCALAPPDATA%\Programs\Ollama;%PATH%"
where ollama >nul 2>&1
if errorlevel 1 goto no_ollama

call :ollamaModels
echo %modelList% | find /i "%modelName%" >nul 2>&1
if errorlevel 1 goto fetch
if not defined bReinstall goto already
set "didWhat=reinstalled"

:fetch
if defined bReinstall call :say "Reinstalling the %modelName% model."
if not defined bReinstall call :say "Fetching the %modelName% model, about 5 GB."
call :log "[installTranslateModel] ollama pull %modelName%"
call :ollamaPullHidden %modelName%
set "code=%errorlevel%"
call :log "[installTranslateModel] pull exit %code%"
if not "%code%"=="0" goto failed
call :action "The %modelName% translation model was %didWhat%."
call :say "Done."
call :log "[installTranslateModel] done"
exit /b 0

:already
call :log "[installTranslateModel] the model was already present and no reinstall was asked for"
exit /b 0

:no_ollama
call :say "Ollama is not installed, so the translation model cannot be fetched."
call :action "The %modelName% translation model was not fetched, because Ollama is not installed."
call :log "[installTranslateModel] FAILED: no ollama"
exit /b 7

:failed
call :say "The %modelName% model did not download. The log has the details."
call :action "The %modelName% translation model did not download."
call :log "[installTranslateModel] FAILED"
exit /b 3

:ollamaModels
set "modelList="
for /f "delims=" %%m in ('powershell -NoProfile -Command "try { (Invoke-RestMethod -Uri http://localhost:11434/api/tags -TimeoutSec 10).models.name -join \" \" } catch { \"\" }" 2^>nul') do set "modelList=%%m"
exit /b 0

:ollamaPullHidden
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
