# FileDir — User Guide

**Version 5.0.54**  
August 2026  
Copyright 2006-2026 by Jamal Mazrui  
MIT License

## Contents

- [Installation](#installation)
- [Introduction](#introduction)
- [Edit Commands](#edit-commands)
- [Find Commands](#find-commands)
- [Go to Commands](#go-to-commands)
- [Navigate Commands](#navigate-commands)
- [Query Commands](#query-commands)
- [Tag Commands](#tag-commands)
- [Transfer Commands](#transfer-commands)
- [Miscellaneous Commands](#miscellaneous-commands)
- [What Is Inside a File](#what-is-inside-a-file)
- [Renaming to What Is Inside](#renaming-to-what-is-inside)
- [The Quick Folder](#the-quick-folder)
- [Going Back](#going-back)
- [Tidying a Folder](#tidying-a-folder)
- [Converting Between Formats](#converting-between-formats)
- [Playing Media](#playing-media)
- [Downloading From the Web](#downloading-from-the-web)
- [Asking About a File](#asking-about-a-file)
- [Translating Files](#translating-files)
- [Hotkey Summary](#hotkey-summary)
- [Logs](#logs)
- [Development Notes](#development-notes)

## Installation

FileDir is installed by a program called FileDir_setup.exe. Get it from the
[FileDir releases page](https://github.com/JamalMazrui/FileDir/releases/latest).
Run it and answer the questions. The default program folder is

```
C:\Program Files\FileDir
```

FileDir needs the .NET Framework 4.8, which is already part of Windows 10 and
Windows 11. It runs as a 64-bit program on both Intel and ARM machines.

The installer puts a FileDir group on the Start menu. From there you can start
FileDir, read the guide, look at the hotkeys, see the licence, turn the link
between FileDir and folders or .zip files on or off, and uninstall.

It also puts one FileDir shortcut on your desktop, and gives that shortcut the
hot key Alt+Control+F. Press those three keys anywhere in Windows to start
FileDir, or to switch to it if it is already running. Only that one shortcut
carries the hot key, so nothing else competes for it. If Alt+Control+F clashes
with something else you use, move to the FileDir item on the desktop, press
Alt+Enter for properties, and set a different key, or clear it.

At the end of the install you are offered four checkboxes. The first three are
already ticked:

- **Install scripts for the JAWS screen reader.** These fine tune what JAWS
  says. Mainly they stop JAWS reading the name of a keystroke out loud, such as
  "Shift S", so you hear just the command name, "Size". Untick this box if you
  use a different screen reader.
- **Install the add-on for the NVDA screen reader.** This box only appears when
  an add-on is included in the release.
- **Start FileDir.**
- **Open the user guide.** This is the one box that is not ticked to begin with.

You can install a new version straight over an older one. Press Alt+F1 for the
About box, which gives the version number and release date. Press Shift+F1 for
the change history.

You do not have to download a new version by hand. Press F11 for the Elevate
Version command, which asks the FileDir project on GitHub what the newest
release is, tells you what you have and what is available, and offers to
download and run the installer for you.

## Introduction

FileDir is a file and directory manager developed in the C# language, which requires the .NET Framework 4.8 to run. Current versions of Windows include it; on older systems it is a free Microsoft download from [Microsoft .NET Framework download](https://dotnet.microsoft.com/download/dotnet-framework)

FileDir is designed to be a powerful, efficient, and convenient alternative to File Explorer for managing files and folders on a Windows-based computer. Almost every command can be done through a mnemonic keystroke, as well as a menu or mouse operation. These commands begin with those available in Windows Explorer. FileDir then adds several beneficial features above this base command set. Finally, a set of optional JAWS scripts provide further fine tuning of the speech interface.

Since the number of possible FileDir commands is large, involving nearly every letter and several punctuation keys, some organizing concepts, explained below, help the learning curve.

The standard environment of FileDir is a view of items in a particular folder of the computer's file system. The title of the window says "FileDir" followed by the path and name of the folder being viewed. Below the title bar is a list of items contained in the folder, one item per line, folder items first, then file items after. The date and time of the current item, its size, the sort order, and filter specification (if any) are displayed on the status line. This may be reviewed with Alt+Z or a screen reader-specific command such as Insert+PageDown in JAWS. Note that the size of a folder item will be -1 until a Size or Yield query causes it to be calculated (by recursively summing the sizes of all contained files and subfolders). On the status line, the size is expressed in an abbreviated manner using K for kilobytes, M for megabytes, or G for gigabytes.

At any time, a single file or folder item has keyboard focus, which may be called the current item -- the location of the PC cursor in Jaws terminology. At any time, zero or more items may be tagged, that is, marked in a way that makes them available for further action by commands that can act on multiple items at once. Note that the focused or current item may not be tagged: focus and tagged state are independent. To remind you that a command may affect multiple, tagged files, the word "tagged" is included as part of its menu name. Conversely, command names without this word apply regardless of tagged state, e.g., to the focused item only, or independent of focus and tagged state.

FileDir implements the list of items as a standard Windows ListBox control. This means, for example, that the up and down arrow keys navigate through the list, and the Home and End keys move focus to the top and bottom of the list, respectively.

Navigation by initial character is another ListBox behavior. Pressing the letter "b," for example, will move focus to the next file or folder item with a name starting with that letter. Pressing multiple letters quickly, however, does not move to the next item starting with that sequence.

Since initial letter navigation works the same whether lower or upper case letters are used, the upper case or shifted versions are used by FileDir to add features through hot keys. For example, the Shift+C hot key lets you copy files to another folder without using the more involved "copy and paste" method of Windows Explorer (though similar functionality is also supported by FileDir). Since you can type a lower case "c" to jump to a file that starts with either the lower or upper case form of that letter, FileDir's use of Shift+C means an extra capability without loss of functionality.

This command needs more information to complete the copy operation, so it prompts for a target folder. It remembers the previous input, if any, which can be accepted by simply pressing Enter. To provide another value instead, type it, replacing other text, and press Enter to activate the default, OK button. You can also pick a folder from a standard tree control by activating the Browse button. Other buttons let you pick a folder from one of three listboxes: directories open in current windows, those opened during this FileDir session, or those with shortcuts in the Quick folder.

If no items are tagged, the command assumes you want to process the current, focused item. With one ore more tagged items, the command performs a batch operation. If you want to verify what files are tagged before copying, use the Shift+L hot key to list tagged items. Use Shift+Space instead to say either the tagged items, or the current item if there are no tags.

A > symbol following an item indicates that it is tagged (putting this symbol after rather than before preserves initial letter navigation in the ListBox. The > symbol is also the key (Shift+Period) to tag an item and automatically move to the next one in the list. The < symbol untags and also moves ahead, thereby allowing you to efficiently go through a directory list and selectively tag items. This tagging approach has benefits over selection in Windows Explorer, e.g., tags are not lost by an accidental key press that moves focus.

FileDir uses a "Multiple Document Interface" (MDI), so any number of directory views may be opened, cycled among with Control+Tab, or closed with Control+F4. It is also a "single instance" application, so the desktop shortcut key, Alt+Control+F, activates the same program when FileDir is found in memory, rather than opening a new copy.

Some commands differ in whether they open a new window ore re-use the existing one. Commands with "Go to" in the name re-use the existing window, whereas "Open" commands start a new one.

Extra speech is provided through a UIA notification that JAWS, NVDA, and Narrator announce. These are intended to be comprehensible chunks of targeted information. For example, pressing Shift+Y gives the "yield" or count and combined size of tagged folder and file items in the current directory view. Speech of this nature can be efficient for screen reader users as opposed to, say, a message box that generates additional screen reader speech describing the dialog, and then has to be dismissed with another key, triggering more screen reader speech.

Typically, a key combination using Alt or Control rather than Shift performs another variation of the command. For example, Shift+L lists all tagged items, Control+L lists all items regardless of tagged state, and Alt+L lists all files, but not folders.

FileDir commands often include verbal confirmation during execution, e.g., announcing the name of each file before a copy attempt and the message "Done!" after batch processing is complete.

FileDir initially opens the Personal/My Documents folder when launched with no command line parameters. If a folder is passed as a parameter, however, that folder is opened instead. In subsequent sessions, FileDir remembers the last directory viewed, including its sort order and filter specification. It remembers other values from the previous session as well, including those for the Copy, Find in Files, FTP, Go To, Jump, Keywords, Move, Open, Unzip, and Zip commands.

The default sort order is reverse date/time, meaning that most recently modified items appear first. All folder items, however, appear before any file items. FileDir applies the current sort order and filter specification when creating a new directory view.

Since there are numerous FileDir commands beyond those in Windows Explorer, learning the software may seem daunting at first. Getting started is not hard, however, since FileDir works similarly to Windows Explorer. You can then learn additional commands according to your time and needs. Remember that you can review this documentation at any time by pressing F1. You can also get a list of commands by pressing Alt+Shift+H for a hotkey summary. If you have trouble remembering the key or menu associated with a command, try the Alternate Menu command, Alt+F10, which lets you pick a command from a complete, alphabetized list. Use the Key Describer command, Control+F1, to toggle a mode in which pressing a key describes its action. If you switch to another application window, the mode is automatically turned off.

FileDir commands can be subdivided into several categories, related to the following conceptual labels: edit, find, go to, navigate, query, select, transfer, and miscellaneous. You can edit file attributes, find items by textual match, go to different folders, navigate among items in a folder, tag files for further action, transfer them to various places, and do other, miscellaneous tasks. The sections below explain these categories.

## Edit Commands

Edit commands change the content, name, or other attribute of an item. Press Alt+Enter for the standard Properties dialog, like in Windows Explorer.

Press Control+W to load the current file into a word processor. Microsoft Word is the default, but a different one may be configured with the Configuration Options command, Alt+Shift+C. Press Control+T to open it in a text editor instead, the configurable default being EdSharp, available at [EdSharp on GitHub](https://github.com/JamalMazrui/EdSharp)

If another word processor or text editor is to be used, its full path may need to be specified if the executable is not located on the Windows search path. Such configuration options may also be manually edited, since they are stored in a standard .ini file, FileDir.ini, with an optional FileDir.inix overlay, located in the FileDir folder under your Windows Application Data directory.

The Rename command, Shift+R or F2, lets you edit the name of the current file or folder item. Control+R lets you rename multiple items using the * and ? wildcard characters. A DOS command is used behind the scenes to accomplish this, so all items in the current directory are processed -- regardless of their tagged state within FileDir. You can use Control+Shift+R to rename tagged items based on a "regular expression" -- a powerful but complex syntax that is beyond the scope of this guide (Google will find many tutorials). Control+Shift+I renames files to the initial line of text inside them (if found), which is often a convenient way of making the name of a file the same as the title of the document inside.

Like the greater than symbol meaning tag, special symbols are associated with folder and file attributes. A backslash symbol after an item indicates that it is a folder rather than a file. A right parenthesis after a list item means that the Hidden attribute is set. As a memory aid, you may think of parenthesis hiding something from full view. The RightParen key, Shift+0, sets the Hidden attribute of the current or tagged items. The LeftParen key does the reverse, removing the Hidden attribute. Similarly, the right bracket symbol means that an item has the ReadOnly attribute set. You may think of a bracket protecting something from being modified. The LeftBracket key removes the ReadOnly attribute. Finally, the right brace symbol means the System attribute is set. You may think of a brace as a character used in programming systems. The LeftBrace key removes the System attribute.

Press Exclamation Point (! or Shift+1) to stamp the current or tagged items with a different modification date and time. FileDir prompts for numeric values for the year, month, day, hour, minute, and second, defaulting to those of the current file or folder item.

## Find Commands

Find commands search for an item by a string of characters in the name or body. Press Alt+Shift+F to find a file anywhere in the current folder or subfolders based on text it contains and/or a wild card pattern. For example, you could search for the term "strategic plan" in each file with a name matching the minutes*.doc specification (meaning a name that has minuttes at the beginning and .doc at the end). FileDir will present a list of files that match the wild card pattern. Use arrow key or initial letter navigation to focus on the item of interest. Pressing Enter will then go to the folder containing that item and place focus on it.

Press Control+J to jump to an item within the current folder, based on a sequence of characters (no wildcards) appearing somewhere within its name. The command also recognizes symbols associated with file attributes. Thus, you can jump to a ReadOnly file by entering a single ] character as the search string. A [ would find the next item without the ReadOnly attribute set. Using the Jump Again command, Alt+J, you can efficiently hop from one match to the next.

The Control+K command searches for a keyword inside the body of a file. The command supports multiple conditions. Use the vertical bar character (|) to separate words or phrases where any one of those terms can produce a match. Use the ampersand character (&) as a separater where all terms must match. For example, entering "C#|Visual Basic" would match files containing either language, whereas "C#&Visual Basic" would require both to match. Press Alt+K to hop to the next matching file.

Press Control+F to filter files in the current folder to a restricted view of those matching a wild card pattern. You can separate multiple patterns with the vertical bar (|) character, meaning the pattern can match alternate conditions, e.g., "*.doc|*.rtf" for files in either Word or Rich Text Format. Press Control+Shift+F to remove any filter and make all items available in the view.

## Go to Commands

Go to commands change the FileDir view to another folder. Press Control+G for a dialog in which you can accept a previous path, enter a new one, or choose from a tree view control. Press Control+Shift+G to go to a folder from a list of those with special names designated by Windows, e.g., My Documents or Start Up. Press Alt+G to go to a drive in a new window. FileDir presents a list of all available drives, and then activates a view of the one chosen in the directory that Windows considers to be the current one on that drive.

Substitute the O key for similar commands where a new window is opened rather than the existing one being reused: Control+O for Open Folder, Control+Shift+O for Open Special Folder, and Alt+O for Open Drive.

Press Alt plus a digit between 1 and 9 to quickly open the drive whose letter is in that numeric position of the alphabet. For example, Alt+1 opens Drive A, and Alt+3 opens Drive C.

There are a few pairs of Open and Go To commands. Open commands preserve the current directory view, including its tagged states, and activate a different directory view in another window. Go To commands reuse the current window, instead, for another directory view. In the following pairs of commands, the shifted version is a Go To command, requiring more conscious effort due to a more destructive nature, since it discards the current directory view. This difference is similar to how Shift+Delete is more destructive then Delete, since the shifted version does not permit recovery from the recycle bin. Enter opens a subfolder whereas Shift+Enter goes to it. Backspace opens the parent folder whereas Shift+Backspace goes to it. Backslash opens the root folder of the current drive whereas Shift+Backslash goes to it. As before, FileDir checks if a view of the target directory already exists, and if so, activates that window rather than creating another for the same directory.

If you would prefer Go To behavior without having to press Shift, then toggle the Recycle with Delete setting with Alt+Shift+R. The more destructive setting is also more convenient (no delay in waiting for items to be copied to the recycle bin). It requires a deliberate change from the default to make sure that is how you want to operate. In that case, the Shift version of a command is for Open rather than Go To.

Press backslash (\) to go to the root level of the current drive. Press comma (,) or Backspace to "come up" a level in the folder tree, going to the parent of the previous folder. Press the Period (.) or F5 key to refresh the current folder. This may be needed if items on disk were changed in a way that FileDir does not automatically track.

When the current item is a folder, pressing Enter goes to it. When it is a zip archive, FileDir presents a view of the contained items that is similar to a directory view. If you would prefer to open a .zip file with another program associated with that extension (e.g., WinZip), change the ZipOpener configuration option to N for No. Also, you could normally view archives with FileDir by pressing Enter, but choose to open them with another program instead by pressing Shift+Enter. This opens them with the default program associated with the extension in the Windows registry.

The Quick Links feature efficiently opens favorite files, folders, or URLs. Press Shift+Q to add a quick link for the current item. A standard Windows shortcut (.lnk file) will be created in the Quick subfolder of the FileDir program folder. FileDir lets yu rename the shortcut before creating it.

Use the Quick URL command, Alt+Shift+Q, to create a quick link to an Internet resource. FileDir prompts for the name and URL, and then creates a standard .url file in the Quick folder. If the clipboard holds a web address, FileDir offers it as the default URL, so you can copy a link in your browser and then create the shortcut with a single command.

Press Control+Q to open the Quick folder, or the grave accent key (`) at the far left of the numeric row (U.S. keyboard) to go to it. You can navigate the Quick folder like any other. Press Enter to execute a quick link. Press Alt+Enter to review or modify the settings of a .lnk file. a .url file is editable text in the standard .ini format.

Alt+R lists recent folders -- every folder or zip archive opened since the start of the current FileDir session, with the most recent first. Choose an item from this standard listbox to open it. Use the Windows Toggle command, Shift+W, to switch between a pair of directory views that you are working with. Each presss returns to the previous window viewed.

Nine commands on the Window menu let you quickly open or go to an existing view on a drive. Drives A through I are associated with the digits 1 through 9. For example, press Alt+1 to go to Drive A or Alt+3 to go to Drive C.

In general, FileDir checks if you are trying to open a folder that already has an open window. If so, it says "Returning and activates that window rather than creating a new one. Press F4 to pick one of the currently open windows from a standard ListBox. Alternatively, Control+Tab or Alt+RightArrow activates the next open window, and Control+Shift+Tab or Alt+LeftArrow activates the previous one. Press Shift+F4 or Alt+NumPad5 to hear the number and titles of all open windows. Press Control+F4 to close the current window, or Control+Shift+F4 to close all windows except the current one. Alt+F4 exits FileDir, and Alt+Shift+F4 restarts Windows (after confirming).

As you type in the edit box for specifying a folder, it guesses the input desired based on existing paths, in the way a web browser completes an address as you type. If the path you ultimately entered is not found on disk, the dialog prompts whether to create it. This makes it convenient to copy, move, or unzip files to a new folder with a single command.

FileDir supports the concept of a "virtual folder" that does not exist as a physical directory on disk. A virtual folder is defined by a path list in a text file. It contains the full paths of files or folders, not necessarily in a single directory, but in any directory and on any drive. You can create such a file in a text editor, or with the help of FileDir commands like Path List, Control+Shift+P, and Export Clipboard, Alt+Shift+E. Press Alt+Shift+O to open a virtual folder definition, or Alt+Shift+G to go to it. In general, you can then process items as if they were in the same directory.

## Navigate Commands

Navigate commands change the focus within a folder, based on a fixed increment or boundary. Press Home to navigate to the beginning or first item, or End for the end or last one. If the current folder contains subfolders, then the beginning item will be a folder since they always appear before files. Press Alt+B to go to the beginning file, skipping over folder items before it.

A group of shifted letter keys navigate similarly within the set of tagged items: Shift+B for Beginning tagged, Shift+E for End tagged, Shift+N for Next tagged, and Shift+P for Previous tagged (if any). These commands let you review or inspect the subset of tagged items. Shift+L also may be useful to list all tagged items.

Press Shift+I for Initial Change, which jumps to the next item that starts with a different letter. Similarly, press Shift+X for Extension Change, which jumps to the next file with a different extension. These commands are most useful when the sort order is by alpha/name or extension.

## Query Commands

Query commands announce aspects of the current environment via speech output. Press Shift+F4 (or Alt+NumPad5) to hear the titles of all FileDir windows currently open.

Press Apostrophe for the name of the current file or folder item, as well as its tagged state if set. Press Shift+Apostrophe for the name of the parent folder containing the current item. Press Control+Apostrophe for the path of the parent folder.

For example, if the current item is the file whose path is

```
C:\Temp\Calendar.doc
```

Pressing Apostrophe says

Calendar.doc

whereas Shift+Apostrophe says

Temp

and Control+Apostrophe says

```
C:\temp
```

Press Alt+P to confirm the complete path

```
C:\temp\Calendar.doc
```

Press Alt+Semicolon to query the current time and date. Press Alt+Apostrophe to hear text currently on the clipboard (You may think of this as quoting the clipboard). Press Question Mark (?) for the What Content command, which verbalizes the textual content of the current file item, or lists items contained in a folder item or zip archive. For technical reasons, the command reads a maximum of about 20K from a file.

Press Shift+S for the size of the current file or folder item. Press Shift+D for its date and time stamp. Press Shift+T for Type, which provides miscellaneous information, including the registered file type and ReadOnly, Hidden, or System attributes set (if any). Press Control+Shift+T to examine all "extended properties" that are available to Windows Explorer. Depending on the type, 32 possible properties may be examined as follows:

Name

Size

Type

Date Modified

Date Created

Date Accessed

Attributes

Status

Owner

Author

Title

Subject

Category

Pages

Comments

Copyright

Artist

Album Title

Year

Track Number

Genre

Duration

Bit Rate

Protected

Camera Model

Date Picture Taken

Dimensions

Episode Name

Program Description

Audio sample size

Audio sample rate

Channels

Press Control+L to list all items in the current folder, or Shift+L for those tagged. Press Alt+L to list files but not folders.

Press Y for the yield, or count and combined size, of items in the current folder. Press Shift+Y for those tagged or Alt+Y for files only. Press Control+Shift+Y for the total size and free space on the current drive. Press Alt+Shift+Y for operating system information, including the Windows version, physical memory, and virtual memory.

Press % (Shift+5) for the Percent Through command, which indicates the relative position of the current item in the list, e.g., "6 of 20 items, 30% through." If you are sequentially examining the files in a folder or ZIP archive, this gives you a sense of how much is done and what remains. Use the Filter Query command, Star (Shift+8), to quickly check the current sort order and filter specification.

## Tag Commands

Tag commands increase or decrease the subset of items that are marked for further action by actions that can operate on multiple items at once. Press Control+A to tag all file and folder items, or Control+Shift+A to clear all tags. Press Alt+Period (associated with the grater than symbol) to select all files but not folders. Alt+Shift+Period tags duplicate files -- any file with the same content as a prior one in the directory list. This may be useful for deleting after downloading files, where some are the same except for their name or date. Control+Shift+Period tags files that match a regular expression you specify.

Press Alt+Comma to untag all items except the current one. Control+I inverts all tagged states, untagging items that were tagged and vice versa. Spacebar toggles the tagged state of the current item. Press semicolon (;) to tag the current item regardless of its previous state, or ForwardSlash (/) to untag it.

To navigate and make tag decisions together, use the GreaterThan key (Shift+.) to tag and go to the next item, or the LessThan key (Shift+,) to untag instead. Alternatively, use arrow keypad commands similar to Windows Explorer. For example, press Shift+DownArrow for Tag and Next, or Shift+UpArrow for Tag and Previous. Press Shift+End for Tag to Bottom, or Shift+Home for Tag to Top. Shift+NumPad5 tags the current item.

Adding the Alt modifier key performs the same actions except for untagging rather than tagging. Thus, Alt+Shift+NumPad5 untags the current item, Alt+Shift+Home untags to the top of the list, Alt+Shift+End untags to the bottom, Alt+Shift+DownArrow untags en route to the next, and Alt+Shift+UpArrow untags en route to the previous.

Other arrow keypad actions duplicate home row commands for navigating among tagged items. Control+Home goes to the Beginning Tagged item, like Shift+B, and Control+End goes to the End one, like Shift+E. Control+DownArrow goes to the Next Tagged item, like Shift+N, and Control+UpArrow goes to the previous one, like Shift+P.

Press Control+S to save tags in the current directory view, and Control+Shift+S to restore them. This could be useful if you need to temporarily change which items are tagged.

## Transfer Commands

Transfer commands take action on a whole item, copying or moving it to another folder, the clipboard, printer, or recycle bin. Press Shift+C to copy, Shift+M to move, or the Delete key to delete. The Recycle Toggle, Alt+Shift+R, determines whether deleted files or folders are moved to the recycle bin. The initial setting is On, and then FileDir remembers the value between sessions. Regardless of the current setting, Shift+Delete deletes without recycling, whereas Control+Delete deletes and recycles. Use the Delete Recycle Now command, Control+D, or Delete Now Command, Control+Shift+D, to quickly delete a single file (but not folder) without a confirmation dialog. Note that the Delete, Copy, and Move commands execute noticeably faster when deleted or replaced items are not moved to the recycle bin. Press Control+B to open the Recycle Bin and recover deleted items.

Press Shift+Z to zip files into a compressed archive, or Shift+U to unzip them. The Control+Z command also zips, but then deletes originals after confirming the integrity of the zip target.

Use the Zip List command, Control+Shift+Z, to create or update a zip archive based on a list of files or folders in a text file. For example, the file backup.lst would contain the full path of the target zip archive as the first line of text. Subsequent lines would contain file or folder names to be added to the archive. Paths are not needed before these names if they are in the same directory as the archive.

Control+U unzips without preserving subfolders. It unzips all files to the chosen folder, but not subfolders below (folder paths, if any, are ignored). Control+Shift+U unzips to a target with the same name as the archive. For example, if focus is on mag0712.zip, then the proposed target path will end in mag0712.

You can test whether a file can be unzipped successfully by pressing Alt+U. Press Alt+Shift+U to set a password to be used by FileDir when creating, extracting, or viewing zip archives. It may also be set in the Options dialog, Alt+O. For security, the password is saved between FileDir sessions in an encrypted form rather than as text with other settings in the FileDir.ini file.

Starting with FileDir 3.7, the unzip commands are now broader, unarchive commands that work with almost any archive format, including .rar, .tar, .gz, .bz2, .chm, .cab,. FileDir does this with the free 7Zip utility behind the scenes, which is also available independently at [the 7-Zip home page](https://www.7-zip.org/)

Although any archive may be viewed or extracted, it is still the case that only a zip archive may be created or modified.

Some commands work with a copy of a zipped item that is unarchived to a temporary folder as needed. This lets you use the What Content command, Question Mark, to identify the content of a file without unzipping the archive that contains it. The Run command, Entor, and Send to Word Processor or Text Editor commands, Control+W or Control+T, also work in this way.

The Copy or Move Tagged commands, Shift+C or Shift+M, prompt whether to overwrite existing folders and files. You are informed whether the date of a target with the same name is older, newer, or current and whether its size is smaller, larger, or equal. You can choose to keep all targets with the same names, replace them, replace them only with updated source items, or increment source names to eliminate conflicts (e.g., ReadMe_01.txt).

Like Windows Explorer, Control+C, Control+X, and Control+V copy, cut, and paste file or folder items between the current directory and clipboard. FileDir enhances these commands with a plain text format in addition to the binary "drop list" that Windows Explorer uses to facilitate drag and drop transfers with a mouse. Since the clipboard can actually hold multiple formats at the same time, FileDir creates both a binary and a text format when copying with Control+C or cutting with Control+X. The text format is simply a list of file or folder paths, one per line. Thus, paths on the clipboard are simultaneously available both to applications like Windows Explorer that look for the binary format, and applications like Notepad that look for plain text.

When pasting, Control+V recognizes the text format as well as the binary one. Since the text format does not indicate whether files had been copied or cut to the clipboard, this command copies, rather than moves, the originals when only text format is found. With either format, you may ensure that the originals are copied with Alt+V, or that they are moved with Alt+Shift+V.

Use the Copy Append command, Alt+C, to add items to the clipboard in both binary and text formats. This lets you build a list on the clipboard from files in different directories. It also lets you build a list by pressing Alt+C when focused on each item of interest, rather than first creating a set of tagged items and then copying them as a batch.

To put a list of file names on the clipboard without preceding paths, press Control+Shift+C. To hear what files are on the clipboard, use the Quote Clipboard command, Alt+Apostrophe. Before saying each path, FileDir says "Path drop list" if it finds this binary format. Otherwise, FileDir only speaks text format -- other binary formats on the clipboard are not interpreted.

Control+P sends current or tagged items to the default printer. Control+M starts a mail message with its body being the textual content of the current item. For example, pressing Control+M when a Microsoft Word document is the current item will extract its text for the message body and use its name (without extension) as the default subject. Control+Shift+M starts a message with the current or tagged items as attached files. If no items are tagged, FileDir both attaches the file and includes its text in the message body.

Use the Batch Mail command, Control+Shift+B, to individually send a message to multiple recipients (please do not use this for spam). FileDir prompts for a text file that defines a batch mail operation. The first nonblank line is assumed to be the subject of the message. The next nonblank line is the full path of a text file that contains the body. Each subsequent line that contains an @ symbol is the address of a recipient. Here is an example definition:

[Content of Batch.eml File]

This is the subject line

```
C:\My Documents\Body.txt
```

[jane@doe.com](mailto:jane@doe.com)

"John Doe" <[john.doe@mail.net](mailto:john.doe@mail.net)

[End of Content]

Before sending a batch email, configure FileDir options for LogInUserName, Password (stored in an encrypted form), SenderAddress, and OutGoingServer (e.g., outgoing.verizon.net). Test the command by sending yourself mail first. This command only works with common SMTP protocol settings.

Press Shift+O to output tagged files in plain text format. The original, source files will not be affected. The target, converted files will have the same names but a .txt extension. Conversions to text are available for Word, Excel, PowerPoint, PDF, HTML, Markdown, and comma separated files, in both their older and newer forms. The conversion is done by 2htm, which comes with FileDir, so no other program has to be installed.

The same conversion mechanism may be used to place text on the Windows clipboard instead of creating new files. Press Shift+A to append the textual body of currently tagged files to the clipboard. They will be separated by a sequence of characters indicating a divider between sections of a composite document: a line of 10 dashes followed by a form feed (hard page break). A termination sequence says "End of Document." This command is useful for combining multiple, related files, e.g., downloaded web pages, into a single document. You can use the Clear Clipboard command, Alt+Shift+', to clear the clipboard before appending to it. The Extract with Regular Expression command, Control+Shift+E, works similarly except that you are prompted for a regular expression, and only matching text is copied.

Alt+P queries the full path of the current item, whereas Alt+Shift+P copies it to the clipboard, e.g.,

C:\Documents and Settings\Owner\My Documents\My Music\MySong.mp3

This may be useful so that the string is available by pressing Control+V to paste it into the open file dialog of another application. To get the short path instead, press the Tilde key (Shift plus the Grave Accent at the top left of the main keyboard). A short path contains no spaces and uses a suffix of a tilde symbol (~) and a number to abbreviate file or folder names. This may be useful when pasting into a command line, since more characters and surrounding quotes are usually needed otherwise to specify a file.

Control+C copies the full path of tagged items to the clipboard, whereas Control+Shift+C copies their names only -- no preceding directories.

Control+Shift+P copies the full paths of all items below a subfolder item in the directory hierarchy. For example, if the My Documents directory is being viewed, and focus is on the My Music subfolder item, pressing Control+Shift+P would copy the paths of all files and subfolders under My Music. After determining what file extensions are present, FileDir prompts for which ones to include in the resulting list. Edit the choices, or just press Enter to accept them all. You can save the path list to disk with the Export Clipboard command, Alt+Shift+E, which prompts for a file name and then saves clipboard text to it.

Press Control+N to create a new folder. Press Control+Shift+N to make a new copy of the current file or folder item. It will have the same name except for a unique numeric suffix after the root, e.g., plan_01.doc would be a copy of plan.doc, and plan_02.doc would be the next copy. Such a file is sometimes useful when you want to preserve the original unaltered and then make changes to a copy in the same folder.

FileDir includes the capability to "put" or upload files to a directory on an FTP server, and to "get" or download from there. For private directories, a user name and password may be set either in the specific dialogs for these commands or in the Configuration Options dialog, Alt+Shift+C. For security, the password is saved between FileDir sessions in an encrypted form rather than as text with other settings in the FileDir.ini file.

Use the FTP Put command, Shift+F, to upload files. FileDir prompts for an FTP directory. If the value entered does not contain the :// sequence of characters, FileDir adds an FTP:// prefix and a / suffix for more convenient typing. For example, a value of

smart.net

would become

ftp://smart.net/

If you include the :// sequence of a protocol, however, FileDir accepts the value verbatim -- without making changes. The URL is remembered as the default value for the next FTP command.

The opposite command is Get FTP, Shift+G, which downloads files from a remote directory. FileDir presents a multiple selection list box with all file names it found in that directory. The files selected will be downloaded to the current directory view. Any existing files with the same names are replaced and sent to the recycle bin according to the Recycle setting, Alt+Shift+R (on by default).

The Web Download command, Alt+Shift+W, lets you pick one or more files to download from a page whose address you specify.  If the clipboard holds a web address, FileDir offers it as the default.  Downloading is done by FileDir itself, with no web browser involved. Each item of the resulting checked listbox shows both the clickable text of the url and its target file name. Press Spacebar to toggle the checked state of an item. After picking files, you are prompted for the target folder on disk. If the URL of a link does not end in a valid file name, FileDir creates a file name for the target on disk based on other characters in the URL. If a file with the same name already exists, a unique name is created by adding a numeric suffix, e.g., page_001.htm, page_002.htm, etc.

A listbox control of the .NET Framework does not support multiple letter navigation, so each letter typed jumps to the next item starting with that letter. To make navigation more flexible and efficient, particularly in a listbox with many items, FileDir adds the following features to a list based dialog. Control+J prompts for text within an item, going to the first match if a new search, or the next match if the previous value is accepted. Alt+J goes to the next match without prompting for a value. The item with focus when the dialog is closed -- but not canceled -- becomes the current item the next time that the same list dialog is invoked (you are notified when it is not the first item). The Jump value of that dialog is also remembered.

Control+F sets a filter to restrict what items are shown via wildcards (* to match any sequence of characters or ? to match a single one). For example, you could browse replace-related commands in the Alternate Menu, Alt+F10, by pressing Control+F after invoking that list and then entering *replace* as the filter expression. Control+Shift+F clears the filter so all items are shown again. The order of items may also be changed: Alt+A for alpha order, Alt+Shift+A for reverse alpha order, Alt+D for default order, or Alt+Shift+D for reverse default order.

Multiple commands support flexible checking or unchecking in a checked listbox such as the Web Download dialog. Press Space to toggle the checked state of the current item, Control+A to check all items, or Control+Shift+A to uncheck all. Press Shift+DownArrow for check and Next, or Shift+UpArrow for check and Previous. Press Shift+End for check to Bottom, or Shift+Home for check to Top. Shift+NumPad5 checks the current item. F8 marks the start of a checking operation, completed with Shift+F8.

Adding the Alt modifier key performs the same action except for uncheckging rather than checkging. Thus, Alt+Shift+NumPad5 unchecks the current item, Alt+Shift+Home unchecks to the top of the list, Alt+Shift+End unchecks to the bottom, Alt+Shift+DownArrow unchecks en route to the next item, and Alt+Shift+UpArrow unchecks en route to the previous. F8 then Alt+Shift+F8 unchecks items in that range.

Other arrow keypad actions navigate among checkged items. Control+Home goes to the top checkged item, and Control+End goes to the bottom one. Control+DownArrow goes to the Next , and Control+UpArrow goes to the previous.

Shift+Space tells you what items are currently checked. Alt+A says the address of the current item in the list, e.g., 11 of 42.

## Miscellaneous Commands

Miscellaneous commands do not fit neatly into previous categories. Use the Configuration Options command, Alt+Shift+C, to configure FileDir options such as the word processor invoked with Control+W or the text editor invoked with Control+T. Alternatively, use the Manual Options command, Alt+Shift+M, to adjust configuration options in the designated text editor.

Extra speech messages may be toggled off -- or reactivated -- with Control+Shift+X. When off, such messages are redirected to a text file called Speech.log, which may be examined in an editing window with Alt+Shift+X. This file is initialized when FileDir starts, and the Extra Speech setting is remembered from the previous session.

With the optional JAWS scripts, you can toggle a speech setting of reading all or no punctuation using JAWSKey plus the grave accent at the top left of the main keypad (U.S. keyboard). All punctuation is useful when reading carefully for details, whereas no punctuation is useful when reading quickly for concepts.

The main interface of FileDir is a ListBox containing items that are either folders or files, with folders listed first. The default order is by most recent date and time, so that a file most recently modified will appear before others and be convenient to locate. Subsequent sort order can be controlled by pressing Alt+A for alphabetic/name order, Alt+S for size order, Alt+D for date/time order, or Alt+T for type/extension order. Add the Shift key to reverse the order, e.g., Alt+Shift+S puts the largest file first (to query its precise size, press Shift+S). If you would prefer files to be listed before subfolders, change the DirsBeforeFiles configuration option to N for No.

Press Alt+Shift+B to burn tagged files to a CD. An external utility is invoked that lets you pick a drive and check estimates of space before and after on the CD.

The Context Menu command, Shift+F10, lets you choose an action to perform on the current file based on those available for its type/extension (in the Windows registry). Also included is the OpenWith action, by which a default program may be associated with files of this type. The Send To Menu, Control+F10, lets you choose among SendTo shortcuts (installed by various applications) to perform on the current or tagged files.

Press Control+Slash (or Control+Backslash with the JAWS scripts) to go to a command prompt in a console mode window. Its current directory will be the same as in FileDir. You can enter DOS-style commands there. Press Alt+Slash (or Alt+Backslash with JAWS) to open the current directory in Windows Explorer.

Use the Iterate Processes command, Alt+I, to list all processes currently running on your computer. Each item displays the executable name without extension, followed by the title of its main window if available. Buttons let you choose whether to activate a process (only possible if it has a window) or terminate it. If Terminate is chosen, FileDir first sends a request for the process to close, and if that fails, asks whether to try to force it. You are then returned to the list of processes in case you wish to examine the next one. End this dialog either by activating a process or choosing Cancel (same as pressing Escape).

Use the Inquire Differences command, Alt+Shift+I, to compare the files in two folders. The current folder is considered the source. You are prompted for a target folder. FileDir generates a report in structured text format and prompts you for where to save it. The default name is Report.txt in the current folder. The report contains three sections: common target files, missing target files, and additional target files. The first section lists target file names that also exist in the source folder, and indicates whether each is newer, older, or current (a time stamp comparison), as well whether it is larger, smaller, or equal (a size comparison). The second section lists file names that are missing in the target folder. The third section lists additional file names found in the target folder.

Use the Volume Format command, Control+Shift+V, to format a disk or storage card. Press Control+Shift+W to launch Windows Control Panel. If you associated FileDir with folders rather than Windows Explorer, you may need to open Control Panel in this way rather than through the Windows Start Menu.

Press Alt+Shift+N to manage network connections. A dialog lets you connect, disconnect, or restore mappings between physical storage and logical drives.

Since FileDir is a program designed to be generally available while running others, it offers a few, simple utilities not directly related to file management. The Evaluate command, Control+Equals, prompts for a mathematical expression, and then copies the result to the clipboard. Standard arithmetic operators may be used, as well as methods of the C# programming language. For example, the expression

3 * 4

produces 12

and

Math.Pow(3, 4)

produces 81.

Use the Convert Units command, number sign (#) or Shift+3, to convert between different units of measure, e.g., between metric and other units of distance, volume, weight, or temperature. Pick the type of conversion from the list box and enter the input value in the edit box. The output value is spoken and copied to the clipboard (and may be reviewed with the Quote Clipboard command, Alt+Apostrophe). About 80 conversions are available as follows:

Acre to hectare

Atmosphere to psi

BTU/hour to watt

Celsius to Fahrenheit

Celsius to Kelvin

Centimeter to inch

Cubic ft to cubic m

Cubic m to cubic ft

Day to hour

Day to minute

Degrees to radians

Fahrenheit to Celsius

Fathom to meter

Foot to inch

Foot to meter

Ft/sec to meter/sec

Gallon (US dry) to liter

Gallon (US dry) to quart (US dry)

Gallon (US liquid) to liter

Gram to ounce (avoirdupois)

Gram to ounce (troy)

Hectare to acre

Horsepower (elec.) to watt

Horsepower (metric) to watt

Hour to day

Hour to minute

Inch to centimeter

Inch to foot

Kelvin to Celsius

Kg/sqcm to psi

Kilogram to pound

Kilogram to ton (UK)

Kilogram to ton (US)

Kilogram to ton (metric)

Kilometer to mile

Kilowatt to watt

Knot to mph

Kph to mph

Light-year to mile

Light-year to parsec

Liter to gallon (US dry)

Liter to gallon (US liquid)

Liter to pint (US dry)

Liter to pint (US liquid)

Meter to fathom

Meter to foot

Meter to yard

Meter/sec to ft/sec

Mile to kilometer

Mile to light-year

Minute to day

Minute to hour

Minute to second

Mph to knot

Mph to kph

Ounce (avoirdupois) to gram

Ounce (troy) to gram

Parsec to light-year

Pascal to psi

Pint to liter (US dry)

Pint to liter (US liquid)

Pound to kilogram

Psi to atmosphere

Psi to kg/sqcm

Psi to pascal

Quart (US dry) to gallon (US dry)

Radians to degrees

Second to minute

Square cm to square in

Square ft to square m

Square in to square cm

Square m to square ft

Ton (UK) to Kilogram

Ton (US) to Kilogram

Ton (metric) to Kilogram

Watt to BTU/hour

Watt to horsepower (elec.)

Watt to horsepower (metric)

Watt to kilowatt

Yard to meter

Conversions may be added, modified, or deleted by editing the Convert.txt file in the FileDir program folder. A new installation of FileDir will replace this file, however, so custom changes would need to be manually backed up and restored.

Timer keys are on Alt+Control. Press **Alt+Control+T** to start a timer; FileDir asks for the announcement interval and the stop time. The announcement interval, in seconds, is how often FileDir says how long has passed since the timer started, so 60 means once a minute. Those announcements happen whatever program is in front. Leave the interval blank or 0 to run the timer silently. Press **Alt+Control+Y** at any time to hear how long has passed. If a timer is running, Alt+Control+T pauses it; if paused, it resumes. Press **Alt+Control+S** to stop the timer and hear the total time it ran.

The timer moved off the F12 row so that F12 and Shift+F12 could ask the AI, as they do in EdSharp.

In the dialog that prompts for the announcement interval, another field is the stop time. A blank or 0 value means that the timer will run until manually stopped by pressing Shift+F12 or exiting FileDir. Instead, a stopping point may be specified as a date and time. The date and time components are each optional. If a date is used, it must include at least the month and day, separated by the forward slash character (/) -- or equivalent for non-U.S. formatting conventions. If a time is used, it must include at least the hour and minute, separated by a colon character (:) -- or non-U.S. equivalent. If both date and time components are used, type the date, a space, and then the time. Without a time, today's date is assumed. A time may use either the military, 24-hour convention, or the AM/PM suffix (otherwise AM is assumed if the hour is less than 13). Examples of valid date/time values are as follows:

2:00 PM

14:00

7/27 6:30

2007/7/27 6:30:15

When the stop time is reached, FileDir plays some chimes and ends the timer. Such an alarm may be used either with or without intervening announcements of time intervals. A timer runs independently of other FileDir operations, so you can continue working in FileDir while using this capability.

Use the Play List command, Control+Shift+L, to create a .m3u file with references of tagged items to play sequentially. Types may include .mp3, .wav, or .cda (the extension of a track on a standard audio CD). FileDir prompts for the name of the play list to create, defaulting to PlayList.m3u in the current directory. Focus is then placed on that file (if in the same directory), so you can simply press Enter to execute the play list. Note that if you want to play tracks on an audio CD, however, you need to save the play list in another directory that permits the creation of new files.

Use the Environment Variables command, Control+E, to review or change such settings of Windows. Choose those of the current process, user, or system as a whole. Jump quickly to a particular variable based on its initial letter, e.g., Alt+P for the PATH setting that determines where Windows searches for an executable file that is not found in the current directory. Changes to process settings affect the current session of FileDir, but not the next time it is run. User settings take effect when you log in again. System settings take affect when you restart the computer.

FileDir windows may be visually organized according to common MDI (multiple document interface) patterns. The Window menu includes the following commands: Arrange Icons, Alt+F11; Cascade, Control+F11; Tile Horizontal, Alt+Shift+F11; and Tile Vertical, Control+Shift+F11.

Use the Elevate Version command, F11, to download and install the latest version of FileDir. You are prompted for confirmation. The installer is downloaded to the folder for temporary Internet files so it will be deleted automatically when Windows reclaims space in that folder. The current FileDir version is then unloaded so that the installer can replace any files that were in use. You can reload the updated version in the usual manner after installation, e.g., by pressing Alt+Control+F.

## What Is Inside a File

Press **Control+Shift+T** for Type Extended. It shows everything known about the
item you are on as one plain list of field names and values, sorted by name
without regard to capitalization.

Three sources are merged into that one list:

- **The Windows properties**, the same ones File Explorer shows on a properties
  page.
- **The file association**, meaning what opens this kind of file, its content
  type, and the verbs registered for it.
- **The metadata inside the file itself**, read by ExifTool. That is the camera,
  lens and exposure of a photograph; the artist, album and track of a song; the
  duration, codecs and frame rate of a video; the author, producer and page
  count of a PDF.

One sorted list rather than three sections, on purpose. Somebody looking for
"Artist" should not have to know which of the three sources knows it, and with a
screen reader, first-letter navigation through one alphabetical list beats
arrowing through three groupings. FileDir says how many fields there are before
the list opens, so you know what you are looking at.

If ExifTool is missing, the Windows properties are still shown and a line at the
end says where FileDir looked and what to install. ExifTool comes from the media tools the installer offers, along with
ffmpeg and yt-dlp.

## Renaming to What Is Inside

Press **Control+Shift+I** for Rename to Identify Content. It names each tagged file after
what is inside it, looking in two places.

**First, the metadata.** The title of a PDF, the song and album of an MP3, the
caption of a photograph, the book name of an EPUB. Every field whose name is
about a title is considered and the longest is taken, because a photograph
carrying both `IMG_4021` and "Sunset over the Cascades" is better called the
second.

Fields that name the camera, the lens, the software or the file itself are
ignored, and so are fields naming the album or series a file belongs to — a song
should be called by its own title, not by the record it came from.

**Only if there is no metadata, the first line of the text.** This is a last
resort, and it runs only for files FileDir can read as text. A first line is
often not a title at all — a date, a byline, "Chapter One" — so the metadata is
always preferred, and for a photograph or a song it is far better. For a note or
a Markdown page, where the title is on the first line and nowhere else, it is
exactly right. Heading marks are stripped from it.

The new name keeps dashes, commas, periods, parentheses and apostrophes, since
those occur naturally in a phrase. Everything else is dropped, and each run of
dropped characters — underscores included — becomes one space. Capitalization is
left exactly as written. If the name is already taken, `-01`, `-02` and so on
are added to the root, so the extension stays intact.

It renames straight away without asking. Each new name is spoken as it happens,
and the cursor lands on the renamed file, so long as the filter in effect still
shows it. Everything else — which field each title came from, and why any file
was skipped — goes to the session log, which Control+F12 puts on your clipboard.

ExifTool comes with the media tools. Without it the command still works from the
text alone, and says so once.


## The Quick Folder

Your Quick folder holds shortcuts and web links you want to reach in one
keystroke. Press **Accent** to go there, or **Control+Q** to open it in a new
window.

Press **Shift+Q** for Quick Shortcut to add the file or folder you are on. Press
**Alt+Shift+Q** for Quick URL to add a web link: copy a link in any browser
first and the address fills itself in, with the site name offered as the name.
Both let you edit the name and the address before anything is written.

A name with punctuation in it is cleaned rather than refused, so "Q: what now?"
becomes "Q what now". A name Windows cannot use at all is refused with the
reason.

## Going Back

Press **Alt+LeftArrow** to return to the folder you were in before, and
**Alt+RightArrow** to go forward again. They work as they do in a web browser:
going somewhere new after going back discards the forward path, and a folder
that has been deleted since you were there is stepped over rather than stopping
you.

Press **Alt+R** for Recent Folders to pick from everywhere you have been this
session, each folder listed once with the most recent first.

Window cycling is **Control+Tab** and **Control+Shift+Tab**.

## Tidying a Folder

Three commands help with a folder that has collected copies and oddly sorted
names. None of them deletes anything: they tag, and Delete Tagged removes what
you tagged after asking.

Press **Alt+Shift+Period** for Tag Duplicate Files. It tags every file whose
content is identical to one already in the list, keeping the first. Identical
means byte for byte, so two files with different names but the same content are
caught, and two files that merely look alike are not.

Press **Alt+Shift+J** for Find Duplicates in Tree. This one looks at the current
folder *and everything under it*, and opens every duplicate it finds as a
virtual folder. That is an ordinary FileDir window, so you can move through it,
hear sizes and dates, read what is inside a file with Question Mark, open one to
check, tag a range with F8, and then Delete Tagged. Only the duplicates are
listed, never the first copy of anything, so tagging everything and deleting
leaves exactly one of each file on disk.

Press **Alt+Shift+Comma** for Tag Similar Files. It finds files that look like
other versions of the same one -- content.pdf beside content-1.pdf,
content_2.pdf and content (3).pdf -- and tags all but the largest, since a
partial download is smaller than the whole. A different extension is a different
file. Names like chapter1 and chapter2 are left alone: those are a book's
chapters, not copies.

Press **Alt+Shift+K** for Reorder Names. It renames files so an alphabetical
list reads in the order you mean. A single leading digit is padded with a zero,
so 2name comes before 11name instead of after it. ReadMe, index, introduction
and overview move to the top. Licence, contributing, change log and credits move
to the bottom. Every rename is shown before anything happens, and nothing is
overwritten.

## Converting Between Formats

Press **Shift+O** for Output Type. FileDir looks at what the file is, offers a
short list of what it can become, and converts the tagged files, or the current
one, keeping each root name. The result lands beside the original and nothing is
ever overwritten.

This was Output to Text, which only ever wrote a .txt file. Text is still one of
the choices; the rest were not available before.

What you are offered depends on what you are on:

- **Documents** — Word, OpenDocument, EPUB, HTML, Markdown, reStructuredText,
  LaTeX, rich text, CSV, PowerPoint, Excel — can become Word, a web page,
  Markdown, plain text, OpenDocument, rich text, EPUB, LaTeX, reStructuredText
  or MediaWiki.
- **Legacy Office files and PDF** — .doc, .ppt, .xls, .pdf — can become plain
  text or a web page.
- **Audio** can become MP3, M4A, WAV, FLAC, Ogg Vorbis or Opus.
- **Video** can become MP4, Matroska, WebM, QuickTime or AVI, or MP3, M4A or WAV
  for the sound alone.
- **Pictures** can become PNG, JPEG, WebP, BMP, GIF, TIFF, a Windows icon or
  AVIF. With the image tools installed, iPhone photos (HEIC), camera raw files,
  SVG drawings and icons can be converted too — ffmpeg cannot read any of those.

**Tables** — .inix records, .csv, .tsv, .xlsx and Markdown tables — convert
between each other keeping their rows and columns, and become Word documents,
web pages or OpenDocument files with the table intact. A spreadsheet can be read
but not written; save it as .csv, which every spreadsheet opens.

So converting a folder of MP4 files to MP3, or MKV to MP4, or PNG to JPEG, is
the same three keystrokes as converting Word to Markdown. The format you last
chose is remembered separately for each kind of file, so picking MP3 for audio
does not become the default the next time you tag a Word document.

Three programs do the work and you do not have to know which: **Pandoc** for
documents, **2htm** for the legacy Office formats and PDF that Pandoc cannot
read, and **ffmpeg** for audio, video and pictures. A file FileDir cannot
convert is skipped with a word saying so, and the closing count says how many
were written, skipped and failed.


## Playing Media

Press **Alt+Shift+L** for Play Media to hear something now without keeping a
list. It looks in three places, in this order:

1. **The clipboard**, when it holds a play list or web addresses. Copy a list of
   YouTube addresses from a mail message and press Alt+Shift+L, and they play in
   order with their titles. Anything mpv can reach works, because yt-dlp is
   handed the addresses.
2. **The tagged files**, if any are tagged.
3. **Everything playable in the folder**, in the order the window is sorted.
   Sort by date and it plays in date order, which is usually what a recording
   session or a downloaded series wants.

The clipboard is used only when every line is a web address or names a file that
exists. One line of ordinary prose and it is ignored, so the command never
surprises you with whatever you copied last.

**While the player is running, Control+V adds more.** mpv reads the clipboard
itself, so copy another address, press Control+V in the player window, and it
joins the end of the list. If nothing is playing it starts at once. That is
mpv's own key, not FileDir's, and it takes one address at a time; a whole list
is what Alt+Shift+L is for.

Other useful keys in the player window: space to pause, left and right arrows to
seek, angle brackets for the previous and next item, and q to stop.

Press **Control+Shift+L** for Play List. What it does depends on what is
selected, in the way you would want:

- **Files tagged** — it writes a play list of them and plays it.
- **Nothing tagged, and you are on a play list** — it plays that.
- **Nothing tagged, and you are on a sound or video file** — it plays that.
- **Nothing tagged, and you are on any readable document** — it looks inside for
  media links and plays those. Put the cursor on a directory of podcasts and
  press the key; no copying needed.

When the set is worth keeping, It writes
an .m3u beside the files and plays it. Move to a play list you made earlier and
press Control+Shift+L again to play that one rather than making another.

Neither command asks anything: they start playing, with sound and picture. The
player runs on its own, so FileDir does not sit waiting while you listen.
Its own keys work in its window: space to pause, arrows to seek, q to stop.

mpv is optional and the installer does not tick it. It is about 60 MB, and it
carries its own copy of ffmpeg which FileDir already has, so it is worth adding
only if you play media from the file list. Converting audio and video does not
need it.

## Downloading From the Web

Press **Alt+Shift+W** for Web Download and give an address. When yt-dlp is
installed, FileDir asks which of two things you want, and remembers the answer:

- **Download media from this page.** yt-dlp fetches the video, or the sound
  alone as an MP3, into the folder you are looking at. It knows a great many
  sites, picks the best streams and joins them.
- **List the files linked from this page.** The original behaviour: FileDir
  reads the page, shows you the links, lets you filter them by extension, and
  downloads the ones you pick.

The two are offered rather than guessed at, because a page holding a video has
no links to list, and a page of documents has no media to fetch. A wrong guess
costs either a large download or an empty list.

## Asking About a File

Two commands ask a language model running on this computer, and they use the
same keys as EdSharp so one habit serves both programs.

Press **F12** for Chat with AI. Type a question and get an answer. Nothing is
attached: this is for a question that has nothing to do with whatever the cursor
is on.

Press **Shift+F12** for Chat about File. Type a question and the text of the
current file travels with it, converted from whatever format it is in, so a Word
document or a PDF works as well as a text file. Summarize this, list the dates
in it, what is this about.

Either way the answer opens in a window with the text in a box you can move
around. Arrow through it line by line, select part of it, copy with Control+C.
Leave it with the Spacebar, Enter, or Escape, whichever is nearer your hand,
without moving off the text first.

The question you last asked is remembered. A file too long to send whole is
trimmed, and the answer says so rather than letting a partial answer look
complete.

These need Ollama, the same installation the Translate File command uses.

**A note on the keys.** The whole F12 row held the Timer commands here
for twenty years. They moved to **Alt+Control+T**, **Alt+Control+S** and
**Alt+Control+Y** so that FileDir and EdSharp agree about the F12 column.
Matching the sibling program is worth more than protecting a habit that few
people had, now that a phone or a smart speaker sets a timer better than a file
manager does.


## Translating Files

FileDir can translate the text of your files into another language, using a
language model running on your own computer. Nothing is uploaded and no part of
any file is sent anywhere, so this is safe for a document you would not paste
into a web page.

Press **Alt+Shift+F7** for Translate File. Name the language you want, and
FileDir works through the tagged files, or the current one if nothing is tagged.
For each file it reads the text, translates it, and writes the result beside the
original as `<name>.<language>.txt`. Nothing is overwritten: if that name is
taken, FileDir picks another.

It reads the same formats the Say Contents command reads: Word, PDF,
PowerPoint, Excel, Markdown and plain text. So you can translate a folder of
Word documents without opening any of them.

**How FileDir reads a file, and what it needs.** A file that is already text is
simply read. Word, OpenDocument, EPUB, web pages, rich text, Markdown,
reStructuredText, LaTeX and CSV are read by Pandoc, which is free and comes with
the installer. PowerPoint and Excel files are read by FileDir itself, straight
out of the file. PDFs are read by PyMuPDF4LLM, which comes with the installer and needs no Word:
it reads the PDF's own structure, so headings stay headings, lists stay lists
and tables stay tables. Only the older .doc, .ppt and .xls formats need
Microsoft Office installed. If a file
cannot be read, FileDir says which tool it tried and why, rather than going
quiet.

Because a model takes its time, FileDir says which file it is on and which part
of that file, so you can tell it is working rather than stuck.

### What You Need

The translation is done by Ollama, which is free and separate from FileDir. The
FileDir installer offers it as a checkbox, along with a second checkbox for a
larger model that translates better. Neither is ticked by default: together they
are several gigabytes, which is not something to download by accident.

- **llama3.2**, about 2 GB, comes with the Ollama checkbox. It translates
  passably and is quick.
- **qwen2.5:7b**, about 5 GB, is a separate checkbox. It translates noticeably
  better.

FileDir uses qwen2.5:7b if you have it and llama3.2 otherwise. There is nothing
to configure: it asks Ollama what is installed and picks the better one.

You can add either later by running `installOllama.cmd` or
`installTranslateModel.cmd` in the FileDir folder. If you already run Ollama for
EdSharp or DbDo, FileDir uses that same installation and those same models --
one copy serves every program on the machine.

If Ollama is not running when you press Alt+Shift+F7, FileDir says so and tells
you what to run, rather than failing quietly.

## Hotkey Summary

Every FileDir command, its key, and what it does are listed in a separate
document, [Hotkeys](Hotkeys.htm), which comes with the program and is on the
Start menu as "FileDir Hotkeys". It gives the same list three ways: in order of
command name, in order of key, and grouped by the modifier a key starts with.

Three commands help while you are learning:

- **Hotkey Summary**, Alt+Shift+H, opens that same document from inside FileDir.
- **Key Describer**, Control+F1, turns on a mode where pressing a command key
  says the command's name, its key, and what it does, instead of running it.
  Press Control+F1 again to turn it off. This is the fastest way to explore.
- **Alternate Menu**, Alt+F10, lists every command in one alphabetical list. You
  can filter that list with Control+F and jump within it with Control+J, then
  press Enter to run the command you land on.

## Logs

FileDir keeps a log of every session, at

```
%LOCALAPPDATA%\FileDir\logs\FileDir_<date>_<time>.log
```

It opens with the version, the program path, the command line and the machine,
and then records every outside program FileDir runs -- Pandoc, ffmpeg, ExifTool,
yt-dlp, 2htm -- with its exit code and, when one fails, the first line of what
it said. So a conversion that did not work can be explained rather than guessed
at.

The newest thirty session logs are kept and older ones are removed, so the
folder never grows without limit.

Press **Control+F12** for Copy Log. It puts the path of this session's log on
the clipboard twice over: as a file, so pasting into a new mail message attaches
the log itself, and as text, so any program that only reads clipboard text gets
the path. That is the whole of "send me the log".

The installer writes its own log in the same folder, `FileDir_setup.log`, and
the Results box shown at the end of the installation names it. EdSharp uses the
same folder shape, the same file naming, and the same Control+F12.

## Development Notes

FileDir is written in C# and runs on the .NET Framework 4.8, which is part of
Windows 10 and Windows 11. It is built with the Roslyn compiler from Microsoft.
The program is one file, FileDir.exe, and needs nothing installed alongside it.

Text is pulled out of documents by 2htm, a separate tool by the same author,
released under the MIT licence, which turns Word, Excel, PowerPoint, PDF, and
Markdown files into accessible HTML or plain text. See
[2htm on GitHub](https://github.com/JamalMazrui/2htm).

The source code comes with the program, in the FileDir folder: FileDir.cs and
Dialogs.cs for FileDir itself, and the shared Homer files Say.cs, Inix.cs,
Web.cs, Util.cs, KeyMap.cs, and Lbc.cs, which EdSharp and DbDo use as well.
Full instructions for rebuilding or changing FileDir are in
[Developer](Developer.htm).

FileDir is free and open source under the MIT licence. See
[License](License.htm) for the terms. The project home, including every
release, is [FileDir on GitHub](https://github.com/JamalMazrui/FileDir).

Feedback helps FileDir improve. When you report a problem, the more detail the
better, especially the steps that lead to it.

Jamal Mazrui

End of Document
