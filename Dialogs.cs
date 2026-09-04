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
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

Label lbl = new Label();
lbl.AutoSize = true;
lbl.Text = sLabel + ":";
lbl.AccessibleName = lbl.Text.Replace("&", "");
bool bPassword = lbl.Text.Contains("Password:");
bool bHistory = !string.IsNullOrEmpty(sHistoryKey) && !bPassword;
int iCount = Homer.InputHistory.DefaultCount;
string sSection = null;
List<string> lsRecent = null;
if (bHistory) {
iCount = Homer.InputHistory.clampCount(App.readValue(App.sIniFile, "General", "historyCount", ""));
sSection = "Recent" + sHistoryKey;
lsRecent = Homer.InputHistory.load(delegate(string sKey) { return App.readValue(App.sIniFile, sSection, sKey, ""); }, iCount);
}

TextBox txt = null;
ComboBox cmb = null;
Control ctlInput;
if (bHistory) {
cmb = new ComboBox();
cmb.DropDownStyle = ComboBoxStyle.DropDown;
cmb.AccessibleName = lbl.AccessibleName;
cmb.AccessibleDescription = "Down arrow selects from up to " + iCount + " recent entries";
foreach (string sOne in lsRecent) cmb.Items.Add(sOne);
cmb.Text = sValue;
cmb.GotFocus += delegate(object o, EventArgs e) {cmb.SelectAll();};
ctlInput = cmb;
}
else {
txt = new TextBox();
txt.AccessibleName = lbl.AccessibleName;
if (bPassword) txt.UseSystemPasswordChar = true;
txt.Text = sValue;
txt.GotFocus += delegate(object o, EventArgs e) {txt.SelectAll();};
ctlInput = txt;
}
flpInput.Controls.AddRange(new Control[] {lbl, ctlInput});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Click += delegate(object o, EventArgs e) { sResult = (cmb != null) ? cmb.Text : txt.Text; frm.Close();};
btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Say("Cancel", true); frm.Close();};
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
frm.ShowDialog(ownerForm());
frm.Dispose();
if (bHistory && sResult != null && sResult.Trim().Length > 0) {
lsRecent = Homer.InputHistory.push(lsRecent, sResult.Trim(), iCount);
Homer.InputHistory.store(lsRecent, delegate(string sKey, string sVal) { App.writeValue(App.sIniFile, sSection, sKey, sVal); }, iCount);
}
return sResult;
} // InputDialog method (history overload)

public static ArrayList FieldDialog(string sTitle, string[] sLabelList, string[] sValueList) {
return FieldDialog(sTitle, sLabelList, sValueList, false);
} // FieldDialog method

public static ArrayList FieldDialog(string sTitle, string[] sLabelList, string[] sValueList, bool bPassword) {
ArrayList sResultList = new ArrayList();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

TableLayoutPanel tlpFields = new TableLayoutPanel();
tlpFields.SuspendLayout();
tlpFields.AutoSize = true;
tlpFields.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
btnCancel.Click += delegate(object o, EventArgs e) { Say("Cancel", true); frm.Close();};
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
frm.ShowDialog(ownerForm());
frm.Dispose();
return sResultList;
} // FieldDialog method

public static List<string> ListInputDialog(string sTitle, string sListLabel, string[] sValueList, string sInputLabel, string sValue, bool bSorted, int iDefaultIndex) {
List<string> listResults = new List<string>();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
btnOK.Click += delegate(object o, EventArgs e) {
listResults.Add(lst.Text);
listResults.Add(txt.Text);
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Say("Cancel", true); frm.Close();};
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
frm.ShowDialog(ownerForm());
frm.Dispose();
return listResults;
} // ListInputDialog method

// ---- info dialog ----

public static void InfoDialog(string sTitle, string sValue, bool bSelectText) {
Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
frm.ShowDialog(ownerForm());
frm.Dispose();
} // InfoDialog method

// ---- answer dialog ----

public static void AnswerDialog(string sTitle, string sLabel, string sText) {
// A long piece of text to read, move around in, and copy from, with one OK
// button and three ways to leave.
//
// WHY NOT InfoDialog
//
// InfoDialog is 300 by 300 with a Close button, which suits a short report. An
// answer from a language model is prose of unknown length, and the reader wants
// to arrow through it line by line, select part of it, and copy that part out.
// So this is larger, it is labelled, and it can be dismissed without leaving
// the text.
//
// READ ONLY BUT NOT DISABLED. A read-only text box still takes focus, still
// moves by character, word and line, and still copies with Control+C, which is
// exactly what is wanted. A disabled one does none of those and a screen reader
// skips it.
//
// THREE WAYS OUT. Escape and Enter are the usual pair, through CancelButton and
// AcceptButton. The Spacebar is added because in a read-only box it does
// nothing else, and a reader whose hand is on the text should not have to find
// another key. Enter is handled on the box as well, since a multiline text box
// consumes it before the form's AcceptButton ever sees it.
Form frm = new Form();
frm.SuspendLayout();
frm.FormBorderStyle = FormBorderStyle.Sizable;
frm.MinimizeBox = false;
frm.MaximizeBox = true;
frm.ShowInTaskbar = false;
frm.StartPosition = FormStartPosition.CenterParent;
frm.Text = sTitle;
frm.ClientSize = new Size(640, 460);

Label lbl = new Label();
lbl.AutoSize = true;
lbl.Location = new Point(12, 9);
// The label names the box for a screen reader, and its trigger letter reaches
// it from the button, per the Homer form guidelines.
lbl.Text = sLabel;

TextBox txt = new TextBox();
txt.Multiline = true;
txt.ReadOnly = true;
txt.WordWrap = true;
txt.ScrollBars = ScrollBars.Vertical;
txt.AcceptsReturn = false;
txt.AcceptsTab = false;
txt.Location = new Point(12, lbl.Bottom + 4);
txt.Size = new Size(616, 380);
txt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
// Windows line endings, or a text box shows one long run with no line breaks.
txt.Text = sText.Replace("\r\n", "\n").Replace("\n", "\r\n");
// The caret starts at the top with nothing selected, so the first arrow key
// reads the first line rather than moving off a selection nobody made.
txt.SelectionStart = 0;
txt.SelectionLength = 0;

Button btnOk = new Button();
// No ampersand on OK, per the Homer form guidelines: its keys are Enter and
// Escape, and leaving O free lets another control claim it.
btnOk.Text = "OK";
btnOk.Location = new Point(12, txt.Bottom + 8);
btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
btnOk.Click += delegate(object o, EventArgs e) { frm.Close(); };

txt.KeyDown += delegate(object o, KeyEventArgs e) {
if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape) {
// Handled, so the box neither beeps nor tries to insert anything.
e.Handled = true;
e.SuppressKeyPress = true;
frm.Close();
}
};

frm.AcceptButton = btnOk;
frm.CancelButton = btnOk;
frm.Controls.AddRange(new Control[] {lbl, txt, btnOk});
frm.ResumeLayout();
// Focus lands in the text, not on the button: reading is why the dialog opened.
frm.Shown += delegate(object sender, EventArgs e) {
txt.Focus();
};
frm.ShowDialog(ownerForm());
frm.Dispose();
} // AnswerDialog method

// ---- button dialog ----

public static string ButtonDialog(string sTitle, string sText, string[] sButtonList, int iDefaultButton) {
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

if (sText != "") {
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
btnCancel.Click += delegate(object o, EventArgs e) { Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
btnCancel.AutoSize = false;
btnCancel.Width = 200;
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
frm.ShowDialog(ownerForm());
frm.Dispose();
Say(sResult.Replace("&", ""));
return sResult;
} // ButtonDialog method

public static object[] ListButtonDialog(string sTitle, object[] aValue, string[] aDisplay, string[] aButton, bool bSort, int iIndex) {
object[] aResult = {};

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpData = new FlowLayoutPanel();
flpData.SuspendLayout();
flpData.Anchor = AnchorStyles.None;
flpData.AutoSize = true;
flpData.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpData.FlowDirection = FlowDirection.LeftToRight;

ListBox lst = new ListBox();
lst.Sorted = false;
if (aDisplay == null) lst.Items.AddRange(aValue);
else lst.Items.AddRange(aDisplay);
if (bSort) lst.Sorted = true;
lst.SelectedIndex = iIndex;

flpData.Controls.Add(lst);
flpData.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

for (int i = 0; i < aButton.Length; i++) {
Button btn = new Button();
btn.Click += delegate(object o, EventArgs e) {
object oItem;
if (aDisplay == null) oItem = lst.Text;
else {
int iValue = Array.IndexOf(aDisplay, lst.Text);
oItem = aValue[iValue];
}
aResult = new object[] {oItem, btn.Text};
Say(btn.Text.Replace("&", ""), true);
frm.Close();
};

btn.Text = aButton[i];
btn.AccessibleName = aButton[i].Replace("&", "");
flpButtons.Controls.Add(btn);
}

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Say("Cancel", true); frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
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
frm.ShowDialog(ownerForm());
frm.Dispose();
return aResult;
} // ListButtonDialog method

// ---- multi-select list dialogs ----

public static ArrayList MultiListDialog(string sTitle, string sLabel, string[] sValueList, bool bSorted, int iDefaultIndex, int[] iSelectList) {
ArrayList sResultList = new ArrayList();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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

for (int i = 0; i < iSelectList.Length; i++) {
lst.SetSelected(iSelectList[i], true);
}

bool bState = lst.GetSelected(iDefaultIndex);
lst.SelectedIndex = iDefaultIndex;
lst.SetSelected(iDefaultIndex, bState);

flpInput.Controls.Add(lst);
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
btnCancel.Click += delegate(object o, EventArgs e) { Say("Cancel", true); frm.Close();};
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
frm.ShowDialog(ownerForm());
frm.Dispose();
return sResultList;
} // MultiListDialog method

public static List<int> MultiListDialog(string sTitle, string[] aValues, bool bSorted) {
List<int> listResults = new List<int>();

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
foreach (int index in lst.SelectedIndices) listResults.Add(index);
frm.Close();};

btnOK.Text = "OK";
btnOK.AccessibleName = btnOK.Text;

Button btnCancel = new Button();
btnCancel.Click += delegate(object o, EventArgs e) { Say("Cancel", true); frm.Close();};
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
frm.ShowDialog(ownerForm());
frm.Dispose();
return listResults;
} // MultiListDialog method

// ---- list / check dialogs (delegate to Dialog helpers) ----

public static string ListDialog(string sTitle, string sLabel, string[] sValueList, bool bSorted, int iDefaultIndex) {
return Dialog.Pick(sTitle, sValueList, bSorted, iDefaultIndex);
} // ListDialog method

public static string[] MultiCheckDialog(string sTitle, string[] aValues, int[] aSelect, bool bSort, int iIndex) {
return Dialog.MultiCheck(sTitle, aValues, aSelect, bSort, iIndex);
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
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
btnBrowse.Click += delegate(object o, EventArgs e) { txt.Text = FolderBrowseDialog("", sValue, false); txt.Select();};
btnBrowse.Text = "&Browse";
btnBrowse.AccessibleName = btnBrowse.Text.Replace("&", "");

flpInput.Controls.AddRange(new Control[] {lbl, txt, btnBrowse});
flpInput.ResumeLayout();

FlowLayoutPanel flpLists = new FlowLayoutPanel();
flpLists.SuspendLayout();
flpLists.Anchor = AnchorStyles.None;
flpLists.AutoSize = true;
flpLists.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpLists.FlowDirection = FlowDirection.LeftToRight;

Button btnCurrent = new Button();

btnCurrent.Click += delegate(object o, EventArgs e) {
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
string[] aDirs = App.lsRecentDirs.ToArray();
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
string sQuickDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FileDir\Quick";
sQuickDir = Path.GetFullPath(sQuickDir);
string[] aLinks = Directory.GetFiles(sQuickDir, "*.lnk");
string sDirs = "";
foreach (string sLink in aLinks) {
object oLink = CreateObject("WScript.Shell");
oLink = CallMethod(oLink, "CreateShortcut", new object[] {sLink});
string sDir = (string) GetProperty(oLink, "TargetPath");
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
string sDir = PickSpecialFolder();
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
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();
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
string s = "Pick Drive to Map";
sDrive = ListDialog(s, "", aUnmapped, true, 0);
if (sDrive.Length == 0) return;

string sExe = "net.exe";
string sParams = "use " + sDrive + ": " + sPath;
try {
string sTempFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "FileDir.tmp");
if (File.Exists(sTempFile)) File.Delete(sTempFile);
string sCommand = sExe + " " + sParams + " 2>" + sTempFile;
MapDrive2Share(sDrive, sPath);
if (File.Exists(sTempFile)) {
string sOutput = File2String(sTempFile).Trim();
if (sOutput.Length > 0) Show(sOutput, "Result");
}
}
catch (Exception ex) {
Show(ex.Message, "Error");
}
}

else {
string sChoice = ConfirmDialog("Confirm", "Cannot find folder " + sResult + "\nCreate it?", "Y");
if (sChoice == "Y") {
try {
DirectoryInfo di = new DirectoryInfo(sResult);
di.Create();
}
catch (Exception ex) {
Show(ex.Message, "Error");
}
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
btnCancel.Click += delegate(object o, EventArgs e) { Say("Cancel", true); sResult = ""; frm.Close();};
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
frm.ShowDialog(ownerForm());
frm.Dispose();
return sResult;
} // DirectoryDialog method

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

public static string Input(string sTitle, string sLabel, string sValue) {
string[] aLabel = new string[] {sLabel};
string[] aValue = new string[] {sValue};
string[] aReturn = MultiInput(sTitle, aLabel, aValue);
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
flpMain.FlowDirection = FlowDirection.TopDown;

TableLayoutPanel tlpFields = new TableLayoutPanel();
tlpFields.SuspendLayout();
tlpFields.Anchor = AnchorStyles.None;
tlpFields.AutoSize = true;
tlpFields.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
btnCancel.Click += delegate(object o, EventArgs e) { frm.Close();};

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
frm.ShowDialog(Lbc.ownerForm());
frm.Dispose();

string s = sb.ToString();
if (s.Length > 0) s = s.Substring(0, s.Length - 1);
string[] aReturn = {};
if (s.Length > 0) aReturn = s.Split('\n');
return aReturn;
} // MultiInput method

public static string Pick(string sTitle, string[] aValue, bool bSort) {
return Pick(sTitle, aValue, null, bSort, 0);
} // Pick method

public static string Pick(string sTitle, string[] aValue, bool bSort, int iIndex) {
return Pick(sTitle, aValue, null, bSort, iIndex);
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
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
btnCancel.Click += delegate(object o, EventArgs e) { frm.Close();};

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
iIndex = -1;
for (int i = 0; i < bs.Count; i++) {
DataRowView row = (DataRowView) bs[i];
if (row[1].ToString() == sItem) {
iIndex = i;
break;
}
}
}
}

if (iIndex == -1) iIndex = 0;
if (iIndex > 0) Lbc.Say("Item " + (iIndex + 1).ToString());
bs.Position = iIndex;
};

frm.ShowDialog(Lbc.ownerForm());
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

public static string[] MultiCheck(string sTitle, string[] aValues, int[] aSelect, bool bSort, int iIndex) {
return MultiCheck(sTitle, null, aValues, aSelect, bSort, iIndex);
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
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

FlowLayoutPanel flpInput = new FlowLayoutPanel();
flpInput.SuspendLayout();
flpInput.Anchor = AnchorStyles.None;
flpInput.AutoSize = true;
flpInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpInput.FlowDirection = FlowDirection.LeftToRight;

CheckedListBox lst = new CheckedListBox();
frm.lst = lst;
lst.Sorted = false;
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

flpInput.Controls.AddRange(new Control[] {lst});
flpInput.ResumeLayout();

FlowLayoutPanel flpButtons = new FlowLayoutPanel();
flpButtons.SuspendLayout();
flpButtons.Anchor = AnchorStyles.None;
flpButtons.AutoSize = true;
flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpButtons.FlowDirection = FlowDirection.LeftToRight;

Button btnOK = new Button();

btnOK.Click += delegate(object o, EventArgs e) {
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
frm.Load += delegate(object sender, EventArgs e) {
if (iIndex == 0) {
for (int i = 0; i < aSelect.Length; i++) lst.SetItemChecked(aSelect[i], true);

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
iIndex = -1;
for (int i = 0; i < bs.Count; i++) {
DataRowView row = (DataRowView) bs[i];
if (row[1].ToString() == sItem) {
iIndex = i;
break;
}
}
}

if (iIndex == -1) iIndex = 0;
if (iIndex > 0) Lbc.Say("Item " + (iIndex + 1).ToString());
bs.Position = iIndex;
}
};

frm.ShowDialog(Lbc.ownerForm());
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
string sItem = ((DataRowView) bs.Current)[1].ToString();
hashItem.Add(sTitle, sItem);
}

return aResults;
} // MultiCheck method

public static string Choose(string sTitle, string sText, string[] aButtons, int iDefault) {
string sResult = "";

Form frm = new Form();
frm.SuspendLayout();
frm.AutoSize = true;
frm.AutoSizeMode = AutoSizeMode.GrowAndShrink;

FlowLayoutPanel flpMain = new FlowLayoutPanel();
flpMain.SuspendLayout();
flpMain.AutoSize = true;
flpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
flpMain.FlowDirection = FlowDirection.TopDown;

if (sText != "") {
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
btnCancel.Click += delegate(object o, EventArgs e) { frm.Close();};
btnCancel.Text = "Cancel";
btnCancel.AccessibleName = btnCancel.Text;
btnCancel.AutoSize = false;
btnCancel.Width = 200;
flpMain.Controls.Add(btnCancel);

flpMain.ResumeLayout();

frm.CancelButton = btnCancel;
frm.StartPosition = FormStartPosition.CenterParent;
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
frm.ShowDialog(Lbc.ownerForm());
frm.Dispose();
Lbc.Say(sResult.Replace("&", ""));
return sResult;
} // Choose method

} // Dialog class

// ===========================================================================
// ListForm - ListBox/CheckedListBox host with filter, sort, jump key handling
// ===========================================================================

public class ListForm : Form {

public ListBox lst;
public DataTable tbl;
public BindingSource bs;
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
for (int i = 0; i < tbl.Rows.Count; i++) this.tblDefault.Rows.Add(tbl.Rows[i][0].ToString(), tbl.Rows[i][1].ToString());
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
for (int i = 0; i < tbl.Rows.Count; i++) this.tblDefault.Rows.Add(tbl.Rows[i][0].ToString(), tbl.Rows[i][1].ToString());
}

DataTable tblNew = new DataTable();
tblNew.Columns.Add("Item", typeof(string));
tblNew.Columns.Add("Value", typeof(string));
for (int i = this.tblDefault.Rows.Count - 1; i >= 0; i--) tblNew.Rows.Add(this.tblDefault.Rows[i][0].ToString(), tblDefault.Rows[i][1].ToString());
tbl = tblNew;
bs.DataSource = tbl;
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
for (int iPosition = iFirst; iPosition <= iLast; iPosition++) ((CheckedListBox) lst).SetItemChecked(iPosition, bState);
if (iAfter != bs.Position && iAfter >= 0 && iAfter < tbl.DefaultView.Count) bs.Position = iAfter;
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

if (Dialog.hashFilter.ContainsKey(this.Text)) Dialog.hashFilter.Remove(this.Text);
if (sFilter.Trim().Length > 0) Dialog.hashFilter.Add(this.Text, sFilter);
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
while (iIndex < iCount && tbl.DefaultView[iIndex][0].ToString().ToLower().IndexOf(sJump) == -1) {
iIndex++;
}
if (iIndex < iCount) bs.Position = iIndex;
else Lbc.Say("Not found!");
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
for (int i = 0; i < aFilters.Length; i++) {
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
if (i == aFilters.Length - 1) s += ")";
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
