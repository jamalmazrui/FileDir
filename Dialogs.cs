// Dialogs.cs - FileDir-local dialog layer, forked from the retired LayoutByCode
// (lbc.dll). The proven WinForms dialog bodies are preserved; utility calls
// delegate to the portable Homer toolkit (Homer.Util) and App speech (App.say).
// This is the last piece that allowed lbc.dll to be dropped entirely.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FileDir {

// NO CONTROL IN HERE SETS AccessibleName.
//
// A screen reader reads a control's accessible name AND the visible text that
// names it. These dialogs used to set the property on 48 controls, always to
// the words already on the button or on the label beside it, so each one was
// announced twice: "Folder colon, Folder colon, edit". A button carries its own
// caption and a text box carries the label before it, and Windows reports both
// without help.
//
// The same clean-out was done in the shared Lbc class. This file is FileDir's
// own older set of dialogs, and it had been missed.
public class Lbc {

// ---- utility shims to the portable Homer toolkit / App speech ----

public static bool Say(object oText) { return App.say(oText); }
public static bool Say(object oText, bool bGlobal) { return App.say(oText, bGlobal); }
public static bool Equiv(string s1, string s2) { return Homer.Util.stringEquiv(s1, s2); }
public static string Pluralize(int iCount, string sItem) { return Homer.Util.stringPlural(sItem, iCount); }
public static string File2String(string sFile) { return Homer.Util.file2String(sFile); }
public static int SetForegroundWindow(int iHandle) { return Homer.Util.setForegroundWindow(iHandle); }
public static object CreateObject(string sProgID) { return Homer.Util.createObject(sProgID); }
public static object CallMethod(object o, string sMethod, object[] aArgs) { return Homer.Util.callMethod(o, sMethod, aArgs); }
public static object CallMethod(object o, string sMethod, string sValue) { return Homer.Util.callMethod(o, sMethod, sValue); }
public static object CallMethod(object o, string sMethod) { return Homer.Util.callMethod(o, sMethod); }
public static object GetProperty(object o, string sProperty) { return Homer.Util.getProperty(o, sProperty); }
public static object SetProperty(object o, string sProperty, object[] aArgs) { return Homer.Util.setProperty(o, sProperty, aArgs); }

public static string Key2String(Keys keyData) {
return TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(keyData);
} // Key2String method

public static Keys String2Key(string sKey) {
return (Keys) TypeDescriptor.GetConverter(typeof(Keys)).ConvertFromString(sKey);
} // String2Key method

public static void Swap(ref int i1, ref int i2) {
int i = i1;
i1 = i2;
i2 = i;
} // Swap method

public static string GetTempFolder() {
object oSystem = CreateObject("Scripting.FileSystemObject");
object oDir = CallMethod(oSystem, "GetSpecialFolder", new object[] {2});
return (string) GetProperty(oDir, "Path");
} // GetTempFolder method

public static bool MapDrive2Share(string sDrive, string sShare) {
bool bResult = false;
NetworkDrive oNetDrive = new NetworkDrive();
try {
oNetDrive.LocalDrive = sDrive + ":";
oNetDrive.ShareName = sShare;
oNetDrive.Force = true;
oNetDrive.Persistent = true;
oNetDrive.PromptForCredentials = true;
oNetDrive.MapDrive();
bResult = true;
}
catch (Exception ex) {
Show(ex.Message, "Error");
bResult = false;
}
oNetDrive = null;
return bResult;
} // MapDrive2Share method

public static string FolderBrowseDialog(string sTitle, string sDefaultDir, bool bNewFolderButton) {
string sReturn = "";
FolderBrowserDialog dlg = new FolderBrowserDialog();
dlg.Description = sTitle;
dlg.ShowNewFolderButton = bNewFolderButton;
dlg.SelectedPath = sDefaultDir;
if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.SelectedPath;
return sReturn;
} // FolderBrowseDialog method

// ---- message boxes ----

public static void Show(object oText) {
Show(oText, "Show");
} // Show method

public static void Show(object oText, object oTitle) {
MessageBox.Show(oText.ToString(), oTitle.ToString());
} // Show method

public static string ConfirmDialog(string sTitle, string sText, string sDefault) {
switch (MessageBox.Show(sText, sTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, (sDefault == "N" ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1))) {
case DialogResult.Yes :
return "Y";
case DialogResult.No :
return "N";
}
return "";
} // ConfirmDialog method

// ---- who a dialog belongs to ----

// ownerForm: the window a dialog should belong to.
//
// These dialogs used to be shown with no owner and then dragged to the front
// with SetForegroundWindow from their own Shown handler. Both halves of that
// were wrong for a screen reader. An unowned dialog is a separate top-level
// window, so opening one reads as leaving the program and arriving somewhere
// new; forcing it to the foreground a second time, after Windows had already
// put it there, raises a second window event, and the reader announces the
// title again. That is how one dialog title came to be spoken more than once.
//
// Giving the dialog an owner also makes CenterParent mean what it says: with
// no owner it silently behaves like CenterScreen.
//
// Form.ActiveForm first, so a dialog opened from inside another dialog belongs
// to that dialog rather than to the main window.
public static Form ownerForm() {
try {
Form frmActive = Form.ActiveForm;
if (frmActive != null && !frmActive.IsDisposed && frmActive.IsHandleCreated) return frmActive;
if (App.frame != null && !App.frame.IsDisposed && App.frame.IsHandleCreated) return App.frame;
}
catch {}
return null;
} // ownerForm method

// ---- file dialogs ----

public static string OpenFileDialog(string sTitle, string sDefaultFile, string sFilter, int iIndex) {
string sReturn = "";
OpenFileDialog dlg = new OpenFileDialog();
if (File.Exists(sDefaultFile)) {
dlg.FileName = sDefaultFile;
string sDefaultDir = Path.GetDirectoryName(sDefaultFile);
if (Directory.Exists(sDefaultDir)) dlg.InitialDirectory = sDefaultDir;
}
dlg.Filter = sFilter;
dlg.ShowReadOnly = false;
dlg.ReadOnlyChecked = false;
dlg.RestoreDirectory = true;
if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.FileName;
return sReturn;
} // OpenFileDialog method

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
dlg.RestoreDirectory = true;
dlg.ValidateNames = true;
if (dlg.ShowDialog() == DialogResult.OK) sReturn = dlg.FileName;
return sReturn;
} // SaveFileDialog method

// ---- input / field dialogs ----

public static string InputDialog(string sTitle, string sLabel, string sValue) {
return InputDialog(sTitle, sLabel, sValue, null);
} // InputDialog method

// InputDialog with input history: when sHistoryKey is given (for example
// "Jump"), the input control is an editable combo box whose dropdown holds
// up to historyCount recent entries for that command, newest first. The
// entries persist through the FileDir settings layer in section
// [Recent<key>] as slot keys term1, term2, and so on, the same layout DbDo
// uses. [General] historyCount sets the depth; default 10, ceiling 100.
// Password prompts and callers that pass no key keep the plain text box,
// so passwords are never recorded.
public static string InputDialog(string sTitle, string sLabel, string sValue, string sHistoryKey) {
// One line of input, built on the kit's LbcDialog.
//
// With a history key the field is an editable combo whose list holds the
// recent answers for that command, newest first -- the Windows Run-dialog
// pattern. The history is kept where it always was, in [Recent<key>] of
// FileDir.ini as term1, term2 and so on, so nobody loses what they had typed.
// A label containing "Password" gets a masked box and no history, so a
// password is never recorded. Returns the text, or "" when cancelled.
string sPrompt = fieldLabel(sLabel);
bool bPassword = sPrompt.Contains("Password");
bool bHistory = !string.IsNullOrEmpty(sHistoryKey) && !bPassword;
int iCount = Homer.InputHistory.DefaultCount;
string sSection = null;
List<string> lsRecent = null;
if (bHistory) {
iCount = Homer.InputHistory.clampCount(App.readValue(App.sIniFile, "General", "historyCount", ""));
sSection = "Recent" + sHistoryKey;
lsRecent = Homer.InputHistory.load(delegate(string sKey) { return App.readValue(App.sIniFile, sSection, sKey, ""); }, iCount);
}
string sResult = "";
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, ownerForm())) {
ComboBox cmb = null;
TextBox txt = null;
if (bHistory) cmb = dlg.addComboHistoryBox(sPrompt, lsRecent, sValue ?? "", null);
else {
txt = dlg.addInputBox(sPrompt, sValue ?? "", null);
if (bPassword) txt.UseSystemPasswordChar = true;
}
if (!dlg.runOkCancel()) {
Say("Cancel", true);
return "";
}
sResult = (cmb != null) ? cmb.Text : txt.Text;
}
if (bHistory && sResult != null && sResult.Trim().Length > 0) {
lsRecent = Homer.InputHistory.push(lsRecent, sResult.Trim(), iCount);
Homer.InputHistory.store(lsRecent, delegate(string sKey, string sVal) { App.writeValue(App.sIniFile, sSection, sKey, sVal); }, iCount);
}
return sResult;
} // InputDialog method (history overload)

// fieldLabel: a label as a field shows it, with the colon these dialogs have
// always had, and no second colon when the caller wrote one.
private static string fieldLabel(string sLabel) {
string sText = (sLabel ?? "").Trim();
if (sText.Length == 0) return "";
return sText.EndsWith(":") ? sText : sText + ":";
} // fieldLabel method

public static ArrayList FieldDialog(string sTitle, string[] sLabelList, string[] sValueList) {
return FieldDialog(sTitle, sLabelList, sValueList, false);
} // FieldDialog method

public static ArrayList FieldDialog(string sTitle, string[] sLabelList, string[] sValueList, bool bPassword) {
// Several lines of input at once, built on the kit's LbcDialog: one labelled
// field per value, in order. Returns the values in the same order, or an
// empty list when the dialog is cancelled, as it always has.
//
// A label containing "Password" gets a masked box, as before. The count in the
// title is kept too -- "Rename (3)" -- unless the caller ends the title with a
// space, which is how a caller has always asked for no count.
ArrayList sResultList = new ArrayList();
if (sLabelList == null || sLabelList.Length == 0) return sResultList;
if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + sValueList.Length + ")";
List<TextBox> lsFields = new List<TextBox>();
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle.TrimEnd(), ownerForm())) {
for (int i = 0; i < sLabelList.Length; i++) {
string sPrompt = fieldLabel(sLabelList[i]);
string sValue = (sValueList != null && i < sValueList.Length) ? sValueList[i] : "";
TextBox txt = dlg.addInputBox(sPrompt, sValue ?? "", null);
if (sPrompt.Contains("Password")) txt.UseSystemPasswordChar = true;
lsFields.Add(txt);
}
if (!dlg.runOkCancel()) {
Say("Cancel", true);
return sResultList;
}
foreach (TextBox txt in lsFields) sResultList.Add(txt.Text);
}
return sResultList;
} // FieldDialog method

public static List<string> ListInputDialog(string sTitle, string sListLabel, string[] sValueList, string sInputLabel, string sValue, bool bSorted, int iDefaultIndex) {
// A choice and a value together -- the units and the number, say. Built on the
// kit's LbcDialog. Returns two strings, the item chosen and the text typed, or
// an empty list when cancelled.
List<string> listResults = new List<string>();
List<string> lsItems = new List<string>(sValueList ?? new string[0]);
if (bSorted) lsItems = Homer.LbcDialog.sortedIgnoringCase(lsItems);
string sSelected = (iDefaultIndex >= 0 && iDefaultIndex < lsItems.Count) ? lsItems[iDefaultIndex] : (lsItems.Count > 0 ? lsItems[0] : "");
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, ownerForm())) {
ListBox lb = dlg.addPickBox(fieldLabel(sListLabel), lsItems, sSelected, null);
dlg.primaryList = lb;
string sPrompt = fieldLabel(sInputLabel);
TextBox txt = dlg.addInputBox(sPrompt, sValue ?? "", null);
if (sPrompt.Contains("Password")) txt.UseSystemPasswordChar = true;
if (!dlg.runOkCancel()) {
Say("Cancel", true);
return listResults;
}
listResults.Add(lb.SelectedItem == null ? "" : lb.SelectedItem.ToString());
listResults.Add(txt.Text);
}
return listResults;
} // ListInputDialog method

// ---- info dialog ----

public static void InfoDialog(string sTitle, string sValue, bool bSelectText) {
// A report to read: the kit's read-only viewer. It opens on the first line
// rather than with everything selected, which is the Homer rule for a text box
// -- bSelectText is kept in the signature so no caller changes, and Control+A
// still selects the lot when that is what is wanted.
Homer.HelpDialog.show(ownerForm(), sTitle, sValue ?? "");
} // InfoDialog method

// ---- answer dialog ----

public static void AnswerDialog(string sTitle, string sLabel, string sText) {
// A long answer to read, move around in and copy from: the kit's read-only
// viewer, which opens on the first line, keeps Control+C, and closes on Enter
// or Escape. The label, when there is one, becomes the first line, since the
// window title already says what the dialog is.
string sBody = string.IsNullOrEmpty(sLabel) ? (sText ?? "") : (sLabel + "\r\n\r\n" + (sText ?? ""));
Homer.HelpDialog.show(ownerForm(), sTitle, sBody);
} // AnswerDialog method

// ---- button dialog ----

public static string ButtonDialog(string sTitle, string sText, string[] sButtonList, int iDefaultButton) {
// A question with a row of answers. Built on the kit's LbcDialog.
//
// THE CONTRACT IS KEPT EXACTLY, because every caller switches on it: the
// answer comes back as the caller wrote it, ampersand and all ("&Yes"), and a
// cancel comes back as "". The kit returns the plain label, so the plain label
// is matched back to the caller's own.
if (sButtonList == null || sButtonList.Length == 0) return "";
List<string> lsButtons = new List<string>(sButtonList);
lsButtons.Add("Cancel");
int iDefault = (iDefaultButton >= 0 && iDefaultButton < sButtonList.Length) ? iDefaultButton : 0;
string sDefault = sButtonList[iDefault].Replace("&", "");
string sPressed;
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, ownerForm())) {
if (!string.IsNullOrEmpty(sText)) dlg.addLabel(sText);
sPressed = dlg.runWithButtons(lsButtons.ToArray(), false, sDefault);
}
if (string.IsNullOrEmpty(sPressed) || sPressed == "Cancel") {
Say("Cancel", true);
return "";
}
foreach (string sButton in sButtonList) {
if (sButton.Replace("&", "") == sPressed) return sButton;
}
return sPressed;
} // ButtonDialog method

public static object[] ListButtonDialog(string sTitle, object[] aValue, string[] aDisplay, string[] aButton, bool bSort, int iIndex) {
// A list and a row of actions to take on the item chosen. Built on the kit's
// LbcDialog. Returns the item's value and the button's label exactly as the
// caller wrote it ("&Activate"), since callers switch on that; an empty array
// when cancelled. The count goes in the title unless it ends with a space.
object[] aResult = {};
if (aValue == null || aValue.Length == 0 || aButton == null || aButton.Length == 0) return aResult;
List<string> lsItems = new List<string>();
if (aDisplay == null) foreach (object oItem in aValue) lsItems.Add(System.Convert.ToString(oItem));
else lsItems.AddRange(aDisplay);
if (bSort) lsItems = Homer.LbcDialog.sortedIgnoringCase(lsItems);
string sSelected = (iIndex >= 0 && iIndex < lsItems.Count) ? lsItems[iIndex] : lsItems[0];
if (sTitle.Length > 0 && sTitle.Length == sTitle.TrimEnd().Length) sTitle += " (" + aValue.Length + ")";
List<string> lsButtons = new List<string>(aButton);
lsButtons.Add("Cancel");
string sPressed;
string sChosen = "";
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle.TrimEnd(), ownerForm())) {
ListBox lb = dlg.addListBox(lsItems, sSelected, null);
dlg.primaryList = lb;
sPressed = dlg.runWithButtons(lsButtons.ToArray(), false, aButton[0].Replace("&", ""));
if (lb.SelectedItem != null) sChosen = lb.SelectedItem.ToString();
}
if (string.IsNullOrEmpty(sPressed) || sPressed == "Cancel") {
Say("Cancel", true);
return aResult;
}
string sButton = sPressed;
foreach (string sOne in aButton) if (sOne.Replace("&", "") == sPressed) sButton = sOne;
object oValue;
if (aDisplay == null) oValue = sChosen;
else {
int iValue = Array.IndexOf(aDisplay, sChosen);
if (iValue < 0) return aResult;
oValue = aValue[iValue];
}
Say(sPressed, true);
return new object[] {oValue, sButton};
} // ListButtonDialog method

// ---- multi-select list dialogs ----

public static ArrayList MultiListDialog(string sTitle, string sLabel, string[] sValueList, bool bSorted, int iDefaultIndex, int[] iSelectList) {
// Choose several items from a list. A CHECKED list rather than a multi-select
// one: a screen reader says "checked" and "not checked" for each item, where a
// multi-select list leaves the person to remember which lines Space turned on.
// Built on the kit's LbcDialog. The indexes to pre-check and to start on refer
// to the list as shown, sorted or not, as they always have. Returns the chosen
// items in list order, or an empty list when cancelled.
ArrayList sResultList = new ArrayList();
if (sValueList == null || sValueList.Length == 0) return sResultList;
List<string> lsItems = new List<string>(sValueList);
if (bSorted) lsItems = Homer.LbcDialog.sortedIgnoringCase(lsItems);
List<int> lsChecked = new List<int>();
if (iSelectList != null) foreach (int iAt in iSelectList) if (iAt >= 0 && iAt < lsItems.Count) lsChecked.Add(iAt);
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, ownerForm())) {
CheckedListBox clb = string.IsNullOrEmpty(sLabel)
? dlg.addCheckListBox(lsItems, lsChecked, null)
: dlg.addCheckListBox(fieldLabel(sLabel), lsItems, lsChecked, null);
if (iDefaultIndex >= 0 && iDefaultIndex < clb.Items.Count) clb.SelectedIndex = iDefaultIndex;
if (!dlg.runOkCancel()) {
Say("Cancel", true);
return sResultList;
}
for (int i = 0; i < clb.Items.Count; i++) if (clb.GetItemChecked(i)) sResultList.Add(clb.Items[i].ToString());
}
return sResultList;
} // MultiListDialog method

public static List<int> MultiListDialog(string sTitle, string[] aValues, bool bSorted) {
// Choose several items; the answer is their positions in the list as shown.
// A checked list, built on the kit's LbcDialog. Empty when cancelled.
List<int> listResults = new List<int>();
if (aValues == null || aValues.Length == 0) return listResults;
List<string> lsItems = new List<string>(aValues);
if (bSorted) lsItems = Homer.LbcDialog.sortedIgnoringCase(lsItems);
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, ownerForm())) {
CheckedListBox clb = dlg.addCheckListBox(lsItems, new List<int>(), null);
if (!dlg.runOkCancel()) {
Say("Cancel", true);
return listResults;
}
for (int i = 0; i < clb.Items.Count; i++) if (clb.GetItemChecked(i)) listResults.Add(i);
}
return listResults;
} // MultiListDialog method

// ---- list / check dialogs (delegate to Dialog helpers) ----

public static string ListDialog(string sTitle, string sLabel, string[] sValueList, bool bSorted, int iDefaultIndex) {
// Pick one item from a list. Built on the kit's LbcDialog, so the list gets
// what every Homer list has: Control+J to jump by name, Control+K to search,
// Control+F to filter, F3 to repeat, and F1 for help on the dialog.
//
// With no label the list names itself, which is the one case the kit gives a
// control an accessible name; with one, the label before it does that job.
// Returns the chosen item, or "" when the dialog is cancelled.
if (sValueList == null || sValueList.Length == 0) return "";
List<string> lsItems = new List<string>(sValueList);
if (bSorted) lsItems = Homer.LbcDialog.sortedIgnoringCase(lsItems);
string sSelected = (iDefaultIndex >= 0 && iDefaultIndex < lsItems.Count) ? lsItems[iDefaultIndex] : lsItems[0];
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, ownerForm())) {
ListBox lb = string.IsNullOrEmpty(sLabel)
? dlg.addListBox(lsItems, sSelected, null)
: dlg.addPickBox(fieldLabel(sLabel), lsItems, sSelected, null);
dlg.primaryList = lb;
if (!dlg.runOkCancel()) return "";
return (lb.SelectedItem == null) ? "" : lb.SelectedItem.ToString();
}
} // ListDialog method

public static string[] MultiCheckDialog(string sTitle, string[] aValues, int[] aSelect, bool bSort, int iIndex) {
// Check several items; the answer is the checked items themselves. Built on
// the kit's LbcDialog. Empty when cancelled.
if (aValues == null || aValues.Length == 0) return new string[0];
List<string> lsItems = new List<string>(aValues);
if (bSort) lsItems = Homer.LbcDialog.sortedIgnoringCase(lsItems);
List<int> lsChecked = new List<int>();
if (aSelect != null) foreach (int iAt in aSelect) if (iAt >= 0 && iAt < lsItems.Count) lsChecked.Add(iAt);
List<string> lsResult = new List<string>();
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, ownerForm())) {
CheckedListBox clb = dlg.addCheckListBox(lsItems, lsChecked, null);
if (iIndex >= 0 && iIndex < clb.Items.Count) clb.SelectedIndex = iIndex;
if (!dlg.runOkCancel()) {
Say("Cancel", true);
return new string[0];
}
for (int i = 0; i < clb.Items.Count; i++) if (clb.GetItemChecked(i)) lsResult.Add(clb.Items[i].ToString());
}
return lsResult.ToArray();
} // MultiCheckDialog method

// ---- special folder picker ----

[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
private static extern int SHGetKnownFolderPath(
[MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

// The known folders, by their official identifier and the name Windows uses
// for each, with a qualifier added where two would otherwise read alike.
//
// WHY A LIST RATHER THAN AN ENUMERATION. This used to walk Shell.Application
// namespaces 0 to 99 and then the Environment.SpecialFolder enumeration, and
// take whatever names those gave. The two disagree about naming, so the list
// mixed "Documents" with "MyDocuments" and "CommonApplicationData"; it offered
// whatever numbered slots happened to exist on that machine; and it could not
// offer Downloads at all, because .NET's enumeration has no member for it.
//
// SHGetKnownFolderPath is the modern way and knows every one of these. A folder
// that does not exist on this machine is left out, so an old Windows shows a
// shorter list rather than broken entries.
//
// THE NAMES ARE UNIQUE, case insensitively, and the list is written already in
// the order it will be shown. "Documents" and "Documents, Public" sort next to
// each other and both begin with the word a person is looking for, which
// matters when the list is being read aloud or navigated by first letter.
private static readonly string[,] c_aKnownFolders = {
{"3D Objects", "31C0DD25-9439-4F12-BF41-7FF4EDA38722"},
{"Administrative Tools", "724EF170-A42D-4FEF-9F26-B60E846FBA4F"},
{"Administrative Tools, Common", "D0384E7D-BAC3-4797-8F14-CBA229B392B5"},
{"Application Data, Local", "F1B32785-6FBA-4FCF-9D55-7B8E7F157091"},
{"Application Data, LocalLow", "A520A1A4-1780-4FF6-BD18-167343C5AF16"},
{"Application Data, Roaming", "3EB685DB-65F9-4CF6-A03A-E3EF65729F3D"},
{"Common Files", "F7F1ED05-9F6D-47A2-AAAE-29D317C6F066"},
{"Common Files, 32-bit", "DE974D24-D9C6-4D3E-BF91-F4455120B917"},
{"Contacts", "56784854-C6CB-462B-8169-88E350ACB882"},
{"Desktop", "B4BFCC3A-DB2C-424C-B029-7FE99A87C641"},
{"Desktop, Public", "C4AA340D-F20F-4863-AFEF-F87EF2E6BA25"},
{"Documents", "FDD39AD0-238F-46AF-ADB4-6C85480369C7"},
{"Documents, Public", "ED4824AF-DCE4-45A8-81E2-FC7965083634"},
{"Downloads", "374DE290-123F-4565-9164-39C4925E467B"},
{"Downloads, Public", "3D644C9B-1FB8-4F30-9B45-F670235F79C0"},
{"Favorites", "1777F761-68AD-4D8A-87BD-30B759FA33DD"},
{"Fonts", "FD228CB7-AE11-4AE3-864C-16F3910AB8FE"},
{"History, Internet", "D9DC8A3B-B784-432E-A781-5A1130A75963"},
{"Links", "BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968"},
{"Music", "4BD8D571-6D19-48D3-BE97-422220080E43"},
{"Music, Public", "3214FAB5-9757-4298-BB61-92A9DEAA44FF"},
{"Network Shortcuts", "C5ABBF53-E17F-4121-8900-86626FC2C973"},
{"OneDrive", "A52BBA46-E9E1-435F-B3D9-28DAA648C0F6"},
{"Pictures", "33E28130-4E1E-4676-835A-98395C3BC3BB"},
{"Pictures, Public", "B6EBFB86-6907-413C-9AF7-4FC2ABF07CC5"},
{"Printer Shortcuts", "9274BD8D-CFD1-41C3-B35E-B13F55A758F4"},
{"Program Data", "62AB5D82-FDC1-4DC3-A9DD-070D1D495D97"},
{"Program Files", "905E63B6-C1BF-494E-B29C-65B732D3D21A"},
{"Program Files, 32-bit", "7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E"},
{"Programs, Start Menu", "A77F5D77-2E2B-44C3-A6A2-ABA601054A51"},
{"Programs, Start Menu, Common", "0139D44E-6AFE-49F2-8690-3DAFCAE6FFB8"},
{"Public", "DFDF76A2-C82A-4D63-906A-5644AC457385"},
{"Quick Launch", "52A4F021-7B75-48A9-9F6B-4B87A210BC8F"},
{"Recent Items", "AE50C081-EBD2-438A-8655-8A092E34987A"},
{"Saved Games", "4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"},
{"Searches", "7D1D3A04-DEBB-4115-95CF-2F29DA2920DA"},
{"Send To", "8983036C-27C0-404B-8F08-102D10DCFD74"},
{"Start Menu", "625B53C3-AB48-4EC1-BA1F-A1EF4146FC19"},
{"Start Menu, Common", "A4115719-D62E-491D-AA7C-E74B8BE3B067"},
{"Startup", "B97D20BB-F46A-4C97-BA10-5E3608430854"},
{"Startup, Common", "82A5EA35-D9CD-47C5-9629-E15D2F714E6E"},
{"System32", "1AC14E77-02E7-4E5D-B744-2EB1AE5198B7"},
{"System32, 32-bit", "D65231B0-B2F1-4857-A4CE-A8E7C6EA7D27"},
{"Templates", "A63293E8-664E-48DB-A079-DF759E0509F7"},
{"Templates, Common", "B94237E7-57AC-4347-9151-B08C6C32D1F7"},
{"Temporary Internet Files", "352481E8-33BE-4251-BA85-6007CAEDCF9D"},
{"User Profile", "5E6C858F-0E22-4760-9AFE-EA3317B67173"},
{"Videos", "18989B1D-99B5-455B-841C-AB7C74E4DDFC"},
{"Videos, Public", "2400183A-6185-49FB-A2D8-4A392A602BA3"},
{"Windows", "F38BF404-1D43-42F2-9305-67DE0B28FC23"},
};

public static string PickSpecialFolder() {
List<string> lsNames = new List<string>();
List<string> lsPaths = new List<string>();
List<string> lsSeen = new List<string>();

for (int i = 0; i < c_aKnownFolders.GetLength(0); i++) {
string sPath = KnownFolderPath(c_aKnownFolders[i, 1]);
if (sPath.Length == 0) continue;
if (!Directory.Exists(sPath)) continue;
// Several identifiers can point at one folder on some installations, and the
// same place twice under two names is a list nobody wants to read.
string sKey = sPath.ToLower().TrimEnd('\\');
if (lsSeen.Contains(sKey)) continue;
lsSeen.Add(sKey);
lsNames.Add(c_aKnownFolders[i, 0]);
lsPaths.Add(sPath);
}

// The temporary folder is not a known folder, and is worth having.
string sTemp = GetTempFolder();
if (sTemp.Length > 0 && Directory.Exists(sTemp)
&& !lsSeen.Contains(sTemp.ToLower().TrimEnd('\\'))) {
lsNames.Add("Temporary Files");
lsPaths.Add(sTemp);
}

if (lsNames.Count == 0) return "";
string[] aNames = lsNames.ToArray();
string[] aPaths = lsPaths.ToArray();
// Sorted by the dialog, which does it case insensitively.
bool bSorted = true;
string sName = ListDialog("Pick", "", aNames, bSorted, 0);
if (sName.Length == 0) return "";
int iName = Array.IndexOf(aNames, sName);
if (iName < 0) return "";
return aPaths[iName];
} // PickSpecialFolder method

private static string KnownFolderPath(string sGuid) {
// The folder for one identifier, or an empty string when this Windows has no
// such folder. Never throws: an unknown identifier on an older Windows is an
// ordinary outcome, not a fault.
IntPtr pPath = IntPtr.Zero;
try {
if (SHGetKnownFolderPath(new Guid(sGuid), 0, IntPtr.Zero, out pPath) != 0) return "";
return Marshal.PtrToStringUni(pPath);
}
catch (Exception) {
return "";
}
finally {
if (pPath != IntPtr.Zero) Marshal.FreeCoTaskMem(pPath);
}
} // KnownFolderPath method

// ---- directory dialog (FileDir-specific: Current/Recent/Quick/Special) ----

public static string DirectoryDialog(string sTitle, string sLabel, string sValue) {
// A folder: typed, browsed for, or picked from the folders open now, the
// folders visited recently, the Quick folders, or the special folders. Built
// on the kit's LbcDialog with runPlain, because five of its buttons do work and
// leave the dialog open rather than closing it.
//
// Behaviour kept exactly: the typed box completes folder names; OK on a UNC
// path offers to map it to a free drive letter; OK on a folder that does not
// exist offers to create it; OK on nothing leaves the dialog open. Returns the
// folder, or "" when cancelled.
string sResult = "";
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, ownerForm())) {
TextBox txt = dlg.addInputBox(fieldLabel(sLabel), sValue ?? "", null);
txt.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
txt.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;

dlg.addBand();
Button btnBrowse = dlg.addButton("&Browse", "Browse for the folder in the Windows folder picker");
Button btnCurrent = dlg.addButton("&Current", "Pick from the folders open in FileDir now");
Button btnRecent = dlg.addButton("&Recent", "Pick from the folders visited recently");
Button btnQuick = dlg.addButton("&Quick", "Pick from the Quick folders");
Button btnSpecial = dlg.addButton("&Special", "Pick a special folder, such as Documents or Downloads");
dlg.endBand();

dlg.addBand();
Button btnOK = dlg.addButton("OK");
Button btnCancel = dlg.addButton("Cancel");
dlg.endBand();

btnBrowse.Click += delegate(object o, EventArgs e) {
string sPicked = FolderBrowseDialog("", txt.Text.Trim().Length > 0 ? txt.Text.Trim() : sValue, false);
if (sPicked.Length > 0) txt.Text = sPicked;
txt.Select();
};
btnCurrent.Click += delegate(object o, EventArgs e) {
List<string> lsDirs = new List<string>();
foreach (MdiChild child in App.frame.MdiChildren) if (Directory.Exists(child.Text)) lsDirs.Add(child.Text);
string sDir = pickFolder(lsDirs);
if (sDir.Length == 0) return;
sResult = sDir;
dlg.close();
};
btnRecent.Click += delegate(object o, EventArgs e) {
string sDir = pickFolder(new List<string>(App.lsRecentDirs));
if (sDir.Length == 0) return;
sResult = sDir;
dlg.close();
};
btnQuick.Click += delegate(object o, EventArgs e) {
List<string> lsDirs = new List<string>();
try {
string sQuickDir = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FileDir\Quick");
foreach (string sLink in Directory.GetFiles(sQuickDir, "*.lnk")) {
object oLink = CreateObject("WScript.Shell");
oLink = CallMethod(oLink, "CreateShortcut", new object[] {sLink});
string sDir = (string) GetProperty(oLink, "TargetPath");
if (Directory.Exists(sDir)) lsDirs.Add(sDir);
}
}
catch (Exception) {}
string sPicked = pickFolder(lsDirs);
if (sPicked.Length == 0) return;
sResult = sPicked;
dlg.close();
};
btnSpecial.Click += delegate(object o, EventArgs e) {
string sDir = PickSpecialFolder();
if (sDir.Length == 0) return;
sResult = sDir;
dlg.close();
};
btnOK.Click += delegate(object o, EventArgs e) {
string sTyped = txt.Text.Trim();
if (sTyped != "" && !Directory.Exists(sTyped)) {
if (sTyped.StartsWith(@"\\")) {
string sUnmapped = "A B C D E F G H I J K L M N O P Q R S T U V W X Y Z ";
foreach (DriveInfo d in DriveInfo.GetDrives()) sUnmapped = sUnmapped.Replace(d.Name.Substring(0, 1) + " ", "");
string sDrive = ListDialog("Pick Drive to Map", "", sUnmapped.Trim().Split(' '), true, 0);
if (sDrive.Length == 0) return;
try { MapDrive2Share(sDrive, sTyped); }
catch (Exception ex) { Show(ex.Message, "Error"); }
}
else if (ConfirmDialog("Confirm", "Cannot find folder " + sTyped + "\nCreate it?", "Y") == "Y") {
try { new DirectoryInfo(sTyped).Create(); }
catch (Exception ex) { Show(ex.Message, "Error"); }
}
}
if (Directory.Exists(sTyped)) {
sResult = sTyped;
dlg.close();
}
else {
txt.SelectAll();
txt.Select();
}
};
btnCancel.Click += delegate(object o, EventArgs e) {
Say("Cancel", true);
sResult = "";
dlg.close();
};
dlg.runPlain(btnOK, btnCancel);
}
return sResult;
} // DirectoryDialog method

// pickFolder: choose one folder from a list, shown by its own name -- or by its
// full path for a drive root, whose name would be empty. "" when cancelled or
// when there is nothing to choose from.
private static string pickFolder(List<string> lsDirs) {
if (lsDirs == null || lsDirs.Count == 0) return "";
List<string> lsNames = new List<string>();
foreach (string sDir in lsDirs) lsNames.Add(sDir.EndsWith(@":\") ? sDir : Path.GetFileName(sDir));
string sName = ListDialog("Pick", "", lsNames.ToArray(), true, 0);
if (sName.Length == 0) return "";
int iAt = lsNames.IndexOf(sName);
return (iAt >= 0) ? lsDirs[iAt] : "";
} // pickFolder method

} // Lbc class

// ===========================================================================
// Dialog - filtered/sorted/jumpable list and input helpers (from lbc.cs)
// ===========================================================================

public class Dialog {
public static string Jump = "";
public static Dictionary<string, string> hashItem = new Dictionary<string, string>();
public static Dictionary<string, string> hashFilter = new Dictionary<string, string>();
public static Dictionary<string, string> hashSort = new Dictionary<string, string>();
public static Dictionary<string, string> hashJump = new Dictionary<string, string>();

public static void Show(object oText) {
Show("Show", oText);
} // Show method

public static void Show(object oTitle, object oText) {
MessageBox.Show(oText.ToString(), oTitle.ToString());
} // Show method



public static string Pick(string sTitle, string[] aValue, bool bSort) {
return Pick(sTitle, aValue, null, bSort, 0);
} // Pick method

public static string Pick(string sTitle, string[] aValue, bool bSort, int iIndex) {
return Pick(sTitle, aValue, null, bSort, iIndex);
} // Pick method

public static string Pick(string sTitle, string[] aValue, string[] aDisplay, bool bSort, int iIndex) {
// Pick one item, shown by one name and answered with another: aDisplay is
// what the list says, aValue what comes back. With no aDisplay the two are the
// same. Built on the kit's LbcDialog, so the list has jump, search and filter.
// Returns the value, or "" when cancelled.
if (aValue == null || aValue.Length == 0) return "";
string[] aShown = (aDisplay != null && aDisplay.Length == aValue.Length) ? aDisplay : aValue;
List<int> lsOrder = new List<int>();
for (int i = 0; i < aShown.Length; i++) lsOrder.Add(i);
if (bSort) lsOrder.Sort(delegate(int a, int b) { return string.Compare(aShown[a], aShown[b], StringComparison.OrdinalIgnoreCase); });
List<string> lsNames = new List<string>();
foreach (int iAt in lsOrder) lsNames.Add(aShown[iAt]);
string sSelected = (iIndex >= 0 && iIndex < lsNames.Count) ? lsNames[iIndex] : lsNames[0];
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, Lbc.ownerForm())) {
ListBox lb = dlg.addListBox(lsNames, sSelected, null);
dlg.primaryList = lb;
if (!dlg.runOkCancel()) {
Lbc.Say("Cancel", true);
return "";
}
// By the text chosen, not the row: a filtered list (Control+F) shows fewer
// rows than it holds, so a row number would point at the wrong item.
if (lb.SelectedItem == null) return "";
int iRow = lsNames.IndexOf(lb.SelectedItem.ToString());
if (iRow < 0) return "";
return aValue[lsOrder[iRow]];
}
} // Pick method



public static string Choose(string sTitle, string sText, string[] aButtons, int iDefault) {
// A question with a row of answers, as ButtonDialog but for callers that list
// their own Cancel. Built on the kit's LbcDialog. The answer comes back as the
// caller wrote it ("&Try anyway"); Escape gives the caller's own "Cancel" when
// it listed one, and "" otherwise. The answer chosen is spoken, as before.
if (aButtons == null || aButtons.Length == 0) return "";
List<string> lsButtons = new List<string>(aButtons);
bool bOwnCancel = false;
foreach (string sOne in aButtons) if (sOne.Replace("&", "") == "Cancel") bOwnCancel = true;
if (!bOwnCancel) lsButtons.Add("Cancel");
int iAt = (iDefault >= 0 && iDefault < aButtons.Length) ? iDefault : 0;
string sPressed;
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, Lbc.ownerForm())) {
if (!string.IsNullOrEmpty(sText)) dlg.addLabel(sText);
sPressed = dlg.runWithButtons(lsButtons.ToArray(), false, aButtons[iAt].Replace("&", ""));
}
if (string.IsNullOrEmpty(sPressed)) sPressed = "Cancel";
if (sPressed == "Cancel" && !bOwnCancel) return "";
foreach (string sOne in aButtons) {
if (sOne.Replace("&", "") == sPressed) {
Lbc.Say(sPressed);
return sOne;
}
}
return "";
} // Choose method

} // Dialog class

// ===========================================================================
// NetworkDrive - map/unmap UNC shares (aejw.com, CC BY-SA 2.5); used by
// DirectoryDialog when the user enters a UNC path.
// ===========================================================================

public class NetworkDrive {

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
private const int CONNECT_INTERACTIVE = 0x00000008;
private const int CONNECT_PROMPT = 0x00000010;
private const int CONNECT_UPDATE_PROFILE = 0x00000001;
private const int CONNECT_REDIRECT = 0x00000080;
private const int CONNECT_COMMANDLINE = 0x00000800;
private const int CONNECT_CMD_SAVECRED = 0x00001000;

private bool lf_SaveCredentials = false;
public bool SaveCredentials{
get{return(lf_SaveCredentials);}
set{lf_SaveCredentials=value;}
}
private bool lf_Persistent = false;
public bool Persistent{
get{return(lf_Persistent);}
set{lf_Persistent=value;}
}
private bool lf_Force = false;
public bool Force{
get{return(lf_Force);}
set{lf_Force=value;}
}
private bool ls_PromptForCredentials = false;
public bool PromptForCredentials{
get{return(ls_PromptForCredentials);}
set{ls_PromptForCredentials=value;}
}

private string ls_Drive = "s:";
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
private string ls_ShareName = @"\\Computer\C$";
public string ShareName{
get{return(ls_ShareName);}
set{ls_ShareName=value;}
}

public void MapDrive(){zMapDrive(null, null);}
public void MapDrive(string Password){zMapDrive(null, Password);}
public void MapDrive(string Username, string Password){zMapDrive(Username, Password);}
public void UnMapDrive(){zUnMapDrive(this.lf_Force);}
public void RestoreDrives(){zRestoreDrive();}
public void ShowConnectDialog(Form ParentForm){zDisplayDialog(ParentForm,1);}
public void ShowDisconnectDialog(Form ParentForm){zDisplayDialog(ParentForm,2);}

private void zMapDrive(string psUsername, string psPassword){
structNetResource stNetRes = new structNetResource();
stNetRes.iScope=2;
stNetRes.iType=RESOURCETYPE_DISK;
stNetRes.iDisplayType=3;
stNetRes.iUsage=1;
stNetRes.sRemoteName=ls_ShareName;
stNetRes.sLocalName=ls_Drive;
int iFlags=0;
if(lf_SaveCredentials){iFlags+=CONNECT_CMD_SAVECRED;}
if(lf_Persistent){iFlags+=CONNECT_UPDATE_PROFILE;}
if(ls_PromptForCredentials){iFlags+=CONNECT_INTERACTIVE+CONNECT_PROMPT;}
if(psUsername==""){psUsername=null;}
if(psPassword==""){psPassword=null;}
if(lf_Force){try{zUnMapDrive(true);}catch{}}
int i = WNetAddConnection2A(ref stNetRes, psPassword, psUsername, iFlags);
if(i>0){throw new System.ComponentModel.Win32Exception(i);}
}

private void zUnMapDrive(bool pfForce){
int iFlags=0;
if(lf_Persistent){iFlags+=CONNECT_UPDATE_PROFILE;}
int i = WNetCancelConnection2A(ls_Drive, iFlags, Convert.ToInt32(pfForce));
if(i!=0) i=WNetCancelConnection2A(ls_ShareName, iFlags, Convert.ToInt32(pfForce));
if(i>0){throw new System.ComponentModel.Win32Exception(i);}
}

private void zRestoreDrive()
{
int i = WNetRestoreConnectionW(0, null);
if(i>0){throw new System.ComponentModel.Win32Exception(i);}
}

private void zDisplayDialog(Form poParentForm, int piDialog)
{
int i = -1;
int iHandle = 0;
if(poParentForm!=null)
{
iHandle = poParentForm.Handle.ToInt32();
}
if(piDialog==1)
{
i = WNetConnectionDialog(iHandle, RESOURCETYPE_DISK);
}else if(piDialog==2)
{
i = WNetDisconnectDialog(iHandle, RESOURCETYPE_DISK);
}
if(i>0){throw new System.ComponentModel.Win32Exception(i);}
if(poParentForm!=null) poParentForm.BringToFront();
}

} // NetworkDrive class

} // FileDir namespace
