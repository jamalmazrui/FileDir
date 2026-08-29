# FileDir

**Version 5.0.42**  
August 2026  
Copyright 2006-2026 by Jamal Mazrui  
MIT License

FileDir manages files and folders on Windows from the keyboard. It does what
File Explorer does, and a good deal more, but almost every command is one
mnemonic keystroke, and it says what it is doing as it goes. It is written by a
blind developer for people who work by keyboard and screen reader.

## Contents

- [What FileDir Gives You](#what-filedir-gives-you)
- [Installing](#installing)
- [Quick Start](#quick-start)
- [The Other Documents](#the-other-documents)
- [Getting Help](#getting-help)

## What FileDir Gives You

- **A folder is a list.** Up and down arrows move through it, folders first,
  then files. Home and End go to the ends. Typing letters jumps to a name.
- **Tags.** You mark any set of items and then act on all of them at once.
  Copy, move, delete, zip, print, mail, convert to text: each works on the
  tagged items, or on the current item when nothing is tagged.
- **Questions you can ask.** One key each for the name, the size, the date, the
  type, the full path, how far through the list you are, what the clipboard
  holds, and what is inside the current file.
- **Two windows are two folders.** Open a second view, work between them, and
  switch with one key.
- **Zip and other archives.** Look inside an archive as if it were a folder.
  Extract from almost any format. Create and update zip files.
- **Speech that adds rather than repeats.** FileDir speaks through JAWS, then
  NVDA, then a Windows notification that Narrator reads. It says only what your
  screen reader does not already say.
- **Conversion between formats, in batches.** Tag files, press Shift+O, pick
  what they should become. Documents, audio, video and pictures all convert,
  and PDFs keep their headings, lists and tables. No Microsoft Office needed.
- **Naming files by what is inside them.** Control+Shift+I renames to the title,
  caption or song name held in the file itself.
- **Finding duplicates anywhere below a folder**, compared byte for byte and
  gathered into a window you can inspect and delete from.
- **Playing media**, from the folder, the tagged files, or a play list you
  copied — including lists of web addresses.
- **Everything a file knows about itself** in one alphabetical list: Windows
  properties, what opens it, and the metadata inside it.
- **Translation and questions, on your own computer.** Translate whole folders,
  or ask an AI about a file. Nothing is sent anywhere.
- **A log of every session**, and one key to put it on your clipboard.


## Installing

Download **FileDir_setup.exe** from the
[FileDir releases page](https://github.com/JamalMazrui/FileDir/releases/latest)
and run it.

You need Windows 10 or Windows 11. The .NET Framework 4.8 that FileDir needs is
already part of both.

The installer puts one shortcut on your desktop and gives it the hot key
**Alt+Control+F**. Press those three keys anywhere in Windows to start FileDir,
or to switch to it if it is already open.

At the end you are offered a list of checkboxes. The first four are about
FileDir itself: install the JAWS scripts, install the NVDA add-on, start FileDir,
and open the guide. The first three are ticked already; untick the screen reader
boxes if they do not apply to you.

Below them are the optional components. **Pandoc is ticked**, because it is what
gives FileDir its conversions: about 100 MB, installed machine wide in
`C:\Program Files\Pandoc` and shared with EdSharp and HomerScribe, so one
download serves all three.

The two for translation are **not ticked**:

- **Ollama with the llama3.2 chat model**, about 2 GB. This is what makes
  Alt+Shift+F7 work at all.
- **The qwen2.5:7b model**, about 5 GB, which translates noticeably better.

Nobody should download several gigabytes by not noticing a checkbox, so you have
to ask for these. Each label tells you whether the box will install, update or
reinstall, and says the size, so there is no guessing. If you already have
Ollama — for EdSharp or DbDo, say — FileDir finds it and offers a reinstall
rather than a second copy. One installation and one set of models serve every
program on the machine.

You can add them later instead: run `installOllama.cmd` or
`installTranslateModel.cmd` in the FileDir folder.

When setup finishes, one Results box tells you how every checkbox fared and
where the log is. Nothing else pauses along the way.

When a new version comes out you do not have to download it by hand. Press
**F11** inside FileDir for the Elevate Version command, which checks GitHub,
tells you what is new, and offers to install it.

## Quick Start

Try these in order. Each is one keystroke.

1. Press **Alt+Control+F**. FileDir opens on a folder, and the title bar gives
   its path.
2. **Arrow up and down** the list. Folders come first, then files.
3. Press **Apostrophe** to hear the name of the item you are on, **Shift+S** for
   its size, **Shift+D** for its date, and **Alt+P** for its full path.
4. Press **Enter** on a folder to open it in a new window, or **Backspace** to
   open the folder above. Hold **Shift** with either to reuse the current window
   instead of opening another.
5. Press **Question Mark** to hear what is inside the current file. This works
   for Word, PDF, and other document formats, not only plain text.
6. Press **Space** on two or three files to tag them, then **Shift+L** to hear
   what you have tagged and **Shift+Y** for how many they are and how big.
7. With those files still tagged, press **Shift+Z** to zip them, or **Shift+C**
   to copy them to another folder.
8. Press **Alt+Shift+F7** to translate them into a language you name, if you
   installed Ollama. FileDir writes a translation beside each file.
9. Press **Control+F1** to turn on Key Describer. Now pressing a command key
   says its name, its key, and what it does, instead of running it. Press
   **Control+F1** again to turn it off. This is the fastest way to explore.
10. Press **Alt+F10** for the Alternate Menu, an alphabetical list of every
   command. Type a few letters to find one, and press Enter to run it.
11. Press **F1** for the full guide.

## The Other Documents

Each of these comes with the program, in both Markdown and web page form, and
is on the Start menu:

- **FileDir** is the complete guide: every command group explained, with the
  keys in context.
- **Tutorials** walks through nine real jobs from start to finish.
- **Questions and Answers** gives short answers to what people ask most.
- **Hotkeys** lists every command three ways: by name, by key, and grouped by
  modifier.
- **History** records what changed in each version.
- **Developer** explains how to rebuild or modify FileDir.
- **License** gives the MIT terms.

## Getting Help

The project home, including every release and the source code, is
[FileDir on GitHub](https://github.com/JamalMazrui/FileDir). Problem reports are
welcome there, and the more detail the better, especially the steps that lead to
the problem.

End of Document
