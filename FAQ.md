# FileDir — Questions and Answers

**Version 5.0.76**  
August 2026  
Copyright 2006-2026 by Jamal Mazrui  
MIT License

Short answers to the things people ask most. The full explanations are in
[the user guide](FileDir.htm), and the worked examples are in
[Tutorials](Tutorials.htm).

## Contents

- [Getting Started](#getting-started)
- [Working With Files](#working-with-files)
- [Converting Between Formats](#converting-between-formats)
- [Screen Readers](#screen-readers)
- [Translating Files](#translating-files)
- [Updates and Problems](#updates-and-problems)

## Getting Started

**What is FileDir?**

A file and folder manager for Windows that you drive from the keyboard. It does
what File Explorer does, and more, but nearly every command is one keystroke,
and it tells you what it is doing as it goes.

**How do I start it?**

Press Alt+Control+F. The installer puts one shortcut on your desktop and gives
it that hot key. If FileDir is already open, the same keys switch to it rather
than starting a second copy.

**Alt+Control+F is already used by something else. Can I change it?**

Yes. Move to the FileDir item on your desktop, press Alt+Enter for properties,
and set a different key, or clear it. Only that one shortcut carries the hot
key, so there is one place to change and nothing else to hunt down.

**Do I have to learn all the keys?**

No. Two commands teach you the rest. Press Control+F1 for Key Describer, and
then any command key says its name, its key, and what it does instead of running
it; press Control+F1 again to turn it off. Press Alt+F10 for the Alternate Menu,
which lists every command in one alphabetical list you can filter and search.

## Working With Files

**What is a tag?**

A mark you put on an item so a command acts on it. Press Space to tag or untag
the item you are on. Then a command like Copy, Zip, or Translate works on
everything you tagged. With nothing tagged, a command works on the item you are
on, so tagging is never something you have to do first.

**How do I tag a run of files?**

Press F8 where the run starts, move to where it ends, and press Shift+F8. Use
Alt+Shift+F8 to untag a range the same way.

**How do I see everything about a file?**

Press Control+Shift+T for Type Extended. You get one alphabetical list of every
field and value: the Windows properties, the file association details, and the
metadata inside the file — camera and exposure for a photograph, artist and
album for a song, duration and codecs for a video, author and page count for a
PDF. ExifTool comes with FileDir and does that last part.

**What do the symbols in a name mean?**

They are attributes. A right parenthesis means hidden, a right bracket means
read only, a right brace means system, and a backslash means the item is a
folder. You can search on those same symbols: Jump accepts them as the text to
find, so typing a backslash finds the next folder.

**What is the difference between Open and Go to?**

Open makes a new window and leaves the current one alone, with its tags intact.
Go to reuses the window you are in and discards it. Enter opens; hold Shift to
go instead.

**Can FileDir look inside a zip file?**

Yes. Press Enter on an archive and its contents appear as though they were a
folder. FileDir extracts from nearly any archive format and creates and updates
zip files.

**How do I hear what is inside a file?**

Press Question Mark. This works for Word, PDF, PowerPoint, Excel and Markdown
files as well as plain text, because FileDir converts them first with 2htm,
which comes with it.

## Screen Readers

**Which screen readers work with FileDir?**

JAWS, NVDA, and Narrator. FileDir speaks only what your screen reader does not
already say: it never repeats a window title or the name of the control you are
on, because your screen reader announces those itself.

**Do I need the JAWS scripts?**

They help. Mostly they stop JAWS reading the name of a keystroke out loud, such
as "Shift S", so you hear just the command name, "Size". The installer offers
them and ticks the box when JAWS is on the computer.

**How does FileDir talk to NVDA?**

Directly, when `nvdaControllerClient.dll` is in the FileDir folder. Without it,
FileDir speaks through a Windows notification, which NVDA reads anyway. The
direct route is a little quicker; nothing is lost without it.

## Converting Between Formats

**How do I convert a Word document to Markdown, or the other way round?**

Tag the files, press Alt+Shift+K, and pick the format you want. FileDir writes
the converted copy beside each original and never overwrites anything.

**Which formats can it read?**

Word (.docx), OpenDocument, EPUB, HTML, Markdown, reStructuredText, LaTeX, rich
text, AsciiDoc, CSV, MediaWiki, PowerPoint (.pptx) and Excel (.xlsx), among
others. It cannot read legacy .doc, .ppt or .xls, and it cannot read PDF. Use
Shift+O, Output to Text, for those: that goes through 2htm, which comes with
FileDir.

**How are PDFs read?**

By PyMuPDF4LLM, a free reader the installer offers as a ticked checkbox. It
reads the PDF's own structure, so you get headings, lists and tables rather than
a wall of text. Microsoft Word is not involved.

**Does the "LLM" in that name mean AI reads my PDFs?**

No. Nothing is sent anywhere and no AI model is involved. The name means the
tool was built to produce Markdown that reads well when handed to an AI later —
it describes the output format, not how the work is done. The reading itself is
ordinary parsing: font sizes become headings, ruled areas become tables.

**Does FileDir install Python?**

Only if you tick the PDF reader and this computer has no Python. The reader runs
under Python, so the installer fetches it — about 30 MB — the same way it
fetches everything else. FileDir itself is a Windows program and needs no
Python for anything else.

A PDF that is a scan of images has no text in it at all, and needs optical
character recognition rather than conversion. FileDir says so rather than going
quiet.

**Can I convert iPhone photos?**

Yes, with the image tools installed — the installer offers them as a checkbox,
unticked. iPhone photos are HEIC, which ffmpeg cannot read whatever build you
have, so ImageMagick handles those along with camera raw files, SVG drawings and
Windows icons. Ordinary PNG and JPEG work without it.

**Do I need Pandoc?**

Yes, for Convert Format. The installer offers it as a checkbox and ticks it,
because it is what gives FileDir its conversions.

**Where does Pandoc go?**

`C:\Program Files\Pandoc`, machine wide. It is about 100 MB and EdSharp and
HomerScribe use the same copy, so one download serves all three rather than
each program carrying its own.

**I already have Pandoc. Will it install a second one?**

No. FileDir checks first, and the checkbox then says Update or Reinstall with
the version, rather than Install.

**Can I play a list of YouTube addresses?**

Yes. Copy the list, press Alt+Shift+L, and it plays in order with the track
titles. Anything mpv can reach works, because the addresses are handed to
yt-dlp. An address that has been taken down is skipped rather than stopping the
rest.

**I copied a podcast directory and nothing played. Why?**

Because a directory usually links to each episode's web page, not to its audio
file. The player has to fetch and examine every page before it can play
anything, which takes seconds each and fails completely for sites it does not
recognise. FileDir now warns you before starting in that case.

What plays immediately is an address ending in .mp3 or .m4a. A podcast's RSS
feed holds those; its web pages do not. If you can get the feed's audio
addresses onto the clipboard, Alt+Shift+L plays them at once.

**What is the difference between Play list and Play queue?**

Play list, Control+Shift+L, hands everything to mpv and steps out of the way:
mpv's own window comes to the front and its keys work. Play queue,
Control+Shift+Q, keeps the list inside FileDir in the Homer Player, where every
control has an Alt key and the player itself has no window at all. The sources
are the same either way -- tagged files, a play list, or the media links in the
document you are on.

**Does the Homer Player remember anything?**

Each play list keeps its own speed, volume, jump size and order, written as you
change them, and where each track had reached when you closed it. Playing the
same thing again starts where you stopped, in the player or in mpv's own window.
Default settings, Alt+D, forgets all of that for the queue you are on.

**Can I save part of a track?**

Yes. Play to where it should begin and press F8, play to where it should end and
press Shift+F8, then Alt+C. FileDir writes that piece as a media file beside the
track it came from and puts the file on the clipboard, ready to paste into a
folder or a message.

**Can I add something while it is playing?**

Yes. Copy an address and press Control+V in the player window. That is mpv's own
key: it appends one file or address to what is already queued. For a whole list,
come back to FileDir and press Alt+Shift+L again.

## Translating Files

**How does FileDir translate a file?**

Press Alt+Shift+F7, name the language, and FileDir reads the text of every tagged
file, translates it, and writes the result beside the original as
`<name>.<language>.txt`. Nothing is overwritten.

**Where does the translation happen?**

On your own computer. FileDir talks to Ollama, which runs a language model
locally and listens only on this machine. No file, and no part of a file, is
sent anywhere. That is why this is worth having rather than pasting into a web
page: you can translate something private.

**Do I have to install anything?**

Yes, and it is optional. The FileDir installer offers Ollama as a checkbox, with
the llama3.2 model, about 2 GB. A second checkbox offers qwen2.5:7b, about 5 GB,
which translates noticeably better. Neither box is ticked to begin with, because
nobody should download gigabytes by not noticing a checkbox.

**I already installed Ollama for EdSharp. Do I need it twice?**

No. One Ollama installation and one set of models serve every program on the
machine. FileDir checks what is already there and offers a reinstall rather than
a second copy.

**Which model does it use?**

qwen2.5:7b if you have it, llama3.2 otherwise. You do not configure this;
FileDir asks Ollama what is installed and picks the better one.

**Can I ask questions about a file?**

Two ways. Press F12 for Chat with AI to ask a plain question, or Shift+F12 for
Chat about File to ask about the file you are on, with its text sent along. It
reads Word, PDF and the rest, not just plain text. The same keys do the same
things in EdSharp.

The answer opens in a box you can arrow through, select from, and copy with
Control+C. Spacebar, Enter or Escape closes it.

**Where did the timer go?**

To Alt+Control+T to start, Alt+Control+S to stop, and Alt+Control+Y to hear the
elapsed time. It moved off F12 so that FileDir and EdSharp agree about which
keys ask the AI.

**How long does it take?**

Longer than you expect the first time, because the model has to load. FileDir
says which file it is on and which part of it, so you can tell it is working
rather than stuck.

**Can I translate a whole folder at once?**

That is what it is for. Tag as many files as you like and answer the language
question once.

## Updates and Problems

**How do I get a new version?**

Press F11 for Elevate Version. FileDir asks GitHub what the newest release is,
tells you what you have and what is available, and offers to download and run
the installer. If that fails, download `FileDir_setup.exe` yourself from the
[FileDir releases page](https://github.com/JamalMazrui/FileDir/releases/latest).

**Can I install a new version over an old one?**

Yes. Your settings are kept.

**Where are the logs if something goes wrong?**

In your local application data folder, under FileDir and then logs. FileDir
writes one log per session, named for the date and time, and the installer
writes `FileDir_setup.log` in the same place. The Results box at the end of the
installation names it.

**How do I send you the log?**

Press Control+F12 for Copy Log, then paste into a new mail message. The log file
itself is attached, because the path goes on the clipboard as a file as well as
as text. The newest thirty logs are kept, so the one you want is there.

**Something is wrong. Where do I report it?**

At [FileDir issues on GitHub](https://github.com/JamalMazrui/FileDir/issues).
If FileDir shows you an unexpected event, the Report a Problem button puts the
whole message on the clipboard and opens that page ready to paste into.

End of Document
