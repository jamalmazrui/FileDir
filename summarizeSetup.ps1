# summarizeSetup.ps1 -- the single Results box, shown after everything.
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
$lLines = @()

function say($sText) {
  # Straight to disk as each line is produced: a summary still in memory when
  # something goes wrong is a summary nobody can read.
  $script:lLines += $sText
  try { Add-Content -LiteralPath $sSummaryFile -Value $sText -Encoding UTF8 } catch { }
  try { if ($sText.Trim() -ne "") { Add-Content -LiteralPath $sLogFile -Value "[summary] $sText" -Encoding UTF8 } } catch { }
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
  # The program's own folder is searched as well as the path, because a tool
  # installed minutes ago is not on this process's path yet. Ollama installs
  # per user, so its profile copy is tried by name.
  $sPrograms = Join-Path $env:LOCALAPPDATA "Programs"
  $lCandidates = @(
    (Join-Path (Join-Path $sPrograms $sName) ($sName + ".exe")),
    (Join-Path (Join-Path $sPrograms "Ollama") ($sName + ".exe"))
  )
  foreach ($sPath in $lCandidates) {
    if (Test-Path -LiteralPath $sPath) { return $sPath }
  }
  $oFound = Get-Command ($sName + ".exe") -ErrorAction SilentlyContinue
  if ($oFound) { return $oFound.Source }
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

function reportPandoc() {
  $sPandoc = findPandoc
  if ($sPandoc -eq "") {
    say "Pandoc: not installed. Convert Format and the richer conversions are unavailable. To add it later, run installPandoc.cmd in the FileDir folder as an administrator."
    return
  }
  $sVersion = runBounded $sPandoc @("--version") 30
  say "Pandoc: installed, $sVersion at $sPandoc"
}

function reportTool($sLabel, $sName, $sLater) {
  $sExe = findExe $sName
  if ($sExe -eq "") {
    say "$sLabel`: not installed. To add it later, $sLater."
    return
  }
  # Not every tool answers --version. ExifTool prints a manual page header to
  # that, which is where the reported version "NAME" came from: the first line
  # of its own documentation. It answers -ver with a bare number.
  $lVersionArgs = @("--version")
  if ($sName -eq "exiftool") { $lVersionArgs = @("-ver") }
  $sVersion = runBounded $sExe $lVersionArgs 20
  if ($sVersion -eq "") {
    say "$sLabel`: installed at $sExe"
    return
  }
  # ffmpeg says "ffmpeg version 8.1.2-full_build-www.gyan.dev Copyright (c)
  # 2000-2026 the FFmpeg developers" in one breath. The version is the useful
  # part; the copyright belongs in the log, not in a box a person is reading.
  $iCopyright = $sVersion.IndexOf("Copyright")
  if ($iCopyright -gt 0) { $sVersion = $sVersion.Substring(0, $iCopyright).Trim() }
  if ($sVersion.Length -gt 60) { $sVersion = $sVersion.Substring(0, 60).Trim() + "..." }
  say "$sLabel`: installed, $sVersion"
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

function reportChatModel($sList) {
  if ((findExe "ollama") -eq "") { return }
  if ($sList -eq "") {
    say "Chat model llama3.2: could not be checked just now. Press Alt+Shift+L in FileDir; it offers to fetch a model if none is there."
    return
  }
  if ($sList -match "llama3\.2") { say "Chat model llama3.2: installed. Alt+Shift+L can translate with it." }
  else { say "Chat model llama3.2: not downloaded. Run installOllama.cmd in the FileDir folder to fetch it." }
}

function reportTranslationModel($sList) {
  if ((findExe "ollama") -eq "") { return }
  if ($sList -match "qwen2\.5:7b") { say "Translation model qwen2.5:7b: installed. Alt+Shift+L will use it." }
  else { say "Translation model qwen2.5:7b: not installed. Alt+Shift+L uses llama3.2, quicker but less accurate." }
}

function main() {
  try { New-Item -ItemType Directory -Force -Path $sLogDir | Out-Null } catch { }
  try { if (Test-Path -LiteralPath $sSummaryFile) { Remove-Item -LiteralPath $sSummaryFile -Force } } catch { }

  say ("FileDir setup results  " + (Get-Date).ToString("yyyy-MM-dd HH:mm"))
  say ""

  # What the installer knew before the checkboxes ran, handed over in a file so
  # that ONE box tells the whole story instead of two telling halves.
  if (Test-Path -LiteralPath $sResultsFile) {
    # Read with the system's own encoding, which is what the installer wrote; a
    # wrong guess here would turn the lines into nonsense.
    # Trailing blank lines are dropped before the block is printed. The handed
    # over message ends with a line break of its own, and one blank line between
    # sections is a separator while three is a gap somebody has to arrow past.
    $lResults = @(Get-Content -LiteralPath $sResultsFile -Encoding Default -ErrorAction SilentlyContinue)
    while (($lResults.Count -gt 0) -and ($lResults[$lResults.Count - 1].Trim() -eq "")) {
      $lResults = $lResults[0..($lResults.Count - 2)]
    }
    foreach ($sLine in $lResults) { say $sLine }
    try { Remove-Item -LiteralPath $sResultsFile -Force } catch { }
  }

  # In the same order as the checkboxes, so the box reads as the page did.
  say ""
  say "Components"
  reportPandoc
  reportTool "ExifTool" "exiftool" "run installMediaTools.cmd in the FileDir folder"
  reportTool "ffmpeg" "ffmpeg" "run installMediaTools.cmd in the FileDir folder"
  reportTool "yt-dlp" "yt-dlp" "run installMediaTools.cmd in the FileDir folder"
  reportTool "Ollama" "ollama" "run installOllama.cmd in the FileDir folder"
  # One probe, read whole: the listing is short, and asking three times would
  # triple the wait on a cold service.
  $sList = ollamaModels
  reportChatModel $sList
  reportTranslationModel $sList

  say ""
  say "Saved as $sSummaryFile"
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
