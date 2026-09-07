# buildTutorial.ps1 -- write the tutorials and speak them, in one command.
#
#   buildTutorial                    every Tutorial*.inix: documents, then audio
#   buildTutorial Tutorial_Tagging   just that one
#   buildTutorial -docs              documents and feed only, no speaking
#   buildTutorial -sapi              use Windows voices; fetch nothing
#   buildTutorial -live              perform it now through JAWS, write no file
#
# WHAT IT DOES, IN ORDER
#
#   1. Makes sure there are two voices worth listening to (see below).
#   2. Runs makeTutorial.py: the sections of Tutorials.md and the feed.
#   3. Speaks each script into its own .mp3.
#   4. Runs makeTutorial.py again, so the feed picks up the audio just made.
#
# THE TWO VOICES, AND WHY THESE
#
# A walkthrough has two speakers and they must never be confused, because the
# whole point is knowing which words came from the program. So one voice should
# sound like a person and the other should sound like a screen reader.
#
# The narrator is PIPER, with the en_US-lessac-medium voice: a small neural
# engine from the Rhasspy project, free, MIT-licensed, entirely offline once
# fetched, and the best free English narration available for Windows. NVDA
# add-ons and other accessibility software use the same engine, so it is a
# known quantity rather than a novelty.
#
# The screen reader's stand-in is ESPEAK NG: free, tiny, and unmistakably
# synthetic -- the voice a listener recognises as a machine within two words.
# That is not a shortcoming here; it is the requirement.
#
# Neither is installed unless it is missing, both are fetched from their own
# projects, and -sapi skips all of it and uses the Windows voices instead. The
# log records every URL, every size and every exit code.

$ErrorActionPreference = "Stop"
$sHere = Split-Path -Parent $MyInvocation.MyCommand.Path
$sLog = Join-Path $sHere "buildTutorial.log"
$sTools = Join-Path $sHere "voices"

trap {
  $sWhere = ""
  try { $sWhere = " at line " + $_.InvocationInfo.ScriptLineNumber } catch { }
  try {
    Add-Content -LiteralPath $sLog -Value ((Get-Date).ToString("yyyy-MM-dd HH:mm:ss") + "  UNEXPECTED: " + $_.Exception.Message + $sWhere)
    Add-Content -LiteralPath $sLog -Value ($_.ScriptStackTrace)
  } catch { }
  Write-Host "Something unexpected stopped the script. The log has it."
  exit 1
}

function note([string] $sText) {
  Add-Content -LiteralPath $sLog -Value ((Get-Date).ToString("yyyy-MM-dd HH:mm:ss") + "  " + $sText)
}

function say([string] $sText) {
  Write-Host $sText
  note $sText
}

if (Test-Path -LiteralPath $sLog) { Remove-Item -LiteralPath $sLog -Force }
note "buildTutorial starting"
note ("script: " + $MyInvocation.MyCommand.Path)
note ("PowerShell: " + $PSVersionTable.PSVersion.ToString())
note ("platform: " + [Environment]::OSVersion.VersionString)
note ("working directory: " + (Get-Location).Path)
note ("command line: " + [Environment]::CommandLine)

# ---- what was asked for ----

$bDocsOnly = $false
$bSapi = $false
$bLive = $false
$sOnly = ""
foreach ($sArg in $args) {
  $sTrimmed = ("" + $sArg).Trim()
  if ($sTrimmed.Length -eq 0) { continue }
  if ($sTrimmed -eq "-docs") { $bDocsOnly = $true; continue }
  if ($sTrimmed -eq "-sapi") { $bSapi = $true; continue }
  if ($sTrimmed -eq "-live") { $bLive = $true; continue }
  if ($sTrimmed.StartsWith("-")) { note ("ignoring unknown switch " + $sTrimmed); continue }
  $sOnly = [System.IO.Path]::GetFileNameWithoutExtension($sTrimmed)
}
note ("docs only: " + $bDocsOnly + ", Windows voices: " + $bSapi + ", live: " + $bLive + ", only: " + $sOnly)

# ---- the scripts to build ----

$lsScripts = @()
if ($sOnly -ne "") {
  $sOne = Join-Path $sHere ($sOnly + ".inix")
  if (-not (Test-Path -LiteralPath $sOne)) { say ($sOnly + ".inix is not here."); exit 1 }
  $lsScripts = @($sOne)
}
else {
  $lsScripts = @(Get-ChildItem -LiteralPath $sHere -Filter "Tutorial*.inix" | Sort-Object Name | ForEach-Object { $_.FullName })
}
if ($lsScripts.Count -eq 0) { say "0 tutorial scripts here."; exit 1 }
note ("scripts: " + (($lsScripts | ForEach-Object { [System.IO.Path]::GetFileName($_) }) -join ", "))

# ---- step 1 of 4: the documents ----

function runMake() {
  $sPython = ""
  foreach ($sTry in @("python.exe", "py.exe")) {
    $oFound = Get-Command $sTry -ErrorAction SilentlyContinue
    if ($oFound -and $sPython -eq "") { $sPython = $oFound.Source }
  }
  if ($sPython -eq "") { say "Python was not found, so the documents cannot be written."; return $false }
  note ("Python: " + $sPython)
  & $sPython (Join-Path $sHere "makeTutorial.py") 2>&1 | ForEach-Object { note ("  | " + $_) }
  note ("makeTutorial.py exit code: " + $LASTEXITCODE)
  return ($LASTEXITCODE -eq 0)
}

say "Writing the tutorials into Tutorials.md ..."
if (-not (runMake)) { say "The documents could not be written. The log has why."; exit 1 }
if ($bDocsOnly) { say "Documents only, as asked. Nothing was spoken."; exit 0 }

# ---- step 2 of 4: the voices ----

$sFfmpeg = Join-Path $sHere "ffmpeg.exe"
if (-not (Test-Path -LiteralPath $sFfmpeg)) {
  $oFound = Get-Command ffmpeg.exe -ErrorAction SilentlyContinue
  if ($oFound) { $sFfmpeg = $oFound.Source }
}
note ("ffmpeg: " + $sFfmpeg)
if (-not $bLive -and -not (Test-Path -LiteralPath $sFfmpeg)) {
  say "ffmpeg was not found, and it is what joins the pieces into one file."
  say "Run installMediaTools.cmd in this folder."
  exit 1
}

function fetchTo([string] $sUrl, [string] $sPath) {
  # One download, with the whole story in the log: where from, where to, and
  # how big it turned out to be.
  note ("fetching " + $sUrl)
  note ("      to " + $sPath)
  try {
    $oOld = $ProgressPreference
    $ProgressPreference = "SilentlyContinue"
    Invoke-WebRequest -Uri $sUrl -OutFile $sPath -UseBasicParsing
    $ProgressPreference = $oOld
    note ("fetched " + (Get-Item -LiteralPath $sPath).Length + " bytes")
    return $true
  }
  catch {
    note ("fetch failed: " + $_.Exception.Message)
    return $false
  }
}

$sPiper = ""
$sPiperVoice = ""
$sEspeak = ""

if (-not $bSapi -and -not $bLive) {
  New-Item -ItemType Directory -Path $sTools -Force | Out-Null

  # PIPER: the narrator. A release zip and one voice, both fetched once.
  $sPiper = Join-Path $sTools "piper\piper.exe"
  if (-not (Test-Path -LiteralPath $sPiper)) {
    say "Fetching the narrator voice. This happens once."
    $sZip = Join-Path $sTools "piper.zip"
    if (fetchTo "https://github.com/rhasspy/piper/releases/latest/download/piper_windows_amd64.zip" $sZip) {
      try {
        Expand-Archive -LiteralPath $sZip -DestinationPath $sTools -Force
        Remove-Item -LiteralPath $sZip -Force
      }
      catch { note ("could not unpack piper: " + $_.Exception.Message) }
    }
  }
  if (-not (Test-Path -LiteralPath $sPiper)) {
    $oFound = Get-ChildItem -LiteralPath $sTools -Filter "piper.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($oFound) { $sPiper = $oFound.FullName }
  }
  note ("piper: " + $sPiper + ", present: " + (Test-Path -LiteralPath $sPiper))

  if (Test-Path -LiteralPath $sPiper) {
    $sPiperVoice = Join-Path $sTools "en_US-lessac-medium.onnx"
    $sPiperJson = $sPiperVoice + ".json"
    $sVoiceBase = "https://huggingface.co/rhasspy/piper-voices/resolve/main/en/en_US/lessac/medium/"
    if (-not (Test-Path -LiteralPath $sPiperVoice)) {
      if (-not (fetchTo ($sVoiceBase + "en_US-lessac-medium.onnx") $sPiperVoice)) { $sPiperVoice = "" }
    }
    if ($sPiperVoice -ne "" -and -not (Test-Path -LiteralPath $sPiperJson)) {
      if (-not (fetchTo ($sVoiceBase + "en_US-lessac-medium.onnx.json") $sPiperJson)) { $sPiperVoice = "" }
    }
  }
  else { $sPiper = "" }

  # ESPEAK NG: the screen reader's stand-in. Already on the machine more often
  # than not, since other accessibility software carries it.
  $oFound = Get-Command espeak-ng.exe -ErrorAction SilentlyContinue
  if ($oFound) { $sEspeak = $oFound.Source }
  if ($sEspeak -eq "") {
    foreach ($sTry in @("$env:ProgramFiles\eSpeak NG\espeak-ng.exe",
                        "${env:ProgramFiles(x86)}\eSpeak NG\espeak-ng.exe")) {
      if ($sEspeak -eq "" -and (Test-Path -LiteralPath $sTry)) { $sEspeak = $sTry }
    }
  }
  if ($sEspeak -eq "" -and (Get-Command winget.exe -ErrorAction SilentlyContinue)) {
    say "Fetching the screen reader voice. This happens once."
    & winget.exe install --id eSpeak-NG.eSpeak-NG --accept-package-agreements --accept-source-agreements --silent 2>&1 |
      ForEach-Object { note ("  | " + $_) }
    note ("winget exit code: " + $LASTEXITCODE)
    foreach ($sTry in @("$env:ProgramFiles\eSpeak NG\espeak-ng.exe",
                        "${env:ProgramFiles(x86)}\eSpeak NG\espeak-ng.exe")) {
      if ($sEspeak -eq "" -and (Test-Path -LiteralPath $sTry)) { $sEspeak = $sTry }
    }
  }
  note ("espeak-ng: " + $sEspeak)
}

$bWindowsVoices = ($sPiper -eq "" -or $sPiperVoice -eq "" -or $sEspeak -eq "")
if ($bWindowsVoices -and -not $bLive) {
  if (-not $bSapi) { say "Falling back to the Windows voices; the log says which fetch fell short." }
  note ("using Windows voices: piper=" + $sPiper + " voice=" + $sPiperVoice + " espeak=" + $sEspeak)
}

# Windows voices are needed for the fallback and for the live narration.
Add-Type -AssemblyName System.Speech
$oSpeaker = New-Object System.Speech.Synthesis.SpeechSynthesizer
$lsVoices = @($oSpeaker.GetInstalledVoices() | Where-Object { $_.Enabled } | ForEach-Object { $_.VoiceInfo.Name })
note ("Windows voices: " + ($lsVoices -join ", "))
$sSapiNarrator = if ($lsVoices.Count -gt 0) { $lsVoices[0] } else { "" }
$sSapiReader = ""
foreach ($sWanted in @("eloquence", "eti-eloquence", "ibmtts")) {
  foreach ($sVoice in $lsVoices) { if ($sSapiReader -eq "" -and $sVoice.ToLower().Contains($sWanted)) { $sSapiReader = $sVoice } }
}
if ($sSapiReader -eq "" -and $lsVoices.Count -gt 1) { $sSapiReader = $lsVoices[1] }
if ($sSapiReader -eq "") { $sSapiReader = $sSapiNarrator }
note ("Windows narrator: " + $sSapiNarrator + ", Windows reader: " + $sSapiReader)

$oJaws = $null
if ($bLive) {
  try { $oJaws = New-Object -ComObject FreedomSci.JawsApi; note "JAWS COM server attached" }
  catch {
    note ("JAWS COM server not available: " + $_.Exception.Message)
    say "JAWS is not running, or its COM server is not available."
    exit 1
  }
}

# SPEED. A screen reader user listens faster than a first-time listener. Piper
# takes a length scale, where less is quicker; eSpeak takes words a minute.
$dNarratorScale = 0.88
$iEspeakRate = 260
$iSapiNarratorRate = 4
$iSapiReaderRate = 6

# ---- reading a SPEAK script ----

function readScript([string] $sPath) {
  $lsSections = New-Object System.Collections.Generic.List[hashtable]
  $dNow = $null
  foreach ($sRaw in (Get-Content -LiteralPath $sPath -Encoding UTF8)) {
    $sLine = $sRaw.Trim()
    if ($sLine.Length -eq 0 -or $sLine.StartsWith(";") -or $sLine.StartsWith("#")) { continue }
    if ($sLine.StartsWith("[") -and $sLine.EndsWith("]")) {
      $dNow = @{ "_name" = $sLine.Substring(1, $sLine.Length - 2).Trim().ToLower() }
      $lsSections.Add($dNow)
      continue
    }
    if ($null -eq $dNow) { continue }
    $iAt = $sLine.IndexOf("=")
    if ($iAt -lt 1) { continue }
    $sField = $sLine.Substring(0, $iAt).Trim()
    $sValue = $sLine.Substring($iAt + 1).Trim()
    if ($sValue.Length -eq 0) { continue }
    if (-not $dNow.ContainsKey($sField)) { $dNow[$sField] = New-Object System.Collections.Generic.List[string] }
    $dNow[$sField].Add($sValue)
  }
  return $lsSections
}

# ---- speaking one tutorial ----

function buildOne([string] $sScript) {
  $sStem = [System.IO.Path]::GetFileNameWithoutExtension($sScript)
  $sOut = Join-Path $sHere ($sStem + ".mp3")
  $sWork = Join-Path $env:TEMP ("buildTutorial_" + [Guid]::NewGuid().ToString("N"))
  $script:iPiece = 0
  $script:lsPieces = New-Object System.Collections.Generic.List[string]
  if (-not $bLive) { New-Item -ItemType Directory -Path $sWork -Force | Out-Null }
  note ("building " + $sStem + ", work folder " + $sWork)

  function pieceFile() {
    $script:iPiece = $script:iPiece + 1
    return (Join-Path $sWork ("piece_{0:D4}.wav" -f $script:iPiece))
  }

  function speakNarrator([string] $sText) {
    if ($bLive) {
      $oSpeaker.SelectVoice($sSapiNarrator)
      $oSpeaker.Rate = $iSapiNarratorRate
      $oSpeaker.SetOutputToDefaultAudioDevice()
      $oSpeaker.Speak($sText)
      note ("live narrator: " + $sText)
      return
    }
    $sFile = pieceFile
    if (-not $bWindowsVoices) {
      # Piper reads its text from standard input and writes one wave file.
      $sText | & $sPiper -m $sPiperVoice --length_scale $dNarratorScale -f $sFile 2>&1 |
        ForEach-Object { note ("  piper | " + $_) }
    }
    else {
      $oSpeaker.SelectVoice($sSapiNarrator)
      $oSpeaker.Rate = $iSapiNarratorRate
      $oSpeaker.SetOutputToWaveFile($sFile)
      $oSpeaker.Speak($sText)
      $oSpeaker.SetOutputToNull()
    }
    if (Test-Path -LiteralPath $sFile) { $script:lsPieces.Add($sFile) }
    else { note ("no audio made for: " + $sText) }
    note ("narrator: " + $sText)
  }

  function speakReader([string] $sText) {
    if ($bLive) {
      $oJaws.SayString($sText, $true) | Out-Null
      Start-Sleep -Milliseconds ([Math]::Max(400, $sText.Length * 38))
      note ("live JAWS: " + $sText)
      return
    }
    $sFile = pieceFile
    if (-not $bWindowsVoices) {
      & $sEspeak -v en-us -s $iEspeakRate -w $sFile $sText 2>&1 | ForEach-Object { note ("  espeak | " + $_) }
    }
    else {
      $oSpeaker.SelectVoice($sSapiReader)
      $oSpeaker.Rate = $iSapiReaderRate
      $oSpeaker.SetOutputToWaveFile($sFile)
      $oSpeaker.Speak($sText)
      $oSpeaker.SetOutputToNull()
    }
    if (Test-Path -LiteralPath $sFile) { $script:lsPieces.Add($sFile) }
    else { note ("no audio made for: " + $sText) }
    note ("reader: " + $sText)
  }

  function gap([double] $dSeconds) {
    if ($bLive) { Start-Sleep -Milliseconds ([int]($dSeconds * 1000)); return }
    $sFile = pieceFile
    & $sFfmpeg -y -loglevel error -f lavfi -i anullsrc=r=22050:cl=mono -t $dSeconds $sFile 2>&1 | Out-Null
    if (Test-Path -LiteralPath $sFile) { $script:lsPieces.Add($sFile) }
  }

  $lsSections = readScript $sScript
  $dAbout = $lsSections | Where-Object { $_["_name"] -eq "about" } | Select-Object -First 1
  $lsSteps = @($lsSections | Where-Object { $_["_name"] -eq "step" })
  if ($lsSteps.Count -eq 0) { say ($sStem + " holds 0 steps."); return $false }
  note ($sStem + ": steps " + $lsSteps.Count)

  speakNarrator "Simulated walk through FileDir. Not a recording. I am Homer, your narrator; the other voice stands in for the screen reader."
  gap 0.45
  if ($dAbout -and $dAbout.ContainsKey("Title")) { speakNarrator $dAbout["Title"][0]; gap 0.35 }
  if ($dAbout -and $dAbout.ContainsKey("Setup")) { speakNarrator $dAbout["Setup"][0]; gap 0.45 }

  foreach ($dStep in $lsSteps) {
    if ($dStep.ContainsKey("Say")) { speakNarrator $dStep["Say"][0]; gap 0.3 }
    if ($dStep.ContainsKey("Key")) { speakNarrator ("Press " + $dStep["Key"][0]); gap 0.35 }
    if ($dStep.ContainsKey("Hear")) {
      foreach ($sHeard in $dStep["Hear"]) { speakReader $sHeard }
      gap 0.4
    }
  }
  speakNarrator "End of the walk."
  if ($dAbout -and $dAbout.ContainsKey("Homework")) {
    gap 0.35
    speakNarrator ("Something to try. " + $dAbout["Homework"][0])
  }

  if ($bLive) { say ($sStem + " was spoken live. No file was written."); return $true }

  note ("pieces: " + $script:lsPieces.Count)
  $sList = Join-Path $sWork "pieces.txt"
  $lsLines = $script:lsPieces | ForEach-Object { "file '" + $_.Replace("'", "'\''") + "'" }
  Set-Content -LiteralPath $sList -Value $lsLines -Encoding ASCII
  & $sFfmpeg -y -loglevel error -f concat -safe 0 -i $sList -ar 22050 -ac 1 -codec:a libmp3lame -q:a 4 $sOut 2>&1 |
    ForEach-Object { note ("ffmpeg | " + $_) }
  $iExit = $LASTEXITCODE
  note ("ffmpeg exit code: " + $iExit)
  try { Remove-Item -LiteralPath $sWork -Recurse -Force } catch { note ("could not clear " + $sWork) }
  if ($iExit -ne 0 -or -not (Test-Path -LiteralPath $sOut)) { say ($sStem + " could not be joined."); return $false }
  say ("Wrote " + [System.IO.Path]::GetFileName($sOut))
  return $true
}

# ---- step 3 of 4: speak them ----

$iDone = 0
foreach ($sScript in $lsScripts) {
  if (buildOne $sScript) { $iDone = $iDone + 1 }
}
$oSpeaker.Dispose()

# ---- step 4 of 4: the feed, now that the audio exists ----

if (-not $bLive -and $iDone -gt 0) {
  say "Writing the feed ..."
  runMake | Out-Null
}

say ($iDone.ToString() + " of " + $lsScripts.Count + " tutorials built.")
if ($iDone -lt $lsScripts.Count) { exit 1 }
exit 0
