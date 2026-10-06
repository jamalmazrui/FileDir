"""placeSources.py -- every file the installer names, where the installer looks.

WHY THIS EXISTS

The move to the Homer layout put each shipped file in a folder of its own --
exec, help, configs, data, scripts, scripts\\jaws -- and the installer script
names every one by its new path. A file the migration missed stops the
installer with "Source file ... does not exist", one file at a time, one build
at a time. Four builds were lost that way.

So this reads the installer script itself, works out which sources it needs,
and for each one that is missing looks through the whole project -- the root,
notes, scripts, exec, help, configs, data -- for a file of that name and moves
it into place. Whatever remains missing is reported by name, all at once. It
runs before the installer on every build and does nothing when everything is
already in place.

RUN IT WITH NO ARGUMENTS from anywhere inside the project. The log is written
to logs\\FileDir-sources-yyyyMMdd-HHmmss.log.
"""

import datetime
import os
import re
import shutil
import sys
import traceback

c_sHere = os.path.dirname(os.path.abspath(__file__))
c_sRoot = os.path.dirname(c_sHere) if os.path.basename(c_sHere).lower() in ("scripts", "exec") else c_sHere
c_sIss = os.path.join(c_sRoot, "FileDir_setup.iss")
c_sStamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
c_sLog = os.path.join(c_sRoot, "logs", "FileDir-sources-" + c_sStamp + ".log")

# Where a misplaced file may be lying. The root first, because that is where
# the flat layout kept everything; notes next, because the migration's tidy-up
# filed anything it did not recognise there.
c_lsLookIn = ["", "notes", "scripts", "exec", "help", "configs", "data", "Scripts"]


def note(sText):
    try:
        os.makedirs(os.path.dirname(c_sLog), exist_ok=True)
        with open(c_sLog, "a", encoding="utf-8") as oFile:
            oFile.write(datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S  ") + sText + "\n")
    except Exception:
        pass


def say(sText):
    print(sText)
    note(sText)


def local(sRelative):
    """An installer path, with the separators this system uses."""
    return sRelative.replace("\\", os.sep).replace("/", os.sep)


def readIss():
    """Every Source line: (relative path, required?)."""
    with open(c_sIss, "r", encoding="utf-8-sig", errors="replace") as oFile:
        sText = oFile.read()
    # The template writes the program's name as {#AppExeName}; every other
    # preprocessor value is resolved from its #define line the same way.
    dDefines = dict(re.findall(r'^#define\s+(\w+)\s+"([^"]*)"', sText, re.M))
    lsWanted = []
    for oMatch in re.finditer(r'^Source:\s*"([^"]+)";([^\n]*)$', sText, re.M):
        sPath = oMatch.group(1).strip()
        for sKey, sValue in dDefines.items():
            sPath = sPath.replace("{#" + sKey + "}", sValue)
        bRequired = "skipifsourcedoesntexist" not in oMatch.group(2)
        lsWanted.append((sPath, bRequired))
    return lsWanted


def findElsewhere(sName):
    """The first place a file of this name turns up, or None."""
    for sFolder in c_lsLookIn:
        sTry = os.path.join(c_sRoot, sFolder, sName)
        if os.path.isfile(sTry):
            return sTry
    return None


def wildcardPresent(sRelative):
    """A wildcard source is satisfied when any file matches it."""
    sFolder = os.path.join(c_sRoot, os.path.dirname(local(sRelative)))
    sPattern = os.path.basename(local(sRelative))
    if not os.path.isdir(sFolder):
        return False
    rx = re.compile("^" + re.escape(sPattern).replace(r"\*", ".*").replace(r"\?", ".") + "$", re.I)
    return any(rx.match(sFile) for sFile in os.listdir(sFolder))


def main():
    note("placeSources " + c_sStamp)
    note("script: " + os.path.abspath(__file__))
    note("Python: " + sys.version.split()[0] + ", platform: " + sys.platform)
    note("project: " + c_sRoot)
    note("installer script: " + c_sIss)
    if not os.path.isfile(c_sIss):
        say("FileDir_setup.iss is not here, so there is nothing to check against.")
        return 1

    lsWanted = readIss()
    note("sources named: %d" % len(lsWanted))
    iMoved = 0
    lsMissing = []
    for sRelative, bRequired in lsWanted:
        sFull = os.path.join(c_sRoot, local(sRelative))
        bWild = any(ch in sRelative for ch in "*?")
        if bWild:
            # A WILDCARD IS CHECKED, NEVER GATHERED. "help\*.md" would otherwise
            # pull ReadMe.md and License.md out of the root, and anything a
            # person filed in notes with it. Only a source named in full is
            # moved; a wildcard that matches nothing is reported.
            if bRequired and not wildcardPresent(sRelative):
                lsMissing.append(sRelative)
            continue
        if os.path.isfile(sFull):
            continue
        sFound = findElsewhere(os.path.basename(local(sRelative)))
        if sFound and os.path.normcase(sFound) != os.path.normcase(sFull):
            os.makedirs(os.path.dirname(sFull), exist_ok=True)
            shutil.move(sFound, sFull)
            note("moved " + os.path.relpath(sFound, c_sRoot) + " to " + sRelative)
            iMoved += 1
            continue
        if bRequired:
            lsMissing.append(sRelative)
            note("MISSING (required): " + sRelative)
        else:
            note("absent (optional): " + sRelative)

    # These are built, not found: the compiler and the build make them after
    # this runs, so they are not counted as missing here.
    lsBuilt = ("exec\\FileDir.exe", "exec\\FileDirScript.dll", "exec\\FileDir_setup.exe")
    lsMissing = [s for s in lsMissing if s not in lsBuilt and not s.lower().endswith(".htm")]

    if iMoved:
        say("Moved %d file%s into the folder the installer expects." % (iMoved, "" if iMoved == 1 else "s"))
    if lsMissing:
        say("%d required source%s the installer names %s not in the project anywhere:"
            % (len(lsMissing), "" if len(lsMissing) == 1 else "s", "is" if len(lsMissing) == 1 else "are"))
        for s in lsMissing:
            say("  " + s)
        return 1
    say("Every source the installer names is in place.")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except Exception as oError:
        say("placeSources stopped: " + str(oError))
        note(traceback.format_exc())
        sys.exit(1)
