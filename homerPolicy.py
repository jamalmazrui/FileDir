r"""homerPolicy.py -- what belongs in a Homer Tools project folder.

This file is the same in every Homer Tools project. Nothing in it names
FileDir, EdSharp or any other program, so a fix made in one project can be
copied to the others without reading it first. Everything specific to a
project comes from two places in that project's own folder: its setup script
and its RepoFiles.txt.

WHY THIS FILE EXISTS

Two programs need the same answer and must not disagree about it: the sweep
that tidies the folder, and the audit that fails the build when something
that does not belong has come back. So the answer lives here, in one place,
and both import it.

The fault it was written against is worth remembering. EdSharp's tidy had a
line meant to spare the documentation set:

    if sPath.endswith((".md", ".htm")) and "/" not in sPath:
        return True

What it actually said was that any file at the top of the folder whose name
ends in .md or .htm is part of the project. Every saved Stack Overflow page,
every saved mailing list message and every old draft ends in .htm or .md and
sits at the top of the folder, so the survey declared 38 of them needed and
printed a clean report. .gitignore could not help either, because .gitignore
has no effect on a file that is already tracked.

THE RULE THAT REPLACES IT

A file belongs only if it is NAMED. No pattern admits a file by the look of
its name. There are two ways to be named:

  1. The setup script names it. <App>_setup.iss is the list of what the
     program ships, and a file on that list is part of the project by
     definition. A folder-wide Source line, such as Convert\*, covers
     everything under that folder.
  2. RepoFiles.txt names it. That file lists what the repository tracks
     beyond what is installed -- the scripts that build, check and release
     the program -- and, separately, what lives on disk without being
     tracked, such as build output and logs.

Anything else is a development aid. It stays on disk, where it is useful,
and out of the repository, where it is not.

TO CHANGE WHAT THE PROJECT CARRIES

Add a Source line to the setup script if the program should install the
file. Add the name to RepoFiles.txt if the build needs it or if it is
generated. Doing neither is how a file is kept out. There is no third place
to look.
"""

import fnmatch
import os
import re

c_sRepoFilesName = "RepoFiles.txt"

# Section headings recognised in RepoFiles.txt. A heading is a line ending in
# a colon; anything else that is not blank and not a comment is a name.
c_sTrackedHeading = "tracked"
c_sLocalHeading = "local"

# Only these endings count as documents. FileDir's sweep does not need this,
# because it moves everything unclaimed; EdSharp's moveNotes moves documents
# only, so the shared module offers the test and each project chooses.
c_lDocumentExtensions = [".doc", ".eml", ".htm", ".html", ".md", ".mht",
                         ".pdf", ".rtf", ".txt"]


def appName(pathRoot):
    """The program's name, taken from the single setup script in the folder.

    When several are present -- which means a stray from another project has
    been copied in -- the one whose name matches the folder wins. When none
    matches, an empty string is returned, because guessing which of two
    setup scripts belongs to the project is how a wrong answer looks
    confident.
    """
    lFound = []
    for sName in sorted(os.listdir(pathRoot)):
        if sName.lower().endswith("_setup.iss"):
            lFound.append(sName[:-len("_setup.iss")])
    if not lFound:
        return ""
    if len(lFound) == 1:
        return lFound[0]
    # More than one setup script means a stray from another project has been
    # copied in. The folder is named after its own program, so that decides
    # it -- and refusing here would be worse than useless, because removing
    # the stray is exactly what the sweep is for.
    sFolder = os.path.basename(os.path.abspath(pathRoot))
    for sCandidate in lFound:
        if sCandidate.lower() == sFolder.lower():
            return sCandidate
    return ""


def setupScriptName(pathRoot):
    """The file name of the project's setup script, or an empty string."""
    sApp = appName(pathRoot)
    if not sApp:
        return ""
    for sName in os.listdir(pathRoot):
        if sName.lower() == (sApp + "_setup.iss").lower():
            return sName
    return ""


def installedFiles(pathRoot):
    """What the setup script says the project ships.

    Returns three things -- a set of exact names, a list of folder prefixes,
    and a list of name patterns -- or None when the setup script cannot be
    read, in which case nothing can be judged and the caller should say so
    rather than guess.
    """
    sIssName = setupScriptName(pathRoot)
    if not sIssName:
        return None
    pathIss = os.path.join(pathRoot, sIssName)
    if not os.path.isfile(pathIss):
        return None
    with open(pathIss, encoding="utf-8-sig", errors="replace") as fileIss:
        sIss = fileIss.read()
    setExact, lFolders, lPatterns = set(), [], []
    for oMatch in re.finditer(r'^\s*Source:\s*"([^"]+)"', sIss, re.M):
        sPath = oMatch.group(1).replace("\\", "/")
        # A source may be written with a full path or with an Inno constant;
        # only the part relative to the project folder is comparable.
        sPath = re.sub(r"^[A-Za-z]:/[^/]+/", "", sPath)
        if sPath.startswith("{"):
            continue
        if sPath.endswith("/*"):
            # A folder-wide line, such as Scripts/*, covers the whole tree.
            sFolder = sPath[:-2]
            if sFolder:
                lFolders.append(sFolder + "/")
            continue
        if "*" in sPath or "?" in sPath:
            # A wildcard on file names, such as 7z.*, is a pattern rather than
            # a folder. Treating it as one turned "7z.*" into a folder called
            # "7z." and let anything starting with those characters through.
            lPatterns.append(sPath.lower())
            continue
        setExact.add(sPath.lower())
    return setExact, lFolders, lPatterns


def repoFiles(pathRoot):
    """The two lists in RepoFiles.txt: what is tracked, and what is local.

    Returns a pair of sets of lower-case names. A missing file gives two
    empty sets, which the caller should treat as a fault rather than as a
    project with no build scripts.
    """
    setTracked, setLocal = set(), set()
    pathList = os.path.join(pathRoot, c_sRepoFilesName)
    if not os.path.isfile(pathList):
        return setTracked, setLocal
    setCurrent = None
    with open(pathList, encoding="utf-8-sig", errors="replace") as fileList:
        for sLine in fileList:
            sLine = sLine.strip()
            if not sLine or sLine.startswith("#"):
                continue
            if sLine.endswith(":"):
                sHeading = sLine[:-1].strip().lower()
                if c_sTrackedHeading in sHeading:
                    setCurrent = setTracked
                elif c_sLocalHeading in sHeading:
                    setCurrent = setLocal
                else:
                    setCurrent = None
                continue
            if setCurrent is not None:
                setCurrent.add(sLine.lower())
    return setTracked, setLocal


def isNamedByInstaller(sPath, oInstalled):
    """Whether the setup script ships this path, by name, folder or pattern."""
    setExact, lFolders, lPatterns = oInstalled
    sPath = sPath.replace("\\", "/").lower()
    if sPath in setExact:
        return True
    for sFolder in lFolders:
        if sPath.startswith(sFolder.lower()):
            return True
    for sPattern in lPatterns:
        if fnmatch.fnmatch(sPath, sPattern):
            return True
    return False


def isRepoFile(sPath, oInstalled, setTracked):
    """Whether a path belongs in the repository at all.

    The whole rule, in one place: named by the setup script, or named in
    RepoFiles.txt as tracked. Nothing is admitted by the look of its name.
    """
    if isNamedByInstaller(sPath, oInstalled):
        return True
    return sPath.replace("\\", "/").lower() in setTracked


def belongsInFolder(sPath, oInstalled, setTracked, setLocal):
    """Whether a path belongs in the project folder, tracked or not.

    Build output, generated sources and logs belong on disk without being
    tracked, so a sweep must not carry them off.
    """
    if isRepoFile(sPath, oInstalled, setTracked):
        return True
    return sPath.replace("\\", "/").lower() in setLocal


def isDocument(sName):
    """Whether a name ends like a document rather than like a program."""
    return os.path.splitext(sName)[1].lower() in c_lDocumentExtensions


def strayFiles(lPaths, oInstalled, setTracked):
    """The paths in a list that do not belong in the repository, by name."""
    return sorted(s for s in lPaths
                  if not isRepoFile(s.replace("\\", "/"), oInstalled, setTracked))
