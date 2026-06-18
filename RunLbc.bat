@echo off
cls
path=c:\windows\Microsoft.NET\Framework\v2.0.50727;c:\winnt\Microsoft.NET\Framework\v2.0.50727;%path%
if exist lbc.dll del lbc.dll
csc.exe /nologo /t:library /r:FileDir.dll /r:Microsoft.VisualBasic.dll /r:Microsoft.VisualBasic.Compatibility.dll lbc.cs
