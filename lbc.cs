//Layout by Code for .NET development
// May 11, 2011
//Copyright 2006-2009 by Jamal Mazrui
//Modified GPL License

using FileDir;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Compatibility.VB6;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Windows.Forms;

[assembly:PermissionSetAttribute(SecurityAction.RequestMinimum, Name = "FullTrust")]
namespace LayoutByCode {
public class Lbc {
public static string SB = "\r\n----------\r\n\f\r\n";
public static string EOD = "\r\n----------\r\nEnd of Document\r\n";
public static object JAWS = null;


public Lbc() {
//Lbc.Say("Please wait");
// InitJFW();
// does not get called
// Lbc.Show("Path", Environment.GetEnvironmentVariable("PATH").Split('\n'));
}

public static string GetBomStringFromBytes(byte[] aBom) {
string sReturn = "";
foreach (byte b in aBom) {
if (sReturn.Length > 0) sReturn += "|";
sReturn += b;
}
return sReturn;
} // GetBomStringFromBytes method

public static string GetBomStringFromFile(string sFile) {
FileStream file = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
byte[] aBom = new byte[4];
int iCount = file.Read(aBom, 0, 4);
file.Close();
byte[] aReturn = new byte[iCount];
for (int i = 0; i < iCount; i++) aReturn[i] = aBom[i];
return GetBomStringFromBytes(aReturn);
} // GetBom method

public static Dictionary<string, int> GetBomDictionary() {
Dictionary<string, int> dCodes = new Dictionary<string, int>();
Dictionary<string, int> dBoms = new Dictionary<string, int>();
dCodes.Add("Unicode (Big-Endian)", 1201);
dCodes.Add("Unicode (UTF-32 Big-Endian)", 12001);
dCodes.Add("Unicode (UTF-32)", 12000);
// dCodes.Add("Unicode (UTF-7)", 65000);
dCodes.Add("Unicode (UTF-8)", 65001);
dCodes.Add("Unicode", 1200);

string sBody = "";
foreach (string sKey in dCodes.Keys) {
int iValue = dCodes[sKey];
Encoding en = Encoding.GetEncoding(iValue);
string sFile = Path.GetTempFileName();
File.WriteAllText(sFile, sBody, en);

string sBom = GetBomStringFromFile(sFile);
// MessageBox.Show(en.EncodingName, sBom);
// if (dBoms.ContainsKey(sBom)) MessageBox.Show(en.EncodingName, Encoding.GetEncoding(dBoms[sBom]).EncodingName);
dBoms.Add(sBom, iValue);
File.Delete(sFile);
}
return dBoms;
} // GetBomDictionary method

public static Encoding GetFileEncoding(string sFile) {
Dictionary<string, int> dBom = GetBomDictionary();
return GetFileEncoding(sFile, dBom);
} // GetFileEncoding method

public static Encoding GetFileEncoding(string sFile, Dictionary<string, int> dBom) {
// foreach (string s in dBom.Keys) Console.WriteLine(s);

string sBom = GetBomStringFromFile(sFile);
Encoding en = Encoding.Default;
// if (dBom.ContainsKey(sBom)) en = Encoding.GetEncoding(dBom[sBom]);
foreach (string s in dBom.Keys) {
if (sBom.StartsWith(s)) {
en = Encoding.GetEncoding(dBom[s]);
break;
}
}
return en;
} // GetFileEncoding method

public static string Encrypt(string sSource) {
byte[] aSource = {};
Array.Resize(ref aSource, sSource.Length);
for (int i = 0; i < sSource.Length; i++) aSource[i] = Convert.ToByte(sSource[i]);
byte[] aTarget = ProtectedData.Protect(aSource, null, DataProtectionScope.CurrentUser);
char[] aChar = {};
Array.Resize(ref aChar, aTarget.Length);
for (int i = 0; i < aTarget.Length; i++) aChar[i] = Convert.ToChar(aTarget[i]);
StringBuilder sb = new StringBuilder();
//foreach (char c in aChar) sb.Append(c);
//foreach (char c in aChar) sb.Append(Convert.ToString(c));
//string sTarget = sb.ToString();
string sTarget = "";
sTarget = new ASCIIEncoding().GetString(aTarget, 0, aTarget.Length);
Lbc.Show(sTarget.Length);
return sTarget;
} // Protect method

public static void Show(object oText) {
Lbc.Show(oText, "Show");
}

public static void Show(object oText, object oTitle) {
MessageBox.Show(oText.ToString(), oTitle.ToString());
}

[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
public static extern int GetShortPathName(string path, StringBuilder shortPath, int shortPathLength);
public static string GetShortPath(string LFN) {
StringBuilder shortPath = new StringBuilder(255);
GetShortPathName(LFN, shortPath, shortPath.Capacity);
return shortPath.ToString();
}

public static string GetLfn(string sPath) {
object oShell = CreateObject("WScript.Shell");
object oShortcut = CallMethod(oShell, "CreateShortcut", "temp.lnk");
SetProperty(oShortcut, "TargetPath", sPath);
string sReturn = (string) GetProperty(oShortcut, "TargetPath");
return sReturn;
} // GetLfn method

[DllImport("User32.dll")]
public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

[DllImport("User32.dll")]
public static extern int GetForegroundWindow();

[DllImport("User32.dll")]
public static extern int SetForegroundWindow(int iHandle);

[ DllImport("user32.dll") ]
static extern int GetWindowText(int iHandle, StringBuilder sBuffer, int iCount);

public static string GetAppTitle() {
int iHandle = GetForegroundWindow();
const int iCount = 256;
StringBuilder sBuffer = new StringBuilder(iCount);

string sTitle = "";
if ( GetWindowText(iHandle, sBuffer, iCount) > 0 ) sTitle = sBuffer.ToString();
return sTitle;
}

public static int SetCaretIndex(ListBox lst, int i) {
return SendMessage(lst.Handle, 414, i, 0);
}

public static bool InitJFW() {
string sDir = GetJFWPath();
if (sDir.Length == 0) return false;

string sPath = Environment.GetEnvironmentVariable("PATH");
sDir += ";";
if (!sPath.ToLower().Contains(sDir.ToLower())) {
sPath = sDir + sPath;
Environment.SetEnvironmentVariable("PATH", sPath);
}
return true;
} // InitJFW method

public static string GetJFWPath() {
RegistryKey key = Registry.LocalMachine;
string sSubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";
string sName = "Path";
string sPath = GetRegString(key, (sSubKey + "jfw.exe"), sName);

if (sPath == "") {
string[] sVersionList = {"12", "11", "10", "90", "81", "80", "8", "71", "70", "7", "62", "61", "60", "6"};
sName = "";
foreach (string sVersion in sVersionList) {
sPath = GetRegString(key, (sSubKey + "jaws" + sVersion + ".exe"), sName);
if (sPath != "") {
sPath = Path.GetDirectoryName(sPath);
break;
}
}
}
if (sPath !="" && !sPath.EndsWith(@"\")) sPath = String.Concat(sPath, @"\");
return sPath;
}

public static string GetWEPath() {
RegistryKey key = Registry.LocalMachine;
string sSubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinEyes.exe";
string sName = "Path";
string sPath = GetRegString(key, sSubKey, sName);
if (sPath !="" && !sPath.EndsWith(@"\")) sPath = String.Concat(sPath, @"\");
return sPath;
}

public static string FormatBytes(long lBytes) {
//float f;
double f = 0;
string s = "";
if (lBytes < 1024d){
f = lBytes;
s = "";
}
else if (lBytes < Math.Pow(1024d, 2)) {
f = lBytes / 1024d;
s = "K";
}
else if (lBytes < Math.Pow(1024d, 3)) {
f = lBytes / Math.Pow(1024d, 2);
s = "M";
}
else if (lBytes < Math.Pow(1024d, 4)) {
f = lBytes / Math.Pow(1024d, 3);
s = "G";
}
else if (lBytes < Math.Pow(1024d, 5)) {
f = lBytes / Math.Pow(1024d, 4);
s = "T";
}

f = Math.Round(f, 1);
//string sResult = f.ToString() + s;
string sResult = f.ToString() + " " + s;
return sResult;
}

public static string GetRegString(RegistryKey key, string sSubKey, string sName) {
RegistryKey subkey = null;
string sData = "";

try {
subkey = key.OpenSubKey(sSubKey);
sData = subkey.GetValue(sName).ToString();
//sData = subkey.GetValue(sName, RegistryValueKind.String).ToString();
//subkey = key.OpenSubKey(sSubKey, RegistryKeyPermissionCheck.ReadSubTree);
//sData = subkey.GetValue(sName, RegistryValueKind.String).ToString();
}
//catch (Exception ex) {
catch {
//MessageBox.Show(ex.Message);
}
finally {
if (subkey != null) subkey.Close();
}
return sData;
}

[ DllImport("shell32.dll") ]
static extern int ShellExecuteA(int i1, string sVerb, string sFile, int i2, int i3, int i4);

public static int ShellExecute(string sVerb, string sFile) {
//return ShellExecuteA(0, sVerb, sFile, 0, 0, 0);
return ShellExecuteA(0, sVerb, sFile, 0, 0, 1);
} // ShellExecute method

[ DllImport("shell32.dll") ]
static extern int ShellExecute(int i1, int i2, string sFile, int i3, int i4, int i5);

public static int ShellDefault(string sFile) {
// sFile = Lbc.Quote(Lbc.Unquote(sFile));
return ShellExecute(0, 0, sFile, 0, 0, 1);
} // ShellDefault method

public static void Open(string sPath) {
int iResult = ShellExecute("Open", sPath);
if (iResult <= 32) Process.Start(sPath);
} // Open method

public static bool SayLines(object oText) {
string sText = oText.ToString();
//string[] aLines = sText.Split('\n');
string[] aLines = sText.Split('.');
foreach (string sLine in aLines) {
//if (!Lbc.Say(sLine)) return false;
Lbc.Say(sLine);
//Lbc.Say("here");
}
return true;
}

public static bool Say(object oText) {
return Say(oText, false);
} // Say method

public static bool IsAppActiveWindow() {
IntPtr h = (IntPtr) Lbc.GetForegroundWindow();
foreach (Form frm in Application.OpenForms) if (frm.Handle == h) return true;
return false;
} // IsAppActiveWindow method

public static bool Say(object oText, bool bGlobal) {
if (!App.ExtraSpeech) {
string s = File2String(App.sSpeechLog);
s += oText.ToString() + "\r\n";
String2File(s, App.sSpeechLog);
return true;
}

if (!bGlobal) {
//if (GetForegroundWindow() != (int) Process.GetCurrentProcess().MainWindowHandle) return false;
if (!IsAppActiveWindow()) return false;

Microsoft.VisualBasic.Devices.Keyboard keyboard = new Microsoft.VisualBasic.Devices.Keyboard();
//if (keyboard.AltKeyDown && keyboard.CtrlKeyDown) return false;
if (keyboard.ScrollLock ) return false;
}

bool bResult = false;
string sText = oText.ToString();

//bResult = JFWSayString(sText, false);
//bResult = JFWSayString(sText);
// bResult = JAWSSayString(sText);
bResult = COM_JAWSSay(sText, ref Lbc.JAWS);
if (!bResult) bResult = WESayString(sText);
if (!bResult) bResult = IsSAActive() && SASay(sText);
if (!bResult) bResult = IsNVDAActive() && NVDASay(sText);
if (!bResult) bResult = SAPISayString(sText);
return bResult;
}

public static bool JFWSayString(string sText) {
bool bResult = false;
if (FindWindowA(0, "JAWS") == 0) return bResult;
try {
// Type JFW = Type.GetTypeFromProgID("JFWApi");
// Console.Beep();
Type JFW = Type.GetTypeFromProgID("FreedomSci.Api");
object speech = Activator.CreateInstance(JFW);
//bResult = (bool) JFW.InvokeMember("SayString", BindingFlags.InvokeMethod, null, speech, inputArguments);
// object[] inputArguments = {sText, (int) 0};
// if (sText.Length <= 20000) JFW.InvokeMember("SayString", BindingFlags.InvokeMethod, null, speech, inputArguments);
// else {
object[] inputArguments = {sText, false};
bResult = (bool) JFW.InvokeMember("SayString", BindingFlags.InvokeMethod, null, speech, inputArguments);
if (bResult) return true;
else {
string sDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileDir");
string sTempFile = Path.Combine(sDataDir, "FileDir.tmp");

Lbc.String2File(sText, sTempFile);
JFW.InvokeMember("RunFunction", BindingFlags.InvokeMethod, null, speech, new object[] {"SayTempFile"});
}
bResult = true;
}
catch {
bResult = false;
}
return bResult;
}

[DllImport("jfwapi.dll")]
public static extern bool JFWSayString(string sText, bool iInterrupt);

[DllImport("jfwapi.dll")]
public static extern bool JFWRunFunction(string sFunction);

[DllImport("jfwapi.dll")]
public static extern bool JFWStopSpeech();

public static void StopSpeech() {
JFWStopSpeech();
} // StopSpeech method

public static bool JAWSSayString(string sText) {
if (FindWindowA(0, "JAWS") == 0) return false;
// if (sText.Length < 2000 && JFWSayString(sText, false)) return true;
if (JFWSayString(sText, false)) return true;

string sDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileDir");
string sTempFile = Path.Combine(sDataDir, "FileDir.tmp");
//Lbc.Show(sTempFile);
Lbc.String2File(sText, sTempFile);
return JFWRunFunction("SayTempFile");
} // JAWSSay method

public static bool COM_JAWSSay(string sText, ref object oJFW) {
try {
if (oJFW == null) oJFW = CreateObject("FreedomSci.JawsApi");
// int iResult = (int) CallMethod(oJFW, "SayString", new object[] {sText, 0});
// return iResult == 1;
// Console.Beep();
bool bResult = (bool) CallMethod(oJFW, "SayString", new object[] {sText, false});
return bResult;
}
catch {
return false;
}
} // COM_JAWSSay method

[DllImport("nvdaControllerClient32.dll", CharSet = CharSet.Unicode)]
public static extern int nvdaController_testIfRunning();

public static bool IsNVDAActive() {
return nvdaController_testIfRunning() == 0;
} // IsNVDAActive method

[DllImport("nvdaControllerClient32.dll", CharSet = CharSet.Unicode)]
public static extern int nvdaController_speakText(string sText);

public static bool NVDASay(string sText) {
return nvdaController_speakText(sText) == 0;
} // NVDASay method
[DllImport("nvdaControllerClient32.dll", CharSet = CharSet.Unicode)]
public static extern int nvdaController_brailleMessage(string sText);

public static bool NVDABraille(string sText) {
return nvdaController_brailleMessage(sText) == 0;
} // NVDASay method



[DllImport("saapi32.dll")]
public static extern int SA_IsRunning();
public static bool IsSAActive() {
try {
return SA_IsRunning() == 1;
}
catch {
return false;
}
} // IsSAActive method

[DllImport("saapi32.dll")]
public static extern int SA_SayU(string sText);
public static bool SASay(string sText) {
try {
return SA_SayU(sText) == 1;
}
catch {
return false;
}
} // SASay method

[ DllImport("user32.dll") ]
static extern int FindWindow(string sClass, string sTitle);

[ DllImport("user32.dll") ]
static extern int FindWindowA(int iClass, string sTitle);

public static bool WESayString(string sText) {
bool bResult = false;
//if (FindWindowA(0, "Window-Eyes") == 0) return bResult;
if (FindWindow("GWMExternalControl", "External Control") == 0) return bResult;

try {
Type Gw = Type.GetTypeFromProgID("GwSpeak.Speak");
object speech = Activator.CreateInstance(Gw);
object[] inputArguments = {sText};
Gw.InvokeMember("SpeakString", BindingFlags.InvokeMethod, null, speech, inputArguments);

/*
object oWE = Lbc.CreateObject("WindowEyes.Application");
object oSpeech = Lbc.GetProperty(oWE, "Speech");
Lbc.CallMethod(oSpeech, "Speak", new string[] {sText});
*/

bResult = true;
}
catch {
bResult = false;
}
return bResult;
}

public static bool SAPISayString(string sText) {
bool bResult = false;
try {
Type SAPI = Type.GetTypeFromProgID("SAPI.SPVoice");
object speech = Activator.CreateInstance(SAPI);
object[] inputArguments = {sText};
SAPI.InvokeMember("Speak", BindingFlags.InvokeMethod, null, speech, inputArguments);
bResult = true;
}
catch {
bResult = false;
}
return bResult;
}

public static string JoinList(string sDivider, ArrayList list) {
string sResult = "";

for (int i = 0; i < list.Count; i++) {
sResult += list[i].ToString();
if (i < list.Count - 1) sResult += sDivider;
}
return sResult;
}

public static string OpenFileDialog(string sTitle, string sDefaultFile, string sFilter, int iIndex) {
string sReturn = "";
OpenFileDialog dlg = new OpenFileDialog();
if (File.Exists(sDefaultFile)) {
dlg.FileName = sDefaultFile;
string sDefaultDir = Path.GetDirectoryName(sDefaultFile);
if (Directory.Exists(sDefaultDir)) dlg.InitialDirectory = sDefaultDir;
}

//dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
dlg.Filter = sFilter;
//dlg.FilterIndex = 2
//dlg.FilterIndex = iIndex;
dlg.ShowReadOnly = false;
dlg.ReadOnlyChecked = false;
//dlg.RestoreDirectory = false;
dlg.RestoreDirectory = true;
if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.FileName;
return sReturn;
}

public static string SaveFileDialog(string sTitle, string sDefaultFile, string sFilter, int iIndex, bool bConfirmReplace) {
string sReturn = "";
SaveFileDialog dlg = new SaveFileDialog();
dlg.AddExtension = true;
dlg.CheckFileExists = false;
dlg.CheckPathExists = true;
dlg.CreatePrompt = false;
dlg.OverwritePrompt = bConfirmReplace;
dlg.FileName = sDefaultFile;
string sDefaultDir = Directory.GetCurrentDirectory();
if (sDefaultFile != "") sDefaultDir = Path.GetDirectoryName(sDefaultFile);
dlg.InitialDirectory = sDefaultDir;
if (sFilter == "") sFilter = "All files (*.*)|*.*";
dlg.Filter = sFilter;
dlg.FilterIndex = iIndex;
//dlg.RestoreDirectory = false;
dlg.RestoreDirectory = true;
dlg.ValidateNames = true;
if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.FileName;
return sReturn;
}

public static bool Equiv(string s1, string s2) {
//return s1.Trim().ToLower() == s2.Trim().ToLower();
return String.Compare(s1, s2, true) == 0;
} // Equiv

public static string DirectoryDialog(string sTitle, string sLabel, string sValue) {
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabel + ":";
TextBox txt = new TextBox();
txt.Width *= 2;
txt.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
txt.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
txt.Text = sValue;
txt.AccessibleName = lbl.Text.Replace("&", "");
txt.GotFocus += delegate(object o, EventArgs e) {txt.SelectAll();};

Button btnBrowse = new Button();
btnBrowse.Click += delegate(object o, EventArgs e) { txt.Text = Lbc.FolderBrowseDialog("", sValue, false); txt.Select();};
btnBrowse.Text = "&Browse";
btnBrowse.AccessibleName = btnBrowse.Text.Replace("&", "");

flpInput.Controls.AddRange(new Control[] {lbl, txt, btnBrowse});
flpInput.ResumeLayout();

FlowLayoutPanel flpLists = new FlowLayoutPanel();
flpLists.SuspendLayout();
flpLists.Anchor = AnchorStyles.None;
flpLists.AutoSize = true;
flpLists.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpLists.FlowDirection = FlowDirection.LeftToRight;

Button btnCurrent = new Button();

btnCurrent.Click += delegate(object o, EventArgs e) {
Lbc.Say("Current", true);
string sDirs = "";
foreach (MdiChild child in App.frame.MdiChildren) {
if (Directory.Exists(child.Text)) sDirs += child.Text + "\n";
}
string[] aDirs = sDirs.Trim().Split('\n');
string[] aNames = new string[aDirs.Length];
for (int i = 0; i < aNames.Length; i++) aNames[i] = (aDirs[i].EndsWith(@":\") ? aDirs[i] : Path.GetFileName(aDirs[i]));
Array.Sort(aDirs, aNames);
string sName = ListDialog("Pick", "", aNames, true, 0);
if (sName.Length == 0) return;

int iName = Array.IndexOf(aNames, sName);
sResult = aDirs[iName];
frm.Close();
};

btnCurrent.Text = "&Current";
btnCurrent.AccessibleName = btnCurrent.Text.Replace("&", "");

Button btnRecent = new Button();
btnRecent.Click += delegate(object o, EventArgs e) {
Lbc.Say("Recent", true);
string[] aDirs = App.listRecentDirs.ToArray();
string[] aNames = new string[aDirs.Length];
for (int i = 0; i < aNames.Length; i++) aNames[i] = (aDirs[i].EndsWith(@":\") ? aDirs[i] : Path.GetFileName(aDirs[i]));
Array.Sort(aDirs, aNames);
string sName = ListDialog("Pick", "", aNames, true, 0);
if (sName.Length == 0) return;

int iName = Array.IndexOf(aNames, sName);
sResult = aDirs[iName];
frm.Close();
};

btnRecent.Text = "&Recent";
btnRecent.AccessibleName = btnRecent.Text.Replace("&", "");

Button btnQuick = new Button();

btnQuick.Click += delegate(object o, EventArgs e) {
Lbc.Say("Quick", true);
string sQuickDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FileDir\Quick";
sQuickDir = Path.GetFullPath(sQuickDir);
string[] aLinks = Directory.GetFiles(sQuickDir, "*.lnk");
string sDirs = "";
foreach (string sLink in aLinks) {
object oLink = Lbc.CreateObject("WScript.Shell");
oLink = Lbc.CallMethod(oLink, "CreateShortcut", new string[] {sLink});
string sDir = (string) Lbc.GetProperty(oLink, "TargetPath");
if (Directory.Exists(sDir)) sDirs += sDir + "\n";
}
string[] aDirs = sDirs.Trim().Split('\n');
string[] aNames = new string[aDirs.Length];
for (int i = 0; i < aNames.Length; i++) aNames[i] = (aDirs[i].EndsWith(@":\") ? aDirs[i] : Path.GetFileName(aDirs[i]));
Array.Sort(aDirs, aNames);
string sName = ListDialog("Pick", "", aNames, true, 0);
if (sName.Length == 0) return;

int iName = Array.IndexOf(aNames, sName);
sResult = aDirs[iName];
frm.Close();
};

btnQuick.Text = "&Quick";
btnQuick.AccessibleName = btnQuick.Text.Replace("&", "");

Button btnSpecial = new Button();

btnSpecial.Click += delegate(object o, EventArgs e) {
Lbc.Say("Special", true);
string sDir = Lbc.PickSpecialFolder();
if (sDir.Length == 0) return;
sResult = sDir;
frm.Close();
};

btnSpecial.Text = "&Special";
btnSpecial.AccessibleName = btnSpecial.Text.Replace("&", "");

flpLists.Controls.AddRange(new Control[] {btnCurrent, btnRecent, btnQuick, btnSpecial});
flpLists.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
//btnOK.Click += delegate(object o, EventArgs e) { sResult = txt.Text;frm.Close();};
btnOK.Click += delegate(object o, EventArgs e) {
sResult = txt.Text.Trim();
if (sResult != "" && !Directory.Exists(sResult)) {
string sPath = sResult;
if (sPath.StartsWith(@"\\")) {
string sDrive;
string sUnmapped = "A B C D E F G H I J K L M N O P Q R S T U V W X Y Z ";
DriveInfo[] allDrives = DriveInfo.GetDrives();
foreach (DriveInfo d in allDrives){
sDrive = d.Name.Substring(0, 1);
sUnmapped = sUnmapped.Replace(sDrive + " ", "");
}
string[] aUnmapped = sUnmapped.Trim().Split(' ');
//string s = "Pick Drive for Mapping " + sPath;
string s = "Pick Drive to Map";
sDrive = Lbc.ListDialog(s, "", aUnmapped, true, 0);
if (sDrive.Length == 0) return;

string sExe = "net.exe";
string sParams = "use " + sDrive + ": " + sPath;
try {
//Process.Start(sExe, sParams);
string sTempFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "FileDir.tmp");
if (File.Exists(sTempFile)) File.Delete(sTempFile);
string sCommand = sExe + " " + sParams + " 2>" + sTempFile;
//Lbc.RunHideWait(sCommand);
//Process.Start(sExe, sParams + " 2>" + sTempFile);
Lbc.MapDrive2Share(sDrive, sPath);
if (File.Exists(sTempFile)) {
string sOutput = File2String(sTempFile).Trim();
if (sOutput.Length > 0) Lbc.Show(sOutput, "Result");
}
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
}
}

else {
string sChoice = Lbc.ConfirmDialog("Confirm", "Cannot find folder " + sResult + "\nCreate it?", "Y");
if (sChoice == "Y") {
try {
DirectoryInfo di = new DirectoryInfo(sResult);
di.Create();
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
}
}
}
//else sResult = "";
}
if (Directory.Exists(sResult)) frm.Close();
else {
txt.SelectAll();
txt.Select();
}
};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); sResult = ""; frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpLists, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return sResult;
}

public static string FolderBrowseDialog(string sTitle, string sDefaultDir, bool iNewFolderButton) {
string sReturn = "";
FolderBrowserDialog dlg = new FolderBrowserDialog();
dlg.Description = sTitle;
dlg.ShowNewFolderButton = iNewFolderButton;
//dlg.RootFolder = sRootFolder;
dlg.SelectedPath = sDefaultDir;

if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.SelectedPath;
return sReturn;
}

public static string InputDialog(string sTitle, string sLabel, string sValue) {
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabel + ":";
lbl.AccessibleName = lbl.Text.Replace("&", "");
TextBox txt = new TextBox();
txt.AccessibleName = lbl.AccessibleName;
if (lbl.Text.Contains("Password:")) txt.UseSystemPasswordChar = true;
txt.Text = sValue;
txt.GotFocus += delegate(object o, EventArgs e) {txt.SelectAll();};
flpInput.Controls.AddRange(new Control[] {lbl, txt});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Click += delegate(object o, EventArgs e) { sResult = txt.Text;frm.Close();};
btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return sResult;
}

[MTAThread]
public string OldFieldDialog(string sTitle, string sLabelList, string sValueList) {
string sResultList = "";

//string[] aLabel = sLabelList.Split(Char.Parse("\t"));
string[] aLabel = sLabelList.Split('\t');
//string[] aValue = sValueList.Split(Char.Parse("\t"));
string[] aValue = sValueList.Split('\t');
string sAppTitle = GetAppTitle();

Interaction.AppActivate("JAWS");
Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

TableLayoutPanel tlpFields = new TableLayoutPanel();
tlpFields.SuspendLayout();
tlpFields.Anchor = AnchorStyles.None;
tlpFields.ColumnCount = 2;

for (int i = 0; i < tlpFields.ColumnCount; i++) {
tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
}

tlpFields.RowCount = aLabel.Length;

for (int i = 0; i < tlpFields.RowCount; i++) {
tlpFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = aLabel[i] + ":";
TextBox txt = new TextBox();
txt.Text = aValue[i];
txt.AccessibleName = lbl.Text.Replace("&", "");
tlpFields.Controls.AddRange(new Control[] {lbl, txt});
}
tlpFields.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (Control ctl in tlpFields.Controls) {
if (ctl.GetType() == typeof(TextBox)) sResultList = sResultList + "\t" + ctl.Text;
}
if (sResultList.Length > 0) sResultList = sResultList.Substring(1, sResultList.Length - 1);
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {tlpFields, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) {frm.Activate();};
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
if (sTitle != "") Interaction.AppActivate(sAppTitle);
return sResultList;
}

public static ArrayList FieldDialog(string sTitle, string[] sLabelList, string[] sValueList) {
return FieldDialog( sTitle, sLabelList, sValueList, false);
}

[MTAThread]
public string FieldDialog(string sTitle, string sLabelList, string sValueList) {
Interaction.AppActivate("JAWS");
return JoinList("\t", FieldDialog( sTitle, sLabelList.Split('\t'), sValueList.Split('\t'), false));
}

public static ArrayList FieldDialog(string sTitle, string[] sLabelList, string[] sValueList, bool bPassword) {
ArrayList sResultList = new ArrayList();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

TableLayoutPanel tlpFields = new TableLayoutPanel();
tlpFields.SuspendLayout();
tlpFields.AutoSize = true;
tlpFields.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
tlpFields.Anchor = AnchorStyles.None;
tlpFields.ColumnCount = 2;

for (int i = 0; i < tlpFields.ColumnCount; i++) {
tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
}

tlpFields.RowCount = sLabelList.Length;

for (int i = 0; i < tlpFields.RowCount; i++) {
tlpFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabelList[i] + ":";
lbl.AccessibleName = lbl.Text.Replace("&", "").Replace(":", "");
TextBox txt = new TextBox();
txt.Width *= 2;
txt.Text = sValueList[i];
txt.AccessibleName = lbl.AccessibleName;
if (lbl.Text.Contains("Password:")) txt.UseSystemPasswordChar = true;
txt.GotFocus += delegate(object o, EventArgs e) {txt.SelectAll();};
tlpFields.Controls.AddRange(new Control[] {lbl, txt});
}
tlpFields.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (Control ctl in tlpFields.Controls) {
if (ctl.GetType() == typeof(TextBox)) sResultList.Add(ctl.Text);
}
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {tlpFields, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + sValueList.Length + ")";
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) {frm.Activate();};
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return sResultList;
}

public static ArrayList MaskedFieldDialog(string sTitle, string[] sLabelList, string[] sValueList, string[] sMaskList) {
ArrayList sResultList = new ArrayList();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

TableLayoutPanel tlpFields = new TableLayoutPanel();
tlpFields.SuspendLayout();
tlpFields.Anchor = AnchorStyles.None;
tlpFields.ColumnCount = 2;

for (int i = 0; i < tlpFields.ColumnCount; i++) {
tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
}

tlpFields.RowCount = sLabelList.Length;

for (int i = 0; i < tlpFields.RowCount; i++) {
tlpFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabelList[i] + ":";
MaskedTextBox txt = new MaskedTextBox();
txt.BeepOnError = true;
txt.Mask = sMaskList[i];
txt.Text = sValueList[i];
txt.AccessibleName = lbl.Text.Replace("&", "");
tlpFields.Controls.AddRange(new Control[] {lbl, txt});
}
tlpFields.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (Control ctl in tlpFields.Controls) {
if (ctl.GetType() == typeof(MaskedTextBox)) sResultList.Add(ctl.Text);
}
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {tlpFields, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return sResultList;
}

public static string MemoDialog(string sTitle, string sText, string sValue, bool bSelectText) {
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

if (sText !="") {
Label lbl = new Label();
lbl.AutoSize = true;
int iLines = sText.Split('\n').Length;
lbl.AutoSize = false;
lbl.Width = 200;
lbl.Height = 16 * iLines + 16;
lbl.Margin = new Padding(3, 3, 3, 3);
lbl.Text = sText;
lbl.AccessibleName = lbl.Text.Replace("&", "");
lbl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
flpMain.Controls.Add(lbl);
}

TextBox memo = new TextBox();
memo.AcceptsReturn = true;
memo.AutoSize = false;
memo.Width = 200;
memo.Height = 200;
//memo.Height = 96; //default height of RichTextBox
memo.Multiline = true;
memo.ScrollBars = ScrollBars.Vertical;
memo.WordWrap = true;
memo.Text = sValue;

if (!bSelectText) {
memo.SelectionLength = 0;
memo.SelectionStart = 0;
}

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Click += delegate(object o, EventArgs e) { sResult = memo.Text;frm.Close();};
btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {memo, flpButtons});
flpMain.ResumeLayout();

frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return sResult;
}

public static void InfoDialog(string sTitle, string sValue, bool bSelectText) {
Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

RichTextBox info = new RichTextBox();
info.AutoSize = false;
info.Width = 300;
info.Height = 300;
info.Multiline = true;
info.ReadOnly = true;
info.ScrollBars = RichTextBoxScrollBars.Vertical;
info.Text = sValue;
info.WordWrap = true;

if (bSelectText) info.SelectAll();
else {
info.SelectionLength = 0;
info.SelectionStart = 0;
}

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnClose = new Button();
btnClose.Click += delegate(object o, EventArgs e) { frm.Close();};
btnClose.Text = "Close";

flpButtons.Controls.Add(btnClose);
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {info, flpButtons});
flpMain.ResumeLayout();

frm.CancelButton = btnClose;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
}

public static List<string> ListInputDialog(string sTitle, string sListLabel, string[] sValueList, string sInputLabel, string sValue, bool bSorted, int iDefaultIndex) {
List<string> listResults = new List<string>();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

Label lblList = new Label();
lblList.Text = sListLabel + ":";
lblList.AccessibleName = lblList.Text.Replace("&", "");

ListBox lst = new ListBox();
if (bSorted) lst.Sorted = true;
lst.Items.AddRange(sValueList);
lst.SelectedIndex = iDefaultIndex;

Label lblInput = new Label();
lblInput.Text = sInputLabel + ":";
lblInput.AccessibleName = lblInput.Text.Replace("&", "");
TextBox txt = new TextBox();
txt.AccessibleName = lblInput.AccessibleName;
if (lblInput.Text.Contains("Password:")) txt.UseSystemPasswordChar = true;
txt.Text = sValue;

flpInput.Controls.AddRange(new Control[] {lblList, lst, lblInput, txt});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Click += delegate(object o, EventArgs e) {
listResults.Add(lst.Text);
listResults.Add(txt.Text);
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return listResults;
}

[MTAThread]
public string ListDialog(string sTitle, string sValueList, int iSorted) {
Interaction.AppActivate("JAWS");
return ListDialog(sTitle, "", sValueList.Split('\t'), ((iSorted == 0) ? false : true), 0);
}

public static string ListDialog(string sTitle, string sLabel, string[] sValueList, bool bSorted, int iDefaultIndex) {
//return Pick(sTitle, sValueList, bSorted, iDefaultIndex);
return Dialog.Pick(sTitle, sValueList, bSorted, iDefaultIndex);
} // ListDialog method

public static string OldListDialog(string sTitle, string sLabel, string[] sValueList, bool bSorted, int iDefaultIndex) {
string sResult = "";
if (sValueList.Length == 0 || (sValueList.Length == 1 && sValueList[0].Trim().Length == 0)) {
Lbc.Say("No items!", true);
return sResult;
}

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

if (sLabel != "") {
Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabel + ":";
lbl.AccessibleName = lbl.Text.Replace("&", "");
flpInput.Controls.Add(lbl);
}

ListBox lst = new ListBox();
if (bSorted) lst.Sorted = true;
lst.Items.AddRange(sValueList);
lst.SelectedIndex = iDefaultIndex;

//flpInput.Controls.AddRange(new Control[] {lbl, lst});
flpInput.Controls.Add(lst);
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Click += delegate(object o, EventArgs e) { sResult = lst.Text;frm.Close();};
btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + sValueList.Length + ")";
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) {frm.Activate();};
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return sResult;
}

public static ArrayList CheckedListDialog(string sTitle, string sLabel, string[] sValueList, bool bSorted, int iDefaultIndex, int[] iCheckedList) {
ArrayList sResultList = new ArrayList();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabel + ":";

CheckedListBox lst = new CheckedListBox();
lst.SelectionMode = SelectionMode.One;
if (bSorted) lst.Sorted = true;
lst.Items.AddRange(sValueList);

for (int i = 0; i < iCheckedList.Length; i ++) {
lst.SetItemChecked(iCheckedList[i], true);
}

lst.SelectedIndex = iDefaultIndex;

flpInput.Controls.AddRange(new Control[] {lbl, lst});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (object item in lst.CheckedItems) {
sResultList.Add(item.ToString());
}
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return sResultList;
}

[MTAThread]
public String MultiListDialog(string sTitle, string sValueList, int iSorted) {
Interaction.AppActivate("JAWS");
return JoinList("\t", MultiListDialog(sTitle, "", sValueList.Split('\t'), ((iSorted == 0) ? false : true),0, new int[] {}));
}

public static ArrayList MultiListDialog(string sTitle, string sLabel, string[] sValueList, bool bSorted, int iDefaultIndex, int[] iSelectList) {
ArrayList sResultList = new ArrayList();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

if (sLabel != "") {
Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabel + ":";
lbl.AccessibleName = lbl.Text.Replace("&", "");
flpInput.Controls.Add(lbl);
}

ListBox lst = new ListBox();
lst.SelectionMode = SelectionMode.MultiSimple;
if (bSorted) lst.Sorted = true;
lst.Items.AddRange(sValueList);

for (int i = 0; i < iSelectList.Length; i ++) {
lst.SetSelected(iSelectList[i], true);
}

bool bState = lst.GetSelected(iDefaultIndex);
lst.SelectedIndex = iDefaultIndex;
lst.SetSelected(iDefaultIndex, bState);
//Lbc.Say(Lbc.SetCaretIndex(lst, iDefaultIndex));

//flpInput.Controls.AddRange(new Control[] {lbl, lst});
flpInput.Controls.Add(lst);
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (int i in lst.SelectedIndices) {
sResultList.Add(lst.Items[i].ToString());
}
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + sValueList.Length + ")";
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) {frm.Activate();};
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return sResultList;
}

public static List<int> MultiListDialog(string sTitle, string[] aValues, bool bSorted) {
List<int> listResults = new List<int>();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

ListBox lst = new ListBox();
lst.SelectionMode = SelectionMode.MultiSimple;
if (bSorted) lst.Sorted = true;
lst.Width = 2 * lst.Width;
lst.Items.AddRange(aValues);

flpInput.Controls.AddRange(new Control[] {lst});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (int index in lst.SelectedIndices)listResults.Add(index);
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + aValues.Length + ")";
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return listResults;
}

[MTAThread]
public string MultiListDialog(string sTitle, string sValueList, string sSelectList, int iSorted) {
string sResultList = "";
string[] aValue = sValueList.Split('\t');
string[] aSelect = sSelectList.Split('\t');

string sAppTitle = GetAppTitle();
Interaction.AppActivate("JAWS");
Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

ListBox lst = new ListBox();
lst.SelectionMode = SelectionMode.MultiSimple;
if (iSorted != 0) lst.Sorted = true;
lst.Items.AddRange(aValue);

for (int i = 0; i < aSelect.Length; i ++) {
lst.SetSelected(Int32.Parse(aSelect[i]), true);
}

flpInput.Controls.AddRange(new Control[] {lst});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (int i in lst.SelectedIndices) {
sResultList = sResultList + "\t" + lst.Items[i].ToString();
}
if (sResultList.Length > 0) sResultList = sResultList.Substring(1, sResultList.Length - 1);
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) {frm.Activate();};
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
if (sTitle != "") Interaction.AppActivate(sAppTitle);
return sResultList;
}

public static string ButtonDialog(string sTitle, string sText, string[] sButtonList, int iDefaultButton) {
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

if (sText !="") {
//Lbc.Say(sText);
Label lbl = new Label();
lbl.AutoSize = true;
int iLines = sText.Split('\n').Length;
lbl.AutoSize = false;
lbl.Width = 200;
lbl.Height = 16 * iLines + 16;
lbl.Margin = new Padding(3, 3, 3, 3);
lbl.Text = sText;
lbl.AccessibleName = lbl.Text.Replace("&", "");
lbl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
flpMain.Controls.Add(lbl);
}

for (int i = 0; i < sButtonList.Length; i++) {
Button btn = new Button();
btn.Click += delegate(object o, EventArgs e) {sResult = btn.Text; frm.Close();};
btn.Text = sButtonList[i];
btn.AccessibleName = sButtonList[i].Replace("&", "");
btn.AutoSize = false;
btn.Width = 200;
btn.Anchor = AnchorStyles.None;
flpMain.Controls.Add(btn);
}

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
btnCancel.AutoSize = false;
btnCancel.Width = 200;
//btnCancel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
flpMain.Controls.Add(btnCancel);

flpMain.ResumeLayout();

frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
//if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + sButtonList.Length + ")";
frm.Text = sTitle;
frm.Controls.Add(flpMain);

int iButton = 0;
foreach (Control ctl in flpMain.Controls) {
if (ctl.GetType() == typeof(Button)) {
if (iButton == iDefaultButton) ctl.Select();
iButton++;
}
}

frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) { Lbc.JFWSayString(sText); };
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
Lbc.Say(sResult.Replace("&", ""));
return sResult;
}

[MTAThread]
public string ButtonDialog(string sTitle, string sButtonList, int iDefaultButton) {
string sResult = "";
string[] aButton = sButtonList.Split('\t');

string sAppTitle = GetAppTitle();
Interaction.AppActivate("JAWS");
Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

for (int i = 0; i < aButton.Length; i++) {
Button btn = new Button();
btn.Click += delegate(object o, EventArgs e) {sResult = btn.Text; frm.Close();};
btn.Text = aButton[i];
btn.AccessibleName = btn.Text.Replace("&", "");
btn.AutoSize = false;
btn.Width = 200;
btn.Anchor = AnchorStyles.None;
flpMain.Controls.Add(btn);
}

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
btnCancel.AutoSize = false;
btnCancel.Width = 200;
//btnCancel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
flpMain.Controls.Add(btnCancel);

flpMain.ResumeLayout();

frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);

int iButton = 0;
foreach (Control ctl in flpMain.Controls) {
if (ctl.GetType() == typeof(Button)) {
if (iButton == iDefaultButton) ctl.Select();
iButton++;
}
}

frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) {frm.Activate();};
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
if (sTitle != "") Interaction.AppActivate(sAppTitle);
return sResult;
}

public static object[] ListButtonDialog(string sTitle, object[] aValue, string[] aDisplay, string[] aButton, bool bSort, int iIndex) {
object[] aResult = {};

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpData = new FlowLayoutPanel();
flpData.SuspendLayout();
flpData.Anchor = AnchorStyles.None;
flpData.AutoSize = true;
flpData.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpData.FlowDirection = FlowDirection.LeftToRight;

ListBox lst = new ListBox();
lst.Sorted = false;
if (aDisplay == null) lst.Items.AddRange(aValue);
else {
for (int i = 0; i < aDisplay.Length; i++) {
lst.Items.Add(aDisplay[i]);
Support.SetItemData(lst, i, i);
}
}
if (bSort) lst.Sorted = true;
lst.SelectedIndex = iIndex;

flpData.Controls.Add(lst);
flpData.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

for (int i = 0; i < aButton.Length; i++) {
Button btn = new Button();
btn.Click += delegate(object o, EventArgs e) {
object oItem;
if (aDisplay == null) oItem = lst.Text;
else {
int iItem = lst.SelectedIndex;
int iValue = Support.GetItemData(lst, iItem);
oItem = aValue[iValue];
}
aResult = new object[] {oItem, btn.Text};
Lbc.Say(btn.Text.Replace("&", ""), true);
frm.Close();
};

btn.Text = aButton[i];
btn.AccessibleName = aButton[i].Replace("&", "");
//btn.AutoSize = false;
//btn.Width = 200;
//btn.Anchor = AnchorStyles.None;
flpButtons.Controls.Add(btn);
}

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
//btnCancel.AutoSize = false;
//btnCancel.Width = 200;
//btnCancel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
flpButtons.Controls.Add(btnCancel);

flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpData, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = (Button) flpButtons.Controls[0];
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + aValue.Length + ")";
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return aResult;
} // ListButton dialog

public static string ConfirmDialog(string sTitle, string sText, string sDefault) {
switch (MessageBox.Show(sText, sTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, (sDefault == "N" ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1))) {
case DialogResult.Yes :
return "Y";
case DialogResult.No :
return "N";
}
return "";
}

public static string OldConfirmDialog(string sTitle, string sText, string sDefault) {
int iDefault = 1;
if (sDefault == "Y") iDefault = 0;
string sResult = Lbc.ButtonDialog(sTitle, sText, new String[] {"&Yes", "&No"}, iDefault);
sResult = sResult.Replace("&", "");
if (sResult.Length > 0) sResult = sResult.Substring(0, 1);
return sResult;
}

public static void AccessTableDialog(string sTitle, string sMdb, String sTable) {
string sConnectString = "Provider=Microsoft.JET.OLEDB.4.0;Data Source=" + sMdb;
string sFilterText = "";
string sJumpText = "";
String sSelectCommand = "select * from " + sTable;
OleDbConnection con = new OleDbConnection();
OleDbCommand cmd = new OleDbCommand();
OleDbDataAdapter da = new OleDbDataAdapter();
DataSet ds = new DataSet();
DataTable tbl = new DataTable();
BindingSource bs = new BindingSource();
ToolStrip bn = new ToolStrip();
ListBox lst = new ListBox();

con.ConnectionString = sConnectString;
cmd.Connection = con;
cmd.CommandText = sSelectCommand;
da.SelectCommand = cmd;

da.RowUpdated += delegate(object sender, OleDbRowUpdatedEventArgs e) {
if(e.StatementType == StatementType.Insert) {
OleDbCommand cmdID = new OleDbCommand("SELECT @@IDENTITY", da.SelectCommand.Connection);
e.Row[0] = (int)cmdID.ExecuteScalar( );
}
};

//da.Fill(tbl);
da.Fill(ds, sTable);
tbl = ds.Tables[0];
OleDbCommandBuilder cb = new OleDbCommandBuilder(da);
DataColumn column = new DataColumn("DisplayFields");
string s = "";
for (int i = 1; i < tbl.Columns.Count; i++) {
if (i < tbl.Columns.Count - 1) {
s += String.Format("IsNull({0}, '') + '\t' + ", tbl.Columns[i].ColumnName);
}
else {
s += String.Format("IsNull({0}, '')", tbl.Columns[i].ColumnName);
}
}
//Lbc.Show(s);
column.Expression = s;

tbl.Columns.Add(column);
bs.DataSource = ds;
//bs.DataSource = tbl;
bs.DataMember = tbl.TableName;
bn.GotFocus += delegate(object sender, EventArgs e) {Lbc.Say("Actions:");};

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpToolBar = new FlowLayoutPanel();
flpToolBar.SuspendLayout();
flpToolBar.Anchor = AnchorStyles.None;
flpToolBar.AutoSize = true;
flpToolBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpToolBar.FlowDirection = FlowDirection.LeftToRight;

Label toolBarLabel = new Label();
toolBarLabel.Text = "&Actions:";

bn.SuspendLayout();
bn.AutoSize = true;
bn.CanOverflow = false;
bn.LayoutStyle = ToolStripLayoutStyle.Flow;
//bn.Stretch = false;

TableLayoutPanel tlpFields = new TableLayoutPanel();
ToolStripStatusLabel positionLabel = new ToolStripStatusLabel();
ToolStripButton beginningButton = new ToolStripButton("&Beginning");
beginningButton.Click += delegate(object sender, EventArgs e) {Lbc.Say("Beginning"); bs.Position = 0; lst.Select();};

ToolStripButton createButton = new ToolStripButton("&Create");
createButton.Click += delegate(object sender, EventArgs e) {
Lbc.Say("Create");
bs.AddNew();
foreach (Control ctl in tlpFields.Controls) {
if (ctl.GetType() == typeof(TextBox)) {
TextBox txt = ctl as TextBox;
txt.ReadOnly = false;
}
}
tlpFields.Controls[1].Select();};

ToolStripButton deleteButton = new ToolStripButton("&Delete");
deleteButton.Click += delegate(object sender, EventArgs e) {
if (Lbc.ConfirmDialog("Confirm Delete", "", "N") == "Y") bs.RemoveCurrent();lst.Select();};

ToolStripButton endButton = new ToolStripButton("&End");
endButton.Click += delegate(object sender, EventArgs e) {Lbc.Say("End"); bs.Position = bs.Count - 1; lst.Select();};

ToolStripButton filterButton = new ToolStripButton("&Filter");
filterButton.Click += delegate(object o, EventArgs e) {
string sText = Lbc.InputDialog("Filter On", "Text", sFilterText);
if (sText.Length == 0) return;
sFilterText = sText;
bs.Filter = "DisplayFields LIKE '*" + sFilterText + "*'";
lst.Select();
};

ToolStripButton indexButton = new ToolStripButton("&Index");
indexButton.Click += delegate(object sender, EventArgs e) {
string[] sItemList = new string[tbl.Columns.Count];
for (int i = 0; i < tbl.Columns.Count; i++) sItemList[i] = tbl.Columns[i].ColumnName;
string sField = Lbc.ListDialog("Index Order", "Field", sItemList, false, 0);
bs.Sort = sField;
};

ToolStripButton jumpButton = new ToolStripButton("&Jump");
jumpButton.Click += delegate(object o, EventArgs e) {
string sText = Lbc.InputDialog("Jump To", "Text", sJumpText);
if (sText.Length == 0) return;
int iStart = 0;
if (sText == sJumpText) iStart = bs.Position + 1;
sJumpText = sText;
bool iFound = false;
for (int i = iStart; i < tbl.Rows.Count; i++) {
for (int j = 0; j < tbl.Columns.Count -1; j++) {
if (tbl.Rows[i][j].ToString().IndexOf(sJumpText) >= 0) {
bs.Position = i;
iFound = true;
break;
}
}
if (iFound) break;
}
if (!iFound) Lbc.Say("Not found!");
lst.Select();
};

ToolStripButton listButton = new ToolStripButton("&List");
listButton.Click += delegate(object sender, EventArgs e) {bs.EndEdit(); lst.Select();};

ToolStripButton modifyButton = new ToolStripButton("&Modify");
modifyButton.Click += delegate(object sender, EventArgs e) {
Lbc.Say("Modify");
foreach (Control ctl in tlpFields.Controls) {
if (ctl.GetType() == typeof(TextBox)) {
TextBox txt = ctl as TextBox;
txt.ReadOnly = false;
}
}
tlpFields.Controls[1].Select();};

ToolStripButton nextButton = new ToolStripButton("&Next");
nextButton.Click += delegate(object sender, EventArgs e) {Lbc.Say("Next"); bs.Position++; lst.Select();};

ToolStripButton previousButton = new ToolStripButton("&Previous");
previousButton.Click += delegate(object sender, EventArgs e) {Lbc.Say("Previous"); bs.Position --; lst.Select();};

ToolStripButton quitButton = new ToolStripButton("&Quit");
quitButton.Click += delegate(object sender, EventArgs e) {
Lbc.Say("Quit");
if (tbl.DataSet.HasChanges()) {
string sChoice = Lbc.ConfirmDialog("Write Changes?", "", "Y");
if (sChoice.Length == 0) return;
if (sChoice == "Y") {
bs.EndEdit();
da.Update(tbl);
}
}
frm.Close();};

ToolStripButton repeatJumpButton = new ToolStripButton("&RepeatJump");
repeatJumpButton.Click += delegate(object o, EventArgs e) {
Lbc.Say("Repeat jump");
string sText = sJumpText;
if (sText.Length == 0) return;
int iStart = 0;
if (sText == sJumpText) iStart = bs.Position + 1;
sJumpText = sText;
bool iFound = false;
for (int i = iStart; i < tbl.Rows.Count; i++) {
for (int j = 0; j < tbl.Columns.Count -1; j++) {
if (tbl.Rows[i][j].ToString().IndexOf(sJumpText) >= 0) {
bs.Position = i;
iFound = true;
break;
}
}
if (iFound) break;
}
if (!iFound) Lbc.Say("Not found!");
lst.Select();
};

ToolStripButton viewButton = new ToolStripButton("&View");
viewButton.Click += delegate(object sender, EventArgs e) {
Lbc.Say("View");
foreach (Control ctl in tlpFields.Controls) {
if (ctl.GetType() == typeof(TextBox)) {
TextBox txt = ctl as TextBox;
txt.ReadOnly = true;
}
}
tlpFields.Controls[1].Select();};

ToolStripButton writeButton = new ToolStripButton("&Write");
writeButton.Click += delegate(object o, EventArgs e) {
Lbc.Say("Write");
bs.EndEdit();
da.Update(tbl);
positionLabel.Text = String.Format("ID {0}\t{1}", tbl.Rows[bs.Position][0], (tbl.DataSet.HasChanges() ? "Modified" : ""));
};

ToolStripItem[] buttons = {beginningButton, createButton, deleteButton, endButton, filterButton, indexButton, jumpButton, listButton, modifyButton, nextButton, previousButton, quitButton, repeatJumpButton, viewButton, writeButton};
foreach (ToolStripItem button in buttons) {
button.DisplayStyle = ToolStripItemDisplayStyle.Text;
button.Size = new Size(23, 22);
}
bn.Items.AddRange(buttons);
//bn.Size = new Size(400, 25);
bn.TabStop = true;
bn.ResumeLayout();

flpToolBar.Controls.AddRange(new Control[] {toolBarLabel, bn});
flpToolBar.ResumeLayout();

FlowLayoutPanel flpData = new FlowLayoutPanel();
flpData.SuspendLayout();
flpData.Anchor = AnchorStyles.None;
flpData.AutoSize = true;
flpData.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpData.FlowDirection = FlowDirection.LeftToRight;

lst.DataSource = bs;
lst.DisplayMember = "DisplayFields";
lst.GotFocus += delegate(object sender, EventArgs e) {bs.EndEdit();};
lst.Size = new Size(200, 200);
lst.UseTabStops = true;

tlpFields.SuspendLayout();
tlpFields.Anchor = AnchorStyles.None;
tlpFields.AutoSize = true;
tlpFields.AutoSizeMode = AutoSizeMode.GrowAndShrink;
tlpFields.ColumnCount = 2;
tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
tlpFields.RowCount = tbl.Columns.Count - 2;

for (int i = 1; i < tbl.Columns.Count - 1; i ++) {
tlpFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = tbl.Columns[i].ColumnName + ":";
TextBox txt = new TextBox();
txt.DataBindings.Add("Text", bs, tbl.Columns[i].ColumnName);
tlpFields.Controls.AddRange(new Control[] {lbl, txt});
}
tlpFields.ResumeLayout();

flpData.Controls.AddRange(new Control[] {lst, tlpFields});
flpData.ResumeLayout();

StatusStrip sb = new StatusStrip();
sb.SuspendLayout();
//bs.PositionChanged += delegate(object o, EventArgs e) {positionLabel.Text = String.Format("ID {0} record {1} of {2}", tbl.Rows[bs.Position][0], bs.Position + 1, bs.Count);};
//bs.PositionChanged += delegate(object o, EventArgs e) {positionLabel.Text = String.Format("ID {0}", tbl.Rows[bs.Position][0]);};
bs.PositionChanged += delegate(object o, EventArgs e) {positionLabel.Text = String.Format("ID {0}\t{1}", tbl.Rows[bs.Position][0], (tbl.DataSet.HasChanges() ? "Modified" : ""));};
sb.Items.AddRange(new ToolStripItem[] {positionLabel});
sb.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpToolBar, flpData, sb});
//flpMain.Controls.AddRange(new Control[] {bn, flpData, sb});
flpMain.ResumeLayout();

frm.Controls.Add(flpMain);
frm.StartPosition = FormStartPosition.CenterParent;
//frm.StartPosition = FormStartPosition.CenterScreen;
frm.Text = sTitle;
positionLabel.Text = String.Format("ID {0}\t{1}", tbl.Rows[bs.Position][0], (tbl.DataSet.HasChanges() ? "Modified" : ""));
lst.Select();
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
//frm.Show();
}

public static int RunHideWait(string sPath) {
return Interaction.Shell(sPath, AppWinStyle.Hide, true, -1);
} //RunHideWait method

public static int RunHide(string sPath) {
return Interaction.Shell(sPath, AppWinStyle.Hide, false, -1);
} //RunHide method

public static int Run(string sPath) {
return Interaction.Shell(sPath, AppWinStyle.NormalFocus, false, -1);
} //Run method

public static int RunWait(string sPath) {
return Interaction.Shell(sPath, AppWinStyle.NormalFocus, true, -1);
} //RunWait method



public static string GetProgramOutput(string sExe, string sParams) {
Process myProcess = new Process();
ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(sExe, sParams);
myProcessStartInfo.UseShellExecute = false;
myProcessStartInfo.RedirectStandardOutput = true;
myProcessStartInfo.CreateNoWindow = true;
myProcessStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
// myProcessStartInfo.WorkingDirectory = ".";
myProcess.StartInfo = myProcessStartInfo;
myProcess.Start();
string myString = myProcess.StandardOutput.ReadToEnd();
myProcess.WaitForExit();
myProcess.Close();
return myString;
}

public static string File2String(string sFile) {
if (!File.Exists(sFile)) return "";
StreamReader textReader = new StreamReader(sFile);
string sBody = textReader.ReadToEnd();
textReader.Close();
return sBody;
} // File2String method

public static void String2File(string sBody, string sFile) {
StreamWriter textWriter = new StreamWriter(sFile);
textWriter.Write(sBody);
textWriter.Close();
} // String2File method

public static bool CreateMdb(string sMdb) {
string sConnectString = "Provider=Microsoft.JET.OLEDB.4.0;Data Source=" + sMdb;
Type ADOXCatalog = Type.GetTypeFromProgID("ADOX.Catalog");
object catalog = Activator.CreateInstance(ADOXCatalog);
object[] inputArguments = {sConnectString};
ADOXCatalog.InvokeMember("Create", BindingFlags.InvokeMethod, null, catalog, inputArguments);

bool bResult = File.Exists(sMdb);
return bResult;
}

public static int ExecuteMdbSql(string sMdb, string sSql) {
string sConnectString = "Provider=Microsoft.JET.OLEDB.4.0;Data Source=" + sMdb;
OleDbConnection connection = new OleDbConnection(sConnectString);
connection.Open();
OleDbCommand command = new OleDbCommand(sSql, connection);
int iResult = command.ExecuteNonQuery();
connection.Close();
return iResult;
}

[ DllImport("KERNEL32.DLL", EntryPoint="GetPrivateProfileString")]
protected internal static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

[ DllImport("KERNEL32.DLL")]
protected internal static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName, int iDefault, string lpFileName) ;

[ DllImport("KERNEL32.DLL", EntryPoint="WritePrivateProfileString")]
protected internal static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

[ DllImport("KERNEL32.DLL", EntryPoint="GetPrivateProfileSection")]
protected internal static extern int GetPrivateProfileSection(string lpAppName,   byte[] lpReturnedString, int nSize, string lpFileName);

[ DllImport("KERNEL32.DLL", EntryPoint="WritePrivateProfileSection")]
protected internal static extern bool WritePrivateProfileSection(string lpAppName,   byte[] data, string lpFileName);

[ DllImport("KERNEL32.DLL", EntryPoint="GetPrivateProfileSectionNames")]
protected internal static extern int GetPrivateProfileSectionNames(byte[] lpReturnedString, int nSize, string lpFileName);


public static String ReadValue(String filename, String section, String key, string sDefault) {
StringBuilder buffer = new StringBuilder(256);
//string sDefault = "";
if (GetPrivateProfileString(section, key, sDefault, buffer, buffer.Capacity, filename) != 0 ) {
return buffer.ToString();
}
else {
//return null;
return sDefault;
}
}

public static bool WriteValue(String filename, String section, String key, String sValue) {
return WritePrivateProfileString(section, key, sValue, filename);
}
public static int GetINIInt(String filename, String section, String key) {
int iDefault=-1;
return GetPrivateProfileInt(section, key, iDefault, filename);
}
public static StringCollection GetINISection(String filename, String section) {
StringCollection items = new StringCollection();
byte[] buffer = new byte[32768];
int bufLen=0;
bufLen = GetPrivateProfileSection(section, buffer, buffer.GetUpperBound(0), filename);
if (bufLen > 0) {
StringBuilder sb = new StringBuilder();
for(int i=0; i < bufLen; i++) {
if (buffer[i] != 0) {
sb.Append((char) buffer[i]);
}
else {
if (sb.Length > 0) {
items.Add(sb.ToString());
sb = new StringBuilder();
}
}
}
}
return items;
}
public static bool WriteINISection(string filename, string section, StringCollection items) {
byte[] b = new byte[32768];
int j=0;
foreach(string s in items) {
ASCIIEncoding.ASCII.GetBytes(s,0,s.Length,b,j);
j += s.Length;
b[j] = 0;
j +=1;
}
b[j]=0;
return WritePrivateProfileSection(section, b, filename);
}

public static StringCollection GetINISectionNames(String filename) {
StringCollection sections = new StringCollection();
byte[] buffer = new byte[32768];
int bufLen=0;
bufLen = GetPrivateProfileSectionNames(buffer, buffer.GetUpperBound(0), filename);
if (bufLen > 0) {
StringBuilder sb = new StringBuilder();
for(int i=0; i < bufLen; i++) {
if (buffer[i] != 0) {
sb.Append((char) buffer[i]);
}
else {
if (sb.Length > 0) {
sections.Add(sb.ToString());
sb = new StringBuilder();
}
}
}
}
return sections;
}

public static string Pluralize(int iCount, string sItem) {
string sResult = iCount.ToString() + " " + sItem;
if (iCount != 1) sResult += "s";
return sResult;
} // Pluralize method

public static object ObjectMethod(string sProgID, string sMethod, object[] args) {
Type t = Type.GetTypeFromProgID(sProgID);
object o = Activator.CreateInstance(t);
return t.InvokeMember(sMethod, BindingFlags.InvokeMethod, null, o, args);
} // ObjectMethod

public static object CreateObject(string sProgID) {
Type t = Type.GetTypeFromProgID(sProgID);
object oResult = Activator.CreateInstance(t);
return oResult;
} // CreateObject method

public static object CallMethod(object o, string sMethod) {
object[] args = {};
return CallMethod(o, sMethod, args);
}

public static object CallMethod(object o, string sMethod, string sValue) {
object[] args = {sValue};
return CallMethod(o, sMethod, args);
}

public static object CallMethod(object o, string sMethod, int iValue) {
object[] args = {iValue};
return CallMethod(o, sMethod, args);
}

public static object CallMethod(object o, string sMethod, object[] args) {
Type t = o.GetType();
object oResult = t.InvokeMember(sMethod, BindingFlags.InvokeMethod, null, o, args);
return oResult;
} // CallMethod

public static object SetProperty(object o, string sProperty, string sValue) {
object[] args = {sValue};
return SetProperty(o, sProperty, args);
}

public static object SetProperty(object o, string sProperty, int iValue) {
object[] args = {iValue};
return SetProperty(o, sProperty, args);
}

public static object SetProperty(object o, string sProperty, object[] args) {
Type t = o.GetType();
object oResult = t.InvokeMember(sProperty, BindingFlags.SetProperty, null, o, args);
return oResult;
} // SetProperty


public static object GetProperty(object o, string sProperty, object[] args) {
Type t = o.GetType();
object oResult = t.InvokeMember(sProperty, BindingFlags.GetProperty, null, o, args);
return oResult;
}

public static object GetProperty(object o, string sProperty) {
object[] args = new object[] {};
return GetProperty(o, sProperty, args);
} // GetProperty

public static void MakeShortcut(string sPath) {
string sDir = @"C:\\temp";

using (StreamWriter writer = new StreamWriter(sDir + "\\" + sPath + ".url"))
{
writer.WriteLine("[InternetShortcut]");
writer.WriteLine("URL=file:///" + sPath);
writer.Flush();
}
} // MakeShortcut method

public static void Path2Link(string sPath, string sLink) {
if (sLink == "") sLink = sPath + ".lnk";
object o = Lbc.CreateObject("WScript.Shell");
o = Lbc.CallMethod(o, "CreateShortcut", new string[] {sLink});
Lbc.SetProperty(o, "TargetPath", new string[] {sPath});
Lbc.SetProperty(o, "WorkingDirectory", new string[] {Path.GetDirectoryName(sPath)});
//Lbc.SetProperty(o, "WindowStyle", new object[] {3});
Lbc.SetProperty(o, "WindowStyle", new object[] {1});
Lbc.CallMethod(o, "Save", new object[] {});
} // Path2Link method

public bool OldPath2Link(string sPath, string sLink) {
object[] args;
if (sLink == "") sLink = sPath + ".lnk";
//try {
Type t = Type.GetTypeFromProgID("WScript.Shell");
object o = Activator.CreateInstance(t);
args = new object[] {sLink};
object oLink = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, o, args);
args = new object[] {sPath};
Type tLink = oLink.GetType();
t.InvokeMember("TargetPath", BindingFlags.SetProperty, null, oLink, args);
//t.InvokeMember("TargetPath", BindingFlags.SetProperty, null, o, args);
//t.InvokeMember("TargetPath", BindingFlags.Instance | BindingFlags.SetProperty, null, o, args);
args = new object[] {3};
//t.InvokeMember("WindowStyle", BindingFlags.SetProperty, null, o, args);
tLink.InvokeMember("WindowStyle", BindingFlags.SetProperty, null, oLink, args);
args = new object[] {};
//t.InvokeMember("Save", BindingFlags.InvokeMethod, null, o, args);
tLink.InvokeMember("Save", BindingFlags.InvokeMethod, null, oLink, args);
//}
//catch {
//return false;
//}
return true;
} // Path2Link method

public static string GetUniqueName(string sSource) {
if (!Directory.Exists(sSource) && !File.Exists(sSource)) return sSource;
string sTarget = "";
string sDir = Path.GetDirectoryName(sSource);
string sRoot = Path.GetFileNameWithoutExtension(sSource);
sRoot = Regex.Replace(sRoot, @"_\d\d$", "");
//Regex rx = new Regex(@"_\d\d$");
//sRoot = rx.Replace(sRoot, "");
string sExt = Path.GetExtension(sSource);
for (int i = 1; i < 100; i++) {
string sNewName = sRoot + "_" + i.ToString().PadLeft(2, '0') + sExt;
sTarget = Path.Combine(sDir, sNewName);
if (!Directory.Exists(sTarget) && !File.Exists(sTarget)) break;
}
if (Directory.Exists(sTarget) || File.Exists(sTarget)) sTarget = "";
return sTarget;
} // GetUniqueName method

public static string[] GetFtpDir(string sUserName, string sPassword, string sURL) {
FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(sURL);
request.Credentials = new NetworkCredential(sUserName, sPassword);
//request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
request.Proxy = GlobalProxySelection.GetEmptyWebProxy();
request.Method = WebRequestMethods.Ftp.ListDirectory;
FtpWebResponse response = null;
//try {
//request.UsePassive = true;
//request.UsePassive = false;
response = (FtpWebResponse)request.GetResponse();
//}
//catch (Exception ex) {
//Lbc.Show(ex.Message, "Error");
//return new string[] {};
//}

StreamReader reader = new StreamReader(response.GetResponseStream());
StringBuilder sb = new StringBuilder();
string sLine = "";
while ((sLine = reader.ReadLine()) != null) sb.Append(sLine + "\n");
reader.Close();
return sb.ToString().Trim().Split('\n');
} // GetFtpDir method

//public static string Eval(string sCSCode) {
public static object Eval(string sCSCode) {
CSharpCodeProvider c = new CSharpCodeProvider();
ICodeCompiler icc = c.CreateCompiler();
CompilerParameters cp = new CompilerParameters();

cp.ReferencedAssemblies.Add("system.dll");
cp.ReferencedAssemblies.Add("system.xml.dll");
cp.ReferencedAssemblies.Add("system.data.dll");
cp.ReferencedAssemblies.Add("system.windows.forms.dll");
cp.ReferencedAssemblies.Add("system.drawing.dll");

cp.CompilerOptions = "/t:library";
cp.GenerateInMemory = true;

StringBuilder sb = new StringBuilder("");
sb.Append("using System;\n" );
sb.Append("using System.Xml;\n");
sb.Append("using System.Data;\n");
sb.Append("using System.Data.SqlClient;\n");
sb.Append("using System.Windows.Forms;\n");
sb.Append("using System.Drawing;\n");

sb.Append("namespace CSCodeEvaler{ \n");
sb.Append("public class CSCodeEvaler{ \n");
sb.Append("public object EvalCode(){\n");
sb.Append("return "+sCSCode+"; \n");
sb.Append("} \n");
sb.Append("} \n");
sb.Append("}\n");

CompilerResults cr = icc.CompileAssemblyFromSource(cp, sb.ToString());
if( cr.Errors.Count > 0 ){
//      MessageBox.Show("ERROR: " + cr.Errors[0].ErrorText, "Error evaluating cs code", MessageBoxButtons.OK, MessageBoxIcon.Error );
Lbc.Show(cr.Errors[0].ErrorText, "Error");
return null;
}

System.Reflection.Assembly a = cr.CompiledAssembly;
object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");

Type t = o.GetType();
MethodInfo mi = t.GetMethod("EvalCode");

object s = mi.Invoke(o, null);
//string s = mi.Invoke(o, null).ToString();
return s;
} // Eval method

public static string Quote(string s) {
return "\"" + s.Trim('"') + "\"";
} // Quote method

public static string Unquote(string s) {
return s.Trim('"');
} // Unquote method

[Serializable]
public struct ShellExecuteInfo {
public int Size;
public uint Mask;
public IntPtr hwnd;
public string Verb;
public string File;
public string Parameters;
public string Directory;
public uint Show;
public IntPtr InstApp;
public IntPtr IDList;
public string Class;
public IntPtr hkeyClass;
public uint HotKey;
public IntPtr Icon;
public IntPtr Monitor;
}

[DllImport("shell32.dll", SetLastError = true)]
extern public static bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

public const uint SW_NORMAL = 1;

public static void OpenWith(string file) {
ShellExecuteInfo sei = new ShellExecuteInfo();
sei.Size = Marshal.SizeOf(sei);
sei.Verb = "openas";
sei.File = file;
sei.Show = SW_NORMAL;
if (!ShellExecuteEx(ref sei))
throw new System.ComponentModel.Win32Exception();
} //OpenAs method

public static void Properties(string sPath) {
Lbc.InvokeVerb(sPath, "P&roperties");
Lbc.InvokeVerb(sPath, "Properties");
//Lbc.Show("", "Return to FileDir When Done");
} // Properties method

public static void InvokeVerb(string sPath, string sVerb) {
object o = Lbc.CreateObject("Shell.Application");
string sDir = Path.GetDirectoryName(sPath);
string sName = Path.GetFileName(sPath);
o = Lbc.CallMethod(o, "Namespace", new string[] {sDir});
o = Lbc.CallMethod(o, "ParseName", new string[] {sName});
o = Lbc.CallMethod(o, "InvokeVerb", new string[] {sVerb});
//Lbc.Show("", "Return to FileDir When Done");
} // InvokeVerb method

public static string[] Verbs(string sPath) {
object o = Lbc.CreateObject("Shell.Application");
string sDir = Path.GetDirectoryName(sPath);
string sName = Path.GetFileName(sPath);
o = Lbc.CallMethod(o, "Namespace", new string[] {sDir});
o = Lbc.CallMethod(o, "ParseName", new string[] {sName});
try {
o = Lbc.CallMethod(o, "Verbs", new object[] {});
}
catch {
return new string[] {};
}
int iCount = (int) Lbc.GetProperty(o, "Count");
StringBuilder sb = new StringBuilder();
for (int i = 0; i < iCount; i++) {
object oVerb = Lbc.CallMethod(o, "Item", new object[] {(int) i});
string sVerb = (string) Lbc.GetProperty(oVerb, "Name");
if (sVerb.Trim() != "") sb.Append(sVerb + "\n");
}
string[] aVerbs = sb.ToString().Trim().Split('\n');
return aVerbs;
} // Verbs method

public static void Path2Clipboard(string[] aPaths, bool bCut) {
IDataObject data = new DataObject(DataFormats.FileDrop, aPaths);
MemoryStream memo = new MemoryStream(4);
byte[] aBytes = null;
if (bCut) aBytes = new byte[]{2, 0, 0, 0};
else aBytes = new byte[]{5, 0, 0, 0};
memo.Write(aBytes, 0, aBytes.Length);
data.SetData("Preferred DropEffect", memo);

string sPaths = String.Join("\r\n", aPaths).Trim();
data.SetData(sPaths);
Clipboard.SetDataObject(data, true);

//GC.KeepAlive(data);
//GC.SuppressFinalize(data);
} // Path2Clipboard method

//public static string[] Clipboard2Path(out byte[] aBytes) {
public static string[] Clipboard2Path(out bool bCut) {
bCut = false;
if (!Clipboard.ContainsFileDropList()) return new string[] {};

IDataObject data = Clipboard.GetDataObject();
string[] aPaths = (string[])data.GetData(DataFormats.FileDrop, true);
MemoryStream stream = (MemoryStream)data.GetData("Preferred DropEffect",true);
byte[] aBytes = new byte[] { (byte)stream.ReadByte(), 0, 0, 0 };
bCut = BytesCompare(aBytes, new byte[] {2, 0, 0, 0}) ? true : false;
return aPaths;
} // Clipboard2Path method

public static bool BytesCompare(byte[] a1, byte[] a2) {
if (a1.Length != a2.Length) return false;
for (int i = 0; i < a1.Length; i++) if (a1[i] != a2[i]) return false;
return true;
} // BytesCompare method

public static string GetTempFolder() {
object oSystem = Lbc.CreateObject("Scripting.FileSystemObject");
Object oDir = Lbc.CallMethod(oSystem, "GetSpecialFolder", new object[] {2});
string sPath = (string) Lbc.GetProperty(oDir, "Path");
return sPath;
}

public static string GetPathType(string sPath) {
string sType;
if (Directory.Exists(sPath)) sType = "Folder";
else {
object oSystem = Lbc.CreateObject("Scripting.FileSystemObject");
Object oFile = Lbc.CallMethod(oSystem, "GetFile", new object[] {sPath});
sType = (string) Lbc.GetProperty(oFile, "Type");
}
return sType;
}

public static string GetUrl() {
object oShell = Lbc.CreateObject("Shell.Application");
//object oWindows = Lbc.GetProperty(oShell, "Windows");
object oWindows = Lbc.CallMethod(oShell, "Windows");
int iCount = (int) Lbc.GetProperty(oWindows, "Count");
string sUrl = "";
if (iCount > 0) {
object oWindow = Lbc.CallMethod(oWindows, "Item", new object[] {iCount - 1});
sUrl = (string) Lbc.GetProperty(oWindow, "LocationURL");
}
return sUrl;
}

public static bool MapDrive2Share(string sDrive, String sShare) {
bool bResult = false;
NetworkDrive oNetDrive  = new NetworkDrive();
try {
oNetDrive.LocalDrive = sDrive + ":";
oNetDrive.ShareName  = sShare;
oNetDrive.Force = true;
oNetDrive.Persistent = true;
oNetDrive.PromptForCredentials = true;
//oNetDrive.SaveCredentials = true;
//oNetDrive.ShowConnectDialog(null);
oNetDrive.MapDrive();
bResult = true;
}
catch(Exception ex) {
Lbc.Show(ex.Message, "Error");
bResult = false;
}
oNetDrive = null;
return bResult;
} // MapDrive2Share method

public static bool UnmapDrive(string sDrive) {
bool bResult = false;
NetworkDrive oNetDrive  = new NetworkDrive();
try {
oNetDrive.LocalDrive = sDrive + ":";
oNetDrive.Force = true;
oNetDrive.Persistent = true;
oNetDrive.PromptForCredentials = true;
oNetDrive.UnMapDrive();
bResult = true;
}
catch(Exception ex) {
Lbc.Show(ex.Message, "Error");
bResult = false;
}
oNetDrive = null;
return bResult;
} // UnmapDrive method

[DllImport("urlmon.dll")]
public static extern int URLDownloadToFile(int i1, string sUrl, string sFile, int i2, int i3, int i4);
public static bool Url2File(string sUrl, string sFile) {
int iResult = URLDownloadToFile(0, sUrl, sFile, 0, 0, 0);
return iResult == 0;
} // Url2File method

public static string FindOnPath(string sName) {
string sPaths = Environment.GetEnvironmentVariable("PATH");
sPaths = Directory.GetCurrentDirectory() + ";" + sPaths;
string[] aPaths = sPaths.Split(';');
string sFile = "";
bool bFound = false;
for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
sPath = sPath.Trim(new char[] {'"', ' '});
if (sPath.Length == 0) continue;

sFile = Path.Combine(sPath, sName);
if (File.Exists(sFile)) {
bFound = true;
break;
} // if
} // for
if (!bFound) sFile = "";
return sFile;
} // FindOnPath method

public static string PickSpecialFolder() {
string sName = "";
string sPath = "";
StringBuilder sbNames = new StringBuilder();
StringBuilder sbPaths = new StringBuilder("\n");
object oShell = Lbc.CreateObject("Shell.Application");
for (int i = 0; i < 100; i++) {
try {
Object oDir = Lbc.CallMethod(oShell, "Namespace", new object[] {i});
Object oItem = Lbc.GetProperty(oDir, "Self");
sPath = (string) Lbc.GetProperty(oItem, "Path");
if (!Directory.Exists(sPath)) continue;
if (sbPaths.ToString().ToLower().Trim('\\').Contains("\n" + sPath.ToLower().Trim('\\') + "\n")) continue;
sbPaths.Append(sPath + "\n");
sName = (string) Lbc.GetProperty(oItem, "Name");
if (Lbc.Equiv(sName, "Temporary Internet Files")) sName = "Internet Cache";
else if (Lbc.Equiv(sName, "History")) sName = "Internet History";
else if (Lbc.Equiv(sName, "NetHood")) sName = "Network Neighborhood";
else if (Lbc.Equiv(sName, "PrintHood")) sName = "Printer Neighborhood";
else if ((@"\" + sPath.ToLower() + @"\").Contains(@"\all users\")) sName = "Common " + sName;
else if (!Lbc.Equiv(sName, "History") && (@"\" + sPath.ToLower() + @"\").Contains(@"\local settings\")) sName = "Local " + sName;
sbNames.Append(sName + "\n");
}
catch {continue;}
}

Environment.SpecialFolder folder;
for (int i = 0; i < 100; i++) {
sPath = "";
try {
folder = (Environment.SpecialFolder) i;
sPath = Environment.GetFolderPath(folder);
}
catch {
continue;
}
if (!Directory.Exists(sPath)) continue;
if (sbPaths.ToString().ToLower().Trim('\\').Contains("\n" + sPath.ToLower().Trim('\\') + "\n")) continue;
sbPaths.Append(sPath + "\n");
sName = folder.ToString();
sbNames.Append(sName + "\n");
}
sbNames.Append("Temp" + "\n");
sbPaths.Append(Lbc.GetTempFolder() + "\n");

string[] aNames = sbNames.ToString().Trim().Split('\n');
//Clipboard.SetText(sbNames.ToString());
string[] aPaths = sbPaths.ToString().Trim().Split('\n');

sName = Lbc.ListDialog("Pick", "", aNames, true, 0);
if (sName.Length == 0) return "";
int iName = Array.IndexOf(aNames, sName);
string sDir = aPaths[iName];
return sDir;
} // PickSpecialFolder method

[DllImport("Shell32.dll")]
public static extern int SHFormatDrive(IntPtr h, int iDrive, int iFormatId,
int iFlags);
public static int FormatDrive(IntPtr h, string sDrive) {
sDrive = sDrive.Substring(0, 1).ToLower();
string sDrives = "abcdefghijklmnopqrstuvwxyz";
int iDrive = sDrives.IndexOf(sDrive);
int iFormatId = 0;
int iFlags = 0;
return SHFormatDrive(h, iDrive, iFormatId, iFlags);
} // FormatDrive method

public static string GetLegalFileRoot(string sRoot) {
StringBuilder sb = new StringBuilder();
for (int i = 0; i < sRoot.Length; i++) {
if (Char.IsLetterOrDigit(sRoot, i)) sb.Append(sRoot.Substring(i, 1));
else sb.Append("_");
}
string sReturn = sb.ToString();
Regex rx = new Regex(@"_+");
sReturn = rx.Replace(sReturn, "_");
sReturn = sReturn.Trim(new Char[] {'_', ' '});
sReturn = sReturn.Replace("_", " ").Trim();
return sReturn;
} // GetLegalFileRoot method

public static string GetUniqueFileName(string sSource) {
if (!Directory.Exists(sSource) && !File.Exists(sSource)) return sSource;
string sTarget = "";
string sDir = Path.GetDirectoryName(sSource);
string sRoot = Path.GetFileNameWithoutExtension(sSource);
Regex rx = new Regex(@"_\d\d$");
sRoot = rx.Replace(sRoot, "");
string sExt = Path.GetExtension(sSource);
for (int i = 1; i < 1000; i++) {
string sNewName = sRoot + "_" + i.ToString().PadLeft(3, '0') + sExt;
sTarget = Path.Combine(sDir, sNewName);
if (!Directory.Exists(sTarget) && !File.Exists(sTarget)) break;
}
return sTarget;
} // GetUniqueFileName method

public static string GetExtensions(string[] aFiles) {
//string[] aFilters = new string[] {"*.*"};
//bool bSubdirs = false;
//string[] aFiles = GetFiles(sDir, aFilters, bSubdirs);
List<string> list = new List<string>(aFiles.Length);
for (int i = 0; i < aFiles.Length; i++) {
string s = aFiles[i];
s = Path.GetExtension(s);
//if (s.Length == 0) continue;
s = s.TrimStart('.');
s = s.ToLower();
if (s.Length > 0 && !list.Contains(s)) list.Add(s);
}

list.Sort();
string[] aExtensions = list.ToArray();
return String.Join(" ", aExtensions);
} // GetExtensions method

public static string[] GetFilesWithExtensions(string[] aFiles, string sExtensions) {
string sResult = "." + sExtensions.Trim().Replace(" ", " .");
string [] aResults = sResult.Split(' ');
List<string> list = new List<string>(aFiles);
for (int i = list.Count -1; i >=0; i--) {
string sFile = list[i];
string sExtension = Path.GetExtension(sFile).ToLower();
if (Array.IndexOf(aResults, sExtension) == -1) list.RemoveAt(i);
}
return list.ToArray();
} // GetFilesWithExtensions method

public static string GetFileFromUri(string sUri) {
string sFile;
Uri oUri = new Uri(sUri);
//if (oUri.IsFile) {
sFile = oUri.LocalPath;
try {
sFile = Path.GetFileName(sFile);
}
catch {
sFile = "";
}
//else {
if (sFile.Length == 0) {
sFile = oUri.PathAndQuery;
sFile = Uri.UnescapeDataString(sFile);
StringBuilder sb = new StringBuilder();
for (int i = 0; i < sFile.Length; i++) {
if (Char.IsLetterOrDigit(sFile, i)) sb.Append(sFile.Substring(i, 1));
else sb.Append("_");
}
sFile = sb.ToString();
sFile = Regex.Replace(sFile, @"_+", "_");
sFile = sFile.Trim(new Char[] {'_', ' '});
if (sFile.Length == 0) sFile = "page";
if (!sFile.ToLower().EndsWith(".htm") && !sFile.ToLower().EndsWith(".html")) sFile += ".htm";
}
if (Path.GetExtension(sFile).Length == 0) sFile += ".htm";
sFile = sFile.Replace("_", " ").Trim();
return sFile;
} // GetFileFromUri method

public static string[] MultiCheckDialog(string sTitle, string[] aValues, int[] aSelect, bool bSort, int iIndex) {
return Dialog.MultiCheck(sTitle, aValues, aSelect, bSort, iIndex);
} // MultiCheckDialog method

public static string Key2String(Keys keyData) {
return TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(keyData);
} // Key2String method

public static Keys String2Key(string sKey) {
return (Keys) TypeDescriptor.GetConverter(typeof(Keys)).ConvertFromString(sKey);
} // String2Key method

public static void Swap(ref int i1, ref int i2) {
int i  = i1;
i1 = i2;
i2 = i;
} // Swap method

public static string ConvertQuotes(string sText) {
string sReturn = sText.Replace(@"?", @"""");
sReturn  = sReturn.Replace(@"?", @"""");
sReturn  = sReturn.Replace(@"-", @"-");
sReturn  = sReturn.Replace(@"?", @"...");
sReturn  = sReturn.Replace(@"?", @"'");
sReturn  = sReturn.Replace(Lbc.Code2String(65533), @"'");
return sReturn;
} // ConvertQuotes method

public static string Convert2Ascii(string sText) {
int iLength = sText.Length;
for (int i = iLength - 1; i >= 0; i--) {
if ((int) sText[i] > 127) sText = sText.Remove(i, 1);
}
return sText;
} //Convert2Ascii method

public static string Convert2MacLineBreak(string sText) {
//Convert to Macintosh line break, \r;
string sMatch, sReplace;

sMatch = "\r\n";
sReplace = "\r";
sText = Regex.Replace(sText, sMatch, sReplace);
sMatch = "\n";
sText = Regex.Replace(sText, sMatch, sReplace);
return sText;
} // Convert2MacLineBreak metod

public static string Convert2UnixLineBreak(string sText) {
//Convert to Unix line break, \n;
string sMatch, sReplace;
sMatch = "\r\n";
sReplace = "\n";
sText = Regex.Replace(sText, sMatch, sReplace);
sMatch = "\r";
sText = Regex.Replace(sText, sMatch, sReplace);
return sText;
} // Convert2UnixLineBreak method

public static string Convert2WinLineBreak(string sText){
//Convert to standard Windows line break, \r\nVar;
string sMatch, sReplace;
sMatch = "\r\n";
sReplace = "\n";
sText = Regex.Replace(sText, sMatch, sReplace);
sMatch = "\r";
sText = Regex.Replace(sText, sMatch, sReplace);
sMatch = "\n";
sReplace = "\r\n";
sText = Regex.Replace(sText, sMatch, sReplace);
return sText;
} // Convert2WinLineBreakMethod

public static char Code2Char(int iCode) {
return (char) iCode;
} // Code2Char method

public static string Code2String(int iCode) {
return Code2Char(iCode).ToString();
} // Code2String method

public static void ActivateTitle(string sTitle) {
object oShell = CreateObject("WScript.Shell");
CallMethod(oShell, "AppActivate", sTitle);
} // ActivateTitle method

[DllImport("user32.dll")]
public static extern int AttachThreadInput(int iThread1, int iThread2, int iAttach);

[DllImport("user32.dll")]
public static extern IntPtr GetActiveWindow();

[DllImport("user32.dll")]
public static extern int BringWindowToTop(IntPtr h);

[DllImport("user32.dll")]
public static extern int ShowWindow(IntPtr h, int iState);

[DllImport("kernel32.dll")]
public static extern int GetCurrentThreadId();

[DllImport("user32.dll")]
public static extern int GetWindowThreadProcessId(IntPtr h, int iProcess);

public static bool ForceWindow(IntPtr h) {
int iForegroundThread = GetWindowThreadProcessId((IntPtr) GetForegroundWindow(), 0);
int iAppThread = GetCurrentThreadId();
if (iForegroundThread == iAppThread) {
BringWindowToTop(h);
ShowWindow(h,3);
}
else {
AttachThreadInput(iForegroundThread, iAppThread, 1);
BringWindowToTop(h);
ShowWindow(h,3);
AttachThreadInput(iForegroundThread, iAppThread, 0);
}

return GetActiveWindow() == h;
} // ForceWindow method

} // Lbc class

public class ListForm : Form {

public ListBox lst;
public DataTable tbl;
public BindingSource bs ;
public string Filter;
public DataTable tblDefault = null;
public int CheckFirst = -1;
public int CheckLast = -1;

protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
ListBox lst = this.lst;
bool bChecked = false;
if (lst is CheckedListBox) bChecked = true;

switch (keyData) {
case Keys.Alt | Keys.A :
Lbc.Say("Alpha order");
bs.Sort = "Item asc";
bs.Position = 0;
return true;
case Keys.Alt | Keys.Shift | Keys.A :
Lbc.Say("Reverse alpha order");
bs.Sort = "Item desc";
bs.Position = 0;
return true;
case Keys.Alt | Keys.D :
Lbc.Say("Default order");
if (this.tblDefault == null) {
this.tblDefault = new DataTable();
this.tblDefault.Columns.Add("Item", typeof(string));
this.tblDefault.Columns.Add("Value", typeof(string));
for (int i = 0; i < tbl.Rows.Count; i++)  this.tblDefault.Rows.Add(tbl.Rows[i][0].ToString(), tbl.Rows[i][1].ToString());
}

tbl = this.tblDefault;
bs.Sort = "";
bs.Position = 0;
return true;
case Keys.Alt | Keys.Shift | Keys.D :
Lbc.Say("Reverse default order");
if (this.tblDefault == null) {
this.tblDefault = new DataTable();
this.tblDefault.Columns.Add("Item", typeof(string));
this.tblDefault.Columns.Add("Value", typeof(string));
for (int i = 0; i < tbl.Rows.Count; i++)  this.tblDefault.Rows.Add(tbl.Rows[i][0].ToString(), tbl.Rows[i][1].ToString());
}

DataTable tblNew = new DataTable();
tblNew.Columns.Add("Item", typeof(string));
tblNew.Columns.Add("Value", typeof(string));
for (int i = this.tblDefault.Rows.Count -1; i >= 0; i--) tblNew.Rows.Add(this.tblDefault.Rows[i][0].ToString(), tblDefault.Rows[i][1].ToString());
tbl = tblNew;
//bs = new BindingSource();
bs.DataSource = tbl;
//this.BS = bs;
//bs.ResetBindings();
bs.Sort = "";
bs.Position = 0;
return true;
case Keys.Alt | Keys.Delete :
Lbc.Say((bs.Position + 1) + " of " + tbl.DefaultView.Count);
return true;
case Keys.Shift | Keys.Space :
if (bChecked) {
int iChecked = ((CheckedListBox) lst).CheckedItems.Count;
if (iChecked == 0) Lbc.Say("No items checked!");
else Lbc.Say("Checked" + iChecked);
List<int> listChecked = new List<int>();
foreach (int i in ((CheckedListBox) lst).CheckedIndices) listChecked.Add(i);
listChecked.Sort();
foreach (int i in listChecked) Lbc.Say(tbl.DefaultView[i][0].ToString());
}
else {
Lbc.Say("Selected");
foreach (int i in lst.SelectedIndices) Lbc.Say(tbl.DefaultView[i][0].ToString());
}
return true;
case Keys.Space :
//if (!bChecked || this.ActiveControl is Button) return base.ProcessCmdKey (ref msg, keyData);
if (!bChecked || !(this.ActiveControl is ListBox)) return base.ProcessCmdKey (ref msg, keyData);

{
int i = bs.Position;
bool b = ((CheckedListBox) lst).GetItemChecked(i);
((CheckedListBox) lst).SetItemChecked(i, !b);
return true;
}
case Keys.Control | Keys.Home :
if (!bChecked || !(this.ActiveControl is ListBox)) return base.ProcessCmdKey (ref msg, keyData);

int iStart = -1;
for (int i = 0; i < tbl.DefaultView.Count; i++) {
if (((CheckedListBox) lst).GetItemChecked(i)) {
iStart = i;
break;
}
}

if (iStart >= 0) bs.Position = iStart;
else Lbc.Say("Not found!");
return true;
case Keys.Control | Keys.End :
if (!bChecked || !(this.ActiveControl is ListBox)) return base.ProcessCmdKey (ref msg, keyData);

int iEnd = -1;
for (int i = tbl.DefaultView.Count - 1; i >= 0; i--) {
if (((CheckedListBox) lst).GetItemChecked(i)) {
iEnd = i;
break;
}
}

if (iEnd >= 0) bs.Position = iEnd;
else Lbc.Say("Not found!");
return true;
case Keys.Control | Keys.Down :
if (!bChecked || !(this.ActiveControl is ListBox)) return base.ProcessCmdKey (ref msg, keyData);

int iNext = -1;
for (int i = bs.Position + 1; i < tbl.DefaultView.Count; i++) {
if (((CheckedListBox) lst).GetItemChecked(i)) {
iNext = i;
break;
}
}

if (iNext >= 0) bs.Position = iNext;
else Lbc.Say("Not found!");
return true;
case Keys.F8 :
case Keys.Shift | Keys.F8 :
case Keys.Alt | Keys.Shift | Keys.F8 :
case Keys.Shift | Keys.Clear :
case Keys.Alt | Keys.Shift | Keys.Clear :
case Keys.Shift | Keys.Down :
case Keys.Alt | Keys.Shift | Keys.Down :
case Keys.Shift | Keys.Up :
case Keys.Alt | Keys.Shift | Keys.Up :
case Keys.Shift | Keys.End :
case Keys.Alt | Keys.Shift | Keys.End :
case Keys.Shift | Keys.Home :
case Keys.Alt | Keys.Shift | Keys.Home :
if (!bChecked || !(this.ActiveControl is ListBox)) return base.ProcessCmdKey (ref msg, keyData);


bool bState;
int iFirst, iLast;
int iAfter = bs.Position;
string sKey = Lbc.Key2String(keyData);

if (keyData == Keys.F8) {
Lbc.Say("Start Check or Uncheck");
this.CheckFirst = iAfter;
return true;
}
else if (keyData == (Keys.Shift | Keys.F8)) {
Lbc.Say("Complete Check");
bState = true;
iFirst = this.CheckFirst;
iLast = iAfter;
}
else if (keyData == (Keys.Alt | Keys.Shift | Keys.F8)) {
Lbc.Say("Complete Uncheck");
bState = false;
iFirst = this.CheckFirst;
iLast = iAfter;
}
else {
if (sKey.IndexOf("Alt+") >= 0) bState = false;
else bState = true;

if (sKey.IndexOf("+End") >= 0) {
iLast = tbl.DefaultView.Count - 1;
iAfter = iLast;
}
else iLast = iAfter;

if (sKey.IndexOf("+Home") >= 0) {
iFirst = 0;
iAfter = iFirst;
}
else iFirst = iAfter;

if (sKey.IndexOf("+Up") >= 0) iAfter--;
if (sKey.IndexOf("+Down") >= 0) iAfter++;

}

if (iFirst > iLast) Lbc.Swap(ref iFirst, ref iLast);
for (int iPosition = iFirst; iPosition <= iLast; iPosition ++) ((CheckedListBox) lst).SetItemChecked(iPosition, bState);
if (iAfter != bs.Position && iAfter >=0 && iAfter < tbl.DefaultView.Count) bs.Position = iAfter;
return true;
case Keys.Control | Keys.Up :
if (!bChecked || !(this.ActiveControl is ListBox)) return base.ProcessCmdKey (ref msg, keyData);

int iPrevious = -1;
for (int i = bs.Position - 1; i >= 0; i--) {
if (((CheckedListBox) lst).GetItemChecked(i)) {
iPrevious = i;
break;
}
}

if (iPrevious >= 0) bs.Position = iPrevious;
else Lbc.Say("Not found!");
return true;
case Keys.Control | Keys.A :
if (!bChecked || !(this.ActiveControl is ListBox)) return base.ProcessCmdKey (ref msg, keyData);

if (bChecked) {
Lbc.Say("Check All");
for (int i = 0; i < tbl.DefaultView.Count; i++) ((CheckedListBox) lst).SetItemChecked(i, true);
}
return true;
case Keys.Control | Keys.Shift | Keys.A :
if (!bChecked || !(this.ActiveControl is ListBox)) return base.ProcessCmdKey (ref msg, keyData);

if (bChecked) {
Lbc.Say("Uncheck All");
for (int i = 0; i < tbl.DefaultView.Count; i++) ((CheckedListBox) lst).SetItemChecked(i, false);
}
return true;
case Keys.Control | Keys.F :
case Keys.Control | Keys.Shift | Keys.F :
string sFilterSql = "";
string sFilter = "";
if (keyData == (Keys.Control | Keys.Shift | Keys.F)) Lbc.Say("Clear filter");
else {
Dialog.hashFilter.TryGetValue(this.Text, out sFilter);
sFilter = Dialog.Input("Filter", "Text", sFilter);
if (sFilter.Length == 0) return true;
//sFilterSql = "Item like '" + sFilter + "'";
sFilterSql = GetFilterSql(sFilter);
}

string sTemp = bs.Filter;
try {
bs.Filter = sFilterSql;
this.Filter = sFilter;
}
catch (Exception ex) {
Dialog.Show("Error", ex.Message);
bs.Filter = sTemp;
return true;
}

bs.Position = 0;
Lbc.Say(Lbc.Pluralize(bs.Count, "item"));

//if (keyData == (Keys.Control | Keys.F)) {
if (Dialog.hashFilter.ContainsKey(this.Text)) Dialog.hashFilter.Remove(this.Text);
if (sFilter.Trim().Length > 0) Dialog.hashFilter.Add(this.Text, sFilter);
//}
return true;
case Keys.Control | Keys.J :
case Keys.Alt | Keys.J :
string sTitle = this.Text;
string sJump = "";
Dialog.hashJump.TryGetValue(sTitle, out sJump);
if (keyData == (Keys.Control | Keys.J)) {
sJump = Dialog.Input("Jump", "Text", sJump);
if (sJump.Length == 0) return true;
}

int iIndex = bs.Position;
if (keyData == (Keys.Alt | Keys.J) || sJump == Dialog.Jump) iIndex++;
else iIndex = 0;
if (Dialog.hashJump.ContainsKey(sTitle)) Dialog.hashJump.Remove(sTitle);
Dialog.hashJump.Add(sTitle, sJump);

int iCount = tbl.DefaultView.Count;
//while (iIndex < iCount && tbl.DefaultView[iIndex].ToString().ToLower().IndexOf(sJump) == -1) iIndex ++;
/*
while (iIndex < iCount && tbl.DefaultView[iIndex].ToString().ToLower().IndexOf(sJump) == -1) {
//Lbc.Say(iIndex);
iIndex ++;
}
*/
while (iIndex < iCount && tbl.DefaultView[iIndex][0].ToString().ToLower().IndexOf(sJump) == -1) {
iIndex ++;
}
//if (iIndex < iCount) bs.Position = iIndex;
if (iIndex < iCount) bs.Position = iIndex;
else Lbc.Say("Not found!");
//lst.Update();
return true;
}

return base.ProcessCmdKey (ref msg, keyData);
} // ProcessCmdKey handler

public string GetFilterSql(string sText) {
if (sText == null) sText = "";
sText = sText.Trim();
if (sText == "" || sText == "*") return "";
string[] aFilters = sText.Split('|');
string s = "";
for (int i =0; i < aFilters.Length; i++) {
if (i == 0) s += "(";
string[] a = aFilters[i].Split('*');
for (int j = 0; j < a.Length; j++) {
string sPrefix = "";
string sSuffix = "";
if (j == 0) s += " (";
if (a[j].Length > 0) {
if (j > 0) sPrefix = "*";
if (j < a.Length - 1) sSuffix = "*";
s += "Item like '" + sPrefix + a[j] + sSuffix + "'";
}

if (j == a.Length - 1) s += ") ";
else s += " and ";
}
if (i == aFilters.Length - 1) s+=")";
else s += " or ";
}

s = s.Replace("( and ", "(");
s = s.Replace(" and )", ")");
s = s.Replace("**", "*");
s = s.Replace("  ", " ");
s = s.Replace("( ", "(");
s = s.Replace(" )", ")");
s = s.Trim();
return s;
} // GetFilterSql method

} // ListForm class

public class Dialog {
public static string Jump = "";
public static Dictionary<string, string> hashItem = new Dictionary<string, string>();
public static Dictionary<string, string> hashFilter = new Dictionary<string, string>();
public static Dictionary<string, string> hashSort = new Dictionary<string, string>();
public static Dictionary<string, string> hashJump = new Dictionary<string, string>();

public static string OpenFile(string sTitle, string sPath) {
string sReturn = "";
string sDir;

OpenFileDialog dlg = new OpenFileDialog();
if (sTitle.Length > 0) dlg.Title = sTitle;
if (File.Exists(sPath)) {
dlg.FileName = sPath;
sDir = Path.GetDirectoryName(sPath);
}
else sDir = sPath;

if (!Directory.Exists(sDir)) sDir = Directory.GetCurrentDirectory();
dlg.InitialDirectory = sDir;
dlg.Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt|Rich Text Format files (*.rtf)|*.rtf";
dlg.FilterIndex = 1;
dlg.ValidateNames = true;
dlg.CheckPathExists = true;

if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.FileName;
dlg.Dispose();
return sReturn;
} // OpenFile method

public static string SaveFile(string sTitle, string sPath) {
string sReturn = "";
string sDir;

SaveFileDialog dlg = new SaveFileDialog();
if (sTitle.Length > 0) dlg.Title = sTitle;
if (Directory.Exists(sPath)) sDir = sPath;
else {
dlg.FileName = sPath;
sDir = Path.GetDirectoryName(sPath);
}

if (Directory.Exists(sDir)) dlg.InitialDirectory = sDir;
dlg.Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt|Rich Text Format files (*.rtf)|*.rtf";
dlg.FilterIndex = 1;
dlg.CheckPathExists = true;
dlg.SupportMultiDottedExtensions = true;

dlg.CreatePrompt = false;
dlg.ValidateNames = true;
dlg.AddExtension = true;
//dlg.AddExtension = false;
//dlg.DefaultExt = "txt";
//dlg.DefaultExt = App.ReadOption("ExtensionDefault", "");

if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.FileName;
dlg.Dispose();
return sReturn;
} // SaveFile method

public static string OldInput(string sTitle, string sLabel, string sValue) {
return Interaction.InputBox(sLabel, sTitle, sValue, -1, -1);
} // Input method

public static string Input(string sTitle, string sLabel, string sValue) {
string[] aLabel = new string[] {sLabel};
string[] aValue = new string[] {sValue};
string[] aReturn = MultiInput(sTitle, aLabel, aValue);
//string sReturn = aReturn[0];
string sReturn = "";
if (aReturn != null && aReturn.Length > 0) sReturn = aReturn[0];
return sReturn;
} // Input method

public static string[] MultiInput(string sTitle, string[] aLabel, string[] aValue) {
Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
frm.AutoScroll = true;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
//flpMain.AutoScroll = true;
flpMain.FlowDirection = FlowDirection.TopDown;

TableLayoutPanel tlpFields = new TableLayoutPanel();
tlpFields.SuspendLayout();
tlpFields.Anchor = AnchorStyles.None;
tlpFields.AutoSize = true;
tlpFields.AutoSizeMode = AutoSizeMode.GrowAndShrink;
//tlpFields.AutoScroll = true;

tlpFields.ColumnCount = 2;

for (int i = 0; i < tlpFields.ColumnCount; i++) {
tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
}

tlpFields.RowCount = aLabel.Length;

for (int i = 0; i < tlpFields.RowCount; i++) {
tlpFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = aLabel[i] + ":";
lbl.AccessibleName = lbl.Text.Replace("&", "");
TextBox txt = new TextBox();
txt.Width *= 2;
txt.Text = aValue[i];
txt.AccessibleName = lbl.AccessibleName;
txt.SelectAll();
tlpFields.Controls.AddRange(new Control[] {lbl, txt});
}
tlpFields.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
//flpButtons.AutoScroll = true;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

StringBuilder sb = new StringBuilder();
btnOK.Click += delegate(object o, EventArgs e) {
foreach (Control ctl in tlpFields.Controls) {
if (ctl.GetType() == typeof(TextBox)) sb.Append(ctl.Text + "\n");
}
frm.Close();
};

Button btnCancel = new Button();
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
btnCancel.Click += delegate(object o, EventArgs e) {/*Lbc.Say("Cancel");*/ frm.Close();};

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {tlpFields, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.ShowDialog();
frm.Dispose();

string s = sb.ToString();
//s = s.TrimEnd('\n');
if (s.Length > 0) s = s.Substring(0, s.Length - 1);
string[] aReturn = {};
if (s.Length > 0) aReturn = s.Split('\n');
return aReturn;
} // MultiInput method

public static string Pick(string sTitle, string[] aValue, bool bSort) {
string[] aDisplay = null;
int iIndex = 0;
return Pick(sTitle, aValue, aDisplay, bSort, iIndex);
} // Pick method

public static string[] MultiPick(string sTitle, string[] aValues, int[] aSelect, bool bSort) {
List<string> listResults = new List<string>();

ListForm frm = new ListForm();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

ListBox lst = new ListBox();
frm.lst = lst;
lst.SelectionMode = SelectionMode.MultiSimple;
if (bSort) lst.Sorted = true;
lst.Items.AddRange(aValues);

for (int i = 0; i < aSelect.Length; i ++) {
lst.SetSelected(aSelect[i], true);
}

flpInput.Controls.AddRange(new Control[] {lst});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (int i in lst.SelectedIndices) {
listResults.Add(lst.Items[i].ToString());
}
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel"); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) {frm.Activate();};
//frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
string[] aResults = listResults.ToArray();
return aResults;

} // MultiPick method

public static string[] MultiCheck(string sTitle, string[] aValues, int[] aSelect, bool bSort, int iIndex) {
string[] aDisplay = null;
return MultiCheck(sTitle, aDisplay, aValues, aSelect, bSort, iIndex);
} // MultiCheck method

public static string[] MultiCheck(string sTitle, string[] aDisplay, string[] aValues, int[] aSelect, bool bSort, int iIndex) {
List<string> listResults = new List<string>();

ListForm frm = new ListForm();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

CheckedListBox lst = new CheckedListBox();
frm.lst = lst;
lst.Sorted = false;
//lst.SelectionMode = SelectionMode.MultiSimple;
lst.SelectionMode = SelectionMode.One;

string[] aTemp = (string[]) aValues.Clone();
if (aDisplay == null) {
if (bSort) Array.Sort(aValues, new CaseInsensitiveComparer());
aDisplay = (string[]) aValues.Clone();
}
else if (bSort) Array.Sort(aDisplay, aValues);

DataTable tbl = new DataTable();
frm.tbl = tbl;
tbl.Columns.Add("Item", typeof(string));
tbl.Columns.Add("Value", typeof(string));
BindingSource bs = new BindingSource();
frm.bs = bs;
bs.DataSource = tbl;
lst.DataSource = bs;
lst.DisplayMember = "Item";
for (int i = 0; i < aDisplay.Length; i++) tbl.Rows.Add(aDisplay[i], aValues[i]);

/*
if (bSort) lst.Sorted = true;
lst.Items.AddRange(aValues);
lst.SelectedIndex = iIndex;
*/

//for (int i = 0; i < aSelect.Length; i ++) lst.SetItemChecked(aSelect[i], true);
//bs.Position = iIndex;

flpInput.Controls.AddRange(new Control[] {lst});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
//foreach (int i in lst.SelectedIndices) {
//foreach (int i in lst.CheckedIndices) {
//listResults.Add(lst.Items[i].ToString());

foreach (int i in lst.CheckedIndices) listResults.Add(((DataRowView) bs[i])[1].ToString());
frm.Close();
for (int i = 0; i < aTemp.Length; i++) aValues[i] = aTemp[i];
};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Lbc.Say("Cancel"); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) {frm.Activate();};
//frm.Shown += delegate(object sender, EventArgs e) {SetForegroundWindow((int) (int) frm.Handle); };
frm.Load += delegate(object sender, EventArgs e) {
if (iIndex == 0) {
for (int i = 0; i < aSelect.Length; i ++) lst.SetItemChecked(aSelect[i], true);

string sFilter = "";
if (hashFilter.TryGetValue(sTitle, out sFilter) && sFilter != null && sFilter != ""){
string sFilterSql = frm.GetFilterSql(sFilter);
bs.Filter = sFilterSql;
if (bs.Count == 0) bs.Filter = "";
else Lbc.Say("Filter " + sFilter);
}

string sSort = "";
if (hashSort.TryGetValue(sTitle, out sSort) && sSort != null && sSort != ""){
bs.Sort = sSort;
if (sSort.EndsWith(" asc")) Lbc.Say("Alpha order");
else if (sSort.EndsWith(" desc")) Lbc.Say("Reverse alpha order");
}

string sItem = "";
if (hashItem.TryGetValue(sTitle, out sItem)) {
//iIndex = lst.FindStringExact(sItem);
iIndex = -1;
for (int i = 0; i < bs.Count; i++) {
DataRowView row = (DataRowView) bs[i];
if (row[1].ToString() == sItem) {
iIndex = i;
break;
}
}
} // iIndex == 0

if (iIndex == -1) iIndex = 0;
if (iIndex > 0) Lbc.Say("Item " + (iIndex + 1).ToString());
//lst.SelectedIndex = iIndex;
bs.Position = iIndex;
}
};

frm.ShowDialog();
frm.Dispose();
string[] aResults = listResults.ToArray();

if (aResults.Length > 0) {
if (hashFilter.ContainsKey(sTitle)) hashFilter.Remove(sTitle);
string sFilter = frm.Filter;
hashFilter.Add(sTitle, sFilter);

if (hashSort.ContainsKey(sTitle)) hashSort.Remove(sTitle);
string sSort = bs.Sort;
hashSort.Add(sTitle, sSort);

if (hashItem.ContainsKey(sTitle)) hashItem.Remove(sTitle);
//sItem = lst.SelectedItem.ToString();
string sItem = ((DataRowView) bs.Current)[1].ToString();
hashItem.Add(sTitle, sItem);
}

return aResults;

} // MultiCheck method

public static string Pick(string sTitle, string[] aValue, bool bSort, int iIndex) {
string[] aDisplay = null;
return Pick(sTitle, aValue, aDisplay, bSort, iIndex);
} // Pick method

public static string Pick(string sTitle, string[] aValue, string[] aDisplay, bool bSort, int iIndex) {
string sReturn = "";

ListForm frm = new ListForm();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
frm.AutoScroll = true;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
//flpMain.AutoScroll = true;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
//flpInput.AutoScroll = true;
flpInput.FlowDirection = FlowDirection.LeftToRight;

ListBox lst = new ListBox();
frm.lst = lst;
lst.Sorted = false;

string[] aTemp = (string[]) aValue.Clone();
if (aDisplay == null) {
if (bSort) Array.Sort(aValue, new CaseInsensitiveComparer());
aDisplay = (string[]) aValue.Clone();
}
else if (bSort) Array.Sort(aDisplay, aValue);

DataTable tbl = new DataTable();
frm.tbl = tbl;
tbl.Columns.Add("Item", typeof(string));
tbl.Columns.Add("Value", typeof(string));
BindingSource bs = new BindingSource();
frm.bs = bs;
bs.DataSource = tbl;
lst.DataSource = bs;
lst.DisplayMember = "Item";
for (int i = 0; i < aDisplay.Length; i++) tbl.Rows.Add(aDisplay[i], aValue[i]);

flpInput.Controls.Add(lst);
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
//flpButtons.AutoScroll = true;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

btnOK.Click += delegate(object o, EventArgs e) {

sReturn = ((DataRowView) bs.Current)[1].ToString();
frm.Close();
for (int i = 0; i < aTemp.Length; i++) aValue[i] = aTemp[i];
};

Button btnCancel = new Button();
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
btnCancel.Click += delegate(object o, EventArgs e) {/*Lbc.Say("Cancel");*/ frm.Close();};

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();

/*
if (iIndex == 0) {
string sItem = "";
if (hashItem.TryGetValue(sTitle, out sItem)) {
if (aDisplay == null) iIndex = lst.Items.IndexOf(sItem);
else {
iIndex = Array.IndexOf(aValue, sItem);
if (iIndex >= 0) iIndex = lst.FindStringExact(aDisplay[iIndex]);
}
}
}

if (iIndex == -1) iIndex = 0;
if (iIndex > 0) Lbc.Say("Item " + (iIndex + 1).ToString());
//lst.SelectedIndex = iIndex;
bs.Position = iIndex;
*/

frm.Load += delegate(object sender, EventArgs e) {
if (iIndex == 0) {
string sFilter = "";
if (hashFilter.TryGetValue(sTitle, out sFilter) && sFilter != null && sFilter != ""){
string sFilterSql = frm.GetFilterSql(sFilter);
bs.Filter = sFilterSql;
if (bs.Count == 0) bs.Filter = "";
else Lbc.Say("Filter " + sFilter);
}

string sSort = "";
if (hashSort.TryGetValue(sTitle, out sSort) && sSort != null && sSort != ""){
bs.Sort = sSort;
if (sSort.EndsWith(" asc")) Lbc.Say("Alpha order");
else if (sSort.EndsWith(" desc")) Lbc.Say("Reverse alpha order");
}

string sItem = "";
if (hashItem.TryGetValue(sTitle, out sItem)) {
//iIndex = lst.FindStringExact(sItem);
iIndex = -1;
for (int i = 0; i < bs.Count; i++) {
DataRowView row = (DataRowView) bs[i];
if (row[1].ToString() == sItem) {
iIndex = i;
break;
}
}
//Dialog.Show(iIndex, sItem);
// Try brace here
}
} // iIndex == 0

if (iIndex == -1) iIndex = 0;
if (iIndex > 0) Lbc.Say("Item " + (iIndex + 1).ToString());
//lst.SelectedIndex = iIndex;
bs.Position = iIndex;
// Try disabling brace
// }
};

// Clipboard.SetText("Index " + iIndex);
// bs.Position = iIndex;
frm.ShowDialog();
frm.Dispose();

if (sReturn.Length > 0) {
if (hashFilter.ContainsKey(sTitle)) hashFilter.Remove(sTitle);
string sFilter = frm.Filter;
hashFilter.Add(sTitle, sFilter);

if (hashSort.ContainsKey(sTitle)) hashSort.Remove(sTitle);
string sSort = bs.Sort;
hashSort.Add(sTitle, sSort);

if (hashItem.ContainsKey(sTitle)) hashItem.Remove(sTitle);
hashItem.Add(sTitle, sReturn);
}
return sReturn;
} // Pick method

public static string Confirm(string sTitle, string sText, string sDefault) {
MessageBoxDefaultButton defaultButton;
if (sDefault.ToLower() == "n") defaultButton = MessageBoxDefaultButton.Button2;
else defaultButton = MessageBoxDefaultButton.Button1;

switch (MessageBox.Show(sText, sTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, defaultButton)) {
case DialogResult.Yes :
//Lbc.Say("Yes");
return "Y";
case DialogResult.No :
//Lbc.Say("No");
return "N";
}
/*Lbc.Say("Cancel");*/
return "";
} // Confirm method

public static void Show(object oText) {
Show("Show", oText);
} // Show method

public static void Show(object oTitle, object oText) {
string sTitle = oTitle.ToString();
string sText = oText.ToString();
if (oTitle is bool) sTitle = ((bool) oTitle) ? "true" : "false";
if (oText is bool) sText = (bool) oText ? "true" : "false";
MessageBox.Show(oText.ToString(), oTitle.ToString());
} // Show method

public static void Properties(string sPath) {
Lbc.InvokeVerb(sPath, "P&roperties");
} // Properties method

public static string Choose (string sTitle, string sText, string[] aButtons, int iDefault) {
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

if (sText !="") {
//Lbc.Say(sText);
Label lbl = new Label();
lbl.AutoSize = true;
int iLines = sText.Split('\n').Length;
lbl.AutoSize = false;
lbl.Width = 200;
lbl.Height = 16 * iLines + 16;
lbl.Margin = new Padding(3, 3, 3, 3);
lbl.Text = sText;
lbl.AccessibleName = lbl.Text.Replace("&", "");
lbl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
flpMain.Controls.Add(lbl);
}

for (int i = 0; i < aButtons.Length; i++) {
Button btn = new Button();
btn.Click += delegate(object o, EventArgs e) {sResult = btn.Text; frm.Close();};
btn.Text = aButtons[i];
btn.AccessibleName = aButtons[i].Replace("&", "");
btn.AutoSize = false;
btn.Width = 200;
btn.Anchor = AnchorStyles.None;
flpMain.Controls.Add(btn);
}

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { /*Lbc.Say("Cancel");*/ frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
btnCancel.AutoSize = false;
btnCancel.Width = 200;
//btnCancel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
flpMain.Controls.Add(btnCancel);

flpMain.ResumeLayout();

frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
//if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + aButtons.Length + ")";
frm.Text = sTitle;
frm.Controls.Add(flpMain);

int iButton = 0;
foreach (Control ctl in flpMain.Controls) {
if (ctl.GetType() == typeof(Button)) {
if (iButton == iDefault) ctl.Select();
iButton++;
}
}

frm.ResumeLayout();
//frm.Shown += delegate(object sender, EventArgs e) { Lbc.JFWSayString(sText); };
frm.Shown += delegate(object sender, EventArgs e) {Lbc.SetForegroundWindow((int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
Lbc.Say(sResult.Replace("&", ""));
return sResult;
} // Choose method

public static object[] PickAndChoose(string sTitle, object[] aValue, string[] aDisplay, string[] aButton, bool bSort, int iIndex) {
object[] aResult = {};

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpData = new FlowLayoutPanel();
flpData.SuspendLayout();
flpData.Anchor = AnchorStyles.None;
flpData.AutoSize = true;
flpData.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpData.FlowDirection = FlowDirection.LeftToRight;

ListBox lst = new ListBox();
lst.Sorted = false;
if (aDisplay == null) lst.Items.AddRange(aValue);
else {
for (int i = 0; i < aDisplay.Length; i++) {
lst.Items.Add(aDisplay[i]);
Support.SetItemData(lst, i, i);
}
}
if (bSort) lst.Sorted = true;
lst.SelectedIndex = iIndex;

flpData.Controls.Add(lst);
flpData.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

for (int i = 0; i < aButton.Length; i++) {
Button btn = new Button();
btn.Click += delegate(object o, EventArgs e) {
object oItem;
if (aDisplay == null) oItem = lst.Text;
else {
int iItem = lst.SelectedIndex;
int iValue = Support.GetItemData(lst, iItem);
oItem = aValue[iValue];
}
aResult = new object[] {oItem, btn.Text};
Lbc.Say(btn.Text.Replace("&", ""));
frm.Close();
};

btn.Text = aButton[i];
btn.AccessibleName = aButton[i].Replace("&", "");
//btn.AutoSize = false;
//btn.Width = 200;
//btn.Anchor = AnchorStyles.None;
flpButtons.Controls.Add(btn);
}

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { /*Lbc.Say("Cancel");*/ frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
//btnCancel.AutoSize = false;
//btnCancel.Width = 200;
//btnCancel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
flpButtons.Controls.Add(btnCancel);

flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpData, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = (Button) flpButtons.Controls[0];
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + aValue.Length + ")";
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {Lbc.SetForegroundWindow((int) frm.Handle); };
frm.ShowDialog();
frm.Dispose();
return aResult;
} // PickAndChoose method

public static object[] GetFont(Font font, Color color) {
//ColorDialog d = new ColorDialog();
//d.ShowDialog();
FontDialog dlg = new FontDialog();
dlg.FontMustExist = true;
dlg.ShowColor = true;
dlg.Font = font;
dlg.Color = color;
object[] aReturn = {};
if(dlg.ShowDialog() == DialogResult.OK) aReturn = new object[] {dlg.Font, dlg.Color};
dlg.Dispose();
return aReturn;
} // GetFont method

public static string OpenFolder(string sTitle, string sLabel, string sValue) {
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabel + ":";
TextBox txt = new TextBox();
txt.Width *= 2;
txt.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
txt.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
txt.Text = sValue;
txt.AccessibleName = lbl.Text.Replace("&", "");
txt.GotFocus += delegate(object o, EventArgs e) {txt.SelectAll();};

Button btnBrowse = new Button();
btnBrowse.Click += delegate(object o, EventArgs e) { txt.Text = Dialog.BrowseForFolder("", sValue, false); txt.Select();};
btnBrowse.Text = "&Browse";
btnBrowse.AccessibleName = btnBrowse.Text.Replace("&", "");

flpInput.Controls.AddRange(new Control[] {lbl, txt, btnBrowse});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Click += delegate(object o, EventArgs e) {
sResult = txt.Text.Trim();
if (sResult != "" && !Directory.Exists(sResult)) {
string sChoice = Dialog.Confirm("Confirm", "Cannot find folder\n" + sResult + "\nCreate it?", "Y");
if (sChoice == "Y") {
try {
DirectoryInfo di = new DirectoryInfo(sResult);
di.Create();
}
catch (Exception ex) {
Dialog.Show("Error", ex.Message);
}
}
}
if (Directory.Exists(sResult)) frm.Close();
else {
txt.SelectAll();
txt.Select();
}
};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { /*Lbc.Say("Cancel");*/ sResult = ""; frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {Lbc.SetForegroundWindow((int) frm.Handle);};
frm.ShowDialog();
frm.Dispose();
return sResult;
} // GetDirectory method

public static string BrowseForFolder(string sTitle, string sDir) {
bool bNewFolder = false;
return BrowseForFolder(sTitle, sDir, bNewFolder);
} // BrowseForFolder method

public static string BrowseForFolder(string sTitle, string sDir, bool bNewFolder) {
string sReturn = "";
FolderBrowserDialog dlg = new FolderBrowserDialog();
dlg.Description = sTitle;
dlg.ShowNewFolderButton = bNewFolder;
//dlg.RootFolder = sRootFolder;
dlg.SelectedPath = sDir;

if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.SelectedPath;
dlg.Dispose();
return sReturn;
} // BrowseForFolder method

public static string[] PickAndInputDialog(string sTitle, string sLblList, string[] aValues, string sLblInput, string sValue, bool bSort, int iIndex) {
string[] aResults = {};

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

Label lblList = new Label();
lblList.Text = sLblList + ":";
lblList.AccessibleName = lblList.Text.Replace("&", "");

ListBox lst = new ListBox();
if (bSort) lst.Sorted = true;
lst.Items.AddRange(aValues);
lst.SelectedIndex = iIndex;

Label lblInput = new Label();
lblInput.Text = sLblInput + ":";
lblInput.AccessibleName = lblInput.Text.Replace("&", "");
TextBox txt = new TextBox();
txt.Width *= 2;
txt.AccessibleName = lblInput.AccessibleName;
if (lblInput.Text.Contains("Password:")) txt.UseSystemPasswordChar = true;
txt.Text = sValue;

flpInput.Controls.AddRange(new Control[] {lblList, lst, lblInput, txt});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Click += delegate(object o, EventArgs e) {
aResults = new string[] {lst.Text, txt.Text};
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { /*Lbc.Say("Cancel");*/ frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;

flpButtons.Controls.AddRange(new Control[] {btnOK, btnCancel});
flpButtons.ResumeLayout();

flpMain.Controls.AddRange(new Control[] {flpInput, flpButtons});
flpMain.ResumeLayout();

frm.AcceptButton = btnOK;
frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.Controls.Add(flpMain);
frm.ResumeLayout();
frm.Shown += delegate(object sender, EventArgs e) {Lbc.SetForegroundWindow((int) frm.Handle);};
frm.ShowDialog();
frm.Dispose();
return aResults;
} // PickAndInput method

} // Dialog class

/*==============================================================================================================

[ cNetworkDrive - Network Drive API Class ]
-------------------------------------------
Copyright (c)2006 aejw.com
http://www.aejw.com/

Build:         0017 - May 2006
Thanks To:     'jsantos98' from CodeProject.com for his update allowing the local / drive not to specifyed

EULA:          Creative Commons - Attribution-ShareAlike 2.5
http://creativecommons.org/licenses/by-sa/2.5/

Disclaimer:    THIS FILES / SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
OF THE USE OF THIS FILES / SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE. USE AT YOUR OWN RISK.

==============================================================================================================*/

/// <summary>
/// Network Drive Interface
/// </summary>

public class NetworkDrive
{

#region API
[DllImport("mpr.dll")] private static extern int WNetAddConnection2A(ref structNetResource pstNetRes, string psPassword, string psUsername, int piFlags);
[DllImport("mpr.dll")] private static extern int WNetCancelConnection2A(string psName, int piFlags, int pfForce);
[DllImport("mpr.dll")] private static extern int WNetConnectionDialog(int phWnd, int piType);
[DllImport("mpr.dll")] private static extern int WNetDisconnectDialog(int phWnd, int piType);
[DllImport("mpr.dll")] private static extern int WNetRestoreConnectionW(int phWnd, string psLocalDrive);

[StructLayout(LayoutKind.Sequential)]
private struct structNetResource{
public int iScope;
public int iType;
public int iDisplayType;
public int iUsage;
public string sLocalName;
public string sRemoteName;
public string sComment;
public string sProvider;
}

private const int RESOURCETYPE_DISK = 0x1;

//Standard
private const int CONNECT_INTERACTIVE = 0x00000008;
private const int CONNECT_PROMPT = 0x00000010;
private const int CONNECT_UPDATE_PROFILE = 0x00000001;
//IE4+
private const int CONNECT_REDIRECT = 0x00000080;
//NT5 only
private const int CONNECT_COMMANDLINE = 0x00000800;
private const int CONNECT_CMD_SAVECRED = 0x00001000;

#endregion

#region Propertys and options
private bool lf_SaveCredentials = false;
/// <summary>
/// Option to save credentials are reconnection...
/// </summary>
public bool SaveCredentials{
get{return(lf_SaveCredentials);}
set{lf_SaveCredentials=value;}
}
private bool lf_Persistent = false;
/// <summary>
/// Option to reconnect drive after log off / reboot ...
/// </summary>
public bool Persistent{
get{return(lf_Persistent);}
set{lf_Persistent=value;}
}
private bool lf_Force = false;
/// <summary>
/// Option to force connection if drive is already mapped...
/// or force disconnection if network path is not responding...
/// </summary>
public bool Force{
get{return(lf_Force);}
set{lf_Force=value;}
}
private bool ls_PromptForCredentials = false;
/// <summary>
/// Option to prompt for user credintals when mapping a drive
/// </summary>
public bool PromptForCredentials{
get{return(ls_PromptForCredentials);}
set{ls_PromptForCredentials=value;}
}

private string ls_Drive = "s:";
/// <summary>
/// Drive to be used in mapping / unmapping...
/// </summary>
public string LocalDrive{
get{return(ls_Drive);}
set{
if(value.Length>=1) {
ls_Drive=value.Substring(0,1)+":";
}else{
ls_Drive="";
}
}
}
private string ls_ShareName = "\\\\Computer\\C$";
/// <summary>
/// Share address to map drive to.
/// </summary>
public string ShareName{
get{return(ls_ShareName);}
set{ls_ShareName=value;}
}
#endregion

#region Function mapping
/// <summary>
/// Map network drive
/// </summary>
public void MapDrive(){zMapDrive(null, null);}
/// <summary>
/// Map network drive (using supplied Password)
/// </summary>
public void MapDrive(string Password){zMapDrive(null, Password);}
/// <summary>
/// Map network drive (using supplied Username and Password)
/// </summary>
public void MapDrive(string Username, string Password){zMapDrive(Username, Password);}
/// <summary>
/// Unmap network drive
/// </summary>
public void UnMapDrive(){zUnMapDrive(this.lf_Force);}
/// <summary>
/// Check / restore persistent network drive
/// </summary>
public void RestoreDrives(){zRestoreDrive();}
/// <summary>
/// Display windows dialog for mapping a network drive
/// </summary>
public void ShowConnectDialog(Form ParentForm){zDisplayDialog(ParentForm,1);}
/// <summary>
/// Display windows dialog for disconnecting a network drive
/// </summary>
public void ShowDisconnectDialog(Form ParentForm){zDisplayDialog(ParentForm,2);}


#endregion

#region Core functions

// Map network drive
private void zMapDrive(string psUsername, string psPassword){
//create struct data
structNetResource stNetRes = new structNetResource();
stNetRes.iScope=2;
stNetRes.iType=RESOURCETYPE_DISK;
stNetRes.iDisplayType=3;
stNetRes.iUsage=1;
stNetRes.sRemoteName=ls_ShareName;
stNetRes.sLocalName=ls_Drive;
//prepare params
int iFlags=0;
if(lf_SaveCredentials){iFlags+=CONNECT_CMD_SAVECRED;}
if(lf_Persistent){iFlags+=CONNECT_UPDATE_PROFILE;}
if(ls_PromptForCredentials){iFlags+=CONNECT_INTERACTIVE+CONNECT_PROMPT;}
if(psUsername==""){psUsername=null;}
if(psPassword==""){psPassword=null;}
//if force, unmap ready for new connection
if(lf_Force){try{zUnMapDrive(true);}catch{}}
//call and return
int i = WNetAddConnection2A(ref stNetRes, psPassword, psUsername, iFlags);
if(i>0){throw new System.ComponentModel.Win32Exception(i);}
}


// Unmap network drive
private void zUnMapDrive(bool pfForce){
//call unmap and return
int iFlags=0;
if(lf_Persistent){iFlags+=CONNECT_UPDATE_PROFILE;}
int i = WNetCancelConnection2A(ls_Drive, iFlags, Convert.ToInt32(pfForce));
if(i!=0) i=WNetCancelConnection2A(ls_ShareName, iFlags, Convert.ToInt32(pfForce));  //disconnect if localname was null
if(i>0){throw new System.ComponentModel.Win32Exception(i);}
}


// Check / Restore a network drive
private void zRestoreDrive()
{
//call restore and return
int i = WNetRestoreConnectionW(0, null);
if(i>0){throw new System.ComponentModel.Win32Exception(i);}
}

// Display windows dialog
private void zDisplayDialog(Form poParentForm, int piDialog)
{
int i = -1;
int iHandle = 0;
//get parent handle
if(poParentForm!=null)
{
iHandle = poParentForm.Handle.ToInt32();
}
//show dialog
if(piDialog==1)
{
i = WNetConnectionDialog(iHandle, RESOURCETYPE_DISK);
}else if(piDialog==2)
{
i = WNetDisconnectDialog(iHandle, RESOURCETYPE_DISK);
}
if(i>0){throw new System.ComponentModel.Win32Exception(i);}
//set focus on parent form
if(poParentForm!=null) poParentForm.BringToFront();
}


#endregion

} // NetWorkDrive class

} //LayoutByCode namespace
