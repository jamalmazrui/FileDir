@echo off
cls
if exist FileDir.dll del FileDir.dll
csc.exe /nologo /t:library /r:Microsoft.JScript.dll /r:LbcJS.dll /r:Tektosyne.dll /r:ICSharpCode.SharpZipLib.dll /r:lbc.dll /r:LbcVB.dll /r:Microsoft.VisualBasic.dll FileDir.cs
if exist FileDir.dll FileDir.dll
