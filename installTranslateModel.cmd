@echo off
rem installTranslateModel.cmd -- install the larger AI model FileDir uses for
rem the Translate File command when it is present. The small chat model
rem translates passably; this one translates well, at about 5 gigabytes.
rem Probe first, log milestones, never pause; the Results box reports the
rem outcome. The console says only what is happening.
setlocal
set "logFile=%LOCALAPPDATA%\FileDir\logs\FileDir_setup.log"
if not exist "%LOCALAPPDATA%\FileDir\logs" mkdir "%LOCALAPPDATA%\FileDir\logs" >nul 2>&1
set "modelName=qwen2.5:7b"
echo [installTranslateModel] started %date% %time% >> "%logFile%"

if exist "%LOCALAPPDATA%\Programs\Ollama" set "PATH=%LOCALAPPDATA%\Programs\Ollama;%PATH%"
where ollama >nul 2>&1
if errorlevel 1 goto no_ollama

call :ollamaModels
echo %modelList% | find /i "%modelName%" >nul 2>&1
if not errorlevel 1 (
  echo The %modelName% model is already installed.
  echo [installTranslateModel] already present >> "%logFile%"
  exit /b 0
)

echo Fetching the %modelName% model, about 5 GB
echo [installTranslateModel] ollama pull %modelName% >> "%logFile%"
call :ollamaPullHidden %modelName%
echo [installTranslateModel] pull exit %errorlevel% >> "%logFile%"
if errorlevel 1 goto failed
echo Done.
echo [installTranslateModel] done >> "%logFile%"
exit /b 0

:no_ollama
echo Ollama is not installed, so the translation model cannot be fetched.
echo Tick the Ollama box as well, or run installOllama.cmd first.
echo [installTranslateModel] FAILED: no ollama >> "%logFile%"
exit /b 7

:failed
echo The model did not download. The log is:
echo %logFile%
echo [installTranslateModel] FAILED >> "%logFile%"
exit /b 3

:ollamaModels
set "modelList="
for /f "delims=" %%m in ('powershell -NoProfile -Command "try { (Invoke-RestMethod -Uri http://localhost:11434/api/tags -TimeoutSec 10).models.name -join \" \" } catch { \"\" }" 2^>nul') do set "modelList=%%m"
exit /b 0

:ollamaPullHidden
powershell -NoProfile -Command "$p = Start-Process -FilePath 'ollama' -ArgumentList 'pull','%~1' -WindowStyle Hidden -PassThru -Wait; exit $p.ExitCode" >> "%logFile%" 2>&1
exit /b %errorlevel%
