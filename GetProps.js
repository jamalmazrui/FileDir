var sPath = oParams[0]
var sDir = Path.GetDirectoryName(sPath)
var sName = Path.GetFileName(sPath)
var oShell = new ActiveXObject("Shell.Application")
var oDir = oShell.Namespace(sDir)
var oDirItems = oDir.Self.Items
var oName = oDir.ParseName(sName)

var sReturn : String = ''
var i
for (i = 0; i < 35; i++) {
var sProperty = oDir.GetDetailsOf(oDirItems, i)
var sValue = oDir.GetDetailsOf(oName, i)
if (sValue.Trim().Length > 0) sReturn += sProperty + ' = ' + sValue + '\n'
}
sReturn
