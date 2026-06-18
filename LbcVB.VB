'Layout by Code for .NET development (Visual Basic library)
'Version 2.9
'November 29, 2007
'Copyright 2006 - 2007 by Jamal Mazrui
'Modified GPL License

Imports LayoutByCode
imports Microsoft.VisualBasic
Imports System.Security.Cryptography
Imports System.IO
Imports System.Collections.ObjectModel
Imports System.Collections.Generic
Imports System.Media

Public Module LbcVB

Public function YieldInOperatingSystem() As String
Dim s As String
s = ""
s = s + String.Format("Full Name = {0}", My.Computer.Info.OSFUllName) + vbNewLine
s = s + String.Format("Version = {0}", My.Computer.Info.OSVersion) + vbNewLine
s = s + String.Format("User Interface Culture = {0}", My.Computer.Info.InstalledUICulture) + vbNewLine
s = s + String.Format("Platform = {0}", My.Computer.Info.OSPlatform) + vbNewLine
s = s + String.Format("Total physical memory = {0}", Lbc.FormatBytes(My.Computer.Info.TotalPhysicalMemory)) + vbNewLine
s = s + String.Format("Available physical memory = {0}", Lbc.FormatBytes(My.Computer.Info.AvailablePhysicalMemory)) + vbNewLine
s = s + String.Format("Total virtual memory = {0}", Lbc.FormatBytes(My.Computer.Info.TotalVirtualMemory)) + vbNewLine
s = s + String.Format("Available virtual memory = {0}", Lbc.FormatBytes(My.Computer.Info.AvailableVirtualMemory)) + vbNewLine
return s
End Function

Public Sub UploadFile(sFile As String, sURL As String, sUserName As String, sPassWord As String)
Dim bShowUI As Boolean = True
'Dim iTimeOut as Integer = new FileInfo(sFile).Length + 1000
Dim iTimeOut as Integer = 100000
Dim CancelOption As Microsoft.VisualBasic.FileIO.UICancelOption = Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException
'Dim CancelOption As Microsoft.VisualBasic.FileIO.UICancelOption = Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing
My.Computer.Network.UploadFile(sFile, sURL, sUserName, sPassword, bShowUI, iTimeOut, CancelOption)
'My.Computer.Network.UploadFile(sFile, sURL, sUserName, sPassword)
End Sub ' UploadFile method

Public Sub DownloadFile(sURL As String, sFile As String, sUserName As String, sPassWord As String)
Dim bShowUI As Boolean = True
Dim iTimeOut as Integer = 100000
Dim bReplace As Boolean = True
'Dim CancelOption As Microsoft.VisualBasic.FileIO.UICancelOption = Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing
Dim CancelOption As Microsoft.VisualBasic.FileIO.UICancelOption = Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException
'My.Computer.Network.DownloadFile(sURL, sFile, sUserName, sPassword)
My.Computer.Network.DownloadFile(sURL, sFile, sUserName, sPassword, bShowUI, iTimeOut, bReplace, CancelOption)
End Sub ' DownloadFile method

Public Sub DeleteDirectory(sDir As String)
My.Computer.FileSystem.DeleteDirectory(sDir, True)
End Sub

Public Sub CopyDirectory(sSource As String, sTarget As String)
My.Computer.FileSystem.CopyDirectory(sSource, sTarget, True)
End Sub

Public Sub MoveDirectory(sSource As String, sTarget As String)
My.Computer.FileSystem.MoveDirectory(sSource, sTarget, True)
End Sub

'Public Function GetFiles(sDir As String, sText As String, sSpec As String) As String
Public Function GetFiles(sDir As String, sSpec As String) As ReadOnlyCollection (Of String)
Dim list As ReadOnlyCollection (Of String)
list = My.Computer.FileSystem.GetFiles(sDir, (FileIO.SearchOption.SearchTopLevelOnly Or FileIO.SearchOption.SearchAllSubDirectories), sSpec)
Return list
End Function

Public Function FindInFiles(sDir As String, sText As String, sSpec As String) As ReadOnlyCollection (Of String)
Dim sFileList as String
Dim bIgnoreCase as Boolean
Dim list As ReadOnlyCollection (Of String)
bIgnoreCase = True
list = My.Computer.FileSystem.FindInFiles(sDir, sText, bIgnoreCase, (FileIO.SearchOption.SearchTopLevelOnly Or FileIO.SearchOption.SearchAllSubDirectories), sSpec)
sFileList = ""
For Each name As String In list
sFileList = sFileList + Name + "\n"
Next

Return list
End Function

Public sub PlaySystemSound(sound As SystemSound)
My.Computer.Audio.PlaySystemSound(sound)
End Sub

Public sub PlayWav(sWav As String)
'My.Computer.Audio.Play(sWav, AudioPlayMode.BackgroundLoop)
My.Computer.Audio.Play(sWav, AudioPlayMode.WaitToComplete)
End Sub

Public sub StopWav()
My.Computer.Audio.Stop()
End Sub

Public Sub Encrypt2File( sText As String, sFile As String)
Dim DataStream As MemoryStream = New MemoryStream( )
Dim Writer As New StreamWriter(DataStream)
Writer.Write(sText)
Writer.Close( )

Dim Data() As Byte = DataStream.ToArray( )
Dim EncodedData() As Byte = ProtectedData.Protect(Data, Nothing, DataProtectionScope.CurrentUser)
My.Computer.FileSystem.WriteAllBytes(sFile, EncodedData, False)
End Sub

Public Function File2Decrypt(sFile As String) As String
Dim EncodedData() As Byte = My.Computer.FileSystem.ReadAllBytes(sFile)
Dim Data() As Byte = ProtectedData.Unprotect(EncodedData, Nothing, DataProtectionScope.CurrentUser)
Dim DataStream As MemoryStream = New MemoryStream(Data)
Dim Reader As New StreamReader(DataStream)
Dim sText As String = Reader.ReadToEnd()
Reader.Close( )
Return sText
End Function

Public Function GetDirectory(sDrive As String) As String
Return FileSystem.CurDir(sDrive)
End Function

Public Sub ChangeDrive(sDrive As String)
FileSystem.ChDrive(sDrive)
End Sub

Public Sub ChangeDirectory(sDir As String) 
FileSystem.ChDir(sDir)
End Sub

Public Sub SetDirectory(sDir As String) 
ChangeDrive(sDir)
ChangeDirectory(sDir)
End Sub

Sub SendMail(sUserName As String, sPassword As String, sSenderAddress As String, sOutgoingServer As String, sSubject As String, sText As String, sRecipient As String)
Dim cdoSendUsingPickup As Integer = 1 'Send message using the local SMTP service pickup directory.
Dim cdoSendUsingPort As Integer = 2 'Send the message using the network (SMTP over the network).
Dim cdoAnonymous As Integer = 0 'Do not authenticate
Dim cdoBasic As Integer = 1 'basic (clear-text) authentication
Dim cdoNTLM As Integer = 2 'NTLM

Dim oMessage As Object
oMessage = CreateObject("CDO.Message")
'oMessage.Subject = "VBNET Example CDO Message"
oMessage.Subject = sSubject
'oMessage.From = """Jamal Mazrui"" <jamal.mazrui@verizon.net>"
oMessage.From = sSenderAddress
'oMessage.To = "jamal@empowermentzone.com"
oMessage.To = sRecipient
'oMessage.TextBody = "This is some sample message text.." & vbCRLF & "It was sent using SMTP authentication."
oMessage.TextBody = sText

'==This section provides the configuration information for the remote SMTP server.

oMessage.Configuration.Fields.Item _
("http://schemas.microsoft.com/cdo/configuration/sendusing") = 2

'Name or IP of Remote SMTP Server
oMessage.Configuration.Fields.Item _
("http://schemas.microsoft.com/cdo/configuration/smtpserver") = sOutgoingServer '"outgoing.verizon.net"

'Type of authentication, NONE, Basic (Base64 encoded), NTLM
oMessage.Configuration.Fields.Item _
("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate") = cdoBasic

'Your UserID on the SMTP server
oMessage.Configuration.Fields.Item _
("http://schemas.microsoft.com/cdo/configuration/sendusername") = sUserName '"jamal.mazrui"

'Your password on the SMTP server
oMessage.Configuration.Fields.Item _
("http://schemas.microsoft.com/cdo/configuration/sendpassword") = sPassword '"jambo6"

'Server port (typically 25)
oMessage.Configuration.Fields.Item _
("http://schemas.microsoft.com/cdo/configuration/smtpserverport") = 25

'Use SSL for the connection (False or True)
oMessage.Configuration.Fields.Item _
("http://schemas.microsoft.com/cdo/configuration/smtpusessl") = False

'Connection Timeout in seconds (the maximum time CDO will try to establish a connection to the SMTP server)
oMessage.Configuration.Fields.Item _
("http://schemas.microsoft.com/cdo/configuration/smtpconnectiontimeout") = 60

oMessage.Configuration.Fields.Update

'==End remote SMTP server configuration section==

oMessage.Send
End Sub

Function GetLinks(sUrl As String) As List (Of String())

Dim listLinks As List (Of String()) = new List (of String())
Dim listRefs As List (Of String) = new List (of String)
Dim oIE, oDoc, oLinks, oLink As Object

oIE = Interaction.CreateObject("InternetExplorer.Application")
oIE.Navigate(sUrl)
While oIE.ReadyState <> 4
System.Threading.Thread.Sleep(100)
End While

oDoc = oIE.Document
oLinks = oDoc.Links

For Each oLink In oLinks
Dim sRef As String = oLink.HRef
'if sRef.IndexOf("@") >= 0 Then Continue For
if sRef.ToLower().StartsWith("mailto:") Then Continue For
Try 
Dim oUri As Uri = new Uri(sRef)
Catch
sRef = ""
End Try

if sRef.Length = 0 Or listRefs.Contains(sRef) Then Continue For
listRefs.Add(sRef)
Dim aLink() As String = {sRef, oLink.InnerText}
listLinks.Add(aLink)
Next

oDoc.Close()
oIE.Quit()
System.Runtime.InteropServices.Marshal.ReleaseComObject(oIE)
oIE = Nothing

return listLinks
End Function

End Module
