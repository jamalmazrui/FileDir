// FileDir -- a keyboard-driven file and directory manager for Windows.
//
// Copyright 2006-2026 by Jamal Mazrui
// MIT License. See License.md, which carries the terms in full.
//
// No version or date is written here. Both went stale the moment they were
// typed: this header said "5.0 beta" and "June 17, 2026" through nineteen
// releases, and claimed the modified GPL for months after the change to MIT.
// The version comes from version.txt by way of BuildVersion, generated at build
// time, and the date is the build date of the assembly.

using BrendanGrant.Helpers.FileAssociation;
using ICSharpCode.SharpZipLib.Zip;
//using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
//using System.Timers;
using System.Windows.Forms;
using Tektosyne.NetMail ;
using Tektosyne.Win32Api;

[assembly: AssemblyTitle("FileDir")]
[assembly: AssemblyProduct("FileDir")]
// Single-sourced from AppVersion in FileDir_setup.iss: BuildFileDir.cmd generates
// Version.cs (BuildVersion.Version) from it.  This replaces the wildcard "5.0.*",
// which made the assembly version a build TIMESTAMP unrelated to the release --
// so the program, the installer, and the release tag could never agree.
// AssemblyFileVersion also stamps the Win32 version resource, so the version is
// visible in Explorer's file properties and readable by tools.
[assembly: AssemblyVersion(BuildVersion.Version)]
[assembly: AssemblyFileVersion(BuildVersion.Version)]
[assembly: AssemblyDescription("FileDir file manager")]
[assembly: AssemblyCompany("NonvisualDevelopment.org")]
[assembly: AssemblyCopyright("Copyright 2006 - 2026 by Jamal Mazrui")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]		

namespace FileDir {

class singleInstanceApplication : WindowsFormsApplicationBase {

public singleInstanceApplication() {
this.IsSingleInstance = true;
// Move to startup delegate to speed up subsequent invocations
/*
this.UnhandledException += UnhandledExceptionMethod;

Homer.Util.initJfw();
//Lbc.Show("Path", Environment.GetEnvironmentVariable("PATH").Split('\n'));
*/
} // singleInstanceApplication method

void UnhandledExceptionMethod(object sender, Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs e) {
/*
Exception ex = (Exception) e.Exception;
if (Lbc.ConfirmDialog("Confirm", "Unexpected event!\n" + ex.Message + ".\nExit FileDir?", "N") == "Y") {
e.ExitApplication = true;
}
else {
e.ExitApplication = false;
}
*/

Exception ex = (Exception) e.Exception;
string sMessage = ex.Message;
sMessage += "\n\nStack trace:\n" + ex.StackTrace;
string[] aButtons = {"&Report a Problem", "Copy to Clipboard", "Exit FileDir"};
string sButton = Dialog.Choose("Unexpected Event", sMessage, aButtons, 0);
  switch (sButton) {
case "&Report a Problem" :
// The message is put on the clipboard first, then the issues page opens, so
// the report can simply be pasted in.  This used to mail a legacy address that
// belonged to the original FileDir; the reborn FileDir lives on GitHub, where a
// report can be tracked and answered in public.
App.say("Message copied. Please add the steps that led to it.");
try {
Clipboard.SetText("FileDir " + BuildVersion.Version + "\n\n" + sMessage);
Process.Start("https://github.com/JamalMazrui/FileDir/issues/new");
}
catch {
}
break;
case "Copy to Clipboard" :
Clipboard.SetText(sMessage);
break;
case "Exit FileDir" :
e.ExitApplication = true;
return;
}
e.ExitApplication = false;
} // UnhandledExceptionMethod

protected override void OnCreateMainForm() {
App.frame = new Frame();
this.MainForm = App.frame;
Homer.Util.setForegroundWindow((int) App.frame.Handle);
} // OnCreateMainForm method

protected override bool OnStartup(StartupEventArgs e) {
this.UnhandledException += UnhandledExceptionMethod;
Homer.Util.initJfw();
return true;
} // onStartUp method

protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e) {
Homer.Util.activateTitle(App.frame.Text);
//Homer.Util.setForegroundWindow((int) App.frame.Handle);
Homer.Util.forceWindow(App.frame.TopLevelControl.Handle);
int iCount = e.CommandLine.Count;
if (iCount == 0) return;
string sPath = e.CommandLine[0];
if (sPath == null) return;
else if (iCount == 1 && App.itemExists(sPath)) App.frame.activate_Helper(sPath);
else Process.Start("Explorer.exe", Microsoft.VisualBasic.Interaction.Command());
} // onStartUpNextInstance method


} // singleInstanceApplication class

public static class App {
// Dotted-numeric version used by the Elevate Version command to compare with
// the latest release tag.
// The version number is NOT stored here.  It lives in exactly one place --
// AppVersion in FileDir_setup.iss -- and BuildFileDir.cmd generates Version.cs
// from it at build time, defining BuildVersion.Version.  The program, the
// installer, and the release tag can therefore never disagree.
public const string VersionString = BuildVersion.Version;

public static string fetchLatestReleaseTag(string sOwnerRepo) {
// Return the tag of the latest GitHub release for "owner/repo", e.g. "v5.0.0".
// The public REST API is tried first (no credentials needed); on any failure
// the releases/latest page is fetched and its post-redirect address, which
// ends in the tag, is used instead. Returns "" if neither path yields a tag.
// Homer.Web supplies the User-Agent header and modern TLS that GitHub needs.
string sApiUrl = "https://api.github.com/repos/" + sOwnerRepo + "/releases/latest";
string sFinalUrl = "";
string sJson = Homer.Web.getPage(sApiUrl, out sFinalUrl);
if (sJson.Length > 0) {
Match matchTag = Regex.Match(sJson, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
if (matchTag.Success) return matchTag.Groups[1].Value;
}
string sPageUrl = "https://github.com/" + sOwnerRepo + "/releases/latest";
Homer.Web.getPage(sPageUrl, out sFinalUrl);
string sRedirect = (sFinalUrl == null ? "" : sFinalUrl).TrimEnd('/');
int iSlash = sRedirect.LastIndexOf('/');
if (iSlash >= 0 && iSlash < sRedirect.Length - 1) {
string sTag = sRedirect.Substring(iSlash + 1);
if (!sTag.Equals("latest", StringComparison.OrdinalIgnoreCase)) return sTag;
}
return "";
} // fetchLatestReleaseTag method

public static int compareVersions(string sA, string sB) {
// Compare two dotted-numeric version strings, e.g. "5.0.1" versus "5.0.0".
// Returns a negative value if sA is older, zero if equal, positive if newer.
// Missing trailing parts count as zero (so "5.0" equals "5.0.0"); any part
// that is not an integer falls back to ordinal string comparison.
string[] asA = (sA == null ? "" : sA).Split('.');
string[] asB = (sB == null ? "" : sB).Split('.');
int iCount = Math.Max(asA.Length, asB.Length);
int iIndex, iValA, iValB;
for (iIndex = 0; iIndex < iCount; iIndex++) {
string sPartA = iIndex < asA.Length ? asA[iIndex].Trim() : "0";
string sPartB = iIndex < asB.Length ? asB[iIndex].Trim() : "0";
bool bNumA = int.TryParse(sPartA, out iValA);
bool bNumB = int.TryParse(sPartB, out iValB);
if (bNumA && bNumB) { if (iValA != iValB) return iValA - iValB; }
else { int iCmp = string.CompareOrdinal(sPartA, sPartB); if (iCmp != 0) return iCmp; }
}
return 0;
} // compareVersions method

public static bool bKeyDescriber = false;
public static Dictionary<string, string> dDirectory = new Dictionary<string, string>();
public static Frame frame = null;
public static string sAppDir;
public static string sDataDir;
public static string sIniFile;
public static string sSpeechLog;
public static string sDefaultDir = "";
public static string sDefaultOrder = "";
public static string sDefaultFilter = "";
public static string sTextEditor = "EdSharp.exe";
public static string sWordProcessor = "WinWord.exe";
public static string sTempFile;
public static List<string> lsTempFile = new List<string>();
public static List<string> lsRecentDirs = new List<string>();

// WHERE THE HISTORY IS KEPT, and why it needed rebuilding.
//
// A visit used to be recorded in ONE place: the MdiChild constructor. So a
// folder was remembered only when a new window was made for it. Going to a
// folder that already had a window -- which activate_Helper does, returning
// early after activating it -- recorded nothing at all, and neither did
// refreshing or any command that reused the window in place. The Quick folder
// on the Accent key is the everyday case: go there twice and the second visit
// left no trace.
//
// recordVisit is now called from the one place every arrival passes through, so
// it does not matter how a folder was reached.
public static int iHistoryAt = -1;
public static bool bReplaying = false;

public static void recordVisit(string sPath) {
// Add a folder to the history, unless this arrival IS the history moving.
//
// Going back must not itself be recorded, or Back and Forward would chase each
// other: every step back would append a new entry and there would never be
// anywhere forward to go. bReplaying says "this arrival came from the history",
// and the pointer is moved by the command instead.
if (bReplaying) return;
if (sPath == null || sPath.Trim().Length == 0) return;

// Arriving where you already are is not a visit. Refreshing a folder, or
// activating the window that is already in front, would otherwise fill the
// history with the same entry.
if (iHistoryAt >= 0 && iHistoryAt < lsRecentDirs.Count
&& Homer.Util.stringEquiv(lsRecentDirs[iHistoryAt], sPath)) return;

// Anything ahead of the current position is discarded, as a browser does:
// going back three folders and then somewhere new makes a new branch, and the
// old forward path is no longer reachable or wanted.
if (iHistoryAt >= 0 && iHistoryAt < lsRecentDirs.Count - 1)
lsRecentDirs.RemoveRange(iHistoryAt + 1, lsRecentDirs.Count - iHistoryAt - 1);

lsRecentDirs.Add(sPath);
// A long session should not grow without limit.
while (lsRecentDirs.Count > 200) lsRecentDirs.RemoveAt(0);
iHistoryAt = lsRecentDirs.Count - 1;
} // recordVisit method

public static string[] recentDistinct() {
// The history for the Recent Folders list: newest first, each folder once.
//
// The history itself keeps every visit in order, because Back and Forward need
// that. A person picking from a list wants each place named once, most recent
// at the top, which is a different question about the same data.
List<string> lsSeen = new List<string>();
for (int i = lsRecentDirs.Count - 1; i >= 0; i--) {
string sPath = lsRecentDirs[i];
bool bHad = false;
foreach (string sOther in lsSeen) if (Homer.Util.stringEquiv(sOther, sPath)) bHad = true;
if (!bHad) lsSeen.Add(sPath);
}
return lsSeen.ToArray();
} // recentDistinct method
public static bool Recycle = true;
public static bool DirsBeforeFiles = true;
public static bool bExtraSpeech = true;
public static bool bZipOpener = true;
public static string sUserName = "";
public static string sPassword = "";
public static string sSenderAddress = "";
public static string sOutgoingServer = "";
public static string[] aTags = {};
public static string sCopyText = "";
public static string sMoveText = "";
public static string sFilterText = "";
public static string sEvaluateText = "";
public static string sCalculateText = "";
public static int iCalculateItem = 0;
public static string sFileFindText = "";
public static string sFileFindFilterText = "*.*";
public static string sFileFindDir = "*.*";
public static string[] aFileFind = {};
public static int iFileFind = -1;
public static string sFTPText = "";
public static string sGoToText = "";
public static string sJumpText = "";
public static string sKeywordsText = "";
public static string sOpenText = "";
public static string sOrderText = "";
public static string sRenameWildcardsText = "";
public static string sRenameRegexText = "";
public static string sUnarchiveText = "";
public static string sUnarchivePassword = "";
public static string sWebText = "";
public static string sZipText = "";
public static string sZipList = "";
public static string sVirtualFolder = "";
public static string sBatchMail = "";
public static Stopwatch stopwatch = new Stopwatch();
//public static Timer timer = null;
public static System.Timers.Timer timer = null;
public static string sTimerInterval = "0";
public static string sTimerStop = "0:00";
public static DateTime dtTimerStart = DateTime.Now;
public static TimeSpan tsElapsed = new TimeSpan();

// ---- Speech adapter (Homer port) ----------------------------------
// Speech flows through Homer.Say.sayForced: JAWS and NVDA are addressed
// through their own interfaces, and otherwise a native UIA notification is
// raised, which Narrator and other compatible readers announce.  Exactly one
// announcement is produced per message.  FileDir's original gating is preserved
// unchanged: when extra speech is off the text is appended to the speech
// log instead of voiced; a non-global announcement is suppressed unless
// FileDir is the active window and ScrollLock is off. Homer.Say.attach is
// called once from the Frame constructor so the live region exists.
[DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
static extern IntPtr getForegroundWindow();

public static bool isAppActiveWindow() {
IntPtr h = getForegroundWindow();
foreach (Form frm in Application.OpenForms) if (frm.Handle == h) return true;
return false;
} // isAppActiveWindow method

public static bool say(object oText) {
return say(oText, false);
} // say method

public static bool say(object oText, bool bGlobal) {
if (!App.bExtraSpeech) {
string sLog = File.Exists(App.sSpeechLog) ? File.ReadAllText(App.sSpeechLog) : "";
File.WriteAllText(App.sSpeechLog, sLog + oText.ToString() + "\r\n");
return true;
}

if (!bGlobal) {
if (!isAppActiveWindow()) return false;
if (Control.IsKeyLocked(Keys.Scroll)) return false;
}

Homer.Say.sayForced(oText.ToString());
return true;
} // say method

// ---- Config layer (Homer port): FileDir.inix over classic .ini -----
// Mirrors EdSharp. Reads are .inix-first with .ini fallback; writes go to
// the .ini (the existing Win32 path) and also sync into FileDir.inix when
// that file exists, so an override never masks a value the app just
// changed. When no FileDir.inix is present, behavior is exactly as before
// (pure .ini), so existing settings are preserved. Only the main settings
// file (App.sIniFile) is layered; other .ini files (e.g. the hotkey map)
// pass straight through. Built on the portable Homer.InixCodec.
static List<Homer.InixCodec.Section> lsInixSections = null;
static bool bInixLoaded = false;

static string getInixPath() {
if (string.IsNullOrEmpty(App.sIniFile)) return null;
if (App.sIniFile.ToLower().EndsWith(".ini")) return App.sIniFile.Substring(0, App.sIniFile.Length - 4) + ".inix";
return App.sIniFile + ".inix";
} // getInixPath method

static void ensureInixLoaded() {
if (bInixLoaded) return;
bInixLoaded = true;
try {
string sPath = getInixPath();
if (sPath != null && File.Exists(sPath)) lsInixSections = Homer.InixCodec.read(sPath);
}
catch { lsInixSections = null; }
} // ensureInixLoaded method

static bool inixTryGet(string sSection, string sKey, out string sValue) {
sValue = null;
ensureInixLoaded();
if (lsInixSections == null) return false;
foreach (Homer.InixCodec.Section sec in lsInixSections) {
if (!string.Equals(sec.Name, sSection, StringComparison.OrdinalIgnoreCase)) continue;
foreach (Homer.InixCodec.Pair pair in sec.Pairs) if (string.Equals(pair.Key, sKey, StringComparison.OrdinalIgnoreCase)) { sValue = pair.Value; return true; }
}
return false;
} // inixTryGet method

static void inixSyncWrite(string sFile, string sSection, string sKey, string sValue) {
if (string.IsNullOrEmpty(sFile) || !string.Equals(sFile, App.sIniFile, StringComparison.OrdinalIgnoreCase)) return;
string sPath = getInixPath();
if (sPath == null || !File.Exists(sPath)) return;
try { Homer.InixCodec.writeValue(sPath, sSection, sKey, sValue); bInixLoaded = false; lsInixSections = null; }
catch {}
} // inixSyncWrite method

public static string readValue(string sFile, string sSection, string sKey, string sDefault) {
string sValue;
if (!string.IsNullOrEmpty(sFile) && string.Equals(sFile, App.sIniFile, StringComparison.OrdinalIgnoreCase) && inixTryGet(sSection, sKey, out sValue)) return sValue;
return Homer.Util.readValue(sFile, sSection, sKey, sDefault);
} // readValue method

public static bool writeValue(string sFile, string sSection, string sKey, string sValue) {
bool bResult = Homer.Util.writeValue(sFile, sSection, sKey, sValue);
inixSyncWrite(sFile, sSection, sKey, sValue);
return bResult;
} // writeValue method

// ---- LbcVB replacement (Homer/BCL port) ----------------------------
// Replaces the former LbcVB.dll Visual Basic helper module. Link
// extraction AND file transfer (HTTP, HTTPS, FTP) now go through Homer.Web,
// shared with EdSharp and DbDo; the rest map onto
// the BCL: SoundPlayer/SystemSound for audio, SmtpClient for mail, DPAPI
// ProtectedData for the optional password store, and ComputerInfo for the
// system summary.
static System.Media.SoundPlayer soundPlayer = null;

public static System.Collections.ObjectModel.ReadOnlyCollection<string> getFiles(string sDir, string sSpec) {
return new System.Collections.ObjectModel.ReadOnlyCollection<string>(new List<string>(Directory.GetFiles(sDir, sSpec, System.IO.SearchOption.AllDirectories)));
} // getFiles method

public static System.Collections.ObjectModel.ReadOnlyCollection<string> findInFiles(string sDir, string sText, string sSpec) {
List<string> lsMatch = new List<string>();
foreach (string sFile in Directory.GetFiles(sDir, sSpec, System.IO.SearchOption.AllDirectories)) {
try { if (File.ReadAllText(sFile).IndexOf(sText, StringComparison.OrdinalIgnoreCase) >= 0) lsMatch.Add(sFile); }
catch {}
}
return new System.Collections.ObjectModel.ReadOnlyCollection<string>(lsMatch);
} // findInFiles method

public static string sizeFriendly(long lBytes) {
// A size a person can take in at once, for speech.
//
// WHY NOT A LIBRARY. Humanizer has a ByteSize humanizer and it is a fine
// library, but it says "1.15 GB": an abbreviation, two decimal places, and a
// NuGet package on a .NET Framework 4.8 build -- which is the Span trap that
// already cost this project a silent failure in 2htm. It also cannot say what
// is wanted here, so there is nothing to gain and a dependency to lose.
//
// WHAT IS WANTED, and why:
//
//   "k" for kilobytes, because a screen reader reads KB as two letters and
//     everyone knows what k means on a file size.
//   "megabytes" and "gigabytes" in full, because MB and GB read as letters too
//     and the words are short enough to say.
//   ONE decimal place at most, and only below ten. "9.4 megabytes" carries
//     information; "94.3 megabytes" is a number nobody holds on to. This is the
//     ordinary significant-figures habit, and it satisfies "no more than one
//     decimal place" without ever printing a pointless ".0".
//   The noun matches the count, so "1 byte" and "1 megabyte" are singular and
//     zero is said plainly as "0 bytes".
//
// Divided by 1024, matching what File Explorer shows for the same file, so the
// two never disagree in front of somebody.
if (lBytes < 0) return "unknown";
if (lBytes < 1024) return lBytes + (lBytes == 1 ? " byte" : " bytes");

string[] aSingular = {"k", "megabyte", "gigabyte", "terabyte", "petabyte"};
string[] aPlural = {"k", "megabytes", "gigabytes", "terabytes", "petabytes"};
double nValue = lBytes;
int iUnit = -1;
while (nValue >= 1024 && iUnit < aSingular.Length - 1) {
nValue /= 1024;
iUnit++;
}

// Below ten, one decimal place; at ten and above, none. A whole number is
// said as a whole number: "2 megabytes", never "2.0 megabytes".
if (nValue < 10) {
double nRounded = Math.Round(nValue, 1);
if (nRounded == Math.Floor(nRounded)) {
long lWhole = (long) nRounded;
return lWhole + " " + (lWhole == 1 ? aSingular[iUnit] : aPlural[iUnit]);
}
return nRounded.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
+ " " + aPlural[iUnit];
}
long lRounded = (long) Math.Round(nValue);
return lRounded + " " + (lRounded == 1 ? aSingular[iUnit] : aPlural[iUnit]);
} // sizeFriendly method

static string formatBytesShort(double nBytes) {
string[] aUnit = {"bytes", "KB", "MB", "GB", "TB"};
int i = 0;
while (nBytes >= 1024 && i < aUnit.Length - 1) { nBytes /= 1024; i++; }
return string.Format("{0:0.##} {1}", nBytes, aUnit[i]);
} // formatBytesShort method

public static string yieldInOperatingSystem() {
Microsoft.VisualBasic.Devices.ComputerInfo computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
string s = "";
s += string.Format("Full Name = {0}", computerInfo.OSFullName) + "\r\n";
s += string.Format("Version = {0}", computerInfo.OSVersion) + "\r\n";
s += string.Format("User Interface Culture = {0}", computerInfo.InstalledUICulture) + "\r\n";
s += string.Format("Platform = {0}", computerInfo.OSPlatform) + "\r\n";
s += string.Format("Total physical memory = {0}", formatBytesShort(computerInfo.TotalPhysicalMemory)) + "\r\n";
s += string.Format("Available physical memory = {0}", formatBytesShort(computerInfo.AvailablePhysicalMemory)) + "\r\n";
s += string.Format("Total virtual memory = {0}", formatBytesShort(computerInfo.TotalVirtualMemory)) + "\r\n";
s += string.Format("Available virtual memory = {0}", formatBytesShort(computerInfo.AvailableVirtualMemory)) + "\r\n";
return s;
} // yieldInOperatingSystem method

public static void downloadFile(string sUrl, string sFile, string sUserName, string sPassword) {
// Homer.Web handles HTTP, HTTPS, and FTP with modern TLS and a real User-Agent.
// This replaces Microsoft.VisualBasic.Devices.Network, so no VB runtime is
// involved and EdSharp, FileDir, and DbDo all transfer files the same way.
if (!Homer.Web.downloadTo(sUrl, sFile, sUserName, sPassword)) throw new IOException("Download failed: " + sUrl);
} // downloadFile method

public static void uploadFile(string sFile, string sUrl, string sUserName, string sPassword) {
if (!Homer.Web.uploadFrom(sFile, sUrl, sUserName, sPassword)) throw new IOException("Upload failed: " + sUrl);
} // uploadFile method

public static void sendMail(string sUserName, string sPassword, string sSenderAddress, string sOutgoingServer, string sSubject, string sText, string sRecipient) {
System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage(sSenderAddress, sRecipient, sSubject, sText);
System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(sOutgoingServer, 25);
smtpClient.EnableSsl = false;
smtpClient.Credentials = new System.Net.NetworkCredential(sUserName, sPassword);
smtpClient.Send(mailMessage);
} // sendMail method

public static List<string[]> getLinks(string sUrl) {
return Homer.Web.getLinks(sUrl);
} // getLinks method

public static void playWav(string sWav) {
if (soundPlayer == null) soundPlayer = new System.Media.SoundPlayer();
soundPlayer.SoundLocation = sWav;
soundPlayer.PlaySync();
} // playWav method

public static void stopWav() {
if (soundPlayer != null) soundPlayer.Stop();
} // stopWav method

public static void playSystemSound(System.Media.SystemSound sound) {
if (sound != null) sound.Play();
} // playSystemSound method

public static void encrypt2File(string sText, string sFile) {
byte[] aData = System.Text.Encoding.UTF8.GetBytes(sText);
byte[] aEncoded = System.Security.Cryptography.ProtectedData.Protect(aData, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
File.WriteAllBytes(sFile, aEncoded);
} // encrypt2File method

public static string file2Decrypt(string sFile) {
byte[] aEncoded = File.ReadAllBytes(sFile);
byte[] aData = System.Security.Cryptography.ProtectedData.Unprotect(aEncoded, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
return System.Text.Encoding.UTF8.GetString(aData);
} // file2Decrypt method

// Extended file properties via the Windows Shell, replacing the former
// GetProps.js JScript run through LbcJS. Late-bound COM through dynamic,
// fully guarded: any failure yields an empty string so the Type Extended
// command still shows its file-association details.
public static string getShellProperties(string sPath) {
string sReturn = "";
try {
Type tShell = Type.GetTypeFromProgID("Shell.Application");
if (tShell == null) return sReturn;
dynamic shell = Activator.CreateInstance(tShell);
dynamic folder = shell.NameSpace(Path.GetDirectoryName(sPath));
if (folder == null) return sReturn;
dynamic item = folder.ParseName(Path.GetFileName(sPath));
if (item == null) return sReturn;
for (int i = 0; i < 320; i++) {
string sHeader = folder.GetDetailsOf(null, i) as string;
string sValue = folder.GetDetailsOf(item, i) as string;
if (!string.IsNullOrEmpty(sHeader) && !string.IsNullOrEmpty(sValue)) sReturn += sHeader + " = " + sValue + "\r\n";
}
}
catch {}
return sReturn;
} // getShellProperties method

// Dynamic expression/script evaluator. Runs JScript .NET in a separate
// assembly (FileDirScript.dll, built from FileDir.js by jsc.exe) loaded by
// reflection, so FileDir.cs keeps no compile-time JScript dependency --
// the EdSharp / DbDo model. The active directory window (frm) and its data
// table (tbl) are exposed to snippets. Returns the script's last value as
// a string, "ERROR: ..." on fault, or an empty string when unavailable.
public static string runScript(string sCode, object frm, object tbl) {
try {
string sDll = Path.Combine(App.sAppDir, "FileDirScript.dll");
if (!File.Exists(sDll)) return "ERROR: FileDirScript.dll not found";
Assembly assembly = Assembly.LoadFrom(sDll);
Type tJs = assembly.GetType("FileDirScript.JS");
if (tJs == null) return "ERROR: FileDirScript.JS not found";
MethodInfo methodInfo = tJs.GetMethod("runScript", new Type[] {typeof(string), typeof(object), typeof(object)});
if (methodInfo == null) return "ERROR: runScript not found";
return (string) methodInfo.Invoke(null, new object[] {sCode, frm, tbl});
}
catch (Exception ex) { return "ERROR: " + ex.Message; }
} // runScript method

public static string getPortableExecutableKind() {
PortableExecutableKinds peKind  ;
ImageFileMachine machine  ;

// Module module = App.Shell.GetType().Module;
Module module = Assembly.GetExecutingAssembly().ManifestModule;
module.GetPEKind(out peKind, out machine);

if ((peKind & PortableExecutableKinds.ILOnly) != 0) // Assembly is platform independent.
{}
else { // assembly is platform dependent
switch (machine) {
case ImageFileMachine.I386: // i386, x86, IA-32, ... dependent.
break;
case ImageFileMachine.IA64: // IA-64 dependent.
break;
case ImageFileMachine.AMD64: // AMD-64, x64 dependent.
break;
} // switch
} // if

Dictionary<string, string> dFlags = new Dictionary<string, string>();
dFlags.Add("NotAPortableExecutableImage", "The file is not in portable executable (PE) file format.");
dFlags.Add("ILOnly", "The executable contains only Microsoft intermediate language (MSIL), and is therefore neutral with respect to 32-bit or 64-bit platforms.");
dFlags.Add("Required32Bit", "The executable can be run on a 32-bit platform, or in the 32-bit Windows on Windows (WOW) environment on a 64-bit platform.");
dFlags.Add("PE32Plus", "The executable requires a 64-bit platform.");
dFlags.Add("Unmanaged32Bit", "The executable contains pure unmanaged code.");
dFlags.Add("I386", "Targets a 32-bit Intel processor.");
dFlags.Add("IA64", "Targets a 64-bit Intel processor.");
dFlags.Add("AMD64", "Targets a 64-bit AMD processor.");
string sReturn = "";
string sPEKind = peKind.ToString();
string[] aPEKind = sPEKind.Split(',');
string sMachine = machine.ToString();
foreach (string s in aPEKind) {
sPEKind = s.Trim();
if (dFlags.ContainsKey(sPEKind)) sReturn += dFlags[sPEKind] + "\n\n";
else sReturn += sPEKind + "\n\n";
} // foreach

// Not useful info
// if (dFlags.ContainsKey(sMachine)) sReturn += dFlags[sMachine] + "\n\n";
// else sReturn += sMachine + "\n\n";
sReturn += "Running in " + (IntPtr.Size == 8 ? "64" : "32") + "-bit mode.";
// sReturn = sReturn.Replace("\nTargets a ", "\nRunning on a ");
// Dialog.Show("Portable Executable Kind", sReturn);
return sReturn;
} // getPortableExecutableKind method



public static void SetDrive(string sDir) {
string sDrive = sDir.Substring(0, 1).ToUpper();
string sOldDir;
if (App.dDirectory.TryGetValue(sDrive, out sOldDir)) sDir = sOldDir;
if (sDir.Length == 1) sDir += @":\";
App.SetDirectory(sDir);
}

public static string GetDirectory(string sDir) {
string sDrive = sDir.Substring(0, 1).ToUpper();
string sOldDir;
if (App.dDirectory.TryGetValue(sDrive, out sOldDir)) sDir = sOldDir;
return sDir;
}

public static void SetDirectory(string sDir) {
string sOldDir = Directory.GetCurrentDirectory();
string sOldDrive = sDir.Substring(0, 1).ToUpper();
if (App.dDirectory.ContainsKey(sOldDrive)) App.dDirectory.Remove(sOldDrive);
App.dDirectory.Add(sOldDrive, sOldDir);
Directory.SetCurrentDirectory(sDir);
}

public static bool killFile(string sFile) {
if (!File.Exists(sFile)) return true;

FileAttributes attr = File.GetAttributes(sFile);
File.SetAttributes(sFile, (attr | FileAttributes.ReadOnly) ^ FileAttributes.ReadOnly);
File.Delete(sFile);
return File.Exists(sFile);
} // killFile method

public static bool itemExists(string sPath) {
return (Directory.Exists(sPath) || File.Exists(sPath));
} // itemExists method

public static void deleteItem(string sPath, bool bRecycle) {
FileAttributes attr = File.GetAttributes(sPath);
FileAttributes flag = FileAttributes.ReadOnly;
File.SetAttributes(sPath, (attr | flag) ^ flag);
if (Directory.Exists(sPath)) deleteDirectory(sPath, bRecycle);
else if (File.Exists(sPath)) deleteFile(sPath, bRecycle);
} // deleteItem method

public static void deleteDirectory(string sPath, bool bRecycle) {
if (!Directory.Exists(sPath)) return;
if (bRecycle) FileSystem.DeleteDirectory(sPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
//if (bRecycle) FileSystem.DeleteDirectory(sPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
else FileSystem.DeleteDirectory(sPath, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently, UICancelOption.ThrowException);
//else FileSystem.DeleteDirectory(sPath, UIOption.AllDialogs, RecycleOption.DeletePermanently, UICancelOption.ThrowException);
} // deleteDirectory method

public static void deleteFile(string sPath, bool bRecycle) {
if (!File.Exists(sPath)) return;
if (bRecycle) FileSystem.DeleteFile(sPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
else FileSystem.DeleteFile(sPath, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently, UICancelOption.ThrowException);
} // deleteFile method

public static void copyDirectory(string sSource, string sTarget, bool bRecycle) {
if (Directory.Exists(sTarget)) App.deleteDirectory(sTarget, bRecycle);
else if (File.Exists(sTarget)) App.deleteFile(sTarget, bRecycle);
//FileSystem.CopyDirectory(sSource, sTarget, UIOption.OnlyErrorDialogs, UICancelOption.ThrowException);
FileSystem.CopyDirectory(sSource, sTarget, UIOption.AllDialogs, UICancelOption.ThrowException);
} // copyDirectory method

public static void moveDirectory(string sSource, string sTarget, bool bRecycle) {
if (Directory.Exists(sTarget)) App.deleteDirectory(sTarget, bRecycle);
else if (File.Exists(sTarget)) App.deleteFile(sTarget, bRecycle);
//FileSystem.MoveDirectory(sSource, sTarget, UIOption.OnlyErrorDialogs, UICancelOption.ThrowException);
FileSystem.MoveDirectory(sSource, sTarget, UIOption.AllDialogs, UICancelOption.ThrowException);
} // moveDirectory method

public static void copyFile(string sSource, string sTarget, bool bRecycle) {
if (Directory.Exists(sTarget)) App.deleteDirectory(sTarget, bRecycle);
else if (File.Exists(sTarget)) App.deleteFile(sTarget, bRecycle);
//FileSystem.CopyFile(sSource, sTarget, UIOption.OnlyErrorDialogs, UICancelOption.ThrowException);
FileSystem.CopyFile(sSource, sTarget, UIOption.AllDialogs, UICancelOption.ThrowException);
} // copyFile method

public static void moveFile(string sSource, string sTarget, bool bRecycle) {
if (Directory.Exists(sTarget)) App.deleteDirectory(sTarget, bRecycle);
else if (File.Exists(sTarget)) App.deleteFile(sTarget, bRecycle);
//FileSystem.MoveFile(sSource, sTarget, UIOption.OnlyErrorDialogs, UICancelOption.ThrowException);
FileSystem.MoveFile(sSource, sTarget, UIOption.AllDialogs, UICancelOption.ThrowException);
} // moveFile method

public static void readIni() {
App.Recycle = App.readValue(App.sIniFile, "Settings", "RecycleWithDelete", (App.Recycle ? "y" : "n")).Trim().ToLower() == "n" ? false : true;
App.DirsBeforeFiles = App.readValue(App.sIniFile, "Settings", "DirsBeforeFiles", (App.DirsBeforeFiles ? "y" : "n")).Trim().ToLower() == "n" ? false : true;
App.bExtraSpeech = App.readValue(App.sIniFile, "Settings", "ExtraSpeech", (App.bExtraSpeech ? "y" : "n")).Trim().ToLower() == "n" ? false : true;
App.bZipOpener = App.readValue(App.sIniFile, "Settings", "ZipOpener", (App.bZipOpener ? "y" : "n")).Trim().ToLower() == "n" ? false : true;
App.sTextEditor = App.readValue(App.sIniFile, "Settings", "TextEditor", App.sTextEditor);
App.sWordProcessor = App.readValue(App.sIniFile, "Settings", "WordProcessor", App.sWordProcessor);
App.sUserName = App.readValue(App.sIniFile, "Settings", "UserName", App.sUserName);
App.sPassword = App.readValue(App.sIniFile, "Settings", "Password", App.sPassword);
App.sUnarchivePassword = App.readValue(App.sIniFile, "Settings", "UnarchivePassword", App.sUnarchivePassword);
App.sSenderAddress = App.readValue(App.sIniFile, "Settings", "SenderAddress", App.sSenderAddress);
App.sOutgoingServer = App.readValue(App.sIniFile, "Settings", "OutgoingServer", App.sOutgoingServer);
// if (File.Exists(Path.Combine(App.sDataDir, "p.bin"))) App.sPassword = App.file2Decrypt(Path.Combine(App.sDataDir, "p.bin"));
// if (File.Exists(Path.Combine(App.sDataDir, "u.bin"))) App.sUnarchivePassword = App.file2Decrypt(Path.Combine(App.sDataDir, "u.bin"));
App.sDefaultDir = App.readValue(App.sIniFile, "Internal", "DefaultDir", App.sDefaultDir);
App.sDefaultOrder = App.readValue(App.sIniFile, "Internal", "DefaultOrder", App.sDefaultOrder);
App.sDefaultFilter = App.readValue(App.sIniFile, "Internal", "DefaultFilter", App.sDefaultFilter);
App.sCopyText = App.readValue(App.sIniFile, "Internal", "Copy", App.sCopyText);
App.sEvaluateText = App.readValue(App.sIniFile, "Internal", "Evaluate", App.sEvaluateText);
App.sCalculateText = App.readValue(App.sIniFile, "Internal", "Calculate", App.sCalculateText);
App.iCalculateItem = Int32.Parse(App.readValue(App.sIniFile, "Internal", "CalculateItem", App.iCalculateItem.ToString()));
App.sFTPText = App.readValue(App.sIniFile, "Internal", "FTP", App.sFTPText);
App.sFileFindText = App.readValue(App.sIniFile, "Internal", "FileFind", App.sFileFindText);
App.sFileFindFilterText = App.readValue(App.sIniFile, "Internal", "FileFindFilter", App.sFileFindFilterText);
App.sGoToText = App.readValue(App.sIniFile, "Internal", "GoTo", App.sGoToText);
App.sJumpText = App.readValue(App.sIniFile, "Internal", "Jump", App.sJumpText);
App.sKeywordsText = App.readValue(App.sIniFile, "Internal", "Keywords", App.sKeywordsText);
App.sMoveText = App.readValue(App.sIniFile, "Internal", "Move", App.sMoveText);
App.sOpenText = App.readValue(App.sIniFile, "Internal", "Open", App.sOpenText);
App.sRenameWildcardsText = App.readValue(App.sIniFile, "Internal", "RenameWildcards", App.sRenameWildcardsText);
App.sRenameRegexText = App.readValue(App.sIniFile, "Internal", "RenameRegex", App.sRenameRegexText);
App.sUnarchiveText = App.readValue(App.sIniFile, "Internal", "Unarchive", App.sUnarchiveText);
App.sWebText = App.readValue(App.sIniFile, "Internal", "Web", App.sWebText);
App.sZipText = App.readValue(App.sIniFile, "Internal", "Zip", App.sZipText);
App.sZipList = App.readValue(App.sIniFile, "Internal", "ZipList", App.sZipList);
App.sVirtualFolder = App.readValue(App.sIniFile, "Internal", "VirtualFolder", App.sVirtualFolder);
App.sBatchMail = App.readValue(App.sIniFile, "Internal", "BatchMail", App.sBatchMail);
App.sTimerInterval = App.readValue(App.sIniFile, "Internal", "TimerInterval", App.sTimerInterval);
App.sTimerStop = App.readValue(App.sIniFile, "Internal", "TimerStop", App.sTimerStop);
} // readIni method

public static void writeIni() {
App.writeValue(App.sIniFile, "Settings", "RecycleWithDelete", (App.Recycle ? "y" : "n"));
App.writeValue(App.sIniFile, "Settings", "DirsBeforeFiles", (App.DirsBeforeFiles ? "y" : "n"));
App.writeValue(App.sIniFile, "Settings", "ExtraSpeech", (App.bExtraSpeech ? "y" : "n"));
App.writeValue(App.sIniFile, "Settings", "ZipOpener", (App.bZipOpener ? "y" : "n"));
App.writeValue(App.sIniFile, "Settings", "TextEditor", App.sTextEditor);
App.writeValue(App.sIniFile, "Settings", "WordProcessor", App.sWordProcessor);
App.writeValue(App.sIniFile, "Settings", "UserName", App.sUserName);
App.writeValue(App.sIniFile, "Settings", "Password", App.sPassword);
App.writeValue(App.sIniFile, "Settings", "UnarchivePassword", App.sUnarchivePassword);
// App.encrypt2File(App.sPassword, Path.Combine(App.sDataDir, "p.bin"));
App.writeValue(App.sIniFile, "Settings", "SenderAddress", App.sSenderAddress);
App.writeValue(App.sIniFile, "Settings", "OutgoingServer", App.sOutgoingServer);
// App.encrypt2File(App.sUnarchivePassword, Path.Combine(App.sDataDir, "u.bin"));
App.writeValue(App.sIniFile, "Internal", "DefaultDir", App.sDefaultDir);
App.writeValue(App.sIniFile, "Internal", "DefaultOrder", App.sDefaultOrder);
App.writeValue(App.sIniFile, "Internal", "DefaultFilter", App.sDefaultFilter);
App.writeValue(App.sIniFile, "Internal", "Copy", App.sCopyText);
App.writeValue(App.sIniFile, "Internal", "Evaluate", App.sEvaluateText);
App.writeValue(App.sIniFile, "Internal", "Calculate", App.sCalculateText);
App.writeValue(App.sIniFile, "Internal", "CalculateItem", App.iCalculateItem.ToString());
App.writeValue(App.sIniFile, "Internal", "FTP", App.sFTPText);
App.writeValue(App.sIniFile, "Internal", "FileFind", App.sFileFindText);
App.writeValue(App.sIniFile, "Internal", "FileFindFilter", App.sFileFindFilterText);
App.writeValue(App.sIniFile, "Internal", "GoTo", App.sGoToText);
App.writeValue(App.sIniFile, "Internal", "Jump", App.sJumpText);
App.writeValue(App.sIniFile, "Internal", "Keywords", App.sKeywordsText);
App.writeValue(App.sIniFile, "Internal", "Move", App.sMoveText);
App.writeValue(App.sIniFile, "Internal", "Open", App.sOpenText);
App.writeValue(App.sIniFile, "Internal", "RenameRegex", App.sRenameRegexText);
App.writeValue(App.sIniFile, "Internal", "Unarchive", App.sUnarchiveText);
App.writeValue(App.sIniFile, "Internal", "Web", App.sWebText);
App.writeValue(App.sIniFile, "Internal", "Zip", App.sZipText);
App.writeValue(App.sIniFile, "Internal", "ZipList", App.sZipList);
App.writeValue(App.sIniFile, "Internal", "VirtualFolder", App.sVirtualFolder);
App.writeValue(App.sIniFile, "Internal", "BatchMail", App.sBatchMail);
App.writeValue(App.sIniFile, "Internal", "TimerInterval", App.sTimerInterval);
App.writeValue(App.sIniFile, "Internal", "TimerStop", App.sTimerStop);
} // writeIni method

[STAThread]
static void Main(string[] args) {
string sApp = System.Reflection.Assembly.GetExecutingAssembly().Location;
sAppDir = Path.GetDirectoryName(sApp);

// The session log, in the Homer Tools convention: one file per run at
// %LOCALAPPDATA%\FileDir\logs\FileDir_<timestamp>.log, beside the setup log the
// installer writes.  It opens with the version and the environment, and every
// outside program FileDir runs adds a line with its exit code, so a conversion
// that failed can be diagnosed rather than guessed at.  The newest thirty are
// kept.  Copy Log, Control+F12, puts the path on the clipboard.
//
// This is started before anything else can fail, and starting it can never fail
// the program: a read-only profile simply means no log.
Homer.Log.start("FileDir", BuildVersion.Version, sApp, args);

// --install-jaws-settings: copy this app's JAWS script family into every
// installed version of JAWS and compile it there, then exit.  The installer's
// Finish page runs this, and the Help menu can run it again later.  The work is
// done by the shared Homer.JawsSettingsInstaller, the same code DbDo and EdSharp
// use -- replacing the separate FileDir_Scripts_setup.exe that used to ship.
bool bQuietJaws = false;
foreach (string sArg in args) if (String.Compare(sArg, "--quiet", true) == 0) bQuietJaws = true;
foreach (string sArg in args) {
if (String.Compare(sArg, "--install-jaws-settings", true) != 0) continue;
int iCopied = 0;
int iCompiled = 0;
string sMessage = Homer.JawsSettingsInstaller.install("FileDir", Path.GetDirectoryName(sApp), new string[] {"FileDir.jsd", "FileDir.jcf", "Homer.jsh", "Homer.jsd", "MSAA.jsh"}, new string[] {"Homer"}, out iCopied, out iCompiled);

// The mpv scripts go in beside them, by the same route.
//
// JAWS matches a script file to the running program by name, so mpv.jss loads
// itself whenever mpv.exe is in front, with nothing to configure. mpv is
// entirely keyboard driven and has no menus, so without this its commands are
// invisible: there is nothing to explore and nothing to read.
//
// Installed whether or not mpv is on the machine yet. The files are harmless
// where there is no mpv, and installing them now means the player works
// properly the day somebody ticks that box, rather than needing the FileDir
// installer run a second time.
int iMpvCopied = 0;
int iMpvCompiled = 0;
try {
Homer.JawsSettingsInstaller.install("mpv", Path.GetDirectoryName(sApp),
new string[] {"mpv.jsd"}, new string[0], out iMpvCopied, out iMpvCompiled);
iCopied += iMpvCopied;
iCompiled += iMpvCompiled;
}
catch (Exception ex) {
// A failure here must not cost the FileDir scripts, which are the point of
// the command.
Homer.Log.write("The mpv scripts were not installed: " + ex.Message);
}
// The detail always goes to the log.  It is worth having -- which folders in
// which JAWS versions received which files -- and it is worth having later
// rather than in front of somebody who is installing a file manager.
Homer.Log.write("JAWS settings: " + iCopied + " copied, " + iCompiled + " compiled.");
foreach (string sLine in sMessage.Replace("\r\n", "\n").Split('\n'))
if (sLine.Trim().Length > 0) Homer.Log.write("  " + sLine.Trim());
// The box appears only when a person asked for this from the Help menu.  The
// installer passes --quiet, because a wall of "JAWS 2024 / enu: jkm jss jsb"
// lines is a report to nobody: it interrupts an installation to say that the
// thing the person ticked has happened, which the Results box says at the end
// anyway, in one line, along with everything else.
if (!bQuietJaws && sMessage.Length > 0) MessageBox.Show(sMessage, "FileDir - JAWS Settings");
return;
}

// --uninstall-jaws-settings: remove exactly the files a previous install
// placed in the JAWS settings folders, tracked in the per-app log, then
// exit. The uninstaller runs this hidden, so no dialog is shown; silence
// is the correct behavior there.
foreach (string sArg in args) {
if (String.Compare(sArg, "--uninstall-jaws-settings", true) != 0) continue;
int iDeleted = 0;
Homer.JawsSettingsInstaller.uninstall("FileDir", out iDeleted);
return;
}

sAppDir = Homer.Util.getShortPath(sAppDir);
string sRoot = Path.GetFileNameWithoutExtension(sApp);
string sName = sRoot + ".ini";
sDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
sDataDir = Path.Combine(sDataDir, sRoot);
if (!Directory.Exists(sDataDir)) Directory.CreateDirectory(sDataDir);
sDataDir = Homer.Util.getShortPath(sDataDir);
string sQuickDir = Path.Combine(sDataDir, "Quick");
if (!Directory.Exists(sQuickDir)) Directory.CreateDirectory(sQuickDir);
sIniFile = Path.Combine(sDataDir, sName);
App.readIni();
// sTempFile = Path.Combine(sDataDir, "FileDir.tmp");
sTempFile = Path.Combine(Homer.Util.getShortPath(sDataDir), "FileDir.tmp");
sSpeechLog = Path.Combine(sDataDir, "speech.log");
if (File.Exists(sSpeechLog)) File.Delete(sSpeechLog);

try {
//Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(true);

singleInstanceApplication application = new singleInstanceApplication();
application.Run(args);

App.sDefaultDir = Directory.GetCurrentDirectory();
if (App.lsRecentDirs.Count > 1){
string s = App.lsRecentDirs[App.lsRecentDirs.Count - 1];
if (App.itemExists(s)) App.sDefaultDir = s;
}
App.sDefaultOrder = App.sOrderText;
App.sDefaultFilter = App.sFilterText;
App.writeIni();
foreach (string sPath in lsTempFile) {
if (File.Exists(sPath)) {
try {
File.Delete(sPath);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
}
}
}
} // try
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
} // catch
//GC.KeepAlive(timer);
} // Main method

public static long DirSize(DirectoryInfo d) {
long Size = 0;
// Add file sizes.
FileInfo[] fis = d.GetFiles();
foreach (FileInfo fi in fis)
{
Size += fi.Length;
}
// Add subdirectory sizes.
DirectoryInfo[] dis = d.GetDirectories();
foreach (DirectoryInfo di in dis)
{
Size += DirSize(di);
}
return(Size);
}

} // App class

public class Frame : Form {
public string LastDescription = "";
public Dictionary<string, string> hashOrder = new Dictionary<string, string>();
public int iMdiChild;
public MenuStrip menuMain;
public HomerToolStripMenuItem menuFile;
public HomerToolStripMenuItem menuFileRefreshFolder;
public HomerToolStripMenuItem menuFileNewViewCopy;
public HomerToolStripMenuItem menuFileNewItemCopy;
public HomerToolStripMenuItem menuFileNewFolder;
public HomerToolStripMenuItem menuFileOpenFolder;
public HomerToolStripMenuItem menuFileGoToFolder;
public HomerToolStripMenuItem menuFileOpenSpecialFolder;
public HomerToolStripMenuItem menuFileGoToSpecialFolder;
public HomerToolStripMenuItem menuFileOpenDrive;
public HomerToolStripMenuItem menuFileGoToDrive;
public HomerToolStripMenuItem menuFileOpenVirtualFolder;
public HomerToolStripMenuItem menuFileGoToVirtualFolder;
public HomerToolStripMenuItem menuFileOpenParentFolder;
public HomerToolStripMenuItem menuFileGoToParentFolder;
public HomerToolStripMenuItem menuFileComeUpLevel;
public HomerToolStripMenuItem menuFileOpenRootFolder;
public HomerToolStripMenuItem menuFileGoToRootFolder;
public HomerToolStripMenuItem menuFileOpenQuickFolder;
public HomerToolStripMenuItem menuFileGoToQuickFolder;
public HomerToolStripMenuItem menuFileQuickShortcut;
public HomerToolStripMenuItem menuFileQuickURL;
public HomerToolStripMenuItem menuFileFind;
public HomerToolStripMenuItem menuFileProperties;
public HomerToolStripMenuItem menuFileOpenItem;
public HomerToolStripMenuItem menuFileGoToItem;
public HomerToolStripMenuItem menuFilePrintTagged;
public HomerToolStripMenuItem menuFileRecentFolders;
public HomerToolStripMenuItem menuFileGoBack;
public HomerToolStripMenuItem menuFileGoForward;
public HomerToolStripMenuItem menuFileWindowToggle;
public HomerToolStripMenuItem menuFileCurrentWindows;
public HomerToolStripMenuItem menuFileNextWindow;
public HomerToolStripMenuItem menuFilePreviousWindow;
public HomerToolStripMenuItem menuFileClose;
public HomerToolStripMenuItem menuFileCloseAllButCurrent;
public HomerToolStripMenuItem menuFileExit;
public HomerToolStripMenuItem menuFileRestartWindows;

public HomerToolStripMenuItem menuEdit;
public HomerToolStripMenuItem menuEditTagAndNext;
public HomerToolStripMenuItem menuEditUntagAndNext;
public HomerToolStripMenuItem menuEditTagAndPrevious;
public HomerToolStripMenuItem menuEditUntagAndPrevious;
public HomerToolStripMenuItem menuEditTagToBottom;
public HomerToolStripMenuItem menuEditUntagToBottom;
public HomerToolStripMenuItem menuEditTagToTop;
public HomerToolStripMenuItem menuEditUntagToTop;
public HomerToolStripMenuItem menuEditTag;
public HomerToolStripMenuItem menuEditUntag;
public HomerToolStripMenuItem menuEditToggleTag;
public HomerToolStripMenuItem menuEditTagAll;
public HomerToolStripMenuItem menuEditUntagAll;
public HomerToolStripMenuItem menuEditTagAllFiles;
public HomerToolStripMenuItem menuEditTagDuplicateFiles;
public HomerToolStripMenuItem menuEditTagSimilarFiles;
public HomerToolStripMenuItem menuEditReorderNames;
public HomerToolStripMenuItem menuEditFindDuplicates;
public HomerToolStripMenuItem menuEditTagWithRegularExpression;
public HomerToolStripMenuItem menuEditUntagAllButCurrent;
public HomerToolStripMenuItem menuEditStartTagOrUntag;
public HomerToolStripMenuItem menuEditCompleteTag;
public HomerToolStripMenuItem menuEditCompleteUntag;
public HomerToolStripMenuItem menuEditInvertTagged;
public HomerToolStripMenuItem menuEditSaveTags;
public HomerToolStripMenuItem menuEditRestoreTags;
public HomerToolStripMenuItem menuEditCopyToClipboardTagged;
public HomerToolStripMenuItem menuEditCopyAppendToClipboardTagged;
public HomerToolStripMenuItem menuEditCutToClipboardTagged;
//public HomerToolStripMenuItem menuEditCopyPathTagged;
public HomerToolStripMenuItem menuEditCopyNameTagged;
public HomerToolStripMenuItem menuEditPathList;
public HomerToolStripMenuItem menuEditCopyTagged;
public HomerToolStripMenuItem menuEditMoveTagged;
public HomerToolStripMenuItem menuEditDeleteAndRecycleTagged;
public HomerToolStripMenuItem menuEditDeleteTagged;
public HomerToolStripMenuItem menuEditDeleteTaggedWithoutRecycle;
public HomerToolStripMenuItem menuEditDeleteFileNow;
public HomerToolStripMenuItem menuEditDeleteFileNowWithoutRecycle;
public HomerToolStripMenuItem menuEditRename;
public HomerToolStripMenuItem menuEditRenameWithWildcards;
public HomerToolStripMenuItem menuEditRenameWithRegex;
public HomerToolStripMenuItem menuEditRenameToTitle;
public HomerToolStripMenuItem menuEditPasteFromClipboard;
public HomerToolStripMenuItem menuEditPasteCopy;
public HomerToolStripMenuItem menuEditPasteMove;
public HomerToolStripMenuItem menuEditStampTagged;
public HomerToolStripMenuItem menuEditHideTagged;
public HomerToolStripMenuItem menuEditShowTagged;
public HomerToolStripMenuItem menuEditReadOnlyTagged;
public HomerToolStripMenuItem menuEditReadWriteTagged;
public HomerToolStripMenuItem menuEditSystemTagged;
public HomerToolStripMenuItem menuEditGeneralTagged;
public HomerToolStripMenuItem menuEditPathToClipboard;
public HomerToolStripMenuItem menuEditShortPathToClipboard;
public HomerToolStripMenuItem menuEditFullFolderToClipboard;
public HomerToolStripMenuItem menuEditClearClipboard;
public HomerToolStripMenuItem menuEditExportClipboardToFile;

public HomerToolStripMenuItem menuNavigate;
public HomerToolStripMenuItem menuNavigateJump;
public HomerToolStripMenuItem menuNavigateJumpAgain;
public HomerToolStripMenuItem menuNavigateKeywords;
public HomerToolStripMenuItem menuNavigateKeywordsAgain;
public HomerToolStripMenuItem menuNavigateSetFilter;
public HomerToolStripMenuItem menuNavigateClearFilter;
public HomerToolStripMenuItem menuNavigateBeginningFile;
public HomerToolStripMenuItem menuNavigateBeginningTagged;
public HomerToolStripMenuItem menuNavigateEndTagged;
public HomerToolStripMenuItem menuNavigateNextTagged;
public HomerToolStripMenuItem menuNavigatePreviousTagged;
public HomerToolStripMenuItem menuNavigateInitialChange;
public HomerToolStripMenuItem menuNavigateExtensionChange;

public HomerToolStripMenuItem menuQuery;
public HomerToolStripMenuItem menuQueryDate;
public HomerToolStripMenuItem menuQueryList;
public HomerToolStripMenuItem menuQueryListTagged;
public HomerToolStripMenuItem menuQuerySelected;
public HomerToolStripMenuItem menuQueryListFiles;
public HomerToolStripMenuItem menuQueryPath;
public HomerToolStripMenuItem menuQuerySize;
public HomerToolStripMenuItem menuQueryType;
public HomerToolStripMenuItem menuQueryTypeExtended;
public HomerToolStripMenuItem menuQueryWindowsOpen;
public HomerToolStripMenuItem menuQueryYield;
public HomerToolStripMenuItem menuQueryYieldTagged;
public HomerToolStripMenuItem menuQueryYieldFiles;
public HomerToolStripMenuItem menuQueryYieldOnDrive;
public HomerToolStripMenuItem menuQueryYieldInOperatingSystem;
public HomerToolStripMenuItem menuQueryStatus;
public HomerToolStripMenuItem menuQueryCharacterEncoding;
public HomerToolStripMenuItem menuQueryPercentThrough;
public HomerToolStripMenuItem menuQueryFilter;
public HomerToolStripMenuItem menuQueryName;
public HomerToolStripMenuItem menuQueryFolderName;
public HomerToolStripMenuItem menuQueryFullFolder;
public HomerToolStripMenuItem menuQueryClipboard;
public HomerToolStripMenuItem menuQueryNow;
public HomerToolStripMenuItem menuQueryWhat;
public HomerToolStripMenuItem menuQueryTimer;

public HomerToolStripMenuItem menuMisc;
public HomerToolStripMenuItem menuMiscConfigurationOptions;
public HomerToolStripMenuItem menuMiscManualOptions;
public HomerToolStripMenuItem menuMiscExtraSpeechToggle;
public HomerToolStripMenuItem menuMiscExtraSpeechLog;
public HomerToolStripMenuItem menuMiscEnvironmentVariables;
public HomerToolStripMenuItem menuMiscRecycleToggle;
public HomerToolStripMenuItem menuMiscOpenRecycleBin;
public HomerToolStripMenuItem menuMiscAlphaOrder;
public HomerToolStripMenuItem menuMiscReverseAlphaOrder;
public HomerToolStripMenuItem menuMiscDateOrder;
public HomerToolStripMenuItem menuMiscReverseDateOrder;
public HomerToolStripMenuItem menuMiscSizeOrder;
public HomerToolStripMenuItem menuMiscReverseSizeOrder;
public HomerToolStripMenuItem menuMiscTypeOrder;
public HomerToolStripMenuItem menuMiscReverseTypeOrder;
public HomerToolStripMenuItem menuMiscSendToWordProcessor;
public HomerToolStripMenuItem menuMiscSendToTextEditor;
public HomerToolStripMenuItem menuMiscOutputTagged;
public HomerToolStripMenuItem menuMiscAppendTagged;
public HomerToolStripMenuItem menuMiscConvertEncodingTagged;
public HomerToolStripMenuItem menuMiscTranslateTagged;
public HomerToolStripMenuItem menuMiscExtractTagged;
public HomerToolStripMenuItem menuMiscBurnTagged;
public HomerToolStripMenuItem menuMiscMailBody;
public HomerToolStripMenuItem menuMiscMailAttachTagged;
public HomerToolStripMenuItem menuMiscBatchMail;
public HomerToolStripMenuItem menuMiscZipTagged;
public HomerToolStripMenuItem menuMiscZipTaggedThenDelete;
public HomerToolStripMenuItem menuMiscZipList;
public HomerToolStripMenuItem menuMiscUnarchiveTagged;
public HomerToolStripMenuItem menuMiscUnarchiveTaggedWithoutSubfolders;
public HomerToolStripMenuItem menuMiscUnarchiveTaggedToSameName;
public HomerToolStripMenuItem menuMiscUnarchiveTest;
public HomerToolStripMenuItem menuMiscUnarchivePassword;
public HomerToolStripMenuItem menuMiscCommandPrompt;
public HomerToolStripMenuItem menuMiscExplorerDir;
public HomerToolStripMenuItem menuMiscFTPPut;
public HomerToolStripMenuItem menuMiscGetFTP;
public HomerToolStripMenuItem menuMiscWebDownload;
public HomerToolStripMenuItem menuMiscEvaluate;
public HomerToolStripMenuItem menuMiscConvertUnits;
public HomerToolStripMenuItem menuMiscStartTimer;
public HomerToolStripMenuItem menuMiscStopTimer;
public HomerToolStripMenuItem menuMiscChatWithAI;
public HomerToolStripMenuItem menuMiscChatAboutFile;
public HomerToolStripMenuItem menuMiscConfigureTimer;
public HomerToolStripMenuItem menuMiscPlayList;
public HomerToolStripMenuItem menuMiscPlayFolder;
public HomerToolStripMenuItem menuMiscIterateProcesses;
public HomerToolStripMenuItem menuMiscInquireDifferences;
public HomerToolStripMenuItem menuMiscNetworkConnections;
public HomerToolStripMenuItem menuMiscVolumeFormat;
public HomerToolStripMenuItem menuMiscWindowsControlPanel;

public HomerToolStripMenuItem menuWindow;
public HomerToolStripMenuItem menuWindowArrangeIcons;
public HomerToolStripMenuItem menuWindowCascade;
public HomerToolStripMenuItem menuWindowTileHorizontal;
public HomerToolStripMenuItem menuWindowTileVertical;
public HomerToolStripMenuItem menuWindowDriveA;
public HomerToolStripMenuItem menuWindowDriveB;
public HomerToolStripMenuItem menuWindowDriveC;
public HomerToolStripMenuItem menuWindowDriveD;
public HomerToolStripMenuItem menuWindowDriveE;
public HomerToolStripMenuItem menuWindowDriveF;
public HomerToolStripMenuItem menuWindowDriveG;
public HomerToolStripMenuItem menuWindowDriveH;
public HomerToolStripMenuItem menuWindowDriveI;

public HomerToolStripMenuItem menuHelp;
public HomerToolStripMenuItem menuHelpAbout;
public HomerToolStripMenuItem menuHelpDocumentation;
public HomerToolStripMenuItem menuHelpChangeHistory;
public HomerToolStripMenuItem menuHelpKeyDescriber;
public HomerToolStripMenuItem menuHelpHotKeys;
public HomerToolStripMenuItem menuHelpContextMenu;
public HomerToolStripMenuItem menuHelpExplorerMenu;
public HomerToolStripMenuItem menuHelpSendToMenu;
public HomerToolStripMenuItem menuHelpAlternateMenu;
public HomerToolStripMenuItem menuHelpElevateVersion;
public HomerToolStripMenuItem menuHelpCopyLog;

public ToolStripStatusLabel statusLabel;
public Frame() {
this.SuspendLayout();
hashOrder_Helper();

menuMain = new MenuStrip();
/*
menuMain.AutoSize = true;
menuMain.CanOverflow = false;
menuMain.LayoutStyle = ToolStripLayoutStyle.Flow;
*/
//menuMain.Stretch = false;

menuFile = menu_Helper("&File");
menuFileRefreshFolder = menu_Helper("Refresh Folder", ". or F5", MenuFileRefreshFolder_Click);
menuFileNewItemCopy = menu_Helper("New Item Copy", "Control+Shift+N", MenuFileNewItemCopy_Click);
menuFileNewFolder = menu_Helper("&New Folder ...", "Control+N", menuFileNewFolder_Click);
menuFileOpenFolder = menu_Helper("&Open Folder ...", "Control+O", menuFileOpenFolder_Click);
menuFileGoToFolder = menu_Helper("&Go to Folder ...", "Control+G", menuFileGoToFolder_Click);
menuFileOpenSpecialFolder = menu_Helper("Open Special Folder ...", "Control+Shift+O", menuFileOpenSpecialFolder_Click);
menuFileGoToSpecialFolder = menu_Helper("Go to Special Folder ...", "Control+Shift+G", menuFileGoToSpecialFolder_Click);
menuFileOpenDrive = menu_Helper("Open Drive ...", "Alt+O", menuFileOpenDrive_Click);
menuFileGoToDrive = menu_Helper("Go to Drive ...", "Alt+G", menuFileGoToDrive_Click);
menuFileOpenVirtualFolder = menu_Helper("Open Virtual Folder ...", "Alt+Shift+O", menuFileOpenVirtualFolder_Click);
menuFileGoToVirtualFolder = menu_Helper("Go to Virtual Folder ...", "Alt+Shift+G", menuFileGoToVirtualFolder_Click);
menuFileOpenParentFolder = menu_Helper("Open Parent Folder", "Backspace", menuFileOpenParentFolder_Click);
menuFileGoToParentFolder = menu_Helper("Go to Parent Folder", ", or Shift+Backspace", menuFileGoToParentFolder_Click);
menuFileOpenRootFolder = menu_Helper("Open Root Folder", @"\", menuFileOpenRootFolder_Click);
menuFileGoToRootFolder = menu_Helper("Go to Root Folder", @"Shift+\", menuFileGoToRootFolder_Click);
menuFileOpenQuickFolder = menu_Helper("Open Quick Folder", "Control+Q", menuFileOpenQuickFolder_Click);
menuFileGoToQuickFolder = menu_Helper("Go to Quick Folder", "`", menuFileGoToQuickFolder_Click);
menuFileQuickShortcut = menu_Helper("Quick Shortcut ...", "Shift+Q", menuFileQuickShortcut_Click);
menuFileQuickURL = menu_Helper("Quick URL ...", "Alt+Shift+Q", menuFileQuickURL_Click);
menuFileFind = menu_Helper("File Find ...", "Alt+Shift+F", menuFileFind_Click);
menuFileProperties = menu_Helper("Properties ...", "Alt+Enter", menuFileProperties_Click);
menuFileOpenItem = menu_Helper("Open Item", "Enter", MenuFileOpenItem_Click);
menuFileGoToItem = menu_Helper("Go to Item", "Shift+Enter", MenuFileGoToItem_Click);
menuFilePrintTagged = menu_Helper("&Print", "Control+P", menuFilePrintTagged_Click);
menuFileRecentFolders = menu_Helper("Recent Folders ...", "Alt+R", menuFileRecentFolders_Click);
menuFileGoBack = menu_Helper("Go Back", "Alt+LeftArrow", MenuFileGoBack_Click);
menuFileGoForward = menu_Helper("Go Forward", "Alt+RightArrow", MenuFileGoForward_Click);
menuFileWindowToggle = menu_Helper("Window Toggle", "Shift+W", menuFileWindowToggle_Click);
menuFileCurrentWindows = menu_Helper("Current Windows ...", "F4", menuFileCurrentWindows_Click);
menuFileNextWindow = menu_Helper("Next Window", "Control+Tab", MenuFileNextWindow_Click);
menuFilePreviousWindow = menu_Helper("Previous Window", "Control+Shift+Tab", MenuFilePreviousWindow_Click);
menuFileClose = menu_Helper("&Close Window", "Control+F4", MenuFileClose_Click);
menuFileCloseAllButCurrent = menu_Helper("Close All But Current Window", "Control+Shift+F4", menuFileCloseAllButCurrent_Click);
menuFileExit = menu_Helper("E&xit FileDir", "Alt+F4", MenuFileExit_Click);
menuFileRestartWindows = menu_Helper("Restart Windows", "Alt+Shift+F4", MenuFileRestartWindows_Click);
//menuFile.DropDownItems.AddRange(new ToolStripItem[] {menuFileRefreshFolder, menuFileNewViewCopy,
menuFile.DropDownItems.AddRange(new ToolStripItem[] {menuFileRefreshFolder, menuFileNewItemCopy, menuFileNewFolder, menuFileOpenFolder, menuFileGoToFolder, menuFileOpenSpecialFolder, menuFileGoToSpecialFolder, menuFileOpenDrive, menuFileGoToDrive, menuFileOpenVirtualFolder, menuFileGoToVirtualFolder, menuFileOpenParentFolder, menuFileGoToParentFolder, menuFileOpenRootFolder, menuFileGoToRootFolder, menuFileOpenQuickFolder, menuFileGoToQuickFolder, menuFileQuickShortcut, menuFileQuickURL, menuFileFind, menuFileProperties, menuFileOpenItem, menuFileGoToItem, menuFilePrintTagged, menuFileRecentFolders, menuFileGoBack, menuFileGoForward, menuFileWindowToggle, menuFileCurrentWindows, menuFileNextWindow, menuFilePreviousWindow, menuFileClose, menuFileCloseAllButCurrent, menuFileExit, menuFileRestartWindows});

menuEdit = menu_Helper("&Edit");
menuEditTagAndNext = menu_Helper("Tag and Next", "> or Shift+DownArrow", MenuEditTagAndNext_Click);
menuEditUntagAndNext = menu_Helper("Untag and Next", "< or Alt+Shift+DownArrow", MenuEditUntagAndNext_Click);
menuEditTagAndPrevious = menu_Helper("Tag and Previous", "Shift+UpArrow", MenuEditTagAndPrevious_Click);
menuEditUntagAndPrevious = menu_Helper("Untag and Previous", "Alt+Shift+UpArrow", MenuEditUntagAndPrevious_Click);
menuEditTagToBottom = menu_Helper("Tag to Bottom", "Shift+End", MenuEditTagToBottom_Click);
menuEditUntagToBottom = menu_Helper("Untag to Bottom", "Alt+Shift+End", MenuEditUntagToBottom_Click);
menuEditTagToTop = menu_Helper("Tag to Top", "Shift+Home", MenuEditTagToTop_Click);
menuEditUntagToTop = menu_Helper("Untag to Top", "Alt+Shift+Home", MenuEditUntagToTop_Click);
menuEditTag = menu_Helper("Tag", "; or Shift+NumPad5", MenuEditTag_Click);
menuEditUntag = menu_Helper("Untag", "/ or Alt+Shift+NumPad5", MenuEditUntag_Click);
menuEditToggleTag = menu_Helper("Toggle Tag", "Space", MenuEditToggleTag_Click);
menuEditTagAll = menu_Helper("Tag &All", "Control+A", MenuEditTagAll_Click);
menuEditUntagAll = menu_Helper("Untag All", "Control+Shift+A", MenuEditUntagAll_Click);
menuEditTagAllFiles = menu_Helper("Tag All Files", "Alt+.", MenuEditTagAllFiles_Click);
menuEditTagDuplicateFiles = menu_Helper("Tag Duplicate Files", "Alt+Shift+.", MenuEditTagDuplicateFiles_Click);
menuEditTagSimilarFiles = menu_Helper("Tag Similar Files", "Alt+Shift+,", MenuEditTagSimilarFiles_Click);
menuEditReorderNames = menu_Helper("Reorder Names", "Alt+Shift+K", MenuEditReorderNames_Click);
menuEditFindDuplicates = menu_Helper("Find Duplicates in Tree", "Alt+Shift+J", MenuEditFindDuplicates_Click);
menuEditTagWithRegularExpression = menu_Helper("Tag with Regular Expression ...", "Control+Shift+.", MenuEditTagWithRegularExpression_Click);
menuEditUntagAllButCurrent = menu_Helper("Untag All But Current", "Alt+,", MenuEditUntagAllButCurrent_Click);
menuEditStartTagOrUntag = menu_Helper("Start Tag or Untag", "F8", MenuEditStartTagOrUntag_Click);
menuEditCompleteTag = menu_Helper("Complete Tag", "Shift+F8", MenuEditCompleteTag_Click);
menuEditCompleteUntag = menu_Helper("Complete Untag", "Alt+Shift+F8", MenuEditCompleteUntag_Click);
menuEditInvertTagged = menu_Helper("&Invert Tagged", "Control+I", MenuEditInvertTagged_Click);
menuEditSaveTags = menu_Helper("&Save Tags", "Control+S", MenuEditSaveTags_Click);
menuEditRestoreTags = menu_Helper("Restore Tags", "Control+Shift+S", MenuEditRestoreTags_Click);
menuEditCopyToClipboardTagged = menu_Helper("&Copy", "Control+C", menuEditCopyToClipboardTagged_Click);
menuEditCopyAppendToClipboardTagged = menu_Helper("Copy Append", "Alt+C", menuEditCopyAppendToClipboardTagged_Click);
menuEditCutToClipboardTagged = menu_Helper("Cut", "Control+X", menuEditCutToClipboardTagged_Click);
//menuEditCopyPathTagged = menu_Helper("Copy Path Tagged to Clipboard", "Alt+C", menuEditCopyPathTagged_Click);
menuEditCopyNameTagged = menu_Helper("Copy Name", "Control+Shift+C", menuEditCopyNameTagged_Click);
menuEditPathList = menu_Helper("Path List to Clipboard", "Control+Shift+P", menuEditPathList_Click);
menuEditCopyTagged = menu_Helper("Copy to Folder", "Shift+C", menuEditCopyTagged_Click);
menuEditMoveTagged = menu_Helper("Move to Folder", "Shift+M", menuEditMoveTagged_Click);
menuEditDeleteTagged = menu_Helper("Delete", "Delete", menuEditDeleteTagged_Click);
menuEditDeleteTaggedWithoutRecycle = menu_Helper("Delete without Recycle", "Shift+Delete", menuEditDeleteTaggedWithoutRecycle_Click);
menuEditDeleteAndRecycleTagged = menu_Helper("Delete and Recycle", "Control+Delete", MenuEditDeleteAndRecycleTagged_Click);
menuEditDeleteFileNow = menu_Helper("Delete and Recycle File Now", "Control+D", MenuEditDeleteFileNow_Click);
menuEditDeleteFileNowWithoutRecycle = menu_Helper("Delete File Now", "Control+Shift+D", MenuEditDeleteFileNowWithoutRecycle_Click);
menuEditRename = menu_Helper("Rename ...", "Shift+R or F2", menuEditRename_Click);
menuEditRenameWithWildcards = menu_Helper("Rename with Wildcards ...", "Control+R", menuEditRenameWithWildcards_Click);
menuEditRenameWithRegex = menu_Helper("Rename with Regular Expression ...", "Control+Shift+R", menuEditRenameWithRegex_Click);
menuEditRenameToTitle = menu_Helper("Rename to Identify Content", "Control+Shift+I", MenuEditRenameToTitle_Click);
menuEditPasteFromClipboard = menu_Helper("Paste", "Control+V", menuEditPasteFromClipboard_Click);
menuEditPasteCopy = menu_Helper("Paste Copy", "Alt+V", menuEditPasteCopy_Click);
menuEditPasteMove = menu_Helper("Paste Move", "Alt+Shift+V", menuEditPasteMove_Click);
menuEditStampTagged = menu_Helper("Stamp with Date and Time ...", "Shift+1 or !", menuEditStampTagged_Click);
menuEditHideTagged = menu_Helper("Hide", ")", menuEditHideTagged_Click);
menuEditShowTagged = menu_Helper("Show", "(", menuEditShowTagged_Click);
menuEditReadOnlyTagged = menu_Helper("ReadOnly", "]", menuEditReadOnlyTagged_Click);
menuEditReadWriteTagged = menu_Helper("ReadWrite", "[", menuEditReadWriteTagged_Click);
menuEditSystemTagged = menu_Helper("System", "}", menuEditSystemTagged_Click);
menuEditGeneralTagged = menu_Helper("General", "{", menuEditGeneralTagged_Click);
menuEditPathToClipboard = menu_Helper("Path to Clipboard", "Alt+Shift+P", menuEditPathToClipboard_Click);
menuEditShortPathToClipboard = menu_Helper("Short Path to Clipboard", "~", menuEditShortPathToClipboard_Click);
menuEditFullFolderToClipboard = menu_Helper("Folder to Clipboard", "Control+Shift+'", menuEditFullFolderToClipboard_Click);
menuEditClearClipboard = menu_Helper("Clear Clipboard", "Alt+Shift+'", menuEditClearClipboard_Click);
menuEditExportClipboardToFile = menu_Helper("Export Clipboard to File ...", "Alt+Shift+E", menuEditExportClipboardToFile_Click);
menuEdit.DropDownItems.AddRange(new ToolStripItem[] {menuEditTagAndNext, menuEditUntagAndNext, menuEditTagAndPrevious, menuEditUntagAndPrevious, menuEditTagToBottom, menuEditUntagToBottom, menuEditTagToTop, menuEditUntagToTop, menuEditTag, menuEditUntag, menuEditToggleTag, menuEditTagAll, menuEditUntagAll, menuEditTagAllFiles, menuEditTagDuplicateFiles, menuEditTagSimilarFiles, menuEditReorderNames, menuEditFindDuplicates, menuEditTagWithRegularExpression, menuEditUntagAllButCurrent, menuEditStartTagOrUntag, menuEditCompleteTag, menuEditCompleteUntag, menuEditInvertTagged, menuEditSaveTags, menuEditRestoreTags, menuEditCopyToClipboardTagged, menuEditCopyAppendToClipboardTagged, menuEditCutToClipboardTagged, menuEditCopyNameTagged, menuEditPathList, menuEditCopyTagged, menuEditMoveTagged, menuEditDeleteTagged, menuEditDeleteAndRecycleTagged, menuEditDeleteTaggedWithoutRecycle, menuEditDeleteFileNow, menuEditDeleteFileNowWithoutRecycle, menuEditRename, menuEditRenameWithWildcards, menuEditRenameWithRegex, menuEditRenameToTitle, menuEditPasteFromClipboard, menuEditPasteCopy, menuEditStampTagged, menuEditHideTagged, menuEditShowTagged, menuEditReadOnlyTagged, menuEditReadWriteTagged, menuEditReadWriteTagged, menuEditSystemTagged, menuEditGeneralTagged, menuEditPathToClipboard, menuEditShortPathToClipboard, menuEditFullFolderToClipboard, menuEditClearClipboard, menuEditExportClipboardToFile});

menuNavigate = menu_Helper("&Navigate");
menuNavigateJump = menu_Helper("&Jump ...", "Control+J", menuNavigateJump_Click);
menuNavigateJumpAgain = menu_Helper("Jump Again", "Alt+J", menuNavigateJumpAgain_Click);
menuNavigateKeywords = menu_Helper("&Keywords ...", "Control+K", menuNavigateKeywords_Click);
menuNavigateKeywordsAgain = menu_Helper("Keywords Again", "Alt+K", menuNavigateKeywordsAgain_Click);
menuNavigateSetFilter = menu_Helper("Set &Filter ...", "Control+F", menuNavigateSetFilter_Click);
menuNavigateClearFilter = menu_Helper("Clear Filter", "Control+Shift+F", menuNavigateClearFilter_Click);
menuNavigateBeginningFile = menu_Helper("Beginning File", "Alt+B", menuNavigateBeginningFile_Click);
menuNavigateBeginningTagged = menu_Helper("Beginning Tagged", "Shift+B or Control+Home", menuNavigateBeginningTagged_Click);
menuNavigateEndTagged = menu_Helper("End Tagged", "Shift+E or Control+End", menuNavigateEndTagged_Click);
menuNavigateNextTagged = menu_Helper("Next Tagged", "Shift+N or Control+DownArrow", menuNavigateNextTagged_Click);
menuNavigatePreviousTagged = menu_Helper("Previous Tagged", "Shift+P or Control+UpArrow", menuNavigatePreviousTagged_Click);
menuNavigateInitialChange = menu_Helper("Initial Change", "Shift+I", menuNavigateInitialChange_Click);
menuNavigateExtensionChange = menu_Helper("Extension Change", "Shift+X", menuNavigateExtensionChange_Click);
menuNavigate.DropDownItems.AddRange(new ToolStripItem[] {menuNavigateJump, menuNavigateJumpAgain, menuNavigateKeywords, menuNavigateKeywordsAgain, menuNavigateSetFilter, menuNavigateClearFilter, menuNavigateBeginningFile, menuNavigateBeginningTagged, menuNavigateEndTagged, menuNavigateNextTagged, menuNavigatePreviousTagged, menuNavigateInitialChange, menuNavigateExtensionChange});

menuQuery = menu_Helper("&Query");
menuQueryDate = menu_Helper("Date", "Shift+D", menuQueryDate_Click);
menuQueryList = menu_Helper("List", "Control+L", menuQueryList_Click);
menuQueryListTagged = menu_Helper("List Tagged", "Shift+L", menuQueryListTagged_Click);
menuQuerySelected = menu_Helper("Selected", "Shift+Space", MenuQuerySelected_Click);
menuQueryListFiles = menu_Helper("List Files", "Alt+L", menuQueryListFiles_Click);
menuQueryPath = menu_Helper("Path", "Alt+P", menuQueryPath_Click);
menuQuerySize = menu_Helper("Size", "Shift+S", menuQuerySize_Click);
menuQueryType = menu_Helper("Type", "Shift+T", menuQueryType_Click);
menuQueryTypeExtended = menu_Helper("Type Extended", "Control+Shift+T", menuQueryTypeExtended_Click);
menuQueryWindowsOpen = menu_Helper("Windows Open", "Shift+F4 or Alt+NumPad5", MenuQueryWindowsOpen_Click);
menuQueryYield = menu_Helper("Yield", "Control+Y", menuQueryYield_Click);
menuQueryYieldTagged = menu_Helper("Yield Tagged", "Shift+Y", menuQueryYieldTagged_Click);
menuQueryYieldFiles = menu_Helper("Yield Files", "Alt+Y", menuQueryYieldFiles_Click);
menuQueryYieldOnDrive = menu_Helper("Yield on Drive", "Control+Shift+Y", menuQueryYieldOnDrive_Click);
menuQueryYieldInOperatingSystem = menu_Helper("Yield in Operating System", "Alt+Shift+Y", MenuQueryYieldInOperatingSystem_Click);
menuQueryStatus = menu_Helper("Status", "Alt+Z", menuQueryStatus_Click);
menuQueryCharacterEncoding = menu_Helper("Character Encoding", "Shift+2 or @", MenuQueryCharacterEncoding_Click);
menuQueryPercentThrough = menu_Helper("Percent Through", "Shift+5 or %", menuQueryPercentThrough_Click);
menuQueryFilter = menu_Helper("Filter and Order", "Shift+8", menuQueryFilter_Click);
menuQueryName = menu_Helper("Item Name", "'", menuQueryName_Click);
menuQueryFolderName = menu_Helper("Folder Name", "Shift+'", menuQueryFolderName_Click);
menuQueryFullFolder = menu_Helper("Folder", "Control+'", menuQueryFullFolder_Click);
menuQueryClipboard = menu_Helper("Clipboard", "Alt+'", menuQueryClipboard_Click);
menuQueryNow = menu_Helper("Time", "Alt+;", menuQueryNow_Click);
menuQueryWhat = menu_Helper("What Content", "?", menuQueryWhat_Click);
menuQueryTimer = menu_Helper("Timer", "Alt+Control+Y", menuQueryTimer_Click);
menuQuery.DropDownItems.AddRange(new ToolStripItem[] {menuQueryDate, menuQueryList, menuQueryListTagged, menuQuerySelected, menuQueryListFiles, menuQueryPath, menuQuerySize, menuQueryType, menuQueryTypeExtended, menuQueryWindowsOpen, menuQueryYield, menuQueryYieldTagged, menuQueryYieldFiles, menuQueryYieldOnDrive, menuQueryYieldInOperatingSystem, menuQueryStatus, menuQueryCharacterEncoding, menuQueryPercentThrough, menuQueryFilter, menuQueryName, menuQueryFolderName, menuQueryFullFolder, menuQueryClipboard, menuQueryNow, menuQueryWhat, menuQueryTimer});

menuMisc = menu_Helper("&Misc");
menuMiscConfigurationOptions = menu_Helper("Configuration Options", "Alt+Shift+C", menuMiscOptions_Click);
menuMiscManualOptions = menu_Helper("Manual Options", "Alt+Shift+M", menuMiscManualOptions_Click);
menuMiscExtraSpeechToggle = menu_Helper("Extra Speech Toggle", "Control+Shift+X", menuMiscExtraSpeechToggle_Click);
menuMiscExtraSpeechLog = menu_Helper("Extra Speech Log", "Alt+Shift+X", menuMiscExtraSpeechLog_Click);
menuMiscEnvironmentVariables = menu_Helper("&Environment Variables ...", "Control+E", menuMiscEnvironmentVariables_Click);
menuMiscRecycleToggle = menu_Helper("Recycle Toggle", "Alt+Shift+R", menuMiscRecycleToggle_Click);
menuMiscOpenRecycleBin = menu_Helper("Recycle Bin", "Control+B", menuMiscOpenRecycleBin_Click);
menuMiscAlphaOrder = menu_Helper("Alpha Order", "Alt+A", menuMiscAlphaOrder_Click);
menuMiscReverseAlphaOrder = menu_Helper("Reverse Alpha Order", "Alt+Shift+A", menuMiscReverseAlphaOrder_Click);
menuMiscDateOrder = menu_Helper("Date Order", "Alt+D", menuMiscDateOrder_Click);
menuMiscReverseDateOrder = menu_Helper("Reverse Date Order", "Alt+Shift+D", menuMiscReverseDateOrder_Click);
menuMiscSizeOrder = menu_Helper("Size Order", "Alt+S", menuMiscSizeOrder_Click);
menuMiscReverseSizeOrder = menu_Helper("Reverse Size Order", "Alt+Shift+S", menuMiscReverseSizeOrder_Click);
menuMiscTypeOrder = menu_Helper("Type Order", "Alt+T", menuMiscTypeOrder_Click);
menuMiscReverseTypeOrder = menu_Helper("Reverse Type Order", "Alt+Shift+T", menuMiscReverseTypeOrder_Click);
menuMiscSendToWordProcessor = menu_Helper("Send to Word Processor", "Control+W", MenuMiscSendToWordProcessor_Click);
menuMiscSendToTextEditor = menu_Helper("Send to Text Editor", "Control+T", MenuMiscSendToTextEditor_Click);
menuMiscOutputTagged = menu_Helper("Output Type", "Shift+O", MenuMiscOutputTagged_Click);
menuMiscAppendTagged = menu_Helper("Append to Clipboard", "Shift+A", MenuMiscAppendTagged_Click);
menuMiscConvertEncodingTagged = menu_Helper("Convert Encoding", "Control+2", MenuMiscConvertEncodingTagged_Click);
menuMiscTranslateTagged = menu_Helper("Translate File", "Alt+Shift+F7", MenuMiscTranslateTagged_Click);
menuMiscExtractTagged = menu_Helper("Extract with Regular Expression", "Control+Shift+E", MenuMiscExtractTagged_Click);
menuMiscBurnTagged = menu_Helper("Burn to CD", "Alt+Shift+B", menuMiscBurnTagged_Click);
menuMiscMailBody = menu_Helper("Mail Body", "Control+M", menuMiscMailBody_Click);
menuMiscMailAttachTagged = menu_Helper("Mail Attachment", "Control+Shift+M", menuMiscMailAttachTagged_Click);
menuMiscBatchMail = menu_Helper("Batch Mail", "Control+Shift+B", menuMiscBatchMail_Click);
menuMiscZipTagged = menu_Helper("Zip ...", "Shift+Z", menuMiscZipTagged_Click);
menuMiscZipTaggedThenDelete = menu_Helper("Zip then Delete ...", "Control+Z", menuMiscZipTaggedThenDelete_Click);
menuMiscZipList = menu_Helper("Zip List ...", "Control+Shift+Z", menuMiscZipList_Click);
menuMiscUnarchiveTagged = menu_Helper("Unarchive", "Shift+U", menuMiscUnarchiveTagged_Click);
menuMiscUnarchiveTaggedWithoutSubfolders = menu_Helper("Unarchive without Subfolders", "Control+U", menuMiscUnarchiveTaggedWithoutSubfolders_Click);
menuMiscUnarchiveTaggedToSameName = menu_Helper("Unarchive to Same Name", "Control+Shift+U", menuMiscUnarchiveTaggedToSameName_Click);
menuMiscUnarchiveTest = menu_Helper("Unarchive Test", "Alt+U", menuMiscUnarchiveTest_Click);
menuMiscUnarchivePassword = menu_Helper("Unarchive Password", "Alt+Shift+U", menuMiscUnarchivePassword_Click);
menuMiscCommandPrompt = menu_Helper("Command Prompt", "Control+/", menuMiscCommandPrompt_Click);
menuMiscExplorerDir = menu_Helper("Explorer Directory", "Alt+/", menuMiscExplorerDir_Click);
menuMiscFTPPut = menu_Helper("FTP Put ...", "Shift+F", menuMiscFTPPut_Click);
menuMiscGetFTP = menu_Helper("Get FTP ...", "Shift+G", menuMiscGetFTP_Click);
menuMiscWebDownload = menu_Helper("Web Download ...", "Alt+Shift+W", menuMiscWebDownload_Click);
menuMiscEvaluate = menu_Helper("Evaluate Expression ...", "Control+Equals", menuMiscEvaluate_Click);
menuMiscConvertUnits = menu_Helper("Convert Units ...", "Shift+3", menuMiscConvertUnits_Click);
menuMiscStartTimer = menu_Helper("Start Timer ...", "Alt+Control+T", menuMiscStartTimer_Click);
menuMiscStopTimer = menu_Helper("Stop Timer", "Alt+Control+S", menuMiscStopTimer_Click);
menuMiscChatWithAI = menu_Helper("Chat with AI", "F12", MenuMiscChatWithAI_Click);
menuMiscChatAboutFile = menu_Helper("Chat about File", "Shift+F12", MenuMiscChatAboutFile_Click);
// menuMiscConfigureTimer = menu_Helper("Configure Timer", "Control+F12", menuMiscConfigureTimer_Click);
menuMiscPlayList = menu_Helper("Play List", "Control+Shift+L", menuMiscPlayList_Click);
menuMiscPlayFolder = menu_Helper("Play Media", "Alt+Shift+L", MenuMiscPlayFolder_Click);
menuMiscIterateProcesses = menu_Helper("Iterate Processes ...", "Alt+I", MenuMiscIterateProcesses_Click);
menuMiscInquireDifferences = menu_Helper("Inquire Differences ...", "Alt+Shift+I", MenuMiscInquireDifferences_Click);
menuMiscNetworkConnections = menu_Helper("Network Connections ...", "Alt+Shift+N", MenuMiscNetworkConnections_Click);
menuMiscVolumeFormat = menu_Helper("Volume Format ...", "Control+Shift+V", menuMiscVolumeFormat_Click);
menuMiscWindowsControlPanel = menu_Helper("Windows Control Panel ...", "Control+Shift+W", MenuMiscWindowsControlPanel_Click);
menuMisc.DropDownItems.AddRange(new ToolStripItem[] {menuMiscConfigurationOptions, menuMiscManualOptions, menuMiscExtraSpeechToggle, menuMiscExtraSpeechLog, menuMiscEnvironmentVariables, menuMiscRecycleToggle, menuMiscOpenRecycleBin, menuMiscDateOrder, menuMiscReverseDateOrder, menuMiscAlphaOrder, menuMiscReverseAlphaOrder, menuMiscSizeOrder, menuMiscReverseSizeOrder, menuMiscTypeOrder, menuMiscReverseTypeOrder, menuMiscSendToWordProcessor, menuMiscSendToTextEditor, menuMiscOutputTagged, menuMiscAppendTagged, menuMiscConvertEncodingTagged, menuMiscTranslateTagged, menuMiscExtractTagged, menuMiscBurnTagged, menuMiscMailBody, menuMiscMailAttachTagged, menuMiscBatchMail, menuMiscZipTagged, menuMiscZipTaggedThenDelete, menuMiscZipList, menuMiscUnarchiveTagged, menuMiscUnarchiveTaggedWithoutSubfolders, menuMiscUnarchiveTaggedToSameName, menuMiscUnarchivePassword, menuMiscUnarchiveTest, menuMiscCommandPrompt, menuMiscExplorerDir, menuMiscFTPPut, menuMiscGetFTP, menuMiscWebDownload, menuMiscEvaluate, menuMiscConvertUnits, menuMiscStartTimer, menuMiscStopTimer, menuMiscChatWithAI, menuMiscChatAboutFile, menuMiscPlayList, menuMiscPlayFolder, menuMiscIterateProcesses, menuMiscInquireDifferences, menuMiscNetworkConnections, menuMiscVolumeFormat, menuMiscWindowsControlPanel});

menuWindow = menu_Helper("&Window");
menuWindowArrangeIcons = menu_Helper("Arrange Icons", "Alt+F11", menuWindowArrangeIcons_Click);
menuWindowCascade = menu_Helper("Cascade", "Control+F11", menuWindowCascade_Click);
menuWindowTileHorizontal = menu_Helper("Tile Horizontal", "Alt+Shift+F11", menuWindowTileHorizontal_Click);
menuWindowTileVertical = menu_Helper("Tile Vertical", "Control+Shift+F11", menuWindowTileVertical_Click);
menuWindowDriveA = menu_Helper("Drive &A", "Alt+1", menuWindowDrive_Click);
menuWindowDriveB = menu_Helper("Drive &B", "Alt+2", menuWindowDrive_Click);
menuWindowDriveC = menu_Helper("Drive &C", "Alt+3", menuWindowDrive_Click);
menuWindowDriveD = menu_Helper("Drive &D", "Alt+4", menuWindowDrive_Click);
menuWindowDriveE = menu_Helper("Drive &E", "Alt+5", menuWindowDrive_Click);
menuWindowDriveF = menu_Helper("Drive &F", "Alt+6", menuWindowDrive_Click);
menuWindowDriveG = menu_Helper("Drive &G", "Alt+7", menuWindowDrive_Click);
menuWindowDriveH = menu_Helper("Drive &H", "Alt+8", menuWindowDrive_Click);
menuWindowDriveI = menu_Helper("Drive &I", "Alt+9", menuWindowDrive_Click);
menuWindow.DropDownItems.AddRange(new ToolStripItem[] {menuWindowArrangeIcons, menuWindowCascade, menuWindowTileHorizontal, menuWindowTileVertical, menuWindowDriveA, menuWindowDriveB, menuWindowDriveC, menuWindowDriveD, menuWindowDriveE, menuWindowDriveF, menuWindowDriveG, menuWindowDriveH, menuWindowDriveI});

menuHelp = menu_Helper("&Help");
menuHelpAbout = menu_Helper("&About", "Alt+F1", MenuHelpAbout_Click);
menuHelpDocumentation = menu_Helper("Documentation", "F1", MenuHelpDocumentation_Click);
menuHelpChangeHistory = menu_Helper("History of Changes", "Shift+F1", MenuHelpChangeHistory_Click);
menuHelpKeyDescriber = menu_Helper("Key Describer", "Control+F1", MenuHelpKeyDescriber_Click);
menuHelpHotKeys = menu_Helper("Hotkey Summary", "Alt+Shift+H", MenuHelpHotKeys_Click);
menuHelpContextMenu = menu_Helper("Context Menu ...", "Shift+F10", MenuHelpContextMenu_Click);
//menuHelpExplorerMenu = menu_Helper("Explorer Menu ...", "Alt+Shift+F10", MenuHelpExplorerMenu_Click);
menuHelpSendToMenu = menu_Helper("SendTo Menu ...", "Control+F10", MenuHelpSendToMenu_Click);
menuHelpAlternateMenu = menu_Helper("Alternate Menu ...", "Alt+F10", MenuHelpAlternateMenu_Click);
menuHelpElevateVersion = menu_Helper("Elevate Version", "F11", menuHelpElevateVersion_Click);
menuHelpCopyLog = menu_Helper("Copy Log", "Control+F12", MenuHelpCopyLog_Click);
menuHelp.DropDownItems.AddRange(new ToolStripItem[] {menuHelpAbout, menuHelpDocumentation, menuHelpChangeHistory, menuHelpKeyDescriber, menuHelpHotKeys, menuHelpContextMenu, menuHelpSendToMenu, menuHelpAlternateMenu, menuHelpElevateVersion, menuHelpCopyLog});
//menuHelp.DropDownItems.AddRange(new ToolStripItem[] {menuHelpAbout, menuHelpDocumentation, menuHelpChangeHistory, menuHelpHotKeys, menuHelpContextMenu, menuHelpExplorerMenu, menuHelpSendToMenu, menuHelpAlternateMenu, menuHelpElevateVersion});

menuMain.Items.AddRange(new ToolStripItem[] {menuFile, menuEdit, menuQuery, menuNavigate, menuMisc, menuWindow, menuHelp});
menuMain.Dock = DockStyle.Top;
menuMain.MdiWindowListItem = menuWindow;

StatusStrip statusBar = new StatusStrip();
statusBar.SuspendLayout();
//ToolStripStatusLabel statusLabel = new ToolStripStatusLabel("Ready");
statusLabel = new ToolStripStatusLabel("Ready");
statusLabel.AutoSize = false;
statusLabel.Width = 400;
statusBar.Items.AddRange(new ToolStripItem[] {statusLabel});
statusBar.AutoSize = true;
statusBar.Dock = DockStyle.Bottom;
statusBar.ResumeLayout(false);

//this.Controls.AddRange(new Control[] { menuMain, statusBar});
this.Controls.AddRange(new Control[] { statusBar, menuMain});
//this.AutoSize = true;
this.IsMdiContainer = true;
string sDir = App.sDefaultDir;
string sFilter = App.sDefaultFilter;
string sOrder = App.sDefaultOrder;
//if (!Directory.Exists(sDir)) sDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
if (!App.itemExists(sDir)) sDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
string[] aArgs = Environment.GetCommandLineArgs();
if (aArgs != null && aArgs.Length > 1 && aArgs[1].Length > 0) sDir = aArgs[1];
sDir = sDir.Trim('"');
string sZip = "";
string[] aPaths = null;
if (!Directory.Exists(sDir) && File.Exists(sDir)) {
sZip = sDir;
sDir = "";
// if (Path.GetExtension(sZip).ToLower() != ".zip") aPaths = Homer.Util.file2String(sZip).Replace("\r\n", "\n").Trim().Split('\n');
if (!testZip(sZip)) aPaths = Homer.Util.file2String(sZip).Replace("\r\n", "\n").Trim().Split('\n');
}
this.KeyPreview = true;
//Lbc.Show(sZip, "here");
//MdiChild mdiChild = null;
//mdiChild = new MdiChild(this, sDir, sOrder, sFilter, sZip);
//this.Load += delegate(object sender, EventArgs e) {MdiChild form = new MdiChild(this, sDir, sOrder, sFilter, sZip);};
this.Load += delegate(object sender, EventArgs e) {MdiChild mdiChild = new MdiChild(this, sDir, sOrder, sFilter, sZip, aPaths);};
this.MainMenuStrip = menuMain;
//this.Size = new Size(440, 600);
this.Size = new Size(600, 600);
this.WindowState = FormWindowState.Maximized;
this.StartPosition = FormStartPosition.CenterScreen;
this.Text = "FileDir";
this.ResumeLayout();
// Attach Homer's UIA live region to the top-level MDI frame so speech
// announcements have a notification source. Done once; safe before Show.
Homer.Say.attach(this);
//this.Shown += delegate(object sender, EventArgs e) { App.say(Homer.Util.stringPlural("item", mdiChild.bs.Count)); };
this.Show();
} // Frame method

void hashOrder_Helper() {
hashOrder.Clear();
if (App.DirsBeforeFiles) {
hashOrder.Add("Type desc, Name", "Alpha");
hashOrder.Add("Type desc, Name desc", "Reverse Alpha");
hashOrder.Add("Type desc, Time", "Date");
hashOrder.Add("Type desc, Time desc", "Reverse Date");
hashOrder.Add("Type desc, Size", "Size");
hashOrder.Add("Type desc, Size desc", "Reverse Size");
hashOrder.Add("Type desc, Ext", "Type");
hashOrder.Add("Type desc, Ext desc", "Reverse Type");
}
else {
hashOrder.Add("Type asc, Name", "Alpha");
hashOrder.Add("Type asc, Name desc", "Reverse Alpha");
hashOrder.Add("Type asc, Time", "Date");
hashOrder.Add("Type asc, Time desc", "Reverse Date");
hashOrder.Add("Type asc, Size", "Size");
hashOrder.Add("Type asc, Size desc", "Reverse Size");
hashOrder.Add("Type asc, Ext", "Type");
hashOrder.Add("Type asc, Ext desc", "Reverse Type");
}
foreach (MdiChild mdiChild in this.MdiChildren) {
if (App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type asc", "Type desc");
else mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
BindingSource bs = mdiChild.bs;
bs.Sort = mdiChild.sOrderText;
// keep focused item
if (bs.Count > 0) bs.Position = 0;
}

if (App.DirsBeforeFiles) App.sOrderText = App.sOrderText.Replace("Type asc", "Type desc");
else App.sOrderText = App.sOrderText.Replace("Type desc", "Type asc");

} // hashOrder_Helper method

HomerToolStripMenuItem menu_Helper(string sText) {
return new HomerToolStripMenuItem(sText);
} // menu_Helper method

HomerToolStripMenuItem menu_Helper(string sText, string sKeys, EventHandler eh) {
// Microsoft.VisualBasic.FileIO.FileSystem.WriteAllText(@"c:\FileDir\Program.ini", sText + "=" + sKeys + "\r\n", true);
//sText += "\t" + sKeys;
//sText += "   " + sKeys;
HomerToolStripMenuItem menuItem = new HomerToolStripMenuItem(sText, null, eh);
menuItem.AccessibleName = (sText + "   " + sKeys).Replace("&", "");
menuItem.ShortcutKeyDisplayString = sKeys;

menuItem.Paint += delegate(object oSender, PaintEventArgs e) {
foreach (ToolStripMenuItem menu in App.frame.menuMain.Items) {
foreach (object o in menu.DropDownItems) {
HomerToolStripMenuItem item = o as HomerToolStripMenuItem;
if (item == null) continue;
if (!item.Selected) continue;
string[] aSummary = item.getKeySummary();
// string sSummary = aSummary[0] + " = " + aSummary[1] + ", " + aSummary[2];
string sDescription = aSummary[2];
if (sDescription != App.frame.LastDescription) {
App.frame.statusLabel.Text = sDescription;
App.frame.LastDescription = sDescription;
}
break;
}
}
};

return menuItem;
} // menu_Helper method

public bool abortInZip() {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild.InZip) {
App.say("Command unavailable in archive view!");
return true;
}
else return false;
} // abortInZip method

void menuFileNewFolder_Click(object sender, EventArgs e) {
App.say("New folder");
string sDir = Lbc.InputDialog("Input", "Folder", "", "NewFolder").Trim();
if (sDir == "") return;
if (Directory.Exists(sDir)) App.say(sDir + " already exists!");
else {
try {
Directory.CreateDirectory(sDir);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
//App.say("Cannot create " + sDir);
return;
}
}
App.say("Done!", true);

sDir = Path.GetFullPath(sDir);
App.sGoToText = sDir;
App.sOpenText = sDir;
if (Homer.Util.stringEquiv(Directory.GetCurrentDirectory(), Path.GetDirectoryName(sDir))) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sGoToText = sDir;
refresh_Helper(sDir);
goTo_Helper(sDir);
}
} // menuFileNewFolder_Click method

void refreshFolder_Helper(MdiChild mdiChild) {
//Lbc.Show(mdiChild.Text);
string[] aTagged = getTagged();
string sPath = Path_Helper();
MdiChild form = new MdiChild(App.frame, "", mdiChild.sOrderText, mdiChild.sFilterText);
mdiChild.Dispose();
applyTagged(aTagged);
goTo_Helper(sPath);
} // refreshFolder_Helper method

void MenuFileRefreshFolder_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Refresh folder");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
refreshFolder_Helper(mdiChild);
} // menuFileRefreshFolder_Click method

void MenuFileNewViewCopy_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("New view copy");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataTable tbl = mdiChild.tbl.Copy();
int i = mdiChild.bs.Position;
MdiChild form = new MdiChild(App.frame, "", mdiChild.sOrderText, mdiChild.sFilterText);
form.tbl = tbl;
//if (form.bs == null) Lbc.Show("null");
//form.bs.Position = i;
} // menuFileNewViewCopy_Click method

void MenuFileNewItemCopy_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

string sSource = Path_Helper();
string sDir = Path.GetDirectoryName(sSource);
string sTarget = Homer.Util.getUniqueName(sSource);
string sName = Path.GetFileName(sTarget);
//string sNewName = Lbc.InputDialog("Input", "&Name", sName);
//if (sNewName == "") return;
//string sTarget = Path.Combine(sDir, sNewName);
if (Directory.Exists(sSource)) {
App.say("New folder copy");
App.copyDirectory(sSource, sTarget, App.Recycle);
}
else if (File.Exists(sSource)) {
App.say("New file copy");
App.copyFile(sSource, sTarget, App.Recycle);
}
else {
App.say(sName + " not found!");
return;
}

refresh_Helper(sTarget);
goTo_Helper(sTarget);
} // menuFileNewItemCopy_Click method

void menuFileOpenFolder_Click(object sender, EventArgs e) {
openFolder_Helper("Open folder");
} // menuFileOpenFolder_Click method

void menuFileOpenSpecialFolder_Click(object sender, EventArgs e) {
openSpecialFolder_Helper("Open special folder");
} // menuFileOpenSpecialFolder_Click method

void menuFileGoToSpecialFolder_Click(object sender, EventArgs e) {
openSpecialFolder_Helper("Go to special folder");
} // menuFileGoToSpecialFolder_Click method

void openSpecialFolder_Helper(string sLabel) {
App.say(sLabel);
bool bOpen = sLabel.StartsWith("Open") ? true : false;

string sDir = Lbc.PickSpecialFolder();
if (sDir.Length == 0) return;
if (!Directory.Exists(sDir)) {
App.say("Folder " + sDir + " not found!");
return;
}

if (bOpen) App.sOpenText = sDir;
else {
App.sGoToText = sDir;
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
if (mdiChild != null) mdiChild.Dispose();
}

activate_Helper(sDir);
} // openSpecialFolder_Helper method


void menuFileGoToFolder_Click(object sender, EventArgs e) {
openFolder_Helper("Go to folder");
} // menuFileGoToFolder_Click method

void openFolder_Helper(string sLabel) {
App.say(sLabel);
bool bOpen = sLabel.StartsWith("Open") ? true : false;
string sDefaultDir;
if (bOpen)sDefaultDir = App.sOpenText;
else sDefaultDir = App.sGoToText;

string sDir = Lbc.DirectoryDialog("Input", "Folder", sDefaultDir).Trim();
if (sDir.Length == 0) return;
if (!Directory.Exists(sDir)) {
App.say("Folder " + sDir + " not found!");
return;
}

if (bOpen) App.sOpenText = sDir;
else {
App.sGoToText = sDir;
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
if (mdiChild != null) mdiChild.Dispose();
}

activate_Helper(sDir);
} // openFolder_Helper method

void menuFileOpenDrive_Click(object sender, EventArgs e) {
openDrive_Helper("Open drive");
} // menuFileOpenDrive_Click method

void menuFileGoToDrive_Click(object sender, EventArgs e) {
openDrive_Helper("Go to drive");
} // menuFileGoToDrive_Click method

void openDrive_Helper(string sLabel) {
App.say(sLabel);
bool bOpen = sLabel.StartsWith("Open") ? true : false;

string sName = "";
string sNameList = "";
//string[] aNames = Environment.GetLogicalDrives();
DriveInfo[] allDrives = DriveInfo.GetDrives();
foreach (DriveInfo d in allDrives){
sName = d.Name.Substring(0, 1);
sName += "\t" + d.DriveType;
if (d.IsReady && d.VolumeLabel != "") sName += "\t" + d.VolumeLabel;
sNameList += sName + "\n";
}
string[] aNames = sNameList.Trim().Split('\n');
sName = Lbc.ListDialog("Pick", "", aNames, true, 0);
if (sName.Length == 0) return;

string sDir = sName.Substring(0, 1) + ":";
DriveInfo drive = new DriveInfo(sDir);
if (!drive.IsReady) {
App.say("Drive not ready!");
return;
}

App.SetDrive(sDir);
sDir = Directory.GetCurrentDirectory();

if (bOpen) App.sOpenText = sDir;
else {
App.sGoToText = sDir;
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
if (mdiChild != null) mdiChild.Dispose();
}

activate_Helper(sDir);
} // openDrive_Helper method

void menuFileOpenVirtualFolder_Click(object sender, EventArgs e) {
openVirtualFolder_Helper("Open virtual folder");
} // menuFileOpenVirtualFolder_Click method

void menuFileGoToVirtualFolder_Click(object sender, EventArgs e) {
openVirtualFolder_Helper("Go to virtual folder");
} // menuFileGoToVirtualFolder_Click method

void openVirtualFolder_Helper(string sLabel) {
App.say(sLabel);
bool bOpen = sLabel.StartsWith("Open") ? true : false;
string sDefaultDir;
if (bOpen)sDefaultDir = App.sOpenText;
else sDefaultDir = App.sGoToText;

string sFilter = "";
string sFile = Lbc.OpenFileDialog("", App.sVirtualFolder, sFilter, 0);
if (sFile.Length == 0) return;

App.sVirtualFolder = sFile;
string sPaths = Homer.Util.file2String(sFile);
string[] aPaths = sPaths.Replace("\r\n", "\n").Trim().Split('\n');

if (!bOpen) {
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
if (mdiChild != null) mdiChild.Dispose();
}

Frame frame = this;
string sDir = "";
string sOrder = "";
sFilter = "";
string sZip = sFile;
new MdiChild(frame, sDir, sOrder, sFilter, sZip, aPaths);
} // openVirtualFolder_Helper method

void MenuFileParentFolder_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Parent folder");
string sDefaultDir = Directory.GetCurrentDirectory();
string sDir = Path.GetDirectoryName(sDefaultDir);
//if (sDir.Length == 0) return;
if (Directory.Exists(sDir)) {
App.sGoToText = sDefaultDir;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild != null) mdiChild.Dispose();
MdiChild form = new MdiChild(App.frame, sDir, App.sOrderText, App.sFilterText);
}
else App.say("No parent folder!");
} // menuFileOpenParentFolder_Click method

void goTo_Helper(string sPath) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
for (int i = 0; i < view.Count; i++) {
//if ((string) view[i]["Path"] == sPath) {
if (Homer.Util.stringEquiv((string) view[i]["Path"], sPath)) {
mdiChild.bs.Position = i;
return;
}
}
} // goTo_Helper method

void parentFolder_Helper(string sLabel, bool bCloseWindow) {
App.say(sLabel);
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
int i = mdiChild.bs.Position;
string sPath = "";
if (i >= 0) {
sPath = (string) view[i]["Path"];
sPath = Path.GetDirectoryName(sPath);
}

string sDir = Directory.GetCurrentDirectory();
if (mdiChild.InZip) sPath = mdiChild.Text;
else {
sPath = sDir;
sDir = Path.GetDirectoryName(sDir);
}

if (Directory.Exists(sDir)) {
App.sGoToText = sDir;
if (mdiChild != null && bCloseWindow) {
//App.say("Closing");
mdiChild.Dispose();
}
//MdiChild form = new MdiChild(App.frame, sDir, App.sOrderText, App.sFilterText);
activate_Helper(sDir);
goTo_Helper(sPath);
}
else App.say("No parent folder!");
} // parentFolder_Helper method

void menuFileOpenParentFolder_Click(object sender, EventArgs e) {
parentFolder_Helper("Open parent folder", false);
} // menuFileOpenParentFolder_Click method

void menuFileGoToParentFolder_Click(object sender, EventArgs e) {
parentFolder_Helper("Go to parent folder", true);
} // menuFileGoToParentFolder_Click method

void menuFileComeUpLevel_Click(object sender, EventArgs e) {
parentFolder_Helper("Come up level", true);
} // menuFileComeUpLevel_Click method

void rootFolder_Helper(string sLabel, bool bCloseWindow) {
App.say(sLabel);
string sDefaultDir = Directory.GetCurrentDirectory();
string sDir = sDefaultDir.Substring(0, 3);
//string sDir = Path.GetRootName(sDefaultDir);
//if (sDir.Length == 0) return;
if (Directory.Exists(sDir)) {
App.sGoToText = sDir;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild != null && bCloseWindow) {
//App.say("Closing");
mdiChild.Dispose();
}
//MdiChild form = new MdiChild(App.frame, sDir, App.sOrderText, App.sFilterText);
activate_Helper(sDir);
}
else App.say("No root folder!");
} // rootFolder_Helper method

void menuFileOpenRootFolder_Click(object sender, EventArgs e) {
rootFolder_Helper("Open root folder", false);
} // menuFileOpenRootFolder_Click method

void menuFileGoToRootFolder_Click(object sender, EventArgs e) {
rootFolder_Helper("Go to root folder", true);
} // menuFileGoToRootFolder_Click method

public void activateLink(string sLink) {
object o = Homer.Util.createObject("WScript.Shell");
o = Homer.Util.callMethod(o, "CreateShortcut", new string[] {sLink});
string sTarget = (string) Homer.Util.getProperty(o, "TargetPath");
if (Directory.Exists(sTarget)) { 
App.say("Open");
activate_Helper(sTarget);
}
else {
try {
//Process.Start(sLink);
//Homer.Util.run(Homer.Util.stringQuote(sLink));
App.say("Run");
Homer.Util.shellDefault(sLink);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
}
}
} // activateLink method

public void activate_Helper(string sPath) {
string sDir = "";
string sZip = "";
sPath = sPath.Trim('"');
if (Directory.Exists(sPath)) sDir = sPath;
else if (File.Exists(sPath)) sZip = sPath;
else return;
activate_Helper(sDir, sZip);
} // activate_Helper method

public void activate_Helper(string sDir, string sZip) {
string[] aPaths = null;
string sExt = Path.GetExtension(sZip).ToLower();
string sTitle = "";
if (sZip == "") {
if (sDir.EndsWith(":")) sDir = App.GetDirectory(sDir);
else sDir = Path.GetFullPath(sDir);
sTitle = sDir;
}
// else if (sExt == ".zip") {
else if (testZip(sZip)) {
// else if (sExt != "txt") {
sZip = Path.GetFullPath(sZip);
sTitle = sZip;
}
else {
sZip = Path.GetFullPath(sZip);
sTitle = "Virtual " + sZip;
aPaths = Homer.Util.file2String(sZip).Replace("\r\n", "\n").Trim().Split('\n');
}

object[] mdiChildren = App.frame.MdiChildren;
foreach (MdiChild mdiChild in mdiChildren) {
//if (mdiChild.Text == sTitle) {
if (Homer.Util.stringEquiv(mdiChild.Text, sTitle)) {
App.say("returning");
mdiChild.Activate();
refreshFolder_Helper(mdiChild);
// THE VISIT THAT WAS BEING LOST. Returning to a folder that already has a
// window never reached the MdiChild constructor, so nothing was recorded --
// which is why the Quick folder, reached twice, appeared in the history once.
App.recordVisit(sZip == "" ? sDir : sZip);
return;
}
}
//MdiChild form = new MdiChild(App.frame, sDir, App.sOrderText, App.sFilterText, sZip);
string sFilter = "";
MdiChild form = new MdiChild(App.frame, sDir, App.sOrderText, sFilter, sZip, aPaths);
} // activate_Helper method

void openQuickFolder_Helper(string sLabel) {
App.say(sLabel);
bool bOpen = sLabel.StartsWith("Open") ? true : false;

string sDir = Path.Combine(App.sDataDir, "Quick");
if (!bOpen) {
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
if (mdiChild != null) mdiChild.Dispose();
}

activate_Helper(sDir);
} // openQuickFolder_Helper method

void menuFileOpenQuickFolder_Click(object sender, EventArgs e) {
openQuickFolder_Helper("Open quick folder");
} // menuFileOpenQuickFolder_Click method

void menuFileGoToQuickFolder_Click(object sender, EventArgs e) {
openQuickFolder_Helper("Go to quick folder");
} // menuFileGoToQuickFolder_Click method

string quickName_Helper(string sName, string sExtension, out string sError) {
// A name typed by a person turned into a file name Windows will accept.
//
// Neither Quick command checked this. A page called "Q: what now?" or a file
// called "report: final.docx" produced a path Windows refuses, and what the
// person got was a raw .NET exception about an illegal character. An empty name
// was worse: it made a file called ".lnk", which is invisible in the list and
// impossible to pick.
//
// The same punctuation rule as Rename to Identify Content: dashes, commas,
// periods, parentheses and apostrophes are kept because they occur in real
// titles, and every run of anything else becomes one space.
sError = "";
if (sName == null) sName = "";
StringBuilder sb = new StringBuilder(sName.Length);
bool bPendingSpace = false;
foreach (char c in sName) {
bool bKeep = Char.IsLetterOrDigit(c)
|| c == '-' || c == ',' || c == '.' || c == '(' || c == ')' || c == '\'' || c == '_';
if (bKeep) {
if (bPendingSpace && sb.Length > 0) sb.Append(' ');
bPendingSpace = false;
sb.Append(c);
}
else bPendingSpace = true;
}
// Windows keeps no name ending in a period or a space.
string sClean = sb.ToString().Trim().TrimEnd(new char[] {'.', ' '});
if (sClean.Length == 0) {
sError = "that name has nothing in it Windows can use";
return "";
}
// The old device names are still reserved, and a file called CON cannot be
// created however it is spelled.
string sStem = sClean.Split('.')[0].ToUpper();
foreach (string sDevice in new string[] {"CON", "PRN", "AUX", "NUL",
"COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
"LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"}) {
if (sStem == sDevice) {
sError = sClean + " is a name Windows reserves for a device";
return "";
}
}
// Room for the folder, the extension and a number if one is needed.
if (sClean.Length > 100) sClean = sClean.Substring(0, 100).TrimEnd(new char[] {'.', ' ', '-', ','});
return sClean + sExtension;
} // quickName_Helper method

void menuFileQuickShortcut_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Quick shortcut");
string sDir = Path.Combine(App.sDataDir, "Quick");
string sPath = Path_Helper();
string sName = Path.GetFileName(sPath);
//sName = Lbc.InputDialog("Input", "&Name", sName).Trim();
//if (sName == "") return;
string[] aFields = {"&Name", "&Path"};
string[] aValues = {sName, sPath};
ArrayList list = Lbc.FieldDialog("Fields", aFields, aValues);
if (list.Count == 0) return;

sName = ((string) list[0]).Trim();
sPath = ((string) list[1]).Trim();
if (!App.itemExists(sPath)) {
// A shortcut to something that is not there is a shortcut to nothing, and the
// mistake is far easier to see now than the next time the link is followed.
Lbc.Show(sPath + "\r\n\r\nis not a file or folder, so there is nothing to make a shortcut to.", "Quick Shortcut");
return;
}
string sError;
string sLeaf = quickName_Helper(sName, ".lnk", out sError);
if (sLeaf.Length == 0) {
Lbc.Show("The shortcut could not be named: " + sError + ".", "Quick Shortcut");
return;
}
if (!Directory.Exists(sDir)) Directory.CreateDirectory(sDir);
string sLink = Path.Combine(sDir, sLeaf);
if (File.Exists(sLink)) File.Delete(sLink);

try {
Homer.Util.path2Link(sPath, sLink);
}
catch (Exception ex) {
Lbc.Show("The shortcut could not be created.\r\n\r\n" + ex.Message, "Quick Shortcut");
return;
}
if (File.Exists(sLink)) App.say(sLeaf + " added to the Quick folder.", true);
else Lbc.Show("The shortcut could not be created.", "Quick Shortcut");
} // menuFileQuickShortcut_Click method

void menuFileQuickURL_Click(object sender, EventArgs e) {
App.say("Quick URL");
// The address and title used to come from WebGet.exe, an external helper that
// scraped Internet Explorer's address bar.  Internet Explorer is gone, so the
// clipboard is the source: copy a link in any browser, then run this command.
// Both fields stay editable, so nothing is lost when the clipboard holds no URL.
string sAddress = Homer.Util.getUrl();
string sTitle = "";
if (sAddress.Length > 0) {
try {
Uri uri = new Uri(sAddress);
sTitle = uri.Host;
if (sTitle.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) sTitle = sTitle.Substring(4);
}
catch { sTitle = ""; }
}
string sDir = Path.Combine(App.sDataDir, "Quick");
string sName = sTitle;
//sName = Lbc.InputDialog("Input", "&Name", sName).Trim();
//if (sName == "") return;
string[] aFields = {"&Name", "&Address"};
string[] aValues = {sName, sAddress};
ArrayList list = Lbc.FieldDialog("Fields", aFields, aValues);
if (list.Count == 0) return;

sName = ((string) list[0]).Trim();
sAddress = ((string) list[1]).Trim();
if (sAddress.Length == 0) {
// A .url file with no address in it opens nothing, and the file would sit in
// the Quick folder looking like a link for ever.
Lbc.Show("No address was given, so there is nothing to link to.\r\n\r\n"
+ "Copy a link in your browser and run this command again, and the address fills itself in.", "Quick URL");
return;
}
// A bare host is what a person pastes half the time, and Windows will not
// follow it without a scheme.
if (sAddress.IndexOf("://", StringComparison.Ordinal) < 0)
sAddress = "https://" + sAddress;
string sError;
string sLeaf = quickName_Helper(sName, ".url", out sError);
if (sLeaf.Length == 0) {
Lbc.Show("The link could not be named: " + sError + ".", "Quick URL");
return;
}
if (!Directory.Exists(sDir)) Directory.CreateDirectory(sDir);
string sURL = Path.Combine(sDir, sLeaf);
if (File.Exists(sURL)) File.Delete(sURL);
App.writeValue(sURL, "InternetShortcut", "URL", sAddress);
if (File.Exists(sURL)) App.say(sLeaf + " added to the Quick folder.", true);
else Lbc.Show("The link could not be created in " + sDir + ".", "Quick URL");
} // menuFileQuickURL_Click method

void menuFileFind_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("File find");
string[] aNames = null;
string[] aPaths = null;
int iIndex = -1;
string sPath = "";
string sName = "";
string sPathList = "";
string sNameList = "";

string sDir = Directory.GetCurrentDirectory();
string sText = App.sFileFindText;
string sFilter = App.sFileFindFilterText;
string sFieldList = "&Text\t&Filter";
string sValueList = sText + "\t" + sFilter;
string[] aFields = sFieldList.Split('\t');
string[] aValues = sValueList.Split('\t');
ArrayList sResultList = Lbc.FieldDialog("Fields", aFields, aValues);
if (sResultList.Count == 0) return;

sText = (string) sResultList[0];
sFilter = (string) sResultList[1];
if (sDir == App.sFileFindDir && sText == App.sFileFindText && sFilter == App.sFileFindFilterText) {
App.say("Repeat search");
aNames = App.aFileFind;
iIndex = App.iFileFind + 1;
if (iIndex == -1) iIndex = 0;
}
else {
App.say("Please wait");
ReadOnlyCollection<string> oPaths = null;
if (sText == "") oPaths = App.getFiles(sDir, sFilter);
else oPaths = App.findInFiles(sDir, sText, sFilter);
if (oPaths.Count == 0) {
App.say("No files found!");
return;
}
foreach (string s in oPaths) {
sPathList += s + "\n";
sName = s.Substring(sDir.Length + 1);
sNameList += sName + "\n";
}
aPaths = sPathList.Trim().Split('\n');
aNames = sNameList.Trim().Split('\n');
iIndex = 0;
}

App.sFileFindText = sText;
App.sFileFindFilterText = sFilter;
//Lbc.Show(App.sFileFindText, App.sFileFindFilterText);
sName = Lbc.ListDialog("Pick", "", aNames, true, iIndex);
if (sName.Length == 0) return;

int iName = Array.IndexOf(aNames, sName);
App.sFileFindDir = sDir;
App.aFileFind = aNames;
App.iFileFind = iName;
sPath = aPaths[iName];
//sPath = Path.GetFullPath(sPath);
sDir = Path.GetDirectoryName(sPath);
if (sDir.Length == 0) return;
if (Directory.Exists(sDir)) {
App.sOpenText = sDir;
//MdiChild form = new MdiChild(App.frame, sDir, App.sOrderText, App.sFilterText);
activate_Helper(sDir);
//Lbc.Show(App.iFileFind, sPath);
goTo_Helper(sPath);
}
else App.say("Folder " + sDir + " not found!");
} // menuFileFind_Click method

void menuFileProperties_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Properties");
string sPath = Path_Helper();
Homer.Util.properties(sPath);
} // menuFileProperties_Click method

void item_Helper(string sLabel, bool bCloseWindow) {
bool bZip = false;
item_Helper(sLabel, bCloseWindow, bZip);
} // item_Helper method

void item_Helper(string sLabel, bool bCloseWindow, bool bZip) {
//App.say("Run");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
// App.say(sLabel);
int i = mdiChild.bs.Position;
string sPath = (string) mdiChild.tbl.DefaultView[i]["Path"];
if (mdiChild.InZip) {
string sZip = mdiChild.Text;
string sDir = Path.GetTempFileName();
App.lsTempFile.Add(sDir);
sDir = Path.GetDirectoryName(sDir);
string sTarget = zipEntry2Dir(sZip, sPath, sDir);
// Clipboard.SetText(sTarget);
App.lsTempFile.Add(sTarget);
//Process.Start(sTarget);
App.say("Run");
App.say("With temp file");
Homer.Util.shellDefault(sTarget);
}
else if (Directory.Exists(sPath)) {
//mdiChild.Text = path;
//MdiChild.fillTable(mdiChild.bs, mdiChild.tbl, path);
if (bCloseWindow) mdiChild.Dispose();
string sDir = sPath;
App.say(sLabel);
activate_Helper(sDir);
}
else if (File.Exists(sPath)) {
// if (App.bZipOpener && Homer.Util.stringEquiv(".zip", Path.GetExtension(sPath)) && App.frame.testZip(sPath)) {
// if (!bCloseWindow && App.bZipOpener && App.frame.testZip(sPath)) {
if (bZip && App.bZipOpener && !Homer.Util.stringEquiv(sLabel, "Run")) {
// Lbc.Show("here");
// if (App.bZipOpener && !Homer.Util.stringEquiv(".txt", Path.GetExtension(sPath))){
string sDir = "";
string sZip = sPath;
App.say(sLabel);
activate_Helper(sDir, sZip);
}
else if (Homer.Util.stringEquiv(".lnk", Path.GetExtension(sPath))) {
activateLink(sPath);
}
//else Process.Start(sPath);
//else Lbc.Open(sPath);
//else Lbc.ShellExecute("Open", sPath);
else { 
App.say("Run");
Homer.Util.shellDefault(sPath);
//else Lbc.Show(Homer.Util.shellDefault(sPath));
//else Homer.Util.run("cmd.exe /c " + Homer.Util.stringQuote(sPath));
}
}
else {
string sName = Path.GetFileName(sPath);
App.say(sName + " not found!");
}
} // item_Helper method

void MenuFileOpenItem_Click(object sender, EventArgs e) {
item_Helper("Open", false);
} // menuFileOpenItem_Click method

void MenuFileGoToItem_Click(object sender, EventArgs e) {
item_Helper("Go to", true);
} // menuFileGoToItem_Click method

void menuFilePrintTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Print");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (Lbc.ConfirmDialog("Confirm", "Sure?", "Y") != "Y") return;
Process process = new Process();
process.StartInfo.Verb = "Print";
process.StartInfo.CreateNoWindow = true;

foreach (string sPath in aPaths) {
string sName = Path.GetFileName(sPath);
App.say(sName);
if (Directory.Exists(sPath)) {
App.say("Skipping folder " + sName);
continue;
}
else if (File.Exists(sPath)) {
process.StartInfo.FileName = sPath;
try {
process.Start();
//Lbc.ShellExecute("Print", sPath);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
}
else App.say(sName + " not found!");
}
App.say("Done!", true);
} // menuFilePrintTagged_Click method

void MenuFileGoBack_Click(object sender, EventArgs e) {
// The folder visited before this one.
//
// Alt+LeftArrow and Alt+RightArrow used to be second keys for Previous and Next
// Window, which is not what those keys mean anywhere else and not what anyone
// reaches for them expecting. Window cycling keeps Control+Tab and
// Control+Shift+Tab, which is where it belongs.
goHistory_Helper(-1, "back");
} // MenuFileGoBack_Click method

void MenuFileGoForward_Click(object sender, EventArgs e) {
goHistory_Helper(1, "forward");
} // MenuFileGoForward_Click method

void goHistory_Helper(int iStep, string sWay) {
// Move one place along the history and go there.
//
// The arrival is marked as a replay, so recordVisit leaves the history alone
// and the pointer is the only thing that moves. Without that, going back would
// append the place it went to, and there would never be anywhere forward.
int iWant = App.iHistoryAt + iStep;
if (App.lsRecentDirs.Count == 0 || iWant < 0 || iWant >= App.lsRecentDirs.Count) {
App.say("No folder " + sWay + ".", true);
return;
}

// A folder deleted or a drive unplugged since the visit is stepped over rather
// than reported as a dead end, so one stale entry does not block the way back.
while (iWant >= 0 && iWant < App.lsRecentDirs.Count && !App.itemExists(App.lsRecentDirs[iWant]))
iWant += iStep;
if (iWant < 0 || iWant >= App.lsRecentDirs.Count) {
App.say("No folder " + sWay + " that still exists.", true);
return;
}

string sPath = App.lsRecentDirs[iWant];
App.iHistoryAt = iWant;
App.say(Path.GetFileName(sPath.TrimEnd(Path.DirectorySeparatorChar)));
App.bReplaying = true;
try {
activate_Helper(sPath);
}
finally {
// Cleared whatever happened, or every later visit would go unrecorded.
App.bReplaying = false;
}
} // goHistory_Helper method

void menuFileRecentFolders_Click(object sender, EventArgs e) {
string sButton = Dialog.Choose("Choose", "", new string[] {"&Folders", "&Shortcuts"}, 0);
if (sButton.Length == 0) return;

sButton = sButton.Replace("&", "");
string[] aNames, aDirs;
string sName, sResult = "";

if (sButton == "Folders") {
// App.say("Recent folders");
aDirs = App.recentDistinct();
Array.Reverse(aDirs);
aNames = new string[aDirs.Length];
for (int i = 0; i < aNames.Length; i++) aNames[i] = (aDirs[i].EndsWith(@":\") ? aDirs[i] : Path.GetFileName(aDirs[i]));
//Array.Sort(aDirs, aNames);
sName = Lbc.ListDialog("Pick", "", aNames, true, 0);
if (sName.Length == 0) return;

int iName = Array.IndexOf(aNames, sName);
sResult = aDirs[iName];
activate_Helper(sResult);
return;
} // if

// App.say("Recent shortcuts");
string sDir = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
DirectoryInfo di = new DirectoryInfo(sDir);
//FileSystemInfo[] aPathInfos = di.GetFileSystemInfos();
FileSystemInfo[] aPathInfos = di.GetFiles();
CompareFileSystemInfo compare = new CompareFileSystemInfo();
Array.Sort(aPathInfos, compare);
Array.Reverse(aPathInfos);
string sPath = "";
sName = "";
string sPathList = "\n";
string sNameList = "\n";
//StringBuilder buildNameList = new StringBuilder();
//StringBuilder buildPathList = new StringBuilder();

string s = @" \(\d+\)\.lnk$";
Regex rx = new Regex(s, RegexOptions.Compiled | RegexOptions.IgnoreCase);
int j = 1;
foreach (FileSystemInfo fs in aPathInfos) {
sName = rx.Replace(fs.Name, ".lnk");
if (sNameList.Contains("\n" + sName + "\n")) continue;
sNameList += Path.GetFileNameWithoutExtension(sName) + "\n";
sPathList += fs.FullName + "\n";
// if (j == 100) break;
j++;
}
string[]aPaths = sPathList.Trim().Split('\n');
aNames = sNameList.Trim().Split('\n');
sName = Lbc.ListDialog("Pick", "", aNames, false, 0);
if (sName == "") return;

int iIndex = Array.IndexOf(aNames, sName);
sPath = aPaths[iIndex];
activateLink(sPath);
} // menuFileRecentFolders_Click method

void menuFileWindowToggle_Click(object sender, EventArgs e) {
App.say("Window Toggle");
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
string sDir = "";
string sPrevDir = "";
if (mdiChild != null) sDir = mdiChild.Text;
bool bFound = false;
for (int i = App.lsRecentDirs.Count - 1; i >=0; i--) {
sPrevDir = App.lsRecentDirs[i];
if (!Homer.Util.stringEquiv(sDir, sPrevDir)) {
bFound = true;
break;
}
}
if (!bFound) {
App.say("No previous window!");
return;
}

activate_Helper(sPrevDir);
} // menuFileWindowToggle_Click method

void menuFileCurrentWindows_Click(object sender, EventArgs e) {
App.say("Current windows");
object[] mdiChildren = App.frame.MdiChildren;
string sTitleList = "";
foreach (MdiChild mdiChild in mdiChildren) {
sTitleList += mdiChild.Text + "\n";
}
string[] aTitles = sTitleList.Trim().Split('\n');
string[] aNames = new string[aTitles.Length];
for (int i = 0; i < aNames.Length; i++) aNames[i] = (aTitles[i].EndsWith(@":\") ? aTitles[i] : Path.GetFileName(aTitles[i]));
Array.Sort(aTitles, aNames);
string sName = Lbc.ListDialog("Pick", "", aNames, true, 0);
//string sName = Lbc.Pick("Pick", aNames, true, 0);
if (sName == "") return;
int iName = Array.IndexOf(aNames, sName);
string sTitle = aTitles[iName];
activate_Helper(sTitle);
} // menuFileCurrentWindows_Click method

void MenuFileNextWindow_Click(object sender, EventArgs e) {
object[] mdiChildren = App.frame.MdiChildren;
if (mdiChildren.Length == 0) App.say("No windows!");
else if (mdiChildren.Length == 1) App.say("Only this window!");
else {
App.say("Next window");
MdiChild mdiChild = getActiveChild();
int iPosition = Array.IndexOf(mdiChildren, mdiChild);
iPosition++;
if (iPosition == mdiChildren.Length) iPosition = 0;
((MdiChild) mdiChildren[iPosition]).Activate();
}
} // menuNextWindowOpen_Click method

void MenuFilePreviousWindow_Click(object sender, EventArgs e) {
object[] mdiChildren = App.frame.MdiChildren;
if (mdiChildren.Length == 0) App.say("No windows!");
else if (mdiChildren.Length == 1) App.say("Only this window!");
else {
App.say("Previous window");
MdiChild mdiChild = getActiveChild();
int iPosition = Array.IndexOf(mdiChildren, mdiChild);
iPosition--;
if (iPosition == -1) iPosition = mdiChildren.Length - 1;
((MdiChild) mdiChildren[iPosition]).Activate();
}
} // menuPreviousWindowOpen_Click method

void MenuFileClose_Click(object sender, EventArgs e) {
App.say("Close");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.Close();
} // menuFileClose method

void menuFileCloseAllButCurrent_Click(object sender, EventArgs e) {
App.say("Close all but current");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

object[] mdiChildren = App.frame.MdiChildren;
int iCount = 0;
foreach (MdiChild o in mdiChildren) {
if (o != mdiChild) {
o.Close();
iCount++;
}
}
//App.say(iCount.ToString() + " closed");
//App.say(iCount);
;App.say(mdiChild.Text);
} // menuFileCloseAllButCurrent_Click method

void MenuFileExit_Click(object sender, EventArgs e) {
App.say("Exit FileDir");
this.Close();
} // MenuFileExit method

void MenuFileRestartWindows_Click(object sender, EventArgs e) {
if (Lbc.ConfirmDialog("Confirm", "Restart Windows?", "N") != "Y") return;
Homer.Util.run("shutdown.exe -r -f -t 1");
} // MenuFileRestartWindows_Clickmethod

void MenuEditTagAndNext_Click(object sender, EventArgs e) {
App.say("Tag and next");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
mdiChild.tbl.DefaultView[i]["Tagged"] = '>';
mdiChild.bs.Position++;
mdiChild.bs.EndEdit();
} // menuEditTagAndNext method

void MenuEditUntagAndNext_Click(object sender, EventArgs e) {
App.say("Untag and next");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
mdiChild.tbl.DefaultView[i]["Tagged"] = ' ';
mdiChild.bs.Position++;
mdiChild.bs.EndEdit();
} // menuEditUntagAndNext method

void MenuEditTagAndPrevious_Click(object sender, EventArgs e) {
App.say("Tag and previous");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
mdiChild.tbl.DefaultView[i]["Tagged"] = '>';
mdiChild.bs.Position--;
mdiChild.bs.EndEdit();
} // menuEditTagAndPrevious method

void MenuEditUntagAndPrevious_Click(object sender, EventArgs e) {
App.say("Untag and previous");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
mdiChild.tbl.DefaultView[i]["Tagged"] = ' ';
mdiChild.bs.Position--;
mdiChild.bs.EndEdit();
} // menuEditUntagAndPrevious method

void MenuEditTagToBottom_Click(object sender, EventArgs e) {
App.say("Tag to bottom");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iStart = mdiChild.bs.Position;
int iEnd = mdiChild.bs.Count - 1;
for (int i = iStart; i <= iEnd; i++) mdiChild.tbl.DefaultView[i]["Tagged"] = '>';
mdiChild.bs.EndEdit();
mdiChild.bs.Position = iEnd;
} // menuEditTagToBottom method

void MenuEditUntagToBottom_Click(object sender, EventArgs e) {
App.say("Untag to bottom");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iStart = mdiChild.bs.Position;
int iEnd = mdiChild.bs.Count - 1;
for (int i = iStart; i <= iEnd; i++) mdiChild.tbl.DefaultView[i]["Tagged"] = ' ';
mdiChild.bs.EndEdit();
mdiChild.bs.Position = iEnd;
} // menuEditUntagToBottom method

void MenuEditTagToTop_Click(object sender, EventArgs e) {
App.say("Tag to top");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iStart = mdiChild.bs.Position;
int iEnd = 0;
for (int i = iStart; i >= iEnd; i--) mdiChild.tbl.DefaultView[i]["Tagged"] = '>';
mdiChild.bs.EndEdit();
mdiChild.bs.Position = iEnd;
} // menuEditTagToTop method

void MenuEditUntagToTop_Click(object sender, EventArgs e) {
App.say("Untag to top");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iStart = mdiChild.bs.Position;
int iEnd = 0;
for (int i = iStart; i >= iEnd; i--) mdiChild.tbl.DefaultView[i]["Tagged"] = ' ';
mdiChild.bs.EndEdit();
mdiChild.bs.Position = iEnd;
} // menuEditUntagToTop method

void MenuEditTag_Click(object sender, EventArgs e) {
App.say("Tag");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
mdiChild.tbl.DefaultView[i]["Tagged"] = '>';
mdiChild.bs.EndEdit();
} // menuEditTag method

void MenuEditUntag_Click(object sender, EventArgs e) {
App.say("Untag");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
mdiChild.tbl.DefaultView[i]["Tagged"] = ' ';
mdiChild.bs.EndEdit();
} // menuEditUntag method

void MenuEditToggleTag_Click(object sender, EventArgs e) {
//App.say("Toggle tag");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
if ((char) mdiChild.tbl.DefaultView[i]["Tagged"] == '>') mdiChild.tbl.DefaultView[i]["Tagged"] = ' ';
else mdiChild.tbl.DefaultView[i]["Tagged"] = '>';
mdiChild.bs.EndEdit();
} // menuEditToggleTag method

void MenuEditTagAll_Click(object sender, EventArgs e) {
App.say("Tag all");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
//Lbc.Show(mdiChild.tbl.DefaultView.Count);
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
row["Tagged"] = '>';
}
mdiChild.bs.EndEdit();
} // menuEditTagAll method

void MenuEditUntagAll_Click(object sender, EventArgs e) {
App.say("Untag all");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
row["Tagged"] = ' ';
mdiChild.bs.EndEdit();
}
} // menuEditUntagAll method

void MenuEditTagAllFiles_Click(object sender, EventArgs e) {
App.say("Tag all files");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
if (!Directory.Exists((string) row["Path"])) row["Tagged"] = '>';
}
mdiChild.bs.EndEdit();
} // menuEditTagAllFiles method

void MenuEditFindDuplicates_Click(object sender, EventArgs e) {
// Every duplicate in this folder AND everything under it, gathered into a
// virtual folder you can walk with the ordinary commands.
//
// WHY A VIRTUAL FOLDER RATHER THAN A DIALOG.
//
// KeyLine's delDupes puts its findings in a multiselect listbox with an All
// button. FileDir already has something better: a virtual folder is a window
// like any other, so every command works in it. You can hear a name, its size
// and its date, read what is inside a file with Question Mark, open one to
// check before deciding, tag a run with F8, invert the tagging, and then use
// Delete Tagged -- which asks before it deletes. Nothing new to learn, and
// nothing about it is a special case.
//
// ONLY THE DUPLICATES ARE LISTED, never the first copy of anything. So Tag All
// and Delete Tagged in this window leaves exactly one of each file on disk,
// which is what a person means by removing duplicates.
if (abortInZip()) return;
string sTitle = "Find Duplicates in Tree";
App.say(sTitle);
string sRoot = Directory.GetCurrentDirectory();
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild != null) {
string sHere = Path_Helper();
if (Directory.Exists(sHere)) sRoot = sHere;
else if (File.Exists(sHere)) sRoot = Path.GetDirectoryName(sHere);
}

App.say("Reading " + Path.GetFileName(sRoot) + " and everything under it");
List<string> lsAll = new List<string>();
gatherFiles_Helper(sRoot, lsAll);
if (lsAll.Count == 0) {
Lbc.Show("No files were found under " + sRoot + ".", sTitle);
return;
}
App.say(Homer.Util.stringPlural("file", lsAll.Count) + " to examine");

// Size first: two files of different lengths cannot be the same, and grouping
// by size means most files are never read at all. On a large tree that is the
// difference between seconds and minutes.
Dictionary<long, List<string>> dBySize = new Dictionary<long, List<string>>();
foreach (string sPath in lsAll) {
long lSize = 0;
try { lSize = new FileInfo(sPath).Length; }
catch (Exception) { continue; }
// Empty files are all identical to each other, and calling them duplicates
// is technically true and never what anybody wants.
if (lSize == 0) continue;
if (!dBySize.ContainsKey(lSize)) dBySize[lSize] = new List<string>();
dBySize[lSize].Add(sPath);
}

List<string> lsDuplicates = new List<string>();
int iRead = 0;
int iGroups = 0;
foreach (KeyValuePair<long, List<string>> pair in dBySize) {
if (pair.Value.Count < 2) continue;
Dictionary<string, string> dByHash = new Dictionary<string, string>();
foreach (string sPath in pair.Value) {
iRead++;
// A count every so often, because a large tree takes a while and silence
// is indistinguishable from a hang.
if ((iRead % 50) == 0) App.say(iRead, true);
string sHash = fileHash_Helper(sPath);
if (sHash.Length == 0) continue;
if (!dByHash.ContainsKey(sHash)) {
dByHash[sHash] = sPath;
continue;
}
if (!sameBytes_Helper(dByHash[sHash], sPath)) continue;
lsDuplicates.Add(sPath);
}
iGroups++;
}

if (lsDuplicates.Count == 0) {
App.say("No duplicates.", true);
Lbc.Show("No duplicate files were found under " + sRoot + ".\n\n"
+ Homer.Util.stringPlural("file", lsAll.Count) + " examined, of which "
+ iRead + " had to be read in full because another file was exactly the same size.", sTitle);
return;
}

// Written where Open Virtual Folder will offer it again, so the same set can
// be reopened later without scanning the tree a second time.
lsDuplicates.Sort(delegate(string sLeft, string sRight) {
return String.Compare(sLeft, sRight, StringComparison.OrdinalIgnoreCase);
});
string sListFile = Path.Combine(Path.GetTempPath(), "FileDir_duplicates.txt");
try {
File.WriteAllText(sListFile, String.Join("\r\n", lsDuplicates.ToArray()), new UTF8Encoding(true));
App.sVirtualFolder = sListFile;
}
catch (Exception ex) {
Homer.Log.write("Could not write the duplicate list: " + ex.Message);
}
Homer.Log.write("Duplicates under " + sRoot + ": " + lsDuplicates.Count + " of " + lsAll.Count + " files.");

App.say(Homer.Util.stringPlural("duplicate", lsDuplicates.Count) + " found. Opening them as a virtual folder.", true);
string sDir = "";
string sOrder = "";
string sFilter = "";
new MdiChild(this, sDir, sOrder, sFilter, sListFile, lsDuplicates.ToArray());
} // MenuEditFindDuplicates_Click method

void gatherFiles_Helper(string sFolder, List<string> lsInto) {
// Every file under a folder, walked by hand rather than with the framework's
// recursive search.
//
// Directory.GetFiles with AllDirectories throws the moment it meets one folder
// it cannot open -- a system folder, a junction, someone else's profile -- and
// loses the entire result. Walking it a folder at a time means an unreadable
// one is skipped and noted, and everything else is still found.
try {
foreach (string sFile in Directory.GetFiles(sFolder)) lsInto.Add(sFile);
}
catch (Exception ex) {
Homer.Log.write("Could not read files in " + sFolder + ": " + ex.Message);
return;
}
string[] aSubFolders;
try {
aSubFolders = Directory.GetDirectories(sFolder);
}
catch (Exception ex) {
Homer.Log.write("Could not list folders in " + sFolder + ": " + ex.Message);
return;
}
foreach (string sSub in aSubFolders) {
// A junction or a symbolic link can point at a parent, and following one
// walks in a circle until the stack gives out.
try {
FileAttributes attributes = File.GetAttributes(sSub);
if ((attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) {
Homer.Log.write("Skipping the link " + sSub);
continue;
}
}
catch (Exception) {
continue;
}
gatherFiles_Helper(sSub, lsInto);
}
} // gatherFiles_Helper method

void MenuEditTagSimilarFiles_Click(object sender, EventArgs e) {
// Tag files that look like other versions of the same thing, keeping the
// largest of each group and tagging the rest.
//
// content.pdf, content-1.pdf, content_2.pdf, content(3).pdf and content[4].pdf
// are one group: the same name with a numeric suffix that a browser or a
// download added.  content.epub is NOT in it, because a different extension is
// a different file rather than another copy.
//
// The largest is kept, on KeyLine's reasoning: a partial download is smaller
// than the whole, so the biggest is the likeliest to be complete.  Nothing is
// deleted here; Delete Tagged does that, and asks first.
if (abortInZip()) return;
App.say("Tag similar files");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
int iCount = view.Count;

// Grouped by stem and extension together.  The stem is the name with a
// trailing "-1", "_2", " (3)" or "[4]" taken off.
Dictionary<string, List<int>> dGroups = new Dictionary<string, List<int>>();
for (int i = 0; i < iCount; i++) {
string sPath = (string) view[i]["Path"];
if (Directory.Exists(sPath) || !File.Exists(sPath)) continue;
string sRoot = Path.GetFileNameWithoutExtension(sPath);
string sExt = Path.GetExtension(sPath).ToLower();
// The separator is REQUIRED, and this is the whole difficulty. "content-1"
// and "content (3)" are copies of content; "chapter1" and "part2" are not --
// they are the first chapter and the second part, and grouping them would tag
// one of a book's chapters for deletion. KeyLine's delSimilar makes the same
// distinction, and its comment says so in as many words.
string sStem = System.Text.RegularExpressions.Regex.Replace(sRoot,
@"(?:[\s_-]+\d+|\s*[\(\[]\d+[\)\]])$", "").Trim();
// A name that is nothing but digits has no stem to group by, and grouping
// 1.jpg with 2.jpg would be wrong: those are different pictures.
if (sStem.Length == 0) continue;
string sKey = sStem.ToLower() + "|" + sExt;
if (!dGroups.ContainsKey(sKey)) dGroups[sKey] = new List<int>();
dGroups[sKey].Add(i);
}

int iSimilar = 0;
foreach (KeyValuePair<string, List<int>> pair in dGroups) {
if (pair.Value.Count < 2) continue;
// Which to keep: the largest, and the earliest in the list to break a tie so
// the same file is kept every time the command is run.
int iKeep = pair.Value[0];
long lBiggest = (long) view[iKeep]["Size"];
foreach (int i in pair.Value) {
long lSize = (long) view[i]["Size"];
if (lSize > lBiggest) { lBiggest = lSize; iKeep = i; }
}
foreach (int i in pair.Value) {
if (i == iKeep) continue;
App.say(Path.GetFileName((string) view[i]["Path"]));
view[i]["Tagged"] = '>';
iSimilar++;
}
}
mdiChild.bs.EndEdit();
if (iSimilar == 0) App.say("No similar files.", true);
else App.say(Homer.Util.stringPlural("similar file", iSimilar) + " tagged, keeping the largest of each. Nothing was deleted.", true);
} // MenuEditTagSimilarFiles_Click method

void MenuEditReorderNames_Click(object sender, EventArgs e) {
// Rename the tagged files, or all of them, so an alphabetical list reads in
// the order a person means.
//
// Three rules, from KeyLine's reorder:
//   A single leading digit is padded with a zero, so 2name comes before
//     11name instead of after it.
//   Front matter -- ReadMe, index, introduction, overview -- is prefixed so it
//     sorts to the top.
//   Back matter -- licence, contributing, change log, credits and the rest --
//     is prefixed with z- so it sorts to the bottom.
//
// Every rename is shown before anything happens, and nothing is overwritten:
// a name already taken gets another.
if (abortInZip()) return;
string sTitle = "Reorder Names";
App.say(sTitle);
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

List<string[]> lsPlan = new List<string[]>();
foreach (string sPath in aPaths) {
string sRoot = Path.GetFileNameWithoutExtension(sPath);
string sExt = Path.GetExtension(sPath);
string sNew = reorderRoot_Helper(sRoot);
if (sNew == sRoot) continue;
lsPlan.Add(new string[] {sPath, Path.Combine(Path.GetDirectoryName(sPath), sNew + sExt)});
}
if (lsPlan.Count == 0) {
App.say("No names need reordering.", true);
return;
}

// Shown before it happens, the way Rename Tagged does, so a surprise costs
// nothing.
StringBuilder sbPlan = new StringBuilder();
foreach (string[] aPair in lsPlan)
sbPlan.Append(Path.GetFileName(aPair[0]) + "  becomes  " + Path.GetFileName(aPair[1]) + "\r\n");
string[] aButtons = {"&Rename", "Cancel"};
if (Dialog.Choose(sTitle, Homer.Util.stringPlural("name", lsPlan.Count) + " will change:\r\n\r\n" + sbPlan.ToString(), aButtons, 0) != "&Rename") return;

int iDone = 0;
int iFailed = 0;
foreach (string[] aPair in lsPlan) {
string sTarget = Homer.Web.uniquePath(aPair[1]);
try {
if (Directory.Exists(aPair[0])) Directory.Move(aPair[0], sTarget);
else File.Move(aPair[0], sTarget);
iDone++;
}
catch (Exception ex) {
App.say(Path.GetFileName(aPair[0]) + ": " + ex.Message);
iFailed++;
}
}
string sMessage = Homer.Util.stringPlural("name", iDone) + " changed";
if (iFailed > 0) sMessage += ", " + iFailed + " failed";
App.say(sMessage + ".", true);
refreshFolder_Helper(mdiChild);
} // MenuEditReorderNames_Click method

string reorderRoot_Helper(string sRoot) {
// The three rules, tried in order, first match wins.
System.Text.RegularExpressions.RegexOptions oIgnore = System.Text.RegularExpressions.RegexOptions.IgnoreCase;
// A single leading digit, padded. Two digits already sort correctly.
if (System.Text.RegularExpressions.Regex.IsMatch(sRoot, @"^\d([^\d]|$)"))
return "0" + sRoot;
// Front matter to the top. ReadMe first of all, then the other openers.
if (System.Text.RegularExpressions.Regex.IsMatch(sRoot, @"^read\s*me", oIgnore))
return "0#" + sRoot;
if (System.Text.RegularExpressions.Regex.IsMatch(sRoot, @"^(index|introduction|intro|overview|about|start|getting.?started)", oIgnore))
return "0-" + sRoot;
// Back matter to the bottom.
if (System.Text.RegularExpressions.Regex.IsMatch(sRoot,
@"^(?!\d)(.*contributors|.*credits|.*bug.?report|bugs|.*code.?of.?conduct|.*contribut(e|ing)|copying|changes|.*change.?log|history|.*licen[cs]e|.*issue.?template|.*pull.?request|appendix.*|glossary|index.?of.*)$", oIgnore))
return "z-" + sRoot;
return sRoot;
} // reorderRoot_Helper method

void MenuEditTagDuplicateFiles_Click(object sender, EventArgs e) {
// Tag every file whose content is identical to one already seen, keeping the
// first and tagging the rest.  Delete Tagged then removes them, after the
// confirmation that command already asks for.
//
// TWO FAULTS FIXED HERE, BOTH SERIOUS.
//
// It used to call File.Delete on every duplicate it found.  A command called
// TAG Duplicate Files deleted files outright, with no confirmation and no way
// back, and a comment beside it said it had been done for one particular job
// and never taken out again.
//
// And it compared files by reading them as TEXT.  Two different pictures whose
// bytes happen to decode to the same string counted as identical, and one was
// deleted.  Files are the same when their BYTES are the same, which is what
// KeyLine's delDupes has always said.
if (abortInZip()) return;
App.say("Tag duplicate files");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
int iCount = view.Count;

// Grouped by size first, because two files of different lengths cannot be the
// same and reading them would be wasted work.  Only a group with more than one
// member is hashed at all.
Dictionary<long, List<int>> dBySize = new Dictionary<long, List<int>>();
for (int i = 0; i < iCount; i++) {
string sPath = (string) view[i]["Path"];
if (Directory.Exists(sPath) || !File.Exists(sPath)) continue;
long lSize = (long) view[i]["Size"];
if (!dBySize.ContainsKey(lSize)) dBySize[lSize] = new List<int>();
dBySize[lSize].Add(i);
}

int iDuplicate = 0;
int iChecked = 0;
foreach (KeyValuePair<long, List<int>> pair in dBySize) {
if (pair.Value.Count < 2) continue;
Dictionary<string, int> dByHash = new Dictionary<string, int>();
foreach (int i in pair.Value) {
string sPath = (string) view[i]["Path"];
iChecked++;
if ((iChecked % 25) == 0) App.say(iChecked, true);
string sHash = fileHash_Helper(sPath);
if (sHash.Length == 0) continue;
if (!dByHash.ContainsKey(sHash)) {
dByHash[sHash] = i;
continue;
}
// A hash match is near certain, but "near" is not good enough before
// somebody deletes something.  The bytes are compared as well.
if (!sameBytes_Helper((string) view[dByHash[sHash]]["Path"], sPath)) continue;
App.say(Path.GetFileName(sPath));
view[i]["Tagged"] = '>';
iDuplicate++;
}
}
mdiChild.bs.EndEdit();
App.say(Homer.Util.stringPlural("duplicate", iDuplicate) + " tagged. Nothing was deleted.", true);
} // menuEditTagDuplicateFiles method

string fileHash_Helper(string sPath) {
// SHA-256 of a file, read in a stream so a large file is not held in memory.
try {
using (SHA256 hasher = SHA256.Create())
using (FileStream stream = File.OpenRead(sPath))
return BitConverter.ToString(hasher.ComputeHash(stream));
}
catch (Exception ex) {
Homer.Log.write("Could not hash " + sPath + ": " + ex.Message);
return "";
}
} // fileHash_Helper method

bool sameBytes_Helper(string sLeft, string sRight) {
// Byte for byte, in blocks.  The last word before anything is tagged for
// deletion.
try {
using (FileStream streamLeft = File.OpenRead(sLeft))
using (FileStream streamRight = File.OpenRead(sRight)) {
if (streamLeft.Length != streamRight.Length) return false;
byte[] aLeft = new byte[65536];
byte[] aRight = new byte[65536];
while (true) {
int iLeft = streamLeft.Read(aLeft, 0, aLeft.Length);
int iRight = streamRight.Read(aRight, 0, aRight.Length);
if (iLeft != iRight) return false;
if (iLeft == 0) return true;
for (int i = 0; i < iLeft; i++) if (aLeft[i] != aRight[i]) return false;
}
}
}
catch (Exception ex) {
Homer.Log.write("Could not compare " + sLeft + " and " + sRight + ": " + ex.Message);
return false;
}
} // sameBytes_Helper method

void MenuEditTagWithRegularExpression_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

App.say("Tag with regular expression");
string sText = Lbc.InputDialog("Input", "Text", mdiChild.sTagWithRegExpText, "TagRegExp");
if (sText.Length == 0) return;

mdiChild.sTagWithRegExpText = sText;
Regex rx;
try {
rx = new Regex(sText, RegexOptions.Multiline);
}
catch (Exception ex) {
Lbc.Show("Error", ex.Message);
return;
}

DataView view = mdiChild.tbl.DefaultView;
int iCount = view.Count;
int iMatch = 0;
for (int i = 0; i < iCount; i++) {
string sFile = (string) view[i]["Path"];
if (Directory.Exists(sFile) || !File.Exists(sFile)) continue;
// string sBody = Homer.Util.file2String(sFile);
string sBody = App.frame.convert2Text(sFile);
if (rx.IsMatch(sBody)) {
App.say(Path.GetFileName(sFile));
view[i]["Tagged"] = '>';
iMatch ++;
}
}
mdiChild.bs.EndEdit();
App.say(iMatch.ToString() + (iMatch == 1 ? " match" : " matches"));
} // menuEditTagWithRegularExpression method

void MenuEditUntagAllButCurrent_Click(object sender, EventArgs e) {
App.say("Untag all but current");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
for (int i = 0; i < view.Count; i++) {
if (i == mdiChild.bs.Position) view[i]["Tagged"] = '>';
else view[i]["Tagged"] = ' ';
mdiChild.bs.EndEdit();
}
} // menuEditUnTagAllButCurrent method

void MenuEditStartTagOrUntag_Click(object sender, EventArgs e) {
App.say("Start tag or untag");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
mdiChild.StartTagOrUntag = mdiChild.bs.Position;
} // menuEditStartTagOrUntag method

void MenuEditCompleteTag_Click(object sender, EventArgs e) {
completeTag_Helper("Complete tag", true);
} // menuEditCompleteTag method

void MenuEditCompleteUntag_Click(object sender, EventArgs e) {
completeTag_Helper("Complete untag", false);
} // menuEditCompleteUntag method

void completeTag_Helper(string sLabel, bool bTag) {
App.say(sLabel);
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
int iStart = mdiChild.StartTagOrUntag;
if (iStart == -1) {
App.say("No start position is set!");
return;
}

int iEnd = mdiChild.bs.Position;
if (iEnd < iStart) {
iStart = mdiChild.bs.Position;
iEnd = mdiChild.StartTagOrUntag;
}

for (int i = iStart; i <= iEnd; i++) {
if (bTag) view[i]["Tagged"] = '>';
else view[i]["Tagged"] = ' ';
mdiChild.bs.EndEdit();
}
} // completeTag_Helper method

void MenuEditInvertTagged_Click(object sender, EventArgs e) {
App.say("Invert tagged");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
foreach (DataRow row in mdiChild.tbl.Rows) {
if ((char) row["Tagged"] == '>') row["Tagged"] = ' ';
else row["Tagged"] = '>';
mdiChild.bs.EndEdit();
}
} // menuEditInvertTagged method

void MenuEditSaveTags_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
App.say("Save");
App.aTags = getTagged();
int iCount = App.aTags.Length;
App.say(Homer.Util.stringPlural("tag", iCount));
} // saveTags_Click method

void MenuEditRestoreTags_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
App.say("Restore");
int iCount = applyTagged(App.aTags);
App.say(Homer.Util.stringPlural("tag", iCount));
} // restoreTags_Click method

void menuEditCopyToClipboardTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Copy");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
bool bCut = false;
Homer.Util.path2Clipboard(aPaths, bCut);
App.say("Done!", true);
} // menuEditCopyToClipboardTagged_Click method

void menuEditCopyAppendToClipboardTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Copy append");

string[] aDirs, aFiles;
bool bCut = false;
string[] aPaths = Homer.Util.clipboard2Path(out bCut);
string sPaths = String.Join("\n", aPaths).Trim() + "\n";
StringBuilder sb = new StringBuilder(sPaths);
aPaths = list_Helper(out aDirs, out aFiles, 1);
foreach (string sPath in aPaths) sb.Append(sPath + "\n");

sPaths = sb.ToString().Trim();
aPaths = sPaths.Split('\n');
if (aPaths.Length == 1 && aPaths[0].Length == 0) aPaths = new String[] {};

Homer.Util.path2Clipboard(aPaths, bCut);
App.say("Done!", true);
} // menuEditCopyAppendToClipboardTagged_Click method

void menuEditCutToClipboardTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Cut");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
bool bCut = true;
Homer.Util.path2Clipboard(aPaths, bCut);
App.say("Done!", true);
} // menuEditCutToClipboardTagged_Click method

void menuEditCopyPathTagged_Click(object sender, EventArgs e) {
//if (abortInZip()) return;
App.say("Copy path");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
string sPathList = String.Join("\r\n", aPaths);
if (sPathList.Length > 0) sPathList += "\r\n";
Clipboard.SetText(sPathList);
App.say("Done!", true);
} // menuEditCopyPathTagged_Click method

void menuEditCopyNameTagged_Click(object sender, EventArgs e) {
App.say("Copy name");
string[] aDirs, aFiles;
string[] aNames = list_Helper(out aDirs, out aFiles, 1);
string sNameList = "";
foreach (string sName in aNames) sNameList += Path.GetFileName(sName) + "\r\n";
Clipboard.SetText(sNameList);
App.say("Done!", true);
} // menuEditCopyNameTagged_Click method

void menuEditPathList_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

string sDir = Path_Helper();
if (!Directory.Exists(sDir)) {
App.say("Command requires a folder item!");
return;
}

App.say("Path List to clipboard");
ReadOnlyCollection<string> pathList = App.getFiles(sDir, mdiChild.sFilterText);
string[] aFiles = new string[pathList.Count];
for (int i = 0; i < pathList.Count; i++) aFiles[i] = pathList[i];
string sExts = Homer.Util.getExtensions(aFiles);
string sResult = Lbc.InputDialog("Input", "Extensions", sExts, "PathListExtensions").Trim();
if (sResult.Length == 0) return;

string[] aResults = Homer.Util.getFilesWithExtensions(aFiles, sResult);
if (aResults.Length == 0) {
App.say("No matching files!");
return;
}

string sText = String.Join("\r\n", aResults) + "\r\n";
Clipboard.SetText(sText);
App.say(Homer.Util.stringPlural("file", aResults.Length));

/*
StringBuilder sb = new StringBuilder();
foreach (string s in pathList) sb.Append(s + "\r\n");
Clipboard.SetText(sb.ToString());
App.say(Homer.Util.stringPlural("file", pathList.Count));
*/

//int iDirCount = 0;
//int iFileCount = 0;
//foreach (object o in pathList) {
//string s = o.ToString();
//if (Directory.Exists(s)) iDirCount++;
//else if (File.Exists(s)) iFileCount++;
//sb.Append(s + "\r\n");
//}
//App.say("Done!", true);
//App.say(Homer.Util.stringPlural("folder", iDirCount));
//App.say(Homer.Util.stringPlural("file", iFileCount));
} // menuEditPathList_Click method

public string compare_Helper(DateTime dSourceDate, long lSourceSize, DateTime dTargetDate, long lTargetSize) {
string sResult = "";
if (dSourceDate > dTargetDate) sResult += "Older";
else if (dSourceDate < dTargetDate) sResult += "Newer";
else sResult += "Current";
if (lSourceSize > lTargetSize) sResult += ", smaller ";
else if (lSourceSize < lTargetSize) sResult += ", larger ";
else sResult += ", equal ";
return sResult;
} // compare_Helper method

void menuEditCopyTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
int iDirCount = 0;
int iFileCount = 0;
App.say("Copy");
DateTime dSourceDate = DateTime.Now;
DateTime dTargetDate = DateTime.Now;
long lSourceSize = -1;
long lTargetSize = -1;
string sChoice = "";
string sCompare = "";
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.sCopyText == null) mdiChild.sCopyText = App.sCopyText;
string sDir = Lbc.DirectoryDialog("Input", "Folder", mdiChild.sCopyText).Trim();
if (sDir.Length == 0) return;
mdiChild.sCopyText = sDir;
App.sCopyText = sDir;
App.sGoToText = sDir;
App.sOpenText = sDir;

for (int i = 0; i < aPaths.Length; i++) {
string sSource = aPaths[i];
string sName = Path.GetFileName(sSource);
App.say(sName);
if (Directory.Exists(sSource)) {
DirectoryInfo diSource = new DirectoryInfo(sSource);
lSourceSize = App.DirSize(diSource);
dSourceDate = File.GetLastWriteTime(sSource);
}
else if (File.Exists(sSource)) {
FileInfo fiSource = new FileInfo(sSource);
lSourceSize = fiSource.Length;
dSourceDate = File.GetLastWriteTime(sSource);
}
else {
App.say(sName + " not found!");
continue;
}

string sTarget = Path.Combine(sDir, sName);
lTargetSize = -1;
if (Directory.Exists(sTarget)) {
DirectoryInfo diTarget = new DirectoryInfo(sTarget);
lTargetSize = App.DirSize(diTarget);
dTargetDate = File.GetLastWriteTime(sTarget);
}
else if (File.Exists(sTarget)) {
FileInfo fiTarget = new FileInfo(sTarget);
lTargetSize = fiTarget.Length;
dTargetDate = File.GetLastWriteTime(sTarget);
}

if (lTargetSize >=0) {
sCompare = compare_Helper(dSourceDate, lSourceSize, dTargetDate, lTargetSize);
if (!sChoice.Contains("All")) sChoice = Lbc.ButtonDialog("Confirm", sCompare + "\n" + sName + "\nalready exists.  Sure?", new string[] {"&No", "&Yes", "&Keep All", "&Replace All", "&Update All", "&Increment All"}, 0);
switch (sChoice) {
case "Cancel" :
return;
case "&No" :
case "&Keep All" :
App.say("Skipping");
continue;
}
if (sChoice == "&Update All" && !sCompare.Contains("Older")) {
App.say("Skipping");
continue;
}
else if (sChoice == "&Increment All") sTarget = Homer.Util.getUniqueName(sTarget);
}

if (Directory.Exists(sSource)) {
try {
App.copyDirectory(sSource, sTarget, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
iDirCount++;
}
else if (File.Exists(sSource) ){
try {
App.copyFile(sSource, sTarget, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
iFileCount++;
}
}
App.say("Done!", true);
//if (iDirCount > 0) App.say(Homer.Util.stringPlural("folder", iDirCount));
//if (iFileCount > 0) App.say(Homer.Util.stringPlural("file", iFileCount));
//if (iDirCount ==0 && iFileCount == 0) App.say("No items!");
} // menuEditCopyTagged_Click method

void menuEditMoveTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Move");
int iDirCount = 0;
int iFileCount = 0;
DateTime dSourceDate = DateTime.Now;
DateTime dTargetDate = DateTime.Now;
long lSourceSize = -1;
long lTargetSize = -1;
string sChoice = "";
string sCompare = "";
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.sMoveText == null) mdiChild.sMoveText = App.sMoveText;
string sDir = Lbc.DirectoryDialog("Input", "Folder", mdiChild.sMoveText).Trim();
if (sDir.Length == 0) return;
mdiChild.sMoveText = sDir;
App.sMoveText = sDir;
App.sGoToText = sDir;
App.sOpenText = sDir;

for (int i = 0; i < aPaths.Length; i++) {
string sSource = aPaths[i];
string sName = Path.GetFileName(sSource);
App.say(sName);
if (Directory.Exists(sSource)) {
DirectoryInfo diSource = new DirectoryInfo(sSource);
lSourceSize = App.DirSize(diSource);
dSourceDate = File.GetLastWriteTime(sSource);
}
else if (File.Exists(sSource)) {
FileInfo fiSource = new FileInfo(sSource);
lSourceSize = fiSource.Length;
dSourceDate = File.GetLastWriteTime(sSource);
}
else {
App.say(sName + " not found!");
continue;
}

string sTarget = Path.Combine(sDir, sName);
lTargetSize = -1;
if (Directory.Exists(sTarget)) {
DirectoryInfo diTarget = new DirectoryInfo(sTarget);
lTargetSize = App.DirSize(diTarget);
dTargetDate = File.GetLastWriteTime(sTarget);
}
else if (File.Exists(sTarget)) {
FileInfo fiTarget = new FileInfo(sTarget);
lTargetSize = fiTarget.Length;
dTargetDate = File.GetLastWriteTime(sTarget);
}

if (lTargetSize >=0) {
sCompare = compare_Helper(dSourceDate, lSourceSize, dTargetDate, lTargetSize);
if (!sChoice.Contains("All")) sChoice = Lbc.ButtonDialog("Confirm", sCompare + "\n" + sName + "\nalready exists.  Sure?", new string[] {"&No", "&Yes", "&Keep All", "&Replace All", "&Update All", "&Increment All"}, 0);
switch (sChoice) {
case "Cancel" :
return;
case "&No" :
case "&Keep All" :
App.say("Skipping");
continue;
}
if (sChoice == "&Update All" && !sCompare.Contains("Older")) {
App.say("Skipping");
continue;
}
else if (sChoice == "&Increment All") sTarget = Homer.Util.getUniqueName(sTarget);
}

if (Directory.Exists(sSource)) {
try {
App.moveDirectory(sSource, sTarget, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
iDirCount++;
delete_Helper(sSource);
}
else if (File.Exists(sSource) ){
try {
App.moveFile(sSource, sTarget, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
iFileCount++;
delete_Helper(sSource);
}
}
App.say("Done!", true);
//if (iDirCount > 0) App.say(Homer.Util.stringPlural("folder", iDirCount));
//if (iFileCount > 0) App.say(Homer.Util.stringPlural("file", iFileCount));
//if (iDirCount ==0 && iFileCount == 0) App.say("No items!");
} // menuEditMoveTagged_Click method

void delete_Helper(string sPath) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
//foreach (DataRowView row in view) {
foreach (DataRow row in mdiChild.tbl.Rows) {
//if ((string) row["Path"] == sPath) {
if (Homer.Util.stringEquiv((string) row["Path"], sPath)) {
row.Delete();
mdiChild.bs.EndEdit();
return;
}
}
} // delete_Helper method

void MenuEditDeleteFileNow_Click(object sender, EventArgs e) {
deleteFileNow_Helper(true);
} // deleteFileNow_Click method

void MenuEditDeleteFileNowWithoutRecycle_Click(object sender, EventArgs e) {
deleteFileNow_Helper(false);
} // deleteFileNowWithoutRecycle_Click method

public void deleteFileNow_Helper(bool bRecycle) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

string sPath = Path_Helper();
if (Directory.Exists(sPath)) {
App.say("Command unavailable on folder item!");
return;
}
else if (File.Exists(sPath)) {
if (bRecycle) App.say("Delete recycle now");
else App.say("Delete now");
App.deleteFile(sPath, bRecycle);
App.say("Done!", true);
delete_Helper(sPath);
}
else {
string sName = Path.GetFileName(sPath);
App.say(sName + " not found!");
return;
}
} // deleteFileNow_Helper method

public void Delete_Recycle(string sLabel) {
App.say(sLabel);
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
//aPaths = aFiles;
if (aPaths.Length == 0) return;
string[] aNames = new string[aPaths.Length];
for (int i = 0; i < aNames.Length; i++) aNames[i] = (aPaths[i].EndsWith(@":\") ? aPaths[i] : Path.GetFileName(aPaths[i]));

string sNames = String.Join("\n", aNames);
//if (Lbc.ConfirmDialog("Confirm", "Sure?", "N") != "Y") return;
if (Lbc.ConfirmDialog("Confirm", "Sure?\n" + sNames, "N") != "Y") return;

MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
ZipFile z = null;
if (mdiChild.InZip) {
z = new ZipFile(mdiChild.Text);
if (App.sUnarchivePassword.Trim().Length > 0) {
App.say("With password");
z.Password = App.sUnarchivePassword;
}
z.BeginUpdate();
}
 
for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
//sName = Path.GetFileName(sName);
string sName = getName(sPath);
App.say(sName);
if (mdiChild.InZip) {
try {
z.Delete(sPath);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
z.Close();
return;
}
delete_Helper(sPath);
}
else if (Directory.Exists(sPath)) {
DirectoryInfo di = new DirectoryInfo(sPath);
try {
App.deleteDirectory(sPath, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
delete_Helper(sPath);
}
else if (File.Exists(sPath) ){
try {
App.deleteFile(sPath, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
delete_Helper(sPath);
}
else App.say(sName + " not found!");
}
if (mdiChild.InZip) {
z.CommitUpdate();
z.Close();
}
App.say("Done!", true);
} // menuEditDelete_Recycle method

void menuEditDeleteTaggedWithoutRecycle_Click(object sender, EventArgs e) {
bool bOldRecycle = App.Recycle;
App.Recycle = false;
//Delete_Recycle("Delete without recycle");
Delete_Recycle("Delete");
App.Recycle = bOldRecycle;
} // menuEditDeleteTaggedWithoutRecycle_Click method

void menuEditDeleteTagged_Click(object sender, EventArgs e) {
//Delete_Recycle("Delete");
string sLabel = "Delete";
if (App.Recycle) sLabel = "Delete recycle";
Delete_Recycle(sLabel);
} // menuEditDeleteTagged_Click method

void menuEditRename_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Rename");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
string sSource = (string) mdiChild.tbl.DefaultView[i]["Path"];
string sName = Path.GetFileName(sSource);
string sDir = Path.GetDirectoryName(sSource);
string sNewName = Lbc.InputDialog("Input", "Name", sName, "Rename").Trim();
if (sNewName.Length == 0) return;
string sTarget = Path.Combine(sDir, sNewName);
try {
if (Directory.Exists(sSource)) {
FileSystem.RenameDirectory(sSource, sNewName);
}
else if (File.Exists(sSource)) {
FileSystem.RenameFile(sSource, sNewName);
}
else {
App.say(sName + " not found!");
return;
}
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}

mdiChild.tbl.DefaultView[i]["Path"] = sTarget;
mdiChild.tbl.DefaultView[i]["Name"] = sNewName;
mdiChild.tbl.DefaultView[i]["Ext"] = Path.GetExtension(sNewName);
mdiChild.bs.EndEdit();
App.say("Done!", true);
} // menuEditRename_Click method

void menuEditRenameWithWildcards_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Rename with wildcards");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (Lbc.ConfirmDialog("Confirm", "This may affect all items in the current directory, not just tagged ones.  Sure?", "Y") != "Y") return;
string[] aFields = {"&Source", "&Target"};
string[] aValues = {"*.*", "*.*"};
ArrayList sResultList = Lbc.FieldDialog("Fields", aFields, aValues);
if (sResultList.Count == 0) return;
//string sCommand = "ren " + sResultList[0] + " " + sResultList[1];
string sCommand = "cmd.exe /c ren " + sResultList[0] + " " + sResultList[1];
try {
Homer.Util.runHideWait(sCommand);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
App.say("Done!", true);
refreshFolder_Helper(mdiChild);
} // menuEditRenameWithWildcards_Click method

void menuEditRenameWithRegex_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Rename with regular expression");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);

string[] aFields = {"&Source", "&Target"};
string[] aValues = {"", ""};
ArrayList sResultList = Lbc.FieldDialog("Fields", aFields, aValues);
if (sResultList.Count == 0) return;
string sSource = (string) sResultList[0];
string sTarget = (string) sResultList[1];
Regex rx = new Regex(sSource, RegexOptions.Compiled | RegexOptions.IgnoreCase);

foreach (string sPath in aPaths) {
string sName = Path.GetFileName(sPath);
string sDir = Path.GetDirectoryName(sPath);
App.say(sName);
string sNewName = rx.Replace(sName, sTarget);
if (sNewName == sName) continue;
try {
if (Directory.Exists(sPath)) FileSystem.RenameDirectory(sPath, sNewName);
else if (File.Exists(sPath)) FileSystem.RenameFile(sPath, sNewName);
else App.say(sName + " not found!");
 }
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
delete_Helper(sPath);
refresh_Helper(Path.Combine(sDir, sNewName));
}
App.say("Done!", true);
//refreshFolder_Helper(mdiChild);
} // menuEditRenameWithRegex_Click method

void MenuEditRenameToTitle_Click(object sender, EventArgs e) {
// Rename the tagged files to what identifies their content: the title, caption
// or name recorded INSIDE the file.
//
// This is renTitle, the command-line utility, brought inside FileDir. That one
// asked ExifTool for one named tag and let ExifTool do the renaming, which
// meant it worked when the tag happened to be called Title and did nothing
// otherwise. This asks for EVERY field, keeps the ones whose name is about a
// title, and takes the longest -- because a photograph carrying both IMG_4021
// and "Sunset over the Cascades" should be named the second.
//
// Every rename is shown before anything happens, and nothing is overwritten.
if (abortInZip()) return;
string sTitle = "Rename to Identify Content";
App.say(sTitle);
if (Homer.Media.exifToolProgram().Length == 0) {
// Not fatal: the first line of the text still gives a name. Said once, so
// nobody wonders why a photograph kept its camera name.
App.say("ExifTool was not found, so only the text of each file can be used.");
Homer.Log.write("Rename to Title without ExifTool. " + Homer.Media.exifToolLog());
}
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

List<string[]> lsPlan = new List<string[]>();
List<string> lsSkipped = new List<string>();
for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
string sName = Path.GetFileName(sPath);
if (Directory.Exists(sPath)) continue;
if (!File.Exists(sPath)) {
lsSkipped.Add(sName + ": not found");
continue;
}
App.say(sName + ", " + (i + 1) + " of " + aPaths.Length);
string sField, sError;
string sFound = Homer.Media.bestTitle(sPath, out sField, out sError);
if (sFound.Length == 0 && Homer.Convert.readableAsText(sPath)) {
// A LAST RESORT, and only for a file FileDir can read as text.
//
// The metadata is the better answer and is always tried first. A first line is
// often not a title at all: a date, a header, a byline, "Chapter One", or the
// opening words of a sentence that runs on. For a binary file it is not even
// text, which is why this is not attempted unless the format is one the
// extraction chain understands.
//
// So this catches the case metadata cannot: a note or a Markdown page that
// carries its title on the first line and nowhere else.
sFound = firstLineTitle_Helper(sPath);
if (sFound.Length > 0) sField = "first line of the text";
}
if (sFound.Length == 0) {
lsSkipped.Add(sName + ": " + (sError.Length > 0 ? sError : "nothing inside it identifies its content"));
continue;
}
string sRoot = titleToRoot_Helper(sFound);
if (sRoot.Length == 0) {
lsSkipped.Add(sName + ": the title had nothing usable in it");
continue;
}
string sExt = Path.GetExtension(sPath);
if (Homer.Util.stringEquiv(sRoot, Path.GetFileNameWithoutExtension(sPath))) continue;
lsPlan.Add(new string[] {sPath, Path.Combine(Path.GetDirectoryName(sPath), sRoot + sExt), sField});
}

if (lsPlan.Count == 0) {
// One file, one sentence, spoken. Several, a window to read: the reasons
// differ per file and a single spoken line could not carry them.
foreach (string sLine in lsSkipped) Homer.Log.write("Not renamed, " + sLine);
if (lsSkipped.Count == 1) {
App.say("Not renamed. " + lsSkipped[0], true);
return;
}
StringBuilder sbNone = new StringBuilder("No file could be renamed.\r\n\r\n");
foreach (string sLine in lsSkipped) sbNone.Append("  " + sLine + "\r\n");
Lbc.AnswerDialog(sTitle, "&Why not", sbNone.ToString());
return;
}

// NO CONFIRMATION. Renaming is not deleting: the file is still there, still
// tagged, and a name that turns out wrong can be changed again. A dialog before
// every rename made the quick thing slow, and the plan it showed was a wall of
// text to read before doing what had already been asked for.
//
// Each rename is spoken as it happens, so the reader hears what became what,
// and every detail goes to the session log for afterwards.
int iDone = 0;
int iFailed = 0;
string sLast = "";
foreach (string[] aPair in lsPlan) {
// A number is added only when the name is taken, and it is added to the ROOT
// so the extension still says what the file is: "Sunset-01.jpg", never
// "Sunset.jpg-01".
string sTarget = uniqueTitleName_Helper(aPair[1]);
try {
File.Move(aPair[0], sTarget);
Homer.Log.write("Renamed " + aPair[0] + " to " + Path.GetFileName(sTarget)
+ " from the " + aPair[2] + " field.");
App.say(Path.GetFileName(sTarget));
sLast = sTarget;
iDone++;
}
catch (Exception ex) {
App.say(Path.GetFileName(aPair[0]) + ": " + ex.Message);
Homer.Log.write("Could not rename " + aPair[0] + ": " + ex.Message);
iFailed++;
}
}
foreach (string sLine in lsSkipped) Homer.Log.write("Not renamed, " + sLine);
string sMessage = Homer.Util.stringPlural("file", iDone) + " renamed";
if (iFailed > 0) sMessage += ", " + iFailed + " failed";
if (lsSkipped.Count > 0) sMessage += ", " + lsSkipped.Count + " skipped";
App.say(sMessage + ".", true);
refreshFolder_Helper(mdiChild);
// Land on the renamed file rather than wherever the list happened to move to.
// goTo_Helper finds it only if it survived the filter in effect; when a filter
// hides it, the cursor stays where the refresh left it, which is the honest
// outcome rather than a jump to nothing.
if (sLast.Length > 0) goTo_Helper(sLast);
} // MenuEditRenameToTitle_Click method

string firstLineTitle_Helper(string sPath) {
// The first line of a file's text that has anything in it.
//
// The text comes from the one extraction chain, so this works on a Word
// document or a PDF as readily as on a text file -- which is more than the old
// Rename to Initial Line managed, since it read only what 2htm knew.
//
// A first line that is merely long is prose rather than a title, so the same
// bounds apply as to a metadata field: too short is not a name, too long is a
// paragraph.
string sError;
string sBody = Homer.Convert.toPlainText(sPath, out sError);
if (sBody.Length == 0) return "";
foreach (string sLine in sBody.Replace("\r\n", "\n").Split('\n')) {
string sTrimmed = sLine.Trim();
// Markdown heading marks and the like are decoration, not part of the title.
sTrimmed = sTrimmed.TrimStart(new char[] {'#', '*', '=', '-', ' ', '\t'}).Trim();
if (sTrimmed.Length < 4) continue;
if (sTrimmed.Length > 120) return sTrimmed.Substring(0, 120);
return sTrimmed;
}
return "";
} // firstLineTitle_Helper method

string titleToRoot_Helper(string sTitle) {
// A title turned into a file name, keeping the punctuation that carries
// meaning and dropping the rest.
//
// KEPT: dashes, commas, periods, parentheses and apostrophes, because they
// occur naturally in a sentence or a descriptive phrase and a name reads
// wrongly without them. Capitalization is left exactly as written -- somebody
// chose it, and lowercasing a title loses proper nouns.
//
// DROPPED: everything Windows forbids, and everything else that is punctuation
// rather than language. Each run of dropped characters becomes ONE space, and
// underscores go the same way, so "Sunset_over__the:Cascades" reads as
// "Sunset over the Cascades" rather than running together.
if (sTitle == null) return "";
StringBuilder sb = new StringBuilder(sTitle.Length);
bool bPendingSpace = false;
foreach (char c in sTitle) {
bool bKeep = Char.IsLetterOrDigit(c)
|| c == '-' || c == ',' || c == '.' || c == '(' || c == ')' || c == '\'';
if (bKeep) {
if (bPendingSpace && sb.Length > 0) sb.Append(' ');
bPendingSpace = false;
sb.Append(c);
}
else {
// Whitespace, underscores and every dropped character alike: one space.
bPendingSpace = true;
}
}
string sRoot = sb.ToString().Trim();
// Windows will not keep a name ending in a period or a space, and a leading
// dash makes a name that command-line tools mistake for an option.
sRoot = sRoot.Trim(new char[] {' ', '.', '-'});
// Room for the folder, a separating backslash, the extension, and a -99
// sequence if one is needed. Cut at a word rather than mid-word.
if (sRoot.Length > 120) {
sRoot = sRoot.Substring(0, 120);
int iSpace = sRoot.LastIndexOf(' ');
if (iSpace > 40) sRoot = sRoot.Substring(0, iSpace);
sRoot = sRoot.Trim(new char[] {' ', '.', '-', ','});
}
return sRoot;
} // titleToRoot_Helper method

string uniqueTitleName_Helper(string sTarget) {
// The name if it is free, otherwise the same name with -01, -02 and so on
// until one is. Two digits because a folder with a hundred files sharing one
// title is a different problem.
if (!File.Exists(sTarget) && !Directory.Exists(sTarget)) return sTarget;
string sDir = Path.GetDirectoryName(sTarget);
string sRoot = Path.GetFileNameWithoutExtension(sTarget);
string sExt = Path.GetExtension(sTarget);
for (int i = 1; i < 1000; i++) {
string sTry = Path.Combine(sDir, sRoot + "-" + i.ToString("00") + sExt);
if (!File.Exists(sTry) && !Directory.Exists(sTry)) return sTry;
}
return Homer.Web.uniquePath(sTarget);
} // uniqueTitleName_Helper method

void MenuMiscExtractTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

App.say("Extract with regular expression");
string sText = Lbc.InputDialog("Input", "Text", mdiChild.sTagWithRegExpText, "TagRegExp");
if (sText.Length == 0) return;

mdiChild.sTagWithRegExpText = sText;
Regex rx;
try {
rx = new Regex(sText, RegexOptions.Multiline | RegexOptions.IgnoreCase);
}
catch (Exception ex) {
Lbc.Show("Error", ex.Message);
return;
}

// DataView view = mdiChild.tbl.DefaultView;
// int iCount = view.Count;
int iMatch = 0;
List<string> l = new List<string>();
string[] aFiles, aDirs;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
int iCount = aFiles.Length;
for (int i = 0; i < iCount; i++) {
// string sFile = (string) view[i]["Path"];
string sFile = aFiles[i];
if (Directory.Exists(sFile) || !File.Exists(sFile)) continue;
// string sBody = Homer.Util.file2String(sFile);
string sBody = App.frame.convert2Text(sFile);
if (rx.IsMatch(sBody)) {
App.say(Path.GetFileName(sFile));
MatchCollection matches = rx.Matches(sBody);
int iMatchCount = matches.Count;
if (iMatchCount == 1) App.say("1 match");
else App.say(iMatchCount + " matches");
iMatch += iMatchCount;

for (int j = 0; j < iMatchCount; j++) l.Add(matches[j].Value);
}
}
sText = String.Join(Homer.Util.sSectionBreak, l.ToArray());
sText += Homer.Util.sEndOfDocument;
Clipboard.SetText(sText);
// App.say("Total of " + iMatch.ToString() + (iMatch == 1 ? " match" : " matches"));
Lbc.Show(iMatch.ToString() + (iMatch == 1 ? " match" : " matches") + " copied to the clipboard", "Result");
} // menuMiscExtractTagged method

void MenuEditDeleteAndRecycleTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
bool bOldRecycle = App.Recycle;
App.Recycle = true;
Delete_Recycle("Delete recycle");
App.Recycle = bOldRecycle;
} // menuEditDeleteAndRecycleTagged method

void pasteCopy_Helper(string[] aPaths) {
if (abortInZip()) return;
App.say("Paste copy");
int iDirCount = 0;
int iFileCount = 0;
DateTime dSourceDate = DateTime.Now;
DateTime dTargetDate = DateTime.Now;
long lSourceSize = -1;
long lTargetSize = -1;
string sChoice = "";
string sCompare = "";

if (aPaths.Length == 0) {
App.say("No items!");
return;
}

MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.sCopyText == null) mdiChild.sCopyText = App.sCopyText;
string sDir = Directory.GetCurrentDirectory();
if (sDir.Length == 0) return;
mdiChild.sCopyText = sDir;
App.sCopyText = sDir;
App.sGoToText = sDir;
App.sOpenText = sDir;

for (int i = 0; i < aPaths.Length; i++) {
string sSource = aPaths[i];
string sName = Path.GetFileName(sSource);
App.say(sName);
if (Directory.Exists(sSource)) {
DirectoryInfo diSource = new DirectoryInfo(sSource);
lSourceSize = App.DirSize(diSource);
dSourceDate = File.GetLastWriteTime(sSource);
}
else if (File.Exists(sSource)) {
FileInfo fiSource = new FileInfo(sSource);
lSourceSize = fiSource.Length;
dSourceDate = File.GetLastWriteTime(sSource);
}
else {
App.say(sName + " not found!");
continue;
}

string sTarget = Path.Combine(sDir, sName);
lTargetSize = -1;
if (Directory.Exists(sTarget)) {
DirectoryInfo diTarget = new DirectoryInfo(sTarget);
lTargetSize = App.DirSize(diTarget);
dTargetDate = File.GetLastWriteTime(sTarget);
}
else if (File.Exists(sTarget)) {
FileInfo fiTarget = new FileInfo(sTarget);
lTargetSize = fiTarget.Length;
dTargetDate = File.GetLastWriteTime(sTarget);
}

if (lTargetSize >=0) {
sCompare = compare_Helper(dSourceDate, lSourceSize, dTargetDate, lTargetSize);
if (!sChoice.Contains("All")) sChoice = Lbc.ButtonDialog("Confirm", sCompare + "\n" + sName + "\nalready exists.  Sure?", new string[] {"&No", "&Yes", "&Keep All", "&Replace All", "&Update All", "&Increment All"}, 0);
switch (sChoice) {
case "Cancel" :
return;
case "&No" :
case "&Keep All" :
App.say("Skipping");
continue;
}
if (sChoice == "&Update All" && !sCompare.Contains("Older")) {
App.say("Skipping");
continue;
}
else if (sChoice == "&Increment All") sTarget = Homer.Util.getUniqueName(sTarget);
}

if (Directory.Exists(sSource)) {
try {
App.copyDirectory(sSource, sTarget, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
iDirCount++;
refresh_Helper(sTarget);
}
else if (File.Exists(sSource) ){
try {
App.copyFile(sSource, sTarget, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
iFileCount++;
refresh_Helper(sTarget);
}
}
App.say("Done!", true);
//if (iDirCount > 0) App.say(Homer.Util.stringPlural("folder", iDirCount));
//if (iFileCount > 0) App.say(Homer.Util.stringPlural("file", iFileCount));
//if (iDirCount ==0 && iFileCount == 0) App.say("No items!");
} // pasteCopy_Helper method

void menuEditPasteFromClipboard_Click(object sender, EventArgs e) {
bool bCut = false;
string[] aPaths = Homer.Util.clipboard2Path(out bCut);
if (aPaths.Length == 0) aPaths = Clipboard.GetText().Replace("\r\n", "\n").Trim().Split('\n');
if (aPaths.Length == 1 && aPaths[0].Length == 0) aPaths = new string[] {};

if (bCut) pasteMove_Helper(aPaths);
else pasteCopy_Helper(aPaths);
} // menuEditPasteFromClipboard_Click method

void menuEditPasteCopy_Click(object sender, EventArgs e) {
/*
string sPathList = Clipboard.GetText();
sPathList = sPathList.Replace("\r\n", "\n").Trim();
string[] aPaths = sPathList.Split('\n');
*/

bool bCut = false;
string[] aPaths = Homer.Util.clipboard2Path(out bCut);
if (aPaths.Length == 0) aPaths = Clipboard.GetText().Replace("\r\n", "\n").Trim().Split('\n');
if (aPaths.Length == 1 && aPaths[0].Length == 0) aPaths = new string[] {};

pasteCopy_Helper(aPaths);
} // menuEditPasteCopy_Click method

void pasteMove_Helper(string[] aPaths) {
if (abortInZip()) return;
App.say("Paste move");
int iDirCount = 0;
int iFileCount = 0;
DateTime dSourceDate = DateTime.Now;
DateTime dTargetDate = DateTime.Now;
long lSourceSize = -1;
long lTargetSize = -1;
string sChoice = "";
string sCompare = "";

if (aPaths.Length == 0) {
App.say("No items!");
return;
}

MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.sMoveText == null) mdiChild.sMoveText = App.sMoveText;
string sDir = Directory.GetCurrentDirectory();
if (sDir.Length == 0) return;
mdiChild.sMoveText = sDir;
App.sMoveText = sDir;
App.sGoToText = sDir;
App.sOpenText = sDir;

for (int i = 0; i < aPaths.Length; i++) {
string sSource = aPaths[i];
string sName = Path.GetFileName(sSource);
App.say(sName);
if (Directory.Exists(sSource)) {
DirectoryInfo diSource = new DirectoryInfo(sSource);
lSourceSize = App.DirSize(diSource);
dSourceDate = File.GetLastWriteTime(sSource);
}
else if (File.Exists(sSource)) {
FileInfo fiSource = new FileInfo(sSource);
lSourceSize = fiSource.Length;
dSourceDate = File.GetLastWriteTime(sSource);
}
else {
App.say(sName + " not found!");
continue;
}

string sTarget = Path.Combine(sDir, sName);
lTargetSize = -1;
if (Directory.Exists(sTarget)) {
DirectoryInfo diTarget = new DirectoryInfo(sTarget);
lTargetSize = App.DirSize(diTarget);
dTargetDate = File.GetLastWriteTime(sTarget);
}
else if (File.Exists(sTarget)) {
FileInfo fiTarget = new FileInfo(sTarget);
lTargetSize = fiTarget.Length;
dTargetDate = File.GetLastWriteTime(sTarget);
}

if (lTargetSize >=0) {
sCompare = compare_Helper(dSourceDate, lSourceSize, dTargetDate, lTargetSize);
if (!sChoice.Contains("All")) sChoice = Lbc.ButtonDialog("Confirm", sCompare + "\n" + sName + "\nalready exists.  Sure?", new string[] {"&No", "&Yes", "&Keep All", "&Replace All", "&Update All", "&Increment All"}, 0);
switch (sChoice) {
case "Cancel" :
return;
case "&No" :
case "&Keep All" :
App.say("Skipping");
continue;
}
if (sChoice == "&Update All" && !sCompare.Contains("Older")) {
App.say("Skipping");
continue;
}
else if (sChoice == "&Increment All") sTarget = Homer.Util.getUniqueName(sTarget);
}

if (Directory.Exists(sSource)) {
try {
App.moveDirectory(sSource, sTarget, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
iDirCount++;
refresh_Helper(sTarget);
}
else if (File.Exists(sSource) ){
try {
App.moveFile(sSource, sTarget, App.Recycle);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
iFileCount++;
refresh_Helper(sTarget);
}
}
Clipboard.Clear();
App.say("Done!", true);
//if (iDirCount > 0) App.say(Homer.Util.stringPlural("folder", iDirCount));
//if (iFileCount > 0) App.say(Homer.Util.stringPlural("file", iFileCount));
//if (iDirCount ==0 && iFileCount == 0) App.say("No items!");
} // pasteMove_Helper method

void menuEditPasteMove_Click(object sender, EventArgs e) {
/*
string sPathList = Clipboard.GetText();
sPathList = sPathList.Replace("\r\n", "\n").Trim();
string[] aPaths = sPathList.Split('\n');
*/

bool bCut = false;
string[] aPaths = Homer.Util.clipboard2Path(out bCut);
if (aPaths.Length == 0) aPaths = Clipboard.GetText().Replace("\r\n", "\n").Trim().Split('\n');
if (aPaths.Length == 1 && aPaths[0].Length == 0) aPaths = new string[] {};

pasteMove_Helper(aPaths);
} // menuEditPasteMove_Click method

public void addTableRow(DataTable tbl, string path) {
char type, hidden, readOnly, system, tagged;
DateTime time;
FileAttributes attr;
long size;
string name, ext;

if (Directory.Exists(path)) {
DirectoryInfo fi = new DirectoryInfo(path);
type = '\\';
size = -1;
}
else {
FileInfo fi = new FileInfo(path);
type = ' ';
size = fi.Length;
}

name = Path.GetFileName(path);
ext = Path.GetExtension(path);
time = File.GetLastWriteTime(path);
attr = File.GetAttributes(path);
hidden = ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) ? ')' : ' ';
readOnly = ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? ']' : ' ';
system = ((attr & FileAttributes.System) == FileAttributes.System) ? '}' : ' ';
tagged = ' ';
tbl.Rows.Add(path, name.PadRight(50), ext, size, time, attr, type, hidden, readOnly, system, tagged);
} // addTableRow method

void refresh_Helper(string sPath) {
bool bFound = false;
char hidden, readOnly, system;
DateTime time;
FileAttributes attr;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;

//foreach (DataRowView row in view) {
foreach (DataRow row in mdiChild.tbl.Rows) {
string file = (string) row["Path"];
//if ((string) row["Path"] == sPath) {
if (Homer.Util.stringEquiv((string) row["Path"], sPath)) {
bFound = true;
time = File.GetLastWriteTime(file);
attr = File.GetAttributes(file);
hidden = ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) ? ')' : ' ';
readOnly = ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? ']' : ' ';
system = ((attr & FileAttributes.System) == FileAttributes.System) ? '}' : ' ';
row["Time"] = time;
row["Attr"] = attr;
row["Hidden"] = hidden;
row["ReadOnly"] = readOnly;
row["System"] = system;
break;
}
}

if (!bFound) {
addTableRow(mdiChild.tbl, sPath);
}
mdiChild.bs.EndEdit();
} // refresh_Helper method

void menuEditStampTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Stamp Date and Time");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;

DateTime dDate = (DateTime) Field_Helper("Time");
string sYear = dDate.Year.ToString();
string sMonth = dDate.Month.ToString();
string sDay = dDate.Day.ToString();
string sHour = dDate.Hour.ToString();
string sMinute = dDate.Minute.ToString();
string sSecond = dDate.Second.ToString();
string[] aFields = {"Year", "Month", "Day", "Hour", "Minute", "Second"};
string[] aValues = {sYear, sMonth, sDay, sHour, sMinute, sSecond};
ArrayList list = Lbc.FieldDialog("Fields", aFields, aValues);
if (list.Count == 0) return;

try {
int iYear = Int32.Parse((string) list[0]);
int iMonth = Int32.Parse((string) list[1]);
int iDay = Int32.Parse((string) list[2]);
int iHour = Int32.Parse((string) list[3]);
int iMinute = Int32.Parse((string) list[4]);
int iSecond = Int32.Parse((string) list[5]);
dDate = new DateTime(iYear, iMonth, iDay, iHour, iMinute, iSecond);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}

for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
string sName = Path.GetFileName(sPath);
App.say(sName);
if (Directory.Exists(sPath)) {
try {
File.SetLastWriteTime(sPath, dDate);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
refresh_Helper(sPath);
}
else if (File.Exists(sPath) ){
try {
File.SetLastWriteTime(sPath, dDate);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
refresh_Helper(sPath);
}
else App.say(sName + " not found!");
}
App.say("Done!", true);
} // menuEditStampTagged_Click method

public void attribute_Helper(FileAttributes flag, bool on) {
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;

for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
string sName = Path.GetFileName(sPath);
FileAttributes attr = File.GetAttributes(sPath);
App.say(sName);
if (Directory.Exists(sPath)) {
try {
if (on) File.SetAttributes(sPath, attr | flag);
else File.SetAttributes(sPath, (attr | flag) ^ flag);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
refresh_Helper(sPath);
}
else if (File.Exists(sPath) ){
try {
if (on) File.SetAttributes(sPath, attr | flag);
else File.SetAttributes(sPath, (attr | flag) ^ flag);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
refresh_Helper(sPath);
}
else App.say(sName + " not found!");
}
App.say("Done!", true);
} // attribute_Helper method

void menuEditHideTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Hide");
attribute_Helper(FileAttributes.Hidden, true);
} // menuEditHideTagged_Click method

void menuEditShowTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Show");
attribute_Helper(FileAttributes.Hidden, false);
} // menuEditShowTagged_Click method

void menuEditReadOnlyTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("ReadOnly");
attribute_Helper(FileAttributes.ReadOnly, true);
} // menuEditReadOnlyTagged_Click method

void menuEditReadWriteTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("ReadWrite");
attribute_Helper(FileAttributes.ReadOnly, false);
} // menuEditReadWriteTagged_Click method

void menuEditSystemTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("System");
attribute_Helper(FileAttributes.System, true);
} // menuEditSystemTagged_Click method

void menuEditGeneralTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("General");
attribute_Helper(FileAttributes.System, false);
} // menuEditGeneralTagged_Click method

void menuEditPathToClipboard_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Path to Clipboard");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
string path = (string) mdiChild.tbl.DefaultView[i]["Path"];
Clipboard.SetText(path);
path = Clipboard.GetText();
App.say(path);
} // menuEditPathToClipboard_Click method

void menuEditShortPathToClipboard_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Short path to Clipboard");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
string Path = (string) mdiChild.tbl.DefaultView[i]["Path"];
Path = Homer.Util.getShortPath(Path);
Clipboard.SetText(Path);
Path = Clipboard.GetText();
App.say(Path);
} // menuEditShortPathToClipboard_Click method

void menuEditFullFolderToClipboard_Click(object sender, EventArgs e) {
App.say("Folder to Clipboard");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
string sDir = mdiChild.Text;
Clipboard.SetText(sDir);
sDir = Clipboard.GetText();
App.say(sDir);
} // menuEditFullFolderToClipboard_Click method

void menuEditClearClipboard_Click(object sender, EventArgs e) {
App.say("Clear clipboard");
//Clipboard.SetText("\n");
Clipboard.Clear();
} // menuEditClearClipboard_Click method

void menuEditExportClipboardToFile_Click(object sender, EventArgs e) {
App.say("Export clipboard");
string sFile = "Clipboard.txt";
string sDir = Directory.GetCurrentDirectory();
sFile = Path.Combine(sDir, sFile);
sFile = Homer.Util.getUniqueName(sFile);
string sFilter = "Text files (*.txt)|*.txt";
sFile = Lbc.SaveFileDialog("", sFile, sFilter, 0, true);
if (sFile == "") return;
string sText = Clipboard.GetText();
Homer.Util.string2File(sText, sFile);
App.say("Done!", true);

if (Homer.Util.stringEquiv(sDir, Path.GetDirectoryName(sFile))) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
refresh_Helper(sFile);
goTo_Helper(sFile);
}
} // menuEditExportClipboardToFile_Click method

void jump_Helper(int iStart) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
int iCount = view.Count;
bool bFound = false;
for (int i = iStart; i < iCount; i++) {
DataRowView row = view[i];
string sName = (string) row["Name"];
sName = sName.ToLower();
char tagged = (char) row["Tagged"];
char hidden = (char) row["Hidden"];
char readOnly = (char) row["ReadOnly"];
char system = (char) row["System"];
string s = App.sJumpText;
if ((s == ">" && tagged == '>')
|| (s == "<" && tagged == ' ')
|| (s == ")" && hidden == ')')
|| (s == "(" && hidden == ' ')
|| (s == "]" && readOnly == ']')
|| (s == "[" && readOnly == ' ')
|| (s == "}" && system == '}')
|| (s == "{" && system == ' ')
|| (sName.Contains(App.sJumpText.ToLower()))) {
bFound = true;
mdiChild.bs.Position = i;
break;
}
}
if (!bFound) App.say("Not found!");
} // jump_Helper method

void menuNavigateJump_Click(object sender, EventArgs e) {
App.say("Jump");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.sJumpText == null) mdiChild.sJumpText = App.sJumpText;
string sText = Lbc.InputDialog("Input", "Text", mdiChild.sJumpText, "Jump");
if (sText.Length == 0) return;
int iStart = 0;
if (sText == mdiChild.sJumpText) iStart = mdiChild.bs.Position + 1;
else {
mdiChild.sJumpText = sText;
App.sJumpText = sText;
}
jump_Helper(iStart);
} // menuNavigateJump_Click method

void menuNavigateJumpAgain_Click(object sender, EventArgs e) {
App.say("Jump again");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iStart = mdiChild.bs.Position + 1;
jump_Helper(iStart);
} // menuNavigateJumpAgain_Click method

void keywords_Helper(int iStart) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
int iCount = view.Count;
char c = '&';
if (App.sKeywordsText.Contains("|")) c = '|';
string[] aKeywords = App.sKeywordsText.ToLower().Split(c);
bool bFound = false;
for (int i = iStart; i < iCount; i++) {
DataRowView row = view[i];
string sPath = (string) row["Path"];
string sBody = Homer.Util.file2String(sPath);
sBody = sBody.ToLower();
bool bSkip = false;
foreach (string sKeyword in aKeywords) {
bool bMatch = sBody.Contains(sKeyword);
if (c == '&' && !bMatch) {
bSkip = true;
break;
}
else if (c == '|' && bMatch) {
bFound = true;
break;
}
}
//if ((c == '&' && bSkip) || (c == '|' && !bFound)) continue;
if (c == '&' && !bSkip) bFound = true;
if (!bFound) continue;
mdiChild.bs.Position = i;
break;
}
if (!bFound) App.say("Not found!");
} // keywords_Helper method

void menuNavigateKeywords_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Keywords");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.sKeywordsText == null) mdiChild.sKeywordsText = App.sKeywordsText;
string sText = Lbc.InputDialog("Input", "Text", mdiChild.sKeywordsText, "Keywords");
if (sText.Length == 0) return;
int iStart = 0;
if (sText == mdiChild.sKeywordsText) iStart = mdiChild.bs.Position + 1;
else {
mdiChild.sKeywordsText = sText;
App.sKeywordsText = sText;
}
keywords_Helper(iStart);
} // menuNavigateKeywords_Click method

void menuNavigateKeywordsAgain_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Keywords again");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iStart = mdiChild.bs.Position + 1;
keywords_Helper(iStart);
} // menuNavigateKeywordsAgain_Click method

public void filter_Helper(Frame frame, MdiChild mdiChild, string sText) {
string sOldText = sText;
string sOldFilter = mdiChild.bs.Filter;
int iOldPosition = mdiChild.bs.Position;
int iOldCount = mdiChild.bs.Count;
if (sText == "*") mdiChild.bs.Filter = "";
else {
//mdiChild.bs.Filter = "Name LIKE '" + App.sFilterText + "'";
sText = sText.Trim();
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
//if ((j > 0) && (a[j - 1].Length == 0)) sPrefix = "*";
if (j > 0) sPrefix = "*";
//if ((j < a.Length - 1) && (a[j + 1].Length == 0)) sSuffix = "*";
if (j < a.Length - 1) sSuffix = "*";
s += "Name like '" + sPrefix + a[j] + sSuffix + "'";
//s += "DisplayFields like '" + sPrefix + a[j] + sSuffix + "'";
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
//Lbc.Show(s);
try {
mdiChild.bs.Filter = s;
}
catch {
App.say("Invalid expression!");
sText = sOldText;
mdiChild.bs.Filter = sOldFilter;
mdiChild.bs.Position = iOldPosition;
return;
}
}
mdiChild.bs.EndEdit();
if (sText != "*" && sText != "" && mdiChild.bs.Count == 0) {
//App.say("No items!");
string sChoice = Lbc.ConfirmDialog("Confirm", "No items match the filter.  Clear it?", "Y");
if (sChoice == "Y") {
sText = sOldFilter;
mdiChild.bs.Filter = sOldFilter;
mdiChild.bs.Position = iOldPosition;
mdiChild.sFilterText = sText;
App.sFilterText = sText;
App.sDefaultFilter = sText;
return;
}
}
mdiChild.bs.Position = 0;
mdiChild.sFilterText = sText;
App.sFilterText = sText;
} // filter_Helper method

void menuNavigateSetFilter_Click(object sender, EventArgs e) {
App.say("Filter");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.sFilterText == null) mdiChild.sFilterText = App.sFilterText;
string sText = Lbc.InputDialog("Input", "Expression", mdiChild.sFilterText, "Filter");
if (sText.Length == 0) return;

filter_Helper(App.frame, mdiChild, sText);
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuNavigateSetFilter_Click method

void menuNavigateClearFilter_Click(object sender, EventArgs e) {
App.say("Clear filter");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.sFilterText == null) mdiChild.sFilterText = App.sFilterText;
string sText = "*";
mdiChild.sFilterText = sText;
App.sFilterText = sText;
if (sText.Length == 0) return;
filter_Helper(App.frame, mdiChild, sText);
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuNavigateClearFilter_Click method

void menuNavigateBeginningFile_Click(object sender, EventArgs e) {
if (App.DirsBeforeFiles) App.say("Beginning file");
else App.say("Beginning subfolder");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
bool bFound = false;
for (int i = 0; i < view.Count; i++) {
//if (File.Exists((string) view[i]["Path"])) {
if (((char) view[i]["Type"]) == (App.DirsBeforeFiles ? ' ' : '\\')) {
mdiChild.bs.Position = i;
bFound = true;
break;
}
}
if (!bFound) App.say("Not found!");
} // menuNavigateBeginningFile_Click method

void menuNavigateBeginningTagged_Click(object sender, EventArgs e) {
App.say("Beginning tagged");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
bool bFound = false;
for (int i = 0; i < view.Count; i++) {
if ((char) view[i]["Tagged"] == '>') {
mdiChild.bs.Position = i;
bFound = true;
break;
}
}
if (!bFound) App.say("Not found!");
} // menuNavigateBeginningTagged_Click method

void menuNavigateEndTagged_Click(object sender, EventArgs e) {
App.say("End tagged");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
bool bFound = false;
for (int i = view.Count - 1; i >= 0; i--) {
if ((char) view[i]["Tagged"] == '>') {
mdiChild.bs.Position = i;
bFound = true;
break;
}
}
if (!bFound) App.say("Not found!");
} // menuNavigateEndTagged_Click method

void menuNavigateNextTagged_Click(object sender, EventArgs e) {
App.say("Next tagged");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
bool bFound = false;
for (int i = mdiChild.bs.Position + 1; i < view.Count; i++) {
if ((char) view[i]["Tagged"] == '>') {
mdiChild.bs.Position = i;
bFound = true;
break;
}
}
if (!bFound) App.say("Not found!");
} // menuNavigateNextTagged_Click method

void menuNavigatePreviousTagged_Click(object sender, EventArgs e) {
App.say("Previous tagged");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
bool bFound = false;
for (int i = mdiChild.bs.Position - 1; i >= 0; i--) {
if ((char) view[i]["Tagged"] == '>') {
mdiChild.bs.Position = i;
bFound = true;
break;
}
}
if (!bFound) App.say("Not found!");
} // menuNavigatePreviousTagged_Click method

void menuNavigateInitialChange_Click(object sender, EventArgs e) {
App.say("Initial change");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
bool bFound = false;
string s1 = (string) view[mdiChild.bs.Position]["Name"];
s1 = s1.Substring(0, 1).ToLower();
//Lbc.Show(s1);
for (int i = mdiChild.bs.Position + 1; i < view.Count; i++) {
string s2 = (string) view[i]["Name"];
s2 = s2.Substring(0, 1).ToLower();
if (s1 != s2) {
mdiChild.bs.Position = i;
bFound = true;
break;
}
}
if (!bFound) App.say("Not found!");
} // menuNavigateInitialChange_Click method

void menuNavigateExtensionChange_Click(object sender, EventArgs e) {
App.say("Extension change");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
DataView view = mdiChild.tbl.DefaultView;
bool bFound = false;
string s1 = (string) view[mdiChild.bs.Position]["Ext"];
s1 = s1.ToLower();
for (int i = mdiChild.bs.Position + 1; i < view.Count; i++) {
string s2 = (string) view[i]["Ext"];
s2 = s2.ToLower();
if (s1 != s2) {
mdiChild.bs.Position = i;
bFound = true;
break;
}
}
if (!bFound) App.say("Not found!");
} // menuNavigateExtensionChange_Click method

void menuQueryDate_Click(object sender, EventArgs e) {
App.say("Date");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
DateTime dt = (DateTime) mdiChild.tbl.DefaultView[i]["Time"];
App.say(dt.ToLongDateString() + " at " + dt.ToLongTimeString());
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuQueryDate_Click method

public string convert2Text(string sFile) {
bool bBlankDefault = false;
return convert2Text(sFile, bBlankDefault);
} // convert2Text method

public string convert2Text(string sFile, bool bBlankDefault) {
// The text of a file, for Say Contents, Append to Clipboard, Translate File
// and Chat about File alike.  One chain for all of them, in Homer.Convert.
//
// It used to send five extensions straight to 2htm and read the raw bytes of
// anything else.  Two things were wrong with that.  2htm reaches Office through
// COM for the legacy formats, so on a computer without Office it returned
// success and produced nothing, and Say Contents said nothing at all -- which
// is exactly what happened in testing.  And reading raw bytes put the innards
// of a zip on the clipboard.
//
// Now Pandoc goes first, the Open XML formats are read here, and 2htm is the
// last resort for PDF and the 1997 Office formats.  A failure says which tool
// was tried and why, out loud and in the log, rather than leaving a silence.
string sName = Path.GetFileName(sFile);
if (!File.Exists(sFile)) {
App.say(sName + " not found!");
return "";
}
string sError;
string sText = Homer.Convert.toPlainText(sFile, out sError);
if (sText.Length > 0) return sText;
Homer.Log.write("No text from " + sName + ": " + sError);
if (bBlankDefault) return "";
// Said rather than shown: this is called from commands that speak, and a box
// in the middle of a batch of twenty files would stop the batch.
App.say(sName + ": " + (sError.Length > 0 ? sError : "no text could be read"));
return "";
} // convert2Text method

public string[] getTagged() {
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
if (mdiChild == null) return new string[] {};
StringBuilder sb = new StringBuilder();
foreach (DataRow row in mdiChild.tbl.Rows) if ((Char) row["Tagged"] == '>') sb.Append((string) row["Path"] + "\n");
string[] aPaths = sb.ToString().Trim().Split('\n');
if (aPaths.Length == 1 && aPaths[0] == "") aPaths = new string[] {};
return aPaths;
} // getTagged method

public int applyTagged(string[] aPaths) {
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
if (mdiChild == null) return 0;
int iCount = 0;
foreach (DataRow row in mdiChild.tbl.Rows) if (Array.IndexOf(aPaths, (string) row["Path"]) >= 0) {
iCount ++;
row["Tagged"] = '>';
}
return iCount;
} // applyTagged method

public string getName(string sPath) {
string sName = sPath;
if (sName.EndsWith("/")) sName = sName.Substring(0, sName.Length - 1);
return Path.GetFileName(sName);
} // getName method

public string[] list_Helper(out string[] aDirs, out string[] aFiles, int iTagged) {
string sPathList = "";
string sDirList = "";
string sFileList = "";
MdiChild mdiChild = App.frame.getActiveChild();
DataView view = mdiChild.tbl.DefaultView;
foreach (DataRowView row in view) {
char c = (char) row["Tagged"];
if ((iTagged == 1 && c == ' ') || (iTagged == -1 && c == '>')) continue;
string sPath = (string) row["Path"];
sPathList += sPath + "\n";
//if (Directory.Exists(sPath)) sDirList += sPath + "\n";
if (((char) row["Type"]) == '\\') sDirList += sPath + "\n";
else sFileList += sPath + "\n";
}
aDirs = sDirList.Trim().Split('\n');
aFiles = sFileList.Trim().Split('\n');
int iDirCount = aDirs.Length;
if (sDirList.Length == 0) iDirCount = 0;
int iFileCount = aFiles.Length;
if (sFileList.Length == 0) iFileCount = 0;
string sYield = iDirCount.ToString() + " tagged folder";
if (iTagged == -1) sYield = sYield.Replace("tagged", "untagged");
if (iTagged == 0) sYield = sYield.Replace("tagged", "");
if (iDirCount != 1) sYield += "s";
if (iDirCount > 0) App.say(sYield);

//revert
if (iDirCount >=0 ) sYield = iFileCount.ToString() + " tagged file";
else sYield = "and " + iFileCount.ToString() + " file";
if (iTagged == -1) sYield = sYield.Replace("tagged", "untagged");
if (iTagged == 0) sYield = sYield.Replace("tagged", "");
if (iFileCount != 1) sYield += "s";
if (iFileCount > 0) App.say(sYield);

if (iDirCount + iFileCount == 0) {
int i = mdiChild.bs.Position;
if (i < 0) App.say("No folder or File!");
else {
sPathList = (string) view[i]["Path"];
//if (Directory.Exists(sPathList)) {
if (((char) view[i]["Type"]) == '\\') {
App.say("Current folder");
aDirs = sPathList.Trim().Split('\n');
}
else App.say("Current file");
aFiles = sPathList.Trim().Split('\n');
}
}
return sPathList.Trim().Split('\n');
} // list_Helper method

void menuQueryList_Click(object sender, EventArgs e) {
App.say("List");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iItemCount = 0;
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
App.say(row["DisplayFields"]);
iItemCount++;
}
if (iItemCount == 0) App.say("No items!");
} // menuQueryList_Click method

void MenuQuerySelected_Click(object sender, EventArgs e) {
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
//foreach (string aPath in aPaths) App.say(Path.GetFileName(aPath));
foreach (string sPath in aPaths) App.say(getName(sPath));
}
// querySelected_Click method

void menuQueryListTagged_Click(object sender, EventArgs e) {
App.say("List tagged");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iItemCount = 0;
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
if ((char) row["Tagged"] == '>') {
App.say(row["DisplayFields"]);
iItemCount++;
}
}
if (iItemCount == 0) App.say("No items!");
} // menuQueryListTagged_Click method

void menuQueryListFiles_Click(object sender, EventArgs e) {
App.say("List files");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iItemCount = 0;
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
if ((char) row["Type"] == ' ') {
App.say(row["DisplayFields"]);
iItemCount++;
}
}
if (iItemCount == 0) App.say("No items!");
} // menuQueryListFiles_Click method

void menuQuerySize_Click(object sender, EventArgs e) {
App.say("Size");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
long lSize = (long) mdiChild.tbl.DefaultView[i]["Size"];
if (lSize == -1) {
// A folder's size is not known until it is added up, which can take a moment
// on a large tree.
App.say("Adding up");
string sDir = (string) mdiChild.tbl.DefaultView[i]["Path"];
DirectoryInfo di = new DirectoryInfo(sDir);
lSize = App.DirSize(di);
mdiChild.tbl.DefaultView[i]["Size"] = lSize;
}
// The friendly form, because this command is answering "how big is it" and a
// nine-digit number is not an answer anybody keeps in their head. The exact
// count is still in the list itself and in the status line for when it matters.
App.say(App.sizeFriendly(lSize));
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuQuerySize_Click method

public string Path_Helper() {
MdiChild mdiChild = App.frame.getActiveChild();
int i = mdiChild.bs.Position;
if (i >=0 ) return (string) mdiChild.tbl.DefaultView[i]["Path"];
else return "";
} // Path_Helper Method

public object Field_Helper(string sField) {
MdiChild mdiChild = App.frame.getActiveChild();
int i = mdiChild.bs.Position;
if (i >=0) return mdiChild.tbl.DefaultView[i][sField];
else return null;
} // Field_Helper Method

public static void status_Helper(Frame frame, MdiChild mdiChild) {
int i = mdiChild.bs.Position;
if (i < 0) return;
//Lbc.Show(i, "position");
DataView view = mdiChild.tbl.DefaultView;
DateTime dDate = (DateTime) view[i]["Time"];
long lSize = (long) view[i]["Size"];
if (mdiChild.sFilterText == null || mdiChild.sFilterText == "" || mdiChild.sFilterText == "*") {
//frame.statusLabel.Text = String.Format("Date: {0}\tSize: {1}\tOrder:
frame.statusLabel.Text = String.Format("Date: {0}   Size: {1}   Order: {2}", (dDate.ToShortDateString() + " " + dDate.ToShortTimeString()), Homer.Util.formatBytes(lSize), frame.hashOrder[mdiChild.sOrderText]);
}
//else frame.statusLabel.Text = String.Format("Date: {0}\tSize: {1
else frame.statusLabel.Text = String.Format("Date: {0}   Size: {1}   Order: {2}   Filter: {3}", (dDate.ToShortDateString() + " " + dDate.ToLongTimeString()), Homer.Util.formatBytes(lSize), frame.hashOrder[mdiChild.sOrderText], mdiChild.sFilterText);
} // status_Helper method

void menuQueryPath_Click(object sender, EventArgs e) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (mdiChild.InZip) App.say("Path in archive");
else App.say("Path");
App.say(Path_Helper());
} // menuQueryPath_Click method

void menuQueryName_Click(object sender, EventArgs e) {
//App.say("File");
App.say(Path.GetFileName(Path_Helper()));
if ((char) Field_Helper("Tagged") == '>') App.say("Tagged");
} // menuQueryName_Click method

void menuQueryFolderName_Click(object sender, EventArgs e) {
//App.say("Folder");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
string sDir = mdiChild.Text;
App.say(Path.GetFileName(sDir));
} // menuQueryFolderName_Click method

void menuQueryFullFolder_Click(object sender, EventArgs e) {
//App.say("Folder");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
string sDir = mdiChild.Text;
App.say(sDir);
} // menuQueryFullFolder_Click method

void menuQueryType_Click(object sender, EventArgs e) {
App.say("Type");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
string ext = (string) mdiChild.tbl.DefaultView[i]["Ext"];
string sPath = (string) mdiChild.tbl.DefaultView[i]["Path"];
App.say(ext);
if (ext.Length > 1) ext = ext.Substring(1);
foreach (char c in ext) {
//App.say(c.ToString());
}

char type = (char) mdiChild.tbl.DefaultView[i]["Type"];
if (type == '\\') App.say("Folder");
else App.say(Homer.Util.getPathType(sPath));

char hidden = (char) mdiChild.tbl.DefaultView[i]["Hidden"];
if (hidden == ')') App.say("Hidden");
char readOnly = (char) mdiChild.tbl.DefaultView[i]["ReadOnly"];
if (readOnly == ']') App.say("ReadOnly");
char system = (char) mdiChild.tbl.DefaultView[i]["System"];
if (system == '}') App.say("System");
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuQueryType_Click method

void menuQueryTypeExtended_Click(object sender, EventArgs e) {
// Everything known about this file, from three sources, as one plain list of
// field names and values sorted by name without regard to capitalization.
//
// One sorted list rather than three sections on purpose.  Somebody looking for
// "Artist" should not have to know whether Windows, the file association, or
// ExifTool is the one that knows it, nor read three lists to find out.  With a
// screen reader, first-letter navigation through one alphabetical list beats
// arrowing through three groupings every time.
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
string sPath = Path_Helper();
System.Collections.Generic.List<string[]> lsPairs = new System.Collections.Generic.List<string[]>();

// 1. The Windows shell properties, which is what this command showed before.
foreach (string sLine in App.getShellProperties(sPath).Replace("\r\n", "\n").Split('\n')) {
int iEquals = sLine.IndexOf(" = ");
if (iEquals < 1) continue;
lsPairs.Add(new string[] {sLine.Substring(0, iEquals).Trim(), sLine.Substring(iEquals + 3).Trim()});
}

// 2. What the file association says.
try {
string sExt = Path.GetExtension(sPath);
FileAssociationInfo fa = new FileAssociationInfo(sExt);
ProgramAssociationInfo pa = new ProgramAssociationInfo(fa.ProgID);
if (pa.Exists) {
lsPairs.Add(new string[] {"ContentType", fa.ContentType});
if (fa.OpenWithList.Length > 0) lsPairs.Add(new string[] {"OpenWithList", String.Join(" ", fa.OpenWithList)});
if (fa.PerceivedType.ToString() != "None") lsPairs.Add(new string[] {"PerceivedType", fa.PerceivedType.ToString()});
lsPairs.Add(new string[] {"ProgID", fa.ProgID});
lsPairs.Add(new string[] {"AlwaysShowExtension", pa.AlwaysShowExtension.ToString()});
if (pa.DefaultIcon.Path.Length > 0) lsPairs.Add(new string[] {"DefaultIcon", pa.DefaultIcon.ToString()});
foreach (ProgramVerb verb in pa.Verbs) lsPairs.Add(new string[] {verb.Name, verb.Command});
}
}
catch (Exception) {
// An unregistered extension is ordinary, not a fault worth a message.
}

// 3. ExifTool, which knows thousands of tags across hundreds of formats: the
// camera and exposure of a photograph, the artist and album of a song, the
// duration and codecs of a video, the author and producer of a PDF.
string sExifError;
foreach (string[] aPair in Homer.Media.readProperties(sPath, out sExifError)) lsPairs.Add(aPair);

// Alphabetical by name, ignoring capitalization, and a stable tie-break on the
// value so two tags of one name keep a predictable order between runs.
lsPairs.Sort(delegate(string[] aLeft, string[] aRight) {
int iCompare = String.Compare(aLeft[0], aRight[0], StringComparison.OrdinalIgnoreCase);
if (iCompare != 0) return iCompare;
return String.Compare(aLeft[1], aRight[1], StringComparison.OrdinalIgnoreCase);
});

StringBuilder sbResult = new StringBuilder();
foreach (string[] aPair in lsPairs) sbResult.Append(aPair[0] + " = " + aPair[1] + "\r\n");
if (sExifError.Length > 0) {
// Said at the end rather than instead of the list: the Windows properties are
// still worth reading when ExifTool is absent.
sbResult.Append("\r\n" + sExifError + "\r\n");
if (Homer.Media.exifToolProgram().Length == 0) sbResult.Append(Homer.Media.exifToolLog());
}
// Match the noun to the count, and say it, because a list this long gives no
// other clue how much there is to read.
string sFieldNoun = lsPairs.Count == 1 ? "field" : "fields";
App.say(lsPairs.Count + " " + sFieldNoun);
Lbc.InfoDialog("Type Extended", sbResult.ToString(), true);
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuQueryTypeExtended_Click method

void MenuQueryWindowsOpen_Click(object sender, EventArgs e) {
object[] mdiChildren = App.frame.MdiChildren;
int iCount = mdiChildren.Length;
if (iCount == 0) App.say("No windows!");
else {
string s = Homer.Util.stringPlural("window", iCount);
App.say(s + " open");
foreach (MdiChild mdiChild in mdiChildren) {
string sTitle = mdiChild.Text;
if (!sTitle.EndsWith(@":\")) sTitle = Path.GetFileName(sTitle);
App.say(sTitle);
}
}
} // menuQueryWindowsOpen_Click method

void menuQueryYield_Click(object sender, EventArgs e) {
App.say("Yield");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iDirs = 0;
int iFiles = 0;
long lDirSize = 0;
long lFileSize = 0;
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
long lSize = (long) row["Size"];
if (lSize == -1) {
string sDir = (string) row["Path"];
DirectoryInfo di = new DirectoryInfo(sDir);
lSize = App.DirSize(di);
row["Size"] = lSize;
}

if ((char) row["Type"] == '\\') {
iDirs++;
lDirSize += lSize;
}
else {
iFiles++;
lFileSize += lSize;
}
}

string s = iDirs.ToString() + " folder";
if (iDirs !=1) s += "s";
if (iDirs > 0) App.say(s);

if (iDirs > 0) App.say(App.sizeFriendly(lDirSize));

s = iFiles.ToString() + " file";
if (iFiles !=1) s += "s";
if (iFiles > 0) App.say(s);

if (iFiles > 0) App.say(App.sizeFriendly(lFileSize));
if (iDirs == 0 && iFiles == 0) App.say("No items!");
} // menuQueryYield_Click method

void menuQueryYieldTagged_Click(object sender, EventArgs e) {
App.say("Yield tagged");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iDirs = 0;
int iFiles = 0;
long lDirSize = 0;
long lFileSize = 0;
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
if ((char) row["Tagged"] != '>') continue;
long lSize = (long) row["Size"];
if (lSize == -1) {
string sDir = (string) row["Path"];
DirectoryInfo di = new DirectoryInfo(sDir);
lSize = App.DirSize(di);
row["Size"] = lSize;
}

if ((char) row["Type"] == '\\') {
iDirs++;
lDirSize += lSize;
}
else {
iFiles++;
lFileSize += lSize;
}
}

string s = iDirs.ToString() + " folder";
if (iDirs !=1) s += "s";
if (iDirs > 0) App.say(s);

if (iDirs > 0) App.say(App.sizeFriendly(lDirSize));

s = iFiles.ToString() + " file";
if (iFiles !=1) s += "s";
if (iFiles > 0) App.say(s);

if (iFiles > 0) App.say(App.sizeFriendly(lFileSize));
if (iDirs == 0 && iFiles == 0) App.say("No items!");
} // menuQueryYieldTagged_Click method

void menuQueryYieldFiles_Click(object sender, EventArgs e) {
App.say("Yield files");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int iDirs = 0;
int iFiles = 0;
long lDirSize = 0;
long lFileSize = 0;
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
if ((char) row["Type"] != ' ') continue;
long lSize = (long) row["Size"];
if (lSize == -1) {
string sDir = (string) row["Path"];
DirectoryInfo di = new DirectoryInfo(sDir);
lSize = App.DirSize(di);
row["Size"] = lSize;
}

if ((char) row["Type"] == '\\') {
iDirs++;
lDirSize += lSize;
}
else {
iFiles++;
lFileSize += lSize;
}
}

string s = iDirs.ToString() + " folder";
if (iDirs !=1) s += "s";
if (iDirs > 0) App.say(s);

if (iDirs > 0) App.say(App.sizeFriendly(lDirSize));

s = iFiles.ToString() + " file";
if (iFiles !=1) s += "s";
if (iFiles > 0) App.say(s);

if (iFiles > 0) App.say(App.sizeFriendly(lFileSize));
if (iDirs == 0 && iFiles == 0) App.say("No items!");
} // menuQueryYieldFiles_Click method

void menuQueryYieldOnDrive_Click(object sender, EventArgs e) {
DriveInfo d = new DriveInfo(Directory.GetCurrentDirectory());
if (!d.IsReady) {
App.say("Drive not ready!");
return;
}

App.say("Yield on drive " + d.Name.Substring(0, 1) + "\t" + d.DriveType);
App.say(d.VolumeLabel);
//App.say(String.Format("  File system: {0}", d.DriveFormat));
//App.say(d.TotalSize.ToString() + " bytes");
App.say(Homer.Util.formatBytes(d.TotalSize) + " total");
//App.say(d.TotalFreeSpace.ToString() + " free");
App.say(Homer.Util.formatBytes(d.AvailableFreeSpace) + " free");
} // menuQueryYieldOnDrive_Click method

void MenuQueryYieldInOperatingSystem_Click(object sender, EventArgs e) {
String sText = App.yieldInOperatingSystem();
//Lbc.Show(sText, "Yield in Operating System");
Lbc.InfoDialog("Yield in Operating System", sText, true);
} // yieldInOperatingSystem method

void menuQueryStatus_Click(object sender, EventArgs e) {
App.say("Status");
string sText = App.frame.statusLabel.Text;
App.say(sText);
} // menuQueryStatus_Click method

void MenuQueryCharacterEncoding_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

string sFile = Path_Helper();
if (!File.Exists(sFile)) {
App.say("Current item must be a file for this command!");
return;
}

/*
Encoding en = Homer.Util.getFileEncoding(sFile);
App.say("Encoding " + en.EncodingName);
App.say("Code Page " + en.CodePage);
*/

// Report the file's encoding.  This used to run Encoding.exe; Homer.Util now
// detects it with the Ude library and reports it the same way, including the
// utf-8b / utf-8n distinction that says whether a byte-order mark is present.
if (!File.Exists(sFile)) {
App.say(Directory.Exists(sFile) ? "A folder has no encoding." : "Not found!");
return;
}
if (!Homer.Util.looksLikeText(sFile)) {
// Reporting an encoding for a picture or a program is a made-up answer, and
// the detector would spend a moment inventing it.
App.say("Not a text file, so it has no character encoding.");
return;
}
App.say(Homer.Util.getFileEncodingName(sFile).ToUpper());
} // menuQueryCharacterEncoding method

void menuQueryPercentThrough_Click(object sender, EventArgs e) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

int iCount = mdiChild.bs.Count;
if (iCount == 0) App.say("No items!");
else {
int i = mdiChild.bs.Position;
i++;
double f = 100.0 * i / iCount;
f = Math.Round(f, 1);
App.say(String.Format("{0} of {1}, {2} percent through", i, iCount, f));
}
} // menuQueryPercentThrough_Click method

void menuQueryFilter_Click(object sender, EventArgs e) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
//App.say("Filter");
App.say(App.frame.hashOrder[mdiChild.sOrderText]);
App.say(mdiChild.sFilterText);
} // menuQueryFilter_Click method

void menuQueryClipboard_Click(object sender, EventArgs e) {
bool bCut = false;
string[] aPaths = Homer.Util.clipboard2Path(out bCut);
if (aPaths.Length > 0) {
App.say("Path drop list");
foreach (string sPath in aPaths) App.say(sPath);
}
else App.say(Clipboard.GetText());
} // menuQueryClipboard_Click method

void menuQueryNow_Click(object sender, EventArgs e) {
DateTime dt = DateTime.Now;
//App.say(dt.ToLongTimeString() + " on " + dt.ToLongDateString());
App.say(dt.ToShortTimeString() + " on " + dt.ToLongDateString());
} // menuQueryNow_Click method

void menuQueryWhat_Click(object sender, EventArgs e) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
string sPath = (string) mdiChild.tbl.DefaultView[i]["Path"];
if (mdiChild.InZip) {
App.say("What");
string sZip = mdiChild.Text;
string sDir = Path.GetTempFileName();
App.lsTempFile.Add(sDir);
sDir = Path.GetDirectoryName(sDir);
string sTarget = zipEntry2Dir(sZip, sPath, sDir);
App.lsTempFile.Add(sTarget);
App.say(Homer.Util.stringConvertQuotes(App.frame.convert2Text(sTarget)));
}
else if (Directory.Exists(sPath)) {
// App.say("Folder items");
// foreach (string s in Directory.GetDirectories(sPath)) App.say(Path.GetFileName(s) + @"\");
// foreach (string s in Directory.GetFiles(sPath)) App.say(Path.GetFileName(s));
List<string> lItems = new List<string>(Directory.GetDirectories(sPath));
lItems.AddRange(Directory.GetFiles(sPath));
App.say(Homer.Util.stringPlural("Folder item", lItems.Count));
foreach (string sItem in lItems) {
string s = Path.GetFileName(sItem);
if (Directory.Exists(sItem)) s += @"\";
App.say(s);
}
}
else if (File.Exists(sPath)) {
if (Homer.Util.stringEquiv(".zip", Path.GetExtension(sPath)) || testZip(sPath)) {
// App.say("Archive items");
// foreach (ZipEntry entry in getZipEntries(sPath)) App.say(entry.Name);
List<string> lEntries = GetZ7Entries(sPath);
int iCount = lEntries.Count;
App.say(Homer.Util.stringPlural("Archive item", iCount));
foreach (string s in lEntries) App.say(s);
}
else {
App.say("What");
//Lbc.SayLines(App.frame.convert2Text(sPath));
//Lbc.Show(Homer.Util.stringConvertQuotes(App.frame.convert2Text(sPath)));
App.say(Homer.Util.stringConvertQuotes(App.frame.convert2Text(sPath)));
}
}
} // menuQueryWhat_Click method

void menuQueryTimer_Click(object sender, EventArgs e) {
if (App.timer == null) {
App.say("No timer!");
return;
}

TimeSpan elapsed;
if (App.timer == null || (!App.timer.Enabled && App.tsElapsed.Ticks == 0)) {
elapsed = DateTime.Now - App.dtTimerStart;
timer_Helper(elapsed);
}
else if (App.timer.Enabled) {
elapsed = App.tsElapsed + (DateTime.Now - App.dtTimerStart);
timer_Helper(elapsed);
}
else timer_Helper(App.tsElapsed);
} // menuQueryTimer_Click method

void menuMiscOptions_Click(object sender, EventArgs e) {
App.say("Configuration options");
//App.readIni();
string[] aFields = {"&Text Editor", "&Word Processor", "&LogInUserName", "&Password", "&SenderAddress", "&OutgoingServer", "&UnarchivePassword", "&RecycleWithDelete", "&DirsBeforeFiles", "E&xtraSpeech", "&ZipOpener"};
string[] aValues = {App.sTextEditor, App.sWordProcessor, App.sUserName, App.sPassword, App.sSenderAddress, App.sOutgoingServer, App.sUnarchivePassword, (App.Recycle ? "y" : "n"), (App.DirsBeforeFiles ? "y" : "n"), (App.bExtraSpeech ? "y" : "n"), (App.bZipOpener ? "y" : "n")};
//Lbc.Show(aFields.Length, aValues.Length);
ArrayList sResultList = Lbc.FieldDialog("Fields", aFields, aValues);
if (sResultList.Count == 0) return;

App.sTextEditor = (string) sResultList[0];
App.sWordProcessor = (string) sResultList[1];
App.sUserName = (string) sResultList[2];
App.sPassword = (string) sResultList[3];
App.sSenderAddress = (string) sResultList[4];
App.sOutgoingServer = (string) sResultList[5];
App.sUnarchivePassword = (string) sResultList[6];
App.Recycle = ((string) sResultList[7]).Trim().ToUpper() == "N" ? false : true;
bool bDirsBeforeFiles = App.DirsBeforeFiles;
App.DirsBeforeFiles = ((string) sResultList[8]).Trim().ToUpper() == "N" ? false : true;
App.bExtraSpeech = ((string) sResultList[9]).Trim().ToUpper() == "N" ? false : true;
App.bZipOpener = ((string) sResultList[10]).Trim().ToUpper() == "N" ? false : true;
App.writeIni();
if (App.DirsBeforeFiles != bDirsBeforeFiles) hashOrder_Helper();
} // menuMiscOptions_Click method

void menuMiscManualOptions_Click(object sender, EventArgs e) {
App.say("Manual options");
App.writeIni();
Process.Start(App.sTextEditor, Homer.Util.stringQuote(Homer.Util.getLfn(App.sIniFile)));
// readIni();
} // menuMiscManualOptions_Click method

void menuMiscExtraSpeechToggle_Click(object sender, EventArgs e) {
bool bSpeech = App.bExtraSpeech;
App.bExtraSpeech = true;
App.say("Extra speech");
App.say(bSpeech ? "Off" : "On");
App.bExtraSpeech = !bSpeech;
} // menuMiscExtraSpeechToggle_Click method

void menuMiscExtraSpeechLog_Click(object sender, EventArgs e) {
App.say("Extra speech log");
Process.Start(App.sSpeechLog);
} // menuMiscExtraSpeechLog_Click method

void menuMiscEnvironmentVariables_Click(object sender, EventArgs e) {
App.say("Environment Variables");
string sChoice = Lbc.ButtonDialog("Target", "", new string[] {"&Process", "&User", "&Machine"}, 0);
if (sChoice.Length == 0) return;

EnvironmentVariableTarget target = EnvironmentVariableTarget.Process;
if (sChoice == "&User") target = EnvironmentVariableTarget.User;
else if (sChoice == "&Machine") target = EnvironmentVariableTarget.Machine;

IDictionary dic = Environment.GetEnvironmentVariables(target);
int iCount = dic.Count;
string[] aLabels = new string[iCount];
string[] aValues = new string[iCount];
string[] aKeys = new string[iCount];

int iKey = 0;
foreach (DictionaryEntry de in dic) {
aKeys[iKey] = ((string) de.Key).ToLower();
aLabels[iKey] = "&" + (string) de.Key;
iKey++;
}
Array.Sort(aKeys, aLabels);

iKey = 0;
foreach (DictionaryEntry de in dic) {
aKeys[iKey] = ((string) de.Key).ToLower();
aValues[iKey] = (string) de.Value;
iKey++;
}
Array.Sort(aKeys, aValues);

string sTitle = "Variables for ";
if (sChoice == "&Process") sTitle += "Process " + Process.GetCurrentProcess().ProcessName;
else if (sChoice == "&User") sTitle += "User " + Environment.UserName;
else if (sChoice == "&Machine") sTitle += "Machine " + Environment.MachineName;
ArrayList listResults = Lbc.FieldDialog(sTitle, aLabels, aValues);
if (listResults.Count == 0) return;

string[] aResults = new string[listResults.Count];
for (int i = 0; i < aResults.Length; i++) aResults[i] = listResults[i].ToString();

try {
for (int i = 0; i < iCount; i++) {
if (aResults[i] == aValues[i]) continue;
if (Lbc.ConfirmDialog("Confirm", "Change " + aKeys[i] + "?", "Y") != "Y") continue;
Environment.SetEnvironmentVariable(aKeys[i], aResults[i], target);
}
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
App.say("Done!", true);
} // menuMiscEnvironmentVariables_Click method

void menuMiscOpenRecycleBin_Click(object sender, EventArgs e) {
App.say("Recycle Bin");
string sExe = "Explorer.exe";
string sParam = "/root,::{645FF040-5081-101B-9F08-00AA002F954E}";
Process.Start(sExe, sParam);
} // menuMiscOpenRecycleBin_Click method

void menuMiscRecycleToggle_Click(object sender, EventArgs e) {
if (App.Recycle) {
App.say("No recycle");
App.Recycle = false;
}
else {
App.say("Recycle with delete");
App.Recycle = true;
}
} // menuMiscRecycleToggle_Click method

void menuMiscAlphaOrder_Click(object sender, EventArgs e) {
App.say("Alpha order");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sOrderText = "Type desc, Name";
if (!App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
mdiChild.bs.Sort = mdiChild.sOrderText;
mdiChild.bs.Position = 0;
App.sOrderText = mdiChild.sOrderText;
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuMiscAlphaOrder_Click method

void menuMiscReverseAlphaOrder_Click(object sender, EventArgs e) {
App.say("Reverse alpha order");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sOrderText = "Type desc, Name desc";
if (!App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
mdiChild.bs.Sort = mdiChild.sOrderText;
mdiChild.bs.Position = 0;
App.sOrderText = mdiChild.sOrderText;
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuMiscReverseAlphaOrder_Click method

void menuMiscDateOrder_Click(object sender, EventArgs e) {
App.say("Date order");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sOrderText = "Type desc, Time";
if (!App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
mdiChild.bs.Sort = mdiChild.sOrderText;
mdiChild.bs.Position = 0;
App.sOrderText = mdiChild.sOrderText;
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuMiscDateOrder_Click method

void menuMiscReverseDateOrder_Click(object sender, EventArgs e) {
App.say("Reverse date order");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sOrderText = "Type desc, Time desc";
if (!App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
mdiChild.bs.Sort = mdiChild.sOrderText;
mdiChild.bs.Position = 0;
App.sOrderText = mdiChild.sOrderText;
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuMiscReverseDateOrder_Click method

void menuMiscSizeOrder_Click(object sender, EventArgs e) {
App.say("Size order");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sOrderText = "Type desc, Size";
if (!App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
mdiChild.bs.Sort = mdiChild.sOrderText;
mdiChild.bs.Position = 0;
App.sOrderText = mdiChild.sOrderText;
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuMiscSizeOrder_Click method

void menuMiscReverseSizeOrder_Click(object sender, EventArgs e) {
App.say("Reverse Size order");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sOrderText = "Type desc, Size desc";
if (!App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
mdiChild.bs.Sort = mdiChild.sOrderText;
mdiChild.bs.Position = 0;
App.sOrderText = mdiChild.sOrderText;
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuMiscReverseSizeOrder_Click method

void menuMiscTypeOrder_Click(object sender, EventArgs e) {
App.say("Type order");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sOrderText = "Type desc, Ext";
if (!App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
mdiChild.bs.Sort = mdiChild.sOrderText;
mdiChild.bs.Position = 0;
App.sOrderText = mdiChild.sOrderText;
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuMiscTypeOrder_Click method

void menuMiscReverseTypeOrder_Click(object sender, EventArgs e) {
App.say("Reverse Type order");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
mdiChild.sOrderText = "Type desc, Ext desc";
if (!App.DirsBeforeFiles) mdiChild.sOrderText = mdiChild.sOrderText.Replace("Type desc", "Type asc");
mdiChild.bs.Sort = mdiChild.sOrderText;
mdiChild.bs.Position = 0;
App.sOrderText = mdiChild.sOrderText;
status_Helper(App.frame, (MdiChild) App.frame.ActiveMdiChild);
} // menuMiscReverseTypeOrder_Click method

void MenuMiscSendToWordProcessor_Click(object sender, EventArgs e) {
App.say("Send to word processor");
if (App.sWordProcessor .Trim().Length == 0) App.sWordProcessor = "WinWord.exe";
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
string sPath = (string) mdiChild.tbl.DefaultView[i]["Path"];
string sName = Path.GetFileName(sPath);
if (mdiChild.InZip) {
App.say("With temp file");
string sZip = mdiChild.Text;
string sDir = Path.GetTempFileName();
App.lsTempFile.Add(sDir);
sDir = Path.GetDirectoryName(sDir);
string sTarget = zipEntry2Dir(sZip, sPath, sDir);
App.lsTempFile.Add(sTarget);
Process.Start(App.sWordProcessor, Homer.Util.stringQuote(sTarget));
}
else if (File.Exists(sPath)) {
Process.Start(App.sWordProcessor, Homer.Util.stringQuote(sPath));
}
else {
App.say(sName + " not found!");
}
} // menuMiscSendToWordProcessor_Click method

void MenuMiscSendToTextEditor_Click(object sender, EventArgs e) {
App.say("Send to text editor");
if (App.sTextEditor.Trim().Length == 0) App.sTextEditor = "EdSharp.exe";
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
int i = mdiChild.bs.Position;
string sPath = (string) mdiChild.tbl.DefaultView[i]["Path"];
string sName = Path.GetFileName(sPath);
if (mdiChild.InZip) {
App.say("With temp file");
string sZip = mdiChild.Text;
string sDir = Path.GetTempFileName();
App.lsTempFile.Add(sDir);
sDir = Path.GetDirectoryName(sDir);
string sTarget = zipEntry2Dir(sZip, sPath, sDir);
App.lsTempFile.Add(sTarget);
Process.Start(App.sTextEditor, Homer.Util.stringQuote(sTarget));
}
else if (File.Exists(sPath)) Process.Start(App.sTextEditor, Homer.Util.stringQuote(sPath));
else {
App.say(sName + " not found!");
}
} // menuMiscSendToTextEditor_Click method

//void MenuHelpExplorerMenu_Click(object sender, EventArgs e) {
void MenuHelpContextMenu_Click(object sender, EventArgs e) {
App.say("Context menu");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);

string[] aVerbs = Homer.Util.verbs(Path_Helper());
bool bFound = false;
foreach (string s in aVerbs) {
if (s.Contains("pen Wit")) bFound = true;
if (bFound) break;
} // foreach s
if (!bFound) {
Array.Resize(ref aVerbs, aVerbs.Length + 1);
aVerbs[aVerbs.Length - 1] = "Open With...";
}

string[] aNames = new string[aVerbs.Length];
for (int iVerb = 0; iVerb < aVerbs.Length; iVerb++) aNames[iVerb] = aVerbs[iVerb].Replace("&", "");
string sName = Lbc.ListDialog("Pick", "", aNames, true, 0);
if (sName == "") return;

int i = Array.IndexOf(aNames, sName);
string sVerb = aVerbs[i];

foreach (string sPath in aPaths) {
sName = Path.GetFileName(sPath);
App.say(sName);
if (mdiChild.InZip) {
App.say("With temp file");
string sZip = mdiChild.Text;
string sDir = Path.GetTempFileName();
App.lsTempFile.Add(sDir);
sDir = Path.GetDirectoryName(sDir);
string sTarget = zipEntry2Dir(sZip, sPath, sDir);
App.lsTempFile.Add(sTarget);
// if (sVerb == "Open With...") Lbc.OpenWith(sTarget);
if (sVerb.Replace("&", "") == "Open With...") Homer.Util.run("Rundll32.exe shell32.dll, OpenAs_RunDLL " + sTarget);
else Homer.Util.invokeVerb(sTarget, sVerb);
}
//else if (File.Exists(sPath)) {
else if (App.itemExists(sPath)) {
// if (sVerb == "Open With...") Lbc.OpenWith(sPath);
if (sVerb.Replace("&", "") == "Open With...") Homer.Util.run("Rundll32.exe shell32.dll, OpenAs_RunDLL " + sPath);
else Homer.Util.invokeVerb(sPath, sVerb);
}
else if (Directory.Exists(sPath)) {
App.say("Skipping " + sName);
continue;
}
else {
App.say(sName + " not found!");
continue;
}
}
App.say("Done!", true);
} // menuHelpExplorerMenu_Click method

void MenuHelpSendToMenu_Click(object sender, EventArgs e) {
App.say("SendTo menu");
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);

string sDir = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);
string[]aLinks = Directory.GetFiles(sDir);
string sNameList = "";
foreach (string s in aLinks) sNameList += Path.GetFileNameWithoutExtension(s) + "\n";
string[]aNames = sNameList.Trim().Split('\n');
string sName = Lbc.ListDialog("Pick", "", aNames, true, 0);
if (sName == "") return;

int i = Array.IndexOf(aNames, sName);
string sLink = aLinks[i];

foreach (string sPath in aPaths) {
sName = Path.GetFileName(sPath);
App.say(sName);
if (mdiChild.InZip) {
App.say("With temp file");
string sZip = mdiChild.Text;
sDir = Path.GetTempFileName();
App.lsTempFile.Add(sDir);
sDir = Path.GetDirectoryName(sDir);
string sTarget = zipEntry2Dir(sZip, sPath, sDir);
App.lsTempFile.Add(sTarget);
Process.Start(sLink, sTarget);
}
else if (File.Exists(sPath)) {
Process.Start(sLink, sPath);
}
else if (Directory.Exists(sPath)) {
App.say("Skipping " + sName);
continue;
}
else {
App.say(sName + " not found!");
continue;
}
}
App.say("Done!", true);
} // menuHelpSendToMenu_Click method

void MenuMiscOutputTagged_Click(object sender, EventArgs e) {
// Output Type.  Look at what the file IS, offer what it can become, and convert
// the tagged files, or the current one, keeping each root name.
//
// This was Output to Text, which only ever wrote .txt.  Text is still one of
// the choices; the rest were behind a second command until now.  A file manager
// reads and writes binary files, so there is no reason to stop at documents:
// mp4 to mp3, mkv to mp4, png to jpg all belong here.
//
// Three engines do the work and nobody has to know which: Pandoc for documents,
// 2htm for the legacy Office formats and PDF that Pandoc cannot read, and
// ffmpeg for audio, video and pictures.
if (abortInZip()) return;
string sTitle = "Output Type";
App.say(sTitle);
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

// The first real file decides what is offered.  A batch is nearly always one
// kind of thing, and asking per file would defeat the point of tagging.
string sDriver = "";
foreach (string sPath in aPaths) {
if (File.Exists(sPath)) { sDriver = sPath; break; }
}
if (sDriver.Length == 0) {
App.say("No file to convert.");
return;
}
string[] aExtensions;
string[] aLabels = Homer.Convert.targetLabelsFor(sDriver, out aExtensions);
if (aLabels.Length == 0) {
Lbc.Show("FileDir does not know what " + Path.GetExtension(sDriver).ToLower()
+ " files can become.\n\nDocuments, legacy Office files, PDF, audio, video and pictures are all understood.", sTitle);
return;
}

// The remembered choice is kept per category, so picking mp3 for audio does not
// become the default the next time a Word document is tagged.
string sCategory = Homer.Convert.categoryOf(sDriver);
string sKey = "OutputType" + sCategory;
int iIndex = 0;
string sRemembered = App.readValue(App.sIniFile, "Data", sKey, "");
if (sRemembered.Length > 0) {
int iFound = Array.IndexOf(aLabels, sRemembered);
if (iFound >= 0) iIndex = iFound;
}
bool bSort = false;
string sPicked = Dialog.Pick("Pick", aLabels, bSort, iIndex);
if (sPicked.Length == 0) return;
App.writeValue(App.sIniFile, "Data", sKey, sPicked);
int iTarget = Array.IndexOf(aLabels, sPicked);
if (iTarget < 0) return;
string sExt = aExtensions[iTarget];

App.say("Writing " + sExt + " files");
string sLastMade = "";
int iDone = 0;
int iSkipped = 0;
int iFailed = 0;
for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
string sName = Path.GetFileName(sPath);
if (Directory.Exists(sPath)) {
App.say("Skipping folder " + sName);
continue;
}
if (!File.Exists(sPath)) {
App.say(sName + " not found!");
continue;
}
if (String.Compare(Path.GetExtension(sPath).TrimStart('.'), sExt, true) == 0) {
App.say("Skipping " + sName + ", already " + sExt);
iSkipped++;
continue;
}
if (Homer.Convert.categoryOf(sPath).Length == 0) {
App.say("Skipping " + sName + ", which FileDir cannot convert");
iSkipped++;
continue;
}
App.say(sName + ", " + (i + 1) + " of " + aPaths.Length);
// Same root name, same folder, made unique so nothing is ever overwritten.
string sTarget = Path.Combine(Path.GetDirectoryName(sPath), Path.GetFileNameWithoutExtension(sPath) + "." + sExt);
sTarget = Homer.Web.uniquePath(sTarget);
string sError;
if (Homer.Convert.convertAny(sPath, sTarget, out sError)) {
iDone++;
sLastMade = sTarget;
}
else {
App.say(sName + ": " + sError);
iFailed++;
}
}
string sFileNoun = iDone == 1 ? "file" : "files";
string sMessage = iDone + " " + sFileNoun + " written";
if (iSkipped > 0) sMessage += ", " + iSkipped + " skipped";
if (iFailed > 0) sMessage += ", " + iFailed + " failed";
App.say(sMessage + ".", true);
refreshFolder_Helper(mdiChild);
// Land on what was just made, rather than wherever the refresh left the
// cursor. goTo_Helper finds the file only if the filter in effect still shows
// it; when a filter hides it the cursor stays put, which is the honest outcome
// rather than a jump to nothing.
if (sLastMade.Length > 0) goTo_Helper(sLastMade);
} // MenuMiscOutputTagged_Click method

void MenuMiscAppendTagged_Click(object sender, EventArgs e) {
// Append to Clipboard.  The text of every tagged file, one after another, so a
// folder of sources becomes one set of notes in the order you tagged them.
//
// Whatever the file is.  A Word document goes through Pandoc, a PDF or a legacy
// .doc through 2htm, and anything already text is simply read.  A format none
// of them can read is skipped with a word saying so, rather than having its raw
// bytes put on the clipboard, which is what happened before: a tagged .zip put
// a screenful of rubbish there and nothing said why.
if (abortInZip()) return;
App.say("Append to clipboard");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

// THE FIRST FILE ONTO AN EMPTY CLIPBOARD KEEPS ITS FORMATTING.
//
// Copying one web page or one Word document and pasting it somewhere that
// understands formatting should give the headings and links, not a flattened
// wall of text. So when the clipboard is empty and exactly one rich file is
// being copied, it goes on as HTML as well as text, and whatever it is pasted
// into takes the richest form it understands.
//
// APPENDING IS DIFFERENT, and has to be. Two documents in two rich formats
// cannot be joined without deciding whose styling wins, and the answer would be
// wrong half the time. So anything appended is Markdown -- which keeps the
// headings, lists and links as text that reads perfectly well on its own, and
// joins cleanly.
string sExisting = "";
try { sExisting = Clipboard.ContainsText() ? Clipboard.GetText().Trim() : ""; }
catch (Exception) { }
bool bFirst = sExisting.Length == 0;
StringBuilder sbText = new StringBuilder(sExisting);
int iDone = 0;
int iSkipped = 0;
string sRichSource = "";
for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
string sName = Path.GetFileName(sPath);
if (Directory.Exists(sPath)) {
App.say("Skipping folder " + sName);
continue;
}
if (!File.Exists(sPath)) {
App.say(sName + " not found!");
continue;
}
App.say(sName);
string sError;
string sBody = Homer.Convert.toPlainText(sPath, out sError);
if (sBody.Trim().Length == 0) {
App.say("Skipping " + sName + ": " + (sError.Length > 0 ? sError : "no text"));
iSkipped++;
continue;
}
if (Homer.Convert.hasRichForm(sPath)) sRichSource = sPath;
if (sbText.Length > 0) sbText.Append(Homer.Util.sSectionBreak);
// Each piece is headed by its file name, because three sources run together
// with no seam is one document nobody can take apart again.
sbText.Append(sName + "\r\n\r\n");
sbText.Append(sBody.Trim());
iDone++;
}
sbText.Append(Homer.Util.sEndOfDocument);

// One rich file onto an empty clipboard: offered as HTML as well, so a paste
// into a mail message or a document keeps the formatting.
bool bRich = false;
if (bFirst && iDone == 1 && sRichSource.Length > 0) {
string sHtmlError;
string sHtml = Homer.Convert.toHtmlFragment(sRichSource, out sHtmlError);
if (sHtml.Length > 0) {
try {
DataObject data = new DataObject();
data.SetData(DataFormats.UnicodeText, sbText.ToString());
data.SetData(DataFormats.Html, Homer.Convert.htmlClipboardFormat(sHtml));
Clipboard.SetDataObject(data, true);
bRich = true;
}
catch (Exception ex) {
Homer.Log.write("Could not put HTML on the clipboard: " + ex.Message);
}
}
else if (sHtmlError.Length > 0) Homer.Log.write("No HTML for the clipboard: " + sHtmlError);
}
if (!bRich) Clipboard.SetText(sbText.ToString());
string sFileNoun = iDone == 1 ? "file" : "files";
string sMessage = iDone + " " + sFileNoun + " appended";
if (iSkipped > 0) sMessage += ", " + iSkipped + " skipped";
App.say(sMessage + ".", true);
} // menuEditAppendTagged_Click method

void MenuMiscConvertEncodingTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
string sTitle = "Convert Encoding";
App.say(sTitle);
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

// The same list Encoding.exe offered: every encoding this computer has, plus
// utf-8b (with byte-order mark), utf-8n (without), and asciify.
string[] aEncodings = Homer.Util.getEncodingNames();
string sEncoding = App.readValue(App.sIniFile, "Data", "ConvertEncoding", "");
int iIndex = -1;
// The remembered choice, matched as it is stored. This looked it up with
// ToUpper while the list holds lower case names, so it never matched and the
// list always opened at the top -- and a debug line beside it overwrote the
// CLIPBOARD with the index, every single time the command was run.
if (sEncoding.Length > 0) iIndex = Array.IndexOf(aEncodings, sEncoding.ToLower());
if (iIndex == -1) iIndex = 0;
bool bSort = false;
sTitle = "Pick";
sEncoding = Dialog.Pick(sTitle, aEncodings, bSort, iIndex).ToLower();
if (sEncoding.Length == 0) return;

App.writeValue(App.sIniFile, "Data", "ConvertEncoding", sEncoding);

for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
string sName = Path.GetFileName(sPath);
if (Directory.Exists(sPath)) {
App.say("Skipping folder " + sName);
}
else if (File.Exists(sPath) ){
App.say(sName);
// Detect the source encoding, rewrite in the chosen one, and keep the .bak
// copy -- exactly Encoding.exe's "backup" task, which is the one FileDir used.
// The target may be utf-8b, utf-8n, asciify, or any named encoding.
try {
Homer.Util.convertFileEncoding(sPath, sEncoding, true);
}
catch (Exception ex) {
App.say(sName + ": " + ex.Message);
}
}
else App.say(sName + " not found!");
}
App.say("Done!", true);
} // menuEditConvertEncodingTagged_Click method

void MenuMiscChatWithAI_Click(object sender, EventArgs e) {
// A plain question, with no file attached, answered by a model on this computer.
// EdSharp puts this on F12 and FileDir now matches it.
//
// Shift+F12 is the one that sends the file.  Keeping them apart matters: a
// question about a folder full of names, or about how to phrase a command, has
// nothing to do with whatever the cursor happens to be on.
string sTitle = "Chat with AI";
App.say(sTitle);
if (!Homer.Ollama.isRunning()) {
Lbc.Show("Ollama is not running on this computer, so there is nothing to ask.\n\nTo add it, run installOllama.cmd in the FileDir folder, or install FileDir again and tick the Ollama box.\n\nOllama is shared with EdSharp and DbDo, so if you have installed it for one of those it is already here.", sTitle);
return;
}
string sModel = Homer.Ollama.bestTranslateModel();
if (sModel.Length == 0) {
Lbc.Show("Ollama is running, but no model is installed.\n\nRun installOllama.cmd in the FileDir folder to fetch llama3.2.", sTitle);
return;
}
string sQuestion = Lbc.InputDialog("Chat with AI", "&Question", "", "ChatWithAI").Trim();
if (sQuestion.Length == 0) return;
App.say("Asking " + sModel + ". This can take a while.");
string sError;
string sAnswer = Homer.Ollama.generate(sModel, sQuestion, 600000, out sError);
if (sError.Length > 0) {
Lbc.Show("The question could not be answered.\n\n" + sError, sTitle);
return;
}
if (sAnswer.Trim().Length == 0) {
Lbc.Show("The model returned nothing. Try asking in fewer words.", sTitle);
return;
}
Lbc.AnswerDialog(sTitle, "&Answer", sAnswer.Trim());
} // MenuMiscChatWithAI_Click method

void MenuMiscChatAboutFile_Click(object sender, EventArgs e) {
// Ask a question about the current file, answered by a model running on this
// computer.  The file's text travels with the question, so "what is this about"
// and "list the dates in it" both work.
//
// The keys match EdSharp exactly: F12 asks a plain question, Shift+F12 asks
// about the file you are on.  The Timer commands held this column here for
// twenty years and moved to Alt+Control, which nothing else used.  Matching the
// sibling program is worth more than protecting a habit few people had.
if (abortInZip()) return;
string sTitle = "Chat about File";
App.say(sTitle);
string sPath = Path_Helper();
if (!File.Exists(sPath)) {
Lbc.Show("Move to a file first. This command asks about one file, not a folder.", sTitle);
return;
}
if (!Homer.Ollama.isRunning()) {
Lbc.Show("Ollama is not running on this computer, so there is nothing to ask.\n\nTo add it, run installOllama.cmd in the FileDir folder, or install FileDir again and tick the Ollama box.\n\nOllama is shared with EdSharp and DbDo, so if you have installed it for one of those it is already here.", sTitle);
return;
}
string sModel = Homer.Ollama.bestTranslateModel();
if (sModel.Length == 0) {
Lbc.Show("Ollama is running, but no model is installed.\n\nRun installOllama.cmd in the FileDir folder to fetch llama3.2.", sTitle);
return;
}

string sError;
string sText = Homer.Convert.toPlainText(sPath, out sError);
if (sText.Trim().Length == 0) {
Lbc.Show("There is no text in " + Path.GetFileName(sPath) + " to ask about"
+ (sError.Length > 0 ? ":\n\n" + sError : ".") , sTitle);
return;
}

string sQuestion = App.readValue(App.sIniFile, "Data", "ChatQuestion", "Summarize this file.");
sQuestion = Lbc.InputDialog("Chat about File", "&Question", sQuestion, "ChatQuestion").Trim();
if (sQuestion.Length == 0) return;
App.writeValue(App.sIniFile, "Data", "ChatQuestion", sQuestion);

// The file is trimmed to what the model can hold.  A quiet truncation would be
// worse than saying so: an answer about the first half of a document, presented
// as an answer about the document, is wrong in a way nobody can see.
System.Collections.Generic.List<string> lsParts = Homer.Ollama.splitForModel(sText, 12000);
string sSent = lsParts.Count > 0 ? lsParts[0] : "";
if (lsParts.Count > 1) App.say("The file is long, so the first part is being asked about.");

App.say("Asking " + sModel + ". This can take a while.");
string sPrompt = sQuestion + "\r\n\r\nHere is the file, " + Path.GetFileName(sPath) + ":\r\n\r\n" + sSent;
string sAnswer = Homer.Ollama.generate(sModel, sPrompt, 600000, out sError);
if (sError.Length > 0) {
Lbc.Show("The question could not be answered.\n\n" + sError, sTitle);
return;
}
if (sAnswer.Trim().Length == 0) {
Lbc.Show("The model returned nothing. Try a shorter question, or a smaller file.", sTitle);
return;
}
if (lsParts.Count > 1)
sAnswer = "(Answered from the first part of the file only; it was too long to send whole.)\r\n\r\n" + sAnswer;
Lbc.AnswerDialog(sTitle, "&Answer", sAnswer.Trim());
} // MenuMiscChatAboutFile_Click method

void MenuMiscTranslateTagged_Click(object sender, EventArgs e) {
// Translate the tagged files, or the current one, with a language model
// running on this computer.  Nothing leaves the machine.
//
// Batch by design: tag a folder full of files, answer one question, and walk
// away.  A file manager is the right place for that, and it is the reason this
// lives here rather than inside an editor.
if (abortInZip()) return;
string sTitle = "Translate File";
App.say(sTitle);
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;

// Ask Ollama what it has before asking the person anything.  Being told the
// feature needs a download is better than typing a language first and being
// told afterwards.
if (!Homer.Ollama.isRunning()) {
Lbc.Show("Ollama is not running on this computer, so nothing can be translated.\n\nTo add it, run installOllama.cmd in the FileDir folder, or install FileDir again and tick the Ollama box.\n\nOllama is shared with EdSharp and DbDo, so if you have installed it for one of those it is already here.", sTitle);
return;
}
string sModel = Homer.Ollama.bestTranslateModel();
if (sModel.Length == 0) {
Lbc.Show("Ollama is running, but neither translation model is installed.\n\nRun installOllama.cmd in the FileDir folder for llama3.2, or installTranslateModel.cmd for qwen2.5:7b, which translates better.", sTitle);
return;
}

string sLanguage = App.readValue(App.sIniFile, "Data", "TranslateLanguage", "Spanish");
sLanguage = Lbc.InputDialog("Translate File", "&Language to translate into", sLanguage, "TranslateLanguage").Trim();
if (sLanguage.Length == 0) return;
App.writeValue(App.sIniFile, "Data", "TranslateLanguage", sLanguage);

App.say("Translating into " + sLanguage + " with " + sModel);
int iDone = 0;
int iFailed = 0;
for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
string sName = Path.GetFileName(sPath);
if (Directory.Exists(sPath)) {
App.say("Skipping folder " + sName);
continue;
}
if (!File.Exists(sPath)) {
App.say(sName + " not found!");
continue;
}
// Match the noun to the count, and say which file of how many, because a
// model takes its time and silence is indistinguishable from a hang.
App.say(sName + ", " + (i + 1) + " of " + aPaths.Length);
string sText = convert2Text(sPath);
if (sText.Trim().Length == 0) {
App.say(sName + " has no text to translate.");
continue;
}
string sTranslated = translate_Helper(sText, sLanguage, sModel, sName);
if (sTranslated.Length == 0) {
iFailed++;
continue;
}
// Beside the original, named for the language, so a folder of translations
// sorts next to its sources and nothing is ever overwritten.
string sOut = Path.Combine(Path.GetDirectoryName(sPath), Path.GetFileNameWithoutExtension(sPath) + "." + sLanguage + ".txt");
sOut = Homer.Web.uniquePath(sOut);
try {
File.WriteAllText(sOut, sTranslated, new UTF8Encoding(true));
iDone++;
}
catch (Exception ex) {
App.say(sName + ": " + ex.Message);
iFailed++;
}
}
string sFileNoun = iDone == 1 ? "file" : "files";
if (iFailed == 0) App.say(iDone + " " + sFileNoun + " translated into " + sLanguage + ".", true);
else App.say(iDone + " " + sFileNoun + " translated, " + iFailed + " not done.", true);
// The translations are new files in this folder, so show them without making
// anyone press Refresh to find out the command worked.
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild != null) refreshFolder_Helper(mdiChild);
} // MenuMiscTranslateTagged_Click method

string translate_Helper(string sText, string sLanguage, string sModel, string sName) {
// One file, in pieces the model can hold, joined back together.  A piece is
// kept well under the context window: a model given more than it can hold
// silently drops the end, which looks like a translation that stops.
System.Collections.Generic.List<string> lsParts = Homer.Ollama.splitForModel(sText, 4000);
StringBuilder sbResult = new StringBuilder();
for (int i = 0; i < lsParts.Count; i++) {
if (lsParts.Count > 1) App.say("Part " + (i + 1) + " of " + lsParts.Count);
string sPrompt = "Translate the following text into " + sLanguage + ".\n"
+ "Reply with the translation only: no preamble, no explanation, no quotation marks around it.\n"
+ "Keep the paragraph breaks of the original.\n\n"
+ lsParts[i];
string sError;
// Ten minutes a part.  A large model on a modest machine is slow rather than
// broken, and giving up early on it would be the wrong answer.
string sPiece = Homer.Ollama.generate(sModel, sPrompt, 600000, out sError);
if (sError.Length > 0) {
Lbc.Show(sName + " could not be translated.\n\n" + sError, "Translate File");
return "";
}
if (sbResult.Length > 0) sbResult.Append("\r\n\r\n");
sbResult.Append(sPiece.Trim());
}
return sbResult.ToString();
} // translate_Helper method

void menuMiscBurnTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Burn to CD");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
string sPathList = String.Join("\r\n", aPaths).Trim() + "\r\n";
App.killFile(App.sTempFile);
Homer.Util.string2File(sPathList, App.sTempFile);
string sExe = Path.Combine(App.sAppDir, "Burn2CD.exe");
//Lbc.Show(Homer.Util.file2String(App.sTempFile), App.sTempFile);
//Process.Start(sExe, App.sTempFile);
Homer.Util.run(sExe + " " + App.sTempFile);
} // menuMiscBurnTagged_Click method

void menuMiscMailBody_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Mail body");
string sPath = Path_Helper();
if (!File.Exists(sPath)) {
App.say("Command requires the current item to be a file!");
return;
}

string sSubject = Path.GetFileNameWithoutExtension(sPath);
sPath = Homer.Util.getShortPath(sPath);
bool bBlankDefault = true;
string sBody = App.frame.convert2Text(sPath, bBlankDefault);
if (sBody.Trim().Length == 0) return;
try {
MapiMail.SendMail(sSubject, sBody, null, null);
}
catch {
return;
}
} // menuMiscMailBody_Click method

void menuMiscMailAttachTagged_Click(object sender, EventArgs e) {
if (abortInZip()) return;
App.say("Mail attach");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;

string sSubject = "";
string sBody = "";
MdiChild mdiChild = App.frame.getActiveChild();
string sPath = Path_Helper();
bool bTags = false;
if (File.Exists(sPath)) {
foreach (DataRowView row in mdiChild.tbl.DefaultView) {
if ((char) row["Tagged"] == '>') {
bTags = true;
break;
}
}

if (!bTags) {
sPath = Homer.Util.getShortPath(sPath);
bool bBlankDefault = true;
sBody = App.frame.convert2Text(sPath, bBlankDefault);
if (sBody.Length > 0) sSubject = Path.GetFileName(sPath) + " (in body and attached)";
}
}

KeyValuePair<string, string>[] aAttachments;
aAttachments = new KeyValuePair<string, string>[aFiles.Length];
for (int i = 0; i < aAttachments.Length; i++) {
string sFile = aFiles[i];
aAttachments[i] = new KeyValuePair<String, String>(Path.GetFileName(sFile), sFile);
}
if (sSubject.Length == 0) sSubject = "(" + Homer.Util.stringPlural("attachment", aAttachments.Length) + ")";
try {
MapiMail.SendMail(sSubject, sBody, null, aAttachments);
//MapiMail.SendMail(sSubject, "", aRecipients, aAttachments);
}
catch {
return;
}
} // menuMiscMailAttachTagged_Click method

void menuMiscBatchMail_Click(object sender, EventArgs e) {
App.say("Batch mail");
string sFilter = "All Files (*.*)|*.*";
string sMail = Lbc.OpenFileDialog("", App.sBatchMail, sFilter, 0);
if (sMail.Length == 0) return;

App.sBatchMail = sMail;
string[] aMail = Homer.Util.file2String(sMail).Trim().Replace("\r\n", "\n").Split('\n');
int iLength = aMail.Length;
if (iLength == 0) {
Lbc.Show("Cannot find message subject", "Error");
return;
}

string sSubject = aMail[0];
string sBody = "";
int iBody = -1;
for (int i = 1; i < iLength; i++) {
sBody = aMail[i].Trim();
if (File.Exists(sBody)) {
iBody = i;
break;
}
}
if (!File.Exists(sBody)) {
Lbc.Show("Cannot find message body file", "Error");
return;
}

string sText = Homer.Util.file2String(sBody);
List<string> list = new List<string>();
for (int i = iBody + 1; i < iLength; i++) {
string sRecipient = aMail[i].Trim();
if (sRecipient.Contains("@")) list.Add(sRecipient);
}
if (list.Count == 0) {
Lbc.Show("Cannot find any recipients", "Error");
return;
}

foreach (string s in list) {
App.say(s);
App.sendMail(App.sUserName, App.sPassword, App.sSenderAddress, App.sOutgoingServer, sSubject, sText, s);
}
App.say("Done!", true);
} // menuMiscBatchMail_Click method

void zip_Helper(string sLabel, bool bDelete) {
zip_Helper(sLabel, bDelete, false);
} // zip_Helper method

void zip_Helper(string sLabel, bool bDelete, bool bZipList) {
if (!bZipList && abortInZip()) return;
App.say(sLabel);
string sOldDir = Directory.GetCurrentDirectory();
string[] aPaths;
string sZip;
string sDir, sFile = "";
string sZipDir = "";

if (bZipList) {
string sFilter = "All Files (*.*)|*.*";
string sLst = Lbc.OpenFileDialog("", App.sZipList, sFilter, 0);
if (sLst.Length == 0) return;

App.sZipList = sLst;
aPaths = Homer.Util.file2String(sLst).Replace("\r\n", "\n").Trim().Split('\n');
sZip = aPaths[0];
sZipDir = Path.GetDirectoryName(sZip);
App.SetDirectory(sZipDir);
}
else {
string[] aDirs, aFiles;
aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;
string sFilter = "Zip archives (*.zip)|*.zip";
sZip = Lbc.SaveFileDialog("", App.sZipText, sFilter, 0, false);
if (sZip == "") return;
App.sZipText = sZip;
}

ZipFile z;
if (File.Exists(sZip)) {
App.say("Updating");
z = new ZipFile(sZip);
}
else z = ZipFile.Create(sZip);
if (App.sUnarchivePassword.Trim().Length > 0) {
App.say("With password");
z.Password = App.sUnarchivePassword;
}
z.BeginUpdate();
int iFileCount = 0;
try {
for (int i = 0; i < aPaths.Length; i++) {
if (bZipList && i == 0) continue;
string sPath = aPaths[i];
if (sPath.Trim().Length == 0) continue;
sPath = Path.GetFullPath(sPath);
sDir = Path.GetDirectoryName(sPath);
//if (bZipList && Directory.Exists(sDir)) App.SetDirectory(sDir);
string sName = Path.GetFileName(sPath);
App.say(sName);
//Lbc.Show(sDir, sPath);
if (Directory.Exists(sPath)) {
sDir = Path.GetDirectoryName(sPath);
//z.AddDirectory(sName);
//z.AddDirectory(sName + @"/");
ReadOnlyCollection<string> list = App.getFiles(sPath, App.sFilterText);
foreach (string s in list) {
if (bZipList) {
sDir = Path.GetDirectoryName(s);
sFile = s;
sDir = Path.GetDirectoryName(s);
if (!Homer.Util.stringEquiv(sZipDir, sDir)) {
if (s.ToLower().StartsWith(sZipDir.ToLower())) sFile = s.Substring(sZipDir.Length + 1);
else sFile = s.Substring(2);
}
}
//else sFile = s.Substring(Path.GetDirectoryName(sPath).Length + 1);
else sFile = s.Substring(Path.GetDirectoryName(sPath).Length).TrimStart('\\');
sName = Path.GetFileName(sFile);
if (!Homer.Util.stringEquiv(Path.GetFullPath(sFile), sZip)) {
App.say(sName);
z.Add(sFile);
iFileCount ++;
}
}
}
else if (File.Exists(sPath)) {
if (bZipList) {
sDir = Path.GetDirectoryName(sPath);
if (!Homer.Util.stringEquiv(sZipDir, sDir)) {
if (sPath.ToLower().StartsWith(sZipDir.ToLower())) sName = sPath.Substring(sZipDir.Length);
else sName = sPath.Substring(2);
}
}
z.Add(sName);
iFileCount ++;
}
else {
App.say(sName +" not found!");
}
}

z.CommitUpdate();
z.Close();
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}

if (testZip(sZip) && bDelete) {
App.say(App.Recycle ? "Recycling" : "Deleting");
foreach (string sItem in aPaths) {
if (Homer.Util.stringEquiv(sItem, sZip)) continue;
App.deleteItem(sItem, App.Recycle);
delete_Helper(sItem);
}
}
if (testZip(sZip)) App.say("Done!", true);
else App.say("Error!");
App.SetDirectory(sOldDir);
//Homer.Util.stringPlural("file", iFileCount);
//Lbc.Show(Homer.Util.stringEquiv(Directory.GetCurrentDirectory(), Path.GetDirectoryName(sZip)));
sDir = Path.GetDirectoryName(sZip);
if (Homer.Util.stringEquiv(Directory.GetCurrentDirectory(), sDir)) {
refresh_Helper(sZip);
goTo_Helper(sZip);
}
else {
App.sGoToText = sDir;
App.sOpenText = sDir;
}
} // zip_Helper method

void menuMiscZipTagged_Click(object sender, EventArgs e) {
zip_Helper("Zip", false);
} // menuMiscZipTagged_Click method

void menuMiscZipTaggedThenDelete_Click(object sender, EventArgs e) {
zip_Helper("Zip then delete", true);
} // menuMiscZipTaggedThenDelete_Click method

void menuMiscZipList_Click(object sender, EventArgs e) {
zip_Helper("Zip List", false, true);
} // menuMiscZipList_Click method

void unarchive_Helper(string sLabel, bool bSubdirs) {
bool bSameName = false;
unarchive_Helper(sLabel, bSubdirs, bSameName);
} // unarchive_Helper method

void unarchive_Helper(string sLabel, bool bSubdirs, bool bSameName) {
App.say(sLabel);
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
aPaths = aFiles;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
if (App.sUnarchivePassword.Trim().Length > 0) {
App.say("With password");
}
string sUnarchiveDir = App.sUnarchiveText;
if (bSameName) {
if (Directory.Exists(sUnarchiveDir)) sUnarchiveDir = Path.GetDirectoryName(sUnarchiveDir);
else sUnarchiveDir = Directory.GetCurrentDirectory();
sUnarchiveDir = Path.Combine(sUnarchiveDir, Path.GetFileNameWithoutExtension(Path_Helper()));
}
else if (!Directory.Exists(sUnarchiveDir)) sUnarchiveDir = Directory.GetCurrentDirectory();

string sDir = Lbc.DirectoryDialog("Input", "Folder", sUnarchiveDir).Trim();
if (sDir == "") return;
if (!bSameName) App.sUnarchiveText = sDir;
App.sGoToText = sDir;
App.sOpenText = sDir;

string sZip = "";
string sSubdir = "";
if (mdiChild.InZip) sZip = mdiChild.Text;
List<ZipEntry> zeList = null;

for (int i = 0; i < aPaths.Length; i++) {
string sPath = aPaths[i];
if (sPath.EndsWith("/") || sPath.EndsWith(@"\")) continue;
string sName = Path.GetFileName(sPath);
App.say(sName);
try {
if (mdiChild.InZip) {
if (bSubdirs) {
sSubdir = Path.Combine(sDir, Path.GetDirectoryName(sPath.Replace("/", @"\")));
if (!Directory.Exists(sSubdir)) FileSystem.CreateDirectory(sSubdir);
}
else sSubdir = sDir;
string sTarget = zipEntry2Dir(sZip, sPath, sSubdir);
}
else {
// if (!sPath.ToLower().EndsWith(".zip")) {
if (true) {
z7Entry2Dir(sPath, "*", sDir, true);
if (i < aPaths.Length - 1) continue;
App.say("Done!", true);
if (Homer.Util.stringEquiv(Path.GetDirectoryName(sDir), Directory.GetCurrentDirectory())) {
refresh_Helper(sDir);
goTo_Helper(sDir);
}
return;
}

zeList = Frame.getZipEntries(sPath);
foreach (ZipEntry ze in zeList) {
sName = ze.Name;
if (sName.EndsWith("/") || sName.EndsWith(@"\")) continue;
sName = Path.GetFileName(sName);
App.say(sName);
try {
if (bSubdirs) {
sSubdir = Path.Combine(sDir, Path.GetDirectoryName(ze.Name.Replace("/", @"\")));
//if (!Directory.Exists(sSubdir)) FileSystem.CreateDirectory(sSubdir);
if (!Directory.Exists(sSubdir)) Directory.CreateDirectory(sSubdir);
}
else sSubdir = sDir;
string sTarget = zipEntry2Dir(sPath, ze.Name, sSubdir);
if (!File.Exists(sTarget) )return;
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
}
}
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
}
App.say("Done!", true);
if (Homer.Util.stringEquiv(Path.GetDirectoryName(sDir), Directory.GetCurrentDirectory())) {
refresh_Helper(sDir);
goTo_Helper(sDir);
}
} // unarchive_Helper method

void menuMiscUnarchiveTagged_Click(object sender, EventArgs e) {
unarchive_Helper("Unarchive", true);
} // menuMiscUnarchiveTagged_Click method

void menuMiscUnarchiveTaggedWithoutSubfolders_Click(object sender, EventArgs e) {
unarchive_Helper("Unarchive without subfolders", false);
} // menuMiscUnarchiveTaggedWithoutSubfolders_Click method

void menuMiscUnarchiveTaggedToSameName_Click(object sender, EventArgs e) {
unarchive_Helper("Unarchive to same name", true, true);
} // menuMiscUnarchiveTaggedToSameName_Click method

void menuMiscUnarchivePassword_Click(object sender, EventArgs e) {
//App.say("Unarchive password");
string sUnarchivePassword = Lbc.InputDialog("Input", "&UnarchivePassword", App.sUnarchivePassword);
if (sUnarchivePassword == "") return;
App.sUnarchivePassword = sUnarchivePassword;
} // menuMiscUnarchivePassword_Click method

public static List<ZipEntry> oldGetZipEntries(string sZip) {
List<ZipEntry> entryList = new List<ZipEntry>();
using ( ZipInputStream s = new ZipInputStream(File.OpenRead(sZip))) {
if (App.sUnarchivePassword.Trim().Length > 0) {
App.say("With password");
s.Password = App.sUnarchivePassword;
}

ZipEntry entry;
while ((entry = s.GetNextEntry()) != null) {
entryList.Add(entry);
} // while
} // using
return entryList;
} // oldGetZipEntries method

// public static string[] GetZ7Entries(string sZip) {
public static List<string> GetZ7Entries(string sZip) {
string[] aLines = MdiChild.getZ7List(sZip);
List<string> lEntries = new List<string>();
for (int i = 0; i < aLines.Length; i++) {
string sLine = aLines[i];
if (!sLine.StartsWith("Path =")) continue;
string sPath = sLine.Substring(6).Trim();
// sPath = sPath.Replace(@"\", "/");
lEntries.Add(sPath);
}
return lEntries;
} // GetZ7Entries

public static List<ZipEntry> getZipEntries(string sZip) {
// public static List<string> getZipEntries(string sZip) {
// return GetZ7Entries(sZip);

List<ZipEntry> entryList = new List<ZipEntry>();
ZipFile z = new ZipFile(sZip);
if (App.sUnarchivePassword.Trim().Length > 0) {
App.say("With password");
z.Password = App.sUnarchivePassword;
}

foreach (ZipEntry ze in z) entryList.Add(ze);
return entryList;
} // getZipEntries method

public bool testZ7(string sPath) {
string sExe = Path.Combine(App.sAppDir, "7z.exe");
string sParams = "t " + Homer.Util.stringQuote(sPath) + " -p" + App.sUnarchivePassword;
string sText = Homer.Util.getProgramOutput(sExe, sParams);
if (App.frame != null && App.frame.TopLevelControl.Handle != (IntPtr) Homer.Util.getForegroundWindow()) Homer.Util.forceWindow(App.frame.TopLevelControl.Handle);
sText = sText.Replace("\r\n", "\n");
string sMatch = "\nEverything is Ok\n";
return sText.IndexOf(sMatch) >= 0;
} // testZ7 method

public bool testZip(string sPath) {
if (!File.Exists(sPath)) return false;
string sExt = Path.GetExtension(sPath);
if (sExt.StartsWith(".")) sExt = sExt.Substring(1).ToLower();
if ("|com|doc|docx|exe|msi|pdf|ppt|pptx|xls|xlsx|".IndexOf("|" + sExt + "|") >= 0) return false;

return testZ7(sPath);
bool bResult = false;
try {
using (ZipFile z = new ZipFile(sPath)) {
if (App.sUnarchivePassword.Trim().Length > 0)  z.Password = App.sUnarchivePassword;
bResult = z.TestArchive(true);
}
}
catch {
bResult = false;
}
return bResult;
} // testZip method

public string z7Entry2Dir(string sZip, string sPath, string sDir, bool bSubfolders) {
string sExe = Path.Combine(App.sAppDir, "7z.exe");
string s = " e ";
if (bSubfolders) s = " x ";
// string sCommand = Homer.Util.stringQuote(sExe) + s+ Homer.Util.stringQuote(sZip) + " " + Homer.Util.stringQuote(sPath) + " " + Homer.Util.stringQuote("-o" + sDir) + " -aoa -p" + App.sUnarchivePassword;
string sParams = s + Homer.Util.stringQuote(sZip) + " " + Homer.Util.stringQuote(sPath) + " " + Homer.Util.stringQuote("-o" + sDir) + " -aoa -p" + App.sUnarchivePassword;
// Homer.Util.runHideWait(sCommand);
string sText = Homer.Util.getProgramOutput(sExe, sParams);
if (App.frame != null && App.frame.TopLevelControl.Handle != (IntPtr) Homer.Util.getForegroundWindow()) Homer.Util.forceWindow(App.frame.TopLevelControl.Handle);
sText = sText.Replace("\r\n", "\n");
string sMatch = "\nEverything is Ok\n";
bool bError = sText.IndexOf(sMatch) < 0;
if (bError) App.say("Error!");
string sReturn = Path.Combine(sDir, Path.GetFileName(sPath));
return sReturn;
} // z7Entry2Dir method

public string zipEntry2Dir(string sZip, string sPath, string sDir) {
bool bSubfolders = false;
return zipEntry2Dir(sZip, sPath, sDir, bSubfolders);
} // zipEntry2Dir method

public string zipEntry2Dir(string sZip, string sPath, string sDir, bool bSubfolders) {
// if (!sZip.ToLower().EndsWith(".zip")) return z7Entry2Dir(sZip, sPath, sDir, bSubfolders);
return z7Entry2Dir(sZip, sPath, sDir, bSubfolders);

string sResult = "";
try {
ZipFile z = new ZipFile(sZip);
if (App.sUnarchivePassword.Trim().Length > 0) {
//App.say("With password");
z.Password = App.sUnarchivePassword;
}
ZipEntry entry = z.GetEntry(sPath);
if (entry == null) Lbc.Show("no entry", sPath);
Stream inStream = z.GetInputStream(entry);
string sFile = Path.GetFileName(sPath.Replace("/", @"\\"));
sResult = Path.Combine(sDir, sFile);
//if (File.Exists(sResult)) File.Delete(sResult);
if (File.Exists(sResult)) App.deleteFile(sResult, App.Recycle);
//StreamWriter outStream = new StreamWriter(sResult);
//long l = inStream.Length;
long l = entry.Size;
//l = 4096;
Byte[] data = new Byte[l];
int size = inStream.Read(data, 0, (int) l);
//Lbc.Show(size, l);
//Homer.Util.string2File(new ASCIIEncoding().GetString(data, 0, size), sResult);
//Homer.Util.string2File(new UnicodeEncoding().GetString(data, 0, size), sResult);
FileSystem.WriteAllBytes(sResult, data, false);
File.SetLastWriteTime(sResult, entry.DateTime);
//outStream.Write(bytes);
//outStream.Write(inStream.Read(bytes, 0, l));
//outStream.Close();
inStream.Close();
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
sResult = "";
}
return sResult;
} // zipEntry2Dir method

void menuMiscUnarchiveTest_Click(object sender, EventArgs e) {
App.say("Unarchive test");
string sPath = Path_Helper();
string sName = Path.GetFileName(sPath);
if (!File.Exists(sPath)) {
App.say(sName +" not found!");
return;
}
if (App.sUnarchivePassword.Trim().Length > 0) {
App.say("With password");
}
bool bResult = testZip(sPath);
if (bResult) App.say("Passed!");
else App.say("Failed!");
} // menuMiscUnarchiveTest_Click method

void menuMiscCommandPrompt_Click(object sender, EventArgs e) {
App.say("Command prompt");
string sCommand = "cmd.exe";
string sParams = "/k";
try {
Process.Start(sCommand, sParams);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
} // menuMiscCommandPrompt_Click method

void menuMiscExplorerDir_Click(object sender, EventArgs e) {
App.say("Explorer directory");
string sCommand = "Explorer.exe";
string sParams = Directory.GetCurrentDirectory();
try {
Process.Start(sCommand, sParams);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
} // menuMiscExplorerDir_Click method

void menuMiscFTPPut_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

App.say("FTP put");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
string sUserName = "";
string sPassword = "";
string sURL = "";

//if (App.sUserName == "") {
if (App.sUserName != null) {
string[] aFields = {"&Address", "&LogInUserName", "&Password"};
string[] aValues = {App.sFTPText, App.sUserName, App.sPassword};
ArrayList sResultList = Lbc.FieldDialog("Fields", aFields, aValues);
if (sResultList.Count == 0) return;
sURL = (string) sResultList[0];
sUserName = (string) sResultList[1];
sPassword = (string) sResultList[2];
}
else {
sUserName = App.sUserName;
sPassword = App.sPassword;
sURL = Lbc.InputDialog("Input", "&Address", App.sFTPText, "FtpAddress");
if (sURL == "") return;
}

if (!sURL.Contains("://")) {
sURL = "ftp://" + sURL;
if (!sURL.EndsWith("/")) sURL += "/";
}
App.sFTPText = sURL;
App.sUserName = sUserName;
App.sPassword = sPassword;
string sBaseURL = sURL;

foreach (string sPath in aPaths) {
string sName = Path.GetFileName(sPath);
App.say(sName);
if (Directory.Exists(sPath)) {
App.say("Skipping " + sName);
continue;
}
else if (File.Exists(sPath)) {
if (sBaseURL.EndsWith("/")) sURL = sBaseURL + Path.GetFileName(sPath);
else sURL = sBaseURL + "/" + Path.GetFileName(sPath);

try {
//Lbc.Show(sFile, sURL);
//Lbc.Show(sUserName, sPassword);
App.uploadFile(sPath, sURL, sUserName, sPassword);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
}
else {
App.say("Cannot find " + sName);
continue;
}
}

App.say("Done!", true);
} // menuMiscFTPPut_Click method

void menuMiscGetFTP_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

App.say("Get FTP");
string sUserName = "";
string sPassword = "";
string sURL = "";
//if (App.sUserName == "") {
if (App.sUserName != null) {
string[] aFields = {"&Address", "&LogInUserName", "&Password"};
string[] aValues = {App.sFTPText, App.sUserName, App.sPassword};
ArrayList sResultList = Lbc.FieldDialog("Fields", aFields, aValues);
if (sResultList.Count == 0) return;
sURL = (string) sResultList[0];
sUserName = (string) sResultList[1];
sPassword = (string) sResultList[2];
}
else {
sUserName = App.sUserName;
sPassword = App.sPassword;
sURL = Lbc.InputDialog("Input", "&Address", App.sFTPText, "FtpAddress");
if (sURL == "") return;
}

if (!sURL.Contains("://")) {
sURL = "ftp://" + sURL;
if (!sURL.EndsWith("/")) sURL += "/";
}
App.sFTPText = sURL;
App.sUserName = sUserName;
App.sPassword = sPassword;
string sBaseURL = sURL;

string[] aFiles = null;
App.say("Please wait");
try {
aFiles = Homer.Util.getFtpDir(sUserName, sPassword, sBaseURL);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
if (aFiles.Length == 0) {
App.say("No files!");
return;
}

ArrayList listFiles = Lbc.MultiListDialog("Multi Pick", "", aFiles, true, 0, new int[] {});
if (listFiles.Count == 0) return;

foreach (string sName in listFiles) {
App.say(sName);
if (sBaseURL.EndsWith("/")) sURL = sBaseURL + sName;
else sURL = sBaseURL + "/" + sName;
string sFile = sName;
try {
App.deleteFile(sFile, App.Recycle);
//Lbc.Show(sURL, sFile);
App.downloadFile(sURL, sFile, sUserName, sPassword);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
string s = Path.Combine(mdiChild.Text, sName);
refresh_Helper(s);
}

App.say("Done!", true);
} // menuMiscGetFTP_Click method

void mediaDownload_Helper(string sUrl) {
// yt-dlp fetches video and audio from the many sites it knows, into the folder
// being looked at.  It is given the whole job: it picks the best streams and
// joins them, which is what it is for and what a hand-rolled download cannot do.
//
// Sound only is offered because it is half of why anyone does this: a talk or a
// lecture is worth keeping as audio and nothing else.  That path needs ffmpeg,
// which yt-dlp calls itself, and FileDir puts the folder holding ffmpeg on the
// path it hands over so a copy beside FileDir is found without anything being
// installed.
string sTitle = "Web Download";
string sExe = Homer.Media.findInstalled("yt-dlp");
if (sExe.Length == 0) {
Lbc.Show("yt-dlp was not found, so media cannot be fetched from a page.\n\nInstall the media tools from the FileDir installer, or put yt-dlp.exe in the FileDir folder.", sTitle);
return;
}
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;
string sDir = Path_Helper();
if (File.Exists(sDir)) sDir = Path.GetDirectoryName(sDir);
if (!Directory.Exists(sDir)) {
Lbc.Show("The current folder could not be worked out, so there is nowhere to save.", sTitle);
return;
}

string[] aWhat = {"Video and sound, best quality", "Sound only, as an MP3 file"};
string sWhat = App.readValue(App.sIniFile, "Data", "MediaDownloadWhat", aWhat[0]);
int iWhat = Array.IndexOf(aWhat, sWhat);
if (iWhat < 0) iWhat = 0;
bool bSort = false;
sWhat = Dialog.Pick("Pick", aWhat, bSort, iWhat);
if (sWhat.Length == 0) return;
App.writeValue(App.sIniFile, "Data", "MediaDownloadWhat", sWhat);

StringBuilder sbArgs = new StringBuilder();
sbArgs.Append("--no-playlist --newline --paths ");
sbArgs.Append(Homer.Util.stringQuote(sDir));
if (sWhat == aWhat[1]) sbArgs.Append(" --extract-audio --audio-format mp3");
string sFfmpeg = Homer.Media.ffmpegProgram();
if (sFfmpeg.Length > 0) {
sbArgs.Append(" --ffmpeg-location ");
sbArgs.Append(Homer.Util.stringQuote(Path.GetDirectoryName(sFfmpeg)));
}
sbArgs.Append(" ");
sbArgs.Append(Homer.Util.stringQuote(sUrl));

App.say("Downloading. This can take a while.");
string sOut, sErr;
int iCode = Homer.Media.run(sExe, sbArgs.ToString(), out sOut, out sErr);
if (iCode == 0) {
App.say("Done!", true);
refreshFolder_Helper(mdiChild);
return;
}
// yt-dlp says plainly what went wrong -- an unsupported site, a private video,
// a missing ffmpeg -- so its own words are shown rather than a code.
string sMessage = sErr.Trim();
if (sMessage.Length == 0) sMessage = sOut.Trim();
if (sMessage.Length == 0) sMessage = "yt-dlp returned " + iCode + ".";
Lbc.Show("Nothing was downloaded.\n\n" + sMessage, sTitle);
} // mediaDownload_Helper method

void menuMiscWebDownload_Click(object sender, EventArgs e) {
if (abortInZip()) return;
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

App.say("Web download");
string sUrl = Homer.Util.getUrl();
if (sUrl.Length == 0) sUrl = App.sWebText;
else App.sWebText = sUrl;

sUrl = Lbc.InputDialog("Input", "Address", sUrl, "WebAddress");
if (sUrl.Length == 0) return;

// A page of links and a page holding a video want opposite treatment, and no
// scrape of the second finds anything: the video is assembled from pieces this
// command cannot see.  When yt-dlp is here, the choice is offered rather than
// guessed at, because a wrong guess costs either a large download or an empty
// list.  The answer is remembered, so the usual case is one keypress.
if (Homer.Media.findInstalled("yt-dlp").Length > 0) {
string[] aHow = {"Download media from this page (yt-dlp)", "List the files linked from this page"};
string sHow = App.readValue(App.sIniFile, "Data", "WebDownloadHow", aHow[0]);
int iHow = Array.IndexOf(aHow, sHow);
if (iHow < 0) iHow = 0;
bool bSortHow = false;
sHow = Dialog.Pick("Pick", aHow, bSortHow, iHow);
if (sHow.Length == 0) return;
App.writeValue(App.sIniFile, "Data", "WebDownloadHow", sHow);
if (sHow == aHow[0]) {
mediaDownload_Helper(sUrl);
return;
}
}

App.say("Please wait");

List<string[]> listLinks = App.getLinks(sUrl);
List<string> listFiles = new List<string>();
string sRef;
// Work out the name each link would be saved as, the way durl.py does: the
// server-recommended name from Content-Disposition, and an extension inferred
// from the MIME type when the URL carries none.  That is what makes the
// "Extensions" filter below work for links like "site.com/get?id=42", which
// used to show no extension and so could never be picked by type.  A link whose
// URL already ends in a name with an extension costs no network call.
Dictionary<string, string> dNames = new Dictionary<string, string>();
foreach (string[] aLink in listLinks) {
sRef = aLink[0];
string sFile = Homer.Web.suggestedName(sRef);
if (sFile.Length == 0) sFile = Homer.Util.getFileFromUri(sRef);
dNames[sRef] = sFile;
listFiles.Add(sFile);
}

string[] aFiles = listFiles.ToArray();
string sText = Homer.Util.getExtensions(aFiles);
string sResult = Lbc.InputDialog("Input", "Extensions", sText, "DownloadExtensions").Replace(".", "").Trim().ToLower();
if (sResult.Length == 0) return;

string[] aResults = Homer.Util.getFilesWithExtensions(aFiles, sResult);

listFiles.Clear();
List<string> listItems = new List<string>();
List<string> listRefs = new List<string>();
foreach (string[] aLink in listLinks) {
sRef = aLink[0];
string sFile = dNames.ContainsKey(sRef) ? dNames[sRef] : Homer.Util.getFileFromUri(sRef);
string sExt = Path.GetExtension(sFile).TrimStart('.').ToLower();
if (Array.IndexOf(aResults, sFile) == -1) continue;

sText = aLink[1];
if (String.IsNullOrEmpty(sText)) sText = sRef;

listItems.Add(sText + " = " + sFile);
listFiles.Add(sFile);
listRefs.Add(sRef);
}

if (listItems.Count == 0) {
App.say("No items!");
return;
}

string[] aValues = listItems.ToArray();
aResults = Lbc.MultiCheckDialog("Pick Files", aValues, new int[] {}, false, 0);
if (aResults.Length == 0) return;

string sDir = Directory.GetCurrentDirectory();
App.say("Downloading");
foreach (string s in aResults) {
int i = listItems.IndexOf(s);
string sFile = listFiles[i];
sRef = listRefs[i];
App.say(sFile);
try {
// Homer.Web assigns the name the server recommends (Content-Disposition, then
// the MIME type for a missing extension), sanitizes it, and makes it unique in
// the target folder -- the durl.py rules.  The name worked out above is passed
// as the suggestion, so it is used when the server offers nothing better.
string sSaved = Homer.Web.download(sRef, sDir, sFile);
if (sSaved.Length == 0) throw new IOException("Download failed: " + sRef);
sFile = sSaved;
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
refresh_Helper(sFile);
}
App.say("Done!", true);

/*
string sUserName = "";
string sPassword = "";
string sExe = Path.Combine(App.sAppDir, "WebGet.exe");
string sTmp = Path.Combine(App.sAppDir, "WebGet.tmp");

string s = Homer.Util.getUrl();
if (s != "") App.sWebText = s;

string sURL = Lbc.InputDialog("Input", "&Address", App.sWebText, "WebAddress");
if (sURL.Length > 0) App.sWebText = sURL;
if (sURL.Trim() == "") return;

if (!sURL.Contains("://")) {
sURL = "http://" + sURL;
//if (!sURL.EndsWith("/")) sURL += "/";
}
App.sWebText = sURL;
string sBaseURL = sURL;

string sFile = "";
string[] aLines = null;
string[] aURLs = null;
string[] aLinks = null;
string[] aFiles = null;
App.say("Please wait");
//string sExe = Path.Combine(App.sAppDir, "WebGet.exe");
try {
Homer.Util.runHideWait(sExe + " " + sURL);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
//string sTmp = Path.Combine(App.sAppDir, "WebGet.tmp");
aLines = Homer.Util.file2String(sTmp).Replace("\r\n", "\n").Trim().Split('\n');
StringBuilder sbURLs  = new StringBuilder();
StringBuilder sbLinks = new StringBuilder();
StringBuilder sbFiles  = new StringBuilder();
foreach (string sLine in aLines) {
string[] aParts = sLine.Split('\t');
if (aParts.Length < 3) {
//Lbc.Show(sLine);
continue;
}
sbURLs.Append(aParts[1] + "\n");
sbLinks.Append(aParts[0] + " = " + aParts[2] + "\n");
sbFiles.Append(aParts[2] + "\n");
}
aURLs = sbURLs.ToString().Trim().Split('\n');
aLinks = sbLinks.ToString().Trim().Split('\n');
aFiles = sbFiles.ToString().Trim().Split('\n');
if (aFiles.Length == 1 && aFiles[0] == "") aFiles = new string[] {};
if (aFiles.Length == 0) {
App.say("No files!");
return;
}

List<int> listFiles = Lbc.MultiListDialog("Multi Pick", aLinks, false);
if (listFiles.Count == 0) return;

string sDir = mdiChild.Text;
foreach (int i in listFiles) {
string sName = aFiles[i];
App.say(sName);
sURL = aURLs[i];
sFile = Path.Combine(sDir, sName);
try {
App.deleteFile(sFile, App.Recycle);
App.downloadFile(sURL, sFile, sUserName, sPassword);
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}
refresh_Helper(sFile);
}

App.say("Done!", true);
*/
} // menuMiscWebDownload_Click method

void menuMiscEvaluate_Click(object sender, EventArgs e) {
App.say("Evaluate");
string sExp = App.sEvaluateText;
sExp = Lbc.InputDialog("Input", "Expression", sExp, "Evaluate");
if (sExp == "") return;
App.sEvaluateText = sExp;
MdiChild mdiChild = App.frame.getActiveChild();
string sResult = App.runScript(sExp, mdiChild, (mdiChild != null ? mdiChild.tbl : null));
if (string.IsNullOrEmpty(sResult)) return;
Clipboard.SetText(sResult);
App.say(sResult);
} // menuMiscEvaluate_Click method

void menuMiscConvertUnits_Click(object sender, EventArgs e) {
//App.say("Convert Units");
App.say("Calculate");
int iItem = App.iCalculateItem;
string sBody = Homer.Util.file2String(Path.Combine(App.sAppDir, "Convert.txt")).Trim().Replace("\r\n", "\n");
string[] aBody = sBody.Split('\n');
//Array.Sort(aBody);
string[] aKeys = new string[aBody.Length];
string[] aValues = new string[aBody.Length];
string[] aItems = new string[aBody.Length];
for (int i = 0; i < aBody.Length; i++) {
string[] a = aBody[i].Split('=');
aKeys[i] = a[0];
aValues[i] = a[1];
aItems[i] = a[0].Replace("_TO_", " to ").Replace("_", " ");
}
//string sItem = Lbc.ListDialog("Pick", "", aItems, false, iItem);
string sTitle = "Pick (" + aItems.Length + ") and Input";
//List<string> list = Lbc.ListInputDialog("Pick and Input", "&Units", aItems, "&Value", App.sCalculateText, 
List<string> list = Lbc.ListInputDialog(sTitle, "&Units", aItems, "&Value", App.sCalculateText, false, iItem);
//if (sItem == "") return;
if (list.Count == 0) return;
//string sValue = Lbc.InputDialog("Input", "&Value", App.sCalculateText).Trim();
string sItem = list[0];
iItem = Array.IndexOf(aItems, sItem);
App.iCalculateItem = iItem;
string sValue = list[1];
if (sValue == "") return;
App.sCalculateText = sValue;
//string sResult = Double.Parse(aValues[iItem]).ToString();
//sValue += " " + aValues[iItem];
sValue += " * 1.0 " + aValues[iItem];
string sEval = App.runScript(sValue, App.frame.getActiveChild(), null);
if (string.IsNullOrEmpty(sEval) || sEval.StartsWith("ERROR")) return;
double f = Double.Parse(sEval);
string sResult = Math.Round(f, 2).ToString();
Clipboard.SetText(sResult);
App.say(sResult);
} // menuMiscConvertUnits_Click method

void menuMiscStartTimer_Click(object sender, EventArgs e) {
//App.playWav(Path.Combine(App.sAppDir, "BuzzerLong.wav"));
//App.playWav(Path.Combine(App.sAppDir, "whistle.wav"));
//App.playSystemSound(SystemSounds.Exclamation);
//App.playSystemSound(SystemSounds.Question);
//App.playSystemSound(SystemSounds.Hand);
//System.Media.SystemSounds.Hand.Play();

TimeSpan elapsed;
int iInterval = 60;
DateTime dtStop = new DateTime();

if (App.timer == null) {
App.say("Start timer");

string[] aFields = {"&Announcement Interval", "&Stop Time"};
String[] aValues = {App.sTimerInterval, App.sTimerStop};
ArrayList list = Lbc.FieldDialog("Fields", aFields, aValues);
if (list.Count == 0) return;

string sInterval = list[0].ToString().Trim();
if (sInterval == "") sInterval = "0";
App.sTimerInterval = sInterval;
string sStop = list[1].ToString().Trim();
if (sStop == "") sStop = "0";
App.sTimerStop = sStop;

try {
if (sInterval != "0") iInterval = Int32.Parse(sInterval);
if (sStop != "0") {
dtStop = DateTime.Parse(sStop);
if (dtStop <= DateTime.Now) {
App.say("Stop time has already occurred!");
return;
}
}
}
catch (Exception ex) {
Lbc.Show(ex.Message, "Error");
return;
}

App.timer = new System.Timers.Timer();
App.timer.Enabled = false;
App.timer.Elapsed += delegate(object o, System.Timers.ElapsedEventArgs args) {
//dtStop = DateTime.Parse(App.sTimerStop);
if (App.sTimerStop != "0" && args.SignalTime >= dtStop) {
string sWav = Path.Combine(App.sAppDir, "chimes.wav");
for (int i = 0; i < 5; i++) App.playWav(sWav);
menuMiscStopTimer.clickOrDescribe();
return;
}
elapsed = App.tsElapsed + (args.SignalTime - App.dtTimerStart);
if (App.sTimerInterval !="" && App.sTimerInterval != "0") timer_Helper(elapsed);};
//}

//if (!App.timer.Enabled && App.tsElapsed.Ticks == 0) {
App.timer.Interval = iInterval * 1000;
App.dtTimerStart = DateTime.Now;
App.timer.Start();
App.dtTimerStart -= TimeSpan.FromMilliseconds(500);
App.say("Go");
}
else if (App.timer.Enabled) {
App.timer.Enabled = false;
App.tsElapsed += DateTime.Now - App.dtTimerStart;
App.say("Pause timer");
timer_Helper(App.tsElapsed);
}
else {
App.timer.Enabled = true;
App.dtTimerStart = DateTime.Now;
App.say("Resume timer");
}
} // menuMiscStartTimer_Click method

void timer_Helper(TimeSpan elapsed) {
if (elapsed.Days > 0) App.say(Homer.Util.stringPlural("day", elapsed.Days), true);
if (elapsed.Hours > 0) App.say(Homer.Util.stringPlural("hour", elapsed.Hours), true);
if (elapsed.Minutes > 0) App.say(Homer.Util.stringPlural("minute", elapsed.Minutes), true);
if (elapsed.Seconds > 0) App.say(Homer.Util.stringPlural("second", elapsed.Seconds), true);
//if (elapsed.Milliseconds > 0) App.say(Homer.Util.stringPlural("millisecond", elapsed.Milliseconds), true);
} // timer_Helper method

void menuMiscStopTimer_Click(object sender, EventArgs e) {
if (App.timer == null) {
App.say("No timer!");
return;
}

App.stopWav();
TimeSpan elapsed;
if (App.timer.Enabled) {
elapsed = App.tsElapsed + (DateTime.Now - App.dtTimerStart);
}
else elapsed = App.tsElapsed;

App.say("Stop timer");
App.timer.Stop();
timer_Helper(elapsed);
App.tsElapsed = new TimeSpan();
App.timer.Dispose();
App.timer = null;
} // menuMiscStopTimer_Click method

void menuMiscConfigureTimer_Click(object sender, EventArgs e) {
App.say("Configure timer");
string[] aFields = {"&Announcement Interval", "&Stop Time"};
String[] aValues = {App.sTimerInterval, App.sTimerStop};
ArrayList list = Lbc.FieldDialog("Fields", aFields, aValues);
if (list.Count == 0) return;

string sInterval = (string) list[0];
string sStop = (string) list[1];

int iInterval = Int32.Parse(sInterval);
App.sTimerInterval = iInterval.ToString();
DateTime dtStop = DateTime.Parse(sStop);
App.sTimerStop = dtStop.ToString();
} // menuMiscConfigureTimer_Click method

void MenuMiscPlayFolder_Click(object sender, EventArgs e) {
// Play something now: whatever is on the clipboard if that is playable,
// otherwise the tagged files, otherwise everything playable in this folder.
//
// THE CLIPBOARD FIRST, AND WHY.
//
// mpv plays anything it can reach, and with yt-dlp beside it that includes a
// YouTube address. So a play list of web addresses is as playable as a folder
// of MP3 files, and the clipboard is where such a list arrives -- copied from a
// mail message, a web page, or a file open in EdSharp.
//
// WHAT MPV DOES AND DOES NOT DO WITH THE CLIPBOARD.
//
// mpv reads the clipboard perfectly well at run time: clipboard/text is a
// read and write property, native clipboard support is on by default, and
// Windows has its own backend. Inside a running mpv, Control+V appends the
// file or address in the clipboard to the play list, and plays it at once if
// nothing is playing.
//
// But that is ONE entry, and there is no --playlist=clipboard and no
// clipboard:// protocol -- --playlist takes a file name. A forty-track list
// cannot be handed over that way.
//
// So FileDir writes the clipboard to a temporary play list and hands mpv that.
// It could pipe the same text to --playlist=- instead, and a file is chosen
// deliberately: it can be replayed, saved, or opened as a virtual folder
// afterwards, and standard input can be handed over only once.
//
// Control+V still works inside the player, so more can be queued while
// listening. That is worth knowing and is in the guide.
//
// The clipboard is only used when it LOOKS playable: a web address, or a line
// naming a file that exists. Ordinary copied prose is ignored and the folder is
// played instead, so the command never does something surprising with whatever
// happened to be copied last.
if (abortInZip()) return;
string sTitle = "Play Media";
App.say(sTitle);
if (Homer.Media.mpvProgram().Length == 0) {
Lbc.Show("mpv was not found, so there is nothing to play with.\r\n\r\n"
+ "Run installMpv.cmd in the FileDir folder, or install FileDir again and tick the mpv box. "
+ "Play List still writes a play list without it.\r\n\r\n"
+ "If mpv IS installed, this is where FileDir looked. Send these lines and the search can be widened:\r\n\r\n"
+ Homer.Media.searchLog(), sTitle);
return;
}
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

List<string> lsPlay = new List<string>();
string sWhere = "";

// 1. The clipboard, when it holds addresses or file names.
foreach (string sLine in clipboardPlaylist_Helper()) lsPlay.Add(sLine);
if (lsPlay.Count > 0) sWhere = "from the clipboard";

// 2. The tagged files.
if (lsPlay.Count == 0) {
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length > 1) {
foreach (string sPath in aPaths)
if (File.Exists(sPath) && isPlayable_Helper(sPath)) lsPlay.Add(sPath);
if (lsPlay.Count > 0) sWhere = "tagged";
}
}

// 3. Everything playable in the window, in the order shown. Sort by date and
// this plays in date order, which is how a recording session or a downloaded
// series wants to be heard.
if (lsPlay.Count == 0) {
DataView view = mdiChild.tbl.DefaultView;
for (int i = 0; i < view.Count; i++) {
string sPath = (string) view[i]["Path"];
if (File.Exists(sPath) && isPlayable_Helper(sPath)) lsPlay.Add(sPath);
}
if (lsPlay.Count > 0) sWhere = "in this folder";
}

if (lsPlay.Count == 0) {
App.say("Nothing to play. Copy a play list or some addresses, or move to a folder holding sound or video.", true);
return;
}

// A temporary list rather than one in the folder: this is playing, not filing,
// and leaving an .m3u behind every time would be litter.  It keeps its name so
// the same thing can be replayed, and Open Virtual Folder can reach it.
string sTemp = Path.Combine(Path.GetTempPath(), "FileDir_play.m3u");
try {
File.WriteAllText(sTemp, "#EXTM3U\r\n" + String.Join("\r\n", lsPlay.ToArray()), new UTF8Encoding(false));
}
catch (Exception ex) {
Lbc.Show("The play list could not be written.\n\n" + ex.Message, sTitle);
return;
}
Homer.Log.write("Play Media: " + lsPlay.Count + " entries " + sWhere + ".");

// Warned before the silence rather than after it. If nothing in the list names
// a media file, every entry is a page the player has to fetch and examine
// first, which takes seconds each and fails outright on a site yt-dlp does not
// know. A hundred and eighty-five of those is a long wait with nothing to hear,
// and that is what happened.
int iPages = 0;
int iAddresses = 0;
foreach (string sEntry in lsPlay) {
if (sEntry.StartsWith("#")) continue;
if (sEntry.IndexOf("://", StringComparison.Ordinal) < 0) continue;
iAddresses++;
if (!isDirectMedia_Helper(sEntry)) iPages++;
}
if (iAddresses > 0 && iPages == iAddresses) {
string[] aButtons = {"&Try anyway", "Cancel"};
string sWarn = Homer.Util.stringPlural("address", iAddresses)
+ ", and not one of them names a media file. They look like web pages.\r\n\r\n"
+ "The player must fetch and examine each page before anything can play. That takes "
+ "several seconds each, and does not work at all for a site it does not recognise, so "
+ "this could be a long wait with nothing to hear.\r\n\r\n"
+ "Addresses that end in .mp3 or .m4a play at once. A podcast's own feed usually holds "
+ "those, where its web pages do not.";
if (Dialog.Choose(sTitle, sWarn, aButtons, 1) != "&Try anyway") return;
}
App.say(Homer.Util.stringPlural("item", lsPlay.Count) + " " + sWhere);
playMedia_Helper(sTemp);
} // MenuMiscPlayFolder_Click method

List<string> linksToPlay_Helper(string sText) {
// Media links found in a piece of text, with their titles.
//
// This was inside the clipboard reader, and is now its own so that Play List
// on a single document can use exactly the same rules. One place decides what
// counts as a link, so the two commands can never disagree about a file.
List<string> lsKept = new List<string>();
List<string> lsSeen = new List<string>();
if (sText == null || sText.Length == 0) return lsKept;
sText = sText.TrimStart('\uFEFF');
int iFound = 0;

foreach (Match link in Regex.Matches(sText, @"\[([^\]]{1,120})\]\(\s*(https?://[^\s\)]+)\s*\)")) {
if (addAddress_Helper(lsKept, lsSeen, link.Groups[2].Value, link.Groups[1].Value)) iFound++;
}
foreach (Match link in Regex.Matches(sText,
@"<a\b[^>]*href\s*=\s*[""']\s*(https?://[^""'\s>]+)\s*[""'][^>]*>(.*?)</a>",
RegexOptions.IgnoreCase | RegexOptions.Singleline)) {
string sTitle = Regex.Replace(link.Groups[2].Value, "<[^>]+>", "").Trim();
if (addAddress_Helper(lsKept, lsSeen, link.Groups[1].Value, sTitle)) iFound++;
}
foreach (Match link in Regex.Matches(sText, @"HYPERLINK\s+""(https?://[^""]+)""", RegexOptions.IgnoreCase)) {
if (addAddress_Helper(lsKept, lsSeen, link.Groups[1].Value, "")) iFound++;
}
foreach (string sRaw in sText.Replace("\r\n", "\n").Split('\n')) {
string sLine = sRaw.Trim();
if (sLine.Length == 0 || sLine.StartsWith("#")) continue;
foreach (Match search in Regex.Matches(sLine, @"ytdl://\S.*$")) {
if (addAddress_Helper(lsKept, lsSeen, search.Value.Trim(), "")) iFound++;
}
if (sLine.StartsWith("ytdl://", StringComparison.OrdinalIgnoreCase)) continue;
foreach (Match bare in Regex.Matches(sLine, @"(?:https?|rtmp|rtsp|mms|srt)://[^\s""'<>\)\]]+")) {
if (addAddress_Helper(lsKept, lsSeen, bare.Value, "")) iFound++;
}
if (sLine.IndexOf("://", StringComparison.Ordinal) < 0) {
bool bFile = false;
try { bFile = File.Exists(sLine); }
catch (Exception) { }
if (bFile && addAddress_Helper(lsKept, lsSeen, sLine, "")) iFound++;
}
}
if (iFound < 2) return new List<string>();

// A direct media address beats a page address, since a page has to be fetched
// and examined before anything can play.
List<string> lsDirect = new List<string>();
int iDirect = 0;
for (int i = 0; i < lsKept.Count; i++) {
string sEntry = lsKept[i];
if (sEntry.StartsWith("#")) continue;
if (!isDirectMedia_Helper(sEntry)) continue;
if (i > 0 && lsKept[i - 1].StartsWith("#EXTINF")) lsDirect.Add(lsKept[i - 1]);
lsDirect.Add(sEntry);
iDirect++;
}
if (iDirect >= 2) return lsDirect;
return lsKept;
} // linksToPlay_Helper method

List<string> clipboardPlaylist_Helper() {
// The clipboard as a list of things to play.
//
// The rules for what counts as a link live in linksToPlay_Helper, which Play
// List uses on a document as well. One place decides, so the two commands can
// never disagree about the same file.
//
// The clipboard is read as HTML and rich text before plain text, because a
// page copied from a browser keeps its addresses in the markup and only the
// visible words in the text: asking for text alone throws the addresses away.
return linksToPlay_Helper(clipboardText_Helper());
} // clipboardPlaylist_Helper method

bool isDirectMedia_Helper(string sAddress) {
// A ytdl search is not a page: yt-dlp resolves it to a media stream, so it
// plays at once and must not raise the warning meant for lists of web pages.
if (sAddress.StartsWith("ytdl://", StringComparison.OrdinalIgnoreCase)) return true;
// Whether an address names a media file outright, rather than a page that has
// one somewhere on it. Judged by the extension before any query string, since a
// podcast address usually carries tracking parameters after a question mark.
string sPath = sAddress;
int iQuery = sPath.IndexOfAny(new char[] {'?', '#'});
if (iQuery >= 0) sPath = sPath.Substring(0, iQuery);
string sExt = "";
try { sExt = Path.GetExtension(sPath).ToLower(); }
catch (Exception) { return false; }
foreach (string sMedia in new string[] {
".mp3", ".m4a", ".aac", ".ogg", ".oga", ".opus", ".flac", ".wav", ".wma",
".mp4", ".m4v", ".mkv", ".webm", ".mov", ".avi", ".flv", ".m3u8"})
if (sExt == sMedia) return true;
return false;
} // isDirectMedia_Helper method

bool addAddress_Helper(List<string> lsKept, List<string> lsSeen, string sAddress, string sTitle) {
// Add one address, with its title when there is one, unless it has been seen.
//
// A saved web page names the same address in several places, and a Markdown
// document often repeats a link in a list and again in prose. Playing a show
// three times because its address appeared three times is not what anybody
// meant.
if (sAddress == null) return false;
sAddress = sAddress.Trim().TrimEnd('.', ',', ';');
if (sAddress.Length == 0) return false;
foreach (string sHad in lsSeen) if (Homer.Util.stringEquiv(sHad, sAddress)) return false;
lsSeen.Add(sAddress);
if (sTitle != null && sTitle.Trim().Length > 0) {
// #EXTINF is where a play list keeps its titles, and mpv reads them.
lsKept.Add("#EXTINF:-1," + sTitle.Trim().Replace("\r", " ").Replace("\n", " "));
}
lsKept.Add(sAddress);
return true;
} // addAddress_Helper method

string clipboardText_Helper() {
// The clipboard as text, whatever format it arrived in.
//
// A page copied from a browser is on the clipboard as HTML as well as text,
// and a passage from Word as rich text. Asking only for plain text throws away
// the addresses, because the visible text of a link is its title and the
// address lives in the markup. So the richer formats are asked for first, and
// the links are read out of the markup itself.
try {
DataObject data = Clipboard.GetDataObject() as DataObject;
if (data == null) return "";
StringBuilder sb = new StringBuilder();
foreach (string sFormat in new string[] {"HTML Format", DataFormats.Rtf}) {
if (!data.GetDataPresent(sFormat)) continue;
object oValue = data.GetData(sFormat);
if (oValue is string) sb.Append((string) oValue).Append("\r\n");
}
if (data.GetDataPresent(DataFormats.UnicodeText)) {
object oText = data.GetData(DataFormats.UnicodeText);
if (oText is string) sb.Append((string) oText);
}
return sb.ToString();
}
catch (Exception) {
try { return Clipboard.ContainsText() ? Clipboard.GetText() : ""; }
catch (Exception) { return ""; }
}
} // clipboardText_Helper method


bool isPlayable_Helper(string sPath) {
// Sound or moving pictures. Asked of Homer.Convert, which already keeps the
// lists that Output Type uses, so a format added there is playable here too
// without a second list to keep in step.
string sCategory = Homer.Convert.categoryOf(sPath);
return sCategory == "audio" || sCategory == "video";
} // isPlayable_Helper method

void menuMiscPlayList_Click(object sender, EventArgs e) {
// Make a play list of the tagged files, and, when mpv is installed, offer to
// play it there and then.
//
// WHY THIS COMMAND RATHER THAN A NEW ONE.
//
// A play list is already the thing that says "these files, in this order" --
// which is exactly what a player needs. Hanging playback off it means no new
// key to learn, no second way of choosing files, and the list is still on disk
// afterwards for any other player. FileDir has few keys left, and this one was
// already the right place.
//
// Without mpv the command behaves exactly as it always has: the list is
// written and nothing else is said. A person who does not play media from the
// file list sees no change at all.
if (abortInZip()) return;
App.say("Play list");
string[] aDirs, aFiles;
string[] aPaths = list_Helper(out aDirs, out aFiles, 1);
if (aPaths.Length == 0) return;

// WITH NOTHING TAGGED, THE COMMAND READS THE ITEM YOU ARE ON.
//
// The same idea as Control+C in EdSharp taking the current line when there is
// no selection: with no selection, act on what is under the cursor, and act on
// it sensibly. A single file is not a set worth writing a list about, so the
// question becomes "what does this one file mean here", and there are three
// useful answers.
//
//   A play list        -- play it. Nothing to write; it already exists.
//   A sound or a video -- play it.
//   Anything readable  -- look inside for links to media and play those, which
//                         is exactly what Alt+Shift+L does with the clipboard.
//                         A directory of podcasts is the case this serves: put
//                         the cursor on it, press the key, and it plays.
//
// Several files tagged still means what it always meant: write a play list of
// them. That is the case where a list is the point.
if (aPaths.Length == 1 && File.Exists(aPaths[0])) {
string sOne = aPaths[0];
string sOneExt = Path.GetExtension(sOne).ToLower();
bool bList = sOneExt == ".m3u" || sOneExt == ".m3u8" || sOneExt == ".pls";
bool bMedia = isPlayable_Helper(sOne);
bool bReadable = Homer.Convert.readableAsText(sOne);
if (bList || bMedia || bReadable) {
if (Homer.Media.mpvProgram().Length == 0) {
Lbc.Show("mpv is not installed, so there is nothing to play with.\r\n\r\n"
+ "Run installMpv.cmd in the FileDir folder, or install FileDir again and tick the mpv box.\r\n\r\n"
+ "Tag two or more files and press this key again to write a play list without playing it.", "Play List");
return;
}
// A play list or a media file goes straight to the player.
if (bList || bMedia) {
playMedia_Helper(sOne);
return;
}
// Otherwise the file is read and its links are gathered, the same way the
// clipboard is read. A document with no links in it is not a failure worth a
// dialog; it simply has nothing to play.
App.say("Looking for media links in " + Path.GetFileName(sOne));
// THE FILE IS READ RAW, NOT AS PLAIN TEXT, and that distinction is the whole
// bug this replaced.
//
// toPlainText hands a web page to Pandoc with "-t plain", which is right for
// reading aloud and fatal here: plain text has no links in it, so Pandoc
// DELETES every address and leaves only the words. A directory of podcasts came
// back as a list of show names with nothing to play. Markdown directories still
// worked, because those are read straight off the disk -- which is exactly why
// this failed on .htm and not on .md.
//
// So the bytes are read as they are, and the links are found in the markup, the
// same way the clipboard is read when a page is copied from a browser.
//
// Named sText, not sBody: further down this same method sBody holds the play
// list being written, and C# will not have one name mean two things in nested
// scopes even when the two never overlap in time.
string sText = "";
try {
sText = Homer.Util.file2String(sOne);
}
catch (Exception ex) {
Homer.Log.write("Could not read " + sOne + ": " + ex.Message);
}
// A format that is not text at all -- a Word document, a PDF -- has no markup
// to scan, so it goes through the extractor after all. Its links are gone, but
// an address written out in the body of the text still survives.
if (sText.Length == 0 || !Homer.Convert.readableAsText(sOne)
|| sText.IndexOf("://", StringComparison.Ordinal) < 0) {
string sReadError;
string sExtracted = Homer.Convert.toPlainText(sOne, out sReadError);
if (sExtracted.Length > 0) sText = sExtracted;
}
List<string> lsFound = linksToPlay_Helper(sText);
if (lsFound.Count == 0) {
// LOGGED, not only spoken. The runs that found nothing left no trace at all,
// so a report that some files worked and some did not could not be told apart
// from a report that the command was never pressed.
Homer.Log.write("Play List: no media links in " + sOne
+ " (" + sText.Length + " characters read).");
App.say("No media links in " + Path.GetFileName(sOne) + ".", true);
return;
}
string sTempList = Path.Combine(Path.GetTempPath(), "FileDir_play.m3u");
try {
File.WriteAllText(sTempList, "#EXTM3U\r\n" + String.Join("\r\n", lsFound.ToArray()), new UTF8Encoding(false));
}
catch (Exception ex) {
Lbc.Show("The play list could not be written.\r\n\r\n" + ex.Message, "Play List");
return;
}
int iLinks = 0;
foreach (string sEntry in lsFound) if (!sEntry.StartsWith("#")) iLinks++;
Homer.Log.write("Play List: " + iLinks + " links from " + sOne);
App.say(Homer.Util.stringPlural("link", iLinks) + " found");
playMedia_Helper(sTempList);
return;
}
}

string sFile = "PlayList.m3u";
string sDir = Directory.GetCurrentDirectory();
sFile = Path.Combine(sDir, sFile);
sFile = Homer.Util.getUniqueName(sFile);
string sFilter = "Play Lists (*.m3u)|*.m3u";
sFile = Lbc.SaveFileDialog("", sFile, sFilter, 0, true);
if (sFile == "") return;

string sBody = String.Join("\r\n", aPaths);
Homer.Util.string2File(sBody, sFile);
App.say("Done!", true);

if (Homer.Util.stringEquiv(sDir, Path.GetDirectoryName(sFile))) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild != null) {
refresh_Helper(sFile);
goTo_Helper(sFile);
}
}

// The list is written and kept; then it plays, without asking.
if (Homer.Media.mpvProgram().Length > 0) playMedia_Helper(sFile);
} // menuMiscPlayList_Click method

void playMedia_Helper(string sPlayList) {
// Hand a play list to mpv and let it play.
//
// NO QUESTIONS. This used to ask whether to play sound and picture or sound
// alone, and remembered the answer. That was a menu in front of a command whose
// whole point is to start playing: the default experience is the whole one, and
// mpv's own keys handle the rest -- there is a key in the player for muting the
// video if that is ever wanted.
//
// mpv is left to run on its own rather than waited for: it is a player, the
// person will be listening to it, and FileDir should not sit frozen meanwhile.
// Its own keys work in its window -- space to pause, arrows to seek, angle
// brackets for the previous and next item, q to stop.
string sTitle = "Play Media";
string sMpv = Homer.Media.mpvProgram();
if (sMpv.Length == 0) return;

StringBuilder sbArgs = new StringBuilder();

// mpv reaches YouTube and the rest through yt-dlp, which it looks for on the
// path. FileDir may hold the only copy -- beside its own program, where mpv
// would never look -- so it is named outright when it is there. Options come
// before the file, as mpv's own synopsis has them.
string sYtDlp = Homer.Media.findInstalled("yt-dlp");
if (sYtDlp.Length > 0) {
sbArgs.Append("--script-opts=ytdl_hook-ytdl_path=");
sbArgs.Append(Homer.Util.stringQuote(sYtDlp));
sbArgs.Append(" ");
}

// A WINDOW STRAIGHT AWAY.
//
// Without this, mpv shows nothing until it has decoded something. A list that
// is slow to start -- or never starts -- leaves a player running with no window
// at all: not on the task bar, not reachable by Alt+Tab, and no way to press q
// at it. That is exactly what happened with a list of podcast page addresses,
// each of which had to be fetched before anything could play.
//
// This is the one option added back after the play list was reduced to a plain
// file name, and it is added because a player nobody can find is worse than no
// player. It changes nothing about what plays.
sbArgs.Append("--force-window=immediate ");

// THE PLAY LIST AS A PLAIN ARGUMENT, which is how a person would type it:
//
//   mpv.exe "C:\...\temp.m3u"
//
// It used to be handed over as --playlist=<file>, and that is where this went
// wrong. mpv opens a play list perfectly well when it is simply named -- its
// own manual says so, "You can play playlists directly, without this option" --
// and --playlist applies security rules of its own that a list of web addresses
// can fall foul of. Typing the plain form at the Run box played the same file
// that FileDir could not, which settled it.
//
// Nothing else is passed. Every option is one more thing that can refuse the
// list, and the default experience is the whole one.
sbArgs.Append(Homer.Util.stringQuote(sPlayList));
Homer.Log.write("Playing " + sPlayList + " with " + sMpv);
Homer.Log.write("  Arguments: " + sbArgs.ToString());
try {
System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
if (Homer.Media.needsShell(sMpv)) {
// A .cmd or .bat wrapper cannot be started directly, so it goes through the
// command interpreter. A real player is preferred over a wrapper wherever
// both exist, because a wrapper that does not forward its arguments
// swallows the play list without a word -- which is exactly what happened.
info.FileName = "cmd.exe";
info.Arguments = "/c " + Homer.Util.stringQuote(sMpv) + " " + sbArgs.ToString();
info.CreateNoWindow = true;
}
else {
info.FileName = sMpv;
info.Arguments = sbArgs.ToString();
}
info.UseShellExecute = false;
info.WorkingDirectory = Path.GetDirectoryName(sPlayList);
System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);
// A player that stops within a second or two has not played anything: mpv
// refusing a list exits at once. Rather than say "Playing" and leave a person
// waiting for sound that is not coming, the exit is noticed and said.
if (process != null && process.WaitForExit(2000)) {
Homer.Log.write("The player exited immediately, code " + process.ExitCode + ".");
App.say("The player closed straight away. Nothing played.", true);
Lbc.Show("mpv started and closed again without playing.\r\n\r\n"
+ "This is what it was given:\r\n\r\n"
+ sMpv + "\r\n" + sbArgs.ToString() + "\r\n\r\n"
+ "The play list is still at " + sPlayList + ", so the same line can be tried by hand.", sTitle);
return;
}
App.say("Playing. Press q in the player window to stop.", true);
}
catch (Exception ex) {
Lbc.Show("mpv could not be started.\r\n\r\n" + ex.Message + "\r\n\r\nIt was found at " + sMpv, sTitle);
}
} // playMedia_Helper method

void MenuMiscNetworkConnections_Click(object sender, EventArgs e) {
string sTitle = "Network Connections";
string[] aButtons = new string[] {"&Connect", "&Disconnect", "&Restore"};
string sResult = Lbc.ButtonDialog(sTitle, "", aButtons, 0);

NetworkDrive oNetDrive  = new NetworkDrive();
switch (sResult) {
case "&Connect" :
oNetDrive.ShowConnectDialog(null);
break;
case "&Disconnect" :
oNetDrive.ShowDisconnectDialog(null);
break;
case "&Restore" :
oNetDrive.RestoreDrives();
break;
}
oNetDrive = null;
} // menuNetworkConnections_Click method

void menuMiscVolumeFormat_Click(object sender, EventArgs e) {
App.say("Volume format");
string sName = "";
string sNameList = "";
//string[] aNames = Environment.GetLogicalDrives();
DriveInfo[] allDrives = DriveInfo.GetDrives();
//foreach (string s in aNames) {
foreach (DriveInfo d in allDrives){
sName = d.Name.Substring(0, 1);
sName += "\t" + d.DriveType;
if (d.IsReady && d.VolumeLabel != "") sName += "\t" + d.VolumeLabel;
sNameList += sName + "\n";
}
string[] aNames = sNameList.Trim().Split('\n');
sName = Lbc.ListDialog("Pick", "", aNames, true, 0);
if (sName.Length == 0) return;
string sDir = sName.Substring(0, 1) + ":";
Homer.Util.formatDrive(App.frame.Handle, sDir);
} // menuMiscVolumeFormat_Click method

void MenuMiscWindowsControlPanel_Click(object sender, EventArgs e) {
App.say("WindowsControl Panel");
string sName = "Control.exe";
string sFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), sName);
if (!File.Exists(sFile)) sFile = Homer.Util.findOnPath(sName);
if (!File.Exists(sFile)) sFile = sName;
//Process.Start(sFile);
Homer.Util.run(sFile);
} // menuWindowsControlPanel_Click method

void MenuMiscIterateProcesses_Click(object sender, EventArgs e) {
int iPid;
string s, sTitle, sButton;
string[] aItems;
object[] aPids, aResult;
Process[] aProcesses;
Process process = null;

string[] aButtons = {"&Activate", "&Terminate"};
int iIndex = 0;
bool bLoop = true;
while (bLoop) {
aProcesses = Process.GetProcesses();
aItems = new string[aProcesses.Length];
aPids = new object[aProcesses.Length];
for (int i = 0; i < aProcesses.Length; i++) {
aItems[i] = aProcesses[i].ProcessName;
s = aProcesses[i].MainWindowTitle;
if (!Homer.Util.stringEquiv(aItems[i], s)) aItems[i] += "\t" + s;
if (!aProcesses[i].Responding) aItems[i] += "\t" + "Not Responding";
}
Array.Sort(aItems, aProcesses);

for (int i = 0; i < aProcesses.Length; i++) aPids[i] = aProcesses[i].Id;

if (iIndex >= aItems.Length) iIndex = 0;
sTitle = "Iterate Processes";
aResult = Lbc.ListButtonDialog(sTitle, aPids, aItems, aButtons, true, iIndex);
if (aResult.Length == 0) return;

iPid = (int) aResult[0];
iIndex = Array.IndexOf(aPids, iPid);
s = aItems[iIndex].Split('\t')[0];
sButton = (string) aResult[1];

try {
process = Process.GetProcessById(iPid);
}
catch {
process = null;
}

switch (sButton) {
case "&Activate" :
if (process == null) Lbc.Show("Can no longer find " + s + "process", "Alert");
else {
Microsoft.VisualBasic.Interaction.AppActivate(iPid);
bLoop = false;
}
break;
case "&Terminate" :
if (process == null) break;
process.CloseMainWindow();
System.Threading.Thread.Sleep(500);
try {
process = Process.GetProcessById(iPid);
if (Lbc.ConfirmDialog("Confirm", s + " did not close by request.  Try to force it?", "Y") == "Y") process.Kill();
System.Threading.Thread.Sleep(500);
 break;
}
catch {
}
break;
default :
bLoop = false;
break;
}
}
process.Dispose();
process = null;
} // menuIterateProcesses_Click method

void MenuMiscInquireDifferences_Click(object sender, EventArgs e) {
App.say("Inquire Differences");
String sDivider = "\n----------\n\f\n";
string sEndOfDocument = "\n----------\nEnd of Document\n";

string sSourceDir = Directory.GetCurrentDirectory();
string sTargetDir = App.sGoToText;
if (sTargetDir == null || sTargetDir.Length == 0) sTargetDir = App.sOpenText;
sTargetDir = Lbc.DirectoryDialog("Input", "Folder", sTargetDir).Trim();
if (sTargetDir.Length == 0) return;

string[] aSourceDirs, aSourceFiles;
string[] aSourcePaths = list_Helper(out aSourceDirs, out aSourceFiles, 0);
string[] aTargetFiles = Directory.GetFiles(sTargetDir);

List<string[]> lCommon = new List<string[]>();
List<string[]> lMissing = new List<string[]>();
List<string[]> lAdditional = new List<string[]>();

foreach (string sSourceFile in aSourceFiles) {
string sName = Path.GetFileName(sSourceFile);
string sTargetFile = Path.Combine(sTargetDir, sName);
if (File.Exists(sTargetFile)) {
FileInfo fiSource = new FileInfo(sSourceFile);
long iSourceSize = fiSource.Length;
DateTime dSourceDate = Directory.GetLastWriteTime(sSourceFile);
FileInfo fiTarget = new FileInfo(sTargetFile);
long iTargetSize = fiTarget.Length;
DateTime dTargetDate = Directory.GetLastWriteTime(sTargetFile);

string sCompare = compare_Helper(dSourceDate, iSourceSize, dTargetDate, iTargetSize);
lCommon.Add(new string[] {sName, sCompare});
}
else lMissing.Add(new string[] {sName, ""});
}

foreach (string sTargetFile in aTargetFiles) {
string sName = Path.GetFileName(sTargetFile);
string sSourceFile = Path.Combine(sSourceDir, sName);
if (!File.Exists(sSourceFile)) lAdditional.Add(new string[] {sName, ""});
}

string sContents = "Folder Comparison" + "\n";
sContents += "Source: " + sSourceDir + "\n";
sContents += "Target: " + sTargetDir + "\n\n";
sContents += "Contents" + "\n\n";

string sTopic = "Common Target Files";
sContents += sTopic + "\n";
string sBody = sTopic + "\n\n";

foreach (string[] a in lCommon) {
string sName = a[0];
string sCompare = a[1];
sBody += sCompare + " " + sName + "\n";
}
sBody += sDivider;

sTopic = "Missing Target Files";
sContents += sTopic +"\n";
sBody += sTopic + "\n\n";
foreach (string[] a in lMissing) {
string sName = a[0];
string sCompare = a[1];
sBody += sName + "\n";
}
sBody += sDivider;

sTopic = "Additional Target Files";
sContents += sTopic +"\n";
sBody += sTopic + "\n\n";
foreach (string[] a in lAdditional) {
string sName = a[0];
string sCompare = a[1];
sBody += sName + "\n";
}
sBody += sEndOfDocument;

sBody = sContents + sDivider + sBody;
string sReportFile = Path.Combine(sSourceDir, "Report.txt");
sReportFile = Homer.Util.getUniqueName(sReportFile);
string sFilter = "Text files (*.txt)|*.txt";
sReportFile = Lbc.SaveFileDialog("", sReportFile, sFilter, 0, true);
if (sReportFile.Length == 0) return;

Homer.Util.string2File(sBody, sReportFile);

if (File.Exists(sReportFile)) {
App.say("Done!");
refresh_Helper(sReportFile);
goTo_Helper(sReportFile);
}
else App.say("Error!");
} // menuInquireDifferences_Click method

void menuWindowArrangeIcons_Click(object sender, EventArgs e) {
App.say("Arrange icons");
this.LayoutMdi(MdiLayout.ArrangeIcons);
} // menuWindowArrangeIcons_Click method

void menuWindowCascade_Click(object sender, EventArgs e) {
App.say("Cascade");
this.LayoutMdi(MdiLayout.Cascade);
} // menuWindowCascade_Click method

void menuWindowTileHorizontal_Click(object sender, EventArgs e) {
App.say("Tile horizontal");
this.LayoutMdi(MdiLayout.TileHorizontal);
} // menuWindowTileHorizontal_Click method

void menuWindowTileVertical_Click(object sender, EventArgs e) {
App.say("Tile vertical");
this.LayoutMdi(MdiLayout.TileVertical);
} // menuWindowTileVertical_Click method

void menuWindowDrive_Click(object sender, EventArgs e) {
HomerToolStripMenuItem menuItem = (HomerToolStripMenuItem) sender;
string sText = menuItem.Text;
string sDrive = sText.Substring(7, 1);
App.say("Drive " + sDrive);
DriveInfo drive = new DriveInfo(sDrive);
if (!drive.IsReady) {
App.say("Drive not ready!");
return;
}

App.SetDrive(sDrive);
string sDir = Directory.GetCurrentDirectory();
activate_Helper(sDir);
} // menuWindowDrive_Click method

void MenuHelpAbout_Click(object sender, EventArgs e) {
// The version comes from BuildVersion, generated by BuildFileDir.cmd from
// version.txt, so the About box, the installer, and the release tag can never
// disagree.  It used to be typed here, and said "5.0 beta" and the wrong
// licence for fourteen releases.  The date is the build date of this assembly.
string sText = "FileDir " + BuildVersion.Version + "\n";
sText += File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("d MMMM yyyy") + "\n\n";
sText += "Copyright 2006 - 2026 by Jamal Mazrui\nMIT License\n\n";
sText += ".NET Framework " + RuntimeEnvironment.GetSystemVersion() + "\n\n";
sText += App.getPortableExecutableKind();
MessageBox.Show(sText, "About");
} // menuHelpAbout_Click method

void MenuHelpDocumentation_Click(object sender, EventArgs e) {
App.say("Documentation");
string sFile = Path.Combine(App.sAppDir, "FileDir.htm");
Process.Start(sFile);
} // menuHelpDocumentation_Click method

void MenuHelpChangeHistory_Click(object sender, EventArgs e) {
App.say("Change history");
// History.htm, generated from History.md by the build.  This opened History.txt,
// which the documentation set replaced and the installer now deletes on upgrade.
string sFile = Path.Combine(App.sAppDir, "History.htm");
Process.Start(sFile);
} // menuHelpChangeHistory_Click method

void MenuHelpKeyDescriber_Click(object sender, EventArgs e) {
if (App.bKeyDescriber) {
App.say("No Key Describer");
App.bKeyDescriber = false;
}
else {
App.say("Key Describer On");
App.bKeyDescriber = true;
}
} // menuHelpKeyDescriber_Click method

void MenuHelpHotKeys_Click(object sender, EventArgs e) {
App.say("Hot keys");
// Hotkeys.htm, generated from Hotkeys.md, which makeKeyMap.ps1 generates from
// Hotkeys.ini.  This opened HotKeys.txt, a hand-kept file that fell out of step
// with the program and is no longer shipped.
string sFile = Path.Combine(App.sAppDir, "Hotkeys.htm");
Process.Start(sFile);
} // menuHelpHotKeys_Click method


void MenuHelpAlternateMenu_Click(object sender, EventArgs e) {
App.say("Alternate Menu");
int iChoice = -1;
List<HomerToolStripMenuItem> items = new List<HomerToolStripMenuItem>();
string sItemList = "";
StringBuilder sb = new StringBuilder();
foreach (HomerToolStripMenuItem menu in menuMain.Items) {
//if (menu == menuWindow) continue;
//foreach (HomerToolStripMenuItem item in menu.DropDownItems) {
foreach (object o in menu.DropDownItems) {
HomerToolStripMenuItem item = o as HomerToolStripMenuItem;
if (item == null) continue;
if (item == menuHelpAlternateMenu) continue;
if (item.IsMdiWindowListEntry) continue;
// string sText = item.Text.Replace("&", "") + "\t" + item.ShortcutKeyDisplayString;
// if ("1234567890".Contains(sText.Substring(0, 1))) continue;
//sItemList += sText + "\n";
string[] aSummary = item.getKeySummary();
string sText = aSummary[0] + " = " + aSummary[1] + ", " + aSummary[2];
sb.Append(sText + "\n");
items.Add(item);
//sItemList += item.Text.Replace("&", "") + "\n";
}
}
sItemList = sb.ToString();
string[] aItems = sItemList.Trim().Split('\n');
string sItem = Lbc.ListDialog("Pick", "", aItems, true, 0);
if (sItem == "") return;
foreach (HomerToolStripMenuItem item in items) {
//if (sItem == item.Text.Replace("&", "")) {
// if (sItem == item.Text.Replace("&", "") + "\t" + item.ShortcutKeyDisplayString) {
string[] aSummary = item.getKeySummary();
string sText = aSummary[0] + " = " + aSummary[1] + ", " + aSummary[2];
if (sItem == sText) {
iChoice = items.IndexOf(item);
break;
}
}
items[iChoice].clickOrDescribe();
} // menuHelpAlternateMenu_Click method

void MenuHelpCopyLog_Click(object sender, EventArgs e) {
// This session's log path to the clipboard, in two formats at once, the way
// EdSharp's Copy Log does and on the same key: a file drop list, so Control+V
// in a new mail message attaches the log file itself, and plain text, so any
// program that only reads clipboard text gets the path. Both matter, and a
// DataObject carries the two together.
//
// This exists so that "send me the log" is one keystroke rather than a hunt
// through a profile folder nobody should have to know the shape of.
string sTitle = "Copy Log";
if (Homer.Log.sFile.Length == 0 || !File.Exists(Homer.Log.sFile)) {
App.say("No log for this session!");
return;
}
try {
DataObject dataLog = new DataObject();
System.Collections.Specialized.StringCollection colFiles = new System.Collections.Specialized.StringCollection();
colFiles.Add(Homer.Log.sFile);
dataLog.SetFileDropList(colFiles);
dataLog.SetText(Homer.Log.sFile);
Clipboard.SetDataObject(dataLog, true);
App.say("Log path copied");
}
catch (Exception ex) {
Lbc.Show(ex.Message, sTitle);
}
} // MenuHelpCopyLog_Click method

void menuHelpElevateVersion_Click(object sender, EventArgs e) {
// Check GitHub Releases for a newer FileDir, then download and run its
// installer. This replaces the old AppStamp.ini / Url2File mechanism with the
// GitHub Releases approach used by the sibling DbDo and EdSharp projects. All
// network work goes through Homer.Web (User-Agent + modern TLS). FileDir is not
// closed here: the Inno Setup installer detects the running copy and offers to
// close it before proceeding. The asset name must match the installer's
// OutputBaseFilename exactly (GitHub download URLs are case-sensitive).
string sOwnerRepo = "JamalMazrui/FileDir";
string sReleasesUrl = "https://github.com/" + sOwnerRepo + "/releases/latest";
string sName = "FileDir_setup.exe";
App.say("Checking for updates");
string sTag = App.fetchLatestReleaseTag(sOwnerRepo);
if (sTag.Length == 0) {
Lbc.Show("Could not check for updates right now.\nPlease check your internet connection and try again.\nYou can also download the latest installer from\n" + sReleasesUrl, "Elevate Version");
return;
}
string sLocal = App.VersionString;
string sLatest = sTag.TrimStart('v', 'V').Trim();
int iCompare = App.compareVersions(sLatest, sLocal);
string sDefault = "N";
string sMsg;
if (iCompare > 0) {
sMsg = "A newer FileDir is available.\nInstalled: " + sLocal + "\nAvailable: " + sLatest + "\n\nDownload and run the new installer now?";
sDefault = "Y";
}
else if (iCompare == 0) sMsg = "FileDir is up to date (version " + sLocal + ").\nLatest release: " + sLatest + "\n\nReinstall the latest release anyway?";
else sMsg = "FileDir is running a newer version (" + sLocal + ") than the latest public release (" + sLatest + ").\n\nReinstall the public release anyway?";
if (Lbc.ConfirmDialog("Elevate Version", sMsg, sDefault) != "Y") return;
App.say("Downloading installer");
string sUrl = sReleasesUrl + "/download/" + sName;
string sFile = Homer.Web.download(sUrl, Path.GetTempPath(), sName);
if (sFile.Length == 0) {
Lbc.Show("The download did not complete.\nYou can download the installer manually from\n" + sReleasesUrl, "Elevate Version");
return;
}
App.say("Starting installer");
try {
ProcessStartInfo processStartInfo = new ProcessStartInfo();
processStartInfo.FileName = sFile;
processStartInfo.UseShellExecute = true;
Process.Start(processStartInfo);
}
catch (Exception ex) {
Lbc.Show("The installer downloaded but could not be started.\n" + ex.Message + "\n\nThe file is here:\n" + sFile, "Elevate Version");
}
} // menuHelpElevateVersion_Click method

void OldMenuHelpContextMenu_Click(object sender, EventArgs e) {
MdiChild mdiChild = App.frame.getActiveChild();
if (mdiChild == null) return;

string sPath = Path_Helper();
if (!File.Exists(sPath)) {
//App.say("Command requires the current item to be a file!");
return;
}

App.say("Context Menu");
Process process = new Process();
process.StartInfo.FileName = sPath;
string[] aVerbs = process.StartInfo.Verbs;
Array.Resize(ref aVerbs, aVerbs.Length + 1);
aVerbs[aVerbs.Length - 1] = "OpenWith";
//Array.Resize(ref aVerbs, aVerbs.Length + 1);
//aVerbs[aVerbs.Length - 1] = "Properties";
if (aVerbs.Length == 0) {
App.say("No actions available for this file type!");
return;
}

string sChoice = Lbc.ListDialog("Pick", "", aVerbs, true, 0);
if (sChoice == "") return;
process.StartInfo.Verb = sChoice;
if (mdiChild.InZip) {
App.say("With temp file");
string sZip = mdiChild.Text;
string sDir = Path.GetTempFileName();
App.lsTempFile.Add(sDir);
sDir = Path.GetDirectoryName(sDir);
string sTarget = zipEntry2Dir(sZip, sPath, sDir);
App.lsTempFile.Add(sTarget);
process.StartInfo.FileName = sTarget;
sPath = sTarget;
}
//if (sChoice == "OpenWith") Lbc.OpenWith(sPath);
Clipboard.SetText(sChoice);
if (sChoice.Replace("&", "").StartsWith("Open With")) Homer.Util.run("rundll32.exe shell32.dll,OpenAs_RunDLL " + sPath + "");
//else if (sChoice == "Properties") Homer.Util.properties(sPath);
else process.Start();
//}
} // menuHelpContextMenu_Click method

protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
return App.frame.processCmdKey_Helper(ref msg, keyData);
} // ProcessCmdKey method

public bool processCmdKey_Helper(ref Message msg, Keys keyData) {
switch (keyData) {
case Keys.Enter :
case Keys.Shift | Keys.Enter :
//if (File.Exists(Path_Helper()) || (App.Recycle && (keyData == Keys.Enter)) || (!App.Recycle && (keyData == (Keys.Shift | Keys.Enter)))) App.frame.menuFileOpenItem.clickOrDescribe();
// if (testZip(Path_Helper()) && (keyData == (Keys.Enter | Keys.Shift))) App.frame.menuFileGoToItem.clickOrDescribe();
bool bZip = testZip(Path_Helper());
if (bZip && (keyData == (Keys.Enter | Keys.Shift))) item_Helper("Run", false, bZip);
// else if (((char) Field_Helper("Type") == ' ') || (App.Recycle && (keyData == Keys.Enter)) || (!App.Recycle && (keyData == (Keys.Shift | Keys.Enter)))) App.frame.menuFileOpenItem.clickOrDescribe();
else if (((char) Field_Helper("Type") == ' ') || (App.Recycle && (keyData == Keys.Enter)) || (!App.Recycle && (keyData == (Keys.Shift | Keys.Enter)))) item_Helper("Open", false, bZip);
//if (((char) Field_Helper("Type") == ' ' && !testZip(Path_Helper())) || (App.Recycle && (keyData == Keys.Enter)) || (!App.Recycle && (keyData == (Keys.Shift | Keys.Enter)))) App.frame.menuFileOpenItem.clickOrDescribe();
// else App.frame.menuFileGoToItem.clickOrDescribe();
else item_Helper("Go to", true, bZip);
return true;
case Keys.Alt | Keys.Enter :
App.frame.menuFileProperties.clickOrDescribe();
return true;
case Keys.Back :
case Keys.Shift | Keys.Back :
if ((App.Recycle && (keyData == Keys.Back)) || (!App.Recycle && (keyData == (Keys.Shift | Keys.Back)))) App.frame.menuFileOpenParentFolder.clickOrDescribe();
else App.frame.menuFileGoToParentFolder.clickOrDescribe();
return true;
case Keys.Oemcomma :
parentFolder_Helper("Come up level", true);
return true;
case Keys.OemBackslash :
case Keys.Shift | Keys.OemBackslash :
case Keys.OemPipe :
case Keys.Shift | Keys.OemPipe :
if ((App.Recycle && (keyData == Keys.OemBackslash)) || (!App.Recycle && (keyData == (Keys.Shift | Keys.OemBackslash)))) App.frame.menuFileOpenRootFolder.clickOrDescribe();
else if ((App.Recycle && (keyData == Keys.OemPipe)) || (!App.Recycle && (keyData == (Keys.Shift | Keys.OemPipe)))) App.frame.menuFileOpenRootFolder.clickOrDescribe();
else App.frame.menuFileGoToRootFolder.clickOrDescribe();
return true;
case Keys.Space :
App.frame.menuEditToggleTag.clickOrDescribe();
return true;
case Keys.Shift | Keys.Space :
App.frame.menuQuerySelected.clickOrDescribe();
return true;
case Keys.Shift | Keys.Down :
case Keys.Shift | Keys.OemPeriod :
App.frame.menuEditTagAndNext.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.Down :
case Keys.Shift | Keys.Oemcomma :
App.frame.menuEditUntagAndNext.clickOrDescribe();
return true;
case Keys.Shift | Keys.Up :
App.frame.menuEditTagAndPrevious.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.Up :
App.frame.menuEditUntagAndPrevious.clickOrDescribe();
return true;
case Keys.Shift | Keys.End :
App.frame.menuEditTagToBottom.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.End :
App.frame.menuEditUntagToBottom.clickOrDescribe();
return true;
case Keys.Shift | Keys.Home :
App.frame.menuEditTagToTop.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.Home :
App.frame.menuEditUntagToTop.clickOrDescribe();
return true;
case Keys.Alt | Keys.OemPeriod :
App.frame.menuEditTagAllFiles.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.OemPeriod :
App.frame.menuEditTagDuplicateFiles.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.Oemcomma :
App.frame.menuEditTagSimilarFiles.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.K :
App.frame.menuEditReorderNames.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.J :
App.frame.menuEditFindDuplicates.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.OemPeriod :
App.frame.menuEditTagWithRegularExpression.clickOrDescribe();
return true;
case Keys.Alt | Keys.Oemcomma :
App.frame.menuEditUntagAllButCurrent.clickOrDescribe();
return true;
case Keys.Shift | Keys.Clear :
case Keys.OemSemicolon :
App.frame.menuEditTag.clickOrDescribe();
return true;
case Keys.Alt | Keys.OemSemicolon :
App.frame.menuQueryNow.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.Clear :
case Keys.OemQuestion :
App.frame.menuEditUntag.clickOrDescribe();
return true;
case Keys.Delete :
App.frame.menuEditDeleteTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.Delete :
App.frame.menuEditDeleteTaggedWithoutRecycle.clickOrDescribe();
return true;
case Keys.Alt | Keys.A :
App.frame.menuMiscAlphaOrder.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.A :
App.frame.menuMiscReverseAlphaOrder.clickOrDescribe();
return true;
case Keys.Shift | Keys.A :
App.frame.menuMiscAppendTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.D2 :
App.frame.menuMiscConvertEncodingTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.A :
App.frame.menuEditTagAll.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.A :
App.frame.menuEditUntagAll.clickOrDescribe();
return true;
case Keys.Control | Keys.C :
App.frame.menuEditCopyToClipboardTagged.clickOrDescribe();
return true;
case Keys.Alt | Keys.C :
//App.frame.menuEditCopyPathTagged.clickOrDescribe();
App.frame.menuEditCopyAppendToClipboardTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.C :
App.frame.menuEditCopyNameTagged.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.C :
App.frame.menuMiscConfigurationOptions.clickOrDescribe();
return true;
case Keys.F8 :
App.frame.menuEditStartTagOrUntag.clickOrDescribe();
return true;
case Keys.Shift | Keys.F8 :
App.frame.menuEditCompleteTag.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.F8 :
App.frame.menuEditCompleteUntag.clickOrDescribe();
return true;
case Keys.Shift | Keys.C :
App.frame.menuEditCopyTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.B :
App.frame.menuMiscOpenRecycleBin.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.B :
App.frame.menuMiscBatchMail.clickOrDescribe();
return true;
case Keys.Alt | Keys.Home :
case Keys.Alt | Keys.B :
App.frame.menuNavigateBeginningFile.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.B :
App.frame.menuMiscBurnTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.Home :
case Keys.Shift | Keys.B :
App.frame.menuNavigateBeginningTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.End :
case Keys.Shift | Keys.E :
App.frame.menuNavigateEndTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.E :
App.frame.menuMiscEnvironmentVariables.clickOrDescribe();
return true;
case Keys.Control | Keys.Oemplus :
App.frame.menuMiscEvaluate.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.E :
App.frame.menuEditExportClipboardToFile.clickOrDescribe();
return true;
case Keys.Control | Keys.Down :
case Keys.Shift | Keys.N :
App.frame.menuNavigateNextTagged.clickOrDescribe();
return true;
case Keys.OemPeriod :
case Keys.F5 :
App.frame.menuFileRefreshFolder.clickOrDescribe();
return true;
case Keys.Control | Keys.M :
App.frame.menuMiscMailBody.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.M :
App.frame.menuMiscMailAttachTagged.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.M :
App.frame.menuMiscManualOptions.clickOrDescribe();
return true;
case Keys.Control | Keys.N :
App.frame.menuFileNewFolder.clickOrDescribe();
return true;
//case Keys.Control | Keys.Shift | Keys.N :
//App.frame.menuFileNewViewCopy.clickOrDescribe();
//return true;
case Keys.Control | Keys.Shift | Keys.N :
App.frame.menuFileNewItemCopy.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.N :
App.frame.menuMiscNetworkConnections.clickOrDescribe();
return true;
case Keys.Shift | Keys.O :
App.frame.menuMiscOutputTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.Up :
case Keys.Shift | Keys.P :
App.frame.menuNavigatePreviousTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.D :
App.frame.menuQueryDate.clickOrDescribe();
return true;
case Keys.Control | Keys.D :
App.frame.menuEditDeleteFileNow.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.D :
App.frame.menuEditDeleteFileNowWithoutRecycle.clickOrDescribe();
return true;
case Keys.Alt | Keys.D :
App.frame.menuMiscDateOrder.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.D :
App.frame.menuMiscReverseDateOrder.clickOrDescribe();
return true;
case Keys.Shift | Keys.F :
App.frame.menuMiscFTPPut.clickOrDescribe();
return true;
case Keys.Control | Keys.F :
App.frame.menuNavigateSetFilter.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.F :
App.frame.menuNavigateClearFilter.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.F :
App.frame.menuFileFind.clickOrDescribe();
return true;
case Keys.Shift | Keys.G :
App.frame.menuMiscGetFTP.clickOrDescribe();
return true;
case Keys.Control | Keys.G :
App.frame.menuFileGoToFolder.clickOrDescribe();
return true;
case Keys.Control |Keys.Shift | Keys.G :
App.frame.menuFileGoToSpecialFolder.clickOrDescribe();
return true;
case Keys.Alt | Keys.G :
App.frame.menuFileGoToDrive.clickOrDescribe();
return true;
case Keys.Alt |Keys.Shift | Keys.G :
App.frame.menuFileGoToVirtualFolder.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.H :
App.frame.menuHelpHotKeys.clickOrDescribe();
return true;
case Keys.Control | Keys.I :
App.frame.menuEditInvertTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.I :
App.frame.menuNavigateInitialChange.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.I :
App.frame.menuEditRenameToTitle.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.E :
App.frame.menuMiscExtractTagged.clickOrDescribe();
return true;
case Keys.Alt | Keys.I :
App.frame.menuMiscIterateProcesses.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.I :
App.frame.menuMiscInquireDifferences.clickOrDescribe();
return true;
case Keys.Control | Keys.J :
App.frame.menuNavigateJump.clickOrDescribe();
return true;
case Keys.Alt | Keys.J :
App.frame.menuNavigateJumpAgain.clickOrDescribe();
return true;
case Keys.Control | Keys.K :
App.frame.menuNavigateKeywords.clickOrDescribe();
return true;
case Keys.Alt | Keys.K :
App.frame.menuNavigateKeywordsAgain.clickOrDescribe();
return true;
case Keys.Shift | Keys.L :
App.frame.menuQueryListTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.L :
App.frame.menuMiscPlayList.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.L :
App.frame.menuMiscPlayFolder.clickOrDescribe();
return true;
case Keys.Alt | Keys.L :
App.frame.menuQueryListFiles.clickOrDescribe();
return true;
case Keys.Control | Keys.L :
App.frame.menuQueryList.clickOrDescribe();
return true;
//case Keys.F7 :
case Keys.Shift | Keys.M :
App.frame.menuEditMoveTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.O :
App.frame.menuFileOpenFolder.clickOrDescribe();
return true;
case Keys.Control |Keys.Shift | Keys.O :
App.frame.menuFileOpenSpecialFolder.clickOrDescribe();
return true;
case Keys.Alt | Keys.O :
App.frame.menuFileOpenDrive.clickOrDescribe();
return true;
case Keys.Alt |Keys.Shift | Keys.O :
App.frame.menuFileOpenVirtualFolder.clickOrDescribe();
return true;
case Keys.Control | Keys.P :
App.frame.menuFilePrintTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.P :
App.frame.menuEditPathList.clickOrDescribe();
return true;
case Keys.Alt | Keys.P :
App.frame.menuQueryPath.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.P :
App.frame.menuEditPathToClipboard.clickOrDescribe();
return true;
//case Keys.Escape :
case Keys.Oemtilde :
App.frame.menuFileGoToQuickFolder.clickOrDescribe();
return true;
case Keys.Control | Keys.Q :
App.frame.menuFileOpenQuickFolder.clickOrDescribe();
return true;
case Keys.Shift | Keys.Q :
App.frame.menuFileQuickShortcut.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.Q :
App.frame.menuFileQuickURL.clickOrDescribe();
return true;
case Keys.F2 :
case Keys.Shift | Keys.R :
App.frame.menuEditRename.clickOrDescribe();
return true;
case Keys.Control | Keys.R :
App.frame.menuEditRenameWithWildcards.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.R :
App.frame.menuEditRenameWithRegex.clickOrDescribe();
return true;
case Keys.Alt | Keys.R :
App.frame.menuFileRecentFolders.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.R :
App.frame.menuMiscRecycleToggle.clickOrDescribe();
return true;
case Keys.Shift | Keys.S :
App.frame.menuQuerySize.clickOrDescribe();
return true;
case Keys.Control | Keys.S :
App.frame.menuEditSaveTags.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.S :
App.frame.menuEditRestoreTags.clickOrDescribe();
return true;
case Keys.Alt | Keys.S :
App.frame.menuMiscSizeOrder.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.S :
App.frame.menuMiscReverseSizeOrder.clickOrDescribe();
return true;
case Keys.Shift | Keys.T :
App.frame.menuQueryType.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.T :
App.frame.menuQueryTypeExtended.clickOrDescribe();
return true;
case Keys.Control | Keys.T :
App.frame.menuMiscSendToTextEditor.clickOrDescribe();
return true;
case Keys.Alt | Keys.T :
App.frame.menuMiscTypeOrder.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.T :
App.frame.menuMiscReverseTypeOrder.clickOrDescribe();
return true;
case Keys.Shift | Keys.U :
App.frame.menuMiscUnarchiveTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.U :
App.frame.menuMiscUnarchiveTaggedWithoutSubfolders.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.U :
App.frame.menuMiscUnarchiveTaggedToSameName.clickOrDescribe();
return true;
case Keys.Alt | Keys.U :
App.frame.menuMiscUnarchiveTest.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.U :
App.frame.menuMiscUnarchivePassword.clickOrDescribe();
return true;
case Keys.Control | Keys.V :
App.frame.menuEditPasteFromClipboard.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.V :
App.frame.menuMiscVolumeFormat.clickOrDescribe();
return true;case Keys.Alt | Keys.V :
App.frame.menuEditPasteCopy.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.V :
App.frame.menuEditPasteMove.clickOrDescribe();
return true;
case Keys.Alt | Keys.Clear :
case Keys.Shift | Keys.F4 :
App.frame.menuQueryWindowsOpen.clickOrDescribe();
return true;
case Keys.Control | Keys.W :
App.frame.menuMiscSendToWordProcessor.clickOrDescribe();
return true;
case Keys.Shift | Keys.W :
App.frame.menuFileWindowToggle.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.W :
App.frame.menuMiscWindowsControlPanel.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.W :
App.frame.menuMiscWebDownload.clickOrDescribe();
return true;

case Keys.Shift | Keys.X :
App.frame.menuNavigateExtensionChange.clickOrDescribe();
return true;
case Keys.Control | Keys.X :
App.frame.menuEditCutToClipboardTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.X :
App.frame.menuMiscExtraSpeechToggle.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.X :
App.frame.menuMiscExtraSpeechLog.clickOrDescribe();
return true;
case Keys.Control | Keys.Delete :
App.frame.menuEditDeleteAndRecycleTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.Y :
App.frame.menuQueryYieldTagged.clickOrDescribe();
return true;
case Keys.Alt | Keys.Y :
App.frame.menuQueryYieldFiles.clickOrDescribe();
return true;
case Keys.Control | Keys.Y :
App.frame.menuQueryYield.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.Y :
App.frame.menuQueryYieldOnDrive.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.Y :
App.frame.menuQueryYieldInOperatingSystem.clickOrDescribe();
return true;
case Keys.Shift | Keys.Z :
App.frame.menuMiscZipTagged.clickOrDescribe();
return true;
case Keys.Control | Keys.Z :
App.frame.menuMiscZipTaggedThenDelete.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.Z :
App.frame.menuMiscZipList.clickOrDescribe();
return true;
case Keys.Alt | Keys.Z :
App.frame.menuQueryStatus.clickOrDescribe();
return true;
case Keys.Shift | Keys.D5 :
App.frame.menuQueryPercentThrough.clickOrDescribe();
return true;
case Keys.Shift | Keys.D2 :
App.frame.menuQueryCharacterEncoding.clickOrDescribe();
return true;
case Keys.Shift | Keys.D3 :
App.frame.menuMiscConvertUnits.clickOrDescribe();
return true;
case Keys.Shift | Keys.D8 :
App.frame.menuQueryFilter.clickOrDescribe();
return true;
case Keys.Alt | Keys.D1 :
App.frame.menuWindowDriveA.clickOrDescribe();
return true;
case Keys.Alt | Keys.D2 :
App.frame.menuWindowDriveB.clickOrDescribe();
return true;
case Keys.Alt | Keys.D3 :
App.frame.menuWindowDriveC.clickOrDescribe();
return true;
case Keys.Alt | Keys.D4 :
App.frame.menuWindowDriveD.clickOrDescribe();
return true;
case Keys.Alt | Keys.D5 :
App.frame.menuWindowDriveE.clickOrDescribe();
return true;
case Keys.Alt | Keys.D6 :
App.frame.menuWindowDriveF.clickOrDescribe();
return true;
case Keys.Alt | Keys.D7 :
App.frame.menuWindowDriveG.clickOrDescribe();
return true;
case Keys.Alt | Keys.D8 :
App.frame.menuWindowDriveH.clickOrDescribe();
return true;
case Keys.Alt | Keys.D9 :
App.frame.menuWindowDriveI.clickOrDescribe();
return true;
case Keys.OemQuotes :
App.frame.menuQueryName.clickOrDescribe();
return true;
case Keys.Shift | Keys.OemQuotes :
App.frame.menuQueryFolderName.clickOrDescribe();
return true;
case Keys.Control | Keys.OemQuotes :
App.frame.menuQueryFullFolder.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.OemQuotes :
App.frame.menuEditFullFolderToClipboard.clickOrDescribe();
return true;
case Keys.Alt | Keys.OemQuotes :
App.frame.menuQueryClipboard.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.OemQuotes :
App.frame.menuEditClearClipboard.clickOrDescribe();
return true;
case Keys.Shift | Keys.OemQuestion :
App.frame.menuQueryWhat.clickOrDescribe();
return true;
case Keys.Control | Keys.OemQuestion :
//case Keys.Control | Keys.OemBackslash :
App.frame.menuMiscCommandPrompt.clickOrDescribe();
return true;
case Keys.Alt | Keys.OemQuestion :
App.frame.menuMiscExplorerDir.clickOrDescribe();
return true;
case Keys.Shift | Keys.D1 :
App.frame.menuEditStampTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.D0 :
App.frame.menuEditHideTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.D9 :
App.frame.menuEditShowTagged.clickOrDescribe();
return true;
case Keys.OemCloseBrackets :
App.frame.menuEditReadOnlyTagged.clickOrDescribe();
return true;
case Keys.OemOpenBrackets :
App.frame.menuEditReadWriteTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.OemCloseBrackets :
App.frame.menuEditSystemTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.OemOpenBrackets :
App.frame.menuEditGeneralTagged.clickOrDescribe();
return true;
case Keys.Shift | Keys.Oemtilde :
App.frame.menuEditShortPathToClipboard.clickOrDescribe();
return true;
case Keys.F1 :
App.frame.menuHelpDocumentation.clickOrDescribe();
return true;
case Keys.Shift | Keys.F1 :
App.frame.menuHelpChangeHistory.clickOrDescribe();
return true;
case Keys.Alt | Keys.F1 :
App.frame.menuHelpAbout.clickOrDescribe();
return true;
case Keys.Control | Keys.F1 :
App.frame.menuHelpKeyDescriber.clickOrDescribe();
return true;
case Keys.F4 :
App.frame.menuFileCurrentWindows.clickOrDescribe();
return true;
case Keys.Control | Keys.Tab :
App.frame.menuFileNextWindow.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.Tab :
App.frame.menuFilePreviousWindow.clickOrDescribe();
return true;
// Back and forward through the folders visited, which is what these two keys
// mean in every program that has them.
case Keys.Alt | Keys.Left :
App.frame.menuFileGoBack.clickOrDescribe();
return true;
case Keys.Alt | Keys.Right :
App.frame.menuFileGoForward.clickOrDescribe();
return true;
case Keys.Control | Keys.F4 :
App.frame.menuFileClose.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.F4 :
App.frame.menuFileCloseAllButCurrent.clickOrDescribe();
return true;
case Keys.Alt | Keys.F4 :
App.frame.menuFileExit.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.F4 :
App.frame.menuFileRestartWindows.clickOrDescribe();
return true;
case Keys.Apps :
case Keys.Shift | Keys.F10 :
App.frame.menuHelpContextMenu.clickOrDescribe();
return true;
//case Keys.Alt | Keys.Shift | Keys.F10 :
//App.frame.menuHelpExplorerMenu.clickOrDescribe();
//return true;
case Keys.Control | Keys.F10 :
App.frame.menuHelpSendToMenu.clickOrDescribe();
return true;
case Keys.Alt | Keys.F10 :
App.frame.menuHelpAlternateMenu.clickOrDescribe();
return true;
case Keys.F11 :
App.frame.menuHelpElevateVersion.clickOrDescribe();
return true;
case Keys.Alt | Keys.F11 :
App.frame.menuWindowArrangeIcons.clickOrDescribe();
return true;
case Keys.Control | Keys.F11 :
App.frame.menuWindowCascade.clickOrDescribe();
return true;
case Keys.Alt | Keys.Shift | Keys.F11 :
App.frame.menuWindowTileHorizontal.clickOrDescribe();
return true;
case Keys.Control | Keys.Shift | Keys.F11 :
App.frame.menuWindowTileVertical.clickOrDescribe();
return true;
// The F12 column is the AI column, matching EdSharp: F12 asks a plain
// question, Shift+F12 asks about the file you are on, and Control+F12 copies
// this session's log path.  The Timer commands held these three keys here for
// twenty years and moved to Alt+Control, below.
case Keys.F12 :
App.frame.menuMiscChatWithAI.clickOrDescribe();
return true;
case Keys.Shift | Keys.F12 :
App.frame.menuMiscChatAboutFile.clickOrDescribe();
return true;
case Keys.Control | Keys.F12 :
App.frame.menuHelpCopyLog.clickOrDescribe();
return true;
// The Timer, on a row nothing else used.
case Keys.Alt | Keys.Control | Keys.T :
App.frame.menuMiscStartTimer.clickOrDescribe();
return true;
case Keys.Alt | Keys.Control | Keys.S :
App.frame.menuMiscStopTimer.clickOrDescribe();
return true;
case Keys.Alt | Keys.Control | Keys.Y :
App.frame.menuQueryTimer.clickOrDescribe();
return true;
// Translate the tagged files with a model running on this computer.
case Keys.Alt | Keys.Shift | Keys.F7 :
App.frame.menuMiscTranslateTagged.clickOrDescribe();
return true;
}
//Lbc.Show(keyData);
//App.writeIni();
return base.ProcessCmdKey (ref msg, keyData);
//return false;
} // processCmdKey_Helper method

public MdiChild getActiveChild() {
MdiChild mdiChild = (MdiChild) App.frame.ActiveMdiChild;
if (mdiChild == null) App.say("Command unavailable outside view window!");
return mdiChild;
} // getActiveChild method

} // Frame class

public class MdiChild : Form {
public int StartTagOrUntag = -1;
public bool InZip = false;
public bool InVirtual = false;
public string sCopyText;
public string sMoveText;
public string sFilterText;
public string sGoToText;
public string sJumpText;
public string sKeywordsText;
public string sTagWithRegExpText = "";
public string sOpenText;
public string sOrderText;
public ListBox lst;
public DataTable tbl;
public BindingSource bs;

public MdiChild(Frame frame, string sDir, string sOrder, string sFilter, string sZip, string[] aPaths) {
string sVirtualFolder = "";
if (aPaths != null) {
sVirtualFolder = sZip;
sZip = "";
}

if (sZip != "") this.InZip = true;
this.SuspendLayout();
this.MdiParent = frame;

if (this.InZip) {
}
else if (aPaths != null) {
}
else if (!Directory.Exists(sDir)) sDir = Directory.GetCurrentDirectory();
if (sOrder == "") sOrder = "Type desc, Time desc";
if (sFilter == "") sFilter = "*";
this.sOrderText = sOrder;
this.sFilterText = sFilter;
App.sOrderText = sOrder;
App.sFilterText = sFilter;

tbl = new DataTable();
tbl.Columns.Add("Path", typeof(string));
tbl.Columns.Add("Name", typeof(string));
tbl.Columns.Add("Ext", typeof(string));
tbl.Columns.Add("Size", typeof(long));
tbl.Columns.Add("Time", typeof(DateTime));
tbl.Columns.Add("Attr", typeof(int));
tbl.Columns.Add("Type", typeof(char));
tbl.Columns.Add("Hidden", typeof(char));
tbl.Columns.Add("ReadOnly", typeof(char));
tbl.Columns.Add("System", typeof(char));
tbl.Columns.Add("Tagged", typeof(char));

// WHAT THE LIST SHOWS, and why this is not an expression column.
//
// It used to be one: DisplayFields carried an Expression joining six columns,
// and the DataTable expression engine parsed and evaluated it for EVERY ROW.
// That engine is a general-purpose interpreter -- it boxes each value, walks a
// parsed tree, and re-evaluates whenever any column it mentions changes. For a
// folder of ten thousand files that is ten thousand interpreted evaluations to
// produce ten thousand string concatenations, and it runs again every time a
// tag is set.
//
// It is an ordinary column now, written by the loader in the same pass that
// reads the folder, and updated in the one place a tag changes. Same text, one
// concatenation each, no interpreter.
DataColumn column = new DataColumn("DisplayFields", typeof(string));
tbl.Columns.Add(column);
watchDisplayFields(tbl);

lst = new ListBox();
lst.GotFocus += checkKeyDescriber;

bs = new BindingSource();
bs.DataSource = tbl;
if (this.InZip) {
sZip = Path.GetFullPath(sZip);
fillTableFromZip(tbl, sZip);
}
else if (aPaths != null) {
fillTableFromVirtual(tbl, aPaths);
}
else {
//Lbc.Show(sDir, Path.GetFullPath(sDir));
sDir = Path.GetFullPath(sDir);
fillTableFromDir(tbl, sDir);
}

bs.Sort = sOrder;
frame.filter_Helper(frame, this, sFilter);
lst.DataSource = bs;
lst.DisplayMember = "DisplayFields";

bs.PositionChanged += delegate(object o, EventArgs e) { Frame.status_Helper(frame, this);};

//lst.Dock = DockStyle.Fill;
lst.Width = 6 * lst.Width;
lst.Height = 4 * lst.Height;
//lst.Font = new Font("Courier New", 12.0f);
lst.Font = new Font("Courier New", 10.0f);
//lst.Left = 14;
//this.DockPadding.All = 14;
lst.Padding = new Padding(4, 4, 4, 4);
//Lbc.Show(lst.Padding.All);
this.Controls.Add(lst);
//this.Text = Path.Combine(dir, spec);
//this.Activated += delegate(object sender, EventArgs e) { if (Directory.Exists(sDir)) App.SetDirectory(sDir); };
//this.Activated += delegate(object sender, EventArgs e) { if (Directory.Exists(sDir)) App.SetDirectory(sDir); App.say(Homer.Util.stringPlural("item", this.bs.Count)); };
this.Activated += delegate(object sender, EventArgs e) {
//App.say("Activated");
string sPath = sDir;
if (this.InZip) {
sDir = Path.GetDirectoryName(sZip); 
sPath = sZip;
}
else if (aPaths != null) {
sDir = Path.GetDirectoryName(sVirtualFolder); 
sPath = sVirtualFolder;
}
else if (!Directory.Exists(sDir)) return;
App.SetDirectory(sDir);

App.recordVisit(sPath);
};

this.Shown += delegate(object sender, EventArgs e) { if (Directory.Exists(sDir)) App.SetDirectory(sDir); App.say(Homer.Util.stringPlural("item", this.bs.Count)); };
//this.Activated += delegate(object sender, EventArgs e) { if (Directory.Exists(sDir)) {App.SetDirectory(sDir); this.Text = sDir;} };
if (this.InZip) this.Text = sZip;
else if (aPaths != null) this.Text = "Virtual " + sVirtualFolder;
else this.Text = sDir;
//this.KeyPreview = true;
this.Load += delegate(object o, EventArgs e) { Frame.status_Helper(frame, this);};

this.StartPosition = FormStartPosition.CenterParent;
//this.WindowState = FormWindowState.Maximized;
this.AutoSize = true;
this.ResumeLayout();
this.Show();
} // MdiChild method

public MdiChild(Frame frame, string sDir, string sOrder, string sFilter, string sZip) {
string[] aPaths = null;
//return new MdiChild(frame, sDir, sOrder, sFilter, sZip, aPaths);
new MdiChild(frame, sDir, sOrder, sFilter, sZip, aPaths);
} // MdiChild method

public MdiChild(Frame frame, string sDir, string sOrder, string sFilter) {
string sZip = "";
string[] aPaths = null;
//return new MdiChild(frame, sDir, sOrder, sFilter, sZip, aPaths);
new MdiChild(frame, sDir, sOrder, sFilter, sZip, aPaths);
} // MdiChild method

public void checkKeyDescriber(object sender, EventArgs e) {
if (App.bKeyDescriber) {
App.say("No Key Describer");
App.bKeyDescriber = false;
}
} // checkKeyDescriber method

public static void fillTableFromZip(DataTable tbl, string sZip) {
fillTableFromZ7(tbl, sZip); 
return;

//if (tbl == null) Lbc.Show("null");
char type, hidden, readOnly, system, tagged;
DateTime time;
FileAttributes attr;
long size;
string sPath, sName, sExt;

List<ZipEntry> entryList = Frame.getZipEntries(sZip);
foreach (ZipEntry entry in entryList) {
sPath = entry.Name;
sName = sPath;
if (sName.EndsWith("/")) sName = sName.Substring(0, sName.Length - 1);
if (entry.IsDirectory) {
type = '\\';
}
else {
type = ' ';
}

sExt = Path.GetExtension(sName);
size = entry.Size;
time = entry.DateTime;
attr = 0;
hidden = ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) ? ')' : ' ';
readOnly = ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? ']' : ' ';
system = ((attr & FileAttributes.System) == FileAttributes.System) ? '}' : ' ';
tagged = ' ';
tbl.Rows.Add(sPath, sName.PadRight(50), sExt, size, time, attr, type, hidden, readOnly, system, tagged);
}
} // fillTableFromZip method

public static string[] getZ7List(string sZip) {
string sExe = Path.Combine(App.sAppDir, "7z.exe");
string sParams = "l " + Homer.Util.stringQuote(sZip) + " -slt -p" + App.sUnarchivePassword;
string sText = Homer.Util.getProgramOutput(sExe, sParams);
if (App.frame != null && App.frame.TopLevelControl.Handle != (IntPtr) Homer.Util.getForegroundWindow()) Homer.Util.forceWindow(App.frame.TopLevelControl.Handle);
sText = sText.Replace("\r\n", "\n");
sText = sText.Replace(@"\", "/");
// THE ARCHIVE'S OWN RECORD, and why it kept appearing in the list.
//
// "7z l -slt" prints a block about the ARCHIVE first -- its own path, type and
// physical size -- then a line of dashes, then a block per entry inside it.
// Everything up to and including those dashes has to go, or the archive turns
// up as an item within itself.
//
// The pattern required a BLANK LINE after the dashes. Some versions of 7-Zip
// print one and some do not, and when there is none the pattern matches
// nothing at all, so nothing is stripped and the archive's own record becomes
// the first row. That is the extra item: it is not a miscount, it is the
// header being read as content.
//
// The blank line is no longer required, and the dash run is counted rather
// than spelled out, since its length has varied between versions too.
string sMatch = @"^(?:.|\n)*?\n-{6,}[ \t]*\r?\n";
string sReplace = "";
sText = Regex.Replace(sText, sMatch, sReplace);
/*
sMatch = @"------------------- ----- ------------ ------------  ------------------------(.|\n)*?$";
sText = Regex.Replace(sText, sMatch, sReplace);
sText = Regex.Replace(sText, @"^\s*", "");
sText = Regex.Replace(sText, @"\s*$", "");
sText = Regex.Replace(sText, @"  +", "\t");
*/
string[] aLines = sText.Split('\n');

// Belt and braces. If 7-Zip ever changes the shape of its header again, a
// record naming the archive itself is still not an item inside it, so it is
// dropped by name as well as by position.
string sOwn = sZip.Replace('\\', '/');
List<string> lsKeep = new List<string>();
bool bSkipping = false;
foreach (string sTest in aLines) {
if (sTest.StartsWith("Path =")) {
string sThis = sTest.Substring(6).Trim();
bSkipping = Homer.Util.stringEquiv(sThis, sOwn);
if (bSkipping) continue;
}
// The lines that follow a skipped Path belong to it, up to the blank line
// that ends the record.
if (bSkipping) {
if (sTest.Trim().Length == 0) bSkipping = false;
continue;
}
lsKeep.Add(sTest);
}
aLines = lsKeep.ToArray();
/*
List<string> lLines = new List<string>();
foreach (string sTestLine in aLines) {
string[] aParts = sTestLine.Split('\t');
if (aParts.Length != 4) continue;
lLines.Add(sTestLine);
}
aLines = lLines.ToArray();
*/
return aLines;
} // getZ7List method

public static void fillTableFromZ7(DataTable tbl, string sZip) {
char type, hidden, readOnly, system, tagged;
DateTime time;
FileAttributes attr;
long size;
string sPath, sName, sExt;

string[] aLines = getZ7List(sZip);

sPath = "";
sName = "";
sExt = "";
size = 0;
time = DateTime.MinValue;
attr = 0;
type = ' ';
hidden = ' ';
readOnly = ' ';
system = ' ';
tagged = ' ';

int iCount = aLines.Length;
for (int i = 0; i < iCount; i++) {
string sLine = aLines[i];

// The brackets matter. This read as "(A and B) or C", so on the LAST line it
// added a row whether or not a path had been read -- an empty entry at the end
// of every listing. It has to be "A and (B or C)".
if (sPath.Length > 0 && ((i > 0 && sLine.StartsWith("Path =")) || (i == iCount - 1))) {
// Lbc.Show("Path", sPath);
tbl.Rows.Add(sPath, sName.PadRight(50), sExt, size, time, attr, type, hidden, readOnly, system, tagged);
attr = 0;
hidden = ' ';
readOnly = ' ';
system = ' ';
tagged = ' ';
}

if (sLine.Length == 0) {
// Do nothing
}
else if (sLine.StartsWith("Path =")) {
sPath = sLine.Substring(6).Trim();
sName = sPath;
if (sName.EndsWith("/")) { 
sName = sName.Substring(0, sName.Length - 1);
type = '\\';
}
else type = ' ';
sExt = Path.GetExtension(sName);
}
else if (sLine.StartsWith("Size =")) {
string s = sLine.Substring(6).Trim();
if (s.Length > 0) size = Int32.Parse(s);
}
else if (sLine.StartsWith("Modified =")) {
DateTime.TryParse(sLine.Substring(10).Trim(), out time);
}
else if (sLine.StartsWith("Attributes =")) {
string s = sLine.Substring(12).Trim();
if (s.IndexOf("h") >= 0) hidden = ')';
if (s.IndexOf("r") >= 0) readOnly = ']';
if (s.IndexOf("s") >= 0) system = '}';
}

}
} // fillTableFromZ7 method

public static void fillTableFromVirtual(DataTable tbl, string[] aPaths) {
char type, hidden, readOnly, system, tagged;
DateTime time;
FileAttributes attr;
long size;
string sName, sExt;

foreach (string s in aPaths) {
string sPath = s.Trim();
if (sPath.Length == 0 || (!Directory.Exists(sPath) && !File.Exists(sPath))) continue;
sName = Path.GetFileName(sPath);
if (Directory.Exists(sPath)) {
type = '\\';
size = -1;
}
else {
type = ' ';
size = (new FileInfo(sPath)).Length;
}

sExt = Path.GetExtension(sName);
time = File.GetLastWriteTime(sPath);
attr = File.GetAttributes(sPath);
hidden = ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) ? ')' : ' ';
readOnly = ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? ']' : ' ';
system = ((attr & FileAttributes.System) == FileAttributes.System) ? '}' : ' ';
tagged = ' ';
tbl.Rows.Add(sPath, sName.PadRight(50), sExt, size, time, attr, type, hidden, readOnly, system, tagged);
}
} // fillTableFromVirtual method

public static void fillTable(DataTable tbl, string sDir) {
char hidden, readOnly, system;
int i;
FileAttributes attr;
long size;
string file, path, name, ext;
DateTime time;

//this.bs.SuspendBinding();
tbl.Clear();

string[] files = Directory.GetDirectories(sDir);
int iDirCount = files.Length;
string sDirYield = iDirCount.ToString() + " folder";
if (iDirCount != 1) sDirYield += "s";
//if (iDirCount > 0) App.say(sDirYield);

for (i = 0; i < iDirCount; i++) {
file = files[i];
path = file;
DirectoryInfo fi = new DirectoryInfo(file);
name = Path.GetFileName(file);
ext = Path.GetExtension(file);
//size = App.DirSize(fi);
size = -1;
time = File.GetLastWriteTime(file);
attr = File.GetAttributes(file);
hidden = ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) ? ')' : ' ';
readOnly = ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? ']' : ' ';
system = ((attr & FileAttributes.System) == FileAttributes.System) ? '}' : ' ';
tbl.Rows.Add(file, name.PadRight(50), ext, size, time, attr, '\\', hidden, readOnly, system, ' ');
}

files = Directory.GetFiles(sDir);
int iFileCount = files.Length;
string sFileYield = iFileCount.ToString() + " file";
if (iFileCount != 1) sFileYield += "s";
//if (iFileCount > 0) App.say(sFileYield);

for (i = 0; i < iFileCount; i++) {
file = files[i];
path = file;
FileInfo fi = new FileInfo(file);
name = Path.GetFileName(file);
ext = Path.GetExtension(file);
//size = File.GetSize(file);
size = fi.Length;
time = File.GetLastWriteTime(file);
attr = File.GetAttributes(file);
hidden = ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) ? ')' : ' ';
readOnly = ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? ']' : ' ';
system = ((attr & FileAttributes.System) == FileAttributes.System) ? '}' : ' ';
tbl.Rows.Add(file, name.PadRight(50), ext, size, time, attr, ' ', hidden, readOnly, system, ' ');
//tbl.Rows.Add(file, name.PadRight(50), ext, size, time, attr, ' ', ' ');
//if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) Lbc.Show(file);
}
//this.bs.ResumeBinding();
} // fillTable method

public static void fillTableFromDir(DataTable tbl, string sDir) {
char type, hidden, readOnly, system;
FileAttributes attr;
long lSize;
string sPath, sName, sExt;
DateTime time;

tbl.Clear();

// STREAMED, not gathered first. GetFileSystemInfos builds an array of every
// entry before the first row can be added, so a folder with a hundred thousand
// files allocated the lot and showed nothing meanwhile. EnumerateFileSystemInfos
// hands them over as the directory is read.
//
// Either way the attributes, times and lengths come from the enumeration itself
// and cost nothing extra: Windows returns them with each entry, and the
// FileSystemInfo keeps them. Asking File.GetAttributes and File.GetLastWriteTime
// per item -- as the older, now unused, fillTable does -- is three more trips to
// the disk for something already in hand.
DirectoryInfo di = new DirectoryInfo(sDir);
tbl.BeginLoadData();
try {
foreach (FileSystemInfo fs in di.EnumerateFileSystemInfos()) {
sPath = fs.FullName;
sName = Path.GetFileName(sPath);
sExt = Path.GetExtension(sPath);
time = fs.LastWriteTime;
attr = fs.Attributes;
hidden = ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) ? ')' : ' ';
readOnly = ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? ']' : ' ';
system = ((attr & FileAttributes.System) == FileAttributes.System) ? '}' : ' ';
if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
type = '\\';
lSize = -1;
}
else {
type = ' ';
lSize = ((FileInfo) fs).Length;
}
tbl.Rows.Add(sPath, sName.PadRight(50), sExt, lSize, time, attr, type, hidden, readOnly, system, ' ');
}
}
finally {
// EndLoadData in a finally, or one unreadable entry would leave the table in
// loading state for the rest of the session.
tbl.EndLoadData();
}
// BeginLoadData suppresses the row events, so the display column is filled in
// one pass here rather than row by row.
refreshDisplayFields(tbl);
} // fillTableFromDir method

public static string displayFields_Helper(DataRow row) {
// The line the list shows: the name, then the four attribute marks and the tag.
// Exactly what the expression built, in one concatenation.
return (string) row["Name"] + " " + row["Type"] + " " + row["Hidden"] + " "
+ row["ReadOnly"] + " " + row["System"] + " " + row["Tagged"];
} // displayFields_Helper method

public static void refreshDisplayFields(DataTable tbl) {
// Fill the display column for every row, in one pass.
foreach (DataRow row in tbl.Rows) row["DisplayFields"] = displayFields_Helper(row);
} // refreshDisplayFields method

public static void watchDisplayFields(DataTable tbl) {
// Keep the display column right without every caller having to remember.
//
// The expression column did this automatically, at the price of an interpreted
// evaluation per row. Two events do the same job for the cost of a string join,
// and only when something that shows actually changes.
//
// FOUR places add rows and TWENTY-FIVE set a tag. Hooking the table once is not
// merely less typing: a hook cannot be forgotten by the next command that sets
// a tag, and twenty-nine edited call sites could be.
tbl.RowChanged += delegate(object o, DataRowChangeEventArgs e) {
if (e.Action != DataRowAction.Add) return;
e.Row["DisplayFields"] = displayFields_Helper(e.Row);
};
tbl.ColumnChanged += delegate(object o, DataColumnChangeEventArgs e) {
// Only the columns that show. Writing DisplayFields here would call this
// again, so that column is passed over.
string sColumn = e.Column.ColumnName;
if (sColumn == "DisplayFields") return;
if (sColumn != "Tagged" && sColumn != "Name" && sColumn != "Type"
&& sColumn != "Hidden" && sColumn != "ReadOnly" && sColumn != "System") return;
e.Row["DisplayFields"] = displayFields_Helper(e.Row);
};
} // watchDisplayFields method

protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
return App.frame.processCmdKey_Helper(ref msg, keyData);
} // ProcessCmdKey method

public void ListBox_KeyPress(Object sender, KeyPressEventArgs e) {
if (skipKey) e.Handled = true;
}

public bool skipKey;
public void listBox_KeyDown(Object sender, KeyEventArgs e) {
skipKey = true;
Keys code = e.KeyCode;
bool alt = e.Alt;
bool control = e.Control;
bool shift = e.Shift;
if (code == Keys.Alt) App.say("alt");
else if (code == Keys.D && !alt && !control && shift) App.frame.menuQueryDate.clickOrDescribe();
else if (code == Keys.D && alt && !control && !shift) App.frame.menuMiscDateOrder.clickOrDescribe();
else if (code == Keys.L && !alt && control && !shift) App.frame.menuQueryList.clickOrDescribe();
else if (code == Keys.S && !alt && !control && shift) App.frame.menuQuerySize.clickOrDescribe();
else {
skipKey = false;
//base.OnKeyDown(e);
}
} // listBox_KeyDown method

public void listBox_KeyUp(Object sender, KeyEventArgs e) {
skipKey = true;
Keys code = e.KeyCode;
bool alt = e.Alt;
bool control = e.Control;
bool shift = e.Shift;
e.Handled = true;
if (code == Keys.Alt) App.say("alt");
else if (code == Keys.D && !alt && !control && shift) App.frame.menuQueryDate.clickOrDescribe();
else if (code == Keys.D && alt && !control && !shift) App.frame.menuMiscDateOrder.clickOrDescribe();
else if (code == Keys.L && !alt && control && !shift) App.frame.menuQueryList.clickOrDescribe();
else if (code == Keys.S && !alt && !control && shift) App.frame.menuQuerySize.clickOrDescribe();
else {
skipKey = false;
//base.OnKeyDown(e);
}
} // listBox_KeyUp method
} // MdiChild class

class CompareFileSystemInfo: IComparer {
public int Compare(object fs1, object fs2) {
FileSystemInfo f1 = (FileSystemInfo) fs1;
FileSystemInfo f2 = (FileSystemInfo) fs2;
return DateTime.Compare(f1.LastWriteTime, f2.LastWriteTime);
} // compareDate method
} // CompareFileInfo class

public class HomerToolStripMenuItem : ToolStripMenuItem {
public HomerToolStripMenuItem(string sText) {
this.Text = sText;
this.Name = sText.Replace("&", "").Replace(" ...", "");
}

public HomerToolStripMenuItem(string sText, string s, EventHandler eh) {
this.Text = sText;
this.Name = sText.Replace("&", "").Replace(" ...", "");
this.Click += eh;
}

public string[] getKeySummary() {
// string sCommand = this.Text.Replace("&", "").Replace(" ...", "");
string sCommand = this.Name;
string sHotkeyIni = Path.Combine(App.sAppDir, "Hotkeys.ini");
string sValue = App.readValue(sHotkeyIni, "Hotkeys", sCommand, "");
if (sCommand.StartsWith("Drive ") && sCommand.Length == 7) {
string sLetter = sCommand.Substring(6, 1);
int iDigit = "ABCDEFGHI".IndexOf(sLetter) + 1;
sValue = "Alt+" + iDigit + ", Open " + sCommand;
}
else if (sValue.Length == 0) sValue = App.readValue(sHotkeyIni, "Hotkeys", "Say " + sCommand, "");

// Hotkeys.ini is a user override and is read first.  The shipped defaults live
// in KeyMap.cs, generated from Hotkeys.ini and compiled into the program,
// because the installer copies Hotkeys.ini with the onlyifdoesntexist flag: a
// machine that already has FileDir never receives an updated copy, so a
// description added in a new version would otherwise never be heard.
if (sValue.Length == 0) sValue = Homer.KeyMap.lookUp(sCommand);

if (sValue.Length == 0) sValue = "No description available";
string sKey = "";
string sDescription = "";
int iComma = sValue.IndexOf(",");
if (iComma >=0) {
sKey = sValue.Substring(0, iComma);
sDescription = sValue.Substring(iComma + 1);
}
else sDescription = sValue;

return new string[] {sCommand, sKey, sDescription};
} // getKeySummary method

public void clickOrDescribe() {
string sCommand = this.Text.Replace("&", "").Replace(" ...", "");
if (App.bKeyDescriber && sCommand != "Key Describer") {
string[] aSummary = getKeySummary();
string sKey = aSummary[1];
string sDescription = aSummary[2];
App.say(sCommand);
App.say(sKey);
App.say(sDescription);
}
else this.PerformClick();
} // clickOrDescribe method
} // HomerToolStripMenuItem class

public class HomerList : List<string> {

public char Delimiter = '|';
public bool CaseSensitive = false;

public int Max {
get {
return this.Count - 1;
}
}

public string Segments {
get {
string[] aSegments = this.ToArray();
string sSegments = String.Join(this.Delimiter.ToString(), aSegments);
return sSegments;
}
set {
string[] aSegments = value.Split(this.Delimiter);
this.Clear();
if (value.Length > 0) this.AddRange(aSegments);
}
} // Segments property

public HomerList() {
//new HomerList(this.Segments, this.Delimiter, this.CaseSensitive);
} // HomerList constructor

public HomerList(string sSegments) {
//new HomerList(sSegments, this.Delimiter, this.CaseSensitive);
this.Segments = sSegments;
//new HomerList();
} // HomerList constructor

public HomerList(string sSegments, char cDelimiter) {
this.Delimiter = cDelimiter;
this.Segments = sSegments;
} // HomerList constructor

public HomerList(string sSegments, char cDelimiter, bool bCaseSensitive) {
this.Delimiter = cDelimiter;
this.Segments = sSegments;
this.CaseSensitive = bCaseSensitive;
} // HomerList constructor

public HomerList(string[] aItems) {
this.AddRange(aItems);
} // HomerList constructor

public new int IndexOf(string sItem) {
if (this.CaseSensitive) return base.IndexOf(sItem);
else {
int iIndex = -1;
string sValue = sItem.ToLower();
for (int i = 0; i < this.Count; i++) {
if (this[i].ToLower() == sValue) {
iIndex = i;
break;
}
}
return iIndex;
}
} // IndexOf method

public new bool Contains(string sItem) {
return this.IndexOf(sItem) >= 0;
} // Contains method

public new void Sort() {
if (this.CaseSensitive) base.Sort();
else {
string[] a = this.ToArray();
Array.Sort(a, new CaseInsensitiveComparer());
this.Clear();
this.AddRange(a);
}
} // Sort method

public string getSegments(char cDelimiter) {
this.Delimiter = cDelimiter;
return this.Segments;
} // getSegments method

public void keepUnique() {
for (int i = this.Count - 1; i >=0; i--) {
string s = this[i];
if (this.IndexOf(s) < i) this.RemoveAt(i);
}
} // keepUnique method

public void removeLike(string sMatch) {
RegexOptions options = RegexOptions.Multiline;
if (!this.CaseSensitive) options |= RegexOptions.IgnoreCase;
Regex rx = new Regex(sMatch, options);

for (int i = this.Count - 1; i >= 0; i--) {
if (rx.IsMatch(this[i])) this.RemoveAt(i);
}
} // removeLike method

public void keepLike(string sMatch) {
HomerList hl = this.findLike(sMatch);
this.Clear();
this.AddRange(hl);
} // keepLike method

public HomerList findLike(string sMatch) {
RegexOptions options = RegexOptions.Multiline;
if (!this.CaseSensitive) options |= RegexOptions.IgnoreCase;
Regex rx = new Regex(sMatch, options);

HomerList hl = new HomerList();
foreach (string s in this) {
if (rx.IsMatch(s)) hl.Add(s);
}
return hl;
} // findLike method

public void replaceLike(string sMatch, string sReplace) {
RegexOptions options = RegexOptions.Multiline;
if (!this.CaseSensitive) options |= RegexOptions.IgnoreCase;
Regex rx = new Regex(sMatch, options);

for (int i = 0; i < this.Count; i++)  this[i] = rx.Replace(this[i], sReplace);
} // replaceLike method

public void Push(string sItem) {
this.Insert(0, sItem);
} // Push method

public string Pop() {
int iUpper = this.Count - 1;
string sItem = this[iUpper];
this.RemoveAt(iUpper);
return sItem;
} // Pop method

public string Shift() {
int iLower = 0;
string sItem = this[iLower];
this.RemoveAt(iLower);
return sItem;
} // Shift method

public new void Remove(string sItem) {
bool bLoop = true;
while (bLoop) {
int iIndex = this.IndexOf(sItem);
if (iIndex == -1) break;
this.RemoveAt(iIndex);
}
} // Remove method

public void RemoveRange(HomerList hl) {
foreach (string sItem in hl) this.Remove(sItem);
} // RemoveRange method

public void RemoveRange(string[] aItems) {
HomerList hl = new HomerList(aItems);
this.RemoveRange(hl);
} // RemoveRange method

public void addUnique(string sItem) {
if (!this.Contains(sItem)) this.Add(sItem);
} // addUnique method

public void pushUnique(string sItem) {
if (!this.Contains(sItem)) this.Push(sItem);
} // pushUnique method

public void RemoveRange(string sSegments) {
Char cDelimiter = '|';
HomerList hl = new HomerList(sSegments, cDelimiter);
this.RemoveRange(hl);
} // RemoveRange method

public void RemoveRange(string sSegments, Char cDelimiter) {
HomerList hl = new HomerList(sSegments, cDelimiter);
this.RemoveRange(hl);
} // RemoveRange method

public void saveAddRange(HomerList hl) {
this.AddRange(hl);
} // AddRange method

public void OldAddRange(string[] aItems) {
HomerList hl = new HomerList(aItems);
this.AddRange(hl);
} // AddRange method

public void AddRange(string sSegments) {
Char cDelimiter = this.Delimiter;
HomerList hl = new HomerList(sSegments, cDelimiter);
this.AddRange(hl);
} // AddRange method

public void AddRange(string sSegments, Char cDelimiter) {
HomerList hl = new HomerList(sSegments, cDelimiter);
this.AddRange(hl);
} // AddRange method

public void addUniqueRange(HomerList hl) {
foreach (string s in hl) if (!this.Contains(s)) this.Add(s);
} // addUniqueRange method

public void addUniqueRange(string[] aItems) {
HomerList hl = new HomerList(aItems);
this.addUniqueRange(hl);
} // addUniqueRange method

public void addUniqueRange(string sSegments) {
Char cDelimiter = this.Delimiter;
HomerList hl = new HomerList(sSegments, cDelimiter);
this.addUniqueRange(hl);
} // addUniqueRange method

public void addUniqueRange(string sSegments, Char cDelimiter) {
HomerList hl = new HomerList(sSegments, cDelimiter);
this.addUniqueRange(hl);
} // addUniqueRange method

public HomerList findRange(HomerList hl) {
HomerList hlReturn = new HomerList();
foreach (string sItem in hl) if (this.Contains(sItem)) hlReturn.Add(sItem);
return hlReturn;
} // findRange method

public void findRange(string[] aItems) {
HomerList hl = new HomerList(aItems);
this.findRange(hl);
} // findRange method

public void findRange(string sSegments) {
Char cDelimiter = '|';
HomerList hl = new HomerList(sSegments, cDelimiter);
this.findRange(hl);
} // findRange method

public void findRange(string sSegments, Char cDelimiter) {
HomerList hl = new HomerList(sSegments, cDelimiter);
this.findRange(hl);
} // findRange method

public HomerList Clone() {
string[] aItems = this.ToArray();
HomerList hl = new HomerList(aItems);
return hl;
} // Clone method

public string minValue() {
if (this.Count == 0) return "";

HomerList hl = this.Clone();
hl.Sort();
return hl[0];
} // minValue method

public string maxValue() {
if (this.Count == 0) return "";

HomerList hl = this.Clone();
hl.Sort();
return hl[hl.Count - 1];
} // maxValue method

public int minLength() {
int iLength = 2000000000;
foreach (string sItem in this) if (sItem.Length < iLength) iLength = sItem.Length;
if (iLength == 2000000000) iLength = 0;
return iLength;
} // minLength method

public int maxLength() {
int iLength = 0;
foreach (string sItem in this) if (sItem.Length > iLength) iLength = sItem.Length;
return iLength;
} // maxLength method

public void sortLength() {
this.Sort(delegate(string s1, string s2) {return s1.Length.CompareTo(s2.Length);} );
} // sortLength method

public void ToLower() {
for (int i = 0; i < this.Count; i++) this[i] = this[i].ToLower();
} // ToLower method

public void ToUpper() {
for (int i = 0; i < this.Count; i++) this[i] = this[i].ToUpper();
} // ToUpper method

public void TrimStart() {
for (int i = 0; i < this.Count; i++) this[i] = this[i].TrimStart();
} // TrimStart method

public void TrimEnd() {
for (int i = 0; i < this.Count; i++) this[i] = this[i].TrimEnd();
} // TrimEnd method

public void TrimStart(char[] a) {
for (int i = 0; i < this.Count; i++) this[i] = this[i].TrimStart(a);
} // TrimStart method

public void TrimEnd(char[] a) {
for (int i = 0; i < this.Count; i++) this[i] = this[i].TrimEnd(a);
} // TrimEnd method

public void PadLeft(int iLength, char c) {
for (int i = 0; i < this.Count; i++) this[i] = this[i].PadLeft(iLength, c);
} // PadLeft method

public void PadRight(int iLength, char c) {
for (int i = 0; i < this.Count; i++) this[i] = this[i].PadRight(iLength, c);
} // PadRight method

public void pushRange(HomerList hl) {
this.InsertRange(0, hl);
} // pushRange method

public void pushRange(string sSegments) {
Char cDelimiter = this.Delimiter;
HomerList hl = new HomerList(sSegments, cDelimiter);
this.pushRange(hl);
} // pushRange method

public void pushRange(string sSegments, Char cDelimiter) {
HomerList hl = new HomerList(sSegments, cDelimiter);
this.pushRange(hl);
} // pushRange method

public void pushUniqueRange(HomerList hl) {
foreach (string s in hl) if (!this.Contains(s)) this.Push(s);
} // pushUniqueRange method

public void pushUniqueRange(string[] aItems) {
HomerList hl = new HomerList(aItems);
this.pushUniqueRange(hl);
} // pushUniqueRange method

public void pushUniqueRange(string sSegments) {
Char cDelimiter = this.Delimiter;
HomerList hl = new HomerList(sSegments, cDelimiter);
this.pushUniqueRange(hl);
} // pushUniqueRange method

public void pushUniqueRange(string sSegments, Char cDelimiter) {
HomerList hl = new HomerList(sSegments, cDelimiter);
this.pushUniqueRange(hl);
} // pushUniqueRange method

} // HomerList class

} // FileDir namespace
