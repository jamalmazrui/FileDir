# FileDir — User Guide

**Version 5.0 beta**  
June 2026  
Copyright 2006-2026 by Jamal Mazrui  
Modified GPL License

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
- [Web Client Utilities](#web-client-utilities)
- [Miscellaneous Commands](#miscellaneous-commands)
- [Hotkey Summary](#hotkey-summary)
- [Development Notes](#development-notes)

## Installation

The installation program for FileDir is called dirsetup.exe. When executed, it prompts for a program folder, the default being

```
C:\Program Files\FileDir
```

The installer also creates a program group for FileDir on the Windows start menu, containing choices to launch FileDir, read Documentation, and uninstall. Additional choices either set or clear an association between FileDir and folders or the .zip extension when opened by other programs. This setting also causes FileDir to opan a view of a zip archive after downloading it with Internet Explorer and choosing the Open button from the "Download Complete" dialog.

The FileDir installer creates a desktop shortcut with a hot key, enabling the program to be conveniently launched by pressing Alt+Control+F. If this hot key happens to conflict with an existing one, navigate to the FileDir item on the desktop, press Alt+Enter for properties, and then change the hot key to something else (or leave it blank).

The FileDir setup program checks whether the required .NET Framework 4.8 is already installed (it ships with current versions of Windows), and if not, lets you conveniently do so. After installing FileDir, the setup program presents a list of two checkboxes that are on by default. The first checkbox offers an an optional set of JAWS scripts to fine tune the FileDir speech interface in a few ways that could not be accomplished otherwise. Mainly, these scripts suppress the often unnecessary verbalization of keystroke names, such as "Shift S," leaving just the command name if appropriate, such as "Size." If the scripts were installed and you would later prefer default JAWS behavior instead, however, you can do this by pressing Insert+0 when FileDir is active, and then down arrowing to the following line:

```
;SwitchToConfiguration("default")
```

Delete the initial semicolon character (;), which uncomments the code, and then press Control+S to save and recompile the scripts. Press Alt+F4 to exit JAWS script manager.

If you prefer not to install the JAWS scripts in the first place, e.g., because you use a screen reader other than JAWS, press Spacebar to uncheck that option of the setup program. The second checkbox in the list, available via DownArrow, offers FileDir documentation in the default web browser (usually Internet Explorer).

FileDir may be safely installed over previous versions. The About option from the Help menu, or Alt+F1 key, indicates the current version number and release date. The Change History option, Shift+F1, summarizes fixes and improvements over time.

## Introduction

FileDir is a file and directory manager developed in the C# language, which requires the .NET Framework 4.8 to run. Current versions of Windows include it; on older systems it is a free Microsoft download from [Microsoft .NET Framework download](https://dotnet.microsoft.com/download/dotnet-framework)

FileDir is designed to be a powerful, efficient, and convenient alternative to Windows Explorer or My Computer for managing files and folders on a Windows-based computer. Almost every command can be done through a mnemonic keystroke, as well as a menu or mouse operation. These commands begin with those available in Windows Explorer. FileDir then adds several beneficial features above this base command set. Finally, a set of optional JAWS scripts provide further fine tuning of the speech interface.

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

Press Control+W to load the current file into a word processor. Microsoft Word is the default, but a different one may be configured with the Configuration Options command, Alt+Shift+C. Press Control+T to open it in a text editor instead, the configurable default being EdSharp, available at [EdSharp text editor](http://www.EmpowermentZone.com/edsetup.exe)

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

If you would prefer Go To behavior without having to press Shift, then toggle the Recycle with Delete setting with Alt+Shift+R. The more destructive setting is also more convenient (no delay in waiting for items to be copied to the recycle bin). It requires a deliberate change from the default to make sure that is how you want to operate. In that case, the Shift version of a command is for Open rather than Go To (similar to how Shift+Enter on a link opens a new window in Internet Explorer, whereas Enter goes to it).

Press backslash (\) to go to the root level of the current drive. Press comma (,) or Backspace to "come up" a level in the folder tree, going to the parent of the previous folder. Press the Period (.) or F5 key to refresh the current folder. This may be needed if items on disk were changed in a way that FileDir does not automatically track.

When the current item is a folder, pressing Enter goes to it. When it is a zip archive, FileDir presents a view of the contained items that is similar to a directory view. If you would prefer to open a .zip file with another program associated with that extension (e.g., WinZip), change the ZipOpener configuration option to N for No. Also, you could normally view archives with FileDir by pressing Enter, but choose to open them with another program instead by pressing Shift+Enter. This opens them with the default program associated with the extension in the Windows registry.

The Quick Links feature efficiently opens favorite files, folders, or URLs. Press Shift+Q to add a quick link for the current item. A standard Windows shortcut (.lnk file) will be created in the Quick subfolder of the FileDir program folder. FileDir lets yu rename the shortcut before creating it.

Use the Quick URL command, Alt+Shift+Q, to create a quick link to an Internet resource. FileDir prompts for the name and URL, and then creates a standard .url file in the Quick folder. If Internet Explorer is open, FileDir attempts to get default values for the name and URL based on the title and address of the web page last opened. For this to work, the Address Bar setting should be checked on the View menu of Internet Explorer. Sometimes, FileDir cannot retrieve the values anyway, so you need to manually type or paste them into the edit boxes.

Press Control+Q to open the Quick folder, or the grave accent key (`) at the far left of the numeric row (U.S. keyboard) to go to it. You can navigate the Quick folder like any other. Press Enter to execute a quick link. Press Alt+Enter to review or modify the settings of a .lnk file. a .url file is editable text in the standard .ini format.

Alt+R lists recent folders -- every folder or zip archive opened since the start of the current FileDir session, with the most recent first. Choose an item from this standard listbox to open it. Use the Windows Toggle command, Shift+W, to switch between a pair of directory views that you are working with. Each presss returns to the previous window viewed.

Nine commands on the Window menu let you quickly open or go to an existing view on a drive. Drives A through I are associated with the digits 1 through 9. For example, press Alt+1 to go to Drive A or Alt+3 to go to Drive C.

In general, FileDir checks if you are trying to open a folder that already has an open window. If so, it says "Returning and activates that window rather than creating a new one. Press F4 to pick one of the currently open windows from a standard ListBox. Alternatively, Control+Tab or Alt+RightArrow activates the next open window, and Control+Shift+Tab or Alt+LeftArrow activates the previous one. Press Shift+F4 or Alt+NumPad5 to hear the number and titles of all open windows. Press Control+F4 to close the current window, or Control+Shift+F4 to close all windows except the current one. Alt+F4 exits FileDir, and Alt+Shift+F4 restarts Windows (after confirming).

As you type in the edit box for specifying a folder, it guesses the input desired based on existing paths -- similar to how Internet Explorer guesses URLS as you type. If the path you ultimately entered is not found on disk, the dialog prompts whether to create it. This makes it convenient to copy, move, or unzip files to a new folder with a single command.

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

Starting with FileDir 3.7, the unzip commands are now broader, unarchive commands that work with almost any archive format, including .rar, .tar, .gz, .bz2, .chm, .cab, and .wepm (a Window-Eyes script package that is the same format as .cab). FileDir does this with the free 7Zip utility behind the scenes, which is also available independently at [7-Zip](http://7zip.com)

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

Press Shift+O to output tagged files in plain text format. The original, source files will not be affected. The target, converted files will have the same names but a .txt extension. Conversions to text are available for the following formats: .doc, .htm, .pdf, .ppt, .rtf, and .xls. Some conversions require Windows 2000 or above.

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

The Web Download command, Alt+Shift+W, lets you pick one or more files to download from a page whose address you specify. If Internet Explorer is open, FileDir uses the value in its address bar as the default. Each item of the resulting checked listbox shows both the clickable text of the url and its target file name. Press Spacebar to toggle the checked state of an item. After picking files, you are prompted for the target folder on disk. If the URL of a link does not end in a valid file name, FileDir creates a file name for the target on disk based on other characters in the URL. If a file with the same name already exists, a unique name is created by adding a numeric suffix, e.g., page_001.htm, page_002.htm, etc.

A listbox control of the .NET Framework does not support multiple letter navigation, so each letter typed jumps to the next item starting with that letter. To make navigation more flexible and efficient, particularly in a listbox with many items, EdSharp adds the following features to a list based dialog. Control+J prompts for text within an item, going to the first match if a new search, or the next match if the previous value is accepted. Alt+J goes to the next match without prompting for a value. The item with focus when the dialog is closed -- but not canceled -- becomes the current item the next time that the same list dialog is invoked (you are notified when it is not the first item). The Jump value of that dialog is also remembered.

Control+F sets a filter to restrict what items are shown via wildcards (* to match any sequence of characters or ? to match a single one). For example, you could browse replace-related commands in the Alternate Menu, Alt+F10, by pressing Control+F after invoking that list and then entering *replace* as the filter expression. Control+Shift+F clears the filter so all items are shown again. The order of items may also be changed: Alt+A for alpha order, Alt+Shift+A for reverse alpha order, Alt+D for default order, or Alt+Shift+D for reverse default order.

Multiple commands support flexible checking or unchecking in a checked listbox such as the Web Download dialog. Press Space to toggle the checked state of the current item, Control+A to check all items, or Control+Shift+A to uncheck all. Press Shift+DownArrow for check and Next, or Shift+UpArrow for check and Previous. Press Shift+End for check to Bottom, or Shift+Home for check to Top. Shift+NumPad5 checks the current item. F8 marks the start of a checking operation, completed with Shift+F8.

Adding the Alt modifier key performs the same action except for uncheckging rather than checkging. Thus, Alt+Shift+NumPad5 unchecks the current item, Alt+Shift+Home unchecks to the top of the list, Alt+Shift+End unchecks to the bottom, Alt+Shift+DownArrow unchecks en route to the next item, and Alt+Shift+UpArrow unchecks en route to the previous. F8 then Alt+Shift+F8 unchecks items in that range.

Other arrow keypad actions navigate among checkged items. Control+Home goes to the top checkged item, and Control+End goes to the bottom one. Control+DownArrow goes to the Next , and Control+UpArrow goes to the previous.

Shift+Space tells you what items are currently checked. Alt+A says the address of the current item in the list, e.g., 11 of 42.

## Web Client Utilities

The Web Client Utilities command, Alt+Shift+Space, is similar to the Research It command of JAWS, Insert+Space. The utilities are handy ways of getting useful information from free web 2.0 services. The following 35 utilities are installed (for efficient navigation in the listbox, each has a unique initial character, which may be a letter, digit, or symbol):

!Odd News - Get recent news items that are strange but true via reuters.com.

@DomainLookup - Get authoritative information about the registrant of an Internet domain name, e.g., AccessibleWorld.org. Note that some firewalls block this utility because it uses a different port than standard HTTP requests. This uses the free WhoisThisDomain utility from [NirSoft utilities](http://nirsoft.net/utils/)

#SportScores - Pick a sport from a list and go to the corresponding page on ESPN.com with recent news and scores.

$Product Search - Enter keywords that describe a product and go to its matching web page on amazon.com.

-TimeInternational - Enter a location (e.g., specified as city, country) and get the current time there via google.com.

=UnitConversion - enter a source value, e.g., 10 miles, and a target unit, e.g., kilometer, and get the converted result via google.com. This works for currency units as well as physical measurements.

1 Mile Stories - Get recent blog or news stories about a location and surrounding area within a one mile radius via the outside.in web service. Enter a location such as your home address on one line by using a comma and space between segments that you might otherwise type on separate lines.

508 Check - Check a web page for compliance with accessibility standards of the United States Government (Section 508 of the Rehabilitation Act), as well as standards of the World Wide Web Consortium (W3C). This checks a web page against 508 standards via CynthiaSays.com (the site limits checks to one per minute from the same client). It also includes the report of Wave, a web evaluation checker from WebAim.org.

0Captcha - Submit a captcha to solona.net ( a free account is required), and have the text solution copied to the clipboard so you can paste it into a web form. The utility waits up to 90 seconds for a human operator to respond. This utility is typically executed from within a browser that is displaying a captcha, though any .png file on disk may be submitted.

Address Lookup - Search for addresses of organizations meeting geographic and other criteria via jigsaw.com. This prompts for an organization name, area code, zip code, web site type, and fortune rank. Fill in one or more fields for the search. For example, input gov for the web site type in order to get government organizations, or 500 in the fortune field to get companies in the top fortune 500.

Business Reviews - Search for reviews of a business specified by a phone number via yelp.com.

Common URLs - Show a list of the 100 most commonly referenced URLs on Twitter at present via TweetMeme.com. These typically point to news stories that people have been retweeting.

Driving Directions - Input a starting and ending location, and get a list of steps to get there by car (a blind person might share this with a friend or cab driver). The location may be specified as a street address in any country. The utility prompts for a starting and ending address, uses the Google Maps API, and puts the estimated distance, duration, and steps in the viewing area. Specify an address as if you were addressing an envelope except for a comma rather than return between each line, e.g.,

1400 East-West Highway, Silver Spring MD 20910, USA

USA is assumed as the country if not specified.

EnglishDictionary Lookup - Get definitions and other information about a word via wiktionary.com.

Feed Find - Get a list of RSS and ATOM feeds made available by a web site. This prompts for a web source and returns a list of RSS or ATOM feeds found. An `http://` prefix is assumed if not specified. For example, entering

cnn.com

finds two RSS feeds related to top stories and latest stories. You can open a feed url to read recent content, or subscribe with a feed reader for regular updates.

Google Search and Set Suggestions - Propose a Google search and get a list of popular searches that are similar. Also Get a list of terms that may be part of the same set. For example, enter a comma-separated list of U.S. presidents and let Google suggest a more complete list.

Horoscope Reading - Input a zodiac sign (e.g., Sagittarius) and get a horoscope for today via my.horoscope.com.

Interesting Places - Get a list of nearby places to eat, shop, or visit via NextStop.com. This prompts for a location, which can be in the city, state format, or a complete address with commas seperating postal address lines. Also input the distance in kilometers to search from that location, and any words that you want to narrow the search, e.g., Chinese for that type of food. An excerpt from a review of each place is also included, if available.

Journalist World Reports - Get world headlines from multiple web sources: the BBC, CNN, Christian Science Monitor, New York Times, Reuters, and Yahoo. A structured text file is generated containing a section of news items from each source. Each item has a title, summary, and URL for the full article.

KnowledgeWikipedia - Input a topic and get a Wikipedia article as both a web page and a text file.

Language Translation - Translate text you specify, between about 100 different languages. You can quickly understand what a foreign phrase means or how to write it. This uses the Google Translate API to translate text among about 100 natural languages. By default, the choice for the source language is unknown and automatically inferred by Google. You pick the target language, and either enter or paste text in the multi-line edit box.

Members of Congress - Based on a U.S. zip code, get a list of House and Senate members with various data including committee assignments and contact information via SunlightLabs.com.

Neighborhood Search - Search for places near a location, e.g., restaurants with a particular cuisine near an address you are visiting (anywhere in the world). This prompts for an address in the same format as Driving Directions and also for one or more keywords specified as if searching on Google, e.g.,

seafood steak

to find restaurants in the area that serve both seafood and steak.

Original URL - Get the original version of a URL, e.g., one that was shortened for sharing in a tweet. This does the reverse of the Short URL utility, prompting for a URL, converting it, and copying the result to the clipboard.

Physician Online - Enter a medical topic and go to a matching web page on WebMD.com.

Quotes of the Day - Get daily food for thought from famous quotes and their authors. This Shows a humorous quote from IHeartQuotes.com, as well as several motivational quotes from QuotationsPage.com.

Recommended URLs - Based on a topic word, get a list of currently popular URLs that people are saving as bookmarks via delicious.com.

Short URL - Get a shortened version of a URL via j.mp.com, e.g., so you can share it in a tweet and have more text to type within the 140 character limit.

Trend Topics - Get a list of currently popular topics on Twitter via LetsBeTrends.com.

Url Downloads - Batch download multiple urls based on an initial page address and the extensions of files linked to it. This puts a space-separated list of extensions found in an input box. Edit it so that only the extensions you want remain. The utility then puts a list of those links in a multiple-selection listbox, showing the link text and URL for each. The items are all selected by default, but you can use arrow keys and Spacebar to unselect ones as desired. The next dialog prompts for a folder for saving the files, which will be remembered as the default choice the next time. The utility says the name of each file as it is being downloaded.

Virtual White Pages - Search the white pages of U.S. phone books for listings of residential phone numbers and postal addresses via WhitePages.com.

Weather Check - Get a summary of current and forecasted conditions for any location via wunderground.com. This works with city, country locations as well as U.S. zip codes.

Xtra Word Info - Get definitions, usage examples, and origins of a word. This shows definition and examples via Wordnik.com; synonyms and antonyms via words.BigHugeLabs.com; and etymology via etymonline.com.

Yahoo! Term Extractions - Get noteworthy noun phrases contained in a web page via yahoo.com.

Zoom Info - Search for employment contacts by name or email address via ZoomInfo.com.

Each web client utility is defined in a script file written in the Python language. The file name begins with the WebClient_ prefix and ends with the .py extension, e.g.,

WebClient_508Check.py

The Python script file is run with a custom Python interpreter, InPy.exe, which is also available separately at [InPy package](http://EmpowermentZone.com/InPy.zip)

That package also includes a Console mode version, InPyC.exe, to aid development of Python scripts. If run without parameters, it opens an interactive shell for testing commands and running scripts, similar to the full python.exe interpreter. The source code file, InPy.py, imports various web2.0 libraries, and defines many convenience functions.

Web client utilities may be added to the list of available ones by using the same naming convention, e.g.,

WebClient_MyNewScript.py

The Web Client Utilities command remembers the last utility you chose, making it the default in the list. The information obtained by the utility is automatically saved in a text file and opened in your text editor (the program associated with .txt files in the Windows registry). Some utilities also open a web page automatically in your default browser.

Note that, at this point, there is not much error checking in the web client utilities, so if a web service returns no useful information for a particular query, a utility may show an error message rather than indicating that no information was available. If you consistently get errors regardless of the search parameters, please email the error log file, InPy.exe.log, located in the WebClient subfolder of the program folder. Errors get appended to this file, so if you want to isolate the error message for a particular utility, delete the log file before running it.

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

F12 related keys provide timer and alarm features (you may associate the number 12 with a clock). Press F12 to start a timer. FileDir prompts for the announcement interval and stop time. The announcement interval , measured in seconds, is how often FileDir will announce the amount of time elapsed since the start of the timer, e.g., a value of 60 means to announce at minute intervals. These verbal announcements occur regardless of what program is currently in the active window. Use a blank or 0 value to run the timer without automatic announcements. Press Alt+F12 at any time to check how much time has elapsed so far. If a timer is already running, the F12 key pauses it. If paused, F12 resumes. Press Shift+F12 to stop the timer and hear the total time it was running.

In the dialog that prompts for the announcement interval, another field is the stop time. A blank or 0 value means that the timer will run until manually stopped by pressing Shift+F12 or exiting FileDir. Instead, a stopping point may be specified as a date and time. The date and time components are each optional. If a date is used, it must include at least the month and day, separated by the forward slash character (/) -- or equivalent for non-U.S. formatting conventions. If a time is used, it must include at least the hour and minute, separated by a colon character (:) -- or non-U.S. equivalent. If both date and time components are used, type the date, a space, and then the time. Without a time, today's date is assumed. A time may use either the military, 24-hour convention, or the AM/PM suffix (otherwise AM is assumed if the hour is less than 13). Examples of valid date/time values are as follows:

2:00 PM

14:00

7/27 6:30

2007/7/27 6:30:15

When the stop time is reached, FileDir plays some chimes and ends the timer. Such an alarm may be used either with or without intervening announcements of time intervals. A timer runs independently of other FileDir operations, so you can continue working in FileDir while using this capability.

Use the Play List command, Control+Shift+L, to create a .m3u file with references of tagged items to play sequentially. Types may include .mp3, .wav, or .cda (the extension of a track on a standard audio CD). FileDir prompts for the name of the play list to create, defaulting to PlayList.m3u in the current directory. Focus is then placed on that file (if in the same directory), so you can simply press Enter to execute the play list. Note that if you want to play tracks on an audio CD, however, you need to save the play list in another directory that permits the creation of new files.

Use the Environment Variables command, Control+E, to review or change such settings of Windows. Choose those of the current process, user, or system as a whole. Jump quickly to a particular variable based on its initial letter, e.g., Alt+P for the PATH setting that determines where Windows searches for an executable file that is not found in the current directory. Changes to process settings affect the current session of EdSharp, but not the next time it is run. User settings take effect when you log in again. System settings take affect when you restart the computer.

FileDir windows may be visually organized according to common MDI (multiple document interface) patterns. The Window menu includes the following commands: Arrange Icons, Alt+F11; Cascade, Control+F11; Tile Horizontal, Alt+Shift+F11; and Tile Vertical, Control+Shift+F11.

Use the Elevate Version command, F11, to download and install the latest version of FileDir. You are prompted for confirmation. The installer is downloaded to the folder for temporary Internet files so it will be deleted automatically when Windows reclaims space in that folder. The current FileDir version is then unloaded so that the installer can replace any files that were in use. You can reload the updated version in the usual manner after installation, e.g., by pressing Alt+Control+F.

## Hotkey Summary

| Command | Keystroke | Description |
|---|---|---|
| Launch FileDir | Alt+Control+F | Launch or activate the FileDirapplication via a Windows desktop shortcut |
| Open Item | Enter | Open subfolder in new window, view zip archive, or launch file |
| Go to Item | Shift+Enter | Go to subfolder in same window, view zip archive, or launch file |
| Properties | Alt+Enter | Invoke Windows properties dialog for current item |
| Open Parent Folder | Backspace | Open parent folder in new window and jump to folder item that was previously open |
| Go to Parent Folder | Comma or Shift+Backspace | Go to parent folder in same window ("come up level") |
| Toggle Tag | Space | Invert tagged state of current item |
| Say Selected | Shift+Space or JAWSKey+Shift+DownArrow | Say tagged items or current one if no tags |
| Append to Clipboard | Shift+A | Append textual content of current or tagged files to the clipboard |
| Convert Encoding | Control+2 | Convert character encoding of current or tagged files |
| Extract with Regular Expression | Control+Shift+E | Extract text from tagged files with regular expression and copy to the clipboard |
| Start Tag or Untag | F8 | Mark start of sequence to be tagged or untagged |
| Complete Tag | Shift+F8 | Complete tagging |
| Complete Untag | Alt+Shift+F8 | Complete untagging |
| Tag All | Control+A | Tag all items |
| Untag All | Control+Shift+A | Untag all items |
| Alpha Order | Alt+A | Sort items in alphabetic/name order |
| Reverse Alpha Order | Alt+Shift+A | Sort items in reverse alphabetic/name order |
| Beginning Tagged | Shift+B or Control+Home | Go to beginning tagged item |
| Recycle Bin | Control+B | Open Windows Recycle Bin to recover deleted items |
| Batch Mail | Control+Shift+B | Send a message individually to multiple recipients |
| Beginning File | Alt+B | Go to beginning file item, skipping over folder items |
| Burn to CD | Alt+Shift+B | Add current or tagged items to CD |
| Copy to Folder | Shift+C | Copy current or tagged items to another folder |
| Copy | Control+C | Copy current or tagged items to clipboard (listing paths in both binary and text formats) |
| Copy Append | Alt+C | Copy and append current or tagged items to clipboard (listing paths in both binary and text formats) |
| Copy Name | Control+Shift+C | Copy name of current or tagged items to clipboard |
| Say Date | Shift+D | Say date and time of current item |
| Date Order | Alt+D | Sort items in date/time order |
| Reverse Date Order | Alt+Shift+D | Sort items in reverse date/time order |
| Delete and Recycle File Now | Control+D | Delete current file item and recycle without confirmation |
| Delete File Now | Control+Shift+D | Delete current file item permanently without confirmation |
| End Tagged | Shift+E or Control+End | Go to end tagged item |
| Environment Variables | Control+E | Change Windows environment variables for the current process, user, or system |
| Evaluate Expression | Control+Equals | Evaluate mathematical expression and copy result to clipboard |
| Export Clipboard to File | Alt+Shift+E | Export clipboard text to disk file |
| FTP Put | Shift+F | Upload current or tagged files to FTP directory |
| Set Filter | Control+F | Set filter with wildcards to view a subset of items |
| Clear Filter | Control+Shift+F | View all items |
| File Find | Alt+Shift+F | Find file in current folder or subfolders based on match of textual content and name filter |
| Get FTP | Shift+G | Download files from FTP directory |
| Web Download | Alt+Shift+W | Download files from a web page |
| Web Client Utilities | Alt+Shift+Space | Run a web client utility |
| Go to Folder | Control+G | Go to folder in same window |
| Go to Special Folder | Control+Shift+G | Pick special folder (e.g., My Documents) to open in same window |
| Go to Virtual Folder | Alt+Shift+G | Open a virtual folder definition in same window |
| Go to Drive | Alt+G | Pick drive to open in same window |
| Initial Change | Shift+I | Go to next item with different initial letter |
| Invert Tagged | Control+I | Invert tagged and untagged state of items |
| Iterate Processes | Alt+I | List running processes and activate or terminate |
| Inquire Differences | Alt+Shift+I | Generate a report that compares files in two folders |
| Hotkey Summary | Alt+Shift+H | Display list of FileDir keys, command names, and descriptions |
| Jump | Control+J | Jump to item based on a string within its name |
| Jump Again | Alt+J | Repeat Jump command with same string |
| Keywords | Control+K | Jump to item based on a string within its content, optionally with multiple match conditions |
| Keywords Again | Alt+K | Repeat Keywords command with same string |
| List | Control+L | Say items in current folder |
| List Tagged | Shift+L | Say tagged items in current folder |
| List Files | Alt+L | Say file items (but not folder items) in current folder |
| Play List | Control+Shift+L | Create .m3u play list containing tagged items |
| Move to Folder | Shift+M | Move current or tagged items to another folder |
| Mail Body | Control+M | Mail textual content of current file as the body of an email message |
| Mail Attachment | Control+Shift+M | Mail current or tagged files as attachments to an email message |
| Manual Options | Alt+Shift+M | Adjust FileDir configuration options in text editor |
| Next Tagged | Shift+N or Control+DownArrow | Go to next tagged item |
| New Folder | Control+N | Create new folder on disk |
| New Item Copy | Control+Shift+N | Create copy of current file or folder item with similar name and numeric suffix |
| Network Connections | Alt+Shift+N | Connect, disconnect, or restore mappings between physical storage and logical drives |
| Output to Text | Shift+O | Output textual content of current or tagged files to files with same names but .txt extensions |
| Configuration Options | Alt+Shift+C | Configure FileDir options |
| Open Folder | Control+O | Open folder in new window |
| Open Special Folder | Control+Shift+O | Pick special folder (e.g., My Documents) to open in new window |
| Open Virtual Folder | Alt+Shift+O | Open a virtual folder definition in new window |
| Open Drive | Alt+O | Pick drive to open in new window |
| Previous Tagged | Shift+P or Control+UpArrow | Go to previous tagged item |
| Print | Control+P | Print current or tagged files |
| Path List to Clipboard | Control+Shift+P | Copy to clipboard file paths below current folder item |
| Say Path | Alt+P | Say full path of current item |
| Path to Clipboard | Alt+Shift+P | Copy full path of current item to clipboard |
| Short Path to Clipboard | Tilde | Copy short path of current item to clipboard |
| Quick Shortcut | Shift+Q | Create .lnk file for current item in Quick folder |
| Quick URL | Alt+Shift+Q | Create .url file for Internet resource in Quick folder |
| Open Quick Folder | Control+Q | Open folder of quick links |
| Go to Quick Folder | Accent | Go to folder of quick links |
| Rename | Shift+R or F2 | Rename current item |
| Rename with Wildcards | Control+R | Rename all items in current folder with wildcards |
| Rename with Regular Expression | Control+Shift+R | Rename current or tagged items with regular expressions |
| Rename to Initial Line | Control+Shift+I | Rename current or tagged files to initial line of text within them |
| Recent Folders | Alt+R | Pick a recent folder or shortcut to open |
| Recycle Toggle | Alt+Shift+R | Toggle On/Off setting for whether deleted or replaced items are moved to the recycle bin |
| Say Size | Shift+S | Say size of current item |
| Size Order | Alt+S | Sort items in size order |
| Reverse Size Order | Alt+Shift+S | Sort items in reverse size order |
| Save Tags | Control+S | Save which items are tagged in current directory view |
| Restore Tags | Control+Shift+S | Apply previously saved tags |
| Say Type | Shift+T | Say type/extension of current item |
| Type Extended | Control+Shift+T | Show all extended properties of current item |
| Type Order | Alt+T | Sort items in type/extension order |
| Reverse Type Order | Alt+Shift+T | Sort items in reverse type/extension order |
| Send to Text Editor | Control+T | Send current file to text editor (default is EdSharp) |
| Unarchive | Shift+U | Unzip current or tagged files |
| Unarchive without Subfolders | Control+U | Unzip current or tagged files without subfolder paths |
| Unarchive to Same Name | Control+Shift+U | Unzip current or tagged files to a directory named like the archive |
| Unarchive Test | Alt+U | Check if current file item can be unzipped successfully |
| Unarchive Password | Alt+Shift+U | Set password to be used when creating, extracting, or viewing zip archives |
| Paste | Control+V | Paste items from clipboard to current folder |
| Volume Format | Control+Shift+V | Format a disk or storage card |
| Paste Copy | Alt+V | Copy items listed on clipboard to current folder |
| Paste Move | Alt+Shift+V | Move items listed on clipboard to current folder |
| Say Windows Open | Shift+F4 or Alt+NumPad5 | Say titles of open windows |
| Send to Word Processor | Control+W | Send current file item to word processor (default is Microsoft Word) |
| Windows Control Panel | Control+Shift+W | Launch Control Panel to configure Windows |
| Extension Change | Shift+X | Go to next item with different extension |
| Cut | Control+X | Cut current or tagged items to clipboard (listing paths in both binary and text formats) |
| Extra Speech Toggle | Control+Shift+X | Toggle extra speech messages on or off, redirecting them to a log file |
| Extra Speech Log | Alt+Shift+X | Open speech log file in configured text editor |
| Yield | Control+Y | Say count and size of items in current folder |
| Yield Tagged | Shift+Y | Say count and size of tagged items in current folder |
| Yield Files | Alt+Y | Say count and size of file items (but not folder items) in current folder |
| Yield on Drive | Control+Shift+Y | Say total size and bytes free on current drive |
| Yield in Operating System | Alt+Shift+Y | Say Windows version, physical memory, and virtual memory |
| Zip | Shift+Z | Add current or tagged files to zip archive |
| Zip then Delete | Control+Z | Add current or tagged files to zip archive, then delete originals |
| Zip List | Control+Shift+Z | Create or update a zip archive based on a list of files or folders |
| Say Status | Alt+Z | Say status line, containing date and time of current item, its size, the sort order, and filter specification (if any) |
| Delete | Delete | Delete current or tagged items and recycle according to setting |
| Delete without Recycle | Shift+Delete | Delete current or tagged items permanently |
| Delete and Recycle | Control+Delete | Delete and recycle current or tagged items |
| Parent Folder | Backspace | Go to parent of current folder |
| Come up Level | Comma or Backspace | Go to parent folder in same window and jump to folder item that was previously open |
| Refresh Folder | Period or F5 | Read current folder again from disk in same window |
| Open Root Folder | Backslash | Open root folder of current drive in new window (e.g., the C:\ folder) |
| Go to Root Folder | Shift+Backslash | Go to root folder of current drive in same window |
| Tag | Semicolon or Shift+NumPad5 | Tag current item |
| Untag | Slash or Alt+Shift+NumPad5 | Untag current item |
| Tag and Next | Greater Than or Shift+DownArrow | Tag current item and Go to next one |
| Untag and Next | Less Than or Alt+Shift+DownArrow | Untag current item and Go to next one |
| Tag and Previous | Shift+UpArrow | Tag current item and Go to previous one |
| Untag and Previous | Alt+Shift+UpArrow | Untag current item and Go to previous one |
| Tag to Bottom | Shift+End | Tag to bottom of list |
| Untag to Bottom | Alt+Shift+End | Untag to bottom of list |
| Tag to Top | Shift+Home | Tag to top of list |
| Untag to Top | Alt+Shift+Home | Untag to top of list |
| Tag All Files | Alt+Period | Tag file items but not subfolders |
| Tag Duplicate Files | Alt+Shift+Period | Tag files with the same content as a prior one in the list |
| Tag with Regular Expression | Control+Shift+Period | Tag files that match a regular expression |
| Untag All But Current | Alt+Comma | Untag all but current item |
| Say Item Name | Apostrophe | Say name of current file or folder item |
| Say Folder Name | Shift+Apostrophe | Say name of folder containing current item |
| Say Folder | Control+Apostrophe | Say folder of archive being viewed (like window title) |
| Folder to Clipboard | Control+Shift+Apostrophe | Copy folder or archive being viewed to clipboard |
| Say Clipboard | Alt+Apostrophe | Say clipboard text |
| Clear Clipboard | Alt+Shift+Apostrophe | Clear clipboard text |
| Say Time | Alt+Semicolon | Say current time and date |
| Say What Content | Question | Say textual content of current file item, or list contained items if current item is a folder or zip archive |
| Command Prompt | Control+Slash or Control+Backslash | Open command window in current folder |
| Explorer Directory | Alt+Slash or Alt+Backslash | Open Windows Explorer in current folder |
| Stamp with Date and Time | Exclamation Point | Stamp date and time of current or tagged items |
| Hide | RightParen | Set Hidden attribute of current or tagged items |
| Show | LeftParen | Remove Hidden attribute of current or tagged items |
| ReadOnly | RightBracket | Set ReadOnly attribute of current or tagged items |
| ReadWrite | LeftBracket | Remove ReadOnly attribute of current or tagged items |
| System | RightBrace | Set System attribute of current or tagged items |
| General | LeftBrace | Remove System attribute of current or tagged items |
| Character Encoding | Shift+2 | Detect and say encoding name of current file |
| Convert Units | Shift+3 | Convert between different units of measure and copy result to clipboard |
| Say Percent Through | Shift+5 or Alt+Delete | Say current position, item count, and percent through |
| Say Filter and Order | Shift+8 | Say current sort order and filter specification |
| Drive Letter | Alt+1 through Alt+9 | Open new window on drive letter corresponding to digit |
| Next Window | Control+Tab or Alt+RightArrow | Activate next open window |
| Previous Window | Control+Shift+Tab or Alt+LeftArrow | Activate previous open window |
| Documentation | F1 | Display FileDir documentation |
| History of Changes | Shift+F1 | Display history of FileDir fixes and enhancements |
| About | Alt+F1 | Display FileDir version number and release date |
| Window Toggle | Shift+W | Toggle between most recently used windows |
| Current Windows | F4 | Pick window to open from list of current ones |
| Close Window | Control+F4 | Close current window |
| Close All But Current Window | Control+Shift+F4 | Close all windows except the current one |
| Exit FileDir | Alt+F4 | Close the FileDir application |
| Restart Windows | Alt+Shift+F4 | Restart Windows after confirming |
| Context Menu | Shift+F10 | Pick action to perform on current or tagged items based on file extension/type |
| SendTo Menu | Control+F10 | Pick Send To shortcut and pass current or tagged items to it |
| Alternate Menu | Alt+F10 | Pick command to execute from complete, alphabetized list |
| Elevate Version | F11 | Download latest FileDir version and run installer (after confirming) |
| Arrange Icons | Alt+F11 | Arrange open windows |
| Cascade | Control+F11 | Cascade open windows |
| Tile Horizontal | Alt+Shift+F11 | Tile open windows horizontally |
| Tile Vertical | Control+Shift+F11 | Tile open windows vertically |
| Start Timer | F12 | Start, pause, or resume timer |
| Stop Timer | Shift+F12 | Stop running or paused timer |
| Say Timer | Alt+F12 | Say elapsed time since start of timer (not counting any paused periods) |

## Development Notes

For the technically curious, I developed FileDir with the C# programming language on the .NET Framework 4.8, built with the Roslyn compiler from Microsoft.

Document text is extracted with the 2htm utility, written by Jamal Mazrui and released under the MIT license, which converts Word, Excel, PowerPoint, PDF, and Markdown files to accessible HTML. It is available at [2htm on GitHub](https://github.com/JamalMazrui/2htm)

This folder contains the complete source code for FileDir in FileDir.cs, the Homer helper modules (Web.cs, Say.cs, and Inix.cs), and the Layout by Code support libraries (lbc.cs and LbcVB.vb), the latter being progressively replaced by the Homer modules.

The code is covered by a modified version of the GNU General Public License (GPL), which is explained at [GNU General Public License](http://gnu.org/copyleft/gpl.html)

Essentially, software that uses the code must be open source, except that I am willing to relax GPL conditions in a particular case if persuaded that a greater good would result.

I welcome feedback, which helps FileDir improve over time. When reporting a problem, the more specifics the better, including steps to reproduce it, if possible.

The latest version of FileDir is available at the same URL, [FileDir download](http://www.EmpowermentZone.com/dirsetup.exe)

This may be downloaded and installed with the Elevate Version command, F11.

Jamal Mazrui

[jamal@EmpowermentZone.com](mailto:jamal@EmpowermentZone.com)

End of Document
