; mpv.jss -- JAWS scripts for the mpv media player.
;
; Copyright 2006-2026 by Jamal Mazrui
; MIT License. See License.md, which carries the terms in full.
;
; Loaded automatically whenever mpv is the active window, because JAWS matches a
; script file to the running program by name: mpv.exe finds mpv.jss.
;
; WHAT THIS IS FOR
;
; mpv is entirely keyboard driven and has no menus, so its commands are
; invisible. There is nothing to explore and nothing to read, and somebody who
; does not already know that 9 lowers the volume cannot find that out from
; inside the player.
;
; So every command appears in a VIRTUAL VIEW as a link. The view is read by
; line, word or character like any other document, and pressing Enter on a line
; runs that command in the player. Nothing has to be remembered, and no new
; keystroke has to be learned: the only key added is the one JAWS already uses
; for hot key help.
;
; HOW THE LINKS WORK, and it is not obvious.
;
; UserBufferAddLink takes the visible text, then a FUNCTION NAME written as a
; string, with its parentheses and arguments, which JAWS calls when Enter is
; pressed on that line. So one dispatcher receives the keystroke to send, and
; there is nothing to keep in step: the link's own target carries the answer.
;
; WHY SOME CALLS SAY Builtin:: AND ONE DOES NOT.
;
; Unqualified names propagate down a script chain and collide silently with
; another script file's. So built-in calls are qualified. But UserBufferAddLink
; resolves one of OUR OWN function names later, and restricting its scope to
; Builtin would restrict that lookup too, where none of our names live. It stays
; unqualified and relies on the mpv prefix instead. This is the reasoning
; HomerView records for the same function, learned the hard way there.

Include "hjconst.jsh"

Void Function mpvSendKey (string sKeys)
; Leave the view and send a keystroke to the player.
;
; The view is closed FIRST. While it is active the keystroke goes to the virtual
; buffer, which swallows it, and nothing reaches mpv at all.
Builtin::UserBufferDeactivate ()
Builtin::TypeKey (sKeys)
EndFunction

Void Function mpvAdd (string sName, string sKeys)
; One command as a link: what it does, the key it uses, and the call that runs
; it.
;
; The name comes first and leads with a verb, so reading down the view and
; moving by first letter both work. The key is shown as well, because somebody
; who uses a command twice would rather press its key the third time than open
; this at all.
Var int iAdded
Let iAdded = UserBufferAddLink (sName + "  (" + sKeys + ")",
    "mpvSendKey (\"" + sKeys + "\")", sName)
EndFunction

Void Function mpvHeading (string sText)
Var int iAdded
Let iAdded = Builtin::UserBufferAddText ("")
Let iAdded = Builtin::UserBufferAddText (sText)
EndFunction

Script HotKeyHelp ()
; The key a JAWS user already presses for this: JAWS key with H.
;
; Not a list of what is BOUND, which JAWS itself reports, but a list of what mpv
; can DO -- which is the part a person cannot otherwise discover.
Var int iAdded, int iActivated
Builtin::UserBufferDeactivate ()
Builtin::UserBufferClear ()
Let iAdded = Builtin::UserBufferAddText ("mpv commands")
Let iAdded = Builtin::UserBufferAddText ("")
Let iAdded = Builtin::UserBufferAddText ("Read this by line, word or character as you would any document. Press Enter on a command to run it in the player. Press Escape to close this and return to mpv.")

mpvHeading ("Playing")
mpvAdd ("Pause or resume", "space")
mpvAdd ("Stop and quit", "q")
mpvAdd ("Quit and remember this position", "shift+q")
mpvAdd ("Step forward one frame", ".")
mpvAdd ("Step back one frame", ",")

mpvHeading ("Moving within what is playing")
mpvAdd ("Seek forward five seconds", "right")
mpvAdd ("Seek back five seconds", "left")
mpvAdd ("Seek forward one minute", "up")
mpvAdd ("Seek back one minute", "down")
mpvAdd ("Seek forward ten minutes", "pagedown")
mpvAdd ("Seek back ten minutes", "pageup")

mpvHeading ("The play list")
mpvAdd ("Play the next item", "shift+.")
mpvAdd ("Play the previous item", "shift+,")
mpvAdd ("Show the play list", "f8")
mpvAdd ("Add the clipboard to the play list", "ctrl+v")
mpvAdd ("Reload the current item", "ctrl+r")

mpvHeading ("Volume")
mpvAdd ("Louder", "0")
mpvAdd ("Quieter", "9")
mpvAdd ("Mute or unmute", "m")

mpvHeading ("Speed")
mpvAdd ("Faster", "]")
mpvAdd ("Slower", "[")
mpvAdd ("Much faster", "shift+]")
mpvAdd ("Much slower", "shift+[")
mpvAdd ("Back to normal speed", "backspace")

mpvHeading ("Tracks and subtitles")
mpvAdd ("Next audio track", "shift+3")
mpvAdd ("Next subtitle track", "j")
mpvAdd ("Previous subtitle track", "shift+j")
mpvAdd ("Subtitles on or off", "v")
mpvAdd ("Show audio and subtitle streams", "f9")

mpvHeading ("What is playing")
mpvAdd ("Show progress and file name", "o")
mpvAdd ("Show statistics", "i")
mpvAdd ("Keep statistics on screen", "shift+i")

mpvHeading ("The window")
mpvAdd ("Full screen on or off", "f")
mpvAdd ("Take a screen shot", "s")
mpvAdd ("Keep on top of other windows", "shift+t")

Let iActivated = Builtin::UserBufferActivate ()
EndScript
