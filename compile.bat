
@echo off
cls
rem path=c:\windows\Microsoft.NET\Framework\v2.0.50727;c:\winnt\Microsoft.NET\Framework\v2.0.50727;%path%
rem path=c:\windows\microsoft.net\framework\v4.0.30319;%path%
if exist FileDir.exe del FileDir.exe
rem c:\Roslyn\csc.exe /platform:x86 /out:FileDir.exe /nologo /t:winexe /r:FileAssociation.dll /r:Microsoft.JScript.dll /r:LbcJS.dll /r:Tektosyne.dll /r:ICSharpCode.SharpZipLib.dll /r:lbc.dll /r:LbcVB.dll /r:Microsoft.VisualBasic.dll FileDir.cs
rem c:\windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /platform:x86 /out:FileDir.exe /nologo /t:winexe /r:FileAssociation.dll /r:Microsoft.JScript.dll /r:LbcJS.dll /r:Tektosyne.dll /r:ICSharpCode.SharpZipLib.dll /r:lbc.dll /r:LbcVB.dll /r:Microsoft.VisualBasic.dll FileDir.cs
"c:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe" /platform:x86 /out:FileDir.exe /nologo /t:winexe /r:FileAssociation.dll /r:Microsoft.JScript.dll /r:LbcJS.dll /r:Tektosyne.dll /r:ICSharpCode.SharpZipLib.dll /r:lbc.dll /r:LbcVB.dll /r:Microsoft.VisualBasic.dll FileDir.cs
if errorlevel 1 goto end
rem C:\Roslyn\csc.exe /platform:x86 /out:FileDir.exe /nologo /t:winexe /r:FileAssociation.dll /r:Microsoft.JScript.dll /r:LbcJS.dll /r:Tektosyne.dll /r:ICSharpCode.SharpZipLib.dll /r:lbc.dll /r:LbcVB.dll /r:Microsoft.VisualBasic.dll FileDir.cs
:end
