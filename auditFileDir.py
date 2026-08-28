r"""auditFileDir.py -- checks on the FileDir sources that a compiler cannot make.

    python auditFileDir.py            (run from C:\FileDir)
    python auditFileDir.py -pathRoot C:\FileDir

BuildFileDir runs this first and stops when anything fails, so a fault is
caught before a build rather than after a release. To run the checks alone
and compile nothing, use:

    BuildFileDir audit

WHAT THIS IS FOR

The compiler proves the code is valid C#. It cannot prove that two commands
do not claim the same key, that every command has a description, that the
About box reports the version actually built, that Elevate Version asks
GitHub for the file the installer produces, or that a document the program
opens is one the installer ships. Every one of those has been wrong in
FileDir, and most were found only by someone hitting them.

Each check prints PASS or FAIL with a plain sentence. The exit code is 0
when everything passes and 1 when anything fails, so a build script can stop
on it. A detailed log is written beside this script, whatever happens.

This script shares homerPolicy with cleanFileDir, so the two cannot form
different opinions about what belongs in the folder. Its shape follows
auditEdSharp, so a check written for one project can be moved to the other.
"""

import datetime
import fnmatch
import os
import re
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import homerPolicy

c_sLogName = "auditFileDir.log"

# The open-minus-close brace count of a correct FileDir.cs. It is not zero
# because braces appear inside strings and comments. What matters is that it
# does not move: an edit that changes it has unbalanced a block. This matters
# because whoever edits this file may not be able to compile it.
c_iBraceBaseline = 15

pathRoot = os.path.dirname(os.path.abspath(__file__))
if "-pathRoot" in sys.argv:
    pathRoot = sys.argv[sys.argv.index("-pathRoot") + 1]
pathLog = os.path.join(os.path.dirname(os.path.abspath(__file__)), c_sLogName)
fileLog = None
lFailures = []
lWarnings = []


def say(sMessage=""):
    print(sMessage)
    if fileLog:
        try:
            fileLog.write(sMessage + "\n")
            fileLog.flush()
        except Exception:
            pass


def report(sName, bPassed, sDetail=""):
    """Print one result. The detail explains a failure, so a pass omits it.

    Printing it either way produced lines like "PASS ... so the download
    fails", which reads as a contradiction and teaches a reader to skim.
    """
    if bPassed:
        say("PASS  " + sName)
        return
    say("FAIL  " + sName + ((": " + sDetail) if sDetail else ""))
    lFailures.append(sName)


def warn(sMessage):
    say("WARN  " + sMessage)
    lWarnings.append(sMessage)


def readFile(sName):
    pathFile = os.path.join(pathRoot, sName)
    if not os.path.isfile(pathFile):
        return None
    with open(pathFile, "r", encoding="utf-8-sig", errors="replace") as fileIn:
        return fileIn.read()


def plural(iCount, sNoun):
    return str(iCount) + " " + sNoun + ("" if iCount == 1 else "s")


def commandsInSource(sCode):
    """Every live menu command: its name, its key text, and its line number.

    Commented-out calls are skipped, and the @ allows for a C# verbatim
    string, as in menu_Helper("Open Root Folder", @"\\", ...). Without the @
    the key is missed, the command looks like a top-level menu, and it
    escapes every check below -- which is how two commands went unchecked.
    """
    lCommands = []
    for iLine, sLine in enumerate(sCode.splitlines(), 1):
        if sLine.lstrip().startswith("//"):
            continue
        oMatch = re.search(r'menu_Helper\(\s*@?"([^"]*)"\s*(?:,\s*@?"([^"]*)")?', sLine)
        if not oMatch:
            continue
        sName = oMatch.group(1).replace("&", "").replace(" ...", "").strip()
        sKey = (oMatch.group(2) or "").strip()
        if not sKey:
            continue                      # a top-level menu, not a command
        lCommands.append((sName, sKey, iLine))
    return lCommands


def hotkeyEntries(sHotkeys):
    """The command-to-value table in Hotkeys.ini."""
    dEntries = {}
    for sLine in sHotkeys.splitlines():
        sLine = sLine.strip()
        if not sLine or sLine.startswith("[") or sLine.startswith(";"):
            continue
        iEquals = sLine.find("=")
        if iEquals < 1:
            continue
        dEntries[sLine[:iEquals].strip()] = sLine[iEquals + 1:].strip()
    return dEntries


def checkBracesBalance(sCode, sName, iBaseline):
    """The brace count must not move from its known value."""
    iOpen, iClose = sCode.count("{"), sCode.count("}")
    iDelta = iOpen - iClose
    report(sName + " brace balance", iDelta == iBaseline,
           "" if iDelta == iBaseline else
           "delta is " + str(iDelta) + " but the baseline is " + str(iBaseline)
           + ". An edit has unbalanced a block. If the change was deliberate, "
           + "update c_iBraceBaseline in this script.")


def checkCommandsDescribed(sCode, sHotkeys):
    """Every command needs a description, in the table the program reads.

    A command with no entry says "No description available", which is the one
    thing a blind user cannot work around. Key Describer, the Hotkey Summary
    and the Alternate Menu all read this table.
    """
    dEntries = hotkeyEntries(sHotkeys)
    lMissing = []
    for sName, sKey, iLine in commandsInSource(sCode):
        if sName.startswith("Drive ") and len(sName) == 7:
            continue                      # built at run time
        if sName in dEntries or ("Say " + sName) in dEntries:
            continue
        lMissing.append(sName + " (line " + str(iLine) + ")")
    report("Every command has a description", not lMissing, ", ".join(lMissing))


def checkCommandsReachable(sCode):
    """Every command that advertises a key must actually answer to one.

    menu_Helper's second argument is only the key DISPLAYED beside the command:
    in the menu, in Key Describer, in the Alternate Menu, and in the generated
    hotkey document. The keystroke itself is dispatched by a separate switch in
    ProcessCmdKey. Nothing connects the two.

    So a command can advertise a key it does not have, and everything a person
    consults will agree with it. That happened: four commands were added with
    keys, and none of the four was reachable from the keyboard; F12 went on
    starting the timer while the menu, the hotkey document and Key Describer all
    said Chat with AI. Only pressing the key showed it, and only because the
    wrong thing spoke.

    A handful of commands are dispatched without going through
    clickOrDescribe -- Enter and Shift+Enter branch on what the item is and call
    item_Helper -- so those are named here rather than reported.
    """
    lHandled = ["Open Item", "Go to Item"]
    setFired = set()
    for sLine in sCode.splitlines():
        sTrim = sLine.strip()
        if sTrim.startswith("//"):
            continue
        oMatch = re.search(r"(?:App\.frame\.)?(menu\w+)\.clickOrDescribe\(\)", sTrim)
        if oMatch:
            setFired.add(oMatch.group(1))
    lUnreachable = []
    for sLine in sCode.splitlines():
        if sLine.lstrip().startswith("//"):
            continue
        oMatch = re.search(r'(menu\w+)\s*=\s*menu_Helper\(\s*@?"([^"]*)"\s*(?:,\s*@?"([^"]*)")?', sLine)
        if not oMatch or not oMatch.group(3):
            continue
        sField = oMatch.group(1)
        sName = oMatch.group(2).replace("&", "").replace(" ...", "").strip()
        if sField in setFired or sName in lHandled:
            continue
        lUnreachable.append(sName + " (" + oMatch.group(3) + ")")
    report("Every command with a key is reachable from the keyboard",
           not lUnreachable,
           ", ".join(lUnreachable) + " -- menu_Helper only sets the key SHOWN; "
           "ProcessCmdKey is what dispatches it, and these have no entry there")


def checkKeysUnique(sCode):
    """No two commands may claim the same key.

    A duplicate means one command is unreachable, and nothing in the program
    complains about it.
    """
    dSeen, lClashes = {}, []
    for sName, sKey, iLine in commandsInSource(sCode):
        for sPart in re.split(r"\s+or\s+|,", sKey):
            sPart = sPart.strip()
            if not sPart:
                continue
            sLower = sPart.lower()
            if sLower in dSeen:
                lClashes.append(sPart + " claimed by both " + dSeen[sLower] + " and " + sName)
            else:
                dSeen[sLower] = sName
    report("No key is claimed twice", not lClashes, "; ".join(lClashes))


def checkNoRetiredCommands(sCode, sHotkeys):
    """Hotkeys.ini must not describe a command that no longer exists.

    The Web Client Utilities were removed and their description stayed
    behind, so the Hotkey Summary advertised a command that was gone. A stale
    entry is a documentation fault no compiler sees.
    """
    lAllowed = ["Launch FileDir", "Drive Letter", "Parent Folder", "Come up Level"]
    setLive = set(s for s, k, i in commandsInSource(sCode))
    for sName in sorted(hotkeyEntries(sHotkeys)):
        if sName in setLive or sName.startswith("Say ") or sName in lAllowed:
            continue
        warn("Hotkeys.ini describes '" + sName + "', which is not a live command. "
             "Remove it, or add it to lAllowed in this check if it is deliberate.")


def checkAboutBox(sCode):
    """The About box must not carry a typed version or licence.

    It said "FileDir 5.0 beta", "June 17, 2026" and the LGPL for fourteen
    releases, because all three were typed into the source. The version must
    come from BuildVersion, which the build generates from version.txt.
    """
    report("The About box takes its version from the build",
           re.search(r'"FileDir \d+\.\d+', sCode) is None,
           "a version number is typed into FileDir.cs; it must use BuildVersion.Version")
    # Any GPL mention, not only the two spelled-out names. The header of
    # FileDir.cs read "//Modified GPL License" for months after the change to
    # MIT, and this check walked past it because it was looking for "GNU
    # General Public" and nothing else. A line that also says MIT is a record
    # of the change rather than a claim, and is left alone.
    lClaims = []
    for iLine, sLine in enumerate(sCode.splitlines(), 1):
        if not re.search(r"\bGPL\b|General Public", sLine, re.I):
            continue
        if re.search(r"\bMIT\b", sLine, re.I):
            continue
        lClaims.append("line " + str(iLine))
    report("No GNU licence claimed in the source", not lClaims,
           ", ".join(lClaims) + " -- FileDir is MIT licensed")


def checkUpdateAssetName(sCode, sIss):
    """Elevate Version must ask for exactly the file the installer produces.

    This one was live and broken: the installer produced FileDir_setup.exe
    while Elevate Version asked GitHub for FileDir_Setup.exe. GitHub download
    addresses are case sensitive, so every update check that found a newer
    release then failed to download it.
    """
    oMethod = re.search(r"(?s)void\s+menuHelpElevateVersion_Click.*?"
                        r"// menuHelpElevateVersion_Click method", sCode)
    if not oMethod:
        warn("Could not find the Elevate Version method to check its asset name.")
        return
    oAsset = re.search(r'string\s+sName\s*=\s*"([^"]+\.exe)"', oMethod.group(0))
    oOutput = re.search(r"(?m)^\s*OutputBaseFilename\s*=\s*(\S+)\s*$", sIss)
    if not oAsset or not oOutput:
        warn("Could not find both the asset name and OutputBaseFilename to compare.")
        return
    sAsset, sWanted = oAsset.group(1), oOutput.group(1) + ".exe"
    report("Elevate Version asks for the installer's own file name",
           sAsset == sWanted,
           "it asks GitHub for '" + sAsset + "' but the installer produces '"
           + sWanted + "'; GitHub addresses are case sensitive, so the download fails")


def checkDocumentsShipped(sCode, sIss):
    """Every document the program opens must be one the installer ships.

    The Change History command opened History.txt and the Hotkey Summary
    opened HotKeys.txt. Both were replaced by the Markdown set, so both
    commands would have failed on an upgraded installation with nothing but a
    Windows error.
    """
    lMissing = []
    for oMatch in re.finditer(r'Path\.Combine\(App\.sAppDir,\s*"([^"]+\.(?:htm|html|md|txt))"\)', sCode):
        sDoc = oMatch.group(1)
        if ('"' + sDoc + '"') not in sIss:
            lMissing.append(sDoc)
    report("Every document the program opens is shipped", not lMissing,
           ", ".join(lMissing) + " -- the command would fail on an installed copy"
           if lMissing else "")


def checkInstallerSourcesExist(sIss):
    """Every file the setup script names must exist.

    Inno reports a missing source as an error only for entries without
    skipifsourcedoesntexist. Entries with that flag fail silently, so a file
    can quietly stop shipping.
    """
    setTracked, setLocal = homerPolicy.repoFiles(pathRoot)
    bInFiles, lMissing, lSilent, lNotYetBuilt = False, [], [], []
    for iLine, sLine in enumerate(sIss.splitlines(), 1):
        sTrim = sLine.strip()
        if sTrim.startswith("["):
            bInFiles = (sTrim == "[Files]")
            continue
        if not bInFiles or not sTrim or sTrim.startswith(";"):
            continue
        oMatch = re.search(r'Source:\s*"([^"]+)"', sTrim)
        if not oMatch:
            continue
        sSource = oMatch.group(1)
        if "*" in sSource or "?" in sSource:
            sFolder = os.path.dirname(sSource.replace("\\", "/")) or "."
            lHere = [s.lower() for s in os.listdir(pathRoot)]
            if sFolder != "." and sFolder.lower() not in lHere:
                warn("Installer pattern matches nothing: " + sSource + " (line " + str(iLine) + ")")
            continue
        if os.path.exists(os.path.join(pathRoot, sSource.replace("\\", "/"))):
            continue
        if sSource.lower() in setLocal:
            # Build output. The audit runs BEFORE the compile, on purpose, so
            # FileDir.exe cannot exist yet on a clean tree. Failing on it would
            # mean the build could never run twice from a fresh clone. If it is
            # still missing when the installer is compiled, Inno says so itself.
            lNotYetBuilt.append(sSource)
            continue
        if "skipifsourcedoesntexist" in sTrim.lower():
            lSilent.append(sSource)
        else:
            lMissing.append(sSource + " (line " + str(iLine) + ")")
    report("Every file the installer names exists", not lMissing, ", ".join(lMissing))
    if lNotYetBuilt:
        say("      (" + ", ".join(lNotYetBuilt) + " will be built before the installer is compiled)")
    for sSource in lSilent:
        warn("Installer source missing, and flagged skipifsourcedoesntexist, so it ships nothing: " + sSource)


def checkNoVersionLiteral(sIss):
    """The setup script must carry no version number of its own.

    This is the fault that cost a release. A stale copy of the script with
    its own AppVersion rewound the version, and the next build re-minted a
    number that was already published. version.txt is the only place a
    version may live.
    """
    report("The installer holds no version literal",
           re.search(r'(?m)^\s*(#define\s+AppVersion\s+"?\d|AppVersion\s*=\s*\d)', sIss) is None,
           "only version.txt may hold the version; the setup script must read it")


def checkDocumentationSet():
    """The standard Homer Tools documentation set must be complete."""
    lWanted = ["ReadMe.md", "FileDir.md", "Developer.md", "License.md",
               "History.md", "Hotkeys.md", "FAQ.md", "Tutorials.md", "Announce.md"]
    lMissing = [s for s in lWanted if not os.path.isfile(os.path.join(pathRoot, s))]
    report("The documentation set is complete", not lMissing, ", ".join(lMissing))


def checkDocumentsCurrent():
    """The documents must not describe a FileDir that no longer exists."""
    lVersion = (readFile("version.txt") or "").strip().splitlines()
    sVersion = lVersion[0].strip() if lVersion else ""
    lAcceptable = []
    if sVersion:
        lAcceptable.append(sVersion)
        # The audit runs BEFORE the build takes the next number, so a document
        # written for the release being prepared names version.txt plus one.
        # Accepting both keeps this from warning on every single build.
        lParts = sVersion.split(".")
        if lParts and lParts[-1].isdigit():
            lParts[-1] = str(int(lParts[-1]) + 1)
            lAcceptable.append(".".join(lParts))
    lLagging = []
    for sDoc in ("ReadMe.md", "FileDir.md", "Developer.md", "History.md",
                 "Hotkeys.md", "Announce.md", "FAQ.md", "Tutorials.md",
                 "License.md"):
        sText = readFile(sDoc)
        if sText is None:
            continue
        sHead = "\n".join(sText.splitlines()[:12])
        if not re.search(r"(?m)^\*\*Version \d", sHead):
            warn(sDoc + " has no version line for the build to stamp. Add a line "
                 "reading **Version 0.0.0** near the top.")
            continue
        if lAcceptable and not any(s in sHead for s in lAcceptable):
            # A NOTE, not a warning. The audit runs BEFORE the version step, so
            # what it sees is whatever the last build left -- and just after
            # unarchiving that is whatever version the archive was made at. The
            # build stamps every document nine lines later, so this is the state
            # of the tree, not a fault. Eight of these on one build said nothing
            # eight times.
            #
            # The real check is the one above: a document with no version line
            # at all is a document the build cannot stamp, and that stays a
            # warning.
            lLagging.append(sDoc)
    if lLagging:
        say("NOTE  " + plural(len(lLagging), "document")
            + " not yet stamped with version " + " or ".join(lAcceptable)
            + ". The build stamps them; this is the state of the tree, not a fault.")

    # History and Developer may NAME the GNU licence in their bodies, because
    # recording what changed means saying what it changed from. Their opening
    # lines are a different matter: those state what the program is now, and
    # History.md's header went on claiming the modified GPL for three releases
    # because a whole-file exemption hid it.
    for sDoc in ("ReadMe.md", "FileDir.md", "Hotkeys.md"):
        sText = readFile(sDoc)
        if sText is None:
            continue
        if re.search(r"\bGPL\b|General Public License", sText, re.I):
            report(sDoc + " names no GNU licence", False, "FileDir is MIT licensed")
    for sDoc in ("History.md", "Developer.md", "Announce.md", "FAQ.md",
                 "Tutorials.md"):
        sText = readFile(sDoc)
        if sText is None:
            continue
        sHead = "\n".join(sText.splitlines()[:10])
        if re.search(r"\bGPL\b|General Public License", sHead, re.I):
            report(sDoc + " names no GNU licence in its header", False,
                   "the body may record the change to MIT, but the opening lines "
                   "state what FileDir is licensed under now")
    for sDoc in ("ReadMe.md", "FileDir.md"):
        sText = readFile(sDoc)
        if sText is None:
            continue
        for sPattern, sWhy in ((r"dirsetup\.exe", "the installer is FileDir_setup.exe"),
                               (r"Web Client Utilit", "they were removed in 5.0")):
            if re.search(sPattern, sText, re.I):
                report(sDoc + " is current", False, sWhy)
        for sPattern, sWhy in ((r"Internet Explorer", "Internet Explorer is gone"),
                               (r"\bGetText\b", "2htm replaced GetText")):
            if re.search(sPattern, sText, re.I):
                warn(sDoc + " still mentions something retired: " + sWhy)


def checkHtmlPairs():
    """Every document must have an HTML twin, and it must hold something.

    The installer ships each .htm with skipifsourcedoesntexist, so a conversion
    that failed ships nothing and says nothing. That happened: 2htm could not
    load System.Memory and all nine documents silently kept whatever HTML was
    there before, with only a warning in the build log to show for it.

    A zero-byte file is treated as missing, and worse than missing: it looks
    like a document on the Start menu and reads as nothing at all.
    """
    lMissing = []
    lEmpty = []
    lStale = []
    for sDoc in ("ReadMe.md", "FileDir.md", "Developer.md", "License.md",
                 "History.md", "Hotkeys.md", "Announce.md", "FAQ.md",
                 "Tutorials.md"):
        pathMd = os.path.join(pathRoot, sDoc)
        if not os.path.isfile(pathMd):
            continue
        pathHtm = os.path.splitext(pathMd)[0] + ".htm"
        if not os.path.isfile(pathHtm):
            lMissing.append(os.path.basename(pathHtm))
            continue
        if os.path.getsize(pathHtm) == 0:
            lEmpty.append(os.path.basename(pathHtm))
            continue
        # Older than its source means the last conversion did not take.
        if os.path.getmtime(pathHtm) < os.path.getmtime(pathMd) - 2:
            lStale.append(os.path.basename(pathHtm))
    # WARNINGS, NOT FAILURES, AND HERE IS WHY.
    #
    # The audit runs BEFORE the documents are converted, on purpose: nothing is
    # compiled until the source checks pass. So this is looking at the state of
    # the tree from the LAST build, not at what this one will produce. Failing
    # here made the build refuse to run the very step that fixes the problem --
    # a missing ReadMe.htm stopped the build that would have written ReadMe.htm.
    #
    # The real check belongs after the conversion, and BuildFileDir does it
    # there: it stops the build when a document ends up with no HTML, which is
    # the moment the answer is "the converter is broken" rather than "you have
    # not built yet".
    if lMissing:
        warn(plural(len(lMissing), "document") + " with no HTML yet: "
             + ", ".join(lMissing) + ". The build writes them; this only matters "
             "if it is still true afterwards.")
    else:
        say("PASS  Every document has an HTML twin")
    if lEmpty:
        warn("Empty HTML: " + ", ".join(lEmpty)
             + ". A zero-byte page looks like a document and reads as nothing. "
             "The build deletes and rewrites them.")
    else:
        say("PASS  No document HTML is empty")
    # One line rather than nine. On a tree that has just been unarchived every
    # document is newer than its HTML, which is normal and says nothing.
    if lStale:
        # No names. This is the ordinary state of a folder just unarchived, and
        # listing the files invites somebody to go and look at nothing.
        say("NOTE  " + plural(len(lStale), "document")
            + " older than its Markdown, which is normal after unarchiving. "
            + "The build rewrites them.")


def checkLicenceDocument():
    """License.md must state the MIT licence and name FileDir.

    The licence is the one document where being out of date is a legal claim
    rather than a stale sentence, so it gets a check of its own. The MIT text
    is kept verbatim so it stays recognisable; only the app and author are
    named around it.
    """
    sText = readFile("License.md")
    if sText is None:
        report("License.md is present", False, "not found in " + pathRoot)
        return
    report("License.md states the MIT licence", "MIT License" in sText,
           "it does not say MIT License")
    report("License.md names FileDir", "FileDir" in sText,
           "the licence should name the program it covers")
    report("License.md names the author", "Jamal Mazrui" in sText,
           "the copyright line is missing")
    # The three sentences that make it the MIT licence rather than something
    # that merely says MIT at the top.
    for sPhrase in ("Permission is hereby granted, free of charge",
                    "without restriction",
                    "THE SOFTWARE IS PROVIDED \"AS IS\""):
        if sPhrase not in sText:
            report("License.md carries the MIT text in full", False,
                   "the phrase " + repr(sPhrase) + " is missing")
            return
    report("License.md carries the MIT text in full", True)
    # Every document that states a licence must state the same one.
    for sDoc in ("ReadMe.md", "FileDir.md", "Developer.md", "History.md",
                 "Hotkeys.md", "Announce.md", "FAQ.md", "Tutorials.md"):
        sOther = readFile(sDoc)
        if sOther is None:
            continue
        sHead = "\n".join(sOther.splitlines()[:10])
        if "License" in sHead and "MIT License" not in sHead:
            report(sDoc + " states the MIT licence in its header", False,
                   "its opening lines name a licence that is not MIT")


def checkNoStrayProject():
    """No document in the folder may belong to another program.

    ReadMe.md and Announce.md here were urlFido's for a while, and would have
    shipped as FileDir's if the installer had named them.
    """
    lStray = []
    for sName in sorted(os.listdir(pathRoot)):
        if not sName.lower().endswith(".md"):
            continue
        sText = readFile(sName) or ""
        sHead = " ".join(sText.splitlines()[:3])
        if re.search(r"urlFido|EdSharp\s+.\s+User Guide|DbDo\s+.\s+User Guide", sHead, re.I):
            lStray.append(sName)
    report("No document belongs to another project", not lStray, ", ".join(lStray))


def checkInstallAndDeleteAgree(sIss):
    """The installer must not ship a file it also deletes.

    [InstallDelete] runs before the files are copied, so an entry in both
    sections means the installer removes the old copy and then puts a new one
    straight back. It read as a retirement and did nothing at all.

    This happened with Scripts\\*, a folder-wide line that shipped the retired
    FileDir_Scripts_setup.exe while [InstallDelete] named that same file.
    """
    setShipped = set()
    lPatterns = []
    bInFiles = False
    for sLine in sIss.splitlines():
        sTrim = sLine.strip()
        if sTrim.startswith("["):
            bInFiles = (sTrim == "[Files]")
            continue
        if not bInFiles or sTrim.startswith(";"):
            continue
        oMatch = re.search(r'Source:\s*"([^"]+)"', sTrim)
        if not oMatch:
            continue
        sSource = oMatch.group(1).replace("\\", "/").lower()
        if "*" in sSource or "?" in sSource:
            lPatterns.append(sSource)
        else:
            setShipped.add(sSource)

    lBoth = []
    bInDelete = False
    for sLine in sIss.splitlines():
        sTrim = sLine.strip()
        if sTrim.startswith("["):
            bInDelete = (sTrim == "[InstallDelete]")
            continue
        if not bInDelete or sTrim.startswith(";"):
            continue
        oMatch = re.search(r'Name:\s*"\{app\}\\([^"]+)"', sTrim)
        if not oMatch:
            continue
        sName = oMatch.group(1).replace("\\", "/").lower()
        if sName in setShipped:
            lBoth.append(sName)
            continue
        for sPattern in lPatterns:
            if fnmatch.fnmatch(sName, sPattern):
                lBoth.append(sName + " (matched by " + sPattern + ")")
                break
    report("The installer ships nothing it also deletes", not lBoth,
           ", ".join(lBoth) + " -- [InstallDelete] runs first, so the file is "
           "removed and then put straight back")


def checkInstallerCode(sIss):
    """The installer script must parse: balanced blocks and honest comments.

    Two faults, both found the hard way while adding the Ollama checkboxes.

    A Pascal brace comment ends at the FIRST closing brace, so a comment that
    mentions an Inno constant -- "so {cmd} is the 64-bit shell" -- ends in the
    middle of its own sentence and hands the rest of the prose to the compiler
    as code. Comments naming a constant must use double slashes.

    And begin, try and end must balance. Inno reports a mismatch far from its
    cause, and whoever edits this file may not be able to compile it.
    """
    iStart = sIss.find("[Code]")
    if iStart < 0:
        return
    sCode = sIss[iStart:]

    # Walk the code once, the way Pascal does: brace comment, then slash
    # comment, then string. Order matters -- checking strings first makes a
    # comment containing an apostrophe swallow the rest of the file.
    lOut = []
    lBadComments = []
    i, n, iLine = 0, len(sCode), sIss[:iStart].count("\n") + 1
    while i < n:
        c = sCode[i]
        if c == "\n":
            iLine += 1
            i += 1
            continue
        if c == "{":
            j = sCode.find("}", i)
            sSeg = sCode[i:(j + 1 if j > 0 else n)]
            # Prose left stranded after the comment closes is the symptom.
            iEol = sCode.find("\n", j + 1) if j > 0 else -1
            sAfter = sCode[j + 1:iEol if iEol > 0 else n].strip()
            if sAfter and not sAfter.startswith((";", "+", ")", "then", "do", "else")):
                lBadComments.append("line " + str(iLine))
            iLine += sSeg.count("\n")
            i = (j + 1 if j > 0 else n)
            continue
        if c == "/" and i + 1 < n and sCode[i + 1] == "/":
            while i < n and sCode[i] != "\n":
                i += 1
            continue
        if c == "'":
            i += 1
            while i < n:
                if sCode[i] == "'":
                    if i + 1 < n and sCode[i + 1] == "'":
                        i += 2
                        continue
                    i += 1
                    break
                if sCode[i] == "\n":
                    iLine += 1
                i += 1
            lOut.append(" STR ")
            continue
        lOut.append(c)
        i += 1
    sClean = "".join(lOut)

    report("No installer comment ends early on a brace", not lBadComments,
           ", ".join(lBadComments) + " -- a brace comment ends at the first "
           "closing brace, so one naming an Inno constant leaves its own prose "
           "to be compiled; use // for those")

    iBegin = len(re.findall(r"\bbegin\b", sClean, re.I))
    iTry = len(re.findall(r"\btry\b", sClean, re.I))
    iEnd = len(re.findall(r"\bend\b", sClean, re.I))
    report("The installer code blocks balance", iBegin + iTry == iEnd,
           "begin " + str(iBegin) + " plus try " + str(iTry) + " needs "
           + str(iBegin + iTry) + " ends, but there are " + str(iEnd))

    # Every routine named by a {code:...} or Check: must actually be defined.
    setDefined = set(m.lower() for m in
                     re.findall(r"\b(?:function|procedure)\s+(\w+)", sClean, re.I))
    lMissing = []
    for sName in sorted(set(re.findall(r"\{code:(\w+)\}", sIss))
                        | set(re.findall(r"Check:\s*(\w+)", sIss))):
        if sName.lower() not in setDefined:
            lMissing.append(sName)
    report("Every installer routine referenced is defined", not lMissing,
           ", ".join(lMissing))


def checkInstallerQuoting(sIss):
    """No installer parameter may use a backslash to escape a quote.

    Inno escapes a quote inside a quoted value by DOUBLING it. A backslash
    means nothing there, and the compiler reports "Mismatched or misplaced
    quotes on parameter" -- a message that names the parameter but not the
    reason.

    This cost a build. The generator that wrote these lines emitted a backslash
    before each quote, which is the C rule, not Inno's. The same mistake in
    PowerShell is caught by checkPowerShellQuoting; this is its counterpart for
    the one other file where quoting is easy to get wrong.
    """
    lFaults = []
    for iLine, sLine in enumerate(sIss.splitlines(), 1):
        sTrim = sLine.strip()
        if sTrim.startswith(";"):
            continue
        if '\\"' in sTrim:
            lFaults.append("line " + str(iLine))
    report("No installer line escapes a quote with a backslash", not lFaults,
           ", ".join(lFaults) + " -- Inno doubles a quote to escape it; a "
           "backslash gives 'Mismatched or misplaced quotes'")


def extensionLists():
    """The extension tables in Convert.cs, read out of the source."""
    sText = readFile("Convert.cs")
    if sText is None:
        return None
    dLists = {}
    for oMatch in re.finditer(r"string\[\]\s+(c_a\w+)\s*=\s*\{(.*?)\};", sText, re.S):
        dLists[oMatch.group(1)] = set(
            s.strip().strip('"').lower()
            for s in oMatch.group(2).replace("\n", "").split(",") if s.strip())
    return dLists


def checkConversionChain():
    """Trace every file type through the conversion chain, on every build.

    Three faults were found by tracing this by hand, and all three were the same
    shape: two tables that had to agree, and did not.

      * Three separate lists claimed to say what Pandoc reads. One routed .bib,
        .jats, .opml and .tsv to Pandoc; another then refused them. A third
        called .pptx and .xlsx Pandoc-readable, which they are not.
      * .pptx and .xlsx were categorised as documents, so Output Type offered
        them ten targets and the converter refused all ten.
      * A PDF was offered only flat text, when the reader produces Markdown from
        which Pandoc can make anything.

    None of those would show up in a compiler, and each would have reached a
    tester as "the command did nothing". So the trace runs here instead.
    """
    dLists = extensionLists()
    if dLists is None:
        return
    setPandoc = dLists.get("c_aPandocReadable", set())
    setDocument = dLists.get("c_aDocumentSources", set())
    setPlain = dLists.get("c_aPlainSources", set())
    lFaults = []

    if not setPandoc:
        lFaults.append("c_aPandocReadable is missing or empty")

    # Every format Pandoc reads must be offered the document targets, or Output
    # Type will say a readable file cannot become anything.
    for sExt in sorted(setPandoc - setDocument):
        lFaults.append(sExt + " is Pandoc-readable but not a document source")

    # And nothing may be called a document that no engine can convert. The two
    # Open XML formats are the deliberate exception: FileDir reads them itself.
    for sExt in sorted(setDocument - setPandoc - {".pptx", ".xlsx"} - setPlain):
        lFaults.append(sExt + " is a document source that nothing can read")

    # Pandoc does not read these, whatever any list says. Checked against what
    # pandoc --list-input-formats actually reports.
    for sExt in (".pdf", ".doc", ".ppt", ".xls", ".pptx", ".xlsx"):
        if sExt in setPandoc:
            lFaults.append(sExt + " is listed as Pandoc-readable and Pandoc cannot read it")

    # Every category the router names must have targets, and every category with
    # targets must have a branch in the router. This is the offered-then-refused
    # fault, caught by construction.
    sText = readFile("Convert.cs") or ""
    setCategories = set(re.findall(r'return "(document|legacy|openxml|pdf|audio|video|image)"', sText))
    setWithTargets = set(re.findall(r'sCategory == "(\w+)"', sText))
    for sCategory in sorted(setCategories - setWithTargets):
        lFaults.append("category " + sCategory + " is returned but no branch handles it")

    report("The conversion chain is consistent", not lFaults, "; ".join(lFaults))


def checkHistoryContents():
    """Every entry in History.md's contents must have a heading, and the reverse.

    Five release entries went missing without a sound. Each was inserted by
    replacing the heading of the release before it, and a Python str.replace
    whose pattern is absent does nothing and says nothing -- so when one
    insertion failed, it removed the anchor for the next, and five in a row were
    lost while the contents list went on advertising them.

    A contents entry with no section is a link to nowhere; a section missing
    from the contents cannot be reached by a reader working down the list. Both
    are worth catching, and neither is visible by reading the top of the file.
    """
    sText = readFile("History.md")
    if sText is None:
        return
    lHeadings = re.findall(r"(?m)^## (.+?)\s*$", sText)
    lContents = re.findall(r"(?m)^- \[(.+?)\]\(#", sText)
    setHeadings = set(lHeadings)
    lOrphanLinks = [s for s in lContents if s not in setHeadings]
    setContents = set(lContents)
    lUnlisted = [s for s in lHeadings
                 if s not in setContents and s.lower() != "contents"]
    report("Every history contents entry has a section", not lOrphanLinks,
           ", ".join(lOrphanLinks) + " -- listed but not present")
    report("Every history section is in the contents", not lUnlisted,
           ", ".join(lUnlisted) + " -- present but not listed")

    # The release the build is about to make should be the newest entry, so a
    # release cannot go out with nothing recorded about it.
    lVersion = (readFile("version.txt") or "").strip().splitlines()
    sVersion = lVersion[0].strip() if lVersion else ""
    if sVersion:
        lParts = sVersion.split(".")
        lWanted = [sVersion]
        if lParts and lParts[-1].isdigit():
            lParts[-1] = str(int(lParts[-1]) + 1)
            lWanted.append(".".join(lParts))
        if not any(("Version " + s) in setHeadings for s in lWanted):
            warn("History.md has no entry for version " + " or ".join(lWanted)
                 + ". Add one before releasing; a release with nothing recorded "
                 "about it cannot be explained later.")


def check2htmAssemblies():
    """2htm needs System.Memory beside it, or it fails on everything silently.

    On .NET Framework 4.8 a package whose members are declared with Span needs
    System.Memory.dll present. Without it 2htm prints "Could not load file or
    assembly" and STILL EXITS WITH CODE 0, so every caller that trusted the exit
    code concluded all was well. That is how Say Contents came to say nothing at
    all on a tester's machine, and how nine documents were silently not
    converted on the developer's.

    A warning rather than a failure: 2htm is optional, Pandoc covers most of
    what it did, and the file is not ours to redistribute from here.
    """
    if not os.path.isfile(os.path.join(pathRoot, "2htm.exe")):
        return
    if os.path.isfile(os.path.join(pathRoot, "System.Memory.dll")):
        say("PASS  2htm has the assemblies it needs")
        return
    warn("2htm.exe is here but System.Memory.dll is not. 2htm will fail on every "
         "file and still exit with code 0. Copy System.Memory.dll into this "
         "folder; the installer ships it when it is present.")


def checkVersionFile():
    """version.txt must be a bare version number with no byte order mark.

    Four things read this file: this audit, BuildFileDir, FileDir_setup.iss and
    tagRelease. Only PowerShell is forgiving about a leading byte order mark,
    and it both writes one and silently strips it again on reading, so the
    fault is invisible from that side. Inno Setup is not forgiving: it read the
    mark as part of the number and refused to compile, reporting
    "VersionInfoVersion is invalid" against a line that was perfectly correct.

    This is a warning rather than a failure, because the version step of the
    build rewrites the file without a mark. Failing here would stop the build
    before it reached the step that repairs the file.
    """
    pathVersion = os.path.join(pathRoot, "version.txt")
    if not os.path.isfile(pathVersion):
        report("version.txt is present", False, "not found in " + pathRoot)
        return
    with open(pathVersion, "rb") as fileVersion:
        aBytes = fileVersion.read()
    if aBytes.startswith(b"\xef\xbb\xbf"):
        warn("version.txt begins with a byte order mark. Inno Setup reads it as "
             "part of the version number and refuses to compile. The build "
             "rewrites the file without one, so this repairs itself.")
        aBytes = aBytes[3:]
    sVersion = aBytes.decode("utf-8", errors="replace").strip()
    report("version.txt holds a bare version number",
           re.match(r"^\d+(\.\d+)*$", sVersion) is not None,
           "it holds " + repr(sVersion) + ", which is not a plain dotted number")


def checkListsAgree():
    """A file may not be in both RepoFiles lists.

    Tracked means "the repository carries it". Local means "it lives here
    without being tracked". A name in both says two opposite things, and which
    one wins would then depend on the order a reader happened to check.

    Note what is NOT a contradiction: a file the installer ships that is also
    Local. FileDir.exe, FileDirScript.dll, KeyMap.cs and Ude.dll are all
    shipped and all produced by the build, which runs before the installer is
    compiled, so ISCC finds them without the repository carrying them. An
    earlier version of this check called that a fault and would have failed
    every build.
    """
    setTracked, setLocal = homerPolicy.repoFiles(pathRoot)
    lAlsoTracked = sorted(setLocal & setTracked)
    report("No file is in both RepoFiles lists", not lAlsoTracked,
           ", ".join(lAlsoTracked) + " -- Tracked and Local mean opposite things")


def checkPowerShellQuoting():
    """No PowerShell script may contain a backslash-escaped quote.

    PowerShell has no \" escape. It escapes a quote with a backtick, or by
    doubling it. A \" is read as a backslash followed by the end of the
    string, and the parser then fails several lines later with a message that
    points at the wrong thing.

    This is here because it happened. The key map generator emitted C# from
    PowerShell string literals and wrote the C# quotes as \", so
    BuildFileDir.ps1 would not parse. A script that will not parse never
    reaches its own logging, so the build produced a wall of parser errors and
    an empty log, which is the worst possible way to fail. The generator now
    lives in makeKeyMap.py, where the quoting is simple, and this check makes
    sure the pattern does not come back.
    """
    lFaults = []
    for sName in sorted(os.listdir(pathRoot)):
        if not sName.lower().endswith(".ps1"):
            continue
        sText = readFile(sName) or ""
        for iLine, sLine in enumerate(sText.splitlines(), 1):
            if sLine.lstrip().startswith("#"):
                continue
            if '\\"' in sLine:
                lFaults.append(sName + " line " + str(iLine))
    report("No PowerShell script uses a backslash-escaped quote", not lFaults,
           ", ".join(lFaults) + " -- PowerShell escapes a quote with a backtick, "
           "and the script will not parse")


def checkPowerShellContinuation():
    """No PowerShell line may begin with a continuation operator.

    PowerShell does not continue a statement because the NEXT line starts with a
    plus. It wants the operator at the END of the line, or a backtick. Written
    the other way round, the statement ends early and the parser reports a
    missing bracket several lines away from the real cause.

    This broke a build, and parse errors are the worst kind here: a script that
    will not parse never runs a line of itself, including the line that opens
    its own log. The wrappers now capture the console so the errors are at least
    recorded; this check stops them being written in the first place.

    Long messages are built a line at a time into a variable instead.
    """
    lFaults = []
    for sName in sorted(os.listdir(pathRoot)):
        if not sName.lower().endswith(".ps1"):
            continue
        sText = readFile(sName) or ""
        for iLine, sLine in enumerate(sText.splitlines(), 1):
            sTrim = sLine.strip()
            if sTrim.startswith("#"):
                continue
            if re.match(r"^[+\-]\s", sTrim) or re.match(r"^-(?:join|and|or|eq|ne|match|replace)\b", sTrim):
                lFaults.append(sName + " line " + str(iLine))
    report("No PowerShell line starts with a continuation operator", not lFaults,
           ", ".join(lFaults) + " -- PowerShell wants the operator at the END of "
           "the line; build the message in a variable instead")


def checkScriptsWellFormed():
    """Every delivered script writes a debug-grade log and acts by default.

    Two Homer guidelines, both learned the hard way. A script must write a
    detailed log beside itself, including the reason it stopped, because a
    console traceback with an empty log is useless for debugging by upload.
    And a script must do its job when run with no parameters: a confirmation
    word is a manual step in disguise.
    """
    oInstalled = homerPolicy.installedFiles(pathRoot)
    setTracked, setLocal = homerPolicy.repoFiles(pathRoot)
    lFaults = []
    for sName in sorted(os.listdir(pathRoot)):
        sLower = sName.lower()
        if not (sLower.endswith(".ps1") or sLower.endswith(".py")):
            continue
        if sLower in ("homerpolicy.py",          # a module, not a command
                      "auditfiledir.py",         # run by the build, and by "BuildFileDir audit"
                      "makekeymap.py",           # a build step, not a command
                      # pdfRich.py is called by FileDir with a source and a
                      # target, and it DOES log and DOES trap -- but beside the
                      # target it was given, as <target>.log, which is what lets
                      # FileDir quote the reason a particular file failed. A
                      # fixed pdfRich.log would be the wrong shape for a helper
                      # that runs many times on many files, and a .cmd wrapper
                      # would earn nothing when the caller is a program.
                      "pdfrich.py",
                      "tagrelease.ps1", "postpage.ps1"):
            continue
        if oInstalled is not None and not homerPolicy.belongsInFolder(
                sName, oInstalled, setTracked, setLocal):
            continue                      # an old draft, on its way to notes
        sText = readFile(sName) or ""
        sStem = os.path.splitext(sName)[0]
        # Its own log, or the shared setup log. An installer component writes to
        # the log the whole installation shares, which is the right place for it
        # -- the person sending a setup problem should send one file, not five.
        bLogs = ((sStem + ".log") in sText) or ("FileDir_setup.log" in sText)
        if not bLogs:
            lFaults.append(sName + " writes no named log")
        # A trap, a top-level try/catch, or Python's traceback. All three put
        # the reason a script stopped into the log; insisting on one shape would
        # be a style rule wearing a safety rule's clothes.
        bTrapped = ("trap {" in sText) or ("traceback.format_exc" in sText) \
                   or (re.search(r"(?m)^\s*\}?\s*catch\s*\{", sText) is not None)
        if not bTrapped:
            lFaults.append(sName + " has no handler for an unexpected failure")
        if not os.path.isfile(os.path.join(pathRoot, sStem + ".cmd")):
            lFaults.append(sName + " has no matching " + sStem + ".cmd wrapper")
    report("Every script logs, traps and has a wrapper", not lFaults, "; ".join(lFaults))


def checkFolderIsSorted():
    """Nothing unexpected may sit at the root.

    cleanFileDir sweeps the folder, but only when it is run. This catches
    whatever lands afterwards and does not belong, so a stray is noticed at
    the next build rather than at the next release. Both this and the sweep
    read homerPolicy, so they cannot disagree.
    """
    oInstalled = homerPolicy.installedFiles(pathRoot)
    if oInstalled is None:
        report("The project folder is sorted", False,
               "the setup script could not be read, so nothing can be judged")
        return
    setTracked, setLocal = homerPolicy.repoFiles(pathRoot)
    if not setTracked:
        report("The project folder is sorted", False,
               "RepoFiles.txt names no tracked file, so every build script looks like a stray")
        return
    lStray = []
    for sName in sorted(os.listdir(pathRoot)):
        if os.path.isdir(os.path.join(pathRoot, sName)):
            continue
        if homerPolicy.belongsInFolder(sName, oInstalled, setTracked, setLocal):
            continue
        lStray.append(sName)
    if lStray:
        warn(plural(len(lStray), "file") + " at the root that nothing claims: "
             + ", ".join(lStray[:12]) + ("..." if len(lStray) > 12 else "")
             + ". Run cleanFileDir to move them into notes, or name them in "
             "FileDir_setup.iss or RepoFiles.txt. The full list is in cleanFileDir's survey.")
    if not lStray:
        say("PASS  The project folder holds only what the setup script and RepoFiles.txt claim")


def checkTrackedFiles():
    """Nothing that does not belong may be tracked by git.

    .gitignore has no effect on a file that is already tracked, so adding a
    name to it after the fact changes nothing at all. Only this check notices.
    """
    if not os.path.isdir(os.path.join(pathRoot, ".git")):
        say("SKIP  Not a git working tree, so tracked files were not checked")
        return
    try:
        oResult = subprocess.run(["git", "ls-files"], cwd=pathRoot, capture_output=True,
                                 text=True, encoding="utf-8", errors="replace")
    except Exception as oError:
        warn("Could not list tracked files: " + str(oError))
        return
    if oResult.returncode == 128:
        say("SKIP  Not a usable git working tree, so tracked files were not checked")
        return
    if oResult.returncode != 0:
        warn("git ls-files returned " + str(oResult.returncode))
        return
    oInstalled = homerPolicy.installedFiles(pathRoot)
    if oInstalled is None:
        return
    setTracked, setLocal = homerPolicy.repoFiles(pathRoot)
    lPaths = [s for s in oResult.stdout.splitlines() if s.strip()]
    lStray = [s for s in homerPolicy.strayFiles(lPaths, oInstalled, setTracked)
              if "/" not in s]        # subfolders are their own business
    if lStray:
        warn(plural(len(lStray), "tracked file") + " that nothing claims: "
             + ", ".join(lStray[:20]) + ("..." if len(lStray) > 20 else "")
             + ". Run cleanFileDir.")
    else:
        say("PASS  Every tracked file at the root is claimed")


def startLog():
    """Open the log and record the environment, before anything can fail."""
    global fileLog
    fileLog = open(pathLog, "w", encoding="utf-8")
    say("FileDir audit " + datetime.datetime.now().isoformat(" ", "seconds"))
    say("  script:            " + os.path.abspath(__file__))
    say("  Python:            " + sys.version.split()[0])
    say("  platform:          " + sys.platform)
    say("  working directory: " + os.getcwd())
    say("  command line:      " + " ".join([os.path.basename(sys.argv[0])] + sys.argv[1:]))
    say("  folder audited:    " + pathRoot)
    # The date and size of each script, for the same reason the build records
    # them: a fault that was already fixed is usually a stale copy, and without
    # this there is no way to tell that from a fix that did not work.
    say("  scripts in use:")
    for sName in ("auditFileDir.py", "homerPolicy.py", "makeKeyMap.py",
                  "cleanFileDir.py", "BuildFileDir.ps1", "FileDir_setup.iss",
                  "RepoFiles.txt"):
        pathScript = os.path.join(pathRoot, sName)
        if not os.path.isfile(pathScript):
            say("    " + sName.ljust(20) + " NOT PRESENT")
            continue
        oStat = os.stat(pathScript)
        sWhen = datetime.datetime.fromtimestamp(oStat.st_mtime).strftime("%Y-%m-%d %H:%M")
        say("    " + sName.ljust(20) + " " + sWhen + "  " + str(oStat.st_size) + " bytes")
    say()


def main():
    startLog()

    sCode = readFile("FileDir.cs")
    sIss = readFile("FileDir_setup.iss")
    sHotkeys = readFile("Hotkeys.ini")

    for sName in ("FileDir.cs", "Dialogs.cs", "Lbc.cs", "Say.cs", "Inix.cs",
                  "Util.cs", "Web.cs", "KeyMap.cs", "Ollama.cs", "Convert.cs", "Media.cs", "Log.cs",
                  "FileDir.js",
                  "FileDir.manifest", "FileDir.ico", "version.txt",
                  "Hotkeys.ini", "FileDir_setup.iss", "RepoFiles.txt",
                  "homerPolicy.py", "makeKeyMap.py"):
        if not os.path.isfile(os.path.join(pathRoot, sName)):
            report("Source present: " + sName, False, "not found in " + pathRoot)

    if sCode is None:
        report("FileDir.cs is present", False, "not found in " + pathRoot)
    else:
        checkBracesBalance(sCode, "FileDir.cs", c_iBraceBaseline)
        checkKeysUnique(sCode)
        checkCommandsReachable(sCode)
        checkAboutBox(sCode)
        if sHotkeys is None:
            report("Hotkeys.ini is present", False, "not found in " + pathRoot)
        else:
            checkCommandsDescribed(sCode, sHotkeys)
            checkNoRetiredCommands(sCode, sHotkeys)
        if sIss is not None:
            checkUpdateAssetName(sCode, sIss)
            checkDocumentsShipped(sCode, sIss)

    if sIss is None:
        report("FileDir_setup.iss is present", False, "not found in " + pathRoot)
    else:
        checkInstallerSourcesExist(sIss)
        checkNoVersionLiteral(sIss)
        checkInstallAndDeleteAgree(sIss)
        checkInstallerCode(sIss)
        checkInstallerQuoting(sIss)

    checkDocumentationSet()
    checkDocumentsCurrent()
    checkNoStrayProject()
    checkHtmlPairs()
    checkLicenceDocument()
    checkConversionChain()
    checkHistoryContents()
    check2htmAssemblies()
    checkVersionFile()
    checkListsAgree()
    checkPowerShellQuoting()
    checkPowerShellContinuation()
    checkScriptsWellFormed()
    checkFolderIsSorted()
    checkTrackedFiles()

    say()
    if lWarnings:
        say(plural(len(lWarnings), "warning") + ". They do not stop the build.")
    if lFailures:
        say(plural(len(lFailures), "check") + " failed: " + ", ".join(lFailures))
    else:
        say("All checks passed.")
    say("Log: " + pathLog)
    return 1 if lFailures else 0


if __name__ == "__main__":
    # An unexpected failure must reach the log too. Without this the script
    # prints a traceback to a console that scrolls away and writes nothing,
    # which is the one outcome a log exists to prevent.
    try:
        sys.exit(main())
    except SystemExit:
        raise
    except Exception:
        import traceback
        say("")
        say("The audit stopped on an unexpected error:")
        for sLine in traceback.format_exc().splitlines():
            say("  " + sLine)
        say("Log: " + pathLog)
        sys.exit(1)
    finally:
        if fileLog:
            fileLog.close()
