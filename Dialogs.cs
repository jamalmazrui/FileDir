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
// class Lbc moved to the kit as Homer.LbcAsk (10 November 2026).

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
Homer.LbcMessage.show(oText.ToString(), oTitle.ToString());
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
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, Homer.LbcAsk.ownerForm())) {
ListBox lb = dlg.addListBox(lsNames, sSelected, null);
dlg.primaryList = lb;
if (!dlg.runOkCancel()) {
Homer.LbcAsk.say("Cancel", true);
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
using (Homer.LbcDialog dlg = new Homer.LbcDialog(sTitle, Homer.LbcAsk.ownerForm())) {
if (!string.IsNullOrEmpty(sText)) dlg.addLabel(sText);
sPressed = dlg.runWithButtons(lsButtons.ToArray(), false, aButtons[iAt].Replace("&", ""));
}
if (string.IsNullOrEmpty(sPressed)) sPressed = "Cancel";
if (sPressed == "Cancel" && !bOwnCancel) return "";
foreach (string sOne in aButtons) {
if (sOne.Replace("&", "") == sPressed) {
Homer.LbcAsk.say(sPressed);
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

// mapDrive2Share: maps sShare to the drive letter sDrive, asking for credentials when needed (10 November 2026). It
// was in FileDir's dialog layer, which moved to the kit as Homer.LbcAsk; it stays here with this class, and FileDir
// gives it to LbcAsk.fMapDrive, so the kit's folder dialog can still map a typed \\server\share path.
public static bool mapDrive2Share(string sDrive, string sShare) {
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
Homer.LbcAsk.show(ex.Message, "Error");
bResult = false;
}
oNetDrive = null;
return bResult;
} // mapDrive2Share method

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
