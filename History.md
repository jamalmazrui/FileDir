# FileDir — Change History

**Version 5.0.52**  
August 2026  
Copyright 2006-2026 by Jamal Mazrui  
MIT License

## Contents

- [Beta 0.7](#beta-0-7)
- [Beta 0.8](#beta-0-8)
- [Beta 0.9](#beta-0-9)
- [Beta 0.93](#beta-0-93)
- [Beta 0.94](#beta-0-94)
- [Beta 0.95](#beta-0-95)
- [Beta 0.96](#beta-0-96)
- [Version 1.0](#version-1-0)
- [Version 1.1](#version-1-1)
- [Version 1.2](#version-1-2)
- [Version 1.3](#version-1-3)
- [Version 1.4](#version-1-4)
- [Version 1.5](#version-1-5)
- [Version 1.6](#version-1-6)
- [Version 1.7](#version-1-7)
- [Version 1.8](#version-1-8)
- [Version 1.9](#version-1-9)
- [Version 2.0](#version-2-0)
- [Version 2.1](#version-2-1)
- [Version 2.2](#version-2-2)
- [Version 2.3](#version-2-3)
- [Version 2.4](#version-2-4)
- [Version 2.5](#version-2-5)
- [Version 2.6](#version-2-6)
- [Version 2.7](#version-2-7)
- [Version 2.8](#version-2-8)
- [Version 2.9](#version-2-9)
- [Version 3.0](#version-3-0)
- [Version 3.1](#version-3-1)
- [Version 3.2](#version-3-2)
- [Version 3.3](#version-3-3)
- [Version 3.4](#version-3-4)
- [Version 3.5](#version-3-5)
- [Version 3.6](#version-3-6)
- [Version 3.7](#version-3-7)
- [Version 3.8](#version-3-8)
- [Version 3.9](#version-3-9)
- [Version 5.0.54](#version-5-0-54)
- [Version 5.0.53](#version-5-0-53)
- [Version 5.0.52](#version-5-0-52)
- [Version 5.0.51](#version-5-0-51)
- [Version 5.0.50](#version-5-0-50)
- [Version 5.0.49](#version-5-0-49)
- [Version 5.0.48](#version-5-0-48)
- [Version 5.0.47](#version-5-0-47)
- [Version 5.0.46](#version-5-0-46)
- [Version 5.0.45](#version-5-0-45)
- [Version 5.0.44](#version-5-0-44)
- [Version 5.0.43](#version-5-0-43)
- [Version 5.0.42](#version-5-0-42)
- [Version 5.0.41](#version-5-0-41)
- [Version 5.0.40](#version-5-0-40)
- [Version 5.0.39](#version-5-0-39)
- [Version 5.0.38](#version-5-0-38)
- [Version 5.0.37, the public release](#version-5-0-37-the-public-release)
- [Version 5.0.36](#version-5-0-36)
- [Version 5.0.35](#version-5-0-35)
- [Version 5.0.34](#version-5-0-34)
- [Version 5.0.33](#version-5-0-33)
- [Version 5.0.31](#version-5-0-31)
- [Version 5.0.30](#version-5-0-30)
- [Version 5.0.29](#version-5-0-29)
- [Version 5.0.28](#version-5-0-28)
- [Version 5.0.25](#version-5-0-25)
- [Version 5.0.24](#version-5-0-24)
- [Version 5.0.23](#version-5-0-23)
- [Version 5.0.20](#version-5-0-20)
- [Version 5.0.19](#version-5-0-19)
- [Version 5.0.18](#version-5-0-18)
- [Version 5.0.17](#version-5-0-17)
- [Version 5.0.15](#version-5-0-15)
- [Version 5.0 beta](#version-5-0-beta)

## Beta 0.7

*Released December 11, 2006*

Documentation now references the web page for downloading the .NET Framework version 2.0: [microsoft.com](http://www.microsoft.com/downloads/details.aspx?familyid=0856eacb-4362-4b0d-8edd-aab15c5e04f5&displaylang=en)

Fixed Hot Key Summary not showing Yield on Drive command, Alt+Y, or Beginning File command, Alt+B.  Alt+Y says the number of bytes occupied and free on the current drive.  Alt+B goes to the first file item, skipping over any folder items at the top of the list.

Clarified that the Open Drive command is invoked with Alt+O rather than Shift+G.  Made the Open Special Folder command, Control+Shift+O, say "My Documents" instead of "Personal."

Changed the status line so that it says the date and size of the current item rather than the current position and item count.  Implemented support for folder as well as file items in the Copy, Delete, and Move Tagged commands (Shift+C, Shift+D, and Shift+M).

Added a query for the current file via the Apostrophe key, and one for the current folder via the Shift+Apostrophe key (Alt+Apostrophe queries the clipboard).

Added the File Find command, Alt+Shift+F, to search for a file based on text it contains and/or a wild card pattern matching its name.  Files in the current folder and subfolders will be searched. Pick from a ListBox of matching files.  A new window will be opened for the associated folder, with focus placed on the matching file within it.

## Beta 0.8

*Released December 13, 2006*

Fixed the What command, invoked with question mark (Shift+Slash), not silencing speech after a key press.  It reads a maximum of about 20K of text from a file.

The size of the current item on the status line is now expressed in an abbreviated manner using K for kilobytes, M for megabytes, or G for gigabytes.  Note that the size of a folder is -1 until a query command such as Size, Shift+S, or Yield Tagged, Shift+Y, forces it to be calculated (by recursively summing the sizes of all contained files and subfolders).

The Copy or Move Tagged commands, Shift+C or Shift+M, now prompt whether to overwrite existing folders and files.  You are informed whether the date of a target with the same name is older, newer, or current and whether its size is smaller, larger, or equal.  You can choose to keep all targets with the same names, replace them, or replace them only with updated source items.

FileDir remembers the directory, sort order, and filter specification at the end of the last session.  Use Control+Shift+C rather than Control+C to copy names but not full paths to the Windows clipboard.  The Open Drive command has been reassigned to Alt+Shift+O.

The Send to Word Processor command, Control+W, and Send to Text Editor command, Control+T, are configurable by the Options command, Alt+O.  This modifies a standard .ini file, FileDir.ini, in the FileDir program folder.  By default, Microsoft Word is the word processor and TextPal is the text editor.  If another executable is specified, its full path may be needed if it is not located on the Windows search path.

In addition to the backslash character indicating that an item is a folder and the greater than sign indicating it is tagged, special symbols are associated with folder and file attributes, and with new commands to manipulate them.  A right parenthesis after a list item means that the Hidden attribute is set.  As a memory aid, you may think of parenthesis hiding something from full view.  The RightParen key, Shift+0, sets the Hidden attribute of the current or tagged items.  The LeftParen key does the reverse, removing the Hidden attribute.  Similarly, the right bracket symbol means that an item has the ReadOnly attribute set.  You may think of a bracket protecting something from being modified.  The LeftBracket key removes the ReadOnly attribute.  Finally, the right brace symbol means the System attribute is set.  You may think of a brace as a character used in programming systems.  The LeftBrace key removes the System attribute.

## Beta 0.9

*Released December 15, 2006*

Fixed Tag or Untag All commands, Control+A or Control+Shift+A, affecting items not included in the current filter.  Changed how accelerator keys are displayed in the menu system so that screen readers will speak them more reliably.  Added extra speech messages via the default SAPI voice if neither JAWS nor Window-Eyes are detected.

Enhanced the Jump command, Control+J, to recognize symbols FileDir associates with file attributes.  Thus, you can jump to a ReadOnly file by entering a single ] character as the search string.  A [ would find the next item without the ReadOnly attribute set.  Using the Jump Again command, Alt+J, you can efficiently hop from one match to the next.

Enhanced the Filter command, Control+F, to match more than one specification.  For example, entering *.doc|*.pdf would limit the view to items having either a .doc or .pdf extension.  To view all items again, enter a single * character for the filter, or use the Clear Filter command, Control+Shift+F.

Enhanced the Type Command, Shift+T, to announce file attributes as well as the extension.  Enhanced the Say Item command, Apostrophe, to indicate whether the current item is tagged.

Introduced Burn to CD command, Alt+Shift+B, (like in TextPal) for adding tagged files or folders to a CD.  Added Paste command, Control+V, to copy files or folders listed on the clipboard to the current folder.  Thus, behavior similar to Windows Explorer is possible, where paths are copied to the clipboard with Control+C, and then the referenced items are pasted into another folder window with Control+V.

Added navigational shortcuts.  The Initial Change command, Shift+I, goes to the next item that begins with a different letter.  The Extension Change command, Shift+X, goes to the next item with a different extension.  These are most useful when the sort order is by Alpha/Name or Type/Extension, respectively.

Added a quick links feature for efficiently opening favorite files.  Press Control+Q to add a quick link for the current item.  A standard Windows shortcut (.lnk file) is created in the Quick subfolder of the FileDir program folder.  Press Shift+Q to open this folder at any time.  You can navigate it just like any other folder.  Press Enter to execute a shortcut item.

FileDir now checks if you are trying to open a folder that already has an open window.  If so, it says "Returning and activates that window rather than starting a new one.   Press F4 to pick one of the currently open windows from a standard ListBox.  Use the Alternate Menu command, Alt+F10, to pick a FileDir command from a complete, alphabetical list.

## Beta 0.93

*Released December 20, 2006*

Strengthened error checking and reporting.  Fixed FileDir not producing enhanced speech messages through the Window-Eyes API.  Made the Filter command, Control+F, indicate if no items match a filter just entered.

Made the Copy or Move commands, Shift+C or Shift+M, speak the comparison with a target having the same name rather than relying on a screen reader to say that part of the confirmation dialog.  Changed the technique of finding drives so that the Open Drive command, Alt+Shift+O, should identify all available ones (feedback sought).

Pressing Enter on a Quick link shortcut that is a folder will now open it in FileDir rather than Windows Explorer.  Pressing Enter on a zip archive will open it as if it is a folder.  This lets you conveniently examine the contents of a zip archive.

The new Zip command, Shift+Z, adds the current or tagged items to a zip archive.  The Unzip command, Shift+U, unarchives the current or tagged items.  The Unzip Test command, Control+U, checks whether the current item is a valid zip archive.

Some commands work with a copy of a zipped item that is unarchived to a temporary folder as needed.  This lets you use the What command, Question Mark, to identify the content of a file without unzipping the archive that contains it.  The Send to Word Processor or Text Editor commands, Control+W or Control+T, also work in this way.

Added the List Files command, Alt+L, to list file items but not folder items in the current directory view.  Similarly, Alt+Y sums the sizes of file but not folder items.  The Yield on Drive command has been reassigned to Alt+Shift+Y, providing the total size and space free on the current drive.

The Mail command, Control+M, starts a new email message with the current or tagged files as attachments.

## Beta 0.94

*Released December 28, 2006*

The Filter command, Control+F, asks for confirmation if no file or folder items match the filter.  Commands that change files or folders say "Done!" when complete.  The dialog that prompts for a folder, invoked by various commands, now guesses the folder name as you type.

The Keywords command, Control+K, now supports multiple conditions.  Use the vertical bar character (|) to separate words or phrases where any one of those terms can produce a match.  Use the ampersand character (&) as a separater where all terms must match.  For example, entering "C#|Visual Basic" would match files containing either language, whereas "C#&Visual Basic" would require both to match.  Press Alt+K to hop to the next matching file.

The Recycle Toggle, Alt+Shift+R, determines whether deleted files or folders are moved to the recycle bin.  The default setting is On.  Regardless of the current setting, Control+X recycles deleted items, whereas Shift+Delete does not.  The Delete, Copy, and Move commands are significantly faster when deleted or replaced items are not moved to the recycle bin.

Shift+R renames the current file or folder item.  Control+R renames all items in the current folder with wildcards you specify, similar to DOS commands.  Control+Shift+R renames items in the current directory view using regular expressions.  Control+Slash goes to a command prompt in the current directory.

Pressing Enter when the current item is a zip archive will open it like a folder.  Commands such as Copy and Move are not available in this archive view, but most navigation and query commands are supported.  The Delete and Unzip commands operate on items within the archive.

As before, the Unzip command, Shift+Z, preserves folder paths in the archive, whereas the new Control+U command does not.  It unzips all files to the chosen folder, but not subfolders below (ignoring folder paths, if any).  Alt+U tests whether an archive may be unzipped successfully.

Control+M now starts a mail message with its body being the textual content of the current item.  For example, pressing Control+M when a Microsoft Word document is the current item will extract its text for the message body.  Control+Shift+M starts a message with the current or tagged items as attached files.

Control+P sends current or tagged items to the default printer.  Alt+R lists recent shortcuts, which may be files or folders for which Windows has created a shortcut in the Recent special folder.  They are listed in reverse chronological order -- most recent first.  Choose an item from this standard listbox to open it.

The Yield on Drive command has been reassigned to Control+Shift+Y.  The new Yield in Operating System command, Alt+Shift+Y, gives information about the version of Windows, physical memory, and virtual memory.

## Beta 0.95

*Released December 30, 2006*

Fixed prompt to confirm filter in empty folder.  Fixed Go to Parent Folder command, Backspace, not working in empty folder.  Fixed Copy, Cut, Paste, and Undo commands in optional JAWS scripts.

Modified Recent Shortcuts command, Alt+R, to show a maximum of 100 shortcuts.  This improves load speed of the list box, since the Windows "Recent" special folder can contain many more shortcuts.  Enhanced dialog for choosing a folder so that it prompts whether to create one if the path entered is not found.  This makes it convenient to copy, move, or unzip files to a new folder with a single command.

Added the Delete Recycle Now command, Control+D, and Delete Now Command, Control+Shift+D, for deleting a single file (but not folder) without a confirmation dialog.  Added the Path List to Clipboard command, Control+Shift+P, for copying to clipboard all file paths under the current folder item.  Added Say Folder command, Control+Apostrophe, to say full path of the current directory view, and Control+Shift+Apostrophe to copy it to the clipboard.  Added Percent Through command, Shift+5 (percent sign), to say the current position, item count, and percent through  the directory list.

Use the New Folder command, Control+N, to create a new folder on disk.  Press Control+Shift+N to create a new copy of the current file or folder item within the same directory.  It will be assigned a similar name except for a numeric suffix that makes it unique, e.g., plan_01.doc for a copy of plan.doc, or plan_02.doc for the second copy.  This can be useful when you want to preserve an original version of a file before editing a copy.

Added nine commands on the Window menu to quickly open or go to an existing view on a drive.  Drives A through I are associated with the digits 1 through 9.  For example, press Alt+1 to go to Drive A or Alt+3 to go to Drive C.

Updated the documentation, F1, so that it accurately reflects the feature set and keyboard assignments that evolved during the beta development process.  Further explanations will respond to user feedback.

## Beta 0.96

*Privately released January 1, 2007*

Made the Mail Body command, Control+M, use the file name without extension as the subject of the message (the content of the file being the message body).  Made tF2 work like Shift+R for keystroke compatibility with Windows Explorer.  Improved visual appearance of directory view, including margins, font, alignment, and length of ListBox items.

## Version 1.0

*Released January 1, 2007*

Now available as an executable installer, dirsetup.exe

FileDir, a file and directory manager, is intended to be a generally superior alternative to Windows Explorer or My Computer for managing a computer file system in an efficient, accessible manner.  It is particularly designed to optimize productivity by users of the JAWS or Window-Eyes screen readers.  Familiar features of Windows Explorer are replicated for starting functionality and ease of learning.  FileDir then adds much in power and convenience.

Developed in the C# language, the FileDir application requires the .NET Framework 2.0 to run.  This is a free Microsoft download from [microsoft.com](http://www.microsoft.com/downloads/details.aspx?familyid=0856eacb-4362-4b0d-8edd-aab15c5e04f5&displaylang=en)

FileDir features include the following:

*  Open any number of directory views in a standard Multiple Document Interface (MDI), where Control+Tab cycles among open windows and Control+F4 closes one.  F4 also lets you pick from a list of those currently open.

*  A standard ListBox rather than ListView control is used to display files and subfolders, so the interface is more streamlined and responsive then Windows Explorer.

*  With single, mnemonic keystrokes, start verbalizing a file or inspect its size, date, or other attributes.

*  Tag files or folders in various ways.  Unlike selection in Windows Explorer, an accidental change of focus in the list does not affect the set of tagged items, and the Control key does not have to be held down to preserve tags while navigating.

*  Via speech, list all files in the current folder, or just the tagged ones -- regardless of how many are visible on the screen.

*  Zip and unzip files, or navigate within a ZIP archive with the same interface as regular folders.

*  Convert various file formats to plain text, including Microsoft Word, Excel, PowerPoint, Adobe PDF, Windows Help, Rich Text Format, and HTML.

*  Capture the full path of a file or set of files to the clipboard for convenient pasting into dialogs of other applications.

*  Combine multiple files into a single compound document.

*  Filter files in the current folder based on a wild card specification.

*  Jump to a file in the current folder based on part of its name or words in its body.  Multiple keyword terms are possible, where a match may require either all terms or any one of them.

*  Navigate among the subset of tagged files, e.g, going to the next tagged file, skipping over untagged ones.

*  Go quickly to another drive, arriving in the folder and on the file that last had focus there.

*  Create a shortcut to any file or folder, and invoke a list of them with a single key.

*  Start an email message with the textual content of a file as its body, or start a message with one or more tagged files as attachments.

*  Burn files onto a CD.

The number of FileDir commands exceeds 100.  Each is available by either menu selection or hot key.  If you have trouble remembering the menu or key associated with a command, try the Alternate Menu command, Alt+F10, which lets you pick from a single, alphabetized list of all commands in a standard list box.

All commands are discussed in sections of the documentation organized by conceptual category.  The documentation is automatically displayed at the end of installation, and may be reopened by pressing F1 in the program.  Alt+Shift+H displays a summary of hot keys.

This is the official, 1.0 release of FileDir, after several rounds of public beta testing and feedback.  I invite people to try it, ask questions, make suggestions, offer programming contributions, and spread the word on this free, open source project.

## Version 1.1

*Released January 4, 2007*

Fixed Come Up Level command, Comma or Backspace, not working in a zip archive view.  Made the New Folder command, Control+N, recognize when the new folder should be added as an item in the current directory view.  Silenced verbalization of the command name when toggling tagged state of the current item with Spacebar.  Made FileDir remember more values from the previous session, including those last used for the Copy, Find in Files, FTP, Go To, Jump, Keywords, Move, Open, Unzip, and Zip commands.

Added the Context Menu command, Shift+F10, for choosing an action to perform on the current file based on those available for its type/extension in the Windows registry.  Added the Send To Menu, Control+F10, for choosing among SendTo shortcuts -- installed by various applications -- to perform on the current or tagged files.

Added the capability to "put" or upload files to a directory on an FTP server, and to "get" or download from there.  For private directories, a user name and password may be stored in FileDir configuration settings with the Options command, Alt+O.  Note that these are stored in a plain text .ini file, so should not be considered secure against untrusted uses of the computer.  If you prefer to be prompted for this information instead, you can leave those settings blank.

Use the FTP Put command, Shift+F, to upload files.  FileDir prompts for an FTP directory.  If the value entered does not contain the :// sequence of characters, FileDir adds an FTP:// prefix and a / suffix for more convenient typing.  For example, a value of smart.net would become ftp://smart.net/ If you include the :// sequence of a protocol, however, FileDir accepts the value verbatim -- without making changes.  The URL is remembered as the default value for the next FTP command.

The opposite command is Get FTP, Shift+G, which downloads files from a remote directory.  FileDir presents a multiple selection list box with all file names it found in that directory.  The files selected will be downloaded to the current directory view.  Any existing files with the same names are replaced and sent to the recycle bin according to the Recycle setting, Alt+Shift+R (on by default).

## Version 1.2

*Released January 9, 2007*

Fixed Unzip commands improperly applying ANSI encoding.  Added the Filter Query command, Star (Shift+8), for checking the current sort order and filter specification.  Improved scope and organization of documentation.

Extended pairs of Open and Go To commands.  Open commands preserve the current directory view, including its tagged states, and then activate a different directory view in another window.  Go To commands reuse the current window, instead, for another directory view.  In the following pairs of commands, the shifted version is a Go To command, requiring more conscious effort due to a more destructive nature, since it discards the current directory view.  This difference is similar to how Shift+Delete is more destructive then Delete, since the shifted version does not permit recovery from the recycle bin.  Enter opens a subfolder whereas Shift+Enter goes to it.  Backspace opens the parent folder whereas Shift+Backspace goes to it.  Backslash opens the root folder of the current drive whereas Shift+Backslash goes to it.  As before, FileDir checks if a view of the target directory already exists, and if so, activates that window rather than creating another for the same directory.

Switched key assignments for the Quick commands to be more consistent with the Open/Go To pattern.  Control+Q opens the Quick folder, whereas Shift+Q creates a quick link to the current file or folder item.  Made the Escape key duplicate Control+Q for opening the Quick folder even more easily.  Modified the Quick Link command to permit a new name for the shortcut when initially created.

Added the Evaluate command, Control+E, which prompts for a mathematical expression and then copies the result to the clipboard.  Standard arithmetic operators may be used, as well as methods of the C# programming language.  Added the Export Clipboard command, Alt+Shift+E, which prompts for a new file name and then saves clipboard text to it.

## Version 1.3

*Released January 11, 2007*

Fixed problems discovered with the following commands:  Unzip (Shift+U), Come Up Level (Comma), Path List (Control+Shift+P), Filter (Control+F), FileFind (Alt+Shift+F), and Burn to CD (Alt+Shift+B).  Improved the visual appearance of a directory view.  Switched the grave accent key (`) for the Escape key (above it) as an alternative to Control+Q for the Quick folder, since it could be pressed extra times when dismissing dialogs, inadvertently changing the current directory view.

Made the FileFind command remember the last search results if the same search is repeated-- by pressing Enter to accept previous search terms when in the same directory.  FileDir defaults to the next choice in the list of search results (like TextPal's FileFind command).

Made commands that can cause a noticeable delay provide progress indications.  These commands involve copying, moving, or deleting a folder, and zipping, unzipping, uploading, or downloading a file.  FileDir no longer speaks, however, if it is not the active window, so you can work in another application window during an extended FileDir process without being interrupted by unrelated speech messages.

## Version 1.4

*Released January 12, 2007*

Fixed display of Yield on Drive keyboard assignment on the Query Menu (Control+Shift+Y).  Fixed key names being spoken by the optional JAWS scripts when no MDI child window is active (e.g., saying "Control+O Open" instead of just "Open").  Made FileDir more reliably become the active window when launched.

FileDir has always been a "Multiple Document Interface" (MDI) application, so any number of directory views may be opened, cycled among with Control+Tab, or closed with Control+F4.  Now FileDir is also a "single instance" application, so the desktop shortcut key, Alt+Control+F, activates the same program when FileDir is found in memory, rather than opening a new copy.  This and other new features (which were always a goal) make FileDir an option as the default program for opening zip archives.  The .zip extension may be associated with FileDir through the standard dialogs of Windos Explorer or My Computer.

For added convenience, new choices in the FileDir program group are also available to either set or clear such an association.  This setting also causes FileDir to opan a view of a zip archive after downloading it with Internet Explorer and choosing the Open button from the "Download Complete" dialog.  Use the new Shift+W command to hear the titles of all FileDir windows currently open.

The new Stamp command is executed with Exclamation Point (! or Shift+5).  It stamps tagged items with a different modification date and time.  FileDir prompts for numeric values for the year, month, day, hour, minute, and second, defaulting to those of the current file or folder item.

## Version 1.5

*Released January 15, 2007*

Fixed comparison of file or folder names sometimes being case sensitive.  Fixed Windows prompting whether it is safe to open a file with FileDir when that prompt does not occur with Windows Explorer.  Reviewed all menu text and made changes to ensure appropriate display of hot keys and ellippses ( ...), indicating a command presents a dialog.  Fixed the last Zip target not being remembered at the start of the nextFileDir session.

The new Control+Z command zips files like Shift+Z, but then deletes originals after confirming the integrity of the zip target.  Introduced a speech-friendly dialog that is triggered when a .NET runtime error occurs unexpectedly.   You are given the choice of exiting or continuing FileDir.

Added the Web Download command, Alt+Shift+W, which lets you download files from a public web site.  FileDir prompts for a URL and inserts the [prefix](http://prefix) if no protocol is specified.  After running an external utility to extract links from the web page, FileDir presents them in a multiple selection ListBox for downloading to the current directory.  If the URL of a link does not end in a valid file name, FileDir creates a file name for the target on disk based on other characters in the URL.

Made the Refresh command, Period or F5, remember the focused item and currently tagged items.  Added the Control+S command to save tags in the current directory view, and Control+Shift+S to restore them.  This could be useful if you need to temporarily change which items are tagged.

Updating FileDir is now more convenient.  Use the Elevate Version command, F11, to download and install the latest version.  You are prompted for confirmation.  The installer is downloaded to the folder for temporary Internet files so it will be deleted automatically when Windows reclaims space in that folder.  The current FileDir version is then unloaded so that the installer can replace any files that were in use.  You can reload the updated version in the usual manner after installation, e.g., by pressing Alt+Control+F.

## Version 1.6

*Released January 20, 2007*

Pressing Enter on a file item now invokes the default action of its type, which is generally, but not always, the Open action.  For example, an application may have set the default action to Edit or Play instead.  The Context Menu, Shift+F10, lets you choose among available actions for the current file type/extension.

FileDir now automatically refreshes a directory view if you return to it from another window.  Thus, if you copy or move a file to a directory being viewed in another window, and then return to that window, the file will appear in its directory view.

Increased speed of FileFind command, Alt+Shift+F, when only the names, but not contents, of files are searched.  Made FileDir announce the number of items in a directory view when it is activated.  Modified the filter confirmation prompt so that it asks whether to clear a filter if no items match it.  Added the Current Time command, Alt+Semicolon, to query the current time and date.

A new capability permits FileDir to be invoked instead of Windows Explorer when an application asks Windows to open a folder view.  The result of the shortcut in the FileDir program group off the Start Menu has been extended to associate FileDir with folders as well as zip archives.  Another shortcut clears these associations, restoring the default behavior of Windows Explorer.

## Version 1.7

*Released January 26, 2007*

Fixed the Zip command, Shift+Z, failing when the target archive is located in another directory.  As before, you can test whether a file can be unzipped successfully by pressing Alt+U.  Use the new Unzip Password command, Alt+Shift+U, to set a temporary password for creating, extracting, or viewing zip archives.  In such cases, FileDir says the phrase "with password" to remind you that it is applying a password to the zip archive.  To clear the password, press Alt+Shift+U again and enter a blank space.

As before, you can use Control+C and Control+V to copy items from one folder to another.  Control+C copies the full paths of tagged files or folders to the clipboard, one per line (as plain text that may be reviewed with the Quote Clipboard command, Alt+Apostrophe).  Control+V copies paths listed on the clipboard into the current folder.  The new Paste Move command, Control+Shift+V, moves the originals rather than copying them.  This is equivalent to Control+X followed by Control+V in Windows Explorer.  FileDir no longer uses Control+X as the Cut Recycle command, because the differing behavior of that key from Windows Explorer could result in inadvertent deletion of files.

Since FileDir is a program designed to be generally available while running others, it offers a few, simple utilities not directly related to file management.  As before, the Evaluate command, Control+E, prompts for a mathematical expression, and then copies the result to the clipboard.  The new F12 related keys provide timer and alarm features (you may associate the number 12 with a clock).

Press F12 to start a timer.  FileDir prompts for the announcement interval and stop time.  The announcement interval , measured in seconds, is how often FileDir will announce the amount of time elapsed since the start of the timer, e.g., a value of 60 means to announce at minute intervals.  These verbal announcements occur regardless of what program is currently in the active window.  Use a blank or 0 value to run the timer without automatic announcements.  Press Alt+F12 at any time to check how much time has elapsed so far.  If a timer is already running, the F12 key pauses it.  If paused, F12 resumes.  Press Shift+F12 to stop the timer and hear the total time it was running.

In the dialog that prompts for the announcement interval, another field is the stop time.  A blank or 0 value means that the timer will run until manually stopped by pressing Shift+F12 or exiting FileDir.  Instead, a stopping point may be specified as a date and time.  The date and time components are each optional.  If a date is used, it must include at least the month and day, separated by the forward slash character (/) -- or equivalent for non-U.S. formatting conventions.  If a time is used, it must include at least the hour and minute, separated by a colon character (:) -- or non-U.S. equivalent.  If both date and time components are used, type the date, a space, and then the time.  Without a time, today's date is assumed.  A time may use either the military, 24-hour convention, or the AM/PM suffix (otherwise AM is assumed if the hour is less than 13).  Examples of valid date/time values are as follows: 2:00 PM 14:00 7/27 6:30 2007/7/27 6:30:15

When the stop time is reached, FileDir plays some chimes and ends the timer.  Such an alarm may be used either with or without intervening announcements of time intervals.  A timer runs independently of other FileDir operations, so you can continue working in FileDir while using this capability.

## Version 1.8

*Released February 1, 2007*

Fixed The Go to Parent Folder command, Backspace, not always working from an archive view when the .zip extension is associated with FileDir.  Note that if you set FileDir to be the default program for browsing folders on the computer, then choosing My Computer on the desktop will launch FileDir instead of Windows Explorer.  You can still launch Windows Explorer either by entering Explorer.exe at the Windows Start/Run prompt or by pressing WindowsKey+E.

Adjusted the optional JAWS configuration file to reduce extraneous speech (particularly with JAWS 8.0) when navigating among items in a directory view.  Specifically, the Text Out Delay in the Advanced Options dialog of JAWS Configuration Manager was raised from a value of 0 to 50 milliseconds (thousandths of a second).

The RecycleWithDelete setting is now remembered between FileDir sessions, and may be set via the general Options dialog, Alt+O.  As before, the Delete key recycles items according to the current setting, which may be toggled with Alt+Shift+R.  Regardless of this setting, the Shift+Delete combination deletes without recycling (like Windows Explorer).  Conversely, the new Control+Delete key always deletes and recycles.  This command was previously assigned to Control+X, but that assignment was subsequently removed because it works differently than the conventional Cut command, which for technical reasons, is not implemented in FileDir.

Fixed the Get FTP command, Shift+G, presenting lines of HTML rather than file names to download with some Internet settings.  Also stopped this command from inserting an extra slash character between a source directory and file, which could cause a download to fail.  Revised the technique for providing progress messages with both this and the FTP Put command, Shift+F.

For increased security, FileDir now prompts for the FTP Password and Unzip Password in edit boxes that hide their values.  FileDir also saves them between sessions in an encrypted form rather than as text with other settings in the FileDir.ini file.  These passwords may be saved either with the specific FTP or Unzip-related dialogs, or with the general Options dialog, Alt+O.

Added the OpenWith action to the Context Menu, Shift+F10.  This invokes the standard Windows dialog for associating a program with the current file type/extension.

## Version 1.9

*Released February 4, 2007*

Fixed the Options dialog, Alt+O, mismatching fields and values.  Modified the RenameWithWildcards command, Control+R, to confirm the intended action beforehand, and to automatically refresh the directory view afterward.

The FileDir setup program now checks whether the required .NET Framework version 2.0 is installed, and if not, permits the user to conveniently do so.

## Version 2.0

*Released February 7, 2007*

Fixed some shortcut files (.lnk extension) not being executed properly.  Fixed Refresh Folder command, Period or F5, producing an error when the current directory is empty.

Added features and key assignments to support conventional functionality of Windows Explorer and of JAWS, thereby easing a transition to FileDir as one's main file manager.  JAWS conventions now implemented by the scripts include saying selected (tagged) items with Shift+JAWSKey+DownArrow, as well as saying the position of the cursor in the current view with Alt+Delete.  Shift+Space also says the current selection, like TextPal.  This command is convenient for confirming selection before a batch operation, e.g., before copying or deleting multiple items.

New Windows Explorer conventions support the Shift key plus UpArrow, DownArrow, Home, or End when tagging items for a subsequent batch command.  Adding the Alt modifier is an enhancement for untagging instead, e.g., Alt+Shift+End untags from the current item to the end of the list.  To tag or untag the current item without changing focus, press Shift+NumPad5 or Alt+Shift+NumPad5.

Furthering use of the arrow keypad, the Control modifier facilitates navigation among the subset of tagged items, e.g., Control+DownArrow is equivalent to Shift+N for going to the next tagged item.  Alt+Home is a keypad synonym for Alt+B, going to the beginning file item, skipping over any preceding folder items.

FileDir's Context Menu, Shift+F10, has been strengthened to offer almost every choice available from that menu of Windows Explorer.  An action may now also be performed on multiple items, not just the current one.

The Quick Links feature as been extended to support .url as well as .lnk files, thereby letting you manage Internet favorites with FileDir.  As before, press Shift+Q to create a shortcut for the current file or folder item.  Press Alt+Shift+Q instead to create a link to an Internet resource.  FileDir prompts for a name and address after attempting to get default values from the current web page of Internet Explorer, if open.  To make this convenient retrieval more likely, ensure that the Address Bar setting is checked in the View menu of Internet Explorer.  The Web Download command, Alt+Shift+W, now also uses this mechanism for a default web address if the current value is blank.

As before, press either Control+Q or the Accent key to open the Quick folder, which may be navigated and maintained like any other.  To review or modify the settings of a .lnk file, you can use the new Properties command, Alt+Enter, which invokes the same dialog as Windows Explorer.  A .url file contains readable text in the .ini format, so you can easily access such settings, e.g., via the Sent to Text Editor command, Control+T.

## Version 2.1

*Released February 14, 2007*

Fixed names of folder items not being displayed in a view of a zip archive.  Also fixed Beginning File command, Alt+B or Alt+Home, not working in an archive view.

Use the Calculate Units command, number sign (#) or Shift+3, to convert between different units of measure, e.g., between metric and other units of distance, volume, weight, or temperature.  Pick the type of conversion from the list box and enter the input value in the edit box.  The output value is spoken and copied to the clipboard (and may be reviewed with the Quote Clipboard command, Alt+Apostrophe).  About 80 conversions are available as follows:

Acre to hectare Atmosphere to psi BTU/hour to watt Celsius to Fahrenheit Celsius to Kelvin Centimeter to inch Cubic ft to cubic m Cubic m to cubic ft Day to hour Day to minute Degrees to radians Fahrenheit to Celsius Fathom to meter Foot to inch Foot to meter Ft/sec to meter/sec Gallon (US dry) to liter Gallon (US dry) to quart (US dry) Gallon (US liquid) to liter Gram to ounce (avoirdupois) Gram to ounce (troy) Hectare to acre Horsepower (elec.) to watt Horsepower (metric) to watt Hour to day Hour to minute Inch to centimeter Inch to foot Kelvin to Celsius Kg/sqcm to psi Kilogram to pound Kilogram to ton (UK) Kilogram to ton (US) Kilogram to ton (metric) Kilometer to mile Kilowatt to watt Knot to mph Kph to mph Light-year to mile Light-year to parsec Liter to gallon (US dry) Liter to gallon (US liquid) Liter to pint (US dry) Liter to pint (US liquid) Meter to fathom Meter to foot Meter to yard Meter/sec to ft/sec Mile to kilometer Mile to light-year Minute to day Minute to hour Minute to second Mph to knot Mph to kph Ounce (avoirdupois) to gram Ounce (troy) to gram Parsec to light-year Pascal to psi Pint to liter (US dry) Pint to liter (US liquid) Pound to kilogram Psi to atmosphere Psi to kg/sqcm Psi to pascal Quart (US dry) to gallon (US dry) Radians to degrees Second to minute Square cm to square in Square ft to square m Square in to square cm Square m to square ft Ton (UK) to Kilogram Ton (US) to Kilogram Ton (metric) to Kilogram Watt to BTU/hour Watt to horsepower (elec.) Watt to horsepower (metric) Watt to kilowatt Yard to meter

Conversions may be added, modified, or deleted by editing the Convert.txt file in the FileDir program folder.  A new installation of FileDir will replace this file, however, so custom changes would need to be manually backed up and restored.

## Version 2.2

*Released February 21, 2007*

Modified the installer so that assignment of the system-wide hot key for launching FileDir, Alt+Control+F, is optional.  Uncheck the appropriate setting to prevent the assignment.  A FileDir shortcut on the Windows desktop is still created.

Adjusted the optional JAWS scripts so that the top item in a directory view is reliably read by the SayLine command, JAWSKey+UpArrow.  This command is also more succinct since it no longer announces the position of the current item in the list, e.g., saying "eleven of 27."  If such information is desired, press the dedicated JAWS command for position information, Alt+Delete.  The same query is performed by FileDir's Percent Through command, Shift+5 (%).

Made the dialog for choosing a folder (e.g., when copying or moving files), return to the edit box if an invalid path is entered and you choose not to create it.  To cancel the dialog instead of specifying another path, press Escape.

Since FileDir is a Multiple Document Interface (MDI) application, Control+Tab cycles to the next open window, Control+Shift+Tab does the reverse, and Control+F4 closes the current window.  As before, F4 is an enhancement to pick an open window from a standard ListBox.  Additionally, the Windows Open command, Shift+W, lets you hear the titles of all windows without changing focus.  Duplicate keys have now been added to support FileDir operations from the numeric keypad:  Alt+RightArrow to go to the next window, Alt+LeftArrow to go to the previous one, and Alt+NumPad5 to say available windows.

## Version 2.3

*Released March 1, 2007*

The What command, invoked with the question mark, now extracts and speaks text of any size from the current file item -- it is not limited to 20K.  Thus, you could press question mark to identify or read a large .doc or .pdf file.  Since a setting of no punctuation may be preferred for such reading, the optional JAWS scripts now include a toggle to change speech between all and no punctuation.  Use the JAWSKey (Insert with the desktop keyboard layout) combined with the grave accent key at the top left of the main keypad (U.S. keyboard).  The JAWS SayLine command, JAWSKey+UpArrow, now spells the name of the current list item if pressed twice quickly in succession.

Items in an archive view now appear with path information so you know what directory structure would be created by the Unzip command, Shift+U.  As before, you can press Control+U instead to unzip all items into a single folder -- without subfolders being created.  Internally, a zip archive uses the forward slash character (/) rather than backslash (\) to separate directory names.

If a shortcut in the FileDir program group is used to associate directories generally with FileDir, the Recycle Bin option on the Windows desktop will no longer work.  This is because the Recycle Bin is a "virtual folder" rather than a standard one.  A new FileDir command has therefore been introduced to launch the Recycle Bin.  Press Control+B to go there for recovering deleted items.

Use the new Play List command, Control+Shift+L, to create a .m3u file with references of tagged items to play sequentially.  Types may include .mp3, .wav, or .cda (the extension of a track on a standard audio CD).  FileDir prompts for the name of the play list to create, defaulting to PlayList.m3u in the current directory.  Focus is then placed on that file (if in the same directory), so you can simply press Enter to execute the play list.  Note that if you want to play tracks on an audio CD, however, you need to save the play list in another directory that permits the creation of new files.

## Version 2.4

*Released May 8, 2007*

Fixed the commands that change drives not remembering the last directory viewed on the drive.  Now the Open Drive command, Alt+Shift+O, and specific drive commands, Alt 1 through Alt+9, open either the root directory or last directory viewed.  The Go to Folder and Open Folder commands, Control+G and Control+O, behave similarly if a drive letter and colon are entered without additional path information.

Reset the Text Out Delay setting to zero in the JAWS configuration file, since a higher number suppresses the guessing of names when in the FileDir dialog for specifying a directory.  The trade off is that JAWS speech will sometimes be repetitive when navigating a list of files or folders.  A user can raise the JAWS setting to balance these considerations according to personal preference.

Made Control+C, Control+X, and Control+V work like Windows Explorer.  File and folder paths are placed on the clipboard, not as plain text, but in the binary format that now facilitates file transfers between FileDir and Explorer views.  For a plain text list of paths instead, press Alt+C, or Alt+Shift+C for names without the preceding parent folder.

Use the Zip List command, Control+Shift+Z, to create or update a zip archive based on a list of files or folders in a text file.  For example, the file backup.lst would contain the full path of the target zip archive as the first line of text.  Subsequent lines would contain file or folder names to be added to the archive.  Paths are not needed before these names if they are in the same directory as the archive.

As before, Control+F4 closes the current window.  A new command, Control+Shift+F4, closes all windows except the current one.  This may be useful if you have opened a lot of windows, making it challenging to choose one of particular interest.  The Windows Open command, Shift+W, now says the number of windows open before listing their titles.

As before, Control+Slash opens a command prompt in the current directory.  Now Alt+Slash opens the directory in Windows Explorer.  The optional JAWS scripts support Control+Backslash and Alt+Backslash as synonyms for these commands for consistency with the "Homer editor interface."  This is part of a JAWS scripting toolkit available as an executable installer, kitsetup.exe, or as a zip archive, kitsetup.zip

## Version 2.5

*Released May 11, 2007*

Fixed the Zip List command.  Added the Manual Options command, Alt+Shift+M, for adjusting FileDir settings directly in a text editor.  With the optional JAWS scripts, made Control+Equals a synonym of Control+E for the Evaluate command (for consistency with the Homer interface).

Doubled the number of special folders available via the Open Special Folder command, Control+Shift+O.  You can now have greater control over your computer by conveniently examining and managing the following 35 folders as needed:

Administrative Tools Application Data Common Administrative Tools Common Application Data Common Desktop Common Documents Common Favorites Common Files Common Programs Common Start Menu Common Startup Common Templates Cookies Desktop Favorites Fonts Internet Cache Internet History Local Application Data My Documents My Pictures MyMusic Network Neighborhood Printer Neighborhood Program Files Programs Recent SendTo Start Menu Startup system32 Temp Templates UserName WINNT

## Version 2.6

*Released June 12, 2007*

Fixed the Yield on Drive command, Control+Shift+Y, reporting megabytes instead of gigabytes.  Fixed the Start Timer command, F12, announcing elapsed time every 60 seconds when 0 was specified for the announcement interval.  Fixed edit boxes within dialogs not always selecting text when receiving focus (such selection is efficient for typing a new entry that automatically replaces the previous one).

Fixed pasting in FileDir causing a runtime error if no files were found on the clipboard.  Fixed files copied to the clipboard not being available for pasting by other applications after FileDir is closed.

For consistency with the Homer interface (multiple JAWS script sets that give a consistent command structure to various applications), assigned Alt+Shift+C for configuring options of FileDir.  This dialog is still available with Alt+O as well.

Made clipboard-related commands more powerful and convenient.  The revised interface is described in the following exerpt from the documentation.

Like Windows Explorer, Control+C, Control+X, and Control+V copy, cut, and paste file or folder items between the current directory and clipboard.  FileDir enhances these commands with a plain text format in addition to the binary "drop list" that Windows Explorer uses to facilitate drag and drop transfers with a mouse.  Since the clipboard can actually hold multiple formats at the same time, FileDir creates both a binary and a text format when copying with Control+C or cutting with Control+X.  The text format is simply a list of file or folder paths, one per line.  Thus, paths on the clipboard are simultaneously available both to applications like Windows Explorer that look for the binary format, and applications like Notepad that look for plain text.

When pasting, Control+V recognizes the text format as well as the binary one.  Since the text format does not indicate whether files had been copied or cut to the clipboard, this command copies, rather than moves, the originals when only text format is found.  With either format, you may ensure that the originals are copied with Alt+V, or that they are moved with Alt+Shift+V.

Use the Copy Append command, Alt+C, to add items to the clipboard in both binary and text formats.  This lets you build a list on the clipboard from files in different directories.  It also lets you build a list by pressing Alt+C when focused on each item of interest, rather than first creating a set of tagged items and then copying them as a batch.

To put a list of file names on the clipboard without preceding paths, press Control+Shift+C.  To hear what files are on the clipboard, use the Quote Clipboard command, Alt+Apostrophe.  Before saying each path, FileDir says "Path drop list" if it finds this binary format.  Otherwise, FileDir only speaks text format -- other binary formats on the clipboard are not interpreted.

## Version 2.7

*Released June 26, 2007*

Fixed a corrupt zip archive being created when its location was in a subfolder of the current folder.  Fixed Home, End, PageUp, and PageDown not working with the JAWS cursor in the optional scripts.

Enhanced the wild card capability of filters.  Previously, a * character could be at the beginning or end of a pattern, or both, but not in the middle (a limitation of the SQL parser built into the .NET Framework).  This restriction has been programmed around, thus allowing a pattern such as the following: calendar*.doc As before, multiple, alternate filters may be separated by a | character, e.g., calendar*.doc|*bill*

As before, Shift+T says the type of the current item in the Windows registry, as well as its attributes such as ReadOnly, Hidden, or System.  A new command, Control+Shift+T, shows all "extended properties" of a file or folder item that are available to Windows Explorer.  Depending on the type, 32 possible properties may be examined as follows:

Name Size Type Date Modified Date Created Date Accessed Attributes Status Owner Author Title Subject Category Pages Comments Copyright Artist Album Title Year Track Number Genre Duration Bit Rate Protected Camera Model Date Picture Taken Dimensions Episode Name Program Description Audio sample size Audio sample rate Channels

## Version 2.8

*Released July 22, 2007*

Fixed a zip archive becoming corrupt if its target name was created in a subdirectory whose contents were being zipped.  Fixed being unable to move a zip archive just after it was created.

Adjusted the optional scripts so that JAWS says the first item of a menu when it is opened.  Also changed the JAWS configuration file so that a list box does not say selection state before speaking an item.  This prevents unnecessary verbalization of state changes in a directory view, at the cost of no state information being automatically spoken in the multiple selection list box of the Web Download command, Alt+Shift+W.  In that command, stopped a couple of PowerBASIC message boxes used in debugging from appearing.

Changed the output of the Type Extended command, Control+Shift+T, and Yield in Operating System command, Alt+Shift+Y, from a message box to a read-only, multiline edit box with all text selected by default.  This permits copying and pasting to the clipboard.

Made FileDir ignore blank lines in a ZipList definition file.  Modified dialogs with multiple list or field values so that they indicate the number of values at the end of the window title enclosed in parentheses.

Added three commands:  Iterate Processes, Network Connections, and Batch Mail.  Use the Iterate Processes command, Alt+I, to list all processes currently running on your computer.  Each item displays the executable name without extension, followed by the title of its main window if available.  Buttons let you choose whether to activate a process (only possible if it has a window) or terminate it.  If Terminate is chosen, FileDir first sends a request for the process to close, and if that fails, asks whether to try to force it.  You are then returned to the list of processes in case you wish to examine the next one.  End this dialog either by activating a process or choosing Cancel (same as Escape).

Press Alt+Shift+N to manage network connections.  A dialog lets you connect, disconnect, or restore mappings between physical storage and logical drives.

Use the Batch Mail command, Control+Shift+B, to individually send a message to multiple recipients (please do not use this for spam).  FileDir prompts for a text file that defines a batch mail operation.  The first nonblank line is assumed to be the subject of the message.  The next nonblank line is the full path of a text file that contains the body.  Each subsequent line that contains an @ symbol is the address of a recipient.  Here is an example definition:

[Content of Batch.eml File] This is the subject line C:\My Documents\Body.txt

[jane@doe.com](mailto:jane@doe.com) "John Doe" <[john.doe@mail.net](mailto:john.doe@mail.net) [End of Content]

Before sending a batch email, configure FileDir options for LogInUserName, Password (stored in an encrypted form), SenderAddress, and OutGoingServer (e.g., outgoing.verizon.net).  Test the command by sending yourself mail first.  This command only works with common SMTP protocol settings.

## Version 2.9

*Released November 1, 2007*

Fixed not being able to zip a subfolder of the root directory.

As before, Alt+Period tags all files, but not subfolders, in the current directory view.  A new command, Alt+Shift+Period, tags duplicate files -- any file with the same content as a prior one in the list.  This may be useful for deleting after downloading files, where some are the same except for their name or date.

As before, Alt+P says the full path of the focused file or folder item, and Alt+Shift+P copies it to the clipboard.  To copy its short path instead, press the Tilde key (Shift plus the Grave Accent at the top left of the main keyboard).  A short path contains no spaces and uses a suffix of a tilde symbol (~) and a number to abbreviate file or folder names.  This may be useful when pasting into a command line, since more characters and surrounding quotes are usually needed otherwise to specify a file.

Enhanced ways of choosing a directory for greater convenience and efficiency.  The Current Windows command, F4, now lists names of open directories without their preceding paths, thus making arrowing through the list less verbose and permitting initial letter navigation to a directory of interest.  Press Shift+F4 to hear what directories are open without invoking the dialog to choose one from a listbox.

The dialog for choosing a directory target -- e.g., in the Open, Copy, or Unzip commands -- now includes a row of buttons that present listboxes of choices.  You can pick from one of three lists:  directories open in current windows, those opened during this FileDir session, or those with shortcuts in the Quick folder.  To create a quick shortcut for a directory, press Shift+Q when it has focus.

## Version 3.0

*Released December 10, 2007*

Fixed the What command, Question Mark, not verbalizing files over 20K in size when using JAWS.  Fixed a root directory (e.g., C:\) displaying as blank in a list of current windows or recent folders.

Made the delete confirmation dialog list the items that would be deleted.  Modified Alt+R to list recent folders rather than shortcuts.  It lets you pick any folder or zip archive opened since the start of the current FileDir session -- with the most recent shown first.  Added a button to list special folders in the dialog for choosing a directory (e.g., the one after Control+O to open or Shift+C to copy).  That dialog now has buttons for current, recent, quick, or special folders.  Added the Window Toggle command, Shift+W, for switching back and forth between the two most recently opened windows.

Added settings to the Configuration Options dialog (Alt+Shift+C).  Setting DirsBeforeFiles to N (for No) causes files to be listed before subfolders in a directory view.  Setting ZipOpener to N causes zip archives to be opened by the default program associated with that extension (e.g., WinZip), rather than by FileDir, when Enter is pressed on a zip archive with focus.

Reorganized and extended equivalence between "open" and "go to" commands.  An open command creates a new directory view whereas a go to command replaces the current one.  Control+O is Open Folder;  Control+Shift+O is Open Special Folder;  and Alt+O is Open Drive.  Substitute the G key for go to commands.  Thus, Control+G is Go to Folder;  Control+Shift+G is Go to Special Folder;  and Alt+G is Go to Drive.  Control+Q opens the Quick folder, whereas the Grave Accent key (top left of the main keyboard) goes to it.

As before, Recycle with Delete, Alt+Shift+R, is a toggle that determines whether deleted items are copied to the Recycle Bin.  Turning off this setting increases efficiency since there is no delay for items to be copied.  Now this setting also makes directory navigation more efficient by discarding windows that are no longer needed.  Three pairs of commands are affected that perform an open or go to operation depending on whether the Shift key is used:  Enter for a subfolder with focus, Backspace for the parent folder, or Backslash for the root folder.  By default, The shifted version does a go to, requiring conscious effort for this more destructive version that replaces the current view rather than creating a new one with open.  If Recycle with Delete is off, however, then the roles of these keys are reversed -- e.g., Enter goes to a subfolder, whereas Shift+Enter opens it (similar to how Shift+Enter opens a new window when pressed on a link in Internet Explorer).

Two new commands may be particularly helpful to users of the Victor Stream or similar devices.  Control+Shift+U unzips to a folder with the same name as the zip archive with focus.  For example, FileDir would propose a path ending in mag0712 if focused on mag0712.zip -- thereby making it convenient to organize Daisy books in separate subfolders, as recommended.  Use the Volume Format command, Control+Shift+V, to format a disk or storage card.

The Windows Control Panel command, Control+Shift+W, launches Control Panel for configuring Windows.  Besides being a convenient hot key, this command may be needed, for technical reasons, as an alternative to navigating to Control Panel via the Windows Start Menu if you have set FileDir instead of Windows Explorer to generally open folders (an option in the FileDir program group of the Start Menu).

The System Access screen reader, as well as JAWS or Window-Eyes, is now supported with enhanced verbalization:  extra messages by FileDir beyond the default verbosity of the screen reader in use.  Such messages may be turned off with the new Extra Speech Toggle, Control+Shift+X.  They are then redirected to a log file (re-initialized at the start of a FileDir session), which may be reviewed with the Alt+Shift+X command in the text editor that has been configured for use.  The default setting for that editor has been changed from TextPal to EdSharp

FileDir compatibility with Windows Vista has been improved by locating data files that are created or modified after installation in a subfolder of Documents and Settings rather than Program Files.  Quick shortcut or URL files already created would need to be manually copied to the new location, e.g., from C:\Program Files\FileDir\Quick to C:\Documents and Settings\Owner\Application Data\FileDir\Quick

The installer for FileDir now compiles its code from "Intermediate Language" to a "native image" in the "Global Assembly Cache" of the .NET Framework.  This significantly speeds up initial loading and execution of a FileDir session.  The Elevate Version command, F11, now compares the currently running version with the web server, indicating whether a newer version is available.

## Version 3.1

*Released January 14, 2008*

Fixed conversions to text not working due to an inconsistent location for the temp file.  Fixed the Manual Options command (Alt+Shift+M) not finding the configuration file (FileDir.ini).

As before, Control+Shift+M initiates a mail message with tagged files as attachments.  If no items are tagged, FileDir now both attaches the current file and includes its text in the message body.

As before, Control+Shift+P copies the full paths of all items below a subfolder item in the directory hierarchy.  As an enhancement, after determining what file extensions are present, FileDir prompts for which ones to include in the resulting list.

Strengthened the Web Download command (Alt+Shift+W) so that it works like the one in EdSharp.  Many hot keys are available for navigating and choosing files to download.  Target names on disk are made unique in case different links end similarly, e.g., with default.htm.  Duplicate files may subsequently be identified and deleted with the Tag Duplicate Files command (Alt+Shift+Period).  This command runs much faster than before.  Other list-based dialogs enhance navigation like Web Download, e.g., Open Drive (Alt+O) and Alternate Menu (Alt+F10).

New commands include the following.  Control+Shift+I renames files to the initial line of text inside them (if found), which is often a convenient way of making the name of a file the same as the title of the document inside (e.g., useful after downloading files with cryptic names).  Control+Shift+Period tags files that match a regular expression you specify.

FileDir now supports the concept of a "virtual folder" that does not exist as a physical directory on disk.  A virtual folder is defined by a path list in a text file.  It contains the full paths of files or folders, not necessarily in a single directory, but in any directory and on any drive.  You can create such a file in a text editor, or with the help of FileDir commands like Path List, Control+Shift+P, and Export Clipboard, Alt+Shift+E.  Press Alt+Shift+O to open a virtual folder definition in a new window, or Alt+Shift+G to go to it in the same window.  In general, you can then process its items as if they were in the same directory.

## Version 3.2

*Released February 18, 2008*

Fixed various visual aspects of menus, dialogs, and the status bar.  Fixed the Context Menu, Shift+F10, producing an error if no "verbs" were found in the Windows registry for the extension of the file with focus.  Fixed the Quick Folder command (Control+Q or GraveAccent) referring to an old location.  Used a more reliable method of detecting whether the Window-Eyes screen reader is running.

Enhanced list-based dialogs with filter and order commands.  Control+F sets a filter to restrict what items are shown via wildcards (* to match any sequence of characters or ? to match a single one).  For example, you could browse replace-related commands in the Alternate Menu, Alt+F10, by pressing Control+F after invoking that list and then entering *replace* as the filter expression.  Control+Shift+F clears the filter so all items are shown again.  The order of items may also be changed:  Alt+A for alpha order, Alt+Shift+A for reverse alpha order, Alt+D for default order, or Alt+Shift+D for reverse default order.

FileDir windows may now be visually organized according to common MDI (multiple document interface) patterns.  The Window menu includes the following commands:  Arrange Icons (Alt+F11), Cascade (Control+F11), Tile Horizontal (Alt+Shift+F11), and Tile Vertical (Control+Shift+F11).

Use the new Environment Variables command, Control+E, to review or change such settings of Windows.  Choose those of the current process, user, or system as a whole.  Jump quickly to a particular variable based on its initial letter, e.g., Alt+P for the PATH setting that determines where Windows searches for an executable file that is not found in the current directory.  Changes to process settings affect the current session of FileDir, but not the next time it is run.  User settings take effect when you log in again.  System settings take affect when you restart the computer.

## Version 3.3

*Released June 27, 2008*

FileDir increases support for Window-Eyes 7.0, which is now available as a public beta at [GWMicro.com](http://GWMicro.com)

The installer option for JAWS scripts is now unchecked by default.  To check it, down arrow and press Spacebar.  There is a new option to install a Window-Eyes script package, which is also unchecked by default.  The optional scripts and set files to fine tune Window-Eyes speech are in an early stage, so I invite feedback and suggestions to improve them.

When FileDir is already open, and you open a folder with it from another program, FileDir should now reliably become the active application window.

## Version 3.4

*Released October 20, 2008*

Improved the optional script package to fine tune speech when using Window-Eyes.  Fixed the installer for the JAWS scripts so that it works with version 10.

Strengthened the error reporting system so that an unexpected event now results in a dialog with options to email the report, copy it to the clipboard, or exit FileDir.  To continue work without further action, press Escape to cancel the dialog.

As before, the copy, move, or paste commands prompt whether to overwrite existing folders and files.  You are informed whether the date of a target with the same name is older, newer, or current and whether its size is smaller, larger, or equal.  You can choose to keep all targets with the same names, replace them, or replace them only with updated source items.  A bug has been fixed that affected file comparisons after the first duplicate name was found in a target folder.

A new option lets you increment source names to eliminate conflicts.  This is helpful when files with the same names contain essentially different content rather than different versions of the same content.  For example, if the file ReadMe.Txt exists in both the source and target folders, the source would be copied to the name ReadMe_01.txt instead.  You might then use the Tag Duplicate Files command, Alt+Shift+Period, to tag and then delete files that actually have identical content.

## Version 3.5

*December 24, 2008*

Added the Extract with Regular Expression command, Control+Shift+E.  This is similar to Append to Clipboard, Shift+A, except that you are prompted for a regular expression, and then only matches in tagged files are copied to the clipboard.

## Version 3.6

*March 3, 2009*

The unzip commands are now broader, unarchive commands that work with almost any archive format, including .rar, .tar, .gz, .bz2, .chm, .cab, and .wepm (a Window-Eyes script package that is the same format as .cab).  FileDir does this with the free 7Zip utility behind the scenes, which is also available independently at [7zip.com](http://7zip.com)

Although any archive may be viewed or extracted, it is still the case that only a zip archive may be created or modified.

The What command, invoked with a question mark, now says the number of items in an archive or subfolder before saying their names.  As before, the Output Text command, Shift+O, converts other file formats to text.  It now does this with an updated conversion tool (GetText.exe).

Inquire Differences, Alt+Shift+I, is a new command for comparing files in two folders.  The current folder is considered the source.  You are prompted for a target folder.  FileDir generates a report in structured text format and prompts you for where to save it.  The default name is Report.txt in the current folder.  The report contains three sections:  common target files, missing target files, and additional target files.  The first section lists target file names that also exist in the source folder, and indicates whether each is newer, older, or current (a time stamp comparison), as well whether it is larger, smaller, or equal (a size comparison).  The second section lists file names that are missing in the target folder.  The third section lists additional file names found in the target folder.

For maximum functionality of FileDir under Windows Vista, you may wish to configure it to "run as administrator."  One way of doing this is by modifying the FileDir shortcut on the desktop.  Press Alt+Enter to open its properties, choose the Advanced button, and mark the checkbox to require administrative priviledges.  Otherwise, for example, the ability to view or change files under the directory tree C:\Program Files will be considerably restricted.

## Version 3.7

*March 29, 2009*

I am pleased to associate my open source projects with the "Raising the Floor" initiative, located on the web at [RaisingTheFloor.net](http://RaisingTheFloor.net)

This is the broadest community initiative I know on technology access regardless of disability or economic position.  participation is a way of enabling human potential at an international level.

The spirit is hopefully expressed in the latest improvements to EdSharp and FileDir, which make significant improvements in international support, help options, and 64-bit compatibility, among other areas.

Additional help options increase ways of learning these applications through both study and discovery.  Control+F1 is a new command that toggles a key describer mode in which pressing a key describes its action.  Switching to another application window also turns off the mode automatically.

In general, the wording of command names in EdSharp and FileDir has been made more consistent, thereby aiding memorization.  As before, complete documentation is available in your default web browser with F1, and a summary of hot keys is available with Alt+Shift+H.  The Alternate Menu command, Alt+F10, now shows descriptions of commands as well as their names and hotkeys.  As before, you can filter what commands are shownin the listbox, e.g., press Control+F for filter, type the *copy* string, and press Enter to show only commands related to a copy operation.  Press Control+Shift+F to clear the filter, showing all commands again.

Recent changes were made for compatibility with 64-bit Windows.  An exception, however, is that the JAWS scripts to refine speech should be manually installed on 64-bit Windows at present, rather than by marking the checkbox at the end of installation.  To do this, choose the Explore Settings option from the JAWS program group of the Windows Start Menu to find the user script folder, and then unarchive the file called ed_scr.zip or dir_scr.zip from the appropriate program folder, e.g., C:\Program Files\FileDir\dir_scr.zip

As before, when an archive file has focus in FileDir, pressing Enter presents a view of its items that is similar to a directory view.  You can now choose to open the archive with another program instead by pressing Shift+Enter.  This runs the default program associated with the file extension in the Windows registry, e.g., WinZip or WinRar.

A new command, Alt+Shift+F4, restarts Windows after prompting for confirmation in a standard message box.

## Version 3.8

*April 8, 2009*

The free, open source screen reader for Windows called Nonvisual Desktop Access, NVDA, is now supported with direct speech messages, just like JAWS, System Access, and Window-Eyes.  NVDA is available either as an installer or portable version from [nvda-project.org](http://nvda-project.org)

The Recent command, Alt+R, now prompts whether to show recent folders opened in FileDir, or recent shortcuts that Windows automatically creates in the special folder called Recent.  These are shortcuts to files or folders that you opened in almost any application.

Web Client Utilities, Alt+Shift+Space, is a new command for tasks that conveniently retrieve information from web sources.  The collection of 35 utilities is described in a dedicated section of documentation.

When navigating the menu system, a tooltip about the current menu item is now displayed on the status line.  This is the same summary information that appears in each list item of the Alternate Menu command, Alt+F10.

The installer for optional JAWS scripts is now compatible with 64-bit versions of Windows.

## Version 3.9

*January 14, 2011*

Improved detection of and conversion among file encodings by incorporating the Encoding utility that is separately available as Encoding.zip

The Query Encoding command (Shift+2) now detects almost any file encoding, not just forms of Unicode.  It uses the same algorithm as Mozilla Firefox, which is usually correct, though not always.

Convert Encoding (Control+3) is a new command for converting a text file to a different encoding.  A list of nearly 100 encodings is offerred.  A few are unofficial terms with special meaning.  UTF-8B means UTF-8 encoding with a byte order mark (BOM) at the beginning.  UTF-8N means UTF-8 without a BOM.  ASCIIFY means 7-bit ASCII except that an attempt is made to substitute ASCII characters or words that are equivalent in meaning to characters found with code points above 127.  DEFAULT means the default encoding or "code page" of the computer in use, e.g., Latin1 or CP1252.

Additional encoding support is made possible by incorporating the Encoding.exe utility, distributed in the  WebClient subdirectory of the FileDir program directory.  This utility is also available separately as Encoding.zip

Added support for Microsoft Office 2007 file formats, which have extensions similar to prior versions except for an additional 'x' character, e.g., .docx for a Word document, .pptx for a PowerPoint presentation, or .xlsx for an Excel spreadsheet.  This affects the What command (invoked with the question mark symbol), the Output to Text command (Shift+O), and the Append Text to Clipboard command (Shift+A).  This support requires the installation of a Microsoft "filter pack," described on the web page [microsoft.com](http://www.microsoft.com/downloads/en/details.aspx?familyid=60c92a37-719c-4077-b5c6-cac34f4227cc&displaylang=en&tm)

The direct download link for 32-bit Windows is [filterpackx86.exe](http://download.microsoft.com/download/b/e/6/be61cfa4-b59e-4f26-a641-5dbf906dee24/filterpackx86.exe)

and the one for 64-bit Windows is [filterpackx64.exe](http://download.microsoft.com/download/b/e/6/be61cfa4-b59e-4f26-a641-5dbf906dee24/filterpackx64.exe)

An option at the end of the FileDir installer lets you install this by simply marking a checkbox.

Sped up time for subsequent invocations of FileDir after the  initial one.  Improved the optional JAWS scripts for FileDir so that titles of top-level windows are more reliably  read.

## Version 5.0.54

*September 2026*

**Play List found no media links in a .htm directory, and the cause was the fix
made in the same round.** Say Contents was reading markup aloud, so Pandoc was
told "-t plain", which is the writer that produces prose. Plain text has no
links in it: Pandoc DELETES every address and leaves only the words. So a page
of podcast links came back as a list of show names with nothing to play.

Markdown directories still worked, because a .md file is read straight off the
disk rather than through Pandoc. That is exactly why this failed on .htm and not
on .md, and why it looked as though some podcasts worked and others did not.

The file is now read RAW when it is text, and the links are found in the markup
-- the same way the clipboard is read when a page is copied from a browser. A
format that is not text at all, a Word document or a PDF, still goes through the
extractor: its links are gone, but an address written out in the body survives.

Say Contents is unaffected and still reads prose. The two wanted different
things from the same file, and now each asks for what it needs.

**A run that found nothing left no trace in the log.** Two runs were recorded,
both successful; the failures were invisible, so a report that some files worked
and some did not could not be told from a report that the command was never
pressed. The empty case is now logged with the file and how much of it was read.

## Version 5.0.53

*September 2026*

**A name collision stopped the build.** The new branch in Play List declared
sBody for the text it had just read, and further down that same method sBody
already holds the play list being written. C# will not let one name mean two
things in nested scopes even when the two never overlap in time, so the
compiler refused it. Renamed to sText.

Worth saying plainly what this cost and what it did not. The compiler found it
in a second, which is the right tool for the job; the checks that run before the
compiler cannot see scope and were never going to. I did try adding one, and it
reported two hundred and thirteen collisions in code that compiles perfectly --
it counted braces across sibling blocks rather than following nesting. A check
that noisy teaches its reader to skim, which is worse than no check, so it was
not kept.

## Version 5.0.52

*September 2026*

**Say Contents on a web page read the markup aloud, and it really was reading
markup.** Pandoc has no writer registered for the .txt extension, and rather
than refuse it falls back to MARKDOWN. So asking for text gave back a hash for
every heading, a pair of asterisks around every bold word, and every link
followed by its address in brackets -- read out character by character.

Pandoc is now told "-t plain" by name, which is the writer that produces prose:
no hashes, no asterisks, and a link reduced to the words a person would read.
Tables survive as aligned columns.

Everything that wants text passes through that one call, so Say Contents,
Append to Clipboard, Translate File and Chat about File were all affected and
are all fixed together. Plain text as an Output Type target is better for it
too: it was quietly producing Markdown.

**Play List acts on the current item when nothing is tagged.** The same idea as
Control+C in EdSharp taking the current line when there is no selection: with no
selection, act on what is under the cursor, and act on it sensibly. A single
file is not a set worth writing a list about, so the question becomes what that
one file means, and there are three useful answers.

A play list is played. A sound or video file is played. Anything else that can
be read is searched for media links, which are then played -- the same rules
Alt+Shift+L applies to the clipboard. That last case is the useful one: put the
cursor on a directory of podcasts, press the key, and it plays, with no copying
at all.

Tag two or more files and the command means what it always meant: write a play
list of them.

The rules for what counts as a link now live in one place that both commands
call, so Play List and Play Media can never disagree about the same file.

## Version 5.0.51

*September 2026*

**A zip archive listed itself as one of its own contents.** Pressing Question
Mark on an archive spoke one item too many, and the extra one was the path of
the archive; opening the archive with Enter showed the same misleading entry at
the top of the list.

It was not a miscount. "7z l -slt" prints a block about the ARCHIVE first --
its own path, type and physical size -- then a line of dashes, then a block per
entry inside. Everything through those dashes has to be stripped, and the
pattern doing it required a BLANK LINE after them. Some versions of 7-Zip print
one and some do not. When there is none the pattern matches nothing at all, so
nothing is stripped, and the archive's own record is read as the first item.

The blank line is no longer required, and the run of dashes is counted rather
than spelled out, since its length has varied between versions as well. As a
second guard, a record naming the archive itself is dropped by name too, so a
future change to the header cannot bring the fault back.

**And an empty row at the end of every listing.** The test that decides when a
record is complete read as "A and B, or C", where it had to be "A and, B or C".
On the last line of the output the third condition alone was enough, so a row
was added whether or not any path had been read. Brackets corrected.

Checked against all three shapes of 7-Zip output, including a header with no
dashes at all: contents only, in each case.

## Version 5.0.50

*September 2026*

**A play list of ytdl searches on the clipboard played nothing, and said
nothing.** The clipboard reader knew three schemes -- https, rtmp and rtsp --
so a file full of lines like

    ytdl://ytsearch1:Albert King Crosscut Saw

read as prose. No playable line was found, so the clipboard was passed over and
the folder was played instead, with no reason given for either.

That address is worth supporting. mpv hands the whole of it to yt-dlp, which
finds and plays the first match, so a play list can name a song without anyone
having to look up a video identifier that will be wrong the day the video is
re-uploaded. It is the honest way to write a playlist of songs, and FileDir
could not read it.

Three changes. The scheme is recognised, and unlike the others it runs to the
end of the line, because a search contains spaces. It counts as direct media,
so it does not raise the warning meant for lists of web pages -- yt-dlp resolves
it to a stream, not to a page that has to be fetched and examined. And mms and
srt were added to the ordinary scheme list while it was open.

Checked against a real 202-entry play list: nothing before, all 202 after.

## Version 5.0.49

*August 2026*

**The links were found; they were the wrong kind of link.** The log shows 185
entries taken from the clipboard and mpv started, and then nothing: no sound, no
window, and no way to reach the player to stop it.

A podcast directory links to each episode's PAGE, not to its audio. Every one of
the 91 addresses in the file was an accesson.pinecast.co episode page. mpv hands
a page to yt-dlp, which has to fetch and examine it before anything can play,
and for a site it has no extractor for nothing plays at all. A hundred and
eighty-five of those, several seconds each, is a long silence.

Three fixes, and the first matters most.

**A window appears at once.** mpv used to show nothing until it had decoded
something, so a player that never got started had no window at all -- not on the
task bar, not reachable by Alt+Tab, and no way to press q at it. It now opens
its window immediately. This is the one option added back since the play list
was reduced to a plain file name, because a player nobody can find is worse than
no player.

**Direct media addresses win.** When the clipboard holds addresses that name
media outright -- ending in .mp3, .m4a and the rest -- only those are used. They
play at once and need no fetching. The extension is read before any query
string, since a podcast address usually carries tracking parameters.

**And when there are none, it says so before launching.** A list where not one
address names a media file now asks first, explaining that each page has to be
fetched, that it may fail entirely, and that a feed usually holds the direct
addresses its web pages do not. Cancel is the default button.

## Version 5.0.48

*August 2026*

**Play Media now finds links inside a document, not only lists of bare
addresses.** This is the failure with the podcast directories: a Markdown list
reads

    - [Casefile True Crime](https://feeds.megaphone.fm/casefile)

and not one line of it IS an address, so a perfectly good list of forty shows
was refused entirely. The same was true of the saved web page, where the
addresses sit inside href attributes.

Addresses are pulled out of whatever the clipboard holds, in four shapes: a
Markdown link, an HTML anchor, a rich-text HYPERLINK field, and a bare address
on its own. The link TEXT becomes the track title, so a play list made from a
directory announces the show names instead of reading URLs aloud. An address
named twice is played once, because a document usually repeats a link in a list
and again in prose.

The clipboard is also read as HTML and rich text before plain text, since a page
copied from a browser keeps its addresses in the markup and only the visible
words in the text. Asking for text alone threw the addresses away.

**Append to Clipboard keeps formatting for the first file.** Copying one web
page or Word document onto an empty clipboard now offers HTML as well as text,
so pasting into a mail message keeps the headings and links. Appending stays
Markdown: two rich documents cannot be joined without deciding whose styling
wins, and Markdown keeps headings, lists and links as text that reads well and
joins cleanly.

Windows does not take bare HTML on the clipboard -- it wants a header naming the
byte offsets of the fragment, counted in BYTES rather than characters, and a
wrong number means an empty paste. The offsets are computed and were checked
against accented text, where a character count would have shifted every one.

**Output Type moves the cursor to the file it just made**, when the filter in
effect still shows it.

**A web page FileDir creates is a .htm.** That was done two releases ago and is
in the build you have not run yet; nothing offers .html any more. The .inix
preference for files in ini format is noted and not yet done: FileDir.ini is the
settings file, and renaming it needs a migration step so nobody loses their
settings, which is worth its own change rather than being tacked on here.

## Version 5.0.47

*August 2026*

**Table.cs would not compile: it called FileDir.InixCodec, and the class is
Homer.InixCodec.** Five errors, all the same one. The namespace was assumed
rather than read, and Table.cs is itself in the Homer namespace, so the name
needs no qualifier at all.

Nothing else was wrong with it -- the method names, the argument counts and the
Section and Pair members all matched what Inix.cs declares. That is the
frustrating shape of this mistake: everything hard was right and one word that
could have been checked in a second was wrong.

So the symbol check that resolves method names now checks NAMESPACES too. Every
qualified reference in the shared sources is compared against the namespace the
class is actually declared in, across all twelve files. Run against the fixed
tree it is clean; run against what shipped it names the fault.

## Version 5.0.46

*August 2026*

**The PDF reader checkbox is ticked, and could not install its own requirement.**
PyMuPDF4LLM runs under Python, and installPdfTools.cmd stopped when there was no
Python and told the person to fetch it from python.org. That is a manual
download in an installer whose stated promise is that nothing is one -- and
since the box is ticked, anyone without Python got a failure and an errand on
their very first run.

It installs Python itself now, by winget, machine wide, like every other
component: about 30 MB. A Python installed a moment ago is not on the console's
path, so the fixed locations are searched again rather than trusting "where" --
the same trap that made FileDir report mpv missing on a machine that had it. The
checkbox label says "plus Python if this computer has none", so the size is
honest before the box is ticked rather than after.

## Version 5.0.45

*August 2026*

**ImageMagick is an optional component, for the pictures ffmpeg cannot read.**

HEIC is the reason. FFmpeg's HEIF support has been an open ticket for years, is
described upstream as partially fixed, and depends entirely on how the binary
was built -- the build FileDir uses does not have it. Meanwhile every photograph
an iPhone takes is HEIC, so the format most photographs now arrive in could not
be converted at all.

With it come camera raw from every maker (CR2, CR3, NEF, ARW, DNG and the rest),
SVG drawings, Windows icons, and the long tail of Photoshop, GIMP and
game-texture formats. Two new targets as well: a Windows icon, written with the
sizes Windows expects rather than whatever size the source happened to be, and
AVIF.

**ffmpeg keeps the ordinary work.** PNG, JPEG, BMP, GIF, TIFF and WebP go
through it as before, because it is already installed and already ticked.
ImageMagick is asked for only when one end of the conversion is a format ffmpeg
cannot reach. A PNG becoming a JPEG never touches it; a PNG becoming an icon
does.

Four details that decide whether the results are any good. An SVG has no size
until it is drawn, and ImageMagick's default of 72 dots per inch turns a full
page into a postage stamp, so it is drawn at 300. A raw file, a Photoshop file
and an animated GIF all hold several images, and without asking for the first
one every layer and thumbnail is written as its own numbered file. An icon is
written at eight sizes, which is what an icon is for. And a transparent picture
becoming a JPEG is flattened onto white, or the clear parts come out black.

Only "magick" is looked for, never "convert": since ImageMagick 7 that is the
single command, and Windows has its own convert.exe which formats disks.

The checkbox is not ticked. ffmpeg already covers the common formats, so this
serves people with phone photographs or a camera and nobody else. Its licence is
derived from Apache 2.0, so unlike ffmpeg there is no GPL question.

**Not added: images to PDF and PDF to images.** ffmpeg cannot, and ImageMagick
looks like the answer, but PyMuPDF is already installed for the PDF reader and
does both directions better. ImageMagick also ships a policy file that disables
PDF by default, a hangover from the 2018 Ghostscript vulnerabilities, so it
would fail out of the box with an unhelpful message.

## Version 5.0.44

*August 2026*

**A web page FileDir creates is a .htm, never a .html.** That is the Homer Tools
convention across every app, and Output Type was offering .html for four of its
target lists: documents, legacy Office files, PDFs and tables.

Both spellings are still READ -- the world writes both, and Pandoc is handed
either without complaint -- and a .html file already on disk keeps the name it
has. This decides only what gets written. The pair of rules that stop a file
being offered its own format still covers both spellings, so a .html file is not
offered a "Web page" target and a .htm file is not either.

The audit refuses any conversion target named .html, and refuses a build that
would generate .html documents. Tested by putting one back, which it caught.

## Version 5.0.43

*August 2026*

**Tables convert between every format that can hold one, .inix included.** They
could not before, and the gaps were worse than they looked.

.inix was read as raw text and written not at all, so the format FileDir's own
tools use for a list of records could not become anything else. .xlsx gave up a
heap of loose strings with the rows and columns thrown away -- fine for reading
a spreadsheet aloud, useless for converting one. And Pandoc reads .csv and .tsv
but writes neither, so those were input only.

Added Table.cs, which reads .inix records, .csv, .tsv, .xlsx and a Markdown pipe
table, and writes .inix, .csv, .tsv and Markdown itself.

**A Markdown pipe table is the intermediate**, for the targets FileDir does not
write. The rows go out as a pipe table and Pandoc turns them into a real table
in a Word document, a web page or an OpenDocument file. One intermediate serves
every one of them, which is the arrangement the PDF reader already uses. So the
round trip runs both ways across the whole list:

    inix <-> csv <-> tsv <-> xlsx in <-> md <-> docx, html, odt out

Three details that decide whether this works on real files. The separated-values
reader understands QUOTING, because a comma inside a quoted field is the entire
reason quoting exists and a naive split on commas breaks every address and every
"Surname, Forename" ever exported. The .xlsx reader places each cell by the
LETTERS of its reference, so an empty cell in the middle of a row stays a gap
rather than shifting every later value one place left, and it reads a numeric
cell from its own value rather than the string table. And a .csv is written with
a byte order mark, or a spreadsheet opening it guesses the code page and turns
every accented letter into rubbish.

Reading a spreadsheet as TEXT now produces a Markdown table too, so Say Contents
and Append to Clipboard give the rows and columns rather than a heap of strings.

**Writing .xlsx is deliberately not here.** It needs the whole Open XML package
written correctly -- styles, shared strings, relationships -- and a spreadsheet
that opens with a repair warning is worse than one that was never written. A
table becomes .csv instead, which every spreadsheet opens directly.

## Version 5.0.42

*August 2026*

Loading a folder into a window is substantially faster, and the reason was in
one line.

**The list text came from a DataTable EXPRESSION column.** DisplayFields carried
an Expression joining six columns, and the DataTable expression engine parsed
and evaluated it for every row. That engine is a general-purpose interpreter: it
boxes each value and walks a parsed tree. For a folder of ten thousand files
that is ten thousand interpreted evaluations to produce ten thousand string
joins -- and it ran again every time a tag was set, on every row that changed.

It is an ordinary string column now, and the text is identical, checked
character for character against what the expression produced.

**Four places add rows and twenty-five set a tag.** Rather than edit
twenty-nine call sites, the table is hooked once: a RowChanged handler fills the
column for a new row and a ColumnChanged handler updates it when a column that
shows actually changes. That is not merely less typing -- a hook cannot be
forgotten by the next command that sets a tag, and twenty-nine edited sites
could be.

**The folder is streamed rather than gathered first.** GetFileSystemInfos builds
an array of every entry before the first row can be added, so a very large
folder allocated the lot and showed nothing meanwhile. EnumerateFileSystemInfos
hands entries over as the directory is read.

**Row loading is bracketed by BeginLoadData and EndLoadData**, which turns off
constraint checking and index maintenance for the fill, with EndLoadData in a
finally so one unreadable entry cannot leave the table in loading state for the
rest of the session.

Worth recording what was already right, since it is the usual culprit
elsewhere: the loader takes attributes, times and lengths from the enumeration
itself, where Windows returns them with each entry, rather than asking
File.GetAttributes and File.GetLastWriteTime per item -- which would be three
more trips to the disk for something already in hand. The older fillTable does
ask that way, and is dead code; it is left alone rather than fixed, since
nothing calls it.

## Version 5.0.41

*August 2026*

Traced and audited the four Quick commands. The Internet Explorer scraping was
already gone -- Quick URL used to run WebGet.exe to read IE's address bar, and
since Internet Explorer no longer exists on Windows it took the address from the
clipboard instead, which works with every browser. That stands, and the comment
saying so stays with it.

What the tracing found was that neither Quick command checked the name it was
about to use.

**A name with ordinary punctuation in it failed with a raw .NET exception.** A
page called "Q: what now?" or a file called "report: final.docx" makes a path
Windows refuses, and what a person got was a message about an illegal character
in a path. Both commands now clean the name by the same rule Rename to Identify
Content uses: dashes, commas, periods, parentheses, apostrophes and underscores
are kept because they occur in real titles, and every run of anything else
becomes one space.

**An empty name made a file called ".lnk"** -- invisible in the list and
impossible to pick again. Refused now, with the reason.

**A reserved device name** such as CON or LPT1 cannot be a file however it is
spelled, and was not checked. Refused, saying why.

**Quick URL would write a link with no address in it**, which opens nothing and
then sits in the Quick folder looking like a link for ever. And an address
pasted without a scheme -- just a host, which is half of what people paste --
was written as it stood, where Windows will not follow it. It gets https now.

**Quick Shortcut would make a shortcut to something that does not exist.** The
path field is editable, so a typo produced a link to nothing, and the mistake
only showed the next time the link was followed. Checked when it is made.

Both commands now say what they added rather than "Done!", and both create the
Quick folder if it has been removed. Tested against twelve names, including
punctuation, empty, reserved and over-long: every one either produces a valid
file name or is refused with a reason.

## Version 5.0.40

*August 2026*

**The history of folders visited was recorded in one place only: the MdiChild
constructor.** So a folder was remembered when a NEW WINDOW was made for it, and
at no other time. Going to a folder that already had a window recorded nothing
at all, because activate_Helper activates that window and returns before any
constructor runs. The Quick folder on the Accent key is the everyday case: go
there twice and only the first visit existed.

Every arrival now passes through App.recordVisit, including the one that was
being lost. Arriving where you already are is not counted, so refreshing does
not fill the history with the same entry, and the list is capped so a long
session cannot grow without limit.

**Alt+LeftArrow and Alt+RightArrow are Go Back and Go Forward.** They were
second keys for Previous and Next Window, which is not what those keys mean in
any other program and not what anyone reaches for them expecting -- the
behaviour reported as broken was the behaviour as written. Window cycling keeps
Control+Tab and Control+Shift+Tab, which is where it belongs and always was.

Going back does not itself record a visit, or back and forward would chase each
other: each step back would append an entry and there would never be anywhere
forward to go. A folder deleted since the visit is stepped over rather than
reported as a dead end, so one stale entry cannot block the way back. Going
somewhere new after going back discards the forward path, as a browser does.

**Alt+R reads the same history**, showing each folder once with the most recent
first. The history keeps every visit in order because back and forward need
that; a list to pick from wants each place named once. One record, two questions
asked of it, so the list and the arrows can no longer disagree.

## Version 5.0.39

*August 2026*

Traced and audited the two character-encoding commands, Shift+2 and Control+2.
The move off the retired Encoding.exe was already done -- detection is the Ude
library, a port of the Mozilla universal detector, and conversion is .NET's own
encoders -- and the tracing found four faults in what replaced it.

**Convert Encoding overwrote the clipboard.** A debug line, Clipboard.SetText of
a list index, ran every single time the command was used. Beside it, the
remembered choice was looked up in upper case while the list holds lower case
names, so it never matched and the list always opened at the top rather than at
what was chosen last.

**Converting a file that is not text destroyed it.** The converter read the
bytes as text and wrote them back, which rewrites every byte that would not
decode. Running it on a picture left a .bak copy and a ruined original, which is
not a rescue anybody wants. It is refused now, by name, with the reason.

**The detector read whole files into memory.** Asking the encoding of a two
gigabyte video read two gigabytes. It reads a megabyte at most, which is far
more than the detector needs: the Mozilla detector settles within a few thousand
bytes.

**Neither command asked whether the file was text at all.** Reporting an
encoding for a JPEG is a made-up answer. Shift+2 now says so plainly, and says
something sensible for a folder as well.

The test is a zero byte in the first eight kilobytes. Text in any encoding this
handles does not contain one and virtually every binary format does, with UTF-16
the exception -- it is full of zeros -- so a byte-order mark settles that case
first. Checked against ASCII, UTF-8, UTF-16 in both orders, windows-1252, an
empty file, PNG, JPEG, a Windows executable, a zip and an MP3.

No NuGet package was added. Ude was already here and doing the detection, and
everything else is in the base class library.

## Version 5.0.38

*August 2026*

**Sizes are spoken in a form a person can take in at once.** Shift+S said
"1610612736 bytes". That is exact and useless: nobody holds a ten-digit number
in their head, and hearing it read out digit by digit is worse. It now says
"1.5 gigabytes".

The rules, and the reason for each. **"k" for kilobytes**, because a screen
reader reads KB as two letters and everyone knows what k means on a file size.
**"megabytes" and "gigabytes" in full**, because MB and GB read as letters too
and the words are short enough to say. **One decimal place at most, and only
below ten**: "9.4 megabytes" carries information where "94.3 megabytes" is a
number nobody keeps. That is the ordinary significant-figures habit, and it
never prints a pointless ".0". **The noun matches the count**, so "1 byte" and
"1 megabyte" are singular, and zero is said plainly as "0 bytes". Sizes are
divided by 1024, matching what File Explorer shows for the same file, so the two
never disagree in front of somebody.

The Yield commands were saying raw byte counts too, in six places altogether.
They all use the one rule now, because Shift+S and Control+Y answering the same
question differently about the same folder would be worse than either.

Humanizer was considered and not used. Its ByteSize humanizer is good work, but
it says "1.15 GB" -- an abbreviation, two decimal places -- which is not what is
wanted here, and it would add a NuGet package to a .NET Framework 4.8 build,
which is the Span trap that already cost this project a silent failure in 2htm.
Nothing to gain and a dependency to lose.

The exact byte count has not gone anywhere: it is still the Size column in the
list and still in the status line, for when precision is the point.

## Version 5.0.37, the public release

*August 2026*

FileDir 5.0 is released. The beta label is gone from the installer, from the
About box and from the documentation, because the work it stood for is done:
the program has been rebuilt, every command it advertises answers to its key,
and the machinery that checks all of that runs on every build.

What arrived since the rebuild began, in the order a person meets it. Files
convert between formats in batches, documents through Pandoc and media through
ffmpeg, with PDFs read for their structure rather than flattened. Files rename
themselves from the title inside them. Duplicates are found anywhere below a
folder, compared byte for byte, and gathered into a window that behaves like any
other. Media plays from the folder, the tagged files, or a play list copied from
anywhere, web addresses included. Everything a file knows about itself appears in
one alphabetical list. Documents translate into any language, and questions can
be asked about them, on this computer and nowhere else. Every session is logged
and one key hands the log over.

None of the AI features send anything anywhere, which is the point of doing them
locally rather than through a web service.

Rewrote the announcement for the release, within the length a LinkedIn post
allows.

### The special folder list

*August 2026*

**The special folder list, Control+Shift+G and Control+Shift+O, is rebuilt from
the official one.** It offered whatever it found by walking Shell.Application
namespaces 0 to 99 and then the Environment.SpecialFolder enumeration, taking
whatever names those gave. Three things were wrong with that.

The two sources disagree about naming, so the list mixed "Documents" with
"MyDocuments" and "CommonApplicationData" -- the enumeration's member names,
which are not what Windows calls those folders.

It offered whatever numbered slots happened to exist, which varies by machine.

And it could not offer **Downloads** at all, because .NET's enumeration has no
member for it. The folder people use most was missing from a list of special
folders.

The list now comes from SHGetKnownFolderPath, which is how Windows itself
answers this question and knows every one of them. Fifty folders are named, plus
the temporary folder, which is not a known folder and is worth having. A folder
this Windows does not have is left out rather than shown broken, and two
identifiers pointing at one place are listed once.

Every name is the official one, with a qualifier added where two would read
alike, and each is unique when compared without regard to case: "Documents" and
"Documents, Public", "Program Files" and "Program Files, 32-bit", "Start Menu"
and "Start Menu, Common". The qualifier goes after the noun so that both entries
begin with the word being looked for, which is what matters when the list is
read aloud or navigated by first letter.

## Version 5.0.36

*August 2026*

**The playing commands no longer ask anything.** Alt+Shift+L put up a list --
sound and picture, or sound alone -- before starting. That is a menu in front of
a command whose whole purpose is to begin playing. The default is the whole
experience, picture included, and mpv has its own keys for everything else.
Play List no longer asks either: it writes the list and plays it.

**The play list is now named as a plain argument, not passed with --playlist.**
Typing

    "C:\Program Files\MPV Player\mpv.exe" c:\users\jamal\downloads\temp.m3u

at the Run box played the very list FileDir could not, which settled where the
fault was. mpv opens a play list perfectly well when it is simply named -- its
own manual says "You can play playlists directly, without this option" -- and
--playlist applies security rules of its own that a list of web addresses can
fall foul of. Every other option was dropped with it: each one is a further
chance for the list to be refused, and the default experience is the whole one.
Only the yt-dlp location is still passed, because mpv cannot find FileDir's copy
without being told.

**A player that closes at once is now noticed.** mpv exits immediately when it
refuses a list, and FileDir was announcing "Playing" regardless, leaving a person
waiting for sound that was never coming. The process is watched for two seconds,
and if it has gone by then, the exact command line is shown along with the path
of the play list, so the same line can be tried by hand.

**And it did not play at all, for a reason worth recording.** The log shows
FileDir launching c:\bin\mpv.cmd -- a batch wrapper sitting in a folder early on
the PATH, while the real player was in Program Files. A wrapper that does not
forward its arguments swallows the play list silently, and there is no way to
tell from outside which kind it is.

Accepting .cmd and .bat was right; taking one in preference to a real program
was not. And the two-pass search written to fix that DID NOTHING for a release,
because one line inside it still called the old findTool, which accepts every
runnable extension -- so the pass meant to find only real programs returned the
wrapper on the PATH before it ever looked in Program Files. The code was
written, shipped, and asked the wrong question. It now asks for the extensions
of the pass it is in.

The search runs twice: the first pass accepts only .exe and .com,
and only if that finds nothing does the second accept a wrapper, because a
wrapper is better than no player at all. The arguments handed to the player are
written to the session log as well, so the next failure of this kind can be read
rather than guessed.

## Version 5.0.35

*August 2026*

The build closes FileDir itself rather than asking you to.

A build stopped with "Access to the path is denied", which explains nothing, so
the previous release taught it to name the process holding the file. That is
better, and still not good enough: being told to close something and build again
costs a whole build to learn what the build could have done itself. It happened
twice in an hour.

FileDir holds nothing a person typed, so closing it loses nothing. The build
asks the window to close the way Alt+F4 does, which lets FileDir save its
settings on the way out, waits five seconds, and only then ends a program that
ignored the request. A holder that will not close at all still stops the build,
named. Anything that is not the file being replaced is left alone: processes are
matched by full path, so a program of the same name elsewhere is never touched.

## Version 5.0.34

*August 2026*

**Rename to Identify Content, Control+Shift+I.** This is renTitle, the command-line utility,
brought inside FileDir and made to work on more than the files it happened to
suit.

renTitle asked ExifTool for one named tag -- `-filename<${title;}` -- and let
ExifTool do the renaming. That works when the tag is called Title and does
nothing at all otherwise, which is why a second copy of the batch file existed
asking for `bookname` instead. ExifTool reports thousands of tags across
hundreds of formats and they disagree: a PDF has Title, an MP3 has Title and
Album, a photograph has ObjectName, Headline, Caption-Abstract and Description,
an EPUB has BookName.

So every field is read, the ones whose NAME is about a title are kept, the ones
that are title-like but never a title are thrown out, and the LONGEST of what
remains is taken.

The exclusions matter as much as the matches. Half the tags containing the word
"name" are about the camera, the lens, the software or the file itself, and
without a second list a photograph gets named after its lens and a PDF after
pdfTeX. Fields naming the COLLECTION a file belongs to are excluded too --
album, show, product -- and that one came out of testing: on a real MP3 tag set,
"Al Green Greatest Hits" beat "Let''s Stay Together" on length alone and named
the song after the record it came from. Longest wins only among fields that
describe THIS file. A value with no letters in it is skipped as well, since a
date or a duration is not a name.

Tested against the tag sets of a photograph, a PDF, an MP3, an EPUB, a video and
a bare scan: caption, title, song title, book name, video title, and nothing at
all for the scan, which is right. Length is a crude measure of how much somebody bothered to
write, and it works: a photograph carrying both IMG_4021 and "Sunset over the
Cascades from Rattlesnake Ridge" gets named the second. Values under four
characters are codes rather than titles, values over 120 are abstracts, and a
value that merely repeats the current file name is ignored.

The sanitising follows renTitle's rules. Dashes, commas, periods, parentheses
and apostrophes are KEPT, because they occur naturally in a sentence and a name
reads wrongly without them. Capitalization is preserved exactly -- somebody
chose it, and lowercasing a title loses its proper nouns. Everything else is
dropped, and each run of dropped characters, including underscores, becomes a
single space, so "Sunset_over__the:Cascades" becomes "Sunset over the Cascades"
rather than running together. A name is never left ending in a period or a
space, which Windows will not keep, nor starting with a dash, which command-line
tools mistake for an option. An over-long title is cut at a word.

When a name is already taken, -01, -02 and so on are added to the ROOT, so the
extension still says what the file is: Sunset-01.jpg, never Sunset.jpg-01.

Every rename is shown first, with the field each title came from, and every file
that could not be renamed is listed with the reason. Nothing is overwritten.

**The build now says WHY it cannot replace a file.** A build stopped with
"Access to the path 'C:\FileDir\FileDir.exe' is denied", which is what .NET
says and explains nothing. The reason was that FileDir was running, started with
Alt+Control+F and never closed.

Before removing a build output, the build looks for a process holding that exact
path -- compared by full path, so a program of the same name elsewhere is not
blamed -- and CLOSES IT. Naming the process was already better than "access is
denied", but being told to close something and build again still costs a whole
build to learn what the build could have done itself. FileDir holds nothing a
person typed, so closing it loses nothing.

Politely first: CloseMainWindow asks the window to close the way Alt+F4 does, so
FileDir saves its settings on the way out. Only a program that ignores that is
ended, and only after five seconds, because a build must not wait on something
that is not going to answer. A holder that will not close at all still stops the
build, named.
A read-only attribute, the other ordinary cause, is simply cleared. Anything
else is reported with the system's own words and a note to look for a virus
scanner or an Explorer preview pane.

**Rename to Identify Content no longer asks first.** Renaming is not deleting:
the file is still there, still tagged, and a name that turns out wrong can be
changed again. The dialog made the quick thing slow and put a wall of text
between the command and what it had already been asked to do. Each rename is now
spoken as it happens, the cursor lands on the renamed file when the filter in
effect still shows it, and every detail -- including which field each title came
from and why a file was skipped -- goes to the session log.

Reorder Names keeps its confirmation, because it changes many names by a rule
rather than one name from its own contents, and reading the plan is the point.

**A play list on the clipboard was rejected because of what surrounded it.**
Alt+Shift+L found nothing to play in a perfectly good .m3u of forty YouTube
addresses. The file was not the problem: Append to Clipboard heads each file's
text with the file NAME and ends with a rule and "End of Document", so the list
arrived with three lines of packaging around it -- and the classifier threw
everything away the moment it met one line that was neither an address nor a
file.

That looked careful and was useless. Unplayable lines are passed over now, and
the test moved to the proportion: at least two playable lines, and at least half
of the real content. A list wrapped in headings passes; a page of prose with one
address in it still does not, which is the case the strictness was for.

**The first line of the text is a LAST resort, and only for files that have
lines.** A first line is often not a title: a date, a byline, "Chapter One", or
the opening words of a sentence that runs on. Metadata is asked first and is the
better answer, especially for a binary file, where a "first line" is not text at
all. So the fallback runs only when the metadata yields nothing AND the format
is one the extraction chain understands.

**Rename to Initial Line is gone, folded into this.** Two commands asking "what
should this file be called" is one too many, and the second was on Control+Shift+J,
which is mnemonic for nothing. Rename to Title takes the old command's key and
its behaviour as a FALLBACK: metadata first, and when there is none, the first
line of the text.

The fallback is better than the command it replaces. The old one read only what
2htm could reach; this reads through the whole extraction chain, so a Word
document or a PDF works as readily as a text file. Markdown heading marks and
underlines are stripped from the line, since they are decoration rather than
part of the title, and the same length bounds apply as to a metadata field: too
short is not a name, too long is a paragraph.

ExifTool missing is no longer fatal either. It says so once and carries on with
the text, so a folder of notes can be renamed on a machine that has no media
tools installed.

Control+Shift+J is free again.

## Version 5.0.33

*August 2026*

**FileDir said mpv was not installed on a machine that had it, three times, and
both guesses about why were wrong.** The directory listing settled it, and there
were two faults, either of which alone was enough.

The search looked only for `.exe`. That machine's PATH held `c:\bin\mpv.cmd`, a
wrapper, which runs mpv perfectly well and was walked straight past. Every
runnable extension is now tried -- .exe, .com, .cmd and .bat, in that order --
and a batch file is started through the command interpreter, since the process
object cannot start one directly.

And the search looked for a folder named after the command. mpv installs into
**"MPV Player"**, not "mpv". Any folder under Program Files, Program Files
(x86) or the user's Programs folder whose NAME CONTAINS the tool name is now
examined, which is how a program is actually named on disk.

**And the message now says where it looked.** Three rounds went by with nothing
to go on but "not installed", each answered by another guess. When a tool is not
found, FileDir lists every place it tried, so the next report can be answered
once. The same list goes to the session log whether the search succeeds or
fails.

That is the lesson worth keeping from this one: a failure that cannot be
diagnosed from its own message costs more rounds than the bug did.

## Version 5.0.31

*August 2026*

Three faults from testing, and one of them explains a contradiction.

**An .m3u could not be appended to the clipboard.** FileDir said it did not know
how to read it as text, which is absurd for a play list. The list of extensions
that are already text simply lacked it, along with .m3u8, .pls, .url and the
subtitle formats. It now has them. Saving a copy as .txt was a fair workaround
and should never have been needed.

**The installer offered to REINSTALL mpv while the Results box said it was not
installed, and both were telling the truth.** The installer's shell was started
after winget put mpv on the machine path; FileDir was started from a desktop
shortcut by an Explorer that had been running since before the install. A
process inherits the environment it was born with, so FileDir's path did not
have mpv in it and would not until the next sign-in.

Asking the path is therefore never enough for a tool that may have been
installed minutes ago. Homer.Media.findInstalled now looks beside the program,
on the path, in Program Files, in the user's Programs folder, in winget's shim
folder, and finally inside the package folder winget unpacks into, which no path
ever mentions. ffmpeg, ffprobe, yt-dlp and mpv all use it, and the Results box
searches the same places, so the installer and the program can no longer
disagree about what is on the machine.

**The Results box named the wrong key for translation.** It said Alt+Shift+L,
which moved to Alt+Shift+F7 when Translate File was aligned with EdSharp. That
text lives in summarizeSetup.ps1 and the rename never reached it, and because
Alt+Shift+L is still a real key -- Play Media now -- it read as plausible
nonsense rather than an obvious error.

A new check refuses any key named in a document or script that the program does
not have. It caught five more stale references at once: a whole paragraph in the
guide still describing the Timer on F12, Shift+F12 and Alt+F12, and Configuration
Options and Manual Options given as Control+F2 and Alt+F2 in the tutorials, which
were wrong before this release. It would NOT have caught the Results box, since
Alt+Shift+L still exists; renaming a key still means reading every shipped file,
and the audit cannot know which sentence was about which command.

The check itself then failed a build, on a working note that mentions EdSharp's
Alt+Control+E: a real key, of another program, in a document no user ever sees.
It read every .md in the folder, where it should read only what the installer
ships, which is what a user can read. Scope corrected.

And the audit told the reader that System.Memory.dll "will be built before the
installer is compiled", which is nonsense: it is a third-party assembly that
ships when present, not build output. Both land in the same list of files whose
absence is not a fault, and the message now tells them apart.

The hotkey table also gained the spelled forms of two keys it recorded only as
symbols, so Shift+Period and Shift+Comma are now written down as the tag and
untag keys they have always been.

## Version 5.0.30

*August 2026*

Three commands taken from KeyLine, the first of which fixes a bug that could
lose files.

**Tag Duplicate Files deleted them.** The command was called Tag, and it called
File.Delete on every duplicate it found: no confirmation, no way back, and a
comment beside it saying it had been done for one particular job and never taken
out again. It also compared files by reading them as TEXT, so two different
pictures whose bytes happened to decode to the same string counted as identical,
and one was deleted.

It now groups by size, hashes only the groups with more than one member, and
compares the bytes of a hash match before tagging anything -- because "near
certain" is not good enough before somebody deletes something. It tags and says
so. Delete Tagged removes them, after the confirmation that command already
asks.

**Tag Similar Files, Alt+Shift+comma.** From KeyLine's delSimilar. It groups
content.pdf with content-1.pdf, content_2.pdf, content (3).pdf and
content[4].pdf, tagging all but the largest, on the reasoning that a partial
download is smaller than the whole. A different extension is a different file,
not another copy.

The separator is required, and that is the whole difficulty of the rule. Testing
it against real names caught the fault: without a required separator, chapter1
and chapter2 group together, and one of a book's chapters gets tagged for
deletion. KeyLine's own comment says not to match chapter1 or part2, and now
neither does this.

**Translate File moved to Alt+Shift+F7, matching EdSharp.** EdSharp puts
Translate Language on that key, and the whole F7 row was free here, so the two
programs can agree. Alt+Shift+L was a poor choice anyway: it named the feature
rather than the family, and it took a letter better spent elsewhere.

**Play Media, Alt+Shift+L**, is what that letter now does. It plays the
clipboard when the clipboard holds something playable, and otherwise the tagged
files, and otherwise everything playable in the folder.

The clipboard first, because a play list of web addresses is as playable as a
folder of MP3 files once yt-dlp is beside mpv, and the clipboard is where such a
list arrives: copied from a mail message, a web page, or a file open in EdSharp.
mpv reads the clipboard perfectly well at run time -- clipboard/text is a read
and write property, native support is on by default, and Windows has its own
backend -- and Control+V inside the player appends the file or address in the
clipboard to the play list. But that is ONE entry: there is no
--playlist=clipboard and no clipboard:// protocol, and --playlist takes a file
name. A forty-track list cannot be handed over that way.

So FileDir writes the clipboard to a temporary play list and hands mpv that. It
could pipe the same text to --playlist=- instead; a file is chosen deliberately,
because it can be replayed, saved or opened as a virtual folder afterwards,
where standard input can be handed over only once. Control+V still works inside
the player for queueing one more thing while listening, and the guide says so.

The clipboard is used only when it looks playable: every line must be a web
address or name a file that exists. One line of ordinary prose and the whole
thing is ignored and the folder is played instead, so the command never does
something surprising with whatever happened to be copied last. #EXTM3U and
#EXTINF lines are kept exactly as they are, since that is where a play list
keeps its track titles.

Two things are passed to mpv that it would otherwise get wrong. yt-dlp is named
outright when FileDir has it, because mpv looks for it on the path and FileDir
may hold the only copy, beside its own program where mpv would never look --
without which a list of web addresses fails saying nothing useful. And the
player is told to keep going when an entry fails, because a list of forty
addresses will have one that has been taken down and stopping there would lose
the other thirty-nine.

Failing a playable clipboard, it plays the tagged sound and video files, and
failing those, everything playable in the folder, in the order the window is
sorted. Play List writes an .m3u worth keeping; this is the other half of
playing, which is most of it -- hear these now, keep nothing. That last part is the useful bit: sort by date and it plays in
date order, which is how a recording session or a downloaded series wants to be
heard. The list goes to a temporary file nobody has to name or tidy up.

What counts as playable is asked of Homer.Convert, which already keeps the
audio and video lists Output Type uses, so a format added there is playable here
too without a second list to keep in step.

**mpv is an optional component, and Play List is how it is reached.** No new
key: a play list already says "these files, in this order", which is exactly
what a player needs, and hanging playback off it means no second way of choosing
files and no key to learn. FileDir has few keys left, and this one was already
the right place.

Control+Shift+L still writes the .m3u as it always did. When mpv is installed it
then asks what to do with it: save it only, play it with sound and picture, or
play it with sound alone -- the last because half of why anyone plays from a
file manager is a talk or a lecture, where a video window only gets in the way.
The answer is remembered. Run the command a second time while sitting on a play
list and it plays that one instead of wrapping it in another, which is what
makes the .m3u worth keeping.

mpv is left to run on its own rather than waited for: it is a player, and
FileDir should not sit frozen while somebody listens. Its own keys work in its
window, which is the point of using it rather than growing a player inside
FileDir.

Without mpv the command behaves exactly as it always has. A person who does not
play media from the file list sees no change at all.

The checkbox is NOT ticked, and it sits last among the installs. mpv statically
links its own copy of ffmpeg, which FileDir already carries, so a good part of
the 60 MB is a second copy of something already on the machine. It buys playback
and nothing else: conversion is ffmpeg's job and stays ffmpeg's job.

**Find Duplicates in Tree, Alt+Shift+J.** KeyLine's delDupes examines a whole
directory tree, which Tag Duplicate Files cannot: that one works on the list in
front of it. This walks the current folder and everything under it, and opens
every duplicate it finds as a VIRTUAL FOLDER rather than in a dialog of its own.

That is the better answer, and FileDir already had the mechanism. A virtual
folder is a window like any other, so every command works in it: hear a name,
its size and its date, read what is inside a file with Question Mark, open one
to check before deciding, tag a range with F8, invert the tagging, and then
Delete Tagged, which asks before it deletes. Nothing new to learn and no special
case. The list is also written where Open Virtual Folder will offer it again, so
the same set reopens without walking the tree twice.

Only duplicates are listed, never the first copy of anything, so Tag All
followed by Delete Tagged leaves exactly one of each file on disk.

The tree is walked a folder at a time rather than with the framework's recursive
search, which throws the moment it meets one folder it cannot open and loses the
whole result; here an unreadable folder is skipped and noted in the log.
Junctions and symbolic links are skipped, because one pointing at a parent walks
in a circle until the stack gives out. Files are grouped by size first, so most
are never read at all, and empty files are ignored: they are all identical to
each other, which is true and never useful.

**Reorder Names, Alt+Shift+K.** From KeyLine's reorder. A single leading digit
is padded with a zero, so 2name sorts before 11name instead of after it; ReadMe
sorts to the top, along with index, introduction and overview; and licence,
contributing, change log, credits and the rest sort to the bottom. Every rename
is shown before anything happens, and a name already taken gets another.

Not taken from KeyLine, and why: listFileProperties, because Type Extended
already shows the same shell columns plus what ExifTool reads, in one sorted
list; ListInstalledPrograms, because querying Win32_Product triggers an MSI
consistency check on every installed package and can reconfigure software as a
side effect; listStartupCommands and phoneNumber, because neither is file work.
mainly.py, which pulls the article out of a saved web page, is worth having and
needs Python with a package, so it belongs with the PDF reader rather than here.

## Version 5.0.29

*August 2026*

**Say Contents and Append to Clipboard failed, and failed silently.** The cause
was not a missing Microsoft Office, which was the first guess and the wrong one:
the tester had Office installed. It was 2htm being unable to load System.Memory.

On .NET Framework 4.8 a package whose members are declared with Span needs
System.Memory.dll beside the executable. Without it 2htm prints "Could not load
file or assembly 'System.Memory, Version=4.0.2.0'" and "Failed to convert 1
file", and then **exits with code 0**. Every caller that trusted the exit code
concluded all was well, so a person pressed Question Mark and heard nothing at
all. The same fault had already skipped nine documents in a build on the
developer's machine, hidden the same way.

Three fixes for that. 2htm's words are read rather than its exit code, and a
failed assembly load produces a message naming the missing file and where to put
it. The installer ships System.Memory.dll when it is in the FileDir folder. And
the audit warns when 2htm.exe is present without it.

**The extraction chain is rebuilt so nothing fundamental depends on a commercial
product.** 2htm is now last rather than first:

1. A file that is already text is read.
2. **Pandoc** handles docx, odt, epub, html, rtf, Markdown, reStructuredText,
   LaTeX and CSV. Free, already installed for the conversion commands, and the
   list was checked against pandoc --list-input-formats rather than assumed.
3. **pptx and xlsx are read by FileDir itself**, straight out of the archive.
   Pandoc reads neither, and both are zip files full of XML; FileDir already
   carries a zip library, so this needs no Office, no COM and no new package.
   Slides are gathered in numeric order, since slide 2 sorts after slide 10 by
   name.
4. **PDF is read by PyMuPDF4LLM through Python**, which is EdSharp's
   arrangement adopted whole: pdfRich.py and installPdfTools.cmd come across
   unchanged in method. It reads a PDF's own structure -- font sizes become
   heading levels, bullet runs become lists, ruled areas become Markdown tables
   -- so a PDF arrives with everything a screen reader user navigates by, rather
   than as a wall of plain text. No Microsoft Word anywhere in it.

   This matters more than it first looks. 2htm reads a PDF through Word's PDF
   Reflow, so its PDF support was Office support all along, which is exactly
   what the reborn Homer Tools are meant not to depend on.
5. **2htm** is the last resort, for the 1997 .doc, .ppt and .xls, where there is
   no reasonable alternative, and as a second try for a PDF the reader could not
   manage.

The PDF reader is a ticked checkbox on the installer, probed like the others by
asking the recorded interpreter to import the package. A machine may carry
several Pythons and only one of them will have it, so installPdfTools records
which one it used, and FileDir and the Results box both ask that one.

Worth doing regardless of the assembly fault: a machine with a broken 2htm now
loses only PDF and the 1997 formats rather than everything.

**Tracing the chain by hand found three more faults, all the same shape: two
tables that had to agree, and did not.**

Three separate lists claimed to say what Pandoc reads. One routed .bib, .jats,
.opml and .tsv to Pandoc; the next refused them because its own list lacked
them. A third called .pptx and .xlsx Pandoc-readable, which they are not --
Pandoc writes those formats, it does not read them. There is one list now, and
every place that asks whether Pandoc can read something asks it.

.pptx and .xlsx were categorised as documents, so Output Type offered them the
ten document targets and the converter then refused all ten. They have their own
category now, as does PDF, and both are extracted first and handed to Pandoc as
Markdown -- so one rich conversion serves every target, which is what EdSharp
does with a PDF.

A PDF was offered only flat text and a web page. Now that the reader produces
Markdown with headings, lists and tables, it is offered Markdown, text, a web
page, Word, OpenDocument and rich text.

**The trace now runs on every build.** The audit walks the extension tables and
refuses a format routed to an engine that cannot read it, a document source
nothing can read, a category returned with no branch to handle it, and any list
claiming Pandoc reads PDF or the Office formats. None of that would show up in a
compiler, and each would have reached a tester as "the command did nothing". It
was tested by reintroducing one of the faults, which it caught.

**And it says why when it fails.** The silence was the real fault. Every failure
names the tool tried and the reason, spoken and written to the session log. Say
Contents, Append to Clipboard, Translate File and Chat about File share the one
chain, so a fix in it reaches all four.

## Version 5.0.28

*August 2026*

The audit warned eight times that the documents did not name the current
version, on a build that stamped all nine of them nine lines later. The audit
runs before that step, so what it sees is whatever the last build left. It is
the state of the tree, not a fault, and it is now a note with a count.

That was the third time this pattern appeared, so it is written down as a rule:
**anything the build regenerates is a note before the build and a failure only
after it.** A check that fires before the step that fixes it teaches the reader
to skim, and a reader who skims misses the one warning that mattered.

## Version 5.0.25

*August 2026*

**Four commands advertised keys they did not have.** F12 went on starting the
timer while the menu, the hotkey document and Key Describer all said Chat with
AI. Chat about File, Copy Log and Translate File were not reachable from the
keyboard at all.

menu_Helper's second argument is only the key DISPLAYED beside a command. The
keystroke is dispatched by a separate switch in ProcessCmdKey, and nothing
connects the two. Changing the first without the second produces a program that
is confidently wrong: the documentation, the description table and the compiled
key map all told the same untrue story, because they all read the same argument.

The dispatcher now matches. The audit checks that every command advertising a
key has an entry in ProcessCmdKey, with Enter and Shift+Enter named as
exceptions since they branch on what the item is. This is the check that would
have caught the fault in the release that introduced it, and it was missing
because every earlier check read the same argument the fault hid behind.

## Version 5.0.24

*August 2026*

The first build in which everything worked: nine documents converted, the
installer compiled, and every optional component found or installed.

Two cosmetic corrections to the Results box. Three blank lines had collected
before the component list; trailing blanks are now dropped, leaving the single
line that separates two sections. And the audit no longer names nine files in a
warning whose point is that the state is normal.

## Version 5.0.23

*August 2026*

**The installer no longer interrupts itself to report the JAWS scripts.** It
showed a box listing every folder in every installed JAWS version -- nine lines
of "JAWS 2024 / enu: jkm jss jsb" -- in the middle of an installation. That is a
report to nobody: it says the thing the person ticked has happened, which the
Results box says at the end anyway. The detail goes to the session log now.
FileDir.exe takes a --quiet switch for this and the installer passes it; running
the same command from the Help menu still shows the box, because there a person
asked.

**Launching FileDir and opening the guide moved to the end** of the finish page,
after every component, because they are not installations. Both unticked, both
worded as EdSharp words them, and both running as the original user.

**Corrected two version reports in the Results box.** ExifTool appeared as
"installed, NAME", the first line of its own manual page: it answers -ver, not
--version. And ffmpeg reported its version, build host and copyright in one
breath; only the version is shown now.

## Version 5.0.20

*August 2026*

**The build no longer depends on one converter being in working order.** A
build produced no HTML for any of the nine documents, and only a warning in the
log said so: 2htm could not load System.Memory, which is the Span trap the
EdSharp handover names -- a modern package on .NET Framework 4.8 needing
System.Memory.dll beside the executable. Every page silently kept whatever HTML
was there before, and the installer, which ships each .htm only if it exists,
would have carried stale documents into a release without a word.

2htm is still tried first, because it is the house tool and produces the house
style. Pandoc is now the fallback, and it is already installed machine wide for
the conversion commands, so nothing new has to be fetched. A page that comes out
zero bytes is deleted rather than kept, since an empty page looks like a
document on the Start menu and reads as nothing.

The check for this went in the wrong place first, and the next build showed it:
the audit runs BEFORE the documents are converted, so a missing ReadMe.htm
stopped the build that would have written ReadMe.htm. In the audit it is now
advice about the state of the tree, phrased as such, since a document with no
HTML is the ordinary condition of a folder just unarchived. The check that
stops a release runs immediately AFTER the conversion, inside the build, which
is the only place the question can be answered: there, no HTML means both
converters failed, and the message says which two things to check.

The direct fix for 2htm itself is to put System.Memory.dll beside 2htm.exe in
the FileDir folder.

**The build hung with nothing on the console, and the log said why.** It stopped
at the first Pandoc call. PowerShell joins an argument list with spaces and
quotes nothing, so

    --metadata title=FileDir - ReadMe

arrived at Pandoc as four separate arguments, and the bare hyphen among them is
what tells Pandoc to read from standard input. It waited for input that was
never coming. That is precisely the trap named in the EdSharp handover, met here
for real.

Two fixes, because one is not enough. Every argument holding a space is now
quoted before the list is handed over. And every command is given an empty
standard input, so a tool that decides to read it gets end-of-file at once
rather than waiting: quoting fixes the case that happened, and this fixes the
class. A build must never wait for a person who is not there.

The log is what made this findable at all. It recorded the exact argument list,
and the fault was visible in that one line.

**A log now appears whatever happens.** Three builds failed on a PowerShell
parse error and each left no log at all, because a script that will not parse
never runs a line of itself, including the line that opens its own log. Both
wrappers, BuildFileDir.cmd and cleanFileDir.cmd, now create the log and write
the first lines BEFORE starting PowerShell or Python, capture everything the
script prints, and append that at the end. Parser errors land in the log with
everything else, and the scripts append to it rather than truncating what the
wrapper wrote. BuildFileDir.log is the file to send, whatever went wrong.

The parse error itself was a multi-line expression whose continuation lines
began with a plus sign. PowerShell wants the operator at the END of the line, or
a backtick; written the other way round the statement ends early and the parser
blames a bracket several lines away. Long messages are built a line at a time
into a variable now, and the audit refuses any PowerShell line that starts with
a continuation operator.

Rewrote the announcement by the kind of work a person does, without naming
individual keys or comparing against earlier versions.

## Version 5.0.19

*August 2026*

**Output As is now Output Type, Shift+O.** The name says what the command asks:
what type should this file be? The behaviour is unchanged.

**Append to Clipboard, Shift+A, now converts every file it appends.** It used to
run each file through the text converter and, for a format that converter did
not know, fall back to reading the raw bytes -- so a tagged .zip put a screenful
of rubbish on the clipboard and nothing said why. Every file now goes through
one extractor that knows which engine reads it: Pandoc for documents, 2htm for
legacy Office and PDF, a plain read for anything already text. A format none of
them can read is skipped by name with the reason. Each file's text is headed by
its own name, because three sources run together with no seam is one document
nobody can take apart again, and the closing count says how many were appended
and how many skipped.

**FileDir keeps a session log.** One file per run at
%LOCALAPPDATA%\FileDir\logs\FileDir_<timestamp>.log, beside the setup log the
installer writes: the same folder, the same naming and the same Control+F12 as
EdSharp. It opens with the version, the program path, the command line and the
machine, and every outside program FileDir runs adds a line with its exit code
and, on failure, the first line of what it said. Pandoc, ffmpeg, ExifTool,
yt-dlp, 2htm and Ollama all report through it, so a conversion that did not work
can be explained instead of guessed at. The newest thirty logs are kept.

Copy Log, Control+F12, puts this session's log path on the clipboard in two
formats at once, as EdSharp's does: a file drop list, so pasting into a new mail
message attaches the log itself, and plain text for anything that only reads
clipboard text. "Send me the log" is now one keystroke rather than a hunt
through a profile folder nobody should have to know the shape of.

Added Log.cs, which holds all of that. It lives in the Homer namespace because
the shared classes are the ones running outside programs, and what those
programs said when they failed is exactly what a log is for; they cannot reach
the application's own class. Nothing in it ever throws: a read-only profile
means no log, not a program that will not start.

The installer's Results box now names the logs folder and reports the JAWS
scripts, saying "not offered" rather than "not installed" when JAWS is not on
the computer, since the latter reads as a failure to somebody who does not use
it.

**The F12 column now matches EdSharp.** F12 is Chat with AI, a plain question
with nothing attached; Shift+F12 is Chat about File, which sends the text of the
file you are on. EdSharp uses those two keys for the same two things, so one
habit serves both programs.

The Timer commands held that column here for twenty years and have moved to
Alt+Control+T, Alt+Control+S and Alt+Control+Y, which nothing used. That is a
deliberate trade: FileDir has few users and fewer who time anything with it, now
that a phone or a smart speaker does it better, and agreement between the
sibling programs is worth more than a habit almost nobody had.

**An answer dialog built for reading.** Lbc.AnswerDialog shows a model's reply
in a labelled multiline box that takes focus, so it can be arrowed through line
by line, selected from, and copied with Control+C. It is read only rather than
disabled, because a disabled box takes no focus and a screen reader skips it. It
closes on the Spacebar, Enter, or Escape: the first because in a read-only box
it does nothing else and a reader whose hand is on the text should not have to
find another key, and Enter is handled on the box itself since a multiline box
consumes it before the form ever sees it. One OK button, no ampersand on it, per
the Homer form guidelines.

**Chat about File.** Ask a question about the current file and a
model running on this computer answers it, with the file's text travelling
alongside -- converted from whatever format it is in by the same extractor.
EdSharp puts Chat with AI on F12 and Chat about Document on Shift+F12; in
FileDir F12, Shift+F12 and Alt+F12 have been the Timer commands for twenty
years, so this takes Control+F12, the nearest free key of that group. A file too
long to send whole is trimmed and the answer says so, because a partial answer
presented as a whole one is wrong in a way nobody can see.

Added toPlainText to Convert.cs, which is the one extractor all of this uses.
Neither Append to Clipboard nor Chat about File should have to know that a .docx
goes through Pandoc, a .pdf through 2htm, and a .cs is simply read.

Rewrote Announce.md as a release announcement in the shape of EdSharp's,
organized by the kind of work a person does rather than by a tour of the
program.

Corrected the last place FileDir still claimed the GNU licence: the first four
lines of FileDir.cs, which read "FileDir 5.0 beta", "June 17, 2026" and
"Modified GPL License". All three had been typed there and all three went stale
-- the same fault the About box had, in the one file every developer opens
first. The header now names the licence only, and says why no version or date is
written in it.

The audit walked past that line for months because it was looking for the words
"GNU General Public" and the header said "GPL". It now flags any mention of GPL
in the source that does not also say MIT, so a record of the change is allowed
and a claim is not.

License.md gained the same header as the rest of the documentation set, a
sentence saying plainly that FileDir was under a modified GNU licence until 2026
and is MIT from version 5.0 onward, and a list of the other programs FileDir
ships or calls with the licence each keeps: 2htm, 7-Zip, SharpZipLib, Tektosyne,
Ude, Pandoc, ExifTool, ffmpeg, yt-dlp and Ollama. Calling a program is not
linking to it, so none of those terms reach FileDir's own source, and the four
installed separately are not redistributed by FileDir at all.

Added a check for the licence document itself: it must say MIT License, name
FileDir and the author, carry the three sentences that make it the MIT licence
rather than something that merely says MIT at the top, and agree with every
other document that states a licence in its header.

The three source files added in this release -- Ollama.cs, Convert.cs and
Media.cs -- now carry a copyright and licence line, which they had lacked.

## Version 5.0.18

*August 2026*

**Output to Text became Output Type, and Shift+O now converts anything to
anything.** It looks at what the file is, offers a short list of what that kind
of file can become, and converts the tagged files keeping each root name.
Documents to documents through Pandoc; legacy Office and PDF to text or HTML
through 2htm; audio, video and pictures through ffmpeg. So a folder of MP4 files
becomes MP3, MKV becomes MP4 and PNG becomes JPEG in the same three keystrokes
that turn Word into Markdown. The chosen format is remembered per kind of file,
so a choice made for audio does not become the default for documents.

The Convert Format command added earlier in this release is gone, folded into
Output Type, and Alt+Shift+K is free again. Two commands that overlap is one too
many when a program has few keys left to spend, and the question "what should
this file become" is the same question either way.

**Web Download now fetches media.** When yt-dlp is installed, Alt+Shift+W asks
whether to download the media on a page or list the files linked from it, and
remembers the answer. Media can be fetched as video or as sound alone in an MP3.
The two are offered rather than guessed at: a page holding a video has no links
to list and a page of documents has no media to fetch, so a wrong guess costs
either a large download or an empty list. yt-dlp is told where ffmpeg is, so a
copy sitting in the FileDir folder is used without anything being installed.

**The media tools moved to a machine-wide install**, consistent with Pandoc.
ExifTool, ffmpeg with ffprobe, and yt-dlp are no longer carried inside the
installer: together they are well over 100 MB and EdSharp, HomerScribe and
FileDir all want them, which is exactly the argument that moved Pandoc to
Program Files. They are one ticked finish-page checkbox now, installed by
winget. Media.cs still prefers a copy in the program folder, so a developer copy
in C:\FileDir is used ahead of anything installed and nothing is disturbed.

**Type Extended, Control+Shift+T, now shows what is inside the file.** It merges
three sources into one plain list sorted by field name without regard to
capitalization: the Windows properties it already showed, the file association
details, and the metadata ExifTool reads from the file itself -- the camera and
exposure of a photograph, the artist and album of a song, the duration and
codecs of a video, the author and page count of a PDF. One sorted list rather
than three sections, because somebody looking for a field should not have to
know which source knows it, and first-letter navigation through one alphabetical
list beats arrowing through three groupings. The count is spoken before the list
opens.

Added Media.cs, which finds and runs ExifTool, ffmpeg and ffprobe. The finding
is adapted from HomerScribe rather than written again: every likely place is
tried, each candidate is RUN to learn its version, and the newest wins, with the
whole search available to show when nothing is found. HomerScribe learned that a
copy installed by winget can sit alongside an older one and no amount of looking
answers which will be used. Version numbers are compared part by part, because
ExifTool released 13.11 after 13.8 and as decimals the older one looks newer.
FileDir differs from HomerScribe in one way: it only reads, so a packaged
ExifTool with its exiftool_files folder is accepted rather than passed over.

exiftool.exe, ffmpeg.exe, ffprobe.exe and yt-dlp.exe now ship in the program
folder, as they do in HomerScribe, and all four are optional: FileDir works
without them and the installer skips any that are absent at build time. They are
kept out of the repository, where over 100 MB of third-party binaries does not
belong.

**Convert Format, Alt+Shift+K.** Pick a format and FileDir converts the tagged
files, or the current one, writing each result beside the original. Ten targets
are offered -- Word, web page, Markdown, plain text, OpenDocument, rich text,
EPUB, LaTeX, reStructuredText and MediaWiki -- with the extension named next to
each so nobody has to guess what lands on disk. A file in a format Pandoc cannot
read is skipped with a word saying so, and the closing count says how many were
converted, skipped and failed.

**Pandoc is now installed machine wide**, in C:\Program Files\Pandoc, rather
than copied into the program folder. It is about 100 MB, and EdSharp and
HomerScribe ship it too; three copies of the same executable under Program Files
is not something to ask anyone to download. It is a finish-page checkbox like
the others, probed first so an existing copy is updated rather than duplicated,
with the label saying whether the box will install, update or reinstall and at
what version. It is the one optional component that IS ticked, because without
it a third of what the Transfer and Query commands can do quietly disappears.

Added Convert.cs, which finds Pandoc and runs it. Shared in shape with the other
Homer Tools, which drive the same machine-wide copy. It knows which extensions
Pandoc can read, so a legacy .doc or a PDF is refused with a sentence naming
Output to Text as the way to handle it, rather than passing the file to Pandoc
and relaying a complaint about a format it never claimed to read.

**Fixed the installer, which would not compile.** The Parameters lines escaped
their quotes with a backslash, which is the C rule; Inno escapes a quote by
doubling it, and reported "Mismatched or misplaced quotes on parameter" without
saying why. The audit now refuses any installer line containing a
backslash-escaped quote, which is the counterpart of the check that already
guards the PowerShell scripts.

## Version 5.0.17

*August 2026*

Beta. FileDir gains its first new feature in years, and the rebuilding work of
the releases before it is complete.

**Translate File, Alt+Shift+F7.** Tag any number of documents, name a language,
and FileDir writes a translation beside each one as <name>.<language>.txt. It
reads the same formats Say Contents reads -- Word, PDF, PowerPoint, Excel,
Markdown and plain text -- so a folder of Word documents can be translated
without opening any of them. Nothing is overwritten; a name already taken gets
another. The folder refreshes at the end so the new files are simply there.

The translation is done by a model running on this computer, through Ollama.
Nothing is uploaded and no part of any file is sent anywhere, which is the point
of doing it this way rather than through a web service: a private document can
be translated privately. FileDir picks qwen2.5:7b if it is installed and
llama3.2 otherwise, by asking Ollama what it has. There is nothing to configure.

The installer offers Ollama and the larger model as finish-page checkboxes, in
the pattern EdSharp arrived at over many iterations and which is not reinvented
here. Each component is probed before it is offered, so an existing installation
is reused rather than duplicated, and each label says whether the box will
install, update or reinstall, with the version and the size. The boxes are
grouped so the ones that do something come first: install, then update, then
reinstall, which is offered last and never ticked. Neither AI box is ticked:
together they are several gigabytes, and nobody should download that by not
noticing a checkbox. One Ollama installation and one set of models serve EdSharp,
DbDo and FileDir alike, so a person who has one of the others downloads nothing.

The probes run while the progress bar is still on screen and can say what is
being checked, because each winget or Ollama query takes a second or two and the
finish page would otherwise take a silent minute to appear. Ollama is asked over
its local web interface rather than by running its command, which starts a
server in a console of its own that looks like a fault. Nothing pauses anywhere,
and one Results box at the very end reports every checkbox by name, with the
version or the exact command to add it later.

Added Ollama.cs, the client for all of that, written against the base class
library alone so no assembly reference is added to a build that has to stay on
.NET Framework 4.8.

Added two documents to the standard set: Tutorials, which walks through nine
real jobs from start to finish, and Questions and Answers. Both are on the Start
menu, and both ship as Markdown and as a web page like the rest.

Two faults were found while writing the installer, both worth recording because
they are easy to repeat. A Pascal brace comment ends at the FIRST closing brace,
so a comment mentioning an Inno constant ends in the middle of its own sentence
and hands the rest of the prose to the compiler as code. And four levels of
quoting meet on the line that asks Ollama for its model list -- Pascal, the
command interpreter, and two kinds of PowerShell quote -- which corrupted that
line once; it is now assembled with Chr(39) so each apostrophe is visibly one
apostrophe. The audit now checks both, along with the begin and end balance of
the installer code and that every routine named by a Check or code reference
actually exists.

## Version 5.0.15

*August 2026*

Housekeeping release: no change to how FileDir behaves for the user, and several
changes to how it is built, documented, and kept honest.

Key names and descriptions are now compiled into the program. They are generated
from Hotkeys.ini into KeyMap.cs at build time, and Hotkeys.ini in the program
folder is still read first as a user override. This fixes a long-standing fault:
the installer shipped Hotkeys.ini with the onlyifdoesntexist flag, so a machine
that already had FileDir never received an updated copy, and any description
added in a new version was never heard. The installer now removes the stale
copy on upgrade. Hotkeys.md, the hotkey reference, is generated from the same
table, so the program and the document cannot disagree.

Added a source audit, auditFileDir.ps1, which the build runs before compiling
and which stops the build on failure. It checks the things a compiler cannot:
the brace balance of FileDir.cs against a known baseline, that every menu
command has a description, that no key is bound twice, that Hotkeys.ini
describes nothing that has been removed, that every file the installer names
exists, that the installer holds no version literal, that the documentation set
is complete and names the current version, and that no document still refers to
the GNU licence, a retired installer name, a retired download address,
Internet Explorer, GetText, or the removed Web Client Utilities. It caught the missing
description for the Key Describer command and a description of the removed Web
Client Utilities, both of which are fixed here.

The build script now compiles the installer as well, so BuildFileDir.cmd
produces both FileDir.exe and FileDir_setup.exe. It skips that step with a note,
rather than failing, when Inno Setup is not installed.

Completed the documentation set: ReadMe, FileDir, Developer, License, History,
and Hotkeys, each with the matching HTML the build generates. The ReadMe and
Announce files in the project folder had belonged to a different program and
have been replaced. The guide's installation and development sections were
rewritten around FileDir_setup.exe and GitHub, replacing text that still
described dirsetup.exe and a download page that no longer exists, and several
paragraphs that named EdSharp by mistake now name FileDir.

Changed the licence from a modified GNU General Public License to the MIT
licence, matching the other Homer Tools. gpl.txt is no longer shipped and is
removed on upgrade.

Corrected the About box, Alt+F1. It had the version number, the release date,
and the licence typed into the source, so it announced "FileDir 5.0 beta", "June
17, 2026", and the GNU Lesser General Public License no matter which of the
fourteen later releases was running. It now takes the version from the same
version.txt the installer and the release tag use, shows the build date of the
program itself, and names the MIT licence.

Fixed two commands that opened files no longer shipped. History of Changes,
Shift+F1, opened History.txt and Hotkey Summary, Alt+Shift+H, opened
HotKeys.txt. Both were hand-kept files that the Markdown documentation set
replaced, so on an upgraded installation both commands would have failed with
nothing but a Windows error. They now open History.htm and Hotkeys.htm, and the
audit checks that every document the program opens is one the installer ships.

Removed two variables left behind by the retired WebGet download path, which the
compiler had been warning about on every build.

Fixed the Elevate Version command, F11, which could find a new release but never
install it. It asked GitHub for FileDir_Setup.exe while the installer produced
FileDir_setup.exe, and GitHub download addresses are case sensitive, so every
download failed. A comment in the source said the two names had to match exactly,
and they did not; the audit now compares them at every build.

The build now stamps the version into every document, from version.txt. Typed by
hand, the version line in the ReadMe and the guide went stale within three
builds, which is the same fault the About box had for fourteen releases. The
documents are now one more thing version.txt is the single source for, and the
audit checks that each one carries a line the build can stamp.

Corrected the licence named at the top of this file, which still said the
modified GPL three releases after the change to MIT. The audit had been told to
ignore GNU licence text in the change history, because recording the change
means naming what it changed from, and that whole-file exemption hid a live
claim in the header. The exemption now covers the body only: the opening lines
of every document state what FileDir is licensed under now.

Every log now records the date and size of each script in the build. A fault
that has already been fixed is usually a stale copy of a file, and without this
there is no way to tell that from a fix that did not work. The build also proves
version.txt has no byte order mark after writing it, rather than assuming so.

Fixed two places where the installer worked against itself. It shipped the whole
Scripts folder, including the retired FileDir_Scripts_setup.exe that the
[InstallDelete] section then named -- and since [InstallDelete] runs before the
files are copied, the installer removed that file and put it straight back. It
now ships only the compiled .jsb scripts. The same fault had just been
introduced for Lbc.cs, newly shipped while a leftover line still deleted
lbc.cs, which on Windows is the same file. A new check compares the two
sections and refuses any file named in both.

Stopped sweeping __pycache__ into notes. Python recreates it the moment a script
runs, so every build moved a folder that came straight back and left another
dated copy behind.

Fixed the installer refusing to compile. BuildFileDir wrote version.txt with
Set-Content, which in Windows PowerShell adds a byte order mark. PowerShell
strips that mark again when it reads the file, so nothing looked wrong from
that side, but Inno Setup read it as part of the number and stopped with
"Value of [Setup] section directive VersionInfoVersion is invalid" against a
line that was perfectly correct. The build now reads version.txt tolerantly and
always writes it without a mark, so a file that already has one is repaired by
the next build, and the audit warns when it sees one.

cleanFileDir now untracks what the repository should not carry. Moving a file
into notes stages its own removal, but a file that stays at the root and should
not be tracked was untouched: build output, a generated source, a log, a working
document. .gitignore does nothing for those, since it has no effect on a file
already tracked. On the working folder that was 10 files, and the repository
went from 302 tracked files at the root to 76, every one of them claimed.

Fixed two faults in the audit itself. It failed the build because the installer
names FileDir.exe, which does not exist when the audit runs — the audit runs
before the compile on purpose, so that could never have passed on a clean tree.
And it listed the documents kept at the root in its own code as well as in
cleanFileDir, in neither RepoFiles.txt nor .gitignore, which is the same
two-lists fault the repository rule exists to prevent. There is one list now.

Fixed a build that would not start. The key map generator was a function inside
BuildFileDir.ps1 that emitted C# source, and it wrote the C# quotes as \" --
which is a C escape, not a PowerShell one, since PowerShell escapes a quote with
a backtick. The script therefore would not parse, and PowerShell parses a whole
script before running any of it, so the build never reached its own logging: it
produced a page of parser errors and an empty log. The generator moved to
makeKeyMap.py, where generating another language's source does not fight the
quoting, and the audit now refuses any PowerShell script containing a
backslash-escaped quote so the fault cannot return unnoticed.

Adopted the repository rule worked out in EdSharp, and made the two projects
share the code rather than only the idea. A file belongs only if it is named,
and there are two places to name one: FileDir_setup.iss, which lists everything
FileDir installs, and the new RepoFiles.txt, which lists what the build needs
and what lives here without being tracked. No pattern admits a file by the look
of its name, which is the fault that let saved web pages sit in the EdSharp
repository for weeks while its tidy reported everything clean.

homerPolicy.py holds that rule and is the same file in every Homer Tools
project: nothing in it names FileDir, and it finds the program's name from the
single setup script in the folder. auditFileDir.py and cleanFileDir.py both read
it, so the check and the sweep cannot disagree, and auditFileDir.py follows the
shape of EdSharp's audit so a check written for one project can be moved to the
other.

cleanFileDir now moves everything the project does not claim into a single
notes folder, whatever kind of file it is. The folder is inside the project and
ignored by git, so nothing leaves the machine and nothing is tracked that should
not be. On the working folder that is 222 items moved, leaving 85 at the root,
every one of them claimed. One folder rather than two because nothing reads the
distinction: none of it goes into the repository, and going through it is a job
for a person rather than a script. The audit also reports anything git is tracking
that nothing claims, which is the case .gitignore cannot see, because .gitignore
has no effect on a file that is already tracked.

Fixed one omission the new rule exposed at once: Lbc.cs was compiled into
FileDir.exe but was the single source the installer never shipped, so the source
that came with the program could not be rebuilt from what was there.

Reduced the project to two scripts, which are the only two commands to run:
BuildFileDir and cleanFileDir. The audit and the key map generator had been
separate scripts; both are now inside BuildFileDir, which is a PowerShell script
with a command wrapper of the same name. The build gained the word audit, which
runs the checks and compiles nothing.

Dropped the last references to the legacy download address. The program's
company name and the error dialog both pointed at the original FileDir's home. The reborn FileDir lives
on GitHub, so the unexpected-event dialog now offers Report a Problem, which puts
the message and the version on the clipboard and opens the issues page ready to
paste into. Dead download links in the older history entries are now plain text,
since the addresses no longer answer.

Tightened two rules across every script delivered with FileDir, and made the
audit enforce them. Each PowerShell script now opens its log before doing
anything that could fail, and installs a handler that records the message, the
exception type, the failing line, and the stack trace, so a failure can never
produce a console traceback and an unfinished log. The build script now records
the exit code of every external command it runs, along with the machine, the
user, and the processor architecture. And a delivered script does its job when
run with no parameters: requiring a confirmation word is a manual step in
disguise, and safety belongs in the design instead.

Added cleanFileDir, which moves the twenty years of working material that had
collected in the project folder — saved reference pages, downloaded sample
libraries, retired binaries, test data — into notes. It runs with no parameters
and does the work; nothing is deleted, so a file moved by mistake is one move
back. Add --survey to list what would move without moving it.

## Version 5.0 beta

*June 2026*

Rebuilt FileDir as a 64-bit application on the .NET Framework 4.8, replacing the original 32-bit, .NET 2.0 build.

Began replacing the Layout by Code libraries with the portable Homer helper modules. Screen-reader speech now goes through a UIA notification that JAWS, NVDA, and Narrator announce (Say.cs); configuration gains an optional FileDir.inix overlay over the classic FileDir.ini (Inix.cs); and the network access used by the Elevate Version command moved to Homer.Web (Web.cs).

Replaced the GetText text-extraction utility with 2htm, which converts Word, Excel, PowerPoint, PDF, and Markdown files to accessible HTML.

Added an application manifest (per-monitor DPI awareness and long-path support), an application icon, and a modernized installer with a single Alt+Control+F desktop shortcut.

Refactored the source to a consistent "Camel Type" style and upgraded the documentation from plain text to Markdown (FileDir.md and History.md).

End of Document
