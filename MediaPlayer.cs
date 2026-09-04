// MediaPlayer.cs -- the FileDir Media Player dialog.
//
// WHAT IT IS
//
// A dialog that plays a queue of tracks. mpv does the playing, with no window,
// no keys and no focus of its own; every control here is an ordinary Windows
// control, and each one turns into a command down mpv's pipe. Mpv.cs is that
// plumbing.
//
// HOW IT DIFFERS FROM PLAY LIST
//
// Play List (Control+Shift+L) hands everything to mpv and steps out of the way:
// mpv's window takes the foreground and mpv's own keys work. That is still
// there, unchanged, and it is the quickest way to start something and walk off.
//
// Play Queue (Control+Shift+Q) keeps the list here, with the names the tracks
// arrived with -- for a document, the words somebody wrote about each link.
//
// WHY IT LOOKS LIKE THE OTHER HOMER DIALOGS
//
// urlFido, HomerScribe and DbDo have settled on a shape, and there is no reason
// for a player to be the odd one out:
//
//   * Ordinary controls, in the order they are used. A list, then what is
//     playing, then the commands, then the settings, then the extras.
//   * Bands. Controls that belong together sit on one row, so Tab goes from a
//     field to the thing that acts on it rather than past the whole dialog.
//   * A mnemonic on every control, all of them different, so anything can be
//     reached with one Alt press whatever has focus.
//   * A tip on every control, written as a sentence, which the status line
//     shows on focus and the Help button lists.
//   * The standard settings band before the button row, ending with Use
//     configuration, which is what makes the settings stick.
//   * Buttons that DO something live in the dialog; the row along the bottom is
//     only Close and Help, because a button in that row closes the dialog.
//
// A MEDIA PLAYER HAS A THOUSAND SETTINGS. This one has four, and every one of
// them is a thing a listener changes while listening: which track, where in it,
// how loud, how fast. Everything else mpv can do is still there for the person
// who wants Play List and mpv's own window.
//
// SPEECH
//
// The screen reader announces the dialog, the control that has focus, the list
// line under the cursor, and the value in a spin box when it changes. So this
// dialog says none of those. It speaks what the reader cannot know: the track
// when playback moves on by itself, the position when asked, the end of the
// queue, and the result of a command with nothing visible to show for it --
// "Address copied", "Stopped". Short, present tense, no punctuation, which is
// the DbDo house style.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

// THE FILEDIR NAMESPACE, not the global one. App, Lbc and the rest of FileDir
// live in it, and a class outside it cannot see them. Homer's own classes are
// named in full -- Homer.LbcDialog, Homer.Mpv, Homer.Util -- which is how
// Dialogs.cs and FileDir.cs refer to them.
namespace FileDir {

// One thing to play: what to hand mpv, and what to call it on screen.
public class MediaTrack {
public string sName;
public string sTarget;

public MediaTrack(string sTrackName, string sTrackTarget) {
sName = sTrackName == null ? "" : sTrackName.Trim();
sTarget = sTrackTarget == null ? "" : sTrackTarget.Trim();
if (sName.Length == 0) sName = shortName(sTarget);
}

// shortName: something to call a track that arrived with no name. The last
// part of an address, tidied, beats the whole address in a list.
public static string shortName(string sTarget) {
if (sTarget == null || sTarget.Length == 0) return "Untitled";
string sText = sTarget;
int iQuery = sText.IndexOf('?');
if (iQuery > 0) sText = sText.Substring(0, iQuery);
sText = sText.TrimEnd('/');
int iSlash = sText.LastIndexOfAny(new char[] { '/', '\\' });
if (iSlash >= 0 && iSlash < sText.Length - 1) sText = sText.Substring(iSlash + 1);
try { sText = Uri.UnescapeDataString(sText); }
catch (Exception) { }
if (sText.Length == 0) return sTarget;
return sText;
}
} // MediaTrack class

public static class MediaPlayer {

// Where the four settings live between sessions, in the section of
// FileDir.ini the .inix overlay can override like any other.
private const string c_sSection = "MediaPlayer";

// run: open the player on a queue of tracks. Returns when the dialog closes,
// by which time mpv has stopped.
public static void run(IWin32Window owner, string sTitle, string sSource, List<MediaTrack> lsTracks) {
if (lsTracks == null || lsTracks.Count == 0) { App.say("0 tracks", true); return; }

string sMpv = Homer.Media.mpvProgram();
if (sMpv.Length == 0) {
Lbc.Show("mpv is not installed, so there is nothing to play with.\r\n\r\n"
+ "Run installMpv.cmd in the FileDir folder, or install FileDir again and tick the mpv box.",
"Media Player");
return;
}

// Settings as they were left. Volume starts below full so the media does not
// drown the screen reader.
int iVolume = readNumber("volume", Homer.Mpv.c_iDefaultVolume, 0, 130);
int iSpeed = readNumber("speed", 100, 25, 400);
bool bRepeat = readFlag("repeat", false);
bool bAnnounce = readFlag("announce", true);
bool bUseConfig = readFlag("useConfiguration", true);

Homer.Mpv player = new Homer.Mpv(sMpv, Homer.Media.findInstalled("yt-dlp"));
string sError;
if (!player.start(out sError)) {
Homer.Log.write("Media Player: mpv would not start. " + sError);
Lbc.Show("The player would not start.\r\n\r\n" + sError, "Media Player");
player.Dispose();
return;
}

List<string> lsNames = new List<string>();
for (int i = 0; i < lsTracks.Count; i++) {
lsNames.Add((i + 1).ToString(CultureInfo.InvariantCulture) + ". " + lsTracks[i].sName);
}

// State the dialog keeps for itself. Everything else is asked of mpv.
int iAnnounced = -1;
bool bWasPlaying = false;
bool bEndSaid = false;

Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, owner);

// ---- what is in the queue, and what is playing ----

// THE LABEL SAYS WHAT IS IN THE LIST, which is McTwit's habit and a good one:
// the title bar is read once when the dialog opens, but a screen reader reads
// the label every time the cursor enters the list.
string sQueueLabel = "&Queue, " + Homer.Util.stringPlural("track", lsTracks.Count);
if (!string.IsNullOrEmpty(sSource)) sQueueLabel = sQueueLabel + " from " + sSource;
ListBox lstQueue = dlg.addPickBox(sQueueLabel + ", Enter plays:", lsNames, lsNames[0],
"The tracks in order. Enter plays the one you are on. Control+J then F3 search the list. F1 lists every key in this dialog.");

TextBox txtNow = dlg.addMemoBox("Now play&ing:", "",
"Track, position and address, kept up to date. Read-only: arrow through it, or copy from it with Control+C.");
txtNow.ReadOnly = true;

// ---- the commands ----

dlg.addBand();
Button btnPlay = dlg.addButton("&Play or pause", "Start what is selected, or pause and resume what is playing.");
Button btnStop = dlg.addButton("&Stop", "Stop playing and stay where you are in the queue.");
Button btnNext = dlg.addButton("&Next", "Move to the next track in the queue.");
Button btnBack = dlg.addButton("&Back", "Move to the previous track in the queue.");
dlg.endBand();

TextBox txtGoTo = dlg.addInlineInputBox("&Go to time",
"", "A time inside the current track, then Enter. 90, 1:30 and 1:05:00 all work. Alt+Shift+Right and Left move ten seconds, Page Down and Page Up a minute, Home returns to the start.");

// ---- the settings a listener changes while listening ----

dlg.addSeparator();

dlg.addBand();
NumericUpDown nudVolume = dlg.addNumericUpDown("&Volume:", iVolume, 0, 130,
"How loud, as a percentage. It starts below full so the media does not drown the screen reader; above 100 is louder than the file was made.");
nudVolume.Increment = 5;
NumericUpDown nudSpeed = dlg.addNumericUpDown("Spee&d:", iSpeed, 25, 400,
"How fast, as a percentage of normal. 100 is the speed it was recorded at. The pitch is corrected, so speech stays understandable.");
nudSpeed.Increment = 25;
dlg.endBand();

CheckBox chkRepeat = dlg.addCheckBox("&Repeat the queue", bRepeat,
"Start again at the first track when the last one ends.");
CheckBox chkAnnounce = dlg.addCheckBox("&Announce each track", bAnnounce,
"Say the name of a track when playback moves to it by itself. The name is not said when the cursor is in the queue, because your screen reader is already reading that line.");

// ---- the extras, and the standard Homer settings control ----

dlg.addBand();
Button btnCopy = dlg.addButton("Cop&y address", "Put the address of the track you are on onto the clipboard.");
Button btnSave = dlg.addButton("Save &list...", "Write the queue as an .m3u8 play list, names included, wherever you choose.");
Button btnOverview = dlg.addButton("&Overview", "Say how many tracks there are and then read their names, without moving the cursor.");
dlg.endBand();

CheckBox chkConfig = dlg.addCheckBox("&Use configuration", bUseConfig,
"Remember the volume, speed and the two boxes above, and start with them next time.");

// ---- what the controls do ----

Homer.Mpv oPlayer = player;
List<MediaTrack> lsRef = lsTracks;

btnPlay.Click += delegate(object o, EventArgs e) {
// The property still holds the state from before the toggle, because the
// answer comes back over the pipe. So the OLD value names the NEW state:
// it was playing, therefore it is now paused.
oPlayer.togglePause();
say(dlg, oPlayer.paused ? "Playing" : "Paused");
};
btnStop.Click += delegate(object o, EventArgs e) { oPlayer.stop(); say(dlg, "Stopped"); };
btnNext.Click += delegate(object o, EventArgs e) { oPlayer.next(); };
btnBack.Click += delegate(object o, EventArgs e) { oPlayer.previous(); };

btnCopy.Click += delegate(object o, EventArgs e) { copyAddress(dlg, oPlayer, lsRef, lstQueue); };
btnSave.Click += delegate(object o, EventArgs e) { saveList(dlg, lsRef); };
btnOverview.Click += delegate(object o, EventArgs e) {
// McTwit's Yield command: how many, then all of them. Faster than arrowing
// through thirty podcast titles to find out what is in the queue.
StringBuilder sbAll = new StringBuilder();
sbAll.Append(Homer.Util.stringPlural("track", lsRef.Count));
foreach (MediaTrack track in lsRef) { sbAll.Append(". "); sbAll.Append(track.sName); }
say(dlg, sbAll.ToString());
};

nudVolume.ValueChanged += delegate(object o, EventArgs e) {
// Nothing is spoken here: a spin box announces its own value as it changes.
oPlayer.setVolume((int) nudVolume.Value);
};
nudSpeed.ValueChanged += delegate(object o, EventArgs e) {
oPlayer.setSpeed(((double) nudSpeed.Value) / 100.0);
};
chkRepeat.CheckedChanged += delegate(object o, EventArgs e) {
oPlayer.setLoopPlaylist(chkRepeat.Checked);
};

// Enter on a track plays it. Enter is the list's own key, and it means the
// same thing here as it does everywhere else in FileDir: act on this item.
lstQueue.KeyDown += delegate(object o, KeyEventArgs ev) {
if (ev.KeyCode != Keys.Enter || ev.Alt || ev.Control || ev.Shift) return;
ev.Handled = true;
ev.SuppressKeyPress = true;
int iPick = lstQueue.SelectedIndex;
if (iPick < 0) return;
oPlayer.playIndex(iPick);
oPlayer.setPause(false);
};

txtGoTo.KeyDown += delegate(object o, KeyEventArgs ev) {
if (ev.KeyCode != Keys.Enter) return;
ev.Handled = true;
ev.SuppressKeyPress = true;
double dWhen = Homer.Mpv.parseTime(txtGoTo.Text);
if (dWhen < 0) { say(dlg, "Not a time"); return; }
oPlayer.seekAbsolute(dWhen);
say(dlg, Homer.Mpv.formatTime(dWhen));
};

// The three keys with no control of their own: seeking, and asking where you
// are. Alt+Shift, so they cannot be mistaken for a mnemonic, and never
// Alt+Control, which belongs to desktop shortcuts.
dlg.form.KeyPreview = true;
dlg.form.KeyDown += delegate(object o, KeyEventArgs ev) {
if (!ev.Alt || !ev.Shift) return;
bool bHandled = true;
switch (ev.KeyCode) {
case Keys.Right: oPlayer.seekRelative(10); say(dlg, positionText(oPlayer)); break;
case Keys.Left: oPlayer.seekRelative(-10); say(dlg, positionText(oPlayer)); break;
case Keys.PageDown: oPlayer.seekRelative(60); say(dlg, positionText(oPlayer)); break;
case Keys.PageUp: oPlayer.seekRelative(-60); say(dlg, positionText(oPlayer)); break;
case Keys.Home: oPlayer.seekAbsolute(0); say(dlg, "Start of track"); break;
case Keys.W: say(dlg, whereText(oPlayer, lsRef)); break;
default: bHandled = false; break;
}
if (bHandled) { ev.Handled = true; ev.SuppressKeyPress = true; }
};

// ---- the one thing that has to keep looking ----
//
// Property changes arrive on the pipe reader's thread, and touching a control
// from there is a fault waiting to happen. A timer on the interface thread
// asks instead. Half a second is faster than anyone notices.
Timer tmrWatch = new Timer();
tmrWatch.Interval = 500;
tmrWatch.Tick += delegate(object o, EventArgs e) {
int iNow = oPlayer.playlistIndex;
if (iNow >= 0 && iNow < lsRef.Count) {
bWasPlaying = true;
bEndSaid = false;
if (iNow != iAnnounced) {
bool bListHasFocus = lstQueue.Focused;
iAnnounced = iNow;
if (lstQueue.SelectedIndex != iNow) lstQueue.SelectedIndex = iNow;
// The reader reads the new line itself when the cursor is in the queue,
// so the name is said only when it is somewhere else.
if (chkAnnounce.Checked && !bListHasFocus) say(dlg, lsRef[iNow].sName);
}
}
if (oPlayer.idle && bWasPlaying && !bEndSaid) {
bEndSaid = true;
say(dlg, "End of queue");
}
txtNow.Text = nowPlayingText(oPlayer, lsRef);
};

dlg.setInitialFocus(lstQueue);

// The whole queue goes to mpv at once, so Next and Back are its own list
// operations rather than something this dialog has to keep in step.
for (int i = 0; i < lsTracks.Count; i++) player.loadFile(lsTracks[i].sTarget, i > 0);
player.setVolume(iVolume);
player.setSpeed(((double) iSpeed) / 100.0);
player.setLoopPlaylist(bRepeat);
tmrWatch.Start();
say(dlg, sQueueLabel.Replace("&", "") + " loaded");

try {
dlg.runWithButtons(new string[] { "Close" });
if (chkConfig.Checked) {
writeValue("volume", ((int) nudVolume.Value).ToString(CultureInfo.InvariantCulture));
writeValue("speed", ((int) nudSpeed.Value).ToString(CultureInfo.InvariantCulture));
writeValue("repeat", chkRepeat.Checked ? "y" : "n");
writeValue("announce", chkAnnounce.Checked ? "y" : "n");
}
writeValue("useConfiguration", chkConfig.Checked ? "y" : "n");
}
finally {
tmrWatch.Stop();
tmrWatch.Dispose();
player.Dispose();
dlg.Dispose();
}
} // run method

// say: speak it and keep it.
//
// FileDir's own App.say decides whether a message is spoken at all -- extra
// speech off sends it to the speech log instead, and Scroll Lock silences it --
// so the speaking goes through there rather than through Lbc's announce. The
// status line gets it either way, which is the point: speech disappears, and a
// screen reader can read a status line back at any time.
private static void say(Homer.LbcDialog dlg, string sText) {
App.say(sText);
if (dlg != null) dlg.appendStatus(sText);
}

// ---- the words the dialog says or shows ----

private static string positionText(Homer.Mpv player) {
string sAt = Homer.Mpv.formatTime(player.position);
string sOf = Homer.Mpv.formatTime(player.duration);
if (sAt.Length == 0) return "Not playing";
if (sOf.Length == 0) return sAt;
return sAt + " of " + sOf;
}

private static string whereText(Homer.Mpv player, List<MediaTrack> lsTracks) {
int iNow = player.playlistIndex;
string sName = (iNow >= 0 && iNow < lsTracks.Count) ? lsTracks[iNow].sName : player.title;
if (sName.Length == 0) return "Nothing playing";
string sCount = "";
if (iNow >= 0) sCount = ", track " + (iNow + 1).ToString(CultureInfo.InvariantCulture)
+ " of " + lsTracks.Count.ToString(CultureInfo.InvariantCulture);
return sName + sCount + ", " + positionText(player);
}

private static string nowPlayingText(Homer.Mpv player, List<MediaTrack> lsTracks) {
int iNow = player.playlistIndex;
StringBuilder sb = new StringBuilder();
if (iNow >= 0 && iNow < lsTracks.Count) {
sb.Append(lsTracks[iNow].sName);
sb.Append("\r\n");
sb.Append(lsTracks[iNow].sTarget);
sb.Append("\r\n");
}
sb.Append(player.paused ? "Paused" : "Playing");
sb.Append(", ");
sb.Append(positionText(player));
return sb.ToString();
}

private static void copyAddress(Homer.LbcDialog dlg, Homer.Mpv player, List<MediaTrack> lsTracks, ListBox lstQueue) {
// The track the cursor is on, which is not always the one playing: having
// found something worth keeping in a long queue, what is wanted is its
// address, and the cursor is where the person is looking.
int iPick = lstQueue.SelectedIndex;
if (iPick < 0) iPick = player.playlistIndex;
if (iPick < 0 || iPick >= lsTracks.Count) { say(dlg, "Nothing to copy"); return; }
try {
Clipboard.SetText(lsTracks[iPick].sTarget);
say(dlg, "Address copied");
}
catch (Exception) { say(dlg, "Could not copy the address"); }
}

private static void saveList(Homer.LbcDialog dlg, List<MediaTrack> lsTracks) {
string sPath = Lbc.SaveFileDialog("Save Play List", "PlayList.m3u8",
"Play lists (*.m3u8)|*.m3u8|All files (*.*)|*.*", 1, true);
if (sPath == null || sPath.Trim().Length == 0) return;
StringBuilder sb = new StringBuilder();
sb.Append("#EXTM3U\r\n");
foreach (MediaTrack track in lsTracks) {
sb.Append("#EXTINF:-1,");
sb.Append(track.sName);
sb.Append("\r\n");
sb.Append(track.sTarget);
sb.Append("\r\n");
}
try {
// UTF-8 WITHOUT a byte order mark, which is the one deliberate exception to
// the Homer text rule: several players treat a mark at the head of a play
// list as part of the first entry and then cannot find it.
File.WriteAllText(sPath, sb.ToString(), new UTF8Encoding(false));
say(dlg, "Saved " + Path.GetFileName(sPath));
Homer.Log.write("Media Player: saved " + lsTracks.Count + " tracks to " + sPath);
}
catch (Exception ex) {
Homer.Log.write("Media Player: could not save " + sPath + ": " + ex.Message);
say(dlg, "Could not save the list");
}
}

// ---- settings ----

private static int readNumber(string sKey, int iDefault, int iMin, int iMax) {
int iValue;
if (!int.TryParse(App.readValue(App.sIniFile, c_sSection, sKey, ""), out iValue)) return iDefault;
if (iValue < iMin || iValue > iMax) return iDefault;
return iValue;
}

private static bool readFlag(string sKey, bool bDefault) {
string sValue = App.readValue(App.sIniFile, c_sSection, sKey, "").Trim().ToLower();
if (sValue.Length == 0) return bDefault;
return sValue.StartsWith("y") || sValue.StartsWith("t") || sValue == "1";
}

private static void writeValue(string sKey, string sValue) {
try { App.writeValue(App.sIniFile, c_sSection, sKey, sValue); }
catch (Exception) { }
}

// ---- turning what FileDir has into tracks ----

// fromPlaylistLines: the m3u lines FileDir already builds, read back as
// tracks. #EXTINF carries the name a document gave the link, which is the
// whole reason this dialog is worth having.
public static List<MediaTrack> fromPlaylistLines(IList<string> lsLines) {
List<MediaTrack> lsTracks = new List<MediaTrack>();
string sPending = "";
foreach (string sRaw in lsLines) {
string sLine = sRaw == null ? "" : sRaw.Trim();
if (sLine.Length == 0) continue;
if (sLine.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase)) {
int iComma = sLine.IndexOf(',');
sPending = (iComma >= 0 && iComma < sLine.Length - 1) ? sLine.Substring(iComma + 1).Trim() : "";
continue;
}
if (sLine.StartsWith("#")) continue;
lsTracks.Add(new MediaTrack(sPending, sLine));
sPending = "";
}
return lsTracks;
}

// fromFiles: plain file names, named by their own file names.
public static List<MediaTrack> fromFiles(IList<string> lsPaths) {
List<MediaTrack> lsTracks = new List<MediaTrack>();
foreach (string sPath in lsPaths) {
if (sPath == null || sPath.Length == 0) continue;
lsTracks.Add(new MediaTrack(Path.GetFileName(sPath), sPath));
}
return lsTracks;
}

} // MediaPlayer class

} // FileDir namespace
