@echo off
cls
path=c:\winnt\Microsoft.NET\Framework\v2.0.50727;%path%
if exist FileDir.exe del FileDir.exe
csc.exe /nologo /r:FileAssociation.dll /r:Microsoft.JScript.dll /r:LbcJS.dll /r:Tektosyne.dll /r:ICSharpCode.SharpZipLib.dll /r:lbc.dll /r:LbcVB.dll /r:Microsoft.VisualBasic.dll FileDir.cs >errors.txt
if exist FileDir.exe FileDir.exe
