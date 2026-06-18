'WebGet 1.2
'May 10, 2007
'Copyright 2006-2007 by Jamal Mazrui
'Modified GPL license

#Include "win32api.inc"
#Include "c:\pdf2txt\EOIni.inc"
#Include "c:\FileDir\MyLib.inc"

Global iExtraSpeech As Long
Global hwin&
Global oSAPI As Dispatch
%vtxtst_COMMAND                                         = &H00000004
Global s_UrlList As String, s_appPath As String

Declare Function JWESayString(s As String, Optional i As Long) As Long
Declare Function SayString(s As String, Optional i As Long) As Long
Declare Function JFWSayString Lib "JFWAPI.DLL" Alias "JFWSayString" _
(ByVal S_text As String, ByVal i_interrupt As Long) As Long

Function SayString(s As String, Optional i As Long) As Long
If IsFalse iExtraSpeech Then Exit Function
JWESayString(s, i)
End Function

Function JWESayString(s As String, Optional i As Long) As Long
'If GetForegroundWindow() <> FindWindow("", "TextPal") Then Exit Function
If IsFalse JFWSayString(s, 0) Then
Local o As Dispatch, v As Variant
If Len(FindWindowName("Window-Eyes")) > 0 Then
Set o = New Dispatch In "GWSPEAK.Speak"
If IsObject(o) Then
v = s
Object Call o.SpeakString(v)
Set o = Nothing
End If
Else
'Shell($DQ + s_AppPath + "speak.exe" + $DQ + " " + s)
'If %False Then
If IsObject(oSAPI) Then
Local v1 As Variant, v2 As Variant
v1 = s
v2 = %vtxtst_COMMAND
'Object Call oSAPI.Speak(v1, v2)
Object Call OSAPI.Speak(v1)
Else
'MsgBox "No SAPI"
End If
End If
End If
End Function

Declare Function ReplaceAll(sBody As String, sMatch As String, sReplace As String) As String

Function GetFileFromURL(s_url As String) As String
Local i As Long
Static iName As Long
Local s As String, s_file As String, s_ext As String

s_file = Path_FindFileName(s_url)
s_ext = LCase$(Path_FindExtension(s_url))
'if InStr(-1, s_url, "/") > InStr(-1, s_url, s_ext) Then s_file = ""
If InStr(-1, s_url, "/") > InStr(-1, s_url, s_ext) Then s_file = s_file + ".htm"
'If s_ext = ".asp" Or s_ext = ".aspx" Or s_ext = ".cfm" Or s_ext = "php" Or s_ext = ".cgi" Then s_file = s_file + ".htm"
If Len(s_ext) = 0 Or InStr("|asp|aspx|cfm|cgi|php|com|biz|edu|info|gov|mil|", "|" + Mid$(s_ext, 2)) Then s_file = s_file + ".htm"
If Left$(s_ext, 5) = ".htm#" Then
i = InStr(-1, s_file, ".htm#")
s_file = Mid$(s_file, 1, i) + "htm"
End If
If Left$(s_ext, 6) = ".html#" Then
i = InStr(-1, s_file, ".html#")
s_file = Mid$(s_file, 1, i) + "html"
End If

Replace "%20" With " " In s_file
Replace "/" With "_" In s_file
Replace "=" With "_" In s_file
Replace "?" With "_" In s_file
s = ""
While s_file <> s
s = s_file
s_file = ReplaceAll(s_file, "__", "_")
s_file = ReplaceAll(s_file, "..", ".")
s_file = ReplaceAll(s_file, " .", ".")
s_file = ReplaceAll(s_file, ". ", ".")
s_file = ReplaceAll(s_file, " _", "_")
s_file = ReplaceAll(s_file, "_ ", "_")
s_file = ReplaceAll(s_file, "_.", ".")
s_file = ReplaceAll(s_file, "._", ".")
Wend
If Len(s_file) > 0 And Left$(s_file, 1) = "#" Then s_file = Mid$(s_file, 2)
If Len(s_file) > 0 And IsFalse InStr(s_file, ".") Then s_File = s_file + ".htm"
If s_file = "" Then
iName = iName + 1
s_file = "NoName" + Format$(iName) + ".htm"
'MsgBox s_url
End If
Function = s_file
End Function

Function ReplaceAll(sBody As String, sMatch As String, sReplace As String) As String
While IsTrue InStr(sBody, sMatch)
Replace sMatch With sReplace In SBody
Wend
Function = sBody
End Function

Function GetShellWindowURLS() As String
Local oShell As Dispatch, oWindows As Dispatch, oWindow As Dispatch
Local i As Long, iCount As Long
Local sURL As String, sURLS As String
Local v As Variant, V1 As Variant

Set oShell = New Dispatch In "Shell.Application"
If IsObject(oShell) Then
Object Call oShell.Windows To V
Set oWindows = v
If IsObject(oWindows) Then
Object Get oWindows.Count To v
iCount = Variant#(v)

'For i = 0 To iCount - 1
While i < iCount
    v1 = i
Object Get oWindows.Items(v1) To v
Set oWindow = v
'msgbox format$(i)
'If IsObject(oWindow) Then
if 1 then
    MsgBox "2"
Object Get oWindow.LocationURL To V
sURL = Variant$(v)
sURLS = sURLS + sURL + $CrLf
End If
Set oWindow = Nothing
'Next
i = i + 1
 WEnd

End If
Set oWindows = Nothing
End If
Set oShell = Nothing
Function = sURLs
End Function

Function GrabTitle(s_url As String) As String
Local i As Long
Local s_title As String
Local v As Variant, v1 As Variant, v2 As Variant
Local o_app As Dispatch, o_doc As Dispatch, o_links As Dispatch, o_link As Dispatch

Set o_app = New Dispatch In "InternetExplorer.Application"
If IsObject(o_app) Then
v = %FALSE
Object Let o_app.Visible = v
v = %True
Object Let o_app.Silent = v
v = s_url
Object Call o_app.navigate(v)
i = %TRUE
While i
Object Get o_app.Busy To v
i = Variant#(v)
Sleep 1
'i = 0
Wend
Object Get o_app.document To V
Set o_doc = v
If IsObject(o_doc) Then
Object Get o_doc.Title To V
s_title = Variant$(v)
Function = Trim$(s_title)
End If
End If
End Function

Function PDFListFromWeb(ByRef s_source As String) As Long
Local s_url As String, s_urlList As String, s_innerText As String
Local i As Long, i_link As Long, i_linkCount As Long
Local v As Variant, v1 As Variant, v2 As Variant
Local o_app As Dispatch, o_doc As Dispatch, o_links As Dispatch, o_link As Dispatch

If Path_IsUrl(s_source) And Equiv(Path_FindExtension(s_source), ".pdf") Then
i_LinkCount = 1
ReDim a$(i_linkCount + 1)
a$(0) = Path_FindPath(s_source)
a$(1) = s_source
Function = 1
Exit Function
End If
s_urlList = ""
Set o_app = New Dispatch In "InternetExplorer.Application"
If IsObject(o_app) Then
v = %FALSE
Object Let o_app.Visible = v
Object Let o_app.Silent = v
v = s_source
Object Call o_app.navigate(v)
i = %TRUE
While i
Object Get o_app.Busy To v
i = Variant#(v)
Sleep 1
Wend
Object Get o_app.document To V
Set o_doc = v
If IsObject(o_doc) Then
Object Get o_doc.Links To V
Set o_links = v
If IsObject(o_links) Then
Object Get o_links.Length To v
i_linkCount = Variant#(v)
ReDim a$(i_linkCount + 1)
i& = 0
a$(0) = s_source
For i_link = 0 To i_linkCount - 1
v1 = i_link
Object Call o_links.item(v1) To v
Set o_link = v
If IsObject(o_link) Then
Object Get o_link.InnerText To V
s_innerText = Variant$(v)
Object Get o_link.Href To V
s_url = Variant$(v)
If %True Then
'If Equiv(Path_FindExtension(s_url), ".pdf") Then
s$ = Trim$(s_innerText)
'If Equiv(s$, "acrobat") Or Equiv(s$, "pdf") Or Equiv(s$, "adobe pdf") Or Equiv(s$, "acrobat pdf") Then s$ = ""
If Len(s$) > 0 Then s$ = s$ + $Tab
If InStr(s_url, "@") Then Iterate
s$ = s$ + s_url + $Tab + GetFileFromURL(s_url)
s_UrlList = s_urlList + s$ + $CrLf
's_UrlList = s_urlList + s_url + $CrLf
i& = i& + 1
a$(i&) = s_url
End If
End If
Set o_link = Nothing
Next
s_urlList = Trim$(s_urlList, Any $CrLf)
Set o_links = Nothing
End If
Set o_doc = Nothing
End If
Object Call o_app.Quit()
Set o_app = Nothing
End If
s_source = s_urlList
If Len(s_urlList) = 0 Then
Function = 0
Else
Function = ParseCount(s_UrlList, $CrLf)
End If
End Function

Function NoURL2File(ByVal z_URL As Asciiz * %MAX_PATH, ByVal z_file As Asciiz * %MAX_PATH) As Long
Function = IsFalse URLDownloadToFile(ByVal 0&, z_url, z_file, ByVal 0&, ByVal 0&)
End Function

Function NoFindWindowName(ByVal sFindThis As String) As String
   Local iCounter    As Long
   Local sEachName   As String
   gsEnumWNResults = ""
   If ENUMWINDOWS(CodePtr(EnumMainWindows), 0) Then            'retrieve top level window names
      For iCounter = 1 To ParseCount(gsEnumWNResults, $CrLf)   'loop through all top level window names
         sEachName = Parse$(gsEnumWNResults, $CrLf, iCounter)  'parse window name from our buffer string
         If InStr(UCase$(sEachName), UCase$(sFindThis)) Then   'search for any case match
            Function = sEachName                               'found correct window so return that name
            Exit For
         End If
      Next iCounter
   End If
End Function

Function ChildCallBack (ByVal hWndChild As Long, lRaram As Long) As Long
Dim sTempStr            As Asciiz * 255
Dim sListText           As String
Dim lResult             As Long
Dim der As Long
der = GetClassName(hWndChild, sTempStr, 255)
If LCase$(Left$(sTempStr,4)) = "edit" Then
hWin& = hWndChild
Function = 0
Exit Function
End If
Function = 1
End Function

Function GrabURL() As String
Local s_url As String
Local h_ie As Long, h_address As Long

%ADDRESS_BAR4=41477
                  h_ie = FindWindow("", FindWindowName("Microsoft Internet Explorer"))
'msgbox format$(h_ie)
EnumChildWindows(h_ie, CodePtr(ChildCallBack), 0&)
'msgbox format$(hwin&)
h_address = hwin&
Local i_length As Long
Local i_return As Long
Local z As Asciiz * 255
i_length = SendMessage(h_address, %WM_GETTEXTLENGTH, ByVal CLng(0), ByVal CLng(0)) + 1
'msgbox format$(i_length)
z = String$(i_length,0)
i_return = SendMessage(h_address, %WM_GETTEXT, ByVal i_length, ByVal VarPtr(z))
'msgbox format$(i_return)
Function = Trim$(z)
End Function

Function PBMain()
'MsgBox GetShellWindowURLS()
Local i As Long, iCount As Long, i_url As Long, i_urlCount As Long, iName As Long
Local s As String, s_command As String, s_page As String, s_DlDir As String, s_TempFile As String, s_file As String, s_Url As String, s_ext As String

If %True Then
'If %False Then
Set oSAPI = New Dispatch In "SAPI.SPVoice"
Local v1 As Variant, v2 As Variant
v1 = ""
v2 = "WebGet"
'Object Call oSAPI.Register(v1, v2)
End If

s_AppPath = Path_FindPath(AppExeName())
ChDrive Left$(s_appPath, 1)
ChDir Left$(s_appPath, Len(s_appPath) - 1)
iExtraSpeech = Val(Ini_GetKey(s_appPath + "TextPal.ini", "Internal", "ExtraSpeech", "1"))

s_DlDir = s_AppPath + "download"
s_tempFile = s_AppPath + "WebGet.tmp"
s_command = Trim$(Command$)
If Path_IsUrl(s_command) Then
'SayString("Getting list of URLs")
s_UrlList = s_command
i_UrlCount = PdfListFromWeb(s_UrlList)
'SayString("Count " + Format$(i_urlCount))
If Path_FileExists(s_tempFile) Then Kill s_tempFile
StringToFile(s_UrlList, s_tempFile)
ElseIf Path_FileExists(s_command) Then
SayString("Downloading")
s_UrlList = FileToString(s_command)
'MsgBox s_command
If IsFalse Path_FileExists(s_DlDir) Then MkDir s_DlDir
i_urlCount = ParseCount(s_UrlList, $CrLf)
'SayString("Downloading " + Format$(i_urlCount))
For i_url = 1 To i_UrlCount
s_url = Parse$(s_UrlList, $CrLf, i_url)
'Msgbox s_url
If IsFalse PathIsURL(ByCopy s_url) Then Iterate
'If IsFalse InStr(s_url, "://") Then Iterate
s_file = GetFileFromURL(s_url)
If Path_FileExists(s_file) Then Kill s_file
s_file = s_DlDir + "\" + s_file
Url2File(s_url, s_file)
s = Path_FindFilename(s_file)
If Path_FileExists(s_file) Then
sayString(s)
iCount = iCount + 1
Else
SayString("Cannot download " + s)
'MsgBox s_url
'MsgBox s
End If
Next
SayString("Downloaded " + Format$(iCount))
Else
'SayString("Getting URL of current IE page")
s_page = GrabURL()
If s_page = "" Then s_page = GrabURL()
's_page = InputBox$("Page:", "Get URls", s_page)
'MsgBox s_page
'msgbox format$(len(s_page))
If s_page <> "" And Path_FileExists(s_tempFile) Then
Local s_title As String
s_title = GrabTitle(s_page)
If s_title = "" Then s_title = GrabTitle(s_page)
s_page = s_page + $CrLf + s_title
End If
'msgbox s_page
If s_page = "" Then s_page = " "
'msgbox format$(len(s_page))
If Path_FileExists(s_tempFile) Then Kill s_tempFile
StringToFile(s_page, s_tempFile)
End If
Set oSAPI = Nothing
End Function
