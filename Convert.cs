// Convert.cs -- document conversion through Pandoc, for Homer Tools.
//
// Copyright 2006-2026 by Jamal Mazrui
// MIT License. See License.md, which carries the terms in full.
//
// WHAT THIS IS FOR
//
// Pandoc converts between about forty markup and document formats. FileDir,
// EdSharp and HomerScribe all want the same conversions, and Pandoc is about
// 100 MB, so one copy lives machine-wide in C:\Program Files\Pandoc and every
// program finds it here.
//
// This file is the finding and the running. What each program does with a
// conversion is its own business: FileDir converts tagged files in a batch,
// EdSharp converts the file in the window.
//
// WHY PANDOC IS NOT SHIPPED INSIDE THE PROGRAM FOLDER
//
// It used to be, and three programs each carrying 100 MB of the same
// executable under Program Files is not something to ask anyone to download.
// The installer offers it as a checkbox instead, probing first so an existing
// copy is updated rather than duplicated.
//
// WHAT PANDOC CANNOT DO
//
// It has no reader for legacy .doc, .ppt or .xls, and no reader for PDF. Those
// go through 2htm, which FileDir already ships. This class says so plainly
// rather than letting a conversion fail with a message from Pandoc about a
// format it never claimed to read.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Homer {

public static class Convert {

// Where Pandoc's own installer puts it, and where winget puts it with
// --scope machine. Checked by path as well as by name, because a Pandoc
// installed a minute ago is not on this process's PATH.
public const string c_sMachinePath = @"C:\Program Files\Pandoc\pandoc.exe";

private static string sPandocCache = null;

public static string pandocPath() {
// The Pandoc to use, or an empty string when there is none. Asked once.
if (sPandocCache != null) return sPandocCache;
sPandocCache = "";
List<string> lsCandidates = new List<string>();
lsCandidates.Add(Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Pandoc\pandoc.exe"));
lsCandidates.Add(c_sMachinePath);
lsCandidates.Add(Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Pandoc\pandoc.exe"));
foreach (string sCandidate in lsCandidates) {
if (sCandidate.Length > 0 && File.Exists(sCandidate)) {
sPandocCache = sCandidate;
return sPandocCache;
}
}
// Then the PATH, for a Pandoc installed somewhere else entirely.
string sPath = Environment.GetEnvironmentVariable("PATH");
if (sPath != null) {
foreach (string sDir in sPath.Split(';')) {
if (sDir.Trim().Length == 0) continue;
try {
string sTry = Path.Combine(sDir.Trim(), "pandoc.exe");
if (File.Exists(sTry)) {
sPandocCache = sTry;
return sPandocCache;
}
}
catch {
// A malformed PATH entry is not worth failing over.
}
}
}
return sPandocCache;
} // pandocPath method

public static bool havePandoc() {
return pandocPath().Length > 0;
} // havePandoc method

public static string pandocVersion() {
string sExe = pandocPath();
if (sExe.Length == 0) return "";
string sOut, sError;
if (!run(sExe, "--version", out sOut, out sError)) return "";
foreach (string sLine in sOut.Replace("\r\n", "\n").Split('\n'))
if (sLine.Trim().Length > 0) return sLine.Trim();
return "";
} // pandocVersion method

// The formats FileDir offers as conversion targets, with the extension each
// one writes.  Kept short on purpose: Pandoc writes about forty, and a list of
// forty is a list nobody reads.  These are the ones a file manager is asked
// for.
public static readonly string[,] c_aTargets = {
{"Word document", "docx"},
{"Web page", "html"},
{"Markdown", "md"},
{"Plain text", "txt"},
{"OpenDocument text", "odt"},
{"Rich text", "rtf"},
{"EPUB book", "epub"},
{"LaTeX", "tex"},
{"reStructuredText", "rst"},
{"MediaWiki", "mediawiki"}
};

// The extensions Pandoc can READ. Legacy .doc, .ppt and .xls are absent
// because Pandoc has no reader for them, and so is .pdf: those go through
// 2htm instead. Saying so here means a person is told why rather than shown
// Pandoc's own complaint.
public static readonly string[] c_aReadable = {
".docx", ".odt", ".epub", ".html", ".htm", ".md", ".markdown", ".rst",
".tex", ".latex", ".rtf", ".txt", ".org", ".adoc", ".asciidoc", ".csv",
".ipynb", ".mediawiki", ".textile", ".typ", ".xlsx", ".pptx", ".fb2", ".djot"
};

public static bool canRead(string sPath) {
string sExt = Path.GetExtension(sPath).ToLower();
return Array.IndexOf(c_aReadable, sExt) >= 0;
} // canRead method

public static string targetName(int iIndex) {
if (iIndex < 0 || iIndex >= c_aTargets.GetLength(0)) return "";
return c_aTargets[iIndex, 0];
} // targetName method

public static string targetExtension(int iIndex) {
if (iIndex < 0 || iIndex >= c_aTargets.GetLength(0)) return "";
return c_aTargets[iIndex, 1];
} // targetExtension method

public static string[] targetLabels() {
// "Word document (docx)" and so on, for a Pick dialog.  Naming the extension
// as well as the format means nobody has to guess what lands on disk.
string[] aLabels = new string[c_aTargets.GetLength(0)];
for (int i = 0; i < aLabels.Length; i++)
aLabels[i] = c_aTargets[i, 0] + " (" + c_aTargets[i, 1] + ")";
return aLabels;
} // targetLabels method

// ---------------------------------------------------------------------------
// What a file can become, by what it is.
//
// The Output As command asks this rather than offering one list of everything:
// a person on an .mp4 does not want to be shown LaTeX, and a person on a .docx
// does not want to be shown Opus. The list is short and it fits the file.
//
// Three engines do the work and the caller does not have to know which:
//   document -- Pandoc
//   legacy   -- 2htm, for the formats Pandoc has no reader for
//   audio, video, image -- ffmpeg
// ---------------------------------------------------------------------------

private static readonly string[] c_aDocumentSources = {
".docx", ".odt", ".epub", ".html", ".htm", ".md", ".markdown", ".rst",
".tex", ".latex", ".rtf", ".txt", ".org", ".adoc", ".asciidoc", ".csv",
".ipynb", ".mediawiki", ".textile", ".typ", ".xlsx", ".pptx", ".fb2", ".djot"
};

// Formats Pandoc cannot read at all. 2htm handles them, which is why FileDir
// ships it: legacy Office and PDF are exactly the files people still have.
private static readonly string[] c_aLegacySources = {".doc", ".ppt", ".xls", ".pdf"};

private static readonly string[] c_aAudioSources = {
".mp3", ".wav", ".flac", ".m4a", ".aac", ".ogg", ".opus", ".wma", ".aiff", ".aif"
};

private static readonly string[] c_aVideoSources = {
".mp4", ".mkv", ".avi", ".mov", ".wmv", ".webm", ".flv", ".m4v", ".mpg",
".mpeg", ".ts", ".m2ts", ".3gp", ".ogv"
};

private static readonly string[] c_aImageSources = {
".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff", ".webp"
};

public static string categoryOf(string sPath) {
string sExt = Path.GetExtension(sPath).ToLower();
if (Array.IndexOf(c_aDocumentSources, sExt) >= 0) return "document";
if (Array.IndexOf(c_aLegacySources, sExt) >= 0) return "legacy";
if (Array.IndexOf(c_aAudioSources, sExt) >= 0) return "audio";
if (Array.IndexOf(c_aVideoSources, sExt) >= 0) return "video";
if (Array.IndexOf(c_aImageSources, sExt) >= 0) return "image";
return "";
} // categoryOf method

public static string[,] targetsFor(string sPath) {
// Name and extension pairs, for a Pick list. The source's own extension is
// left out further down, since converting a file to what it already is has no
// meaning.
string sCategory = categoryOf(sPath);
if (sCategory == "document") return c_aTargets;
if (sCategory == "legacy")
return new string[,] {
{"Plain text", "txt"},
{"Web page", "htm"}
};
if (sCategory == "audio")
return new string[,] {
{"MP3 audio", "mp3"},
{"M4A audio (AAC)", "m4a"},
{"WAV audio, uncompressed", "wav"},
{"FLAC audio, lossless", "flac"},
{"Ogg Vorbis audio", "ogg"},
{"Opus audio", "opus"}
};
if (sCategory == "video")
return new string[,] {
{"MP4 video", "mp4"},
{"Matroska video", "mkv"},
{"WebM video", "webm"},
{"QuickTime video", "mov"},
{"AVI video", "avi"},
{"MP3 audio, sound only", "mp3"},
{"M4A audio, sound only", "m4a"},
{"WAV audio, sound only", "wav"}
};
if (sCategory == "image")
return new string[,] {
{"PNG image", "png"},
{"JPEG image", "jpg"},
{"WebP image", "webp"},
{"BMP image", "bmp"},
{"GIF image", "gif"},
{"TIFF image", "tif"}
};
return new string[0, 2];
} // targetsFor method

public static string[] targetLabelsFor(string sPath, out string[] aExtensions) {
// The labels and their extensions, with the source's own format removed.
string[,] aTargets = targetsFor(sPath);
string sOwn = Path.GetExtension(sPath).ToLower().TrimStart('.');
List<string> lsLabels = new List<string>();
List<string> lsExts = new List<string>();
for (int i = 0; i < aTargets.GetLength(0); i++) {
string sExt = aTargets[i, 1];
if (String.Compare(sExt, sOwn, true) == 0) continue;
// .jpeg and .jpg are the same picture; offering both would be noise.
if (sOwn == "jpeg" && sExt == "jpg") continue;
if (sOwn == "htm" && sExt == "html") continue;
if (sOwn == "html" && sExt == "htm") continue;
lsLabels.Add(aTargets[i, 0] + " (" + sExt + ")");
lsExts.Add(sExt);
}
aExtensions = lsExts.ToArray();
return lsLabels.ToArray();
} // targetLabelsFor method

// Extensions that are already text and can simply be read.
private static readonly string[] c_aPlainSources = {
".txt", ".md", ".markdown", ".csv", ".log", ".ini", ".inix", ".json", ".xml",
".yml", ".yaml", ".cs", ".js", ".py", ".ps1", ".cmd", ".bat", ".vbs", ".sql",
".c", ".cpp", ".h", ".java", ".php", ".rb", ".go", ".rs", ".ts", ".css",
".jss", ".jsh", ".jsd", ".jkm", ".iss", ".bas", ".vb", ".sh", ".tex", ".rst"
};

public static string toPlainText(string sPath, out string sError) {
// The text of a file, whatever the file is, by whichever engine can read it.
//
// This is what Append to Clipboard and Chat about File both want, and neither
// should have to know that a .docx goes through Pandoc, a .pdf through 2htm,
// and a .cs is simply read. A format nothing can read says so plainly rather
// than returning the raw bytes, which is what the old path did: a .zip read as
// text put a screenful of rubbish on the clipboard.
sError = "";
if (!File.Exists(sPath)) {
sError = "not found";
return "";
}
string sExt = Path.GetExtension(sPath).ToLower();
string sCategory = categoryOf(sPath);

// Already text. Read it, and let the encoding detector work out how.
if (Array.IndexOf(c_aPlainSources, sExt) >= 0) {
try {
return Homer.Util.file2String(sPath);
}
catch (Exception ex) {
sError = ex.Message;
return "";
}
}

// Legacy Office and PDF: 2htm, which comes with FileDir.
// Documents Pandoc reads: Pandoc, whose plain output keeps the structure as
// indentation and blank lines rather than throwing it away.
if (sCategory == "legacy" || sCategory == "document") {
string sTemp = Path.Combine(Path.GetTempPath(),
Path.GetFileNameWithoutExtension(sPath) + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".txt");
bool bMade = false;
if (sCategory == "legacy") bMade = convertLegacy(sPath, sTemp, out sError);
else bMade = convertFile(sPath, sTemp, out sError);
if (!bMade) return "";
try {
string sText = Homer.Util.file2String(sTemp);
File.Delete(sTemp);
return sText;
}
catch (Exception ex) {
sError = ex.Message;
return "";
}
}

sError = sExt + " is not a text format";
return "";
} // toPlainText method

public static bool convertAny(string sSource, string sTarget, out string sError) {
// One file to one target, whichever engine is right for it. The caller names
// the two files and nothing else.
sError = "";
string sCategory = categoryOf(sSource);
if (sCategory == "document") return convertFile(sSource, sTarget, out sError);
if (sCategory == "legacy") return convertLegacy(sSource, sTarget, out sError);
if (sCategory == "audio" || sCategory == "video" || sCategory == "image")
return convertMedia(sSource, sTarget, out sError);
sError = "FileDir does not know what " + Path.GetExtension(sSource).ToLower()
+ " files can become.";
return false;
} // convertAny method

private static bool convertLegacy(string sSource, string sTarget, out string sError) {
// 2htm, which comes with FileDir, reads the formats Pandoc cannot: legacy
// Word, PowerPoint and Excel, and PDF. Plain text uses its -p switch; anything
// else gets its HTML.
sError = "";
string sExe = Path.Combine(Homer.Media.exeFolder(), "2htm.exe");
if (!File.Exists(sExe)) {
sError = "2htm.exe was not found in the FileDir folder, so this format cannot be converted.";
return false;
}
string sExt = Path.GetExtension(sTarget).ToLower().TrimStart('.');
string sOutDir = Path.GetDirectoryName(sTarget);
StringBuilder sbArgs = new StringBuilder();
if (sExt == "txt") sbArgs.Append("-p ");
sbArgs.Append("-f -o ");
sbArgs.Append(Homer.Util.stringQuote(sOutDir));
sbArgs.Append(" ");
sbArgs.Append(Homer.Util.stringQuote(sSource));
string sOut, sErr;
int iCode = Homer.Media.run(sExe, sbArgs.ToString(), out sOut, out sErr);
// 2htm names its own output after the source, so the file it wrote may not be
// the name asked for; it is renamed rather than left with a surprising name.
string sMade = Path.Combine(sOutDir, Path.GetFileNameWithoutExtension(sSource) + "." + (sExt == "txt" ? "txt" : "htm"));
if (File.Exists(sMade)) {
if (String.Compare(sMade, sTarget, true) != 0) {
try {
if (File.Exists(sTarget)) File.Delete(sTarget);
File.Move(sMade, sTarget);
}
catch (Exception ex) {
sError = ex.Message;
return false;
}
}
return true;
}
sError = sErr.Trim();
if (sError.Length == 0) sError = "2htm returned " + iCode + " and wrote no file.";
return false;
} // convertLegacy method

private static bool convertMedia(string sSource, string sTarget, out string sError) {
// ffmpeg works out both formats from the two file names, so the command stays
// short and the same line serves audio, video and pictures.
//
//   -y            overwrite without asking; the caller already made the name unique
//   -nostdin      never wait for a keypress, which would hang a batch for ever
//   -loglevel     errors only, since nothing reads the progress here
//   -vn           on an audio target from a video, or ffmpeg tries to carry the
//                 pictures into a file that cannot hold them
sError = "";
string sExe = Homer.Media.ffmpegProgram();
if (sExe.Length == 0) {
sError = "ffmpeg was not found, so audio, video and pictures cannot be converted. Install the media tools from the FileDir installer, or put ffmpeg.exe in the FileDir folder.";
return false;
}
string sTargetExt = Path.GetExtension(sTarget).ToLower();
bool bAudioTarget = Array.IndexOf(c_aAudioSources, sTargetExt) >= 0;
StringBuilder sbArgs = new StringBuilder();
sbArgs.Append("-y -nostdin -loglevel error -i ");
sbArgs.Append(Homer.Util.stringQuote(sSource));
if (bAudioTarget && categoryOf(sSource) == "video") sbArgs.Append(" -vn");
sbArgs.Append(" ");
sbArgs.Append(Homer.Util.stringQuote(sTarget));
string sOut, sErr;
int iCode = Homer.Media.run(sExe, sbArgs.ToString(), out sOut, out sErr);
if (iCode == 0 && File.Exists(sTarget)) return true;
sError = sErr.Trim();
if (sError.Length == 0) sError = "ffmpeg returned " + iCode + ".";
// ffmpeg is talkative when it fails; the first line is the useful one.
int iBreak = sError.IndexOf('\n');
if (iBreak > 0) sError = sError.Substring(0, iBreak).Trim();
return false;
} // convertMedia method

public static bool convertFile(string sSource, string sTarget, out string sError) {
// One file to another format.  Standalone output for the formats that need a
// document wrapper rather than a fragment, which is what makes a converted
// web page open properly instead of starting mid-sentence.
sError = "";
string sExe = pandocPath();
if (sExe.Length == 0) {
sError = "Pandoc is not installed. Run installPandoc.cmd in the FileDir folder as an administrator, or install FileDir again and leave the Pandoc box ticked.";
return false;
}
if (!File.Exists(sSource)) {
sError = "The file was not found.";
return false;
}
if (!canRead(sSource)) {
sError = "Pandoc cannot read " + Path.GetExtension(sSource).ToLower()
+ " files. Use Output to Text for that format instead, which goes through 2htm.";
return false;
}
StringBuilder sbArgs = new StringBuilder();
sbArgs.Append(Homer.Util.stringQuote(sSource));
string sExt = Path.GetExtension(sTarget).ToLower().TrimStart('.');
if (sExt == "html" || sExt == "htm" || sExt == "epub" || sExt == "tex" || sExt == "rtf")
sbArgs.Append(" --standalone");
sbArgs.Append(" -o ");
sbArgs.Append(Homer.Util.stringQuote(sTarget));
string sOut;
// The program and its arguments are handed to the process object separately
// below, so no command interpreter quoting rule applies to either.
if (!run(sExe, sbArgs.ToString(), out sOut, out sError)) return false;
if (!File.Exists(sTarget)) {
if (sError.Length == 0) sError = "Pandoc reported nothing, but wrote no file.";
return false;
}
return true;
} // convertFile method

private static bool run(string sExe, string sArguments, out string sOut, out string sError) {
// A process with NO WINDOW.  A console flashing up during a batch of twenty
// files is the kind of thing that makes a program feel broken.
sOut = "";
sError = "";
try {
ProcessStartInfo info = new ProcessStartInfo();
info.FileName = sExe;
info.Arguments = sArguments;
info.UseShellExecute = false;
info.CreateNoWindow = true;
info.RedirectStandardOutput = true;
info.RedirectStandardError = true;
using (Process process = Process.Start(info)) {
// Both streams are read before waiting: a full pipe buffer would otherwise
// deadlock the wait that follows.
sOut = process.StandardOutput.ReadToEnd();
string sErr = process.StandardError.ReadToEnd();
process.WaitForExit();
Homer.Log.command(sExe, sArguments, process.ExitCode, sErr);
if (process.ExitCode != 0) {
sError = sErr.Trim();
if (sError.Length == 0) sError = "Pandoc returned " + process.ExitCode + ".";
return false;
}
}
return true;
}
catch (Exception ex) {
sError = ex.Message;
Homer.Log.write("Could not start " + sExe + ": " + ex.Message);
return false;
}
} // run method

} // Convert class

} // Homer namespace
