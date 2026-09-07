"""makeTutorial.py -- write the walkthrough section of Tutorials.md.

WHAT IT DOES

Reads Tutorial.inix, which holds one step to a section: the narration, the key
to press, what a screen reader says in answer, and a note for the reader. Writes
that out as Markdown, into Tutorials.md, between two markers.

WHY A GENERATED SECTION RATHER THAN A NEW DOCUMENT

Tutorials.md already ships, is already converted to HTML by the build, and is
already where somebody looks for a tutorial. A new document would need a line in
the installer script, a line in the repository list, and a place in every list of
documents. The walkthrough is a section of the tutorials, so it lives in the
tutorials.

Everything between the markers is replaced each time. Everything outside them is
left exactly as it was, so the hand-written tutorials and this one share a file
without either disturbing the other.

RUN IT WITH NO ARGUMENTS. The log is written beside this script.
"""

import datetime
import os
import platform
import sys
import traceback

c_sScript = os.path.abspath(__file__)
c_sHere = os.path.dirname(c_sScript)
c_sLog = os.path.join(c_sHere, "makeTutorial.log")
import glob

# EVERY SCRIPT IN THE FOLDER, in name order. Tutorial.inix is the first walk;
# Tutorial_Tagging.inix, Tutorial_Zipping.inix and the rest are one task each.
# Adding a tutorial means adding a file, and nothing else.
c_sPattern = os.path.join(c_sHere, "Tutorial*.inix")
c_sTarget = os.path.join(c_sHere, "Tutorials.md")

c_sStartMark = "<!-- walkthrough: written by makeTutorial.py, do not edit between the markers -->"
c_sEndMark = "<!-- walkthrough ends -->"


def note(sText):
    """One line to the log, and nothing to the screen unless it matters."""
    try:
        with open(c_sLog, "a", encoding="utf-8") as oFile:
            oFile.write(datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S  ") + sText + "\n")
    except Exception:
        pass


def say(sText):
    """A short plain sentence on the screen, and the same line in the log."""
    print(sText)
    note(sText)


def startLog():
    try:
        if os.path.isfile(c_sLog):
            os.remove(c_sLog)
    except Exception:
        pass
    note("makeTutorial " + datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S"))
    note("script: " + c_sScript)
    note("Python: " + platform.python_version())
    note("platform: " + sys.platform)
    note("working directory: " + os.getcwd())
    note("command line: " + " ".join(sys.argv))
    note("scripts: " + c_sPattern)
    note("target: " + c_sTarget)


def readFile(sPath):
    """Text of a file, whatever byte order mark it carries."""
    with open(sPath, "r", encoding="utf-8-sig") as oFile:
        return oFile.read().replace("\r\n", "\n")


def writeFile(sPath, sText):
    """Homer standard: UTF-8 with a byte order mark, and CRLF line endings."""
    with open(sPath, "wb") as oFile:
        oFile.write(b"\xef\xbb\xbf" + sText.replace("\n", "\r\n").encode("utf-8"))


def readSteps(sText):
    """The sections of Tutorial.inix, in order.

    A tolerant reader rather than a strict one: a key may repeat, and repeats
    are collected in order, which is how a step holds several lines of speech.
    """
    lsSections = []
    dNow = None
    for sRaw in sText.split("\n"):
        sLine = sRaw.strip()
        if len(sLine) == 0 or sLine.startswith(";") or sLine.startswith("#"):
            continue
        if sLine.startswith("[") and sLine.endswith("]"):
            dNow = {"_name": sLine[1:-1].strip().lower()}
            lsSections.append(dNow)
            continue
        if dNow is None or "=" not in sLine:
            continue
        sField, sValue = sLine.split("=", 1)
        sField = sField.strip()
        sValue = sValue.strip()
        if len(sValue) == 0:
            continue
        dNow.setdefault(sField, []).append(sValue)
    return lsSections


def firstOf(dSection, sField):
    lsValues = dSection.get(sField, [])
    return lsValues[0] if lsValues else ""


def buildMarkdown(lsSections, sNumber):
    """One tutorial, as Markdown, numbered as the caller says."""
    dAbout = {}
    lsSteps = []
    for dSection in lsSections:
        if dSection["_name"] == "about":
            dAbout = dSection
        elif dSection["_name"] == "step":
            lsSteps.append(dSection)

    lsOut = []
    lsOut.append("## " + sNumber + ". " + (firstOf(dAbout, "Title") or "A Walk Through FileDir"))
    lsOut.append("")
    sIntro = firstOf(dAbout, "Intro")
    if sIntro:
        lsOut.append(sIntro)
        lsOut.append("")
    sSetup = firstOf(dAbout, "Setup")
    if sSetup:
        lsOut.append("**Before you start:** " + sSetup)
        lsOut.append("")

    iNumber = 0
    for dStep in lsSteps:
        iNumber += 1
        sSay = firstOf(dStep, "Say")
        sKey = firstOf(dStep, "Key")
        lsHear = dStep.get("Hear", [])
        sNote = firstOf(dStep, "Note")

        lsOut.append("### Step " + str(iNumber) + (": " + sKey if sKey else ""))
        lsOut.append("")
        if sSay:
            lsOut.append(sSay)
            lsOut.append("")
        if lsHear:
            lsOut.append("You hear:")
            lsOut.append("")
            for sHeard in lsHear:
                lsOut.append("- " + sHeard)
            lsOut.append("")
        if sNote:
            lsOut.append(sNote)
            lsOut.append("")

    return "\n".join(lsOut).rstrip()


def spliceIntoTutorials(sDocument, sSection):
    """The walkthrough put back into Tutorials.md.

    Between the markers if they are there; after the contents list if they are
    not, which is where a first tutorial belongs.
    """
    iStart = sDocument.find(c_sStartMark)
    iEnd = sDocument.find(c_sEndMark)
    if iStart >= 0 and iEnd > iStart:
        return sDocument[:iStart] + sSection + sDocument[iEnd + len(c_sEndMark):]

    sAnchor = "\n## 1. "
    iAnchor = sDocument.find(sAnchor)
    if iAnchor < 0:
        note("no place found for the walkthrough; appending it")
        return sDocument.rstrip() + "\n\n" + sSection + "\n"
    return sDocument[:iAnchor] + "\n" + sSection + "\n" + sDocument[iAnchor:]


def addToContents(sDocument, sSection):
    """Contents lines for the generated tutorials, in front of the first one.

    Taken from the headings just written, so the list and the sections cannot
    disagree, and a tutorial added as a file needs nothing else done to it.
    """
    lsEntries = []
    for sLine in sSection.split("\n"):
        if not sLine.startswith("## "):
            continue
        sTitle = sLine[3:].strip()
        sAnchor = "#" + "".join(
            (ch.lower() if ch.isalnum() else ("-" if ch in " ." else ""))
            for ch in sTitle).strip("-")
        while "--" in sAnchor:
            sAnchor = sAnchor.replace("--", "-")
        lsEntries.append("- [" + sTitle + "](" + sAnchor + ")")
    if not lsEntries:
        return sDocument

    # Old generated lines go, so nothing accumulates across runs.
    lsKept = [sLine for sLine in sDocument.split("\n")
              if not (sLine.startswith("- [0") and "](#0" in sLine)]
    sDocument = "\n".join(lsKept)

    sFirst = "- [1. Your First Five Minutes]"
    iAt = sDocument.find(sFirst)
    if iAt < 0:
        note("no contents list found; leaving it alone")
        return sDocument
    return sDocument[:iAt] + "\n".join(lsEntries) + "\n" + sDocument[iAt:]


def main():
    startLog()
    lsSources = sorted(glob.glob(c_sPattern))
    note("scripts found: " + str(len(lsSources)) + " -- " + ", ".join(os.path.basename(s) for s in lsSources))
    if len(lsSources) == 0:
        say("0 tutorial scripts here, so there is nothing to write.")
        return 1
    if not os.path.isfile(c_sTarget):
        say("Tutorials.md is not here, so there is nowhere to write.")
        note("missing target: " + c_sTarget)
        return 1

    lsBlocks = []
    iStepsAll = 0
    iLetter = 0
    for sSource in lsSources:
        try:
            lsSections = readSteps(readFile(sSource))
        except Exception as oError:
            say(os.path.basename(sSource) + " could not be read.")
            note("read failed: " + str(oError))
            note(traceback.format_exc())
            return 1
        iSteps = len([d for d in lsSections if d["_name"] == "step"])
        note(os.path.basename(sSource) + ": sections " + str(len(lsSections)) + ", steps " + str(iSteps))
        if iSteps == 0:
            say(os.path.basename(sSource) + " holds 0 steps.")
            return 1
        iStepsAll += iSteps
        # The first walk is section 0; the task tutorials are 0a, 0b and so on,
        # which keeps them together at the front without renumbering anything
        # that was already written.
        sNumber = "0" if len(lsBlocks) == 0 else "0" + chr(ord("a") + iLetter)
        if len(lsBlocks) > 0:
            iLetter += 1
        lsBlocks.append(buildMarkdown(lsSections, sNumber))

    try:
        sSection = c_sStartMark + "\n\n" + "\n\n".join(lsBlocks) + "\n\n" + c_sEndMark
        sDocument = readFile(c_sTarget)
        sDocument = addToContents(spliceIntoTutorials(sDocument, sSection), sSection)
        writeFile(c_sTarget, sDocument)
    except Exception as oError:
        say("Tutorials.md could not be written.")
        note("write failed: " + str(oError))
        note(traceback.format_exc())
        return 1

    say("Wrote " + str(len(lsSources)) + " tutorials into Tutorials.md: " + str(iStepsAll) + " steps.")
    note("finished")
    return 0


if __name__ == "__main__":
    sys.exit(main())
