# FileDir — Tutorials

**Version 5.0.27**  
August 2026  
Copyright 2006-2026 by Jamal Mazrui  
MIT License

Nine short walkthroughs, each a real job done from start to finish. Work
through the first one and the rest will make sense; after that, take whichever
matches what you need today.

Every key is named in full the first time it appears in a tutorial. If you
forget one, press Control+F1 for Key Describer and press the key: it says the
command's name and what it does instead of running it.

## Contents

- [1. Your First Five Minutes](#1-your-first-five-minutes)
- [2. Finding Something in a Crowded Folder](#2-finding-something-in-a-crowded-folder)
- [3. Tagging, and Why It Changes Everything](#3-tagging-and-why-it-changes-everything)
- [4. Copying and Moving Between Two Folders](#4-copying-and-moving-between-two-folders)
- [5. Reading a Document Without Opening It](#5-reading-a-document-without-opening-it)
- [6. Zipping and Unzipping](#6-zipping-and-unzipping)
- [6a. Converting a Folder to Another Format](#6a-converting-a-folder-to-another-format)
- [7. Translating a Folder of Files](#7-translating-a-folder-of-files)
- [8. Renaming a Batch of Files](#8-renaming-a-batch-of-files)
- [9. Making FileDir Yours](#9-making-filedir-yours)

## 1. Your First Five Minutes

**The job: get oriented.**

1. Press **Alt+Control+F**. FileDir opens on a folder, and the title bar gives
   its path.
2. **Arrow up and down** the list. Folders come first, then files, each sorted
   by name.
3. Press **Apostrophe** to hear the name of the item you are on. This is the
   key you will press most.
4. Press **Shift+S** for its size, **Shift+D** for its date, **Shift+T** for its
   type, and **Alt+P** for the full path.
5. Press **Shift+P** to hear how far down the list you are: item so many of so
   many.
6. Press **Enter** on a folder. A new window opens on it, and the first window
   is still there behind it.
7. Press **Backspace** to open the folder above. Notice that this opened another
   window too.
8. Press **Control+F6** to move between the windows you have open, and
   **Control+F4** to close the one you are in.
9. Press **Shift+Backspace**. This time the same window moves up instead of
   opening a new one. That pairing is everywhere in FileDir: the plain key
   opens, and Shift reuses.
10. Press **F1** for the guide when you want the whole picture.

**What you learned:** where you are, what you are on, and the difference between
opening and going.

## 2. Finding Something in a Crowded Folder

**The job: get to one file among hundreds.**

1. Open a folder with a lot in it — your Downloads folder will do.
2. **Type the first few letters** of the name. FileDir moves as you type.
3. Press **Control+J** for Jump. Type any part of a name and press Enter, and
   FileDir goes to the next item containing it. Not just the start: any part.
4. Press **Control+J** again and press Enter with the box empty to repeat the
   last jump.
5. Try jumping to a **backslash**. That is the folder symbol, so it takes you to
   the next folder. The attribute symbols all work this way: a right parenthesis
   for hidden, a right bracket for read only, a right brace for system.
6. Press **Control+F** for Filter. Type a word and the list narrows to items
   containing it. Clear the filter to bring the rest back.
7. Press **Shift+F** to search inside files rather than in names, when what you
   remember is a phrase in a document rather than its name.

**What you learned:** four different ways to find something, each quicker than
scrolling.

## 3. Tagging, and Why It Changes Everything

**The job: act on many files at once.**

Tagging is the idea the rest of FileDir is built on. A command works on what you
tagged; with nothing tagged, it works on the item you are on. So you never have
to tag, and when you do, one command does the work of twenty.

1. Move to a file and press **Space**. It is now tagged. Press Space again to
   untag it.
2. Tag three or four files this way.
3. Press **Shift+L** to hear the list of what you have tagged.
4. Press **Shift+Y** to hear how many they are and how much they come to
   altogether.
5. Press **Greater Than** on an untagged file: it tags and moves down, which is
   quicker when you are tagging a run by hand. **Less Than** untags and moves
   down.
6. Now a range. Move to where a run starts and press **F8**. Move to where it
   ends and press **Shift+F8**. Everything between is tagged.
7. Press **Alt+Shift+F8** to untag a range the same way.
8. Press **Slash** to untag everything, and **Semicolon** to tag everything.
9. Press **Alt+I** — Invert Tagged — to swap what is tagged for what is not.
   Tagging the three files you do not want and inverting is often quicker than
   tagging the forty you do.
10. Press **Control+S** to save your tags, so you can come back to the same set
    later.

**What you learned:** the whole tag vocabulary. Every command from here on uses
it.

## 4. Copying and Moving Between Two Folders

**The job: get files from one place to another.**

1. Open the folder you are copying **from**.
2. Press **Control+O** to open the folder you are copying **to** in a second
   window. Type or paste its path.
3. Press **Control+F6** to go back to the first window.
4. **Tag** the files you want, as in tutorial 3.
5. Press **Shift+C** for Copy Tagged. FileDir asks where to. The other open
   window is offered, so you usually just press Enter.
6. Press **Control+F6** to look at the destination and confirm they arrived.
7. Press **Shift+M** instead of Shift+C to move rather than copy.
8. Press **Alt+P** on a file and then paste elsewhere: FileDir put the full path
   on the clipboard, which is often what you actually wanted.

**What you learned:** two windows are two folders, and tagging plus one key
moves any number of files.

## 5. Reading a Document Without Opening It

**The job: find out what is in a file without waiting for Word.**

1. Move to a document — a .docx, a .pdf, a .md, or a plain text file.
2. Press **Question Mark**. FileDir converts the file to text and reads it out.
   Word and PowerPoint files need Office installed; PDF and Markdown do not.
3. Press **Shift+O** — Output to Text — on a tagged set to write each one out as
   a .txt file you can read or search.
4. Press **Shift+A** — Append Tagged — to put the text of every tagged file on
   the clipboard, one after another. Useful for gathering notes scattered across
   a folder.
5. Press **E** to open the file in your text editor when you want to change it
   rather than read it.

**What you learned:** the fastest way to answer "what is this file?"

## 6. Zipping and Unzipping

**The job: pack files up and take them apart.**

1. **Tag** the files you want to pack.
2. Press **Shift+Z** for Zip Tagged. FileDir asks for a name and makes the
   archive in the current folder.
3. Move to the new archive and press **Enter**. You are now looking inside it as
   though it were a folder.
4. Press **Backspace** to come out again.
5. Tag one or more archives and press **Shift+X** — Unarchive Tagged — to
   extract them. FileDir handles nearly every archive format, not only zip.
6. Press **Alt+Shift+X** to extract without recreating the folders inside, when
   you want everything in one place.
7. Press **Control+Shift+X** to test an archive without extracting anything.

**What you learned:** archives are just folders, and packing is one key.

## 6a. Converting a Folder to Another Format

**The job: turn a folder of Word documents into Markdown, or anything into
anything.**

This needs Pandoc, which the FileDir installer ticks by default. If you unticked
it, run `installPandoc.cmd` in the FileDir folder as an administrator.

1. Open the folder and **tag** the files to convert.
2. Press **Alt+Shift+K** for Convert Format.
3. A list of ten formats appears, each with its extension named. **Arrow to the
   one you want** and press Enter. FileDir remembers your choice for next time.
4. Listen. FileDir says which file it is on and how many there are.
5. A file Pandoc cannot read — a legacy .doc, or a PDF — is skipped, and FileDir
   says so by name. Use **Shift+O**, Output to Text, for those instead.
6. At the end you hear how many were converted, how many skipped, and how many
   failed. The folder refreshes so the new files are there.
7. Press **Question Mark** on one to hear the result.

**What you learned:** batch conversion between any two formats Pandoc handles,
and what to do about the ones it does not.

## 7. Translating a Folder of Files

**The job: read documents in a language you do not speak, without sending them
anywhere.**

This needs Ollama, which the FileDir installer offers as a checkbox. If you did
not tick it, run `installOllama.cmd` in the FileDir folder, or install FileDir
again and tick the box. If you already installed Ollama for EdSharp or DbDo, it
is already here — one installation serves them all.

1. Open the folder holding the documents.
2. **Tag** the ones to translate. One is fine; twenty is the point.
3. Press **Alt+Shift+L** for Translate File.
4. Type the language you want — Spanish, French, Arabic, whatever it is — and
   press Enter. FileDir remembers your answer for next time.
5. Listen. FileDir says which file it is on and which part of it, because a
   model takes its time and silence is hard to tell from a fault.
6. When it finishes, the folder refreshes and you will see a new file beside
   each original, named `<name>.<language>.txt`. Nothing was overwritten.
7. Press **Question Mark** on one to hear the translation.

**Two things worth knowing.** The translation happens on your own computer:
nothing is uploaded, which is why this is safe for something private. And if you
install the larger qwen2.5:7b model, FileDir uses it automatically and the
translations are noticeably better; you do not configure anything.

**What you learned:** batch translation of a whole folder, entirely offline.

## 8. Renaming a Batch of Files

**The job: fix forty file names at once.**

1. **Tag** the files to rename.
2. Press **Control+R** for Rename Tagged.
3. Give the pattern you want. FileDir shows you what each name will become
   before anything changes.
4. Read the list, and press Enter to go ahead or Escape to think again.
5. Press **Control+N** to make a new folder first if the renamed files should
   live somewhere of their own.

**What you learned:** renaming is a review-then-commit operation, so a mistake
costs you nothing.

## 9. Making FileDir Yours

**The job: change the things that annoy you.**

1. Press **Control+F1** and wander the keys for a while. Key Describer is the
   fastest way to find out what is there.
2. Press **Alt+F10** for the Alternate Menu and read the whole command list
   alphabetically. Filter it with Control+F.
3. Press **Alt+Shift+H** to open the hotkey document, which lists every command
   three ways: by name, by key, and grouped by the modifier a key starts with.
4. Press **Control+F2** for Configuration Options and set what you want changed
   for good.
5. Press **Alt+F2** for Manual Options to change something for this session
   only.
6. Press **Control+Shift+X** — Extra Speech Toggle — if FileDir says more than
   you want. Press it again to bring the extra messages back.
7. Press **F11** now and then for Elevate Version, which checks whether a newer
   FileDir has been released.

**What you learned:** where the settings are, and how to explore the rest on
your own.

End of Document
