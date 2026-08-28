Include "HJConst.jsh"
Use "Homer.jsb"

Globals
Int iiFileDirInitialized,
Handle hPrevious,
String sFileDirIniFile, String sFileDirTempFile

Void Function AutoStartEvent()
Var
int i,
String s

;SwitchToConfiguration("default")
If !iiFileDirInitialized Then
Let s = GetActiveConfiguration()
Let sFileDirIniFile = GetJAWSSettingsDirectory() + "\\" + s + ".ini"
Let i = StringContains(sFileDirIniFile, "\\Freedom Scientific\\")
Let sFileDirTempFile = StringLeft(sFileDirIniFile, i) + s + "\\" + s + ".tmp"
Let iiFileDirInitialized = True
EndIf

Let s = IniReadString("Options", "Punctuation", "", sFileDirIniFile)
If !StringIsBlank(s) Then
SetJCFOption(OPT_PUNCTUATION,  StringToInt(s))
EndIf
SayString(GetWindowName(GetTopLevelWindow(GetFocus())))
EndFunction

void Function WindowActivatedEvent(handle hWnd)
If GetAppFileName() != "FileDir.exe" Then
Return
EndIf

If GetWindowClass(hWnd) == "#32770" Then
Return
EndIf

; SayString(GetWindowName(GetTopLevelWindow(GetFocus())))
SayString(GetWindowName(hwnd))
EndFunction

Void Function WindowMinMaxEvent (handle hWnd, int nMinMaxRest, int nShow)
If hWnd == hPrevious Then
Return
EndIf

Let hPrevious = hWnd
If GetAppFileName() != "FileDir.exe" Then
Return
EndIf

; saystring("nMinMaxRest")
; SayInteger(nMinMaxRest)
; SayString("nShow")
; SayInteger(nShow)
If nMinMaxRest != 1 Then
; If nMinMaxRest > 1 Then
Return
EndIf

; SayString(GetWindowName(GetTopLevelWindow(GetFocus())))
SayString(GetWindowName(hwnd))
EndFunction

int Function HandleCustomWindows(handle h)
;copytoclipboard(getclipboardtext() + "\n" + GetWindowClass(h))
If MenusActive() && GetWindowClass (h) == "WindowsForms10.Window." Then
SayFocusedObject()
Return True
EndIf
Return HandleCustomWindows(h)
EndFunction

Void Function SaveVoiceSetting(String sSetting, Int iLevel)
Var
Int iLoop,
String sJcf, String sVoice, String sVoiceList

Let sJcf = GetActiveConfiguration() + ".jcf"
Let sVoiceList = "Global|Error|Keyboard|Screen|PCCursor|JAWSCursor|Message"
Let iLoop = 1
While iLoop
Let sVoice = StringSegment(sVoiceList, "|", iLoop)
If StringIsBlank(sVoice) Then
Let iLoop = 0
Else
Let sVoice = "eloq-" + sVoice + "Context"
IniWriteInteger(sVoice, sSetting, iLevel, sJcf)
Let iLoop = iLoop + 1
EndIf
EndWhile
EndFunction

Int Function new_SayFileByLine(String sFile)
Var
Object oFile, Object oNull, Object oSystem,
String sLine

Let oSystem =CreateObjectEx("Scripting.FileSystemObject", False)
Let oFile =oSystem.OpenTextFile(sFile, 1, 0)
While !oFile.AtEndOfStream && !IsKeyWaiting()
Let sLine =oFile.ReadLine()
If !StringIsBlank(sLine) Then
SayString(sLine)
EndIf
EndWhile
oFile.Close()
Let oFile =oNull
Let oSystem =oNull
EndFunction

String Function FileToString(String sFile)
;Get content of text (not binary) file

Var
Int iRead,
Object oSystem, Object oFile, Object oNull,
String sReturn

Let oSystem =CreateObject("Scripting.FilesystemObject")
Let oFile =oSystem.OpenTextFile(sFile)
Let sReturn =oFile.ReadAll()
oFile.close()

Let oFile = oNull
Let oSystem = oNull
Return sReturn
EndFunction

Int Function StringToFile(String sText, String sFile)
;Saves string to text file, replacing if it exists

Var
Int iReturn, Int iReplace,
Object oSystem, Object oFile, Object oNull

Let oSystem =CreateObject("Scripting.FilesystemObject")
Let iReplace = True
Let oFile =oSystem.CreateTextFile(sFile, iReplace)
oFile.write(sText)
oFile.close()
Let iReturn = FileExists(sFile)
Let oFile = oNull
Let oSystem = oNull

Return iReturn
EndFunction

Int Function SayTempFile()
If FileExists(sFileDirTempFile) Then
Var
String sText

new_SayFileByLine(sFileDirTempFile)
;SayString(FileToString(sFileDirTempFile))
Return True
Else
Return False
EndIf
EndFunction

Void Function TestForFileDirScripts()
EndFunction

Script BottomOfFile()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript BottomOfFile()
EndIf
EndScript

Script CloseDocumentWindow()
If !IsVirtualPCCursor() && GetWindowClass(GetCurrentWindow()) == "RichEdit20A" Then
SayString("Close window")
TypeCurrentScriptKey()
Else
PerformScript CloseDocumentWindow()

EndIf
EndScript

Script CommandLineDirectory()
TypeKey("Control+Slash")
EndScript

Script ControlDownArrow()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript ControlDownArrow()
EndIf
EndScript

Script ControlUpArrow()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript ControlUpArrow()
EndIf
EndScript

Script CopySelectedTextToClipboard()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript CopySelectedTextToClipboard()
EndIf
EndScript

Script CutToClipboard()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript CutToClipboard()
EndIf
EndScript

Script DeleteWord()
; Control+Delete = Delete word and say new one
If CaretVisible() Then
SpeechOff()
PCCursor()
SelectNextWord()
{Delete}
Pause()
SpeechOn()
SayWord()
Else
TypeCurrentScriptKey()
EndIf
EndScript

Script DeleteWordBack()
; Control+Backspace = Delete word back and say new one
Var
String s

If CaretVisible() Then
SpeechOff()
PCCursor()
PriorWord()
Let s = GetWord()
SelectNextWord()
{Delete}
Pause()
SpeechOn()
SayString(s)
Else
PerformScript ControlBackspace()
EndIf
EndScript

Script ExplorerDirectory()
TypeKey("Alt+Slash")
EndScript

Script JAWSBackspace()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript JAWSBackspace()
EndIf
EndScript

Script JAWSEnd()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript JAWSEnd()
EndIf
EndScript

Script JAWSHome()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript JAWSHome()
EndIf
EndScript

Script JAWSPageDown()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript JAWSPageDown()
EndIf
EndScript

Script JAWSPageUp()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript JAWSPageUp()
EndIf
EndScript

Script MouseDown()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript MouseDown()
EndIf
EndScript

Script MouseUp()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript MouseUp()
EndIf
EndScript

Script NextParagraph()
If !IsVirtualPCCursor() && GetWindowClass(GetCurrentWindow()) == "RichEdit20A" Then
TypeKey("F9")
Else
TypeCurrentScriptKey()
EndIf
EndScript

Script PasteFromClipboard()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript PasteFromClipboard()
EndIf
EndScript

Script PreviousParagraph()
If !IsVirtualPCCursor() && GetWindowClass(GetCurrentWindow()) == "RichEdit20A" Then
TypeKey("Shift+F9")
Else
TypeCurrentScriptKey()
EndIf
EndScript

Script SayActiveCursor()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeKey("Shift+5")
Else
PerformScript SayActiveCursor()
EndIf
EndScript

Script SayCurrentAccessKey()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript SayCurrentAccessKey()
EndIf
EndScript

Script SayLine()
Var
Int i,
Object o, Object oNull,
String sKey, String sClass, String sText
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
;SayLine()
Let o = GetFocusObject(0)
If o Then
Let i = o.AccFocus
Let sText = o.AccName(i)
EndIf
If StringIsBlank(sText) Then
Let sText = GetLine()
EndIf
If IsSameScript() Then
SpellString(sText)
Else
SayString(sText)
EndIf
Let o = oNull
Else
PerformScript SayLine()
EndIf
EndScript

Script sayparagraph ()
If !IsVirtualPCCursor() && GetWindowClass(GetCurrentWindow()) == "RichEdit20A" Then
TypeKey("Alt+F9")
Else
TypeCurrentScriptKey()
EndIf
EndScript

Script SaySelectedText()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeKey("Shift+Space")
Else
PerformScript SaySelectedText()
EndIf
EndScript

Script ScriptFileName ()
ScriptAndAppNames ("FileDir")
EndScript

Script SelectFromStartOfLine()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript SelectFromStartOfLine()
EndIf
EndScript

Script SelectNextLine()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript SelectNextLine()
EndIf
EndScript

Script SelectPriorLine()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript SelectNextLine()
EndIf
EndScript

Script SelectToEndOfLine()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript SelectToEndOfLine()
EndIf
EndScript

Script SpeedFaster ()
; Control+Accent = make voice faster
Var
Int iLevel, Int iMax, Int iMin,
String sSetting

SayString("Speed faster")
Let sSetting = "Speed"
Let iLevel =GetVoiceRate(VCTX_GLOBAL , True)
GetSynthRateRange(iMin, iMax)
If iLevel == iMax Then
SayString("Top")
Else
Let iLevel = iLevel +(5 *(iMax -iMin) /100)
Let iLevel =Min(iLevel, iMax)
SetVoiceRate(VCTX_GLOBAL , iLevel)
;GetVoiceRate(VCTX_GLOBAL, True)
SaveVoiceSetting(sSetting, iLevel)
SayString(IntToString(100 *(iLevel -iMin)/(iMax -iMin)) +" percent")
EndIf
EndScript

Script SpeedSlower ()
; Control+Shift+Accent = make voice slower
Var
Int iLevel, Int iMax, Int iMin,
String sSetting

SayString("Speed slower")
Let sSetting = "Speed"
Let iLevel =GetVoiceRate(VCTX_GLOBAL , True)
GetSynthRateRange(iMin, iMax)
If iLevel == iMin Then
SayString("Bottom")
Else
Let iLevel =iLevel -(5 * (iMax -iMin) /100)
Let iLevel =max(iLevel, iMin)
SetVoiceRate(VCTX_GLOBAL , iLevel)
;GetVoiceRate(VCTX_GLOBAL, True)
SaveVoiceSetting(sSetting, iLevel)
SayString(IntToString(100 *(iLevel -iMin)/(iMax -iMin)) +" percent")
EndIf
EndScript

Script TogglePunctuation()
; Insert+Accent = Toggle between all and no punctuation
Var
Int iLevel,
String sSetting

Let sSetting = "Punctuation"
Let iLevel = GetJCFOption(OPT_PUNCTUATION)
If iLevel != 0 Then
SayString("No Punctuation")
Let iLevel = 0
Else
SayString("All punctuation")
Let iLevel = 3
EndIf
SetJCFOption(OPT_PUNCTUATION, iLevel)
;SaveVoiceSetting(sSetting, iLevel)
IniWriteInteger("Options", "Punctuation", iLevel, sFileDirIniFile)
EndScript

Script TopOfFile()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript TopOfFile()
EndIf
EndScript

Script TypeCurrentScriptKey()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()
If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
Else
SayCurrentScriptKeyLabel()
EndIf
TypeCurrentScriptKey()
EndScript

Script UIEvaluateExpression()
TypeKey("Control+E")
EndScript

Script Undo()
Var
String sKey, String sClass
Let sClass = GetWindowClass(GetFocus())
Let sKey = GetCurrentScriptKeyName()

If (!IsVirtualPCCursor() && IsPCCursor() && sClass == "WindowsForms10.LISTBOX." || sClass == "WindowsForms10.MDICLIENT.") Then
TypeCurrentScriptKey()
Else
PerformScript Undo()
EndIf
EndScript

Script VolumeLouder()
; Alt+Accent = make voice louder
Var
Int iLevel, Int iMax, Int iMin,
String sSetting

SayString("Volume louder")
Let sSetting = "Volume"
Let iLevel =GetVoiceVolume(VCTX_GLOBAL , True)
GetVoiceVolumeRange(iMin, iMax)
Let iLevel =GetSystemVolume()
GetSystemVolumeRange(iMin, iMax)
If iLevel ==iMax Then
SayString("Top")
Else
Let iLevel =iLevel +(5 *(iMax -iMin) /100)
Let iLevel =Min(iLevel, iMax)
SetVoiceVolume(VCTX_GLOBAL , iLevel)
SetSystemVolume(iLevel)
SaveVoiceSetting(sSetting, iLevel)
SayString(IntToString(100 *(iLevel -iMin)/(iMax -iMin)) +" percent")
EndIf
EndScript

Script VolumeSofter()
; Alt+Shift+Accent = make voice softer
Var
Int iLevel, Int iMax, Int iMin,
String sSetting

SayString("Volume softer")
Let sSetting = "Volume"
Let iLevel =GetVoiceVolume(VCTX_GLOBAL , True)
GetVoiceVolumeRange(iMin, iMax)
Let iLevel =GetSystemVolume()
GetSystemVolumeRange(iMin, iMax)
If iLevel ==iMin Then
SayString("Bottom")
Else
Let iLevel =iLevel -(5 * (iMax -iMin) /100)
Let iLevel =max(iLevel, iMin)
SetVoiceVolume(VCTX_GLOBAL , iLevel)
SetSystemVolume(iLevel)
SaveVoiceSetting(sSetting, iLevel)
SayString(IntToString(100 *(iLevel -iMin)/(iMax -iMin)) +" percent")
EndIf
EndScript

String Function StringTrimWhiteSpace(String sText)
; Trim leading and trailing white space characters

Var
String sReturn

Let sReturn = StringTrim(sText)
Let sReturn = RegExpReplaceCase(sReturn, "^\\s+", "")
Let sReturn = RegExpReplaceCase(sReturn, "\\s+$", "")
Return sReturn
EndFunction

String Function PathGetBase(String sFile)
;Get base/root name

Var
Object oSystem, Object oNull,
String sReturn

Let oSystem =ObjectCreate("Scripting.FileSystemObject")
Let sReturn =oSystem.GetBaseName(sFile)

Let oSystem = oNull
Return sReturn
EndFunction

String Function DialogPickWithIndex(String sTitle, String sValues, Int bSort, Int iIndex)
;Get choice from a standard list box
Var
Handle h,
Int iChoice,
String sReturn

If StringIsBlank(sTitle) Then
Let sTitle = "Pick"
EndIf
If bSort Then
Let sValues = StringSegmentSort(sValues, "\7")
EndIf
Let iChoice = DlgSelectItemInList(sValues, sTitle, False, iIndex)
If iChoice Then
Let sReturn = StringSegment(sValues, "\7", iChoice)
EndIf
Return sReturn
EndFunction

String Function ConvertToUnixLineBreak(String sText)
;Convert to Unix line break, \n
Var
String sMatch, String sReplace

Let sMatch = "\r\n"
Let sReplace = "\n"
Let sText = RegExpReplaceCase(sText, sMatch, sReplace)
Let sMatch = "\r"
Let sText = RegExpReplaceCase(sText, sMatch, sReplace)
Return sText
EndFunction

Script UIWebClientUtilities()
Var
Int bSort,
Int i, Int iCount, Int iIndex,
String sWebClientFile, String sBody, String sCommand, String sExe, String sDir, String sFiles, String sFile, String sNames, String sName, String sValues, String sValue, String sBase, String sTitle, String sInputFile, String sOutputFile, String sCodeFile

Let sDir = GetJAWSSettingsDirectory()
Let sFiles = PathGetDir(sDir, "WebClient_*.py", "")
Let sFiles = ConvertToUnixLineBreak(sFiles)
Let i = 1
Let iCount = StringSegmentCount(sFiles, "\n")
While i <= iCount
Let sFile = StringSegment(sFiles, "\n", i)
Let sName = PathGetName(sFile)
Let sBase = PathGetBase(sName)
Let sBase = StringChopLeft(sBase, StringLength("WebClient_"))
Let sNames = sNames + sBase + "\7"
Let sValue = PathCombine(sDir, sName)
Let sValues = sValues + sValue + "\7"
Let i = i + 1
EndWhile
Let sNames = StringChopRight(sNames, 1)
Let sValues = StringChopRight(sValues, 1)

Let sBase = IniReadSetting("WebClientUtilities", "")
If StringLength(sBase) Then 
Let iIndex = StringSegmentIndex(sNames, "\7", sBase)
EndIf
If iIndex == 0 Then
Let iIndex = 1
EndIf

Let sTitle = "Web Client Utilities"
Let bSort = False
Let sName = DialogPickWithIndex(sTitle, sNames, bSort, iIndex)
If Not sName Then
Return
EndIf

IniWriteSetting("WebClientUtilities", sName)
Let iIndex = StringSegmentIndex(sNames, "\7", sName)
Let sFile = StringSegment(sValues, "\7", iIndex)

Let sExe = PathCombine(sDir, "InPy.exe")
Let sExe = PathGetShort(sExe)
Let sInputFile = PathCombine(sDir, GetActiveConfiguration() + ".ini")
Let sBase = PathGetBase(sFile)
Let sOutputFile = PathCombine(sDir, sBase + ".txt")

if 1 then
Let sCodeFile = sFile
Else
Let sWebClientFile = PathCombine(sDir, "WebClient.py")
Let sBody = StringTrimWhiteSpace(FileToString(sWebClientFile)) + "\r\n\r\n" + StringTrimWhiteSpace(FileToString(sFile)) + "\r\n"
Let sCodeFile = PathGetTempFile()
StringToFile(sBody, sCodeFile)
EndIf

Let sCommand = sExe + " " + StringQuote(sCodeFile) + " " + StringQuote(sInputFile) + " " + StringQuote(sOutputFile)
FileDelete(sOutputFile)
ShellRun(sCommand, 1, True)
; FileDelete(sCodeFile)
If FileExists(sOutputFile) Then
ShellRun(StringQuote(sOutputFile), 1, False)
EndIf
EndScript

