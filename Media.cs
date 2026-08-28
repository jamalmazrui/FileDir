// Media.cs -- finding and running ExifTool, ffmpeg and ffprobe, for Homer Tools.
//
// Copyright 2006-2026 by Jamal Mazrui
// MIT License. See License.md, which carries the terms in full.
//
// WHAT THIS IS FOR
//
// ExifTool reads and writes the metadata inside a file: the camera that took a
// photograph, the artist and album of a song, the duration and codecs of a
// video, the author and producer of a PDF. It knows thousands of tags across
// hundreds of formats, which is why FileDir asks it rather than parsing any of
// those formats itself.
//
// ffmpeg and ffprobe are here for the same reason and are located the same way.
//
// This code is adapted from HomerScribe, which uses ExifTool to write picture
// descriptions. The finding is the hard part and HomerScribe already solved it;
// what each program does with the tool afterwards differs.
//
// WHY EVERY CANDIDATE IS RUN RATHER THAN GUESSED AT
//
// HomerScribe's note is worth keeping whole: a copy installed with winget sat
// alongside an older one in the build folder, and the question "which will be
// used" could not be answered by looking. So every likely place is tried, each
// candidate is RUN to learn its version, and all of them are recorded with the
// choice and the reason. The log then answers that question without anybody
// going to look.
//
// ONE DIFFERENCE FROM HOMERSCRIBE
//
// HomerScribe insists on a single-file ExifTool, because a packaged one --
// a small launcher plus an "exiftool_files" folder holding Perl -- complicates
// what it has to ship. FileDir only READS, so a packaged copy works perfectly
// well and is accepted. A single file beside FileDir.exe is still preferred,
// and the log says which was chosen.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Homer {

public static class Media {

private static string sExifToolCache = null;
private static string sExifToolLog = "";

public static string findTool(string sName) {
// Beside the program first, then the PATH. A tool the developer dropped into
// the program folder is the one meant to be used, whatever else is installed.
string sBeside = Path.Combine(exeFolder(), sName + ".exe");
if (File.Exists(sBeside)) return sBeside;
string sPath = Environment.GetEnvironmentVariable("PATH");
if (sPath == null) return "";
foreach (string sFolder in sPath.Split(Path.PathSeparator)) {
if (sFolder.Trim().Length == 0) continue;
string sTry = "";
try {
sTry = Path.Combine(sFolder.Trim(), sName + ".exe");
}
catch (Exception) {
continue;
}
if (File.Exists(sTry)) return sTry;
}
return "";
} // findTool method

public static string exeFolder() {
try {
return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
}
catch (Exception) {
return Directory.GetCurrentDirectory();
}
} // exeFolder method

public static double versionRank(string sVersion) {
// A version turned into something that sorts correctly.
//
// "13.11" is LATER than "13.8" -- ExifTool numbers its releases that way --
// but as decimals 13.8 is the larger and the older copy would be chosen. Each
// part is a whole number, so the second is scaled rather than treated as a
// fraction.
Match parts = Regex.Match(sVersion.Trim(), @"^(\d+)(?:\.(\d+))?");
if (!parts.Success) return -1.0;
double nMajor = 0.0;
double nMinor = 0.0;
double.TryParse(parts.Groups[1].Value, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out nMajor);
if (parts.Groups[2].Success)
double.TryParse(parts.Groups[2].Value, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out nMinor);
return nMajor * 1000.0 + nMinor;
} // versionRank method

public static string exifToolLog() {
// What the search found and why, for the Type Extended dialog to show when
// nothing was found. A person who is told "not found" and nothing else has no
// idea what to do next.
return sExifToolLog;
} // exifToolLog method

public static string exifToolProgram() {
// Every ExifTool on this machine, run, and the newest chosen.
//
// The places cover winget's two habits -- a real install under Program Files
// or the user's Programs folder, and a shim under WinGet\Links -- as well as
// the package folder it unpacks into, FileDir's own folder, and the PATH.
if (sExifToolCache != null) return sExifToolCache;
sExifToolCache = "";
StringBuilder sbLog = new StringBuilder();

List<string> lsWhere = new List<string>();
lsWhere.Add(Path.Combine(exeFolder(), "exiftool.exe"));
lsWhere.Add(@"C:\FileDir\exiftool.exe");
foreach (string sRoot in new string[] {
Environment.GetEnvironmentVariable("ProgramFiles"),
Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
Environment.GetEnvironmentVariable("ProgramW6432")}) {
if (sRoot != null && sRoot.Length > 0) lsWhere.Add(Path.Combine(sRoot, "ExifTool", "exiftool.exe"));
}
string sLocal = Environment.GetEnvironmentVariable("LOCALAPPDATA");
if (sLocal != null && sLocal.Length > 0) {
lsWhere.Add(Path.Combine(sLocal, "Programs", "ExifTool", "exiftool.exe"));
// winget's shim folder, which is on the PATH of a shell opened after the
// install but not of one opened before it.
lsWhere.Add(Path.Combine(sLocal, "Microsoft", "WinGet", "Links", "exiftool.exe"));
string sPackages = Path.Combine(sLocal, "Microsoft", "WinGet", "Packages");
try {
if (Directory.Exists(sPackages)) {
foreach (string sOne in Directory.GetDirectories(sPackages, "*ExifTool*")) {
foreach (string sExe in Directory.GetFiles(sOne, "exiftool.exe", SearchOption.AllDirectories))
lsWhere.Add(sExe);
}
}
}
catch (Exception) {
}
}
string sOnPath = findTool("exiftool");
if (sOnPath.Length > 0) lsWhere.Add(sOnPath);

string sBest = "";
string sBestVersion = "";
double nBest = -1.0;
List<string> lsSeen = new List<string>();
foreach (string sOne in lsWhere) {
if (sOne == null || sOne.Length == 0 || !File.Exists(sOne)) continue;
bool bAlready = false;
foreach (string sHad in lsSeen) if (String.Compare(sHad, sOne, true) == 0) bAlready = true;
if (bAlready) continue;
lsSeen.Add(sOne);
string sOut, sErr;
int iCode = run(sOne, "-ver", out sOut, out sErr);
string sVersion = (sOut + sErr).Trim();
if (iCode != 0 || sVersion.Length == 0 || !Regex.IsMatch(sVersion, @"^\d+(\.\d+)?")) {
sbLog.AppendLine("  " + sOne + " -- will not run");
continue;
}
double nVersion = versionRank(sVersion);
sbLog.AppendLine("  " + sOne + " -- version " + sVersion);
if (nVersion > nBest) {
nBest = nVersion;
sBest = sOne;
sBestVersion = sVersion;
}
}
if (sBest.Length == 0) {
sbLog.AppendLine("No ExifTool was found. Put exiftool.exe in the FileDir folder,");
sbLog.AppendLine("or install it with: winget install OliverBetz.ExifTool");
}
else {
sbLog.AppendLine("Chosen: " + sBest + ", version " + sBestVersion
+ (lsSeen.Count > 1 ? ", the newest of " + lsSeen.Count + " found." : "."));
}
sExifToolLog = sbLog.ToString();
sExifToolCache = sBest;
return sExifToolCache;
} // exifToolProgram method

public static string ffmpegProgram() {
return findTool("ffmpeg");
} // ffmpegProgram method

public static string ffprobeProgram() {
return findTool("ffprobe");
} // ffprobeProgram method

public static List<string[]> readProperties(string sPath, out string sError) {
// Every metadata field ExifTool can see in one file, as name and value pairs.
//
// -s3 would give the values alone; the group-and-name form is wanted here, so
// the plain listing is parsed instead. -charset UTF8 matters: without it a
// caption in anything but the system code page comes back as nonsense.
// -a keeps duplicate tags rather than silently dropping all but one, and -G
// prefixes each with its group so two tags of the same name stay apart.
sError = "";
List<string[]> lsPairs = new List<string[]>();
string sExe = exifToolProgram();
if (sExe.Length == 0) {
sError = "ExifTool was not found, so only the Windows properties are shown.";
return lsPairs;
}
if (!File.Exists(sPath)) {
sError = "The file was not found.";
return lsPairs;
}
string sOut, sErr;
int iCode = run(sExe, "-a -G -charset UTF8 -m -q " + Homer.Util.stringQuote(sPath), out sOut, out sErr);
if (iCode != 0 && sOut.Trim().Length == 0) {
sError = sErr.Trim();
if (sError.Length == 0) sError = "ExifTool returned " + iCode + ".";
return lsPairs;
}
foreach (string sLine in sOut.Replace("\r\n", "\n").Split('\n')) {
int iColon = sLine.IndexOf(':');
if (iColon < 1) continue;
string sName = sLine.Substring(0, iColon).Trim();
string sValue = sLine.Substring(iColon + 1).Trim();
if (sName.Length == 0) continue;
lsPairs.Add(new string[] {sName, sValue});
}
return lsPairs;
} // readProperties method

public static int run(string sProgram, string sArguments, out string sOut, out string sErr) {
// A process with NO WINDOW, with both streams read before the wait: a full
// pipe buffer would otherwise deadlock it. The program and its arguments go
// in separately, so no command interpreter quoting rule applies.
sOut = "";
sErr = "";
try {
Process process = new Process();
process.StartInfo.FileName = sProgram;
process.StartInfo.Arguments = sArguments;
process.StartInfo.UseShellExecute = false;
process.StartInfo.RedirectStandardOutput = true;
process.StartInfo.RedirectStandardError = true;
process.StartInfo.CreateNoWindow = true;
process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
process.Start();
sOut = process.StandardOutput.ReadToEnd();
sErr = process.StandardError.ReadToEnd();
process.WaitForExit();
Homer.Log.command(sProgram, sArguments, process.ExitCode, sErr);
return process.ExitCode;
}
catch (Exception ex) {
sErr = ex.Message;
Homer.Log.write("Could not start " + sProgram + ": " + ex.Message);
return -1;
}
} // run method

} // Media class

} // Homer namespace
