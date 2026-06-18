/*
Layout by Code for .NET development (JScript library)
Version 2.9
November 1, 2007
Copyright 2006-2007 by Jamal Mazrui
Modified GPL License
*/

import System;
import System.IO;
import System.Text;
import System.Windows.Forms;

public class LbcJS {
static function Eval(sCode : String , oParams : Object[]) {
return eval(sCode, "unsafe");
}
}
