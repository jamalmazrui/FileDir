
@echo off
cls
rem path=c:\windows\Microsoft.NET\Framework\v2.0.50727;c:\winnt\Microsoft.NET\Framework\v2.0.50727;%path%
rem path=c:\windows\microsoft.net\framework\v4.0.30319;%path%
if exist FileDir.exe del FileDir.exe
c:\Roslyn\csc.exe /platform:x86 /out:FileDir.exe /nologo /t:winexe /r:FileAssociation.dll /r:Microsoft.JScript.dll /r:LbcJS.dll /r:Tektosyne.dll /r:ICSharpCode.SharpZipLib.dll /r:lbc.dll /r:LbcVB.dll /r:Microsoft.VisualBasic.dll FileDir.cs
if errorlevel 1 goto end
rem C:\Roslyn\csc.exe /platform:x86 /out:FileDir.exe /nologo /t:winexe /r:FileAssociation.dll /r:Microsoft.JScript.dll /r:LbcJS.dll /r:Tektosyne.dll /r:ICSharpCode.SharpZipLib.dll /r:lbc.dll /r:LbcVB.dll /r:Microsoft.VisualBasic.dll FileDir.cs
:end
