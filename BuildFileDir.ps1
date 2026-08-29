# BuildFileDir.ps1 -- build FileDir.exe and FileDir_setup.exe.
#
# There are two scripts in this project and this is one of them.  Everything the
# build needs is here: the source audit, the key map generator, the compile, the
# documents, and the installer.  The other script is cleanFileDir.
#
#     BuildFileDir              full build, taking the next version number
#     BuildFileDir nobump       recompile without taking a new number
#     BuildFileDir noinstall    build the program but not the installer
#     BuildFileDir audit        run the checks only, and compile nothing
#
# The checks live in auditFileDir.py, which shares homerPolicy.py with
# cleanFileDir. There are still only two commands to run: this one and
# cleanFileDir.
#
# The shape follows the EdSharp build:
#
#   1. Run makeKeyMap.py to generate KeyMap.cs and Hotkeys.md from Hotkeys.ini,
#      the single source for every command name, key, and description.
#   2. Run auditFileDir.py and stop on failure.  Nothing is compiled until the
#      checks a compiler cannot make have passed.
#   3. Take the next version number from version.txt, the single source of truth
#      for the version, and generate Version.cs from it.
#   4. Compile FileDir.js to FileDirScript.dll, then FileDir.cs to FileDir.exe.
#   5. Regenerate the .htm for every Markdown document.
#   6. Compile the installer to FileDir_setup.exe.
#
# Everything is logged to BuildFileDir.log beside this script, written line by
# line as it happens, so a build that dies still leaves a log.  When a build
# fails, that is the file to send.

[CmdletBinding()]
param(
    [string] $Action = ""
)

$ErrorActionPreference = "Stop"

$sScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sLogPath = Join-Path $sScriptDir "BuildFileDir.log"

$iWarnings = 0

function writeLog {
    param([string] $sText)
    $sStamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Add-Content -Path $sLogPath -Value ("[" + $sStamp + "] " + $sText) -Encoding UTF8
}

function saySection {
    param([string] $sText)
    writeLog ""
    writeLog ("---- " + $sText + " ----")
}

function sayWarning {
    param([string] $sText)
    $script:iWarnings = $script:iWarnings + 1
    Write-Host ("WARNING: " + $sText)
    writeLog ("WARNING: " + $sText)
}

function stopHere {
    param([string] $sText)
    writeLog ("BUILD FAILED: " + $sText)
    writeLog ("Finished " + (Get-Date).ToString("yyyy-MM-dd HH:mm:ss"))
    Write-Host ""
    Write-Host ("BUILD FAILED: " + $sText)
    Write-Host ("The whole story is in " + $sLogPath + " -- send that file.")
    exit 1
}

# ---- the log is already open ------------------------------------------------
# BuildFileDir.cmd creates it and writes the first lines BEFORE PowerShell is
# started, so that a log exists even when this script will not parse -- and a
# script that will not parse never runs a line of itself, including its logging.
# So this appends; it must not truncate what the wrapper wrote.
Add-Content -Path $sLogPath -Value "PowerShell started and parsed." -Encoding UTF8

# ---- catch anything unexpected, so a failure never reaches only the console --
# Without this, a terminating error prints a wall of red text and the log ends
# mid-sentence, which is the one thing a log must never do.
trap {
    writeLog "UNEXPECTED FAILURE -- the build stopped here."
    writeLog ("Message: " + $_.Exception.Message)
    writeLog ("Type: " + $_.Exception.GetType().FullName)
    if ($_.InvocationInfo) {
        writeLog ("Line " + $_.InvocationInfo.ScriptLineNumber + ": " + $_.InvocationInfo.Line.Trim())
    }
    writeLog ("Stack trace: " + $_.ScriptStackTrace)
    if ($_.Exception.InnerException) {
        writeLog ("Inner exception: " + $_.Exception.InnerException.Message)
    }
    Write-Host ""
    Write-Host ("BuildFileDir stopped: " + $_.Exception.Message)
    Write-Host ("The whole story is in " + $sLogPath + " -- send that file.")
    exit 1
}

writeLog ("Started " + (Get-Date).ToString("yyyy-MM-dd HH:mm:ss"))
writeLog ("Script: " + $MyInvocation.MyCommand.Path)
writeLog ("PowerShell: " + $PSVersionTable.PSVersion.ToString())
writeLog ("PowerShell edition: " + $PSVersionTable.PSEdition)
writeLog ("Platform: " + [System.Environment]::OSVersion.VersionString)
writeLog ("64-bit process: " + [System.Environment]::Is64BitProcess)
writeLog ("Processor architecture: " + $env:PROCESSOR_ARCHITECTURE)
writeLog ("Computer: " + $env:COMPUTERNAME + ", user: " + [System.Environment]::UserName)
writeLog ("Working directory: " + (Get-Location).Path)
writeLog ("Command line: " + $MyInvocation.Line)

# Record the date and size of every script in the build. A failure that was
# already fixed is almost always a stale copy of one of these -- a zip that was
# not unarchived, or unarchived after the build ran -- and without this the log
# gives no way to tell that apart from a fix that did not work.
writeLog "Scripts in use:"
foreach ($sName in @("BuildFileDir.ps1", "BuildFileDir.cmd", "auditFileDir.py",
                     "makeKeyMap.py", "cleanFileDir.py", "cleanFileDir.cmd",
                     "homerPolicy.py", "FileDir_setup.iss", "RepoFiles.txt")) {
    $sPath = Join-Path $sScriptDir $sName
    if (Test-Path $sPath) {
        $item = Get-Item $sPath
        writeLog ("  " + $sName.PadRight(20) + " " + $item.LastWriteTime.ToString("yyyy-MM-dd HH:mm") + "  " + $item.Length + " bytes")
    }
    else {
        writeLog ("  " + $sName.PadRight(20) + " NOT PRESENT")
    }
}

$bNoBump = ($Action -match "(?i)nobump")
$bNoInstall = ($Action -match "(?i)noinstall")
$bAuditOnly = ($Action -match "(?i)^audit$")
writeLog ("Setting Action: '" + $Action + "'")
writeLog ("Setting nobump: " + $bNoBump)
writeLog ("Setting noinstall: " + $bNoInstall)
writeLog ("Setting auditOnly: " + $bAuditOnly)
writeLog ("Project directory: " + $sScriptDir)

Set-Location $sScriptDir

# =============================================================================
# runProgram -- run an external program, log the command and its exit code, and
# append its output to the build log.  Program and arguments are passed
# separately, never as one string: the command interpreter's quote stripping
# after /c defeats a quoted program with a quoted argument, and PowerShell joins
# an argument list with spaces and quotes nothing.  Passing them apart means no
# quoting rule applies at all.
# =============================================================================
function runProgram {
    param(
        [string] $sProgram,
        [string[]] $asArguments,
        [string] $sLabel
    )
    writeLog ("Running " + $sLabel + ": " + $sProgram)
    # QUOTE ANYTHING WITH A SPACE IN IT. PowerShell joins an argument list with
    # spaces and quotes NOTHING, so an argument such as
    #
    #     --metadata title=FileDir - ReadMe
    #
    # arrives at the program as four arguments, and the bare "-" among them told
    # Pandoc to read from standard input. It then waited for input that was never
    # coming, and the build hung with nothing on the console -- which is exactly
    # the trap named in the EdSharp handover, met here for real.
    $asQuoted = @()
    foreach ($sArgument in $asArguments) {
        if ($sArgument -match '\s' -and -not $sArgument.StartsWith('"')) {
            $asQuoted += ('"' + $sArgument + '"')
        }
        else {
            $asQuoted += $sArgument
        }
    }
    writeLog ("  Arguments: " + ($asQuoted -join " "))
    $sOutFile = Join-Path $env:TEMP ("buildFileDir_" + [System.Guid]::NewGuid().ToString("N") + ".txt")
    # AND GIVE IT AN EMPTY STANDARD INPUT. Quoting fixes the case above, but any
    # tool that decides to read standard input would hang the build the same way.
    # An empty file means such a tool reads end-of-file at once and gets on with
    # it. A build must never wait for a person who is not there.
    $sInFile = $sOutFile + ".in"
    Set-Content -Path $sInFile -Value "" -NoNewline -Encoding UTF8
    $process = Start-Process -FilePath $sProgram -ArgumentList $asQuoted -NoNewWindow -Wait -PassThru -RedirectStandardInput $sInFile -RedirectStandardOutput $sOutFile -RedirectStandardError ($sOutFile + ".err")
    $iExit = $process.ExitCode
    Remove-Item $sInFile -Force -ErrorAction SilentlyContinue
    foreach ($sFile in @($sOutFile, ($sOutFile + ".err"))) {
        if (-not (Test-Path $sFile)) { continue }
        foreach ($sLine in (Get-Content $sFile -ErrorAction SilentlyContinue)) {
            if ($sLine.Trim() -ne "") { writeLog ("  | " + $sLine) }
        }
        Remove-Item $sFile -Force -ErrorAction SilentlyContinue
    }
    writeLog ("  " + $sLabel + " exit code: " + $iExit)
    return $iExit
}

# =============================================================================
# findTool -- return the first path in a list that exists, or an empty string.
# =============================================================================
function findTool {
    param([string[]] $asPaths)
    foreach ($sPath in $asPaths) {
        if ($sPath -eq "") { continue }
        if (Test-Path $sPath) { return $sPath }
    }
    return ""
}

# =============================================================================
# Python
# =============================================================================
# Two build steps are Python: the key map generator and the audit.  Python is
# found once, here, and used for both.
#
# Windows answers "where python" with an app execution alias under WindowsApps
# that is not Python at all: it advertises the Microsoft Store.  Any candidate
# must be checked and any path through WindowsApps rejected.
saySection "Python"
$sPython = findTool @(
    "$env:LOCALAPPDATA\Programs\Python\Python314\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python313\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python311\python.exe",
    "$env:ProgramFiles\Python314\python.exe",
    "$env:ProgramFiles\Python313\python.exe",
    "$env:ProgramFiles\Python312\python.exe",
    "$env:ProgramFiles\Python311\python.exe"
)
if ($sPython -eq "") {
    foreach ($found in (Get-Command "python.exe" -All -ErrorAction SilentlyContinue)) {
        if ($found.Source -match "WindowsApps") { continue }
        $sPython = $found.Source
        break
    }
}
if ($sPython -eq "") { stopHere "No Python found. The key map generator and the audit both need it. Install Python from python.org, not from the Microsoft Store." }
writeLog ("Python: " + $sPython)

# =============================================================================
# Step 1: key map
# =============================================================================
# KeyMap.cs holds the shipped default key and description for every command,
# compiled into the program, and Hotkeys.md is the reference document.  Both are
# generated from Hotkeys.ini, so the table, the program and the document cannot
# disagree.  This runs before the audit because the audit checks what it makes.
saySection "Key map"
if (-not (Test-Path "makeKeyMap.py")) { stopHere "makeKeyMap.py not found. KeyMap.cs and Hotkeys.md are generated from Hotkeys.ini and cannot be written without it." }
Write-Host "Generating KeyMap.cs and Hotkeys.md from Hotkeys.ini ..."
$iExit = runProgram $sPython @("makeKeyMap.py") "makeKeyMap.py"
if ($iExit -ne 0) { stopHere ("makeKeyMap.py returned " + $iExit + ". See makeKeyMap.log.") }

# =============================================================================
# Step 2: audit
# =============================================================================
# auditFileDir.py checks what a compiler cannot.  It is a separate Python
# script, and shares homerPolicy.py with cleanFileDir, so the sweep and the
# check cannot form different opinions about what belongs.  Its shape follows
# EdSharp's audit, so a check written for one project moves to the other.
saySection "Audit"
if (-not (Test-Path "auditFileDir.py")) { stopHere "auditFileDir.py not found. The build will not compile without the checks." }
Write-Host "Auditing sources ..."
$iExit = runProgram $sPython @("auditFileDir.py") "auditFileDir.py"
if ($iExit -ne 0) {
    Write-Host ""
    Write-Host "AUDIT FAILED. Nothing was compiled."
    Write-Host ("The reasons are in auditFileDir.log, and above in " + $sLogPath + ".")
    writeLog "AUDIT FAILED - nothing compiled."
    writeLog ("Finished " + (Get-Date).ToString("yyyy-MM-dd HH:mm:ss"))
    exit 1
}
Write-Host "Audit passed."
writeLog "Audit passed."

if ($bAuditOnly) {
    Write-Host "You asked for the audit only, so nothing was compiled."
    writeLog "Audit only; nothing compiled."
    writeLog ("Finished " + (Get-Date).ToString("yyyy-MM-dd HH:mm:ss"))
    exit 0
}

# =============================================================================
# Step 3: version
# =============================================================================
# version.txt is one line and nothing else, and is the ONLY place a version
# number lives.  This script increments it, generates Version.cs from it, and
# FileDir_setup.iss reads the same file, so the program, the installer, and the
# release tag always agree -- which is what Elevate Version, F11, compares.
#
# No network call is made here.  An earlier build asked GitHub whether a number
# was taken, but the GitHub command line tool has no timeout, so a slow network
# hung the build with no message.  tagRelease does that check, where a stall is
# visible.
saySection "Version"
$sVersionPath = Join-Path $sScriptDir "version.txt"
if (-not (Test-Path $sVersionPath)) { stopHere "version.txt not found. It must hold the current version, for example 5.0.14" }
# Read tolerantly and write back clean.  Windows PowerShell's Set-Content
# -Encoding UTF8 writes a byte order mark, and an earlier build used it here.
# PowerShell strips that mark again when it reads the file, so nothing looked
# wrong -- but Inno Setup does not, and read the mark as part of the number.
# VersionInfoVersion must be numeric, so the installer refused to compile with
# "Value of [Setup] section directive VersionInfoVersion is invalid", pointing
# at a line that was perfectly correct.  The mark is stripped here and never
# written again, so a file that already has one is repaired by the next build.
$sVersion = [System.IO.File]::ReadAllText($sVersionPath)
$sVersion = $sVersion.TrimStart([char]0xFEFF).Trim()
if ($sVersion -eq "") { stopHere "version.txt is empty." }
if ($sVersion.Contains("`n")) { $sVersion = $sVersion.Split("`n")[0].Trim() }
writeLog ("Current version: " + $sVersion)

# version.txt is written as plain UTF-8 with no byte order mark and a single
# newline.  Inno Setup, git and the shell all read it, and only PowerShell is
# forgiving about a mark.
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

if ($bNoBump) {
    Write-Host ("Version: " + $sVersion + " (nobump: keeping the current number)")
    writeLog ("Version kept at " + $sVersion + " (nobump).")
    # Rewrite it even when the number is unchanged, so a file carrying a byte
    # order mark from an older build is repaired rather than carried forward.
    [System.IO.File]::WriteAllText($sVersionPath, $sVersion + "`r`n", $utf8NoBom)
}
else {
    $asParts = $sVersion.Split(".")
    if ($asParts.Length -lt 2) { stopHere ("Could not work out the next version from '" + $sVersion + "'.") }
    $iLast = 0
    if (-not [int]::TryParse($asParts[$asParts.Length - 1], [ref] $iLast)) {
        stopHere ("The last part of version '" + $sVersion + "' is not a number.")
    }
    $asParts[$asParts.Length - 1] = ($iLast + 1).ToString()
    $sNew = $asParts -join "."
    [System.IO.File]::WriteAllText($sVersionPath, $sNew + "`r`n", $utf8NoBom)
    Write-Host ("Version: " + $sVersion + " -> " + $sNew)
    writeLog ("Version: " + $sVersion + " -> " + $sNew)
    $sVersion = $sNew
}

# Prove the file that Inno will read is clean, rather than assuming it. This
# check is here because the failure it catches cost two builds: PowerShell hides
# a byte order mark from itself, so nothing on this side looked wrong, and Inno
# reported the fault against a line that was perfectly correct.
$aVersionBytes = [System.IO.File]::ReadAllBytes($sVersionPath)
if ($aVersionBytes.Length -ge 3 -and $aVersionBytes[0] -eq 0xEF -and $aVersionBytes[1] -eq 0xBB -and $aVersionBytes[2] -eq 0xBF) {
    stopHere "version.txt still begins with a byte order mark after being written. Inno Setup would read it as part of the version number and refuse to compile."
}
writeLog ("version.txt verified: no byte order mark, " + $aVersionBytes.Length + " bytes.")

# Version.cs is generated output: do not edit it, and keep it out of git.
$sVersionCs = @"
// Generated by BuildFileDir.ps1 from version.txt.  Do not edit; do not commit.
public static class BuildVersion
{
    public const string Version = "$sVersion";
}
"@
$utf8Bom2 = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText((Join-Path $sScriptDir "Version.cs"), $sVersionCs.Replace("`r`n", "`n").Replace("`n", "`r`n"), $utf8Bom2)
writeLog ("Generated Version.cs for " + $sVersion + ".")

# =============================================================================
# Step 4: compile
# =============================================================================
saySection "Compile"

# Character-encoding autodetection.  The base class library cannot detect a text
# file's encoding; Ude, a port of the Mozilla universal detector, can, and
# EdSharp uses the same library so both programs detect identically.  Without
# Ude.dll the HAVEUDE symbol is simply not defined and detection falls back to
# the byte order mark, which was the old behaviour.  This can never fail a build.
if (-not (Test-Path "Ude.dll")) {
    $sUdeElsewhere = Join-Path (Split-Path -Parent $sScriptDir) "EdSharp\Ude.dll"
    if (Test-Path $sUdeElsewhere) {
        Copy-Item $sUdeElsewhere "Ude.dll" -Force
        writeLog "Ude.dll copied from the EdSharp project."
    }
}
$asUdeArgs = @()
if (Test-Path "Ude.dll") {
    $asUdeArgs = @("/reference:Ude.dll", "/define:HAVEUDE")
    writeLog "Ude.dll present - character encoding autodetection enabled."
}
else {
    writeLog "Ude.dll absent - encoding detection limited to byte order marks."
    Write-Host "NOTE: Ude.dll not found. Copy it from the EdSharp folder to enable"
    Write-Host "      character encoding autodetection."
}

# Locate csc.exe: prefer Roslyn for the newer C#, fall back to the Framework.
$sCsc = findTool @(
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe",
    "$env:ProgramFiles\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
    "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe",
    "$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "$env:SystemRoot\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)
if ($sCsc -eq "") { stopHere "No csc.exe found. Install Visual Studio Build Tools, or repair the .NET Framework." }
writeLog ("C# compiler: " + $sCsc)

# jsc.exe, for the JScript .NET expression evaluator.  Framework only.
$sJsc = findTool @(
    "$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\jsc.exe",
    "$env:SystemRoot\Microsoft.NET\Framework\v4.0.30319\jsc.exe"
)
if ($sJsc -eq "") { stopHere "No jsc.exe found. Repair the .NET Framework." }
writeLog ("JScript compiler: " + $sJsc)

# UIA notification assemblies for Homer.Say.  Prefer the .NET 4.8 reference
# assemblies; fall back to the GAC, present on any machine with .NET 4.x.
$sRefDir = "${env:ProgramFiles(x86)}\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
$sGacDir = "$env:SystemRoot\Microsoft.NET\assembly\GAC_MSIL"
$sUiaProvider = findTool @(
    (Join-Path $sRefDir "UIAutomationProvider.dll"),
    (Join-Path $sGacDir "UIAutomationProvider\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationProvider.dll")
)
$sUiaTypes = findTool @(
    (Join-Path $sRefDir "UIAutomationTypes.dll"),
    (Join-Path $sGacDir "UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll")
)
if ($sUiaProvider -eq "") { stopHere "UIAutomationProvider.dll not found. Install the .NET 4.8 Developer Pack." }
if ($sUiaTypes -eq "") { stopHere "UIAutomationTypes.dll not found. Install the .NET 4.8 Developer Pack." }
writeLog ("UIAutomationProvider: " + $sUiaProvider)
writeLog ("UIAutomationTypes: " + $sUiaTypes)

function clearForBuild($sFile) {
    # Remove a build output, and when that cannot be done, say WHY.
    #
    # "Access to the path is denied" is what .NET says and it explains nothing.
    # The reason is nearly always that the program is running -- FileDir was
    # started with Alt+Control+F and never closed -- and the second most likely
    # is a read-only attribute left by a copy from elsewhere. Both are worth
    # naming, because the answer differs: close it, or clear the attribute.
    if (-not (Test-Path $sFile)) { return }

    # Anything holding the file open, found by comparing full paths rather than
    # by name, so a program of the same name elsewhere is not blamed.
    $sFull = (Resolve-Path $sFile).Path
    $lProcesses = @()
    try {
        foreach ($oProcess in Get-Process -ErrorAction SilentlyContinue) {
            $sTheirs = ""
            try { $sTheirs = $oProcess.Path } catch { }
            if ($sTheirs -and ($sTheirs -eq $sFull)) {
                $lProcesses += $oProcess
            }
        }
    }
    catch { }
    if ($lProcesses.Count -gt 0) {
        # CLOSE IT RATHER THAN COMPLAIN ABOUT IT.
        #
        # Saying "close it and build again" was already better than the system's
        # "access is denied", but it still costs a whole build to be told
        # something the build could have done itself. This is the developer's
        # own program, built from its own folder, holding nothing a person
        # typed: closing it loses nothing.
        #
        # Politely first. CloseMainWindow asks the window to close the way Alt+F4
        # does, so FileDir saves its settings on the way out. Only a program that
        # ignores that is killed, and after five seconds, because a build must
        # not wait on something that is not going to answer.
        foreach ($oHolder in $lProcesses) {
            $sWho = $oHolder.ProcessName + ", process " + $oHolder.Id
            Write-Host ("Closing " + $sWho + ", which is holding " + $sFile + " open ...")
            writeLog ("Closing " + $sWho + " to replace " + $sFile + ".")
            $bClosed = $false
            try { $bClosed = $oHolder.CloseMainWindow() } catch { }
            if ($bClosed) {
                try { $null = $oHolder.WaitForExit(5000) } catch { }
            }
            if (-not $oHolder.HasExited) {
                writeLog ($sWho + " did not close when asked; ending it.")
                try { $oHolder.Kill(); $null = $oHolder.WaitForExit(5000) } catch { }
            }
            if ($oHolder.HasExited) { writeLog ($sWho + " closed.") }
            else {
                $sStop = "Cannot replace " + $sFile + ": " + $sWho + " will not close. "
                $sStop = $sStop + "Close it by hand and build again."
                stopHere $sStop
            }
        }
        # Windows releases the file a moment after the process ends.
        Start-Sleep -Milliseconds 300
    }

    # Read only is the other ordinary cause, and this one can simply be fixed.
    try {
        $oItem = Get-Item $sFile -Force
        if ($oItem.IsReadOnly) {
            $oItem.IsReadOnly = $false
            writeLog ("Cleared the read-only attribute on " + $sFile + ".")
        }
    }
    catch { }

    try {
        Remove-Item $sFile -Force
    }
    catch {
        writeLog ("Could not remove " + $sFile + ": " + $_.Exception.Message)
        $sStop = "Cannot replace " + $sFile + ": " + $_.Exception.Message + " "
        $sStop = $sStop + "Nothing is holding it open that this build can see, so check for a virus scanner, a backup tool, or an Explorer preview pane showing the folder."
        stopHere $sStop
    }
}

Write-Host "Compiling FileDir.js to FileDirScript.dll ..."
clearForBuild "FileDirScript.dll"
$iExit = runProgram $sJsc @("/nologo", "/target:library", "/out:FileDirScript.dll", "FileDir.js") "jsc.exe"
if ($iExit -ne 0) { stopHere ("jsc.exe returned " + $iExit + ". Its output is above in this log.") }

Write-Host "Compiling FileDir.cs to FileDir.exe ..."
clearForBuild "FileDir.exe"
$asCscArgs = @(
    "/nologo", "/target:winexe", "/platform:anycpu", "/optimize+", "/nowarn:0162",
    "/win32manifest:FileDir.manifest"
)
if (Test-Path "FileDir.ico") { $asCscArgs += "/win32icon:FileDir.ico" }
$asCscArgs += @(
    "/reference:FileAssociation.dll",
    "/reference:Microsoft.CSharp.dll",
    "/reference:Tektosyne.dll",
    "/reference:ICSharpCode.SharpZipLib.dll",
    "/reference:Microsoft.VisualBasic.dll",
    "/reference:System.Security.dll",
    "/reference:System.Web.dll",
    ("/reference:" + $sUiaProvider),
    ("/reference:" + $sUiaTypes)
)
$asCscArgs += $asUdeArgs
$asCscArgs += @(
    "/out:FileDir.exe",
    "Version.cs", "KeyMap.cs", "FileDir.cs", "Lbc.cs", "Say.cs",
    "Inix.cs", "Web.cs", "Util.cs", "Dialogs.cs", "Ollama.cs", "Convert.cs", "Media.cs", "Log.cs"
)
$iExit = runProgram $sCsc $asCscArgs "csc.exe"
if ($iExit -ne 0) { stopHere ("csc.exe returned " + $iExit + ". The compiler errors are above in this log.") }
writeLog "FileDir.exe built."

# =============================================================================
# Step 5: documents
# =============================================================================
# The version line in each document is stamped from version.txt before the HTML
# is generated, so version.txt is the single source for the documents too. Typed
# by hand they went stale within three builds, exactly as the About box had gone
# stale for fourteen releases -- and a guide that names the wrong version tells
# the reader the wrong thing about the software in front of them.
saySection "Document versions"
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
foreach ($sDoc in @("ReadMe.md", "FileDir.md", "Developer.md", "History.md",
                    "Hotkeys.md", "Announce.md", "FAQ.md", "Tutorials.md",
                    "License.md")) {
    if (-not (Test-Path $sDoc)) { continue }
    $sText = [System.IO.File]::ReadAllText((Join-Path $sScriptDir $sDoc))
    $sStamped = [regex]::Replace($sText, "(?m)^\*\*(?:Version |FileDir )?\d+(?:\.\d+)*\*\*",
                                 ("**Version " + $sVersion + "**"), 1)
    if ($sStamped -ne $sText) {
        [System.IO.File]::WriteAllText((Join-Path $sScriptDir $sDoc), $sStamped, $utf8Bom)
        writeLog ("Stamped " + $sDoc + " with version " + $sVersion + ".")
    }
    else {
        writeLog ("NOTE: no version line found in " + $sDoc + "; left unchanged.")
    }
}

# =============================================================================
# Step 5b: HTML
# =============================================================================
# Every Markdown document gets a matching .htm. Commit a new .md before tagging,
# or its .htm will not exist for the release.
#
# TWO CONVERTERS, ON PURPOSE. 2htm is the house tool and produces the house
# style, so it is tried first. Pandoc is the fallback, and it is here because
# 2htm failed on all nine documents in one build with
#
#   Could not load file or assembly System.Memory, Version=4.0.2.0
#
# which is the Span trap: a modern package on .NET Framework 4.8 needs
# System.Memory.dll beside the executable, and it was not in the folder. Every
# document silently kept its previous HTML, and only a warning in the log said
# so. A release should not depend on one tool being in working order when
# another that is already installed can do the same job.
saySection "Documents"
$lsDocuments = @("ReadMe.md", "FileDir.md", "Developer.md", "License.md",
                 "History.md", "Hotkeys.md", "Announce.md", "FAQ.md",
                 "Tutorials.md")
$sPandocExe = findTool @(
    "$env:ProgramFiles\Pandoc\pandoc.exe",
    "${env:ProgramFiles(x86)}\Pandoc\pandoc.exe"
)
if ($sPandocExe -eq "") {
    $found = Get-Command "pandoc.exe" -ErrorAction SilentlyContinue
    if ($found) { $sPandocExe = $found.Source }
}
writeLog ("Pandoc for documents: " + $(if ($sPandocExe -eq "") { "not found" } else { $sPandocExe }))

$iConverted = 0
$iFailedDocs = 0
foreach ($sDoc in $lsDocuments) {
    if (-not (Test-Path $sDoc)) {
        writeLog ("NOTE: " + $sDoc + " not present, no HTML generated for it.")
        continue
    }
    $sHtm = [System.IO.Path]::ChangeExtension($sDoc, ".htm")
    $bMade = $false

    if (Test-Path "2htm.exe") {
        $iExit = runProgram (Join-Path $sScriptDir "2htm.exe") @("-f", $sDoc) ("2htm " + $sDoc)
        if ($iExit -eq 0) { $bMade = $true }
        else { writeLog ("2htm returned " + $iExit + " for " + $sDoc + "; trying Pandoc.") }
    }

    if ((-not $bMade) -and ($sPandocExe -ne "")) {
        # A title and a language on every page: an empty lang attribute is worse
        # than none, because a screen reader may then announce the wrong
        # language rather than falling back to the system one.
        # No bare hyphen in the title. Quoting now protects it, but a lone "-"
        # is the character that means standard input to a great many programs,
        # and it does not need to be here at all.
        $sTitle = "FileDir " + [System.IO.Path]::GetFileNameWithoutExtension($sDoc)
        $iExit = runProgram $sPandocExe @("-f", "markdown", "-t", "html5", "--standalone",
                                          "--metadata", ("title=" + $sTitle),
                                          "--metadata", "lang=en",
                                          "-o", $sHtm, $sDoc) ("pandoc " + $sDoc)
        if ($iExit -eq 0) { $bMade = $true }
    }

    # Whichever made it, the file must exist and hold something. A zero-byte
    # .htm is worse than none: it looks like a document and reads as nothing.
    if ($bMade -and (Test-Path $sHtm)) {
        $iSize = (Get-Item $sHtm).Length
        if ($iSize -eq 0) {
            Remove-Item $sHtm -Force
            $bMade = $false
            writeLog ($sHtm + " came out empty and was deleted.")
        }
    }

    if ($bMade) {
        $iConverted = $iConverted + 1
        writeLog ("Wrote " + $sHtm + ".")
    }
    else {
        $iFailedDocs = $iFailedDocs + 1
        sayWarning ("No HTML could be generated for " + $sDoc + ". Neither 2htm nor Pandoc succeeded; see the lines above.")
    }
}
$sDocNoun = "documents"
if ($iConverted -eq 1) { $sDocNoun = "document" }
Write-Host ("Documents: " + $iConverted + " " + $sDocNoun + " converted to HTML.")
writeLog ("Documents: " + $iConverted + " converted, " + $iFailedDocs + " failed.")
# The real check, and it stops the build. This is AFTER the conversion, which is
# the only place the question can be answered: before it, "no HTML yet" is the
# ordinary state of a tree just unarchived. Here it means both converters
# failed, and a release that ships stale or missing documents is worse than one
# that does not happen.
#
# The installer ships each .htm with skipifsourcedoesntexist, so nothing would
# have complained: the pages would simply have been the previous release's, or
# absent, with a warning buried in this log.
$lsNoHtml = New-Object System.Collections.ArrayList
foreach ($sDoc in $lsDocuments) {
    if (-not (Test-Path $sDoc)) { continue }
    $sHtm = [System.IO.Path]::ChangeExtension($sDoc, ".htm")
    if ((Test-Path $sHtm) -and ((Get-Item $sHtm).Length -gt 0)) { continue }
    $null = $lsNoHtml.Add($sHtm)
}
if ($lsNoHtml.Count -gt 0) {
    # Built up a line at a time into a variable, then passed as one argument.
    # A multi-line expression whose continuation lines BEGIN with a plus sign
    # does not parse: PowerShell wants the operator at the END of the line, or a
    # backtick. That broke a build, and the script never reached its own logging
    # because a parse error happens before any of it runs.
    $sNoHtmlMessage = "No HTML was produced for: " + ($lsNoHtml -join ", ") + ". "
    $sNoHtmlMessage = $sNoHtmlMessage + "Neither 2htm nor Pandoc could convert them, and the installer ships each page only if it exists, so this release would have carried stale or missing documents. "
    $sNoHtmlMessage = $sNoHtmlMessage + "If 2htm reported System.Memory, copy System.Memory.dll next to 2htm.exe in the FileDir folder. "
    $sNoHtmlMessage = $sNoHtmlMessage + "If Pandoc is absent, run installPandoc.cmd as an administrator."
    stopHere $sNoHtmlMessage
}

# =============================================================================
# Step 6: installer
# =============================================================================
# Inno Setup's command line compiler, ISCC.exe, produces FileDir_setup.exe.  The
# installer script holds no version literal: it reads version.txt, so the
# installer is always stamped with the number assigned above.
if ($bNoInstall) {
    saySection "Installer skipped (noinstall)"
}
else {
    saySection "Installer"
    $sIscc = findTool @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 5\ISCC.exe"
    )
    if ($sIscc -eq "") {
        $found = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
        if ($found) { $sIscc = $found.Source }
    }
    if ($sIscc -eq "") {
        writeLog "NOTE: ISCC.exe not found; installer not built."
        Write-Host ""
        Write-Host "NOTE: Inno Setup was not found, so FileDir_setup.exe was not built."
        Write-Host "      Install Inno Setup, or compile FileDir_setup.iss in its IDE."
    }
    else {
        writeLog ("Inno Setup compiler: " + $sIscc)
        Write-Host "Compiling FileDir_setup.iss to FileDir_setup.exe ..."
        if (Test-Path "FileDir_setup.exe") { Remove-Item "FileDir_setup.exe" -Force }
        $iExit = runProgram $sIscc @("/Q", "FileDir_setup.iss") "ISCC.exe"
        if ($iExit -ne 0) { stopHere ("ISCC.exe returned " + $iExit + ". FileDir.exe was built, but FileDir_setup.exe was not. The Inno Setup errors are above in this log.") }
        if (Test-Path "FileDir_setup.exe") { writeLog "FileDir_setup.exe built." }
        else { sayWarning "ISCC reported success but FileDir_setup.exe is not here." }
    }
}

# =============================================================================
# Done
# =============================================================================
Write-Host ""
Write-Host "Build complete:"
Write-Host ("  FileDir.exe        -- the application, version " + $sVersion)
if (Test-Path "FileDir_setup.exe") { Write-Host ("  FileDir_setup.exe  -- the installer, version " + $sVersion) }
if ($iWarnings -gt 0) {
    $sWarnNoun = "warnings"
    if ($iWarnings -eq 1) { $sWarnNoun = "warning" }
    Write-Host ("  " + $iWarnings + " " + $sWarnNoun + " -- see " + $sLogPath + ".")
}
Write-Host ""
Write-Host "Next: git add -A, git commit, git push, then tagRelease."
writeLog ("BUILD COMPLETE: version " + $sVersion + ".")
writeLog ("Finished " + (Get-Date).ToString("yyyy-MM-dd HH:mm:ss"))
exit 0
