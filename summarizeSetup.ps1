# summarizeSetup.ps1 -- the single Results box, shown after everything.
#
# WHAT THE BOX SAYS
#
# Only what this installation session actually DID. A component that was
# already there and needed nothing is not mentioned: reading nine lines saying
# "installed" to find the one line that changed is work, and the person did not
# ask for an inventory. Each component script writes one plain line to
# FileDir_setup_actions.txt when it installs, updates, reinstalls or fails, and
# those lines are the box.
#
# The full inventory -- every component, where it is and which version -- still
# goes to the LOG, every time. That is what a support conversation needs, and
# the log is where technical detail belongs.
#
# WHY POWERSHELL RATHER THAN A BATCH FILE
#
# EdSharp's batch version of this summary died with exit code 255 partway
# through its first run, taking the Results box with it and leaving the
# installation looking as though it had vanished: it runs hidden, so there
# was no window to find. Batch has no way to bound a command that hangs, no
# reliable quoting for the text these lines carry, and a missing label ends
# the whole script without a word. This does the same job with none of that:
# every probe is given a time limit, every line is written the moment it is
# produced, and any failure is reported inside the box rather than ending it.
#
# Run by summarizeSetup.cmd, which supplies the PowerShell parameters.

param([switch]$bQuiet)

$sLogDir = Join-Path $env:LOCALAPPDATA "FileDir\logs"
$sLogFile = Join-Path $sLogDir "FileDir_setup.log"
$sSummaryFile = Join-Path $sLogDir "FileDir_setup_summary.txt"
$sResultsFile = Join-Path $sLogDir "FileDir_setup_results.txt"
$sActionsFile = Join-Path $sLogDir "FileDir_setup_actions.txt"
$lLines = @()

function say($sText) {
  # A line for the person: the box, the summary file, and the log.
  # Straight to disk as each line is produced: a summary still in memory when
  # something goes wrong is a summary nobody can read.
  $script:lLines += $sText
  try { Add-Content -LiteralPath $sSummaryFile -Value $sText -Encoding UTF8 } catch { }
  try { if ($sText.Trim() -ne "") { Add-Content -LiteralPath $sLogFile -Value "[summary] $sText" -Encoding UTF8 } } catch { }
}

function note($sText) {
  # A line for the log alone. Everything technical comes here.
  try { Add-Content -LiteralPath $sLogFile -Value "[inventory] $sText" -Encoding UTF8 } catch { }
}

function startHidden($sExe, $lArguments, $sOutFile, $sErrFile) {
  # A process with NO WINDOW. Start-Process cannot manage this once output is
  # redirected: it ignores -WindowStyle Hidden, and a console appears -- which
  # is how an Ollama window came to open during EdSharp's setup. The .NET
  # process object honours CreateNoWindow.
  $oInfo = New-Object System.Diagnostics.ProcessStartInfo
  $oInfo.FileName = $sExe
  $oInfo.Arguments = ($lArguments -join " ")
  $oInfo.UseShellExecute = $false
  $oInfo.CreateNoWindow = $true
  if ($sOutFile -ne "") {
    $oInfo.RedirectStandardOutput = $true
    $oInfo.RedirectStandardError = $true
  }
  $oProcess = New-Object System.Diagnostics.Process
  $oProcess.StartInfo = $oInfo
  [void]$oProcess.Start()
  if ($sOutFile -ne "") {
    # Both streams are read before waiting: a full pipe buffer would otherwise
    # deadlock the wait that follows.
    $sOut = $oProcess.StandardOutput.ReadToEnd()
    $sErr = $oProcess.StandardError.ReadToEnd()
    try { Set-Content -LiteralPath $sOutFile -Value $sOut -Encoding UTF8 } catch { }
    if ($sErrFile -ne "") { try { Set-Content -LiteralPath $sErrFile -Value $sErr -Encoding UTF8 } catch { } }
  }
  return $oProcess
}

function runBounded($sExe, $lArguments, $iSeconds) {
  # Run a command, wait no longer than $iSeconds, and return its first line of
  # output. A tool that hangs -- some report versions over the network -- must
  # never hold up the installation.
  $lQuoted = @()
  foreach ($sArgument in $lArguments) {
    if ($sArgument -match "\s") { $lQuoted += ('"' + $sArgument + '"') } else { $lQuoted += $sArgument }
  }
  $lArguments = $lQuoted
  $sOutFile = Join-Path $sLogDir ("FileDir_probe_" + [guid]::NewGuid().ToString("N") + ".tmp")
  try {
    $oProcess = startHidden $sExe $lArguments $sOutFile ($sOutFile + ".err")
    if (-not $oProcess.WaitForExit($iSeconds * 1000)) {
      try { $oProcess.Kill() } catch { }
      return ""
    }
    $sText = ""
    if (Test-Path -LiteralPath $sOutFile) { $sText = (Get-Content -LiteralPath $sOutFile -ErrorAction SilentlyContinue) -join "`n" }
    if ($sText.Trim() -eq "" -and (Test-Path -LiteralPath ($sOutFile + ".err"))) { $sText = (Get-Content -LiteralPath ($sOutFile + ".err") -ErrorAction SilentlyContinue) -join "`n" }
    foreach ($sLine in $sText -split "`n") { if ($sLine.Trim() -ne "") { return $sLine.Trim() } }
    return ""
  } catch {
    return ""
  } finally {
    foreach ($sLeftover in @($sOutFile, ($sOutFile + ".err"))) {
      try { if (Test-Path -LiteralPath $sLeftover) { Remove-Item -LiteralPath $sLeftover -Force } } catch { }
    }
  }
}

function findExe($sName) {
  # Everywhere a tool might be, not merely on this process's path. A tool
  # installed minutes ago is not on the path of a process that started before
  # it -- which is how the installer offered "Reinstall mpv" while this summary
  # said it was not installed. Both were true of their own environment.
  #
  # THE FOLDER AN INSTALLER ACTUALLY USES IS NOT THE COMMAND'S NAME. mpv's
  # installer creates "MPV Player"; looking only in a folder called "mpv"
  # missed it and reported a program that was plainly there as not installed.
  $sPrograms = Join-Path $env:LOCALAPPDATA "Programs"
  $sWinGet = Join-Path $env:LOCALAPPDATA "Microsoft\WinGet"
  $dOfficial = @{
    "mpv"      = @("MPV Player", "mpv", "MPV Media Player", "mpv.net")
    "pandoc"   = @("Pandoc")
    "exiftool" = @("ExifTool")
    "ffmpeg"   = @("ffmpeg")
    "ffprobe"  = @("ffmpeg")
    "magick"   = @("ImageMagick")
    "yt-dlp"   = @("yt-dlp")
  }
  $lCandidates = @()
  foreach ($sFolder in ($dOfficial[$sName] + @($sName))) {
    if (-not $sFolder) { continue }
    foreach ($sRoot in @($env:ProgramFiles, ${env:ProgramFiles(x86)}, $sPrograms)) {
      if (-not $sRoot) { continue }
      $lCandidates += (Join-Path (Join-Path $sRoot $sFolder) ($sName + ".exe"))
      $lCandidates += (Join-Path (Join-Path (Join-Path $sRoot $sFolder) "bin") ($sName + ".exe"))
    }
  }
  $lCandidates += (Join-Path (Join-Path $sPrograms "Ollama") ($sName + ".exe"))
  # Both WinGet link folders. The machine one is where a portable package
  # installed by an elevated setup lands, and it was missing here.
  $lCandidates += (Join-Path (Join-Path $sWinGet "Links") ($sName + ".exe"))
  if ($env:ProgramFiles) { $lCandidates += (Join-Path (Join-Path $env:ProgramFiles "WinGet\Links") ($sName + ".exe")) }
  foreach ($sPath in $lCandidates) {
    if (Test-Path -LiteralPath $sPath) { return $sPath }
  }
  $oFound = Get-Command ($sName + ".exe") -ErrorAction SilentlyContinue
  if ($oFound) { return $oFound.Source }
  # Last, the folders winget unpacks a portable package into. Their names carry
  # the publisher and version, so they are searched rather than guessed.
  $lPackageRoots = @((Join-Path $sWinGet "Packages"))
  if ($env:ProgramFiles) { $lPackageRoots += (Join-Path $env:ProgramFiles "WinGet\Packages") }
  foreach ($sPackages in $lPackageRoots) {
    if (-not (Test-Path -LiteralPath $sPackages)) { continue }
    try {
      $oExe = Get-ChildItem -LiteralPath $sPackages -Filter ($sName + ".exe") -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
      if ($oExe) { return $oExe.FullName }
    } catch { }
  }
  return ""
}

function findPandoc() {
  # Machine-wide, in Program Files, which is where Pandoc's own installer puts
  # it and where every Homer Tools program looks. Checked by path as well as by
  # name, because a Pandoc installed a minute ago is not on this process's path.
  $sPath = Join-Path $env:ProgramFiles "Pandoc\pandoc.exe"
  if (Test-Path -LiteralPath $sPath) { return $sPath }
  $oFound = Get-Command "pandoc.exe" -ErrorAction SilentlyContinue
  if ($oFound) { return $oFound.Source }
  return ""
}

function notePdfReader() {
  # Asked of the interpreter that installed it, since a machine may carry
  # several Pythons and only one of them will have the package.
  $sPython = "python"
  $sRecord = Join-Path $sLogDir "FileDir_python.txt"
  if (Test-Path -LiteralPath $sRecord) {
    $sNoted = (Get-Content -LiteralPath $sRecord -ErrorAction SilentlyContinue | Select-Object -First 1)
    if ($sNoted) { $sPython = $sNoted.Trim() }
  }
  $sAnswer = runBounded $sPython @("-c", "import pymupdf4llm; print('ready')") 40
  if ($sAnswer -match "ready") { note "PDF reader (PyMuPDF4LLM): installed, read by $sPython" }
  else { note "PDF reader (PyMuPDF4LLM): not installed" }
}

function notePandoc() {
  $sPandoc = findPandoc
  if ($sPandoc -eq "") { note "Pandoc: not installed"; return }
  $sVersion = runBounded $sPandoc @("--version") 30
  note "Pandoc: installed, $sVersion at $sPandoc"
}

function noteTool($sLabel, $sName) {
  $sExe = findExe $sName
  if ($sExe -eq "") { note "$sLabel`: not installed"; return }
  # Not every tool answers --version. ExifTool prints a manual page header to
  # that, which is where the reported version "NAME" came from: the first line
  # of its own documentation. It answers -ver with a bare number.
  $lVersionArgs = @("--version")
  if ($sName -eq "exiftool") { $lVersionArgs = @("-ver") }
  $sVersion = runBounded $sExe $lVersionArgs 20
  if ($sVersion -eq "") { note "$sLabel`: installed at $sExe"; return }
  note "$sLabel`: installed, $sVersion at $sExe"
}

function ollamaModels() {
  # Ollama's own web interface, which answers without starting anything.
  # Running the command line client instead can start the server in a console
  # of its own, and that window on screen during setup looks like a fault.
  try {
    $oTags = Invoke-RestMethod -Uri "http://localhost:11434/api/tags" -TimeoutSec 10
    return (($oTags.models | ForEach-Object { $_.name }) -join " ")
  } catch {
    return ""
  }
}

function noteInventory() {
  # The whole picture, for the log only. This is what a support conversation
  # needs, and none of it belongs in a box about what just happened.
  note "---- components on this computer ----"
  notePandoc
  noteTool "ExifTool" "exiftool"
  noteTool "ffmpeg" "ffmpeg"
  noteTool "yt-dlp" "yt-dlp"
  notePdfReader
  noteTool "ImageMagick" "magick"
  noteTool "mpv" "mpv"
  noteTool "Ollama" "ollama"
  # One probe, read whole: the listing is short, and asking twice would double
  # the wait on a cold service.
  $sList = ollamaModels
  if ((findExe "ollama") -ne "") {
    if ($sList -eq "") { note "Ollama models: could not be listed" }
    else { note "Ollama models: $sList" }
  }
}

function sayActions() {
  # One line per thing this session did, written by the component scripts and
  # by the installer itself. No file, or an empty one, means nothing changed.
  $lActions = @()
  if (Test-Path -LiteralPath $sActionsFile) {
    # Read with the system's own encoding, which is what wrote them; a wrong
    # guess here would turn the lines into nonsense.
    foreach ($sLine in @(Get-Content -LiteralPath $sActionsFile -Encoding Default -ErrorAction SilentlyContinue)) {
      if ($sLine.Trim() -ne "") { $lActions += $sLine.Trim() }
    }
  }
  say "Actions"
  if ($lActions.Count -eq 0) {
    say "  Nothing needed changing."
  }
  else {
    foreach ($sAction in $lActions) { say ("  " + $sAction) }
  }
  # Removed once read, so the next installation starts from an empty sheet even
  # if it never reaches the point where the installer clears it.
  try { if (Test-Path -LiteralPath $sActionsFile) { Remove-Item -LiteralPath $sActionsFile -Force } } catch { }
}

function main() {
  try { New-Item -ItemType Directory -Force -Path $sLogDir | Out-Null } catch { }
  try { if (Test-Path -LiteralPath $sSummaryFile) { Remove-Item -LiteralPath $sSummaryFile -Force } } catch { }

  say ("FileDir setup results  " + (Get-Date).ToString("yyyy-MM-dd HH:mm"))
  say ""

  # What the installer knew before the checkboxes ran, handed over in a file so
  # that ONE box tells the whole story instead of two telling halves.
  if (Test-Path -LiteralPath $sResultsFile) {
    $lResults = @(Get-Content -LiteralPath $sResultsFile -Encoding Default -ErrorAction SilentlyContinue)
    # Trailing blank lines are dropped before the block is printed. One blank
    # line between sections is a separator; three is a gap to arrow past.
    while (($lResults.Count -gt 0) -and ($lResults[$lResults.Count - 1].Trim() -eq "")) {
      $lResults = $lResults[0..($lResults.Count - 2)]
    }
    foreach ($sLine in $lResults) { say $sLine }
    try { Remove-Item -LiteralPath $sResultsFile -Force } catch { }
    say ""
  }

  sayActions

  # The inventory is gathered AFTER the box's own lines are settled, so a slow
  # probe cannot delay what the person is waiting to read.
  noteInventory

  say ""
  say "Full log: $sLogFile"
  say ""
  say "To start FileDir, press Alt+Control+F."

  if (-not $bQuiet) {
    try {
      Add-Type -AssemblyName System.Windows.Forms
      [void][System.Windows.Forms.MessageBox]::Show(($lLines -join "`r`n"), "FileDir Setup Results")
    } catch {
      # The box is a courtesy; the file is the record.
      try { Add-Content -LiteralPath $sLogFile -Value "[summary] the results box could not be shown: $($_.Exception.Message)" } catch { }
    }
  }
}

try {
  main
  exit 0
} catch {
  try {
    Add-Content -LiteralPath $sLogFile -Value "[summary] FAILED: $($_.Exception.Message)"
    Add-Type -AssemblyName System.Windows.Forms
    [void][System.Windows.Forms.MessageBox]::Show("The setup summary could not be completed:`r`n" + $_.Exception.Message + "`r`n`r`nThe log is:`r`n" + $sLogFile, "FileDir Setup Results")
  } catch { }
  exit 0
}
