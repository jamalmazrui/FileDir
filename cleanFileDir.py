r"""cleanFileDir.py -- sort the project folder so it holds FileDir and nothing else.

    cleanFileDir              (sort the folder, and untrack what moved)
    cleanFileDir --survey     (print the plan and change nothing)

WHAT THIS IS FOR

C:\FileDir accumulated about twenty years of working material: saved web
pages, downloaded sample libraries, retired programs, files from other
projects, old drafts and test data. It is worth keeping -- it is why several
decisions were made the way they were -- but it is not part of FileDir, and
mixed in with the sources it made the folder hard to read and the repository
large and hard to keep clean.

A folder answers this better than any pattern can. .gitignore cannot sort by
what a file is FOR, only by what its name looks like, and the names look
exactly like the documentation set: both end in .md or .htm and both sit at
the root. Once the material is in a folder of its own, one line in
.gitignore covers all of it, for ever, and no future file has to be judged.

WHAT MOVES, AND WHERE

Everything at the root is one of two things, decided by homerPolicy -- the
same module the audit reads, so this cannot form its own opinion:

  * It belongs. FileDir_setup.iss ships it, or RepoFiles.txt names it as
    tracked or as local. Nothing touches it.
  * Nothing claims it. It moves into notes\, whatever it is: a saved page, a
    draft, a retired program, a downloaded library, an old copy, a test file.

One folder rather than two on purpose. Sorting the unclaimed material into
kinds is work that only pays if something reads the kinds, and nothing does:
none of it goes into the repository, and going through it by hand is a job
for a person, not a script. A second ignored folder would be one more name
to remember for no gain.

The folder stays inside the project and is added to .gitignore, so git stops
tracking what moves while everything remains one step away. Nothing is
deleted, ever, and subdirectories are never entered: what is inside a folder
is that folder's business.

A working document that is neither shipped nor needed by the build, but that
you want left at the root anyway, goes under Local: in RepoFiles.txt. That is
the same list that keeps build output and logs in place, and it means there is
no second list here for the two to drift apart.

A detailed log is written beside this script, whatever happens.
"""

import argparse
import datetime
import os
import shutil
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import homerPolicy

c_sNotesFolder = "notes"
c_sLogName = "cleanFileDir.log"

# Folders kept whole, whatever is inside them.
# Folders kept whole, whatever is inside them. __pycache__ is here because
# Python recreates it the moment a script runs: sweeping it moves a folder that
# comes straight back, and leaves a dated copy in notes on every single build.
c_lKeepFolders = [".git", "Quick", "Scripts", "scripts", "__pycache__",
                  c_sNotesFolder]

pathRoot = os.path.dirname(os.path.abspath(__file__))
pathLog = os.path.join(pathRoot, c_sLogName)
fileLog = None


def say(sMessage=""):
    print(sMessage)
    if fileLog:
        try:
            fileLog.write(sMessage + "\n")
            fileLog.flush()
        except Exception:
            pass


def startLog():
    """Open the log and record the environment, before anything can fail.

    Every setting that could explain a surprising result belongs here: which
    copy of the script ran, which Python, from where, and with what on the
    command line. A log that begins with only a date leaves the reader
    guessing at all four.
    """
    global fileLog
    try:
        # Append, not overwrite: cleanFileDir.cmd opened this file and wrote the
        # first lines before Python started, so that a log exists even when this
        # script will not parse. Truncating here would throw that away.
        fileLog = open(pathLog, "a", encoding="utf-8")
    except Exception as oError:
        print("Could not open the log: " + str(oError))
        return
    say("cleanFileDir  " + datetime.datetime.now().isoformat(" ", "seconds"))
    say("  script:            " + os.path.abspath(__file__))
    say("  Python:            " + sys.version.split()[0])
    say("  platform:          " + sys.platform)
    say("  working directory: " + os.getcwd())
    say("  command line:      " + " ".join([os.path.basename(sys.argv[0])] + sys.argv[1:]))
    say("  project folder:    " + pathRoot)
    say("  notes folder:      " + os.path.join(pathRoot, c_sNotesFolder))
    say()


def run(lCommand):
    """Run a command, recording it and its result in the log."""
    say("  > " + " ".join(lCommand))
    try:
        oResult = subprocess.run(lCommand, cwd=pathRoot, capture_output=True,
                                 text=True, encoding="utf-8", errors="replace")
    except Exception as oError:
        say("    could not run it: " + str(oError))
        return None
    say("    exit code " + str(oResult.returncode))
    if oResult.returncode != 0:
        for sLine in (oResult.stderr or "").splitlines():
            say("    " + sLine)
    return oResult


def plural(iCount, sNoun):
    return str(iCount) + " " + sNoun + ("" if iCount == 1 else "s")


def sizeOf(pathItem):
    if os.path.isfile(pathItem):
        try:
            return os.path.getsize(pathItem)
        except OSError:
            return 0
    iTotal = 0
    for sDir, lDirs, lFiles in os.walk(pathItem):
        for sName in lFiles:
            try:
                iTotal += os.path.getsize(os.path.join(sDir, sName))
            except OSError:
                pass
    return iTotal


def sortRoot():
    """Work out what moves where.

    Returns the list of names to move, or None when the setup script cannot
    be read -- because then nothing can be judged, and guessing is how the
    last fault happened.
    """
    oInstalled = homerPolicy.installedFiles(pathRoot)
    if oInstalled is None:
        return None
    setTracked, setLocal = homerPolicy.repoFiles(pathRoot)
    if not setTracked:
        say("WARNING: RepoFiles.txt named no tracked file. Every build script")
        say("         would look like a stray, so nothing will be moved.")
        return None
    say("  setup script ships " + plural(len(oInstalled[0]), "named file")
        + ", " + plural(len(oInstalled[1]), "folder")
        + " and " + plural(len(oInstalled[2]), "pattern"))
    say("  RepoFiles.txt names " + plural(len(setTracked), "tracked file")
        + " and " + plural(len(setLocal), "local file"))
    say()

    setKeepFolders = set(s.lower() for s in c_lKeepFolders)
    lMove = []

    for sName in sorted(os.listdir(pathRoot)):
        pathItem = os.path.join(pathRoot, sName)
        sLower = sName.lower()
        if os.path.isdir(pathItem):
            if sLower in setKeepFolders:
                say("  belongs, folder:   " + sName)
                continue
            if homerPolicy.isNamedByInstaller(sName + "/", oInstalled):
                say("  belongs, folder:   " + sName)
                continue
            lMove.append(sName)
            say("  to notes, folder:  " + sName)
            continue
        if homerPolicy.belongsInFolder(sName, oInstalled, setTracked, setLocal):
            say("  belongs:           " + sName)
            continue
        lMove.append(sName)
        say("  to notes:          " + sName)
    say()
    return lMove


def addIgnoreEntry():
    """Make sure .gitignore holds the notes folder, once."""
    pathIgnore = os.path.join(pathRoot, ".gitignore")
    sExisting = ""
    if os.path.exists(pathIgnore):
        with open(pathIgnore, encoding="utf-8", errors="replace") as fileIgnore:
            sExisting = fileIgnore.read()
    lLines = sExisting.splitlines()
    lMissing = [s for s in (c_sNotesFolder + "/",) if s not in lLines]
    if not lMissing:
        say("  .gitignore already holds " + c_sNotesFolder + "/")
        return
    with open(pathIgnore, "a", encoding="utf-8", newline="\n") as fileIgnore:
        fileIgnore.write("\n# Swept aside by cleanFileDir: kept on disk, never part of the\n"
                         "# project, and gone through by hand when there is time.\n")
        for sLine in lMissing:
            fileIgnore.write(sLine + "\n")
    say("  added to .gitignore: " + ", ".join(lMissing))


def untrackLocalFiles():
    """Take out of the repository anything tracked that does not belong in it.

    Moving a file into notes stages its removal, because the path it was
    tracked under no longer exists. A file that STAYS at the root and should
    not be tracked is a different matter: build output, a generated source, a
    log, a working document. .gitignore does nothing for those, because
    .gitignore has no effect on a file that is already tracked -- adding a name
    to it after the fact changes nothing at all. Only git rm --cached takes it
    out, and the file stays on disk.
    """
    if not os.path.isdir(os.path.join(pathRoot, ".git")):
        say("  not a git working tree, so nothing was untracked")
        return
    oResult = run(["git", "ls-files"])
    if oResult is None or oResult.returncode != 0:
        say("  could not list tracked files, so nothing was untracked")
        return
    oInstalled = homerPolicy.installedFiles(pathRoot)
    if oInstalled is None:
        return
    setTracked, setLocal = homerPolicy.repoFiles(pathRoot)
    lPaths = [s for s in oResult.stdout.splitlines() if s.strip() and "/" not in s]
    lStray = [s for s in lPaths
              if not homerPolicy.isRepoFile(s, oInstalled, setTracked)]
    if not lStray:
        say("  every tracked file at the root belongs in the repository")
        return
    say("  " + plural(len(lStray), "tracked file") + " that the repository should not carry:")
    for sName in lStray:
        say("    " + sName)
    oResult = run(["git", "rm", "--cached", "--quiet", "--"] + lStray)
    if oResult is not None and oResult.returncode == 0:
        say("  untracked, and left on disk")
    else:
        say("  COULD NOT UNTRACK them; they are still in the repository")


def moveOne(sName, sFolder):
    """Move one item into a folder, reporting what happened."""
    pathFrom = os.path.join(pathRoot, sName)
    pathTo = os.path.join(pathRoot, sFolder, sName)
    if os.path.exists(pathTo):
        # Two files with one name are two files, so the newcomer is dated
        # rather than allowed to overwrite what an earlier run put there.
        sBase, sExt = os.path.splitext(sName)
        sStamp = datetime.datetime.now().strftime("%Y-%m-%d_%H%M%S")
        pathTo = os.path.join(pathRoot, sFolder, sBase + "_" + sStamp + sExt)
        say("  name already in " + sFolder + ", saving as " + os.path.basename(pathTo))
    try:
        shutil.move(pathFrom, pathTo)
    except Exception as oError:
        say("  COULD NOT MOVE " + sName + ": " + str(oError))
        return False
    say("  moved to " + sFolder + ": " + sName)
    return True


def main():
    oParser = argparse.ArgumentParser(
        description="Move everything the project does not claim into the notes folder.")
    oParser.add_argument("--survey", action="store_true",
                         help="print the plan and change nothing")
    oArguments = oParser.parse_args()

    startLog()
    say("=" * 68)
    say("SORTING")
    say("=" * 68)
    say()
    oSorted = sortRoot()
    if oSorted is None:
        say("FileDir_setup.iss is not here, so nothing can be judged.")
        say("Run this from C:\\FileDir.")
        say("The log is at " + pathLog)
        return 1
    lMove = oSorted

    say("=" * 68)
    say("PLAN")
    say("=" * 68)
    say()
    if not lMove:
        say("Nothing to move. The folder already holds FileDir and nothing else.")
        say("The log is at " + pathLog)
        return 0

    iBytes = sum(sizeOf(os.path.join(pathRoot, s)) for s in lMove)
    say(plural(len(lMove), "item") + " will move into " + c_sNotesFolder
        + "\\, " + format(iBytes, ",") + " bytes in all:")
    say()
    for sName in lMove:
        say("  " + sName)
    say()

    say("Everything stays on disk. The folder is added to .gitignore, so")
    say("nothing in it can join the repository by accident.")
    say()

    if oArguments.survey:
        say("This was a description only (--survey). Nothing has been changed.")
        say()
        say("Run it again without --survey to carry the plan out:")
        say()
        say("    cleanFileDir")
        say("The log is at " + pathLog)
        return 0

    say("=" * 68)
    say("DOING IT")
    say("=" * 68)
    say()
    iMoved = 0
    pathFolder = os.path.join(pathRoot, c_sNotesFolder)
    if not os.path.isdir(pathFolder):
        try:
            os.makedirs(pathFolder)
            say("  created " + pathFolder)
        except Exception as oError:
            say("  COULD NOT CREATE " + pathFolder + ": " + str(oError))
            say("The log is at " + pathLog)
            return 1
    for sName in lMove:
        if moveOne(sName, c_sNotesFolder):
            iMoved += 1
    addIgnoreEntry()
    say()

    # Whatever moved was probably tracked, so git is now holding paths that no
    # longer exist. Staging the removals here means the next commit is
    # coherent, and .gitignore keeps the new folders out from the start.
    if os.path.isdir(os.path.join(pathRoot, ".git")) and iMoved:
        run(["git", "add", "-A", "--", "."])
        say()

    untrackLocalFiles()

    say("=" * 68)
    say("AFTERWARDS")
    say("=" * 68)
    say()
    say(plural(iMoved, "item") + " moved into " + c_sNotesFolder + "\\.")
    lLeft = sortRoot()
    if lLeft:
        say("Still unaccounted for at the root: " + ", ".join(lLeft))
    else:
        say("Nothing unaccounted for remains at the root.")
    say()
    say("Nothing was deleted from disk. Run BuildFileDir to confirm the build")
    say("still works, then git commit and git push to record the changes.")
    say()
    say("The log is at " + pathLog)
    return 0


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
        say("cleanFileDir stopped on an unexpected error:")
        for sLine in traceback.format_exc().splitlines():
            say("  " + sLine)
        say("The log is at " + pathLog + ". Nothing further was attempted.")
        sys.exit(1)
    finally:
        if fileLog:
            fileLog.close()
