#Compile Exe

#Register None

#Dim All

#Include "Win32Api.Inc"

'    %HKEY_CLASSES_ROOT = &H80000000
%REG_SZ = 1
$Extention = ".zip"
$Title = "acroexch.document"
$Path = "FileDir.exe"
$Cmt = "Portable Document Format"

Function AppPath() As String
  Local hModule As Long
  Local buffer  As Asciiz * 256
  Local I As Long
  hModule = GetModuleHandle(ByVal 0&)
  GetModuleFileName hModule, Buffer, 256
  Local xStr$
  xStr$ = Trim$(buffer)
  For I = 1 To Len(xStr$)
   If Mid$(xStr$, Len(xStr$) - I, 1) = "/" Or Mid$(xStr$, Len(xStr$) - I, 1) = "\" Then
      Function = Left$(xStr$, Len(xStr$) - I)
      Exit Function
   End If
  Next I
  Function = xStr$
End Function

Function PBMain
Dim KeyName   As Asciiz * 256
Dim KeyValue  As Asciiz * 256
Dim KeyHandle As Long

KeyName = $Extention: KeyValue = $Title

KeyName = "Folder\shell": KeyValue = ""

RegCreateKey %HKEY_CLASSES_ROOT, KeyName, KeyHandle

RegSetValue KeyHandle, "", %REG_SZ, KeyValue, 0&
RegCloseKey KeyHandle


RegCreateKey %HKEY_CLASSES_ROOT, KeyName, KeyHandle

RegSetValue KeyHandle, "", %REG_SZ, KeyValue, 0&

RegCloseKey KeyHandle

If %FALSE Then
KeyName = $Title: KeyValue = $Cmt

RegCreateKey %HKEY_CLASSES_ROOT, KeyName, KeyHandle

RegSetValue KeyHandle, "", %REG_SZ, KeyValue, 0&

RegCloseKey KeyHandle

KeyName = $Title: KeyValue = Chr$(34) + AppPath() + $Path + Chr$(34) + " %1"
'KeyValue = command$ + " %1"
'KeyValue = Chr$(34) + Command$ + Chr$(34) + " %1"
RegCreateKey %HKEY_CLASSES_ROOT, KeyName, KeyHandle

RegSetValue KeyHandle, "shell\open\command", %REG_SZ, KeyValue, %MAX_PATH

RegCloseKey KeyHandle
End If

If Command$ <> "/silent" Then MsgBox ("Folders and .zip extension are no longer associated with FileDir"), (%MB_TASKMODAL Or %MB_ICONINFORMATION Or %MB_OK), "Status"
End Function
