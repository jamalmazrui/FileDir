// MediaPlayer.cs -- the Homer Player dialog.
//
// WHAT IT IS
//
// A dialog that plays a queue of tracks. mpv does the playing, with no window,
// no keys and no focus of its own; every control here is an ordinary Windows
// control and each one becomes a command down mpv's pipe. Mpv.cs is that
// plumbing.
//
// THE LAYOUT
//
//   Tracks     the queue: name, presenter and length
//   Order      how the queue is sorted, then Next and Previous
//   Forward    Backward, and the Increment slider those two move by
//   Rate       and Volume
//   Go         Stop, Help, Close
//
// Five rows, each holding the controls that belong together, so Tab goes from
// a thing to the things that act on it rather than past the whole dialog.
//
// NAMES
//
// Not one control here has its AccessibleName set. A button carries its caption
// and a list carries the label above it, and setting the property to the same
// words makes some screen readers say them twice. Colons follow the labels that
// are separate controls -- Tracks, Order, Increment, Rate, Volume -- and never
// the buttons, which carry their own captions.
//
// SPEECH
//
// The reader announces the dialog, the control with focus, the list line under
// the cursor and a slider's value as it moves, so none of that is spoken here.
// What is spoken is what the reader cannot know: the track when playback moves
// on by itself, the position after a jump, the end of the queue.
//
// Speech goes out as global, which bypasses FileDir's Scroll Lock silence.
// That is deliberate: Scroll Lock is this dialog's play and pause key, so half
// the time it is on, and a player that fell silent on every other press of its
// own pause key would be unusable.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FileDir {

// One thing to play: what to hand mpv, and what to call it on screen.
public class MediaTrack {
public double dSeconds = -1;
public string sName;
public string sPresenter = "";
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

// sortTitle: the title as it should be compared -- case ignored, and a
// leading A, An or The set aside, which is how a shelf is ordered.
public string sortTitle() {
string sText = sName.Trim().ToLower();
foreach (string sWord in new string[] { "a ", "an ", "the " }) {
if (sText.StartsWith(sWord)) return sText.Substring(sWord.Length).Trim();
}
return sText;
}

// sortPresenter: by surname, case ignored, as a list of authors is ordered.
// The last word of a name is the surname often enough to be useful and is
// never worse than sorting by first name.
public string sortPresenter() {
string sText = sPresenter.Trim();
if (sText.Length == 0) return "zzzz";
int iSpace = sText.LastIndexOf(' ');
if (iSpace > 0) return sText.Substring(iSpace + 1).ToLower() + " " + sText.Substring(0, iSpace).ToLower();
return sText.ToLower();
}

// display: one line of the Tracks list. Whatever is known, in the order a
// person would say it, with nothing invented for what is not known.
public string display(int iNumber) {
StringBuilder sb = new StringBuilder();
sb.Append(iNumber.ToString(CultureInfo.InvariantCulture));
sb.Append(". ");
sb.Append(sName);
if (sPresenter.Length > 0) { sb.Append(", "); sb.Append(sPresenter); }
string sLength = Homer.Mpv.formatTime(dSeconds);
if (sLength.Length > 0) { sb.Append(", "); sb.Append(sLength); }
return sb.ToString();
}
} // MediaTrack class

public static class MediaPlayer {

// SETTINGS BELONG TO A PLAY LIST, NOT TO THE PROGRAM.
//
// Volume, speed, the jump size and the order suit the thing being played: a
// lecture wants a different speed from music, and a podcast page wants its own
// order. So each queue keeps its own settings, in a section named after where
// the queue came from, in the player's own .inix file. A queue nobody has
// played before gets the built-in defaults, which is what Defaults restores by
// deleting the section.
private static string settingsPath() {
return Path.Combine(App.sDataDir, "HomerPlayer.inix");
}

// sectionFor: a name for this queue's settings. Where it came from, which is
// stable across sessions: the same document or play list played again finds
// its own settings, and a different one does not.
private static string sectionFor(string sSource, List<MediaTrack> lsTracks) {
string sName = (sSource == null) ? "" : sSource.Trim();
if (sName.Length == 0 && lsTracks.Count > 0) sName = lsTracks[0].sTarget;
if (sName.Length == 0) return "Queue";
StringBuilder sb = new StringBuilder();
foreach (char ch in sName) {
if (char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' || ch == '_' || ch == ' ') sb.Append(ch);
else sb.Append('_');
}
string sClean = sb.ToString().Trim();
if (sClean.Length > 80) sClean = sClean.Substring(0, 80);
return (sClean.Length > 0) ? sClean : "Queue";
}

// How the Tracks list can be ordered. The first is the order the queue
// arrived in, which is what a play list is FOR: somebody chose that order.
private static readonly string[] c_asOrders = new string[] {
"Play list order", "Title", "Presenter", "Length, shortest first", "Length, longest first" };

// How far Forward and Backward move. In increasing order, because that is the
// order the sizes have -- alphabetical would put 15 minutes before 15 seconds.
// A list rather than a slider: a slider announces its position, so a person
// hears "4" where they need to hear "3 minutes".
private static readonly int[] c_aiSteps = new int[] { 15, 30, 60, 180, 300, 600, 900, 1800, 3600 };
private static readonly string[] c_asSteps = new string[] {
"15 seconds", "30 seconds", "1 minute", "3 minutes", "5 minutes",
"10 minutes", "15 minutes", "30 minutes", "1 hour" };
private const int c_iDefaultStep = 2;   // 1 minute
private const int c_iDefaultRate = 100;
private const int c_iDefaultOrder = 0;

// run: open the player on a queue of tracks. Returns when the dialog closes,
// by which time mpv has stopped and has written down where it had reached.
public static void run(IWin32Window owner, string sTitle, string sSource, List<MediaTrack> lsTracks) {
if (lsTracks == null || lsTracks.Count == 0) { App.say("0 tracks", true); return; }

string sMpv = Homer.Media.mpvProgram();
if (sMpv.Length == 0) {
Lbc.Show("mpv is not installed, so there is nothing to play with.\r\n\r\n"
+ "Run installMpv.cmd in the FileDir folder, or install FileDir again and tick the mpv box.",
"Homer Player");
return;
}

string sSettings = sectionFor(sSource, lsTracks);
int iVolume = readNumber(sSettings, "volume", Homer.Mpv.c_iDefaultVolume, 0, 130);
int iRate = readNumber(sSettings, "rate", c_iDefaultRate, 25, 400);
int iStep = readNumber(sSettings, "step", c_iDefaultStep, 0, c_aiSteps.Length - 1);
int iOrder = readNumber(sSettings, "order", c_iDefaultOrder, 0, c_asOrders.Length - 1);

Homer.Mpv player = new Homer.Mpv(sMpv, Homer.Media.findInstalled("yt-dlp"));
string sError;
if (!player.start(out sError)) {
Homer.Log.write("Homer Player: mpv would not start. " + sError);
Lbc.Show("The player would not start.\r\n\r\n" + sError, "Homer Player");
player.Dispose();
return;
}

// aOrder maps a row of the Tracks list to a place in mpv's own play list,
// which never changes. Sorting rearranges the rows and this map, and never
// the queue mpv is playing, so a sort during playback disturbs nothing.
int[] aOrder = new int[lsTracks.Count];
for (int i = 0; i < aOrder.Length; i++) aOrder[i] = i;
int iOrderNow = iOrder;
bool bMovingTimeline = false;
double dMarkStart = -1;
double dMarkEnd = -1;
int iAnnounced = -1;
bool bWasPlaying = false;
bool bEndSaid = false;
DateTime dtLastSaid = DateTime.MinValue;
string sLastNote = "";

Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, owner);
Homer.Mpv oPlayer = player;
List<MediaTrack> lsRef = lsTracks;

// ---- Tracks ----

dlg.addBand();
string sTracksLabel = "&Track list, " + Homer.Util.stringPlural("track", lsTracks.Count);
if (!string.IsNullOrEmpty(sSource)) sTracksLabel = sTracksLabel + " from " + sSource;
ListBox lstTracks = dlg.addPickBox(sTracksLabel + ":", orderedNames(lsRef, aOrder), null,
"The queue, with each track's name, presenter and length where they are known. Moving through it chooses nothing; Enter plays the one you are on.");
dlg.endBand();

// ---- Order, Next, Previous ----

dlg.addBand();
ListBox lstOrder = dlg.addPickBox("&Order of list:", new List<string>(c_asOrders), c_asOrders[iOrder],
"How the list above is arranged. Sorting moves the rows only; the queue keeps playing in the order it was given.");
Button btnNext = dlg.addButton("&Next track", "Play the next track in the queue.");
Button btnPrevious = dlg.addButton("&Previous track", "Play the previous track in the queue.");
dlg.endBand();

// ---- Forward, Backward, Increment ----

dlg.addBand();
Button btnForward = dlg.addButton("&Forward in track", "Jump forward inside the current track by the increment.");
Button btnBackward = dlg.addButton("&Backward in track", "Jump backward inside the current track by the increment.");
ListBox lstIncrement = dlg.addPickBox("&Increment of jump:", new List<string>(c_asSteps), c_asSteps[iStep],
"How far Forward and Backward move. One minute to begin with. Choosing here changes nothing else: the next Forward or Backward uses whatever is chosen.");
dlg.endBand();

// ---- Rate, Volume ----

// Chapters and the timeline: moving about INSIDE one track, which is a
// different job from moving between tracks and belongs on its own row.
dlg.addBand();
// A PAIR SHOULD READ AS A PAIR. The first two names here were Chapter ahead
// and Chapter behind, one taking its letter from the second word and the other
// from the first, which is two things to remember instead of one. Same first
// word, contrasting second, and the letter always from the second: press the
// same shape of key for the same shape of command.
Button btnChapterAhead = dlg.addButton("Chapter &more", "Move to the next chapter of this track. Many tracks have no chapters, and nothing happens on those.");
Button btnChapterBehind = dlg.addButton("Chapter &less", "Move to the previous chapter of this track, which usually means the start of the one playing. Press it again for the one before.");
TrackBar barTimeline = dlg.addSlider("&Where in track:", 0, 0, 100, 5,
"How far through the track to move, as a percentage. Moving it moves the playback. It is set when you tab into it and does not follow along while playing, because a control that changed twice a second would be read out over everything else.",
delegate(int iPercent) { return iPercent.ToString(CultureInfo.InvariantCulture) + " percent"; });
dlg.endBand();

dlg.addBand();
TrackBar barRate = dlg.addSlider("&Rate percent:", iRate, 25, 400, 5,
"How fast to play, as a percentage of normal. The pitch is corrected, so speech stays understandable.",
delegate(int iPercent) { return iPercent.ToString(CultureInfo.InvariantCulture) + " percent"; });
TrackBar barVolume = dlg.addSlider("&Volume percent:", iVolume, 0, 130, 5,
"How loud. It starts below full so the media does not drown the screen reader; above 100 is louder than the recording.",
delegate(int iPercent) { return iPercent.ToString(CultureInfo.InvariantCulture) + " percent"; });
dlg.endBand();

// ---- Go, Stop, Help, Close ----

dlg.addBand();
// EXECUTE PLAYBACK, not Go.
//
// Go was the right word and the wrong key: Gemini claims Alt+G across the whole
// desktop, so the dialog never saw it. A global hotkey belongs to whoever
// registered it first and no dialog can take it back, so the button moved
// rather than the key. Run was the other candidate and collides with Rate;
// Start collides with Stop, which has no better name than Stop playback.
Button btnGo = dlg.addButton("&Execute playback", "Play the track the cursor is on, or resume what is paused, applying the order the cursor is on. Control+Enter does this from anywhere in the dialog, and so does Scroll Lock.");
Button btnStop = dlg.addButton("&Stop playback", "Stop playing and stay exactly where you are, in the queue and in the track. Execute playback carries on from there. It is a pause; the name is Stop because Previous track already has the P.");
Button btnDefaults = dlg.addButton("&Default settings", "Forget what this queue has been set to and go back to the built-in settings: one minute, normal speed, the play list's own order.");
Button btnClip = dlg.addButton("&Clip to file", "Write the part you marked with F8 and Shift+F8 to a media file of its own, and put that file on the clipboard so it can be pasted into a folder or a message.");
Button btnHelp = dlg.addButton("&Help topics", "List every control in this dialog with what it does, and the keys that have no control.");
Button btnClose = dlg.addButton("Close", "Close the player. Where each track had reached is written down first, so playing it again starts there.");
dlg.endBand();

// ---- what the controls do ----

// MOVING THROUGH A LIST CHOOSES NOTHING. IT IS HOW A LIST IS READ.
//
// Arrowing down the Tracks list is how a screen reader user finds out what is
// in the queue, and arrowing down Order is how they find out what the orders
// are. If either acted as it was passed over, there would be no way to look
// without doing: every glance at the third order would sort the queue, and
// every glance at track nine would start playing it.
//
// So neither list has a SelectedIndexChanged handler. The cursor moves, the
// reader reads, and nothing happens. Go is what makes it happen -- it applies
// the order the cursor is on, and plays the track the cursor is on. Enter in
// either list does the same, which is what Enter means everywhere in FileDir.
//
// Stop is not needed first. Go on a queue that is already playing simply moves
// to what was chosen.
EventHandler ehGo = delegate(object o, EventArgs e) {
int iWantOrder = lstOrder.SelectedIndex;
if (iWantOrder >= 0 && iWantOrder != iOrderNow) {
iOrderNow = iWantOrder;
sortQueue(lsRef, aOrder, iWantOrder);
refillTracks(lstTracks, lsRef, aOrder);
writeValue(sSettings, "order", iWantOrder.ToString(CultureInfo.InvariantCulture));
say(dlg, "Ordered by " + c_asOrders[iWantOrder]);
}
goOrResume(dlg, oPlayer, lsRef, lstTracks, aOrder);
};

btnGo.Click += ehGo;
btnStop.Click += delegate(object o, EventArgs e) {
oPlayer.stop();
// Stopping is not the queue running out. Without this, pausing at the start of
// a track was announced as "End of queue" a moment later.
bWasPlaying = false;
bEndSaid = true;
say(dlg, "Stopped");
};
btnNext.Click += delegate(object o, EventArgs e) { oPlayer.next(); hear(oPlayer); };
btnPrevious.Click += delegate(object o, EventArgs e) { oPlayer.previous(); hear(oPlayer); };
btnForward.Click += delegate(object o, EventArgs e) {
oPlayer.seekRelative(stepSeconds(lstIncrement));
hear(oPlayer);
say(dlg, positionText(oPlayer));
};
btnBackward.Click += delegate(object o, EventArgs e) {
oPlayer.seekRelative(-stepSeconds(lstIncrement));
hear(oPlayer);
say(dlg, positionText(oPlayer));
};
btnChapterAhead.Click += delegate(object o, EventArgs e) {
oPlayer.nextChapter();
hear(oPlayer);
say(dlg, positionText(oPlayer));
};
btnChapterBehind.Click += delegate(object o, EventArgs e) {
oPlayer.previousChapter();
hear(oPlayer);
say(dlg, positionText(oPlayer));
};

// THE TIMELINE IS SET WHEN YOU ARRIVE AT IT, AND NOT AFTERWARDS.
//
// Reading its current place as the cursor lands on it is safe: a reader
// announces a slider's value when focus arrives anyway, so the number it
// announces may as well be true. Following playback while the cursor sits
// elsewhere is not safe, and is not done.
//
// bMovingTimeline keeps that refresh from being mistaken for a person dragging
// the slider, which would seek to where playback already was.
barTimeline.GotFocus += delegate(object o, EventArgs e) {
double dWhole = oPlayer.duration;
double dAt = oPlayer.position;
if (dWhole <= 0 || dAt < 0) return;
int iPercent = (int) ((dAt * 100.0) / dWhole);
if (iPercent < 0) iPercent = 0;
if (iPercent > 100) iPercent = 100;
bMovingTimeline = true;
try { barTimeline.Value = iPercent; }
finally { bMovingTimeline = false; }
};
barTimeline.ValueChanged += delegate(object o, EventArgs e) {
if (bMovingTimeline) return;
double dWhole = oPlayer.duration;
if (dWhole <= 0) { say(dlg, "No length known"); return; }
oPlayer.seekAbsolute((dWhole * barTimeline.Value) / 100.0);
hear(oPlayer);
};

btnClip.Click += delegate(object o, EventArgs e) {
clipToFile(dlg, oPlayer, lsRef, dMarkStart, dMarkEnd);
};

btnHelp.Click += delegate(object o, EventArgs e) { dlg.showHelp(); };
btnClose.Click += delegate(object o, EventArgs e) { dlg.close(); };

btnDefaults.Click += delegate(object o, EventArgs e) {
forgetSettings(sSettings);
bMovingTimeline = true;
try { barTimeline.Value = 0; }
finally { bMovingTimeline = false; }
lstIncrement.SelectedIndex = c_iDefaultStep;
barRate.Value = c_iDefaultRate;
barVolume.Value = Homer.Mpv.c_iDefaultVolume;
lstOrder.SelectedIndex = c_iDefaultOrder;
iOrderNow = c_iDefaultOrder;
sortQueue(lsRef, aOrder, c_iDefaultOrder);
refillTracks(lstTracks, lsRef, aOrder);
say(dlg, "Defaults restored");
};

// A slider is different from a list: moving it IS changing it, which is what
// its arrow keys have always meant in Windows. So these take effect at once,
// and are written down at once.
barRate.ValueChanged += delegate(object o, EventArgs e) {
oPlayer.setSpeed(((double) barRate.Value) / 100.0);
writeValue(sSettings, "rate", barRate.Value.ToString(CultureInfo.InvariantCulture));
};
barVolume.ValueChanged += delegate(object o, EventArgs e) {
oPlayer.setVolume(barVolume.Value);
writeValue(sSettings, "volume", barVolume.Value.ToString(CultureInfo.InvariantCulture));
};

// The increment is read when Forward or Backward is pressed, so choosing in
// this list changes nothing by itself either. It is written down when the
// dialog closes, along with whatever the cursor was left on.
lstTracks.KeyDown += delegate(object o, KeyEventArgs ev) {
if (ev.KeyCode != Keys.Enter || ev.Alt || ev.Control || ev.Shift) return;
ev.Handled = true;
ev.SuppressKeyPress = true;
ehGo(null, EventArgs.Empty);
};
lstOrder.KeyDown += delegate(object o, KeyEventArgs ev) {
if (ev.KeyCode != Keys.Enter || ev.Alt || ev.Control || ev.Shift) return;
ev.Handled = true;
ev.SuppressKeyPress = true;
ehGo(null, EventArgs.Empty);
};

// SCROLL LOCK IS PLAY AND PAUSE, from any control in this dialog and nowhere
// else.
//
// It is claimed through ProcessCmdKey rather than KeyDown. Scroll Lock is a
// toggle key: Windows acts on it and a KeyDown handler behind KeyPreview may
// never see it, which is why the first attempt did nothing. ProcessCmdKey runs
// ahead of the controls and ahead of that.
//
// It has a cost worth knowing: FileDir treats Scroll Lock as silence, so while
// it is on, ordinary FileDir speech is suppressed. That is why this dialog
// speaks globally.
// SHIFT AND A NAVIGATION KEY IS THE TRANSPORT.
//
// A screen reader turns Num Lock off and leaves it off, so the keypad sends the
// same virtual keys as the six-pack: keypad 4 IS Left, keypad 8 IS Up, keypad 5
// is Clear. Binding NumPad4 and its neighbours, as the first attempt did, binds
// keys that never arrive. Those bindings are still here at the bottom, for a
// keyboard with Num Lock on, but they are not the scheme.
//
// The scheme is Shift with the navigation keys, which works the same on both
// pads because both send the same keys. Bare navigation belongs to whatever
// control has focus -- the Track list is read with it -- and Control with those
// keys belongs to the dialog itself, where Control+Home and Control+End move to
// the first and last field. Shift with them is free: this dialog has nothing to
// select and nothing to tag, and a single-selection list does nothing with
// Shift at all.
//
// Each key keeps the sense it already has, applied to the media instead of a
// list: arrows step, the Page keys move by a bigger unit, Home and End are the
// ends, and Control makes it the whole way.
//
//   Shift+Left, Shift+Right          back and forward by the increment
//   Shift+Up, Shift+Down             previous and next track
//   Shift+PageUp, Shift+PageDown     previous and next chapter
//   Shift+Home, Shift+End            start and end of this track
//   Control+Shift+Home, +End         first and last track
//   Control+Shift+PageUp, +PageDown  first and last chapter
//   Shift+Clear (keypad 5)           play or pause
dlg.commandKey = delegate(Keys keyData) {
if (keyData == (Keys.Shift | Keys.Left)) { oPlayer.seekRelative(-stepSeconds(lstIncrement)); hear(oPlayer); say(dlg, positionText(oPlayer)); return true; }
if (keyData == (Keys.Shift | Keys.Right)) { oPlayer.seekRelative(stepSeconds(lstIncrement)); hear(oPlayer); say(dlg, positionText(oPlayer)); return true; }
if (keyData == (Keys.Shift | Keys.Up)) { oPlayer.previous(); hear(oPlayer); return true; }
if (keyData == (Keys.Shift | Keys.Down)) { oPlayer.next(); hear(oPlayer); return true; }
if (keyData == (Keys.Shift | Keys.PageUp)) { oPlayer.previousChapter(); hear(oPlayer); say(dlg, positionText(oPlayer)); return true; }
if (keyData == (Keys.Shift | Keys.PageDown)) { oPlayer.nextChapter(); hear(oPlayer); say(dlg, positionText(oPlayer)); return true; }
if (keyData == (Keys.Shift | Keys.Home)) { oPlayer.seekAbsolute(0); hear(oPlayer); say(dlg, "Start of track"); return true; }
if (keyData == (Keys.Shift | Keys.End)) { seekToEnd(dlg, oPlayer); hear(oPlayer); return true; }
if (keyData == (Keys.Control | Keys.Shift | Keys.Home)) { oPlayer.playIndex(0); oPlayer.setPause(false); say(dlg, "First track"); return true; }
if (keyData == (Keys.Control | Keys.Shift | Keys.End)) { oPlayer.playIndex(lsRef.Count - 1); oPlayer.setPause(false); say(dlg, "Last track"); return true; }
if (keyData == (Keys.Control | Keys.Shift | Keys.PageUp)) { firstChapter(dlg, oPlayer); hear(oPlayer); return true; }
if (keyData == (Keys.Control | Keys.Shift | Keys.PageDown)) { lastChapter(dlg, oPlayer); hear(oPlayer); return true; }
if (keyData == (Keys.Shift | Keys.Clear)) { oPlayer.togglePause(); say(dlg, oPlayer.paused ? "Playing" : "Paused"); return true; }

// And the same commands on the digits, for a keyboard whose Num Lock is on.
// The grid reads as one sentence: the left column goes back, the right column
// goes forward, and each row is a different size of step -- chapters on top,
// the increment in the middle, whole tracks at the bottom.
if (keyData == Keys.NumPad5) { oPlayer.togglePause(); say(dlg, oPlayer.paused ? "Playing" : "Paused"); return true; }
if (keyData == Keys.NumPad0) { oPlayer.stop(); bWasPlaying = false; bEndSaid = true; say(dlg, "Stopped"); return true; }
if (keyData == Keys.NumPad4) { oPlayer.seekRelative(-stepSeconds(lstIncrement)); hear(oPlayer); say(dlg, positionText(oPlayer)); return true; }
if (keyData == Keys.NumPad6) { oPlayer.seekRelative(stepSeconds(lstIncrement)); hear(oPlayer); say(dlg, positionText(oPlayer)); return true; }
if (keyData == Keys.NumPad7) { oPlayer.previousChapter(); hear(oPlayer); say(dlg, positionText(oPlayer)); return true; }
if (keyData == Keys.NumPad9) { oPlayer.nextChapter(); hear(oPlayer); say(dlg, positionText(oPlayer)); return true; }
if (keyData == Keys.NumPad8) { oPlayer.seekAbsolute(0); hear(oPlayer); say(dlg, "Start of track"); return true; }
if (keyData == Keys.NumPad1) { oPlayer.previous(); hear(oPlayer); return true; }
if (keyData == Keys.NumPad3) { oPlayer.next(); hear(oPlayer); return true; }
if (keyData == Keys.NumPad2) { say(dlg, positionText(oPlayer)); return true; }
if (keyData == Keys.Decimal) { say(dlg, whereText(oPlayer, lsRef)); return true; }

// Control on a digit means all the way, matching Control+Shift on the
// navigation keys above.
if (keyData == (Keys.Control | Keys.NumPad4)) { oPlayer.seekAbsolute(0); hear(oPlayer); say(dlg, "Start of track"); return true; }
if (keyData == (Keys.Control | Keys.NumPad6)) { seekToEnd(dlg, oPlayer); hear(oPlayer); return true; }
if (keyData == (Keys.Control | Keys.NumPad1)) { oPlayer.playIndex(0); oPlayer.setPause(false); say(dlg, "First track"); return true; }
if (keyData == (Keys.Control | Keys.NumPad3)) { oPlayer.playIndex(lsRef.Count - 1); oPlayer.setPause(false); say(dlg, "Last track"); return true; }
if (keyData == (Keys.Control | Keys.NumPad7)) { firstChapter(dlg, oPlayer); hear(oPlayer); return true; }
if (keyData == (Keys.Control | Keys.NumPad9)) { lastChapter(dlg, oPlayer); hear(oPlayer); return true; }

if (keyData == Keys.Subtract) { barVolume.Value = Math.Max(barVolume.Minimum, barVolume.Value - 5); return true; }
if (keyData == Keys.Add) { barVolume.Value = Math.Min(barVolume.Maximum, barVolume.Value + 5); return true; }
if (keyData == Keys.Divide) { barRate.Value = Math.Max(barRate.Minimum, barRate.Value - 5); return true; }
if (keyData == Keys.Multiply) { barRate.Value = Math.Min(barRate.Maximum, barRate.Value + 5); return true; }

// F8 AND SHIFT+F8 MARK A SPAN, which is what they do everywhere else in Homer
// Tools: start the selection, then complete it. Here the two ends are moments
// in the track rather than lines in a file, and the span they make is what
// Clip to file writes out.
if (keyData == Keys.F8) {
dMarkStart = oPlayer.position;
dMarkEnd = -1;
say(dlg, (dMarkStart >= 0) ? ("Start marked at " + Homer.Mpv.formatTime(dMarkStart)) : "Nothing playing");
return true;
}
if (keyData == (Keys.Shift | Keys.F8)) {
// SHIFT+F8 ON ITS OWN MEANS FROM THE BEGINNING. Shift+End selects from here to
// the end of a line and Shift+Home from the start of one; completing a
// selection nobody started is the same idea, and it saves going back to the
// top of the track to press F8 there.
if (dMarkStart < 0) {
dMarkStart = 0;
say(dlg, "Marking from the start of the track");
}
dMarkEnd = oPlayer.position;
if (dMarkEnd <= dMarkStart) { say(dlg, "The end must come after the start"); dMarkEnd = -1; return true; }
say(dlg, "Marked " + Homer.Mpv.formatTime(dMarkStart) + " to " + Homer.Mpv.formatTime(dMarkEnd)
+ ", " + Homer.Mpv.formatTime(dMarkEnd - dMarkStart) + " long");
return true;
}
// CONTROL+ENTER IS ALWAYS GO, whatever the default button is at the time.
// The default button changes with what the player is doing -- Stop while
// something plays -- and a person who wants to start something should not
// have to work out which button Enter would press at that moment.
if (keyData == (Keys.Control | Keys.Enter)) {
ehGo(null, EventArgs.Empty);
return true;
}
if ((keyData & Keys.KeyCode) != Keys.Scroll) return false;
Homer.Log.write("Homer Player: Scroll Lock, play or pause");
oPlayer.togglePause();
// The property still holds the state from before the toggle, because the
// answer travels back over the pipe: it was playing, so it is now paused.
say(dlg, oPlayer.paused ? "Playing" : "Paused");
return true;
};

// The other keys are the commands no control expresses well. Alt+Shift with a
// letter, never a navigation key: in Windows, and in FileDir and DbDo in
// particular, Home, End, the arrows and the Page keys mean selecting and
// moving, and a player command wearing one of those is a false promise.
dlg.form.KeyPreview = true;
dlg.form.KeyDown += delegate(object o, KeyEventArgs ev) {
if (!ev.Alt || !ev.Shift) return;
bool bHandled = true;
switch (ev.KeyCode) {
case Keys.N: oPlayer.nextChapter(); say(dlg, positionText(oPlayer)); break;
case Keys.P: oPlayer.previousChapter(); say(dlg, positionText(oPlayer)); break;
case Keys.T: oPlayer.seekAbsolute(0); say(dlg, "Start of track"); break;
case Keys.Z: oPlayer.revertSeek(); say(dlg, positionText(oPlayer)); break;
case Keys.A: say(dlg, positionText(oPlayer)); break;
case Keys.W: say(dlg, whereText(oPlayer, lsRef)); break;
case Keys.O: sayOverview(dlg, lsRef, aOrder); break;
case Keys.C: copyAddress(dlg, oPlayer, lsRef, lstTracks, aOrder); break;
case Keys.L: saveList(dlg, lsRef, aOrder); break;
default: bHandled = false; break;
}
if (bHandled) { ev.Handled = true; ev.SuppressKeyPress = true; }
};

// THE PLAYER NEVER MOVES THE CURSOR.
//
// The first version selected the playing track in the Tracks list as playback
// moved on. That looked helpful and was not: the cursor belongs to the person
// reading with it, and moving it under them means a Say Line in some other
// list reads a track instead, or an arrow key starts from somewhere they never
// put it. A track beginning is news; it is announced, and that is all.
//
// Nothing else is watched. A position that changes twice a second is not news,
// and a control that reports it is a control the reader talks over everything
// else to read.
// THE DEFAULT BUTTON FOLLOWS THE PLAYER.
//
// Nothing playing: Enter means Go, because starting is the only thing left to
// want. Something playing: Enter means Stop, because stopping is. Either way
// the answer to "what does Enter do here" is the obvious one, and Control+Enter
// is Go throughout for the person who wants to start something else while this
// one plays.
//
// Enter inside the Tracks or Order list still plays what the cursor is on:
// those two handle Enter themselves, and this default is for everything that
// does not.
bool bDefaultIsStop = false;
Timer tmrWatch = new Timer();
tmrWatch.Interval = 500;
tmrWatch.Tick += delegate(object o, EventArgs e) {
bool bPlayingNow = !oPlayer.idle && !oPlayer.paused;
if (bPlayingNow != bDefaultIsStop) {
bDefaultIsStop = bPlayingNow;
try { dlg.form.AcceptButton = bPlayingNow ? btnStop : btnGo; }
catch (Exception) { }
}
// The standing note on the status line. It is not a live region and nothing
// announces it: it is there to be read with the screen reader's own key for
// the status line, when the person wants it.
string sNote = statusNote(oPlayer, lsRef);
if (sNote != sLastNote) { sLastNote = sNote; dlg.setStatusExtra(sNote); }

int iNow = oPlayer.playlistIndex;
bool bPlayingNow2 = !oPlayer.idle && !oPlayer.paused;
if (iNow >= 0 && iNow < lsRef.Count && bPlayingNow2) {
bWasPlaying = true;
bEndSaid = false;
if (iNow != iAnnounced) {
iAnnounced = iNow;
// At most one name every second and a half: a queue of addresses that
// will not play walks itself to the end in seconds, and a name for each
// is noise rather than news.
if ((DateTime.Now - dtLastSaid).TotalMilliseconds >= 1500) {
dtLastSaid = DateTime.Now;
say(dlg, lsRef[iNow].sName);
}
}
}
if (oPlayer.idle && bWasPlaying && !bEndSaid) {
bEndSaid = true;
say(dlg, "End of queue");
}
};

dlg.setInitialFocus(lstTracks);
if (iOrder != 0) { sortQueue(lsRef, aOrder, iOrder); refillTracks(lstTracks, lsRef, aOrder); }

// The queue goes over once the dialog is up, not while the window is still
// being born, and mpv's own list operations then drive Next and Previous.
Homer.Mpv oLoader = player;
List<MediaTrack> lsToLoad = lsTracks;
int iStartVolume = iVolume;
int iStartRate = iRate;
dlg.form.Shown += delegate(object o, EventArgs e) {
// PAUSED BEFORE ANYTHING IS LOADED. mpv starts playing the moment it is given
// a file, so the queue arriving was enough to start the first track talking
// over the dialog that had just opened. Nothing should play until somebody
// asks: Execute playback, Enter, or Scroll Lock all clear the pause.
oLoader.setPause(true);
for (int i = 0; i < lsToLoad.Count; i++) oLoader.loadFile(lsToLoad[i].sTarget, i > 0);
oLoader.setVolume(iStartVolume);
oLoader.setSpeed(((double) iStartRate) / 100.0);
};
tmrWatch.Start();

try {
// Go is the default button to start with, and Close is what Escape presses.
// The default changes to Stop while something plays; see the watcher above.
dlg.runPlain(btnGo, btnClose);

// ESCAPE SAVES. Everything here is written as it changes, so by now the file
// already holds it -- except the increment, which is read when Forward or
// Backward is pressed rather than watched. Written last, along with the order
// the list was left in, so the whole of what was chosen survives the exit
// however the dialog was closed.
if (lstIncrement.SelectedIndex >= 0)
writeValue(sSettings, "step", lstIncrement.SelectedIndex.ToString(CultureInfo.InvariantCulture));
if (lstOrder.SelectedIndex >= 0)
writeValue(sSettings, "order", lstOrder.SelectedIndex.ToString(CultureInfo.InvariantCulture));
if (barRate != null) writeValue(sSettings, "rate", barRate.Value.ToString(CultureInfo.InvariantCulture));
if (barVolume != null) writeValue(sSettings, "volume", barVolume.Value.ToString(CultureInfo.InvariantCulture));
}
finally {
tmrWatch.Stop();
tmrWatch.Dispose();
// WHERE EACH TRACK HAD REACHED IS WRITTEN DOWN, which is mpv's own
// quit-watch-later and what uppercase Q does in its player window. Playing
// the same thing again starts where it stopped, in this dialog or in mpv's.
try { player.quitRemembering(); }
catch (Exception) { }
player.Dispose();
dlg.Dispose();
}
} // run method

// ---- the commands the buttons and keys share ----

// hear: after a command that MOVES somewhere, start playing.
//
// A person who cannot see the display finds out where they have landed by
// listening to it. Next track, a chapter, a jump, the timeline -- each of those
// is a question about where the media goes, and silence is not an answer. So
// every command that moves clears the pause; the ones that only report, like
// Alt+Shift+A, leave it alone.
private static void hear(Homer.Mpv player) {
player.setPause(false);
}

// stepSeconds: how far Forward and Backward move, read at the moment they are
// pressed rather than watched for changes.
private static int stepSeconds(ListBox lstIncrement) {
int iPick = lstIncrement.SelectedIndex;
if (iPick < 0 || iPick >= c_aiSteps.Length) iPick = c_iDefaultStep;
return c_aiSteps[iPick];
}

private static void goOrResume(Homer.LbcDialog dlg, Homer.Mpv player, List<MediaTrack> lsTracks,
ListBox lstTracks, int[] aOrder) {
int iRow = lstTracks.SelectedIndex;
int iTrack = (iRow >= 0 && iRow < aOrder.Length) ? aOrder[iRow] : -1;
if (iTrack < 0) { say(dlg, "No track"); return; }
// Go on the track already playing means resume rather than start again,
// which is what a person pressing Go on a paused player wants.
if (iTrack == player.playlistIndex && player.paused) {
player.setPause(false);
say(dlg, "Playing");
return;
}
player.playIndex(iTrack);
player.setPause(false);
}

private static void sayOverview(Homer.LbcDialog dlg, List<MediaTrack> lsTracks, int[] aOrder) {
StringBuilder sb = new StringBuilder();
sb.Append(Homer.Util.stringPlural("track", lsTracks.Count));
for (int iRow = 0; iRow < aOrder.Length; iRow++) { sb.Append(". "); sb.Append(lsTracks[aOrder[iRow]].sName); }
say(dlg, sb.ToString());
}

private static void copyAddress(Homer.LbcDialog dlg, Homer.Mpv player, List<MediaTrack> lsTracks,
ListBox lstTracks, int[] aOrder) {
int iRow = lstTracks.SelectedIndex;
int iTrack = (iRow >= 0 && iRow < aOrder.Length) ? aOrder[iRow] : player.playlistIndex;
if (iTrack < 0 || iTrack >= lsTracks.Count) { say(dlg, "Nothing to copy"); return; }
try {
Clipboard.SetText(lsTracks[iTrack].sTarget);
say(dlg, "Address copied");
}
catch (Exception) { say(dlg, "Could not copy the address"); }
}

private static void saveList(Homer.LbcDialog dlg, List<MediaTrack> lsTracks, int[] aOrder) {
string sPath = Lbc.SaveFileDialog("Save Play List", "PlayList.m3u8",
"Play lists (*.m3u8)|*.m3u8|All files (*.*)|*.*", 1, true);
if (sPath == null || sPath.Trim().Length == 0) return;
StringBuilder sb = new StringBuilder();
sb.Append("#EXTM3U\r\n");
// Saved in the order shown, because the order shown is the one just chosen.
for (int iRow = 0; iRow < aOrder.Length; iRow++) {
MediaTrack track = lsTracks[aOrder[iRow]];
sb.Append("#EXTINF:");
sb.Append(((int) (track.dSeconds > 0 ? track.dSeconds : -1)).ToString(CultureInfo.InvariantCulture));
sb.Append(",");
if (track.sPresenter.Length > 0) { sb.Append(track.sPresenter); sb.Append(" - "); }
sb.Append(track.sName);
sb.Append("\r\n");
sb.Append(track.sTarget);
sb.Append("\r\n");
}
try {
// UTF-8 WITHOUT a byte order mark, the one deliberate exception to the
// Homer text rule: several players read a mark at the head of a play list
// as part of the first entry and then cannot find it.
File.WriteAllText(sPath, sb.ToString(), new UTF8Encoding(false));
say(dlg, "Saved " + Path.GetFileName(sPath));
Homer.Log.write("Homer Player: saved " + lsTracks.Count + " tracks to " + sPath);
}
catch (Exception ex) {
Homer.Log.write("Homer Player: could not save " + sPath + ": " + ex.Message);
say(dlg, "Could not save the list");
}
}

// ---- the list and its order ----

private static List<string> orderedNames(List<MediaTrack> lsTracks, int[] aOrder) {
List<string> lsNames = new List<string>();
for (int iRow = 0; iRow < aOrder.Length; iRow++) lsNames.Add(lsTracks[aOrder[iRow]].display(iRow + 1));
return lsNames;
}

private static void refillTracks(ListBox lstTracks, List<MediaTrack> lsTracks, int[] aOrder) {
lstTracks.BeginUpdate();
try {
lstTracks.Items.Clear();
foreach (string sLine in orderedNames(lsTracks, aOrder)) lstTracks.Items.Add(sLine);
if (lstTracks.Items.Count > 0) lstTracks.SelectedIndex = 0;
}
finally { lstTracks.EndUpdate(); }
}

// sortQueue: rearrange the ROWS, never the queue mpv is playing. A plain
// insertion sort, because a play list is short and a stable order matters
// more than speed: tracks that compare the same keep the order they came in.
private static void sortQueue(List<MediaTrack> lsTracks, int[] aOrder, int iOrder) {
for (int i = 0; i < aOrder.Length; i++) aOrder[i] = i;
if (iOrder == 0) return;
for (int i = 1; i < aOrder.Length; i++) {
int iHeld = aOrder[i];
int j = i - 1;
while (j >= 0 && comesAfter(lsTracks[aOrder[j]], lsTracks[iHeld], iOrder)) {
aOrder[j + 1] = aOrder[j];
j--;
}
aOrder[j + 1] = iHeld;
}
}

private static bool comesAfter(MediaTrack left, MediaTrack right, int iOrder) {
if (iOrder == 1) return string.CompareOrdinal(left.sortTitle(), right.sortTitle()) > 0;
if (iOrder == 2) return string.CompareOrdinal(left.sortPresenter(), right.sortPresenter()) > 0;
// A length nobody knows sorts last either way, rather than pretending to be
// zero and heading the list.
double dLeft = left.dSeconds >= 0 ? left.dSeconds : double.MaxValue;
double dRight = right.dSeconds >= 0 ? right.dSeconds : double.MaxValue;
if (iOrder == 3) return dLeft > dRight;
if (left.dSeconds < 0 || right.dSeconds < 0) return dLeft > dRight;
return dLeft < dRight;
}

// seekToEnd: the last few seconds rather than the very last instant, so the end
// can be heard instead of the next track starting.
private static void seekToEnd(Homer.LbcDialog dlg, Homer.Mpv player) {
double dWhole = player.duration;
if (dWhole <= 0) { say(dlg, "No length known"); return; }
double dTarget = dWhole - 3;
if (dTarget < 0) dTarget = 0;
player.seekAbsolute(dTarget);
say(dlg, "End of track");
}

private static void firstChapter(Homer.LbcDialog dlg, Homer.Mpv player) {
if (player.chapterCount <= 0) { say(dlg, "No chapters"); return; }
player.setChapter(0);
say(dlg, "First chapter");
}

private static void lastChapter(Homer.LbcDialog dlg, Homer.Mpv player) {
int iCount = player.chapterCount;
if (iCount <= 0) { say(dlg, "No chapters"); return; }
player.setChapter(iCount - 1);
say(dlg, "Last chapter");
}

// ---- taking a piece of a track away with you ----

// clipToFile: write the marked span to a media file of its own, then put that
// file on the clipboard.
//
// ON THE CLIPBOARD QUESTION. Windows has no clipboard format for a piece of
// audio that other programs will accept -- the old CF_WAVE is legacy and
// almost nothing pastes it, and there is nothing at all for video. What every
// program on Windows 11 does accept is a FILE: a file on the clipboard pastes
// into Explorer, into mail, into a chat window, into anything that takes a
// dropped file. So the clip is written to disk and the file goes on the
// clipboard, which is the same thing the person wanted by a route that works.
//
// ffmpeg does the cutting, with the streams copied rather than re-encoded: it
// is quick, it loses nothing, and the clip keeps the format it came from.
private static void clipToFile(Homer.LbcDialog dlg, Homer.Mpv player, List<MediaTrack> lsTracks,
double dStart, double dEnd) {
if (dStart < 0 || dEnd <= dStart) {
say(dlg, "Nothing marked. Press F8 where the piece should start and Shift+F8 where it should end.");
return;
}
int iNow = player.playlistIndex;
if (iNow < 0 || iNow >= lsTracks.Count) { say(dlg, "Nothing playing"); return; }
MediaTrack track = lsTracks[iNow];

string sFfmpeg = Homer.Media.findInstalled("ffmpeg");
if (sFfmpeg.Length == 0) {
Lbc.Show("ffmpeg is not installed, and it is what cuts the clip.\r\n\r\n"
+ "Install FileDir again with the media tools box ticked, or run installMediaTools.cmd in the FileDir folder.",
"Homer Player");
return;
}

// Where it goes: beside the file it came from when that is a file of ours,
// and in the folder FileDir is looking at otherwise -- which is where a person
// who just made something expects to find it.
string sExtension = ".mp3";
string sFolder = App.sDefaultDir;
try {
if (File.Exists(track.sTarget)) {
sFolder = Path.GetDirectoryName(track.sTarget);
sExtension = Path.GetExtension(track.sTarget);
if (string.IsNullOrEmpty(sExtension)) sExtension = ".mp3";
}
}
catch (Exception) { }
if (string.IsNullOrEmpty(sFolder) || !Directory.Exists(sFolder)) sFolder = Path.GetTempPath();

string sLeaf = safeName(track.sName) + " " + stamp(dStart) + " to " + stamp(dEnd) + sExtension;
string sPath = Path.Combine(sFolder, sLeaf);

say(dlg, "Making the clip");
try {
System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
info.FileName = sFfmpeg;
info.Arguments = "-y -ss " + dStart.ToString("0.###", CultureInfo.InvariantCulture)
+ " -to " + dEnd.ToString("0.###", CultureInfo.InvariantCulture)
+ " -i " + Homer.Util.stringQuote(track.sTarget)
+ " -c copy " + Homer.Util.stringQuote(sPath);
info.UseShellExecute = false;
info.CreateNoWindow = true;
Homer.Log.write("Homer Player: " + info.FileName + " " + info.Arguments);
System.Diagnostics.Process oFfmpeg = System.Diagnostics.Process.Start(info);
// A minute is generous for a copy with no re-encoding, and a limit means a
// stalled download cannot leave the dialog waiting for ever.
if (!oFfmpeg.WaitForExit(60000)) {
try { oFfmpeg.Kill(); } catch (Exception) { }
say(dlg, "The clip took too long and was stopped");
return;
}
Homer.Log.write("Homer Player: ffmpeg exit " + oFfmpeg.ExitCode);
if (oFfmpeg.ExitCode != 0 || !File.Exists(sPath)) {
say(dlg, "The clip could not be made. The log has what ffmpeg said.");
return;
}
}
catch (Exception ex) {
Homer.Log.write("Homer Player: clip failed. " + ex.Message);
say(dlg, "The clip could not be made");
return;
}

try {
System.Collections.Specialized.StringCollection lsFiles = new System.Collections.Specialized.StringCollection();
lsFiles.Add(sPath);
Clipboard.SetFileDropList(lsFiles);
say(dlg, "Clip saved as " + sLeaf + " and put on the clipboard");
}
catch (Exception) {
say(dlg, "Clip saved as " + sLeaf);
}
}

// stamp: a time as it can appear in a file name, since a colon cannot.
private static string stamp(double dSeconds) {
return Homer.Mpv.formatTime(dSeconds).Replace(":", "-");
}

// safeName: a track name with the characters Windows will not have in a file
// name taken out.
private static string safeName(string sName) {
StringBuilder sb = new StringBuilder();
foreach (char ch in (sName ?? "")) {
if (Array.IndexOf(Path.GetInvalidFileNameChars(), ch) < 0) sb.Append(ch);
}
string sClean = sb.ToString().Trim();
if (sClean.Length > 60) sClean = sClean.Substring(0, 60).Trim();
return (sClean.Length > 0) ? sClean : "Clip";
}

// ---- the words the dialog says ----

// say: speak it and keep it.
//
// Global, so Scroll Lock -- this dialog's play and pause key -- does not
// silence the player it is driving. The status line keeps every message, so a
// screen reader's say-status-bar key can read back what was said.
private static void say(Homer.LbcDialog dlg, string sText) {
App.say(sText, true);
if (dlg != null) dlg.appendStatus(sText);
}

// statusNote: what is playing and how far in, in one line, for the status bar.
// Read on demand with a screen reader's status key; never spoken.
// THE NOTE CHANGES WHEN SOMETHING CHANGES, NOT WHEN THE CLOCK MOVES.
//
// The position was in it, so it was rewritten twice a second, and a status bar
// that changes twice a second is one a screen reader may decide to read. It now
// carries the track, the count and whether it is playing -- facts that change
// when the person does something -- and the position is left to Alt+Shift+A and
// to the Where in track slider, which are asked rather than announced.
private static string statusNote(Homer.Mpv player, List<MediaTrack> lsTracks) {
int iNow = player.playlistIndex;
StringBuilder sb = new StringBuilder();
if (iNow >= 0 && iNow < lsTracks.Count) {
sb.Append("Track ");
sb.Append((iNow + 1).ToString(CultureInfo.InvariantCulture));
sb.Append(" of ");
sb.Append(lsTracks.Count.ToString(CultureInfo.InvariantCulture));
sb.Append(", ");
sb.Append(lsTracks[iNow].sName);
sb.Append(", ");
}
sb.Append(player.idle ? "stopped" : (player.paused ? "paused" : "playing"));
string sOf = Homer.Mpv.formatTime(player.duration);
if (sOf.Length > 0) { sb.Append(", "); sb.Append(sOf); sb.Append(" long"); }
return sb.ToString();
}

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

// ---- settings ----

private static int readNumber(string sSection, string sKey, int iDefault, int iMinimum, int iMaximum) {
try {
string sPath = settingsPath();
if (!File.Exists(sPath)) return iDefault;
foreach (Homer.InixCodec.Section section in Homer.InixCodec.read(sPath)) {
if (!string.Equals(section.Name, sSection, StringComparison.OrdinalIgnoreCase)) continue;
int iValue;
if (!int.TryParse(section.get(sKey), out iValue)) return iDefault;
if (iValue < iMinimum || iValue > iMaximum) return iDefault;
return iValue;
}
}
catch (Exception ex) { Homer.Log.write("Homer Player: could not read settings. " + ex.Message); }
return iDefault;
}

// Written the moment it changes, so a session that ends any other way still
// leaves the answer behind.
private static void writeValue(string sSection, string sKey, string sValue) {
try { Homer.InixCodec.writeValue(settingsPath(), sSection, sKey, sValue); }
catch (Exception ex) { Homer.Log.write("Homer Player: could not save " + sKey + ". " + ex.Message); }
}

// forgetSettings: remove this queue's section entirely, so the next time it is
// played it starts from the built-in defaults. Deleting beats writing the
// defaults back: a default that changes later should reach a queue that never
// asked for anything else.
private static void forgetSettings(string sSection) {
try {
string sPath = settingsPath();
if (!File.Exists(sPath)) return;
List<Homer.InixCodec.Section> lsKeep = new List<Homer.InixCodec.Section>();
foreach (Homer.InixCodec.Section section in Homer.InixCodec.read(sPath)) {
if (!string.Equals(section.Name, sSection, StringComparison.OrdinalIgnoreCase)) lsKeep.Add(section);
}
Homer.InixCodec.writeAsConfig(sPath, lsKeep);
}
catch (Exception ex) { Homer.Log.write("Homer Player: could not clear settings. " + ex.Message); }
}

// ---- turning what FileDir has into tracks ----

// fromPlaylistLines: the m3u lines FileDir already builds, read back as
// tracks. #EXTINF carries a length and the name a document gave the link, and
// by long convention a name written as "Presenter - Title" means exactly that.
public static List<MediaTrack> fromPlaylistLines(IList<string> lsLines) {
List<MediaTrack> lsTracks = new List<MediaTrack>();
string sPending = "";
double dPending = -1;
foreach (string sRaw in lsLines) {
string sLine = sRaw == null ? "" : sRaw.Trim();
if (sLine.Length == 0) continue;
if (sLine.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase)) {
sPending = "";
dPending = -1;
int iColon = sLine.IndexOf(':');
int iComma = sLine.IndexOf(',');
if (iComma >= 0 && iComma < sLine.Length - 1) sPending = sLine.Substring(iComma + 1).Trim();
if (iColon >= 0 && iComma > iColon) {
double dSeconds;
if (double.TryParse(sLine.Substring(iColon + 1, iComma - iColon - 1).Trim(),
NumberStyles.Float, CultureInfo.InvariantCulture, out dSeconds) && dSeconds > 0) dPending = dSeconds;
}
continue;
}
if (sLine.StartsWith("#")) continue;
MediaTrack track = new MediaTrack(sPending, sLine);
track.dSeconds = dPending;
int iDash = sPending.IndexOf(" - ", StringComparison.Ordinal);
if (iDash > 0 && iDash < sPending.Length - 3) {
track.sPresenter = sPending.Substring(0, iDash).Trim();
track.sName = sPending.Substring(iDash + 3).Trim();
}
lsTracks.Add(track);
sPending = "";
dPending = -1;
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
