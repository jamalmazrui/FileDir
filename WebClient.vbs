Dim wshShell : Set wshShell = WScript.CreateObject("WScript.Shell")
Dim sCurDir : sCurDir = wshShell.CurrentDirectory


DIM oWebClient ' as object
DIM sURL ' as string
DIM sFileSpec ' as string

 Set oWebClient = CreateObject("System.Net.WebClient")

 sURL = "http://www.bcxgurus.com/bcxlogo.jpg"

 sFileSpec = sCurDir & "\bcxlogo.jpg"

 oWebClient.DownloadFile sURL, sFileSpec

MsgBox "Your File is here: " & sFileSpec, vbInformation, "vbs Script"

Set oWebClient = nothing
Set wshShell = nothing
WScript.Quit
