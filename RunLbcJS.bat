@echo off
cls
if exist LbcJS.dll del LbcJS.dll
jsc.exe /nologo /r:System.Drawing.dll /r:Accessibility.dll /t:library LbcJS.js
