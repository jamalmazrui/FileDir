Option Explicit

Dim aSourceFiles
Dim sSourceSpec, sSourceFolder, sWildcards, sSourceFile, sSourceName, sTargetFile, sTargetFormat, sTargetExtension, sTargetFolder 
Dim iSourceFile, iTargetFormat, iArgCount, iConvertCount, iSourceCount
Dim oApp, oDocs, oDoc, oExtensions

Const xUTF8 = "EFBBBF"
Const xUTF16 = "FFFE"

Const msoEncodingUTF8 = 65001 ' &HFDE9
Const wdFormatRTF = 6
Const wdFormatUnicodeText = 7
Const wdFormatPDF = 17 ' &H11
Const wdFormatDocumentDefault = 16 ' &H10
Const wdFormatDocument = 0
Const wdFormatFilteredHTML = 10
Const wdFormatText = 2
Const wdFormatPlainText = 22 ' &H16

Function CreateDictionary()
Dim oDictionary
Set oDictionary = CreateObject("Scripting.Dictionary")
oDictionary.CompareMode = vbTextCompare
Set CreateDictionary = oDictionary
End Function



Function FileCopy(sSource, sTarget)
' Copy source to destination file, replacing if it exists

Dim oSystem

FileCopy = False
If Not FileDelete(sTarget) Then Exit Function

Set oSystem =CreateObject("Scripting.FileSystemObject")
On Error Resume Next
oSystem.CopyFile sSource, sTarget
On Error GoTo 0
FileCopy = FileExists(sTarget)

Set oSystem = Nothing
End Function

Function FileDelete(sFile)
' Delete a file if it exists, and test whether it is subsequently absent
' either because it was successfully deleted or because it was not present in the first place

Dim oSystem

FileDelete = True
If Not FileExists(sFile) Then Exit Function

Set oSystem =CreateObject("Scripting.FileSystemObject")
Call oSystem.DeleteFile(sFile, True)
FileDelete = Not FileExists(sFile)

Set oSystem = Nothing
End Function

Function FileExists(sFile)
' Test whether File exists

Dim oSystem

Set oSystem =CreateObject("Scripting.FileSystemObject")
FileExists =Not oSystem.FolderExists(sFile) And oSystem.FileExists(sFile)

Set oSystem =Nothing
End Function

Function FileGetDate(sFile)
' Get date of a file

Dim oSystem, oFile

FileGetDate = vbNull
If Not FileExists(sFile) Then Exit Function

Set oSystem =CreateObject("Scripting.FileSystemObject")
Set oFile =oSystem.GetFile(sFile)
FileGetDate =oFile.DateLastModified

Set oFile = Nothing
Set oSystem = Nothing
End Function

Function FileGetSize(sFile)
' Get size of a file

Dim oSystem, oFile

FileGetSize = 0
If Not FileExists(sFile) Then Exit Function

Set oSystem =CreateObject("Scripting.FileSystemObject")
Set oFile =oSystem.GetFile(sFile)
FileGetSize =oFile.size

Set oFile = Nothing
Set oSystem = Nothing
End Function

Function FileGetType(sFile)
' Get file type

Dim oSystem, oFile

FileGetType = ""
If Not FileExists(sFile) Then Exit Function

Set oSystem =CreateObject("Scripting.FileSystemObject")
Set oFile =oSystem.GetFile(sFile)
FileGetType =oFile.Type

Set oFile = Nothing
Set oSystem = Nothing
End Function

Function FileIsUTF8(sFile)
' Test whether file is UTF-8
Const ForReading = 1
Const ASCII = 0
Const Unicode = -1
Dim oSystem, oFile
Dim s1, s2, s3

FileIsUTF8 = False
If FileGetSize(sFile) < 3 Then Exit Function

Set oSystem = CreateObject("Scripting.FileSystemObject")
Set oFile = oSystem.OpenTextFile(sFile, ForReading, ASCII)
s1 = Hex(AscB(MidB(oFile.Read(1), 1, 1)))
s2 = Hex(AscB(MidB(oFile.Read(1), 1, 1)))
s3 = Hex(AscB(MidB(oFile.Read(1), 1, 1)))
oFile.Close
If s1 & s2 & s3 = xUTF8 Then FileIsUTF8 = True
Set oFile = Nothing
Set oSystem = Nothing
End Function

Function FileIsUnicode(sFile)
' Test whether file is Unicode
Const ForReading = 1
Const ASCII = 0
Const Unicode = -1
Dim oSystem, oFile
Dim s1, s2

FileIsUnicode = False
If FileGetSize(sFile) < 2 Then Exit Function

Set oSystem = CreateObject("Scripting.FileSystemObject")
Set oFile = oSystem.OpenTextFile(sFile, ForReading, ASCII)
s1 = Hex(AscB(MidB(oFile.Read(1), 1, 1)))
s2 = Hex(AscB(MidB(oFile.Read(1), 1, 1)))
oFile.Close
' msgbox xutf16, 0, s1 & s2
If s1 & s2 = xUTF16 Then FileIsUnicode = True
If s2 & s1 = xUTF16 Then FileIsUnicode = True
Set oFile = Nothing
Set oSystem = Nothing
End Function

Function FileMove(sSource, sTarget)
' Move source to destination file, replacing if it exists

Dim oSystem

FileMove = False
If Not FileDelete(sTarget) Then Exit Function

Set oSystem =CreateObject("Scripting.FileSystemObject")
Call oSystem.MoveFile(sSource, sTarget)
FileMove = FileExists(sTarget)

Set oSystem = Nothing
End Function

Function FileToString(sFile)
' Get content of text file

Const ForReading = 1
Const ASCII = 0
Const Unicode = -1
Dim oSystem, oFile

FileToString = ""
If FileGetSize(sFile) = 0 Then Exit Function

Set oSystem =CreateObject("Scripting.FilesystemObject")
' if 1 then
If FileIsUnicode(sFile) Then
' DialogShow "unicode", ""
Set oFile = oSystem.OpenTextFile(sFile, ForReading, False, Unicode)
Else
' DialogShow "ascii", ""
Set oFile = oSystem.OpenTextFile(sFile, ForReading, False, ASCII)
End If
FileToString =oFile.ReadAll
oFile.Close

Set oFile = Nothing
Set oSystem = Nothing
End Function



Function PathCombine(sFolder, sName)
' Combine folder and name to form a valid path

Dim sPath

sPath = Trim(sFolder) & "\" & Trim(sName)
PathCombine = Replace(sPath, "\\", "\")
End Function

Function PathCreateTempFolder()
' Create temporary folder and return its full path

Dim sFolder

PathCreateTempFolder = ""
sFolder = PathGetTempFolder() & "\" & PathGetTempName()
If FolderCreate(sFolder) Then PathCreateTempFolder = sFolder
End Function

Function PathExists(sPath)
' Test whether path exists

Dim oSystem

Set oSystem =CreateObject("Scripting.FileSystemObject")
PathExists =oSystem.FolderExists(sPath) Or oSystem.FileExists(sPath)

Set oSystem =Nothing
End Function

Function PathGetBase(sPath)
' Get base/root name of a file or folder

Dim oSystem

Set oSystem =CreateObject("Scripting.FileSystemObject")
PathGetBase =oSystem.GetBaseName(sPath)

Set oSystem = Nothing
End Function

Function PathGetCurrentDirectory()
' Get current directory of active process

Dim oShell

Set oShell =CreateObject("Wscript.Shell")
PathGetCurrentDirectory =oShell.CurrentDirectory

Set oShell = Nothing
End Function

Function PathGetExtension(sPath)
' Get extention of file or folder

Dim oSystem

Set oSystem =CreateObject("Scripting.FileSystemObject")
PathGetExtension =oSystem.GetExtensionName(sPath)

Set oSystem = Nothing
End Function

Function PathGetFolder(sPath)
' Get the parent folder of a file or folder

Dim oSystem
Set oSystem =CreateObject("Scripting.FileSystemObject")
PathGetFolder =oSystem.GetParentFolderName(sPath)

Set oSystem = Nothing
End Function

Function PathGetLong(sPath)
' Get long name of file or folder

Dim oShell, oShortcut

Set oShell = CreateObject("WScript.Shell")
Set oShortcut = oShell.CreateShortcut("temp.lnk")
oShortcut.TargetPath = sPath
PathGetLong = oShortcut.TargetPath

Set oShortcut = Nothing
Set oShell = Nothing
End Function

Function PathGetName(sPath)
' Get the file or folder name at the end of a path

Dim oSystem

Set oSystem =CreateObject("Scripting.FileSystemObject")
PathGetName =oSystem.GetFileName(sPath)

Set oSystem = Nothing
End Function

Function PathGetShort(sPath)
' Get short path (8.3 style) of a file or folder

Dim oSystem, oFile, oFolder

Set oSystem =CreateObject("Scripting.FileSystemObject")
If FolderExists(sPath) Then
Set oFolder =oSystem.GetFolder(sPath)
PathGetShort =oFolder.ShortPath
Else
Set oFile =oSystem.GetFile(sPath)
PathGetShort =oFile.ShortPath
End If

Set oFile = Nothing
Set oFolder = Nothing
Set oSystem = Nothing
End Function

Function PathGetSpec(sDir, sWildcards, sFlags)
' Get an Array of paths, specifying folder, wild card pattern, and sort order

Const WindowStyle = 0 'hidden
Const Wait = True
Dim aReturn
Dim i, iBound
Dim s, sCommand,sTempFile, sReturn

sCommand = "%COMSPEC% /c dir /b " &  sFlags & " " & Chr(34) & sDir & "\" & sWildcards & Chr(34)
sTempFile = PathGetTempFile()
sCommand = sCommand & " >" & sTempFile
ShellRun sCommand, WindowStyle, Wait
' sReturn = StringTrimWhiteSpace(FileToString(sTempFile))
sReturn = Trim(FileToString(sTempFile))
FileDelete(sTempFile)
PathGetSpec = Array()
If Len(sReturn) = 0 Then Exit Function

aReturn = Split(sReturn, vbCrLf)
' iBound = ArrayBound(aReturn)
iBound = UBound(aReturn)
For i = 0 To iBound
s = aReturn(i)
If Not InStr(s, ":") Then aReturn(i) = PathCombine(sDir, s)
Next
PathGetSpec = aReturn
End Function

Function PathGetSpecialFolder(sFolder)
' Get a special folder of Windows
Dim oShell, oFolders
Dim s

PathGetSpecialFolder = ""
Set oShell =CreateObject("WScript.Shell")
Set oFolders =oShell.SpecialFolders
For Each s In oFolders
If StringTrail("\" & s, sFolder, False) Then
PathGetSpecialFolder = s
End If
Next

Set oFolders = Nothing
Set oShell = Nothing
End Function

Function PathGetTempFile()
' Get full path of a temporary file

PathGetTempFile = PathGetTempFolder() & "\" & PathGetTempName()
End Function

Function PathGetTempFolder()
' Get Windows folder for temporary files

Const TempFolder = 2
Dim oSystem

Set oSystem =CreateObject("Scripting.FileSystemObject")
PathGetTempFolder =oSystem.GetSpecialFolder(TempFolder).path

Set oSystem = Nothing
End Function

Function PathGetTempName()
' Get Name for temporary file or folder

Dim oSystem

Set oSystem =CreateObject("Scripting.FileSystemObject")
PathGetTempName = oSystem.GetTempName()

Set oSystem = Nothing
End Function

Function PathGetValid(sDir, sBase, sExt, bUnique)
Dim i, iCount
Dim s, sIllegal, sLine, sPrintable, sSourceDir, sSourceExt, sSourceBase, sTargetDir, sTargetExt, sTargetFile, sTargetBase, sViewable

sIllegal = "@%*+\|':'<>/?" & Chr(34)
sViewable = "!#$%&'()*+,-./0123456789:'<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
sPrintable = " " & sViewable

sSourceDir = sDir
sSourceBase = sBase
sSourceExt = sExt
If Len(sSourceExt) > 0 And Not StringLead(sSourceExt, ".", False) Then sSourceExt = "." & sSourceExt
sLine = sSourceBase
iCount = Len(sIllegal)
For i = 1 To iCount
s = Mid(sIllegal, i, 1)
If InStr(sLine, s) Then sLine = Replace(sLine, s, " ")
Next

sLine = StringReplaceAll(sLine, "  ", " ")
sLine = Trim(sLine)

sTargetBase = sLine
sTargetFile = sSourceDir & "\" & sTargetBase & sSourceExt
If bUnique And FileExists(sTargetFile) Then
s = "_01"
sTargetFile = sSourceDir & "\" & sTargetBase & s &sSourceExt
i = 1
Do While FileExists(sTargetFile) And i <= 99
If i < 10 Then
s = "_0" & i
Else
s = "_" & i
End If

sTargetFile = sSourceDir & "\" & sTargetBase & s & sSourceExt
i = i +1
Loop
End If
PathGetValid = sTargetFile
End Function

Function PathSetCurrentDirectory(sDir)
' Set current directory of active process

Dim oShell

Set oShell =CreateObject("Wscript.Shell")
oShell.CurrentDirectory = sDir

Set oShell = Nothing
End Function

Function ShellCreateShortcut(sFile, sTargetPath, sWorkingDirectory, iWindowStyle, sHotkey)
' Create a .lnk or .url file

Dim oShell, oShortcut

ShellCreateShortcut = False
If Not FileDelete(sFile) Then Exit Function

Set oShell = CreateObject("WScript.Shell")
Set oShortcut = oShell.CreateShortcut(sFile)
oShortcut.TargetPath = sTargetPath
oShortcut.WorkingDirectory = sWorkingDirectory
oShortcut.WindowStyle = iWindowStyle
oShortcut.Hotkey = sHotkey
oShortcut.Save()
ShellCreateShortcut = FileExists(sFile)

Set oShortcut = Nothing
Set oShell = Nothing
End Function

Function ShellExec(sCommand)
' Run a console mode command and return its standard output

Dim oShell, oExec, oOutput

Set oShell =CreateObject("Wscript.Shell")
Set oExec =oShell.Exec(sCommand)
Do While oExec.Status =0
Sleep(10)
Loop

Set oOutput =oExec.StdOut
ShellExec =oOutput.ReadAll()
oExec.Terminate()

Set oOutput = Nothing
Set oExec = Nothing
Set oShell = Nothing
End Function

Function ShellExecute(sFile, sParams, sFolder, sVerb, iWindowStyle)
ShellExecute = True
Set oShell = CreateObject("Shell.Application")
On Error Resume Next
oShell.ShellExecute sFile, sParams, sFolder, sVerb, iWindowStyle
On Error GoTo 0
If Err.Number Then ShellExecute = False
End Function

Function ShellExpandEnvironmentVariables(sText)
' Replace environment variables with their values

Dim oShell

Set oShell =CreateObject("Wscript.Shell")
ShellExpandEnvironmentVariables =oShell.ExpandEnvironmentStrings(sText)

Set oShell = Nothing
End Function

Function ShellGetDrives()
' Return a string sequence of drives that are ready for access

Dim i
Dim oSystem, oDrive
Dim sReturn, sDrive,sDrives

sReturn = ""
Set oSystem = CreateObject("Scripting.FileSystemObject")
sDrives = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
i = 1
Do While i <= 26
sDrive = Mid(sDrives, i, 1)
If oSystem.DriveExists(sDrive) Then
Set oDrive = oSystem.GetDrive(sDrive)
If oDrive.IsReady Then
sReturn = sReturn & sDrive
End If

Set oDrive = Nothing
End If
i = i + 1
Loop
ShellGetDrives = sReturn

Set oSystem = Nothing
End Function

Function ShellGetEnvironmentVariable(sVariable)
' Get the value of an environment variable

Dim oShell, oEnv

Set oShell =CreateObject("Wscript.Shell")
Set oEnv =oShell.Environment
ShellGetEnvironmentVariable =oEnv.Item(sVariable)

Set oEnv = Nothing
Set oShell = Nothing
End Function

Function ShellGetRecentPaths(sType)
Dim aPaths, aLinks
Dim bAdd
Dim oPaths
Dim sFolder, sWildcards, sFlags, sLink, sPath

sType = LCase(Trim(sType))
If sType = "" Then sType = "both"

ArrayClear aPaths
Set oPaths = CreateDictionary

sFolder = PathGetSpecialFolder("Recent")
sWildcards = "*.lnk"
sFlags = "/o:-d"
aLinks = PathGetSpec(sFolder, sWildcards, sFlags)
For Each sLink in aLinks
sPath = ShellGetShortcutTargetPath(sLink)
bAdd = False
If oPaths.Exists(sPath) Then
' Do nothing
ElseIf sType = "both" And PathExists(sPath) Then
bAdd = True
ElseIf sType = "folders" And FolderExists(sPath) Then
bAdd = True
ElseIf sType = "files" And FileExists(sPath) Then
bAdd = True
End If

If bAdd Then
oPaths.Add sPath, ""
ArrayAdd aPaths, sPath
End If
Next

ShellGetRecentPaths = aPaths
End Function

Function ShellGetShortcutTargetPath(sFile)
' Get the target path of a shortcut file

Dim oShell, oShortcut

Set oShell = CreateObject("WScript.Shell")
Set oShortcut = oShell.CreateShortcut(sFile)
ShellGetShortcutTargetPath = oShortcut.TargetPath
End Function

Function ShellGetSpecialFolder(vFolder)
Dim oShell, oNamespace, oFolder

Set oShell = CreateObject("Shell.Application")
Set oNamespace = oShell.Namespace(vFolder)
Set oFolder = oNamespace.Self
ShellGetSpecialFolder = oFolder.Path
Set oFolder = Nothing
Set oNamespace = Nothing
Set oShell = Nothing
End Function

Function ShellGetSpecialFolders()
Dim aNames, aValues
Dim i, iCount
Dim oShell, oFolder, oFolders, oPaths, oNames, oItem
Dim s, sName, sValue

Set oShell =CreateObject("WScript.Shell")
Set oFolders =oShell.SpecialFolders
iCount = oFolders.Count
aNames = Array()
aValues = Array()
Set oPaths = CreateDictionary
Set oNames = CreateDictionary
For i = 0 To iCount - 1
sValue = oFolders.Item(i)
sName = PathGetName(sValue)
If InStr(1, sValue, "\All Users\", vbTextCompare) Then
sName = "Common " & sName
ElseIf InStr(1, sValue, "\Users\", vbTextCompare) Then
sName = "My " & sName
ElseIf oNames.Exists(sName) And InStr(1, sValue, "\Local Settings\", vbTextCompare) Then
sName = "Local " & sName
End If
sName = Replace(sName, "Common My ", "Common ")
s = Left(sName, 1)
' If Not oPaths.Exists(sValue) And Not IsNumeric(sName) And s <> ":" And s <> "{" Then
If Not oPaths.Exists(sValue) And FolderExists(sValue) And Not IsNumeric(sName) Then
oPaths.Add sValue, sName
If Not oNames.Exists(sName) Then oNames.Add sName, ""
End If
Next

Set oShell = CreateObject("Shell.Application")
For i = 0 To 100
Set oFolder = oShell.Namespace(i)
If IsSomething(oFolder) Then
Set oItem = oFolder.Self
sValue = oItem.Path
sName = PathGetName(sValue)
If InStr(1, sValue, "\All Users\", vbTextCompare) Then
sName = "Common " & sName
ElseIf InStr(1, sValue, "\Users\", vbTextCompare) Then
sName = "My " & sName
ElseIf oNames.Exists(sName) And InStr(1, sValue, "\Local Settings\", vbTextCompare) Then
sName = "Local " & sName
End If
sName = Replace(sName, "Common My ", "Common ")
s = Left(sName, 1)
' If Not oPaths.Exists(sValue) And Not IsNumeric(sName) And s <> ":" And s <> "{" Then
If Not oPaths.Exists(sValue) And FolderExists(sValue) And Not IsNumeric(sName) Then
oPaths.Add sValue, sName
If Not oNames.Exists(sName) Then oNames.Add sName, ""
End If
End If
Next

OPaths.Add PathGetTempFolder, "Temp"
oPaths.Add ClientInformation.ScriptPath, "Window-Eyes User Profile"
Set ShellGetSpecialFolders = oPaths
Set oPaths = Nothing
Set oNames = Nothing
Set oShell = Nothing
End Function

Function ShellGetWindowsNTName()
' Get name of the Windows NT version installed
Dim iKey
Dim sSubkey, sValueName

iKey = 2 ' HKEY_LOCAL_MACHINE
sSubkey = "SOFTWARE\Microsoft\Windows NT\CurrentVersion"
sValueName = "ProductName"
ShellGetWindowsNTName = RegistryRead("HKEY_LOCAL_MACHINE\" & sSubkey & "\" & sValueName)
End Function



Function ShellInvokeVerb(sPath, sVerb)
Dim oShell, oFolder, oName
Dim sFolder, sName

sFolder = PathGetFolder(sPath)
sName = PathGetName(sPath)
Set oShell = CreateObject("Shell.Application")
Set oFolder = oShell.Namespace(sFolder)
Set oName = oFolder.ParseName(sName)
oName.InvokeVerb sVerb
End Function

Function ShellRun(sFile, iStyle, bWait)
' Launch a program or file, indicating its window style and whether to wait before returning
' window styles:
' 0 Hides the window and activates another window.
' 1 Activates and displays a window. If the window is minimized or maximized, the
' system restores it to its original size and position. This flag should be used
' when specifying an application for the first time.
' 2 Activates the window and displays it minimized.
' 3 Activates the window and displays it maximized.
' 4 Displays a window in its most recent size and position. The active window
' remains active.
' 5 Activates the window and displays it in its current size and position.
' 6 Minimizes the specified window and activates the next top-level window in the Z
' order.
' 7 Displays the window as a minimized window. The active window remains active.
' 8 Displays the window in its current state. The active window remains active.
' 9 Activates and displays the window. If it is minimized or maximized, the system
' restores it to its original size and position. An application should specify
' this flag when restoring a minimized window.
' 10 Sets the show state based on the state of the program that started the
' application.

Dim oShell

Set oShell =CreateObject("Wscript.Shell")
ShellRun = -2
On Error Resume Next
ShellRun =oShell.Run(sFile, iStyle, bWait)
On Error GoTo 0

Set oShell = Nothing
End Function

Function ShellRunCommandPrompt(sDir)
' Open a command prompt in the directory specified

ShellRun "%COMSPEC% /k cd " & Chr(34) & sDir & Chr(34), 1, False
End Function

Function ShellRunExplorerWindow(sDir)
' Open Windows Explorer in the directory specified

ShellOpen sDir
' ShellRun "explorer.exe " & Chr(34) & sDir & Chr(34), 1, False
End Function

Function ShellOpen(sPath)
ShellRun StringQuote(sPath), 1, False
End Function

Function ShellOpenWith(sExe, sParam)
ShellOpenWith = ShellRun(StringQuote(sExe) & " " & StringQuote(sParam), 1, False)
End Function

Function ShellUrlToFile(sUrl, sFile)
Dim oXhttp, oAdoDb
Dim sBody, sExe

ShellUrlToFile = False
If Not FileDelete(sFile) Then Exit Function

On Error Resume Next
Set oXhttp = CreateObject("WinHttp.WinHttpRequest.5.1")
' Set oXhttp = CreateAjaxObject()
Const WinHttpRequestOption_SslErrorIgnoreFlags  = 4  
oXhttp.Option(WinHttpRequestOption_SslErrorIgnoreFlags) = &H3300 ' ignore all server certificate errors  
Const WinHttpRequestOption_EnableHttpsToHttpRedirects = 12  
oXhttp.Option(WinHttpRequestOption_EnableHttpsToHttpRedirects  ) = True

oXhttp.Open "GET", sUrl, False
oXhttp.Send

If oXhttp.Status = 200 Then
sBody = oXhttp.ResponseBody
Set oAdoDb = CreateObject("ADODB.Stream")
oAdoDb.Type = 1 ' binary
oAdoDb.Open
oAdoDb.Write sBody
oAdoDb.SaveToFile sFile, 1 ' create if not exist
oAdoDb.Close
End If
On Error GoTo 0

sExe = PathGetShort(ClientInformation.ScriptPath) & "\url2file.exe"
If FileGetSize(sFile) = 0 And FileExists(sExe) Then
ShellRun sExe & " " & sUrl & " " & StringQuote(sFile), 0, True
End If

sExe = PathGetShort(ClientInformation.ScriptPath) & "\NetUrl2File.exe"
If FileGetSize(sFile) = 0 And FileExists(sExe) Then
ShellRun sExe & " " & sUrl & " " & StringQuote(sFile), 0, True
End If

If FileGetSize(sFile) > 0 Then ShellUrlToFile = True
Set oXhttp = Nothing
Set oAdoDb = Nothing
End Function

Function ShellWait(sPath)
ShellRun StringQuote(sPath), 0, True
End Function

Function StringPlural(sItem, iCount)
' Return singular or plural form of a string, depending on whether count equals one

Dim sReturn

sReturn = CStr(iCount) & " " & sItem
If iCount <> 1 Then sReturn = sReturn & "s"
StringPlural = sReturn
End Function






' Main program
Set oExtensions = CreateDictionary()
oExtensions("txt") = wdFormatText  
oExtensions("doc") = wdFormatDocument  
oExtensions("docx") = wdFormatDocumentDefault  
oExtensions("rtf") = wdFormatRTF  
oExtensions("pdf") = wdFormatPDF  
oExtensions("htm") = wdFormatFilteredHTML  
oExtensions("html") = wdFormatFilteredHTML  

iArgCount = WScript.Arguments.Count
sSourceSpec = WScript.Arguments(0)
sTargetExtension = "txt"
If iArgCount > 1 Then sTargetExtension = WScript.Arguments(1)
iTargetFormat = oExtensions(sTargetExtension)

sSourceFolder = PathGetFolder(sSourceSpec)
If sSourceFolder = "" Then sSourceFolder = PathGetCurrentDirectory()
sTargetFolder = sSourceFolder
If iArgCount > 2 Then sTargetFolder = WScript.Arguments(2)
sWildcards = PathGetName(sSourceSpec)
aSourceFiles = PathGetSpec(sSourceFolder, sWildcards, "")
For iSourceFile = 0 To UBound(aSourceFiles)
If iSourceFile = 0 Then
Set oApp = CreateObject("Word.Application")
oApp.Visible = False
oApp.DisplayAlerts = False
oApp.ScreenUpdating = False

Set oDocs = oApp.Documents
End If

sSourceFile = aSourceFiles(iSourceFile)
sTargetFile = PathCombine(sTargetFolder, PathGetBase(sSourceFile) & "." & sTargetExtension)
If FileExists(sSourceFile) And (LCase(sSourceFile) <> LCase(sTargetFile)) Then
iSourceCount = iSourceCount + 1
sSourceName = PathGetName(sSourceFile)
WScript.echo("Converting " & sSourceName)


' Set oDoc = oDocs.Open(sSourceFile, AddToRecentFiles = False, ReadOnly = True, ConfirmConversions = False)
Set oDoc = oDocs.Open(sSourceFile, False, True, False)
' oDoc.SaveAs(FileName, FileFormat, LockComments, Password, AddToRecentFiles, WritePassword, ReadOnlyRecommended, EmbedTrueTypeFonts, SaveNativePictureFormat, SaveFormsData, SaveAsAOCELetter, Encoding, InsertLineBreaks, AllowSubstitutions, LineEnding, AddBiDiMarks)
If FileExists(sTargetFile) Then FileDelete(sTargetFile)
WScript.Echo(sTargetFile)
Call oDoc.SaveAs(sTargetFile, iTargetFormat, False, "", False, "", False, False, False, False, False, msoEncodingUTF8  )
If FileExists(sTargetFile) Then iConvertCount = iConvertCount + 1
oDoc.Close()
End If
If iSourceFile = UBound(aSourceFiles) Then oApp.Quit()
Next

WScript.Echo("Converted " & iSourceCount & " out of " & StringPlural("file", iConvertCount))
