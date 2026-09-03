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
using System.Text.RegularExpressions;

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
// NEW WEB PAGES ARE .htm, NOT .html.
//
// The Homer Tools convention throughout: a file this program CREATES gets .htm.
// Both spellings are read, because the world writes both, and a .html file
// already on disk is left with the name it has. Only what is written is
// decided here.
public static readonly string[,] c_aTargets = {
{"Word document", "docx"},
{"Web page", "htm"},
{"Markdown", "md"},
{"Plain text", "txt"},
{"OpenDocument text", "odt"},
{"Rich text", "rtf"},
{"EPUB book", "epub"},
{"LaTeX", "tex"},
{"reStructuredText", "rst"},
{"MediaWiki", "mediawiki"}
};

// ONE list of what Pandoc reads, used by everything.
//
// There were three, and they disagreed. toPlainText routed .bib, .jats, .opml
// and .tsv to Pandoc, and convertFile then refused them because its own list
// did not have them. The other list called .pptx and .xlsx Pandoc-readable,
// which they are not -- Pandoc writes those, it does not read them.
//
// The lists are gone. c_aPandocReadable above is the only one, and every place
// that asks "can Pandoc read this" asks it.

public static bool canRead(string sPath) {
// Whether PANDOC can read it. The name is kept because callers read well with
// it, but there is now one list behind it rather than three.
string sExt = Path.GetExtension(sPath).ToLower();
return Array.IndexOf(c_aPandocReadable, sExt) >= 0;
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

// What Pandoc actually READS. Checked against "pandoc --list-input-formats":
// it reads docx, odt, epub, html, rtf, markdown, rst, latex, csv and more, and
// it does NOT read pptx, xlsx or pdf, whatever its output list suggests.
public static readonly string[] c_aPandocReadable = {
".docx", ".odt", ".epub", ".html", ".htm", ".rtf", ".md", ".markdown",
".rst", ".tex", ".latex", ".csv", ".tsv", ".org", ".textile", ".mediawiki",
".ipynb", ".fb2", ".typ", ".jats", ".opml", ".bib"
};

// What Output Type treats as a document: everything Pandoc reads, plus the two
// Open XML formats FileDir reads itself. They are documents to a person even
// though Pandoc cannot read them, and categoryOf must agree with that or a
// .pptx would be offered no targets at all.
private static readonly string[] c_aDocumentSources = {
".docx", ".odt", ".epub", ".html", ".htm", ".rtf", ".md", ".markdown",
".rst", ".tex", ".latex", ".csv", ".tsv", ".org", ".textile", ".mediawiki",
".ipynb", ".fb2", ".typ", ".jats", ".opml", ".bib", ".txt",
".pptx", ".xlsx"
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

// Pictures ffmpeg cannot read, which ImageMagick can.
//
// HEIC is the one that matters: FFmpeg's HEIF support has been an open ticket
// for years, is described upstream as partially fixed, and depends on how the
// binary was built -- the build shipped here does not have it. Every photograph
// an iPhone takes is HEIC, so without this the format most photos now arrive in
// cannot be converted at all.
//
// The rest are real but narrower: camera raw from every maker, SVG drawings
// which need rasterising rather than decoding, Windows icons, and the long tail
// of Photoshop, GIMP and game-texture formats.
private static readonly string[] c_aMagickSources = {
".heic", ".heif", ".avif",
".cr2", ".cr3", ".nef", ".arw", ".dng", ".orf", ".rw2", ".raf", ".pef", ".srw",
".svg", ".ico", ".psd", ".xcf", ".tga", ".pcx", ".pnm", ".ppm", ".pgm", ".dds",
".jp2", ".jxl", ".exr", ".hdr"
};

public static bool readableAsText(string sPath) {
// Whether toPlainText has any engine for this file at all.
//
// Asked before falling back to a file's first line, because a first line only
// means something in a file that HAS lines. Reading the opening bytes of a
// JPEG and calling them a title is how a folder of photographs ends up named
// after binary rubbish.
string sExt = Path.GetExtension(sPath).ToLower();
if (Array.IndexOf(c_aPlainSources, sExt) >= 0) return true;
if (Array.IndexOf(c_aPandocReadable, sExt) >= 0) return true;
if (sExt == ".pptx" || sExt == ".xlsx" || sExt == ".pdf") return true;
if (Array.IndexOf(c_aLegacySources, sExt) >= 0) return true;
return false;
} // readableAsText method

public static string categoryOf(string sPath) {
string sExt = Path.GetExtension(sPath).ToLower();
// Order matters: .pptx and .xlsx are documents to a person, but Pandoc cannot
// read either, so they are their own category and are extracted here first.
// Calling them documents offered them ten targets and then refused all ten.
// A TABLE FIRST. .csv, .tsv, .inix and .xlsx all hold rows and columns, and
// converting one to another should keep them. Pandoc reads .csv and .tsv but
// writes neither, has never heard of .inix, and cannot read .xlsx at all --
// so this category is handled by Homer.Table, which reads all four and writes
// the ones Pandoc cannot.
if (Homer.Table.canRead(sPath) && sExt != ".md" && sExt != ".markdown") return "table";
if (sExt == ".pptx") return "openxml";
if (sExt == ".pdf") return "pdf";
if (Array.IndexOf(c_aDocumentSources, sExt) >= 0) return "document";
if (Array.IndexOf(c_aLegacySources, sExt) >= 0) return "legacy";
if (Array.IndexOf(c_aAudioSources, sExt) >= 0) return "audio";
if (Array.IndexOf(c_aVideoSources, sExt) >= 0) return "video";
if (Array.IndexOf(c_aImageSources, sExt) >= 0) return "image";
// A picture is a picture whichever tool reads it, so the same targets are
// offered and convertMedia decides who does the work.
if (Array.IndexOf(c_aMagickSources, sExt) >= 0) return "image";
return "";
} // categoryOf method

public static string[,] targetsFor(string sPath) {
// Name and extension pairs, for a Pick list. The source's own extension is
// left out further down, since converting a file to what it already is has no
// meaning.
string sCategory = categoryOf(sPath);
if (sCategory == "document") return c_aTargets;
if (sCategory == "table")
// Everything that holds rows and columns. Markdown is included because a pipe
// table is how this reaches Word, a web page and OpenDocument.
return new string[,] {
{"Inix records", "inix"},
{"Comma separated", "csv"},
{"Tab separated", "tsv"},
{"Markdown table", "md"},
{"Word document", "docx"},
{"Web page", "htm"},
{"OpenDocument text", "odt"}
};
if (sCategory == "openxml" || sCategory == "pdf")
// Extracted first, then Pandoc makes anything from that. A PDF becomes rich
// Markdown with its headings, lists and tables, so Word and a web page are
// worth offering rather than only flat text.
return new string[,] {
{"Markdown", "md"},
{"Plain text", "txt"},
{"Web page", "htm"},
{"Word document", "docx"},
{"OpenDocument text", "odt"},
{"Rich text", "rtf"}
};
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
{"TIFF image", "tif"},
// Written by ImageMagick only. Offered whatever the source, because turning a
// photograph into an icon is a real thing to want and ffmpeg cannot do it.
{"Windows icon", "ico"},
{"AVIF image", "avif"}
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
// A .html file is offered "Web page" and would get a .htm; the two are the same
// format under two spellings, so neither is offered to the other.
if (sOwn == "html" && sExt == "htm") continue;
if (sOwn == "htm" && sExt == "html") continue;
lsLabels.Add(aTargets[i, 0] + " (" + sExt + ")");
lsExts.Add(sExt);
}
aExtensions = lsExts.ToArray();
return lsLabels.ToArray();
} // targetLabelsFor method

// Extensions that are already text and can simply be read.
private static readonly string[] c_aPlainSources = {
// Play lists and shortcuts are plain text and were missing, so Append to
// Clipboard refused an .m3u -- the very file the player commands produce.
".m3u", ".m3u8", ".pls", ".url", ".cue", ".srt", ".vtt", ".sub", ".ass",
".txt", ".md", ".markdown", ".csv", ".tsv", ".log", ".ini", ".inix", ".json", ".xml",
".yml", ".yaml", ".cs", ".js", ".py", ".ps1", ".cmd", ".bat", ".vbs", ".sql",
".c", ".cpp", ".h", ".java", ".php", ".rb", ".go", ".rs", ".ts", ".css",
".jss", ".jsh", ".jsd", ".jkm", ".iss", ".bas", ".vb", ".sh", ".tex", ".rst"
};

// Formats worth putting on the clipboard as HTML rather than flat text.
// Anything Pandoc can turn into a web page, and the two Open XML formats that
// go through it as Markdown first.
private static readonly string[] c_aRichForms = {
".docx", ".odt", ".rtf", ".html", ".htm", ".epub", ".md", ".markdown",
".rst", ".tex", ".csv", ".tsv", ".inix", ".xlsx", ".pdf"
};

public static bool hasRichForm(string sPath) {
return Array.IndexOf(c_aRichForms, Path.GetExtension(sPath).ToLower()) >= 0;
} // hasRichForm method

public static string toHtmlFragment(string sPath, out string sError) {
// A file as HTML, for the clipboard.
//
// A fragment rather than a whole page: the clipboard wants the body, and a
// <html><head> wrapper pasted into a mail message is at best ignored and at
// worst shown as markup. So Pandoc is NOT given --standalone here, which is the
// one place in FileDir where that is right.
sError = "";
string sTemp = Path.Combine(Path.GetTempPath(),
Path.GetFileNameWithoutExtension(sPath) + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".htm");
try {
// A table format becomes a real HTML table rather than a pipe table in a
// paragraph, which is the whole reason to bother with the rich form.
if (categoryOf(sPath) == "table") {
Homer.Table table = Homer.Table.read(sPath, out sError);
if (table == null || table.fieldCount() == 0) return "";
if (!table.write(sTemp, out sError)) return "";
}
else {
string sText = toPlainText(sPath, out sError);
if (sText.Length == 0) return "";
string sMd = Path.ChangeExtension(sTemp, ".md");
File.WriteAllText(sMd, sText, new UTF8Encoding(true));
bool bMade = convertFile(sMd, sTemp, out sError);
try { File.Delete(sMd); }
catch (Exception) { }
if (!bMade) return "";
}
string sHtml = Homer.Util.file2String(sTemp);
File.Delete(sTemp);
return sHtml;
}
catch (Exception ex) {
sError = ex.Message;
return "";
}
} // toHtmlFragment method

public static string htmlClipboardFormat(string sHtml) {
// Windows does not take bare HTML on the clipboard. It wants a header naming
// the byte offsets of the fragment within it, and the offsets have to be right
// or the paste is empty -- which is why this is built rather than guessed.
//
// The header is a fixed shape with ten-digit numbers, so its own length is
// known before the numbers are filled in.
string sPrefix = "<html><body><!--StartFragment-->";
string sSuffix = "<!--EndFragment--></body></html>";
string sHeader = "Version:0.9\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\n"
+ "StartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\n";
int iHeader = String.Format(sHeader, 0, 0, 0, 0).Length;
// Counted in BYTES, not characters: Windows reads the offsets against the
// UTF-8 bytes, so an accented letter would shift every number.
int iStartHtml = iHeader;
int iStartFragment = iStartHtml + Encoding.UTF8.GetByteCount(sPrefix);
int iEndFragment = iStartFragment + Encoding.UTF8.GetByteCount(sHtml);
int iEndHtml = iEndFragment + Encoding.UTF8.GetByteCount(sSuffix);
return String.Format(sHeader, iStartHtml, iEndHtml, iStartFragment, iEndFragment)
+ sPrefix + sHtml + sSuffix;
} // htmlClipboardFormat method

public static string toPlainText(string sPath, out string sError) {
// The text of a file, whatever the file is, by whichever engine can read it.
//
// THE ORDER MATTERS, AND IT IS DELIBERATE.
//
// A goal of the reborn Homer Tools is that nothing fundamental depends on a
// commercial product being installed. 2htm reaches Office through COM for the
// legacy formats, so it is LAST, not first -- which is what it used to be. On a
// computer without Office it returned success and produced nothing, and the
// Say Contents command simply said nothing at all.
//
//   1. Already text        -- read it.
//   2. Pandoc              -- docx, odt, epub, html, rtf, markdown, rst,
//                             latex, csv and the rest it reads. Free, and
//                             already installed for the conversion commands.
//   3. Open XML, read here -- pptx and xlsx, which Pandoc does not read. They
//                             are zip archives of XML, and FileDir already
//                             carries a zip library, so no Office and no new
//                             package.
//   4. 2htm                -- pdf, and legacy .doc, .ppt and .xls. The last
//                             two reach Office through COM, and there is no
//                             reasonable alternative for a 1997 .doc file.
//
// AND IT SAYS WHY WHEN IT FAILS. Silence was the real fault: a person pressed a
// key and nothing happened, with nothing to read and nothing in the log.
sError = "";
if (!File.Exists(sPath)) {
sError = "the file was not found";
return "";
}
string sExt = Path.GetExtension(sPath).ToLower();

// 1. Already text.
if (Array.IndexOf(c_aPlainSources, sExt) >= 0) {
try {
return Homer.Util.file2String(sPath);
}
catch (Exception ex) {
sError = ex.Message;
return "";
}
}

// 2. Pandoc, for everything it reads.
if (Array.IndexOf(c_aPandocReadable, sExt) >= 0) {
if (!havePandoc()) {
sError = "Pandoc is not installed, and it is what reads " + sExt + " files. "
+ "Run installPandoc.cmd in the FileDir folder as an administrator, or install FileDir again and leave the Pandoc box ticked.";
return "";
}
string sViaPandoc = throughFile(sPath, ".txt", out sError);
if (sViaPandoc.Length > 0) return sViaPandoc;
if (sError.Length == 0) sError = "Pandoc read " + Path.GetFileName(sPath) + " but found no text in it.";
return "";
}

// 3. Open XML, read here rather than through anything else.
if (sExt == ".xlsx") {
// A spreadsheet read for its text becomes a Markdown table, so the rows and
// columns survive being read aloud instead of arriving as a heap of strings.
Homer.Table table = Homer.Table.read(sPath, out sError);
if (table != null && table.fieldCount() > 0) return table.markdownText();
}
if (sExt == ".pptx" || sExt == ".xlsx") {
string sOpenXml = openXmlText(sPath, out sError);
if (sOpenXml.Length > 0) return sOpenXml;
if (sError.Length == 0) sError = "No text was found inside " + Path.GetFileName(sPath) + ".";
return "";
}

// 4. PDF, through the same reader EdSharp uses: PyMuPDF4LLM, which reads a
// PDF's own structure -- font sizes become headings, bullet runs become lists,
// ruled areas become tables -- and writes Markdown. Plain text from a PDF
// throws away everything a screen reader user navigates by.
//
// Word is not involved. 2htm reads a PDF through Word's PDF Reflow, so on a
// machine without Word it produces nothing, and it produced nothing on a
// machine WITH Word too when it could not load one of its own assemblies.
if (sExt == ".pdf") {
string sRich = pdfMarkdown(sPath, out sError);
if (sRich.Length > 0) return sRich;
// 2htm is still tried, in case Word can do what the reader could not.
string sViaTwoHtm = throughTwoHtm(sPath, out sError);
if (sViaTwoHtm.Length > 0) return sViaTwoHtm;
if (sError.Length == 0)
sError = "No text could be got out of this PDF. If it is a scan of images it needs "
+ "optical character recognition rather than conversion.";
return "";
}

// 5. 2htm, for the legacy Office formats only.
if (sExt == ".doc" || sExt == ".ppt" || sExt == ".xls") {
string sLegacy = throughTwoHtm(sPath, out sError);
if (sLegacy.Length > 0) return sLegacy;
if (sError.Length == 0)
sError = "2htm could not get any text out of " + Path.GetFileName(sPath) + ". "
+ "The 1997 Office formats need Microsoft Office installed. Saving the file as "
+ (sExt == ".doc" ? ".docx" : sExt == ".ppt" ? ".pptx" : ".xlsx")
+ " avoids that, and FileDir reads those without Office.";
return "";
}

sError = sExt + " is not a format FileDir can read as text.";
return "";
} // toPlainText method

public static string pythonProgram() {
// The interpreter that has the PDF reader, or the best one found.
//
// installPdfTools.cmd records which interpreter it installed the package with,
// because a machine may carry several Pythons and only one of them can import
// it. That record is read first. Windows also answers "where python" with an
// app execution alias under WindowsApps which is NOT Python -- it advertises
// the Microsoft Store and exits -- so any path through WindowsApps is rejected.
try {
string sRecord = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
Path.Combine("FileDir", Path.Combine("logs", "FileDir_python.txt")));
if (File.Exists(sRecord)) {
string sNoted = File.ReadAllText(sRecord).Trim();
if (sNoted.Length > 0 && File.Exists(sNoted)) return sNoted;
}
}
catch (Exception) {
}
string sFound = Homer.Media.findTool("python");
if (sFound.Length > 0 && sFound.IndexOf("WindowsApps", StringComparison.OrdinalIgnoreCase) < 0)
return sFound;
foreach (string sFolder in new string[] {
Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Programs\Python\Python313"),
Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Programs\Python\Python312"),
Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Programs\Python\Python311"),
Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Python313"),
Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Python312"),
Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Python311")}) {
string sTry = Path.Combine(sFolder, "python.exe");
if (File.Exists(sTry)) return sTry;
}
return "";
} // pythonProgram method

public static string pdfMarkdown(string sPath, out string sError) {
// A PDF as rich Markdown, by pdfRich.py and PyMuPDF4LLM.
sError = "";
string sScript = Path.Combine(Homer.Media.exeFolder(), "pdfRich.py");
if (!File.Exists(sScript)) {
sError = "pdfRich.py is not in the FileDir folder.";
return "";
}
string sPython = pythonProgram();
if (sPython.Length == 0) {
sError = "Python is not installed, and the PDF reader needs it. "
+ "Install Python from python.org, then run installPdfTools.cmd in the FileDir folder.";
return "";
}
string sTemp = Path.Combine(Path.GetTempPath(),
Path.GetFileNameWithoutExtension(sPath) + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".md");
string sOut, sErr;
int iCode = Homer.Media.run(sPython,
Homer.Util.stringQuote(sScript) + " " + Homer.Util.stringQuote(sPath) + " " + Homer.Util.stringQuote(sTemp),
out sOut, out sErr);
// pdfRich writes its reasons beside the target, and removes that log when it
// succeeds. So a log still there is the explanation, in its own words.
string sLog = sTemp + ".log";
if (iCode != 0 || !File.Exists(sTemp)) {
if (File.Exists(sLog)) {
try {
sError = firstMeaningful(File.ReadAllText(sLog));
File.Delete(sLog);
}
catch (Exception) {
}
}
if (sError.Length == 0) sError = firstLine((sOut + "\n" + sErr).Trim());
if (sError.Length == 0) sError = "The PDF reader returned " + iCode + ".";
return "";
}
try {
string sText = Homer.Util.file2String(sTemp);
File.Delete(sTemp);
if (File.Exists(sLog)) File.Delete(sLog);
return sText;
}
catch (Exception ex) {
sError = ex.Message;
return "";
}
} // pdfMarkdown method

private static string firstMeaningful(string sText) {
// The first line that says something went wrong, or the last line with words
// on it. pdfRich's log opens with the environment before it reaches the point.
string sBest = "";
foreach (string sLine in sText.Replace("\r\n", "\n").Split('\n')) {
string sTrim = sLine.Trim();
if (sTrim.Length == 0) continue;
if (sTrim.StartsWith("FAILED")) return sTrim;
sBest = sTrim;
}
return sBest;
} // firstMeaningful method

private static bool fromText(string sText, string sTarget, out string sError) {
// Text or Markdown already in hand, written out as the target asks.
//
// A .txt target is simply the text. Anything else goes through Pandoc, which
// is told the input is Markdown -- because what the PDF reader and the Open
// XML reader produce IS Markdown, and reading it as such keeps the headings
// and tables rather than flattening them into one paragraph.
sError = "";
string sExt = Path.GetExtension(sTarget).ToLower().TrimStart('.');
try {
if (sExt == "txt" || sExt == "md" || sExt == "markdown") {
File.WriteAllText(sTarget, sText, new UTF8Encoding(true));
return true;
}
}
catch (Exception ex) {
sError = ex.Message;
return false;
}
string sExe = pandocPath();
if (sExe.Length == 0) {
sError = "Pandoc is not installed, and it is what makes a " + sExt + " file from the text.";
return false;
}
string sTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N").Substring(0, 8) + ".md");
try {
File.WriteAllText(sTemp, sText, new UTF8Encoding(true));
}
catch (Exception ex) {
sError = ex.Message;
return false;
}
bool bMade = convertFile(sTemp, sTarget, out sError);
try { File.Delete(sTemp); }
catch (Exception) { }
return bMade;
} // fromText method

private static string throughTwoHtm(string sPath, out string sError) {
// 2htm into a temporary text file, read back.
sError = "";
string sExe = Path.Combine(Homer.Media.exeFolder(), "2htm.exe");
if (!File.Exists(sExe)) {
sError = "2htm.exe is not in the FileDir folder.";
return "";
}
string sTemp = Path.Combine(Path.GetTempPath(),
Path.GetFileNameWithoutExtension(sPath) + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".txt");
if (!convertLegacy(sPath, sTemp, out sError)) return "";
try {
string sText = Homer.Util.file2String(sTemp);
File.Delete(sTemp);
return sText;
}
catch (Exception ex) {
sError = ex.Message;
return "";
}
} // throughTwoHtm method

private static string throughFile(string sPath, string sTargetExt, out string sError) {
// Convert to a temporary file and read it back. Pandoc writes files rather
// than talking on a pipe, and a temporary file avoids every question about
// encodings on the way through.
sError = "";
string sTemp = Path.Combine(Path.GetTempPath(),
Path.GetFileNameWithoutExtension(sPath) + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + sTargetExt);
if (!convertFile(sPath, sTemp, out sError)) return "";
try {
string sText = Homer.Util.file2String(sTemp);
File.Delete(sTemp);
return sText;
}
catch (Exception ex) {
sError = ex.Message;
return "";
}
} // throughFile method

public static string openXmlText(string sPath, out string sError) {
// The text inside a .pptx or .xlsx, read straight out of the archive.
//
// Both are zip files full of XML, and FileDir already carries SharpZipLib for
// its archive commands, so this needs no Office, no COM and no new package.
// Pandoc does not read either format, which is why this exists.
//
//   pptx -- every ppt/slides/slideN.xml, and the text is in <a:t> elements.
//   xlsx -- xl/sharedStrings.xml, which holds every string in the workbook in
//           <t> elements. Numbers live in the sheets and are not gathered:
//           what a person wants read aloud from a spreadsheet is its words.
sError = "";
StringBuilder sbText = new StringBuilder();
try {
using (ICSharpCode.SharpZipLib.Zip.ZipFile zip = new ICSharpCode.SharpZipLib.Zip.ZipFile(sPath)) {
System.Collections.Generic.List<string> lsNames = new System.Collections.Generic.List<string>();
foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry entry in zip) {
if (!entry.IsFile) continue;
string sName = entry.Name.Replace('\\', '/').ToLower();
bool bWanted = sName.StartsWith("ppt/slides/slide") && sName.EndsWith(".xml");
if (sName == "xl/sharedstrings.xml") bWanted = true;
if (bWanted) lsNames.Add(entry.Name);
}
// Slide 2 must not come before slide 10 by name alone, and it does when the
// names are compared as text. The number decides.
lsNames.Sort(delegate(string sLeft, string sRight) {
return slideNumber(sLeft).CompareTo(slideNumber(sRight));
});
foreach (string sName in lsNames) {
ICSharpCode.SharpZipLib.Zip.ZipEntry entry = zip.GetEntry(sName);
if (entry == null) continue;
string sXml;
using (Stream stream = zip.GetInputStream(entry))
using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
sXml = reader.ReadToEnd();
string sPart = xmlRunText(sXml);
if (sPart.Trim().Length == 0) continue;
if (sbText.Length > 0) sbText.Append("\r\n\r\n");
sbText.Append(sPart.Trim());
}
}
}
catch (Exception ex) {
sError = ex.Message;
return "";
}
return sbText.ToString();
} // openXmlText method

private static int slideNumber(string sName) {
// The digits in "ppt/slides/slide12.xml", or a large number for anything else
// so that shared strings sort last rather than first.
Match digits = Regex.Match(sName, @"slide(\d+)\.xml", RegexOptions.IgnoreCase);
if (!digits.Success) return int.MaxValue;
int iNumber = 0;
int.TryParse(digits.Groups[1].Value, out iNumber);
return iNumber;
} // slideNumber method

private static string xmlRunText(string sXml) {
// The text of every <a:t> and <t> element, which is where Open XML keeps the
// words. Written by hand rather than with an XML parser because a namespace
// declaration missing from a fragment would stop a parser dead, and this only
// has to find text between two known tags.
StringBuilder sb = new StringBuilder();
foreach (Match run in Regex.Matches(sXml, @"<(?:a:)?t(?:\s[^>]*)?>(.*?)</(?:a:)?t>", RegexOptions.Singleline)) {
string sRun = run.Groups[1].Value;
sRun = sRun.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"")
.Replace("&apos;", "'").Replace("&amp;", "&");
sb.Append(sRun);
// A paragraph end in either format closes the run that precedes it.
sb.Append(" ");
}
// Line breaks where the markup had them, so a slide does not arrive as one
// long sentence.
string sText = sb.ToString();
sText = Regex.Replace(sText, "[ \t]{2,}", " ");
return sText.Trim();
} // xmlRunText method

public static bool convertAny(string sSource, string sTarget, out string sError) {
// One file to one target, whichever engine is right for it. The caller names
// the two files and nothing else.
sError = "";
string sCategory = categoryOf(sSource);
if (sCategory == "document") return convertFile(sSource, sTarget, out sError);
if (sCategory == "legacy") return convertLegacy(sSource, sTarget, out sError);
if (sCategory == "table") {
// Read as rows and columns and written back as rows and columns, so a
// conversion between table formats keeps the table.
Homer.Table table = Homer.Table.read(sSource, out sError);
if (table == null) return false;
if (table.fieldCount() == 0) {
if (sError.Length == 0) sError = "no table was found in " + Path.GetFileName(sSource);
return false;
}
return table.write(sTarget, out sError);
}
if (sCategory == "openxml" || sCategory == "pdf") {
// Extract once, then let Pandoc make the target from that. One rich
// conversion serves every target, which is what EdSharp does with a PDF.
string sText = toPlainText(sSource, out sError);
if (sText.Length == 0) return false;
return fromText(sText, sTarget, out sError);
}
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

// 2HTM REPORTS FAILURE IN ITS OUTPUT AND STILL RETURNS ZERO.
//
// This is what made the fault so hard to see. On a machine where 2htm cannot
// load one of its assemblies it prints
//
//   <file>: Could not load file or assembly 'System.Memory, Version=4.0.2.0'
//   Failed to convert 1 file:
//
// and exits with code 0. Every caller that trusted the exit code concluded all
// was well, and the person got silence. So its words are read, not its code.
string sSaid = (sOut + "\n" + sErr).Trim();
if (sSaid.Length > 0) {
if (sSaid.IndexOf("Could not load file or assembly", StringComparison.OrdinalIgnoreCase) >= 0) {
sError = "2htm is missing an assembly it needs, so it cannot convert anything. "
+ "Copy System.Memory.dll into the FileDir folder, beside 2htm.exe. "
+ "It reported: " + firstLine(sSaid);
return false;
}
if (sSaid.IndexOf("Failed to convert", StringComparison.OrdinalIgnoreCase) >= 0) {
sError = "2htm reported: " + firstLine(sSaid);
return false;
}
}

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

private static string firstLine(string sText) {
// The first line with anything on it. A tool that fails is often talkative,
// and the first line is nearly always the one that says what went wrong.
foreach (string sLine in sText.Replace("\r\n", "\n").Split('\n'))
if (sLine.Trim().Length > 0) return sLine.Trim();
return sText.Trim();
} // firstLine method

private static bool convertWithMagick(string sSource, string sTarget, out string sError) {
// ImageMagick, for the pictures ffmpeg cannot reach.
//
//   magick "in.heic" "out.jpg"
//
// The command works out both formats from the two names, as ffmpeg does, so
// nothing else has to be passed. Two exceptions are worth the extra option.
sError = "";
string sExe = Homer.Media.magickProgram();
if (sExe.Length == 0) {
string sExt = Path.GetExtension(sSource).ToLower();
sError = "ImageMagick was not found, and it is what reads " + sExt + " files. "
+ "Run installImageTools.cmd in the FileDir folder as an administrator, or install "
+ "FileDir again and tick the image tools box. ffmpeg cannot read this format.";
return false;
}
StringBuilder sbArgs = new StringBuilder();
string sSourceExt = Path.GetExtension(sSource).ToLower();
string sWantExt = Path.GetExtension(sTarget).ToLower();

// A drawing has no size of its own until it is drawn, and ImageMagick's default
// of 72 dots per inch turns a full page into a postage stamp.
if (sSourceExt == ".svg") sbArgs.Append("-density 300 ");

sbArgs.Append(Homer.Util.stringQuote(sSource));

// A raw file, a PSD and an animated GIF all hold several images, and without
// this every one of them is written as its own numbered file. The first is the
// picture; the rest are thumbnails and layers.
if (sSourceExt != ".gif" && sWantExt != ".gif" && sWantExt != ".ico")
sbArgs.Append("[0]");

// An icon holds several sizes, and Windows picks the one it needs. Written
// without this, it holds whatever size the source happened to be.
if (sWantExt == ".ico") sbArgs.Append(" -define icon:auto-resize=256,128,64,48,32,16");
// A format with no transparency needs something behind a transparent picture,
// or the clear parts come out black.
if (sWantExt == ".jpg" || sWantExt == ".jpeg" || sWantExt == ".bmp")
sbArgs.Append(" -background white -flatten");

sbArgs.Append(" ");
sbArgs.Append(Homer.Util.stringQuote(sTarget));
string sOut, sErr;
int iCode = Homer.Media.run(sExe, sbArgs.ToString(), out sOut, out sErr);
if (iCode == 0 && File.Exists(sTarget)) return true;
sError = firstLine((sErr + "\n" + sOut).Trim());
if (sError.Length == 0) sError = "ImageMagick returned " + iCode + ".";
return false;
} // convertWithMagick method

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
string sSourceExt = Path.GetExtension(sSource).ToLower();
string sWantExt = Path.GetExtension(sTarget).ToLower();

// WHICH TOOL. ImageMagick when either end is a format ffmpeg cannot handle,
// ffmpeg otherwise. ffmpeg stays the default for the ordinary picture formats
// because it is already installed and already ticked; ImageMagick is asked for
// only when it is the only one that can do the job.
bool bNeedMagick = Array.IndexOf(c_aMagickSources, sSourceExt) >= 0
|| Array.IndexOf(c_aMagickSources, sWantExt) >= 0;
if (bNeedMagick) return convertWithMagick(sSource, sTarget, out sError);

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

// PLAIN TEXT HAS TO BE ASKED FOR BY NAME.
//
// Pandoc has no writer registered for the .txt extension, and rather than
// refuse it falls back to MARKDOWN. So asking for text gave back "# Heading",
// "**bold**" and "[link](https://...)" -- which is why Say Contents on a web
// page sounded like it was reading markup aloud. It was.
//
// "-t plain" is the writer that produces prose: no hashes, no asterisks, and a
// link reduced to the words a person would read. Everything that wants text
// comes through here -- Say Contents, Append to Clipboard, Translate File and
// Chat about File -- so this one line settles all four.
if (sExt == "txt" || sExt == "text") sbArgs.Append(" -t plain");
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
