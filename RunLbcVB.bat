@echo off
cls
if exist LbcVB.dll del LbcVB.dll
vbc.exe /nologo /t:library /r:Microsoft.VisualBasic.dll /r:lbc.dll LbcVB.vb
