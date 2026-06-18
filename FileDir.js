/*
FileDir.js -- JScript .NET scripting host for FileDir, compiled by jsc.exe
into FileDirScript.dll. FileDir.exe loads this assembly at run time via
Assembly.LoadFrom(<full path>) and calls FileDirScript.JS.runScript by
reflection, so FileDir.cs takes no compile-time dependency on JScript .NET
(Microsoft.JScript). This is the Evaluate Expression evaluator, following
the EdSharp.js / DbDo.js model by Jamal Mazrui.

A distinct package name (FileDirScript, not FileDir) is used so the JScript
namespace never collides with FileDir.cs's own C# namespace FileDir; the
host resolves the type by string ("FileDirScript.JS") from the specific
DLL, which is unambiguous.

Globals visible to user snippets inside the eval scope:
  frm -- the active directory window (MdiChild), passed in from C#.
  tbl -- shortcut for the directory listing's data table (frm.tbl).

Camel Type: lower-camelCase functions and variables; frm and tbl are the
conventional short forms. The entry point is runScript rather than eval,
because eval is a JScript built-in we cannot shadow in our own body.

The returned string is the script's last expression value via ToString(),
"" if null/undefined, or "ERROR: " + message on any compile or runtime
fault. The script does NOT throw out to the host, so FileDir's UI stays
responsive.
*/

import System;
import System.Collections;
import System.Data;
import System.IO;
import System.Reflection;
import System.Text;
import System.Text.RegularExpressions;
import System.Windows.Forms;

package FileDirScript {

public class JS {

  public static function runScript(sCode : String, frmArg : Object, tblArg : Object) : String {
    var frm = frmArg, result = null, tbl = tblArg;
    try {
      result = eval(sCode, "unsafe");
      if (result == null) return "";
      if (result == undefined) return "";
      return result.ToString();
    }
    catch (ex) {
      return "ERROR: " + ex.message;
    }
  }

} // class JS

} // package FileDirScript
