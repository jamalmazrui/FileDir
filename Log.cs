// Log.cs -- the session log, in the Homer Tools convention.
//
// Copyright 2006-2026 by Jamal Mazrui
// MIT License. See License.md, which carries the terms in full.
//
// WHAT THIS IS FOR
//
// One file per session, at
//
//   %LOCALAPPDATA%\<App>\logs\<App>_yyyyMMdd_HHmmss.log
//
// beside the setup log the installer writes. It opens with the version and the
// environment, and every outside command the program runs adds a line with its
// exit code -- so a conversion that failed can be diagnosed from the log rather
// than guessed at. The newest thirty session logs are kept and older ones are
// pruned, because a log that fills a profile folder is a fault of its own.
//
// This is the same arrangement EdSharp uses, in the same place, with the same
// naming, and reached by the same key: Copy Log on Control+F12.
//
// WHY THE SHARED CLASSES WRITE HERE TOO
//
// Homer.Media, Homer.Convert and Homer.Ollama all run outside programs, and
// what those programs said when they failed is exactly what a log is for. They
// cannot reach the application's own class, so the path lives here, in the
// namespace they already share. The application sets it once at startup.
//
// NOTHING HERE EVER THROWS. A missing folder, a locked file or a read-only
// profile must not affect the work being logged. A log is a courtesy; the work
// is the point.

using System;
using System.IO;
using System.Text;

namespace Homer {

public static class Log {

// The file for this session, or an empty string when logging could not start.
public static string sFile = "";

public static string start(string sAppName, string sVersion, string sProgram, string[] aArguments) {
// Open the log and record the environment, before anything can fail. Every
// setting that could explain a surprising result belongs here: which copy ran,
// which version, with what on the command line, and on what.
try {
string sDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                           Path.Combine(sAppName, "logs"));
Directory.CreateDirectory(sDir);
sFile = Path.Combine(sDir, sAppName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
StringBuilder sb = new StringBuilder();
sb.Append(stamp()).Append(sAppName).Append(" ").Append(sVersion).Append(" starting.\r\n");
sb.Append(stamp()).Append("Program: ").Append(sProgram).Append("\r\n");
sb.Append(stamp()).Append("Arguments: ").Append(aArguments == null ? "" : String.Join(" ", aArguments)).Append("\r\n");
sb.Append(stamp()).Append("Windows: ").Append(Environment.OSVersion.ToString())
  .Append(", 64-bit process: ").Append(Environment.Is64BitProcess).Append("\r\n");
sb.Append(stamp()).Append(".NET: ").Append(Environment.Version.ToString()).Append("\r\n");
sb.Append(stamp()).Append("User: ").Append(Environment.UserName).Append("\r\n");
sb.Append(stamp()).Append("Working directory: ").Append(Environment.CurrentDirectory).Append("\r\n");
File.AppendAllText(sFile, sb.ToString());
prune(sDir, sAppName);
}
catch (Exception) {
sFile = "";
}
return sFile;
} // start method

private static void prune(string sDir, string sAppName) {
// The newest thirty, and no more. Sorted by name, which for this naming is
// sorted by time.
try {
string[] aOld = Directory.GetFiles(sDir, sAppName + "_2*.log");
Array.Sort(aOld);
for (int i = 0; i < aOld.Length - 30; i++) {
try { File.Delete(aOld[i]); }
catch (Exception) { }
}
}
catch (Exception) {
}
} // prune method

private static string stamp() {
return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  ";
} // stamp method

public static void write(string sMessage) {
// One timestamped line. Never throws.
if (sFile == null || sFile.Length == 0) return;
try { File.AppendAllText(sFile, stamp() + sMessage + "\r\n"); }
catch (Exception) { }
} // write method

public static void command(string sProgram, string sArguments, int iExitCode, string sError) {
// One outside command, with its exit code and, when it failed, the first line
// of what it complained about. That first line is nearly always the useful
// one, and a whole page of ffmpeg output would bury the rest of the log.
StringBuilder sb = new StringBuilder();
sb.Append("Command: ").Append(sProgram);
if (sArguments != null && sArguments.Length > 0) sb.Append(" ").Append(sArguments);
sb.Append("  -- exit code ").Append(iExitCode);
write(sb.ToString());
if (iExitCode == 0 || sError == null || sError.Trim().Length == 0) return;
string sFirst = sError.Trim().Replace("\r\n", "\n");
int iBreak = sFirst.IndexOf('\n');
if (iBreak > 0) sFirst = sFirst.Substring(0, iBreak).Trim();
write("  Error: " + sFirst);
} // command method

} // Log class

} // Homer namespace
