; Wordvert
; Beta 0.5
; October 15, 2011
; Copyright 2011 by Jamal Mazrui
; GNU Lesser General Public License (LGPL)

#Include <File.au3>
#Include <GuiEdit.au3>
#Include <Misc.au3>
#include <SendMessage.au3>



Dim $aSourceFiles
Dim $sSourceSpec, $sSourceFolder, $sWildcards, $sSourceFile, $sSourceName, $sTargetFile, $sTargetFormat, $sTargetExtension, $sTargetFolder
Dim $iSourceFile, $iTargetFormat, $iArgCount, $iConvertCount, $iSourceCount
Dim $oApp, $oDocs, $oDoc, $oExtensions

Const $vbTextCompare = 1
Const $xUTF8 = "EFBBBF"
Const $xUTF16 = "FFFE"

Const $msoEncodingUTF8 = 65001
Const $wdFormatRTF = 6
Const $wdFormatUnicodeText = 7
Const $wdFormatPDF = 17
Const $wdFormatDocumentDefault = 16
Const $wdFormatDocument = 0
Const $wdFormatFilteredHTML = 10
Const $wdFormatText = 2
Const $wdFormatPlainText = 22

Func _Show($sTitle, $sMessage = "")
Return MsgBox(4096, $sTitle, $sMessage)
EndFunc



Func _CreateDictionary()
Dim $oDictionary
$oDictionary = ObjCreate("Scripting.Dictionary")
$oDictionary.CompareMode = $vbTextCompare
Return $oDictionary
EndFunc

Func _FileDelete($sFile)
; Delete a file if it exists, and test whether it is subsequently absent
; either because it was successfully deleted or because it was not present in the first place

Dim $oSystem

If Not _FileExists($sFile) Then Return True

$oSystem =ObjCreate("Scripting.FileSystemObject")
Call $oSystem.DeleteFile($sFile, True)
Return Not _FileExists($sFile)
EndFunc

Func _FileExists($sFile)
; Test whether File exists

Dim $oSystem

$oSystem =ObjCreate("Scripting.FileSystemObject")
Return Not $oSystem.FolderExists($sFile) And $oSystem.FileExists($sFile)
EndFunc

Func _FileToString($sFile)
; Get content of text file

Const $ForReading = 1
Const $ASCII = 0
Const $Unicode = -1
Dim $oSystem, $oFile

$oSystem =ObjCreate("Scripting.FilesystemObject")
$oFile = $oSystem.OpenTextFile($sFile, $ForReading, $False, $ASCII)
$sReturn = $oFile.ReadAll()
$oFile.Close()
Return $sReturn
EndFunc

Func _FolderExists($sFolder)
; Test whether folder exists

Dim $oSystem

$oSystem =ObjCreate("Scripting.FileSystemObject")
Return $oSystem.FolderExists($sFolder)
EndFunc

Func _PathCombine($sFolder, $sName)
; Combine folder and name to form a valid path

Dim $sPath

$sPath = StringStripWS($sFolder, 3) & "\" & StringStripWS($sName, 3)
Return StringReplace($sPath, "\\", "\")
EndFunc

Func _PathGetBase($sPath)
; Get base/root name of a file or folder

Dim $oSystem

$oSystem =ObjCreate("Scripting.FileSystemObject")
Return $oSystem.GetBaseName($sPath)
EndFunc

Func _PathGetCurrentDirectory()
; Get current directory of active process

Dim $oShell

$oShell =ObjCreate("Wscript.Shell")
Return $oShell.CurrentDirectory
EndFunc

Func _PathGetExtension($sPath)
; Get extention of file or folder

Dim $oSystem

$oSystem =ObjCreate("Scripting.FileSystemObject")
Return $oSystem.GetExtensionName($sPath)
EndFunc

Func _PathGetFolder($sPath)
; Get the parent folder of a file or folder

Dim $oSystem
$oSystem =ObjCreate("Scripting.FileSystemObject")
Return $oSystem.GetParentFolderName($sPath)
EndFunc

Func _PathGetName($sPath)
; Get the file or folder name at the end of a path

Dim $oSystem

$oSystem =ObjCreate("Scripting.FileSystemObject")
Return $oSystem.GetFileName($sPath)
EndFunc

Func _StringPlural($sItem, $iCount)
; Return singular or plural form of a string, depending on whether count equals one

Dim $sReturn

$sReturn = $iCount & " " & $sItem
If $iCount <> 1 Then $sReturn = $sReturn & "s"
Return $sReturn
EndFunc

; Main program
$oExtensions = _CreateDictionary()
$oExtensions("txt") = $wdFormatText
$oExtensions("doc") = $wdFormatDocument
$oExtensions("docx") = $wdFormatDocumentDefault
$oExtensions("rtf") = $wdFormatRTF
$oExtensions("pdf") = $wdFormatPDF
$oExtensions("htm") = $wdFormatFilteredHTML
$oExtensions("html") = $wdFormatFilteredHTML

$iArgCount = $CmdLine[0]
$sSourceSpec = "*.doc"
If $iSourceCount > 0 Then $sSourceSpec = $CmdLine[1]
$sTargetExtension = "txt"
If $iArgCount > 1 Then $sTargetExtension = $CmdLine[2]
$iTargetFormat = $oExtensions($sTargetExtension)

$sSourceFolder = _PathGetFolder($sSourceSpec)
If $sSourceFolder = "" Then $sSourceFolder = _PathGetCurrentDirectory()
$sTargetFolder = $sSourceFolder
If $iArgCount > 2 Then $sTargetFolder = $CmdLine(2)
$sWildcards = _PathGetName($sSourceSpec)
$aSourceFiles = _FileListToArray($sSourceFolder, $sWildcards, 1)
If IsArray($aSourceFiles) Then $iSourceCount  = $aSourceFiles[0]
For $iSourceFile = 1 To $iSourceCount
If $iSourceFile = 1 Then
$oApp = ObjCreate("Word.Application")
$oApp.Visible = False
$oApp.DisplayAlerts = False
$oApp.ScreenUpdating = False
$oDocs = $oApp.Documents
EndIf

$sSourceFile = $aSourceFiles[$iSourceFile]
$sSourceFile = _PathCombine($sSourceFolder, $sSourceFile)
$sTargetFile = _PathCombine($sTargetFolder, _PathGetBase($sSourceFile) & "." & $sTargetExtension)
If Not _FileExists($sSourceFile) Or (StringLower($sSourceFile) = StringLower($sTargetFile)) Then ContinueLoop

$iSourceCount = $iSourceCount + 1
$sSourceName = _PathGetName($sSourceFile)
ConsoleWrite("Converting " & $sSourceName)

; Set $oDoc = $oDocs.Open($sSourceFile, AddToRecentFiles = False, ReadOnly = True, ConfirmConversions = False)
_Show($sSourceFile)
$oDoc = $oDocs.Open($sSourceFile, False, True, False)
; $oDoc.SaveAs(FileName, FileFormat, LockComments, Password, AddToRecentFiles, WritePassword, ReadOnlyRecommended, EmbedTrueTypeFonts, SaveNativePictureFormat, SaveFormsData, SaveAsAOCELetter, Encoding, InsertLineBreaks, AllowSubstitutions, LineEnding, AddBiDiMarks)
If _FileExists($sTargetFile) Then _FileDelete($sTargetFile)
ConsoleWrite($sTargetFile)
$oDoc.SaveAs($sTargetFile, $iTargetFormat, False, "", False, "", False, False, False, False, False, $msoEncodingUTF8  )
If _FileExists($sTargetFile) Then $iConvertCount = $iConvertCount + 1
$oDoc.Close()

If $iSourceFile = $iSourceCount Then $oApp.Quit()
Next

ConsoleWrite("Converted " & $iSourceCount & " out of " & _StringPlural("file", $iConvertCount))
