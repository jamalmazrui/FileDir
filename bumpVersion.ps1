# bumpVersion.ps1
#
# Assign the next version number to this project, at BUILD time.
#
# Why this exists
# ---------------
# The version has to be settled BEFORE the program and the installer are compiled,
# because both of them bake it in: Build<App>.cmd generates Version.cs from it, and
# Inno Setup stamps it into the installer's version resource. If the number were
# instead chosen later (at release time), every build of an already-released version
# would have to be bumped and then rebuilt -- an endless loop.
#
# So: the build assigns the version, and tagRelease only publishes what the build
# produced. tagRelease never changes a version number.
#
# What it does
# ------------
#   * Finds <App>_setup.iss, where <App> is the name of the current directory.
#   * Reads AppVersion (both Inno styles are supported):
#         #define AppVersion "1.0.126"      (DbDo)
#         AppVersion=5.0.2                  (EdSharp, FileDir)
#   * Increments the last dotted-numeric part: 5.0.2 -> 5.0.3, 1.0.126 -> 1.0.127.
#     A two-part version gains a third part (5.0 -> 5.0.1), because 5.0 and 5.0.0
#     compare EQUAL and an existing install would not see it as newer.
#   * Writes the new number back into every version-bearing line of the .iss that
#     holds a literal (AppVersion, and AppVerName / VersionInfoVersion when they do
#     not use the {#AppVersion} token).
#   * Prints the new version.
#
# Build<App>.cmd calls this, then reads AppVersion back from the .iss and generates
# Version.cs from it -- so the .iss remains the single source of truth.
#
# Usage (normally only from the build script):
#     powershell -NoProfile -ExecutionPolicy Bypass -File bumpVersion.ps1
#     powershell -NoProfile -ExecutionPolicy Bypass -File bumpVersion.ps1 -Version 6.0
#
# Exit code 0 on success, 1 on failure (the build script aborts on failure).

[CmdletBinding()]
param(
    [string] $Version
)

$ErrorActionPreference = 'Stop'

function compareVersions {
    # $true if $sA is strictly greater than $sB (dotted-numeric, zero-padded, so
    # 5.0 and 5.0.0 compare equal).
    param([string] $sA, [string] $sB)
    if (-not $sA) { return $false }
    if (-not $sB) { return $true }
    $aA = @($sA.Split(".")); $aB = @($sB.Split("."))
    for ($i = 0; $i -lt 4; $i++) {
        $iA = 0; $iB = 0
        if ($i -lt $aA.Count) { [void][int]::TryParse($aA[$i], [ref] $iA) }
        if ($i -lt $aB.Count) { [void][int]::TryParse($aB[$i], [ref] $iB) }
        if ($iA -gt $iB) { return $true }
        if ($iA -lt $iB) { return $false }
    }
    return $false
}

try {
    $sRepoPath = $PWD.Path
    $sApp = Split-Path -Leaf $sRepoPath

    # ---- Find <App>_setup.iss ----
    $aFound = @(Get-ChildItem -LiteralPath $sRepoPath -Filter "$($sApp)_setup.iss" -File -ErrorAction SilentlyContinue)
    if ($aFound.Count -ne 1) {
        throw "Expected exactly one $($sApp)_setup.iss in $sRepoPath (found $($aFound.Count))."
    }
    $sIssPath = $aFound[0].FullName
    $aLines = [System.IO.File]::ReadAllText($sIssPath) -split "`r?`n"

    # ---- Read the current AppVersion ----
    $sOld = ''
    foreach ($sLine in $aLines) {
        if ($sLine -match '^\s*#define\s+AppVersion\s+"([^"]+)"') { $sOld = $Matches[1]; break }
    }
    if (-not $sOld) {
        foreach ($sLine in $aLines) {
            if ($sLine -match '^\s*AppVersion\s*=\s*([^\s{]+)\s*$') { $sOld = $Matches[1]; break }
        }
    }
    if (-not $sOld) { throw "Could not find AppVersion in $(Split-Path -Leaf $sIssPath)." }

    # ---- Floor: never assign a version that is already released ----
    # The .iss is just a file on disk, so it can be WRONG: an old copy restored from
    # a backup, or unzipped over the project, rewinds the number -- and the next
    # build would then re-assign a version that has already been published.  GitHub
    # is the authority on what actually shipped, so ask it, and start from whichever
    # is higher: the .iss or the latest release.  If gh is missing or offline, fall
    # back to the .iss alone.
    $sFloor = $sOld
    if (Get-Command gh -ErrorAction SilentlyContinue) {
        $ErrorActionPreference = "Continue"
        $sLatest = (& gh release view --json tagName --jq .tagName 2>$null | Out-String).Trim()
        $ErrorActionPreference = "Stop"
        if ($sLatest) {
            $sLatest = $sLatest -replace "^[vV]", ""
            if (compareVersions $sLatest $sFloor) {
                Write-Host "Latest GitHub release is v$sLatest, higher than the .iss ($sOld)."
                Write-Host "Starting from the released version, so the new number is genuinely new."
                $sFloor = $sLatest
            }
        }
    }

    # ---- Decide the new version ----
    if ($Version) {
        $sNew = $Version.Trim()
    }
    else {
        $aParts = $sFloor.Trim().Split('.')
        foreach ($sPart in $aParts) {
            if ($sPart -notmatch '^\d+$') {
                throw "Version '$sFloor' is not dotted-numeric, so it cannot be bumped automatically. Pass -Version X.Y.Z."
            }
        }
        if ($aParts.Count -lt 3) { $aParts = @($aParts) + '1' }
        else { $aParts[$aParts.Count - 1] = [string]([int]$aParts[$aParts.Count - 1] + 1) }
        $sNew = ($aParts -join '.')
    }

    # ---- Write it back into every literal version line ----
    $bChanged = $false
    $aOut = @()
    foreach ($sLine in $aLines) {
        $sEdit = $sLine
        if ($sLine -match '^\s*#define\s+AppVersion\s+"[^"]+"') {
            $sEdit = $sLine -replace '"(?:[^"]+)"', "`"$sNew`""
        }
        elseif ($sLine -match '^\s*AppVersion\s*=\s*[^\s{]+\s*$') {
            $sEdit = "AppVersion=$sNew"
        }
        elseif ($sLine -match '^\s*VersionInfoVersion\s*=\s*[\d\.]+\s*$') {
            $sEdit = "VersionInfoVersion=$sNew"
        }
        elseif ($sLine -match '^\s*AppVerName\s*=\s*(.+)$') {
            # Replace only a literal dotted-numeric version, keeping any words around it
            # (e.g. "EdSharp 5.0.2 beta" -> "EdSharp 5.0.3 beta").
            $sRest = $Matches[1]
            if ($sRest -match '\d+(\.\d+)+') {
                $sEdit = "AppVerName=" + ($sRest -replace '\d+(\.\d+)+', $sNew)
            }
        }
        if ($sEdit -ne $sLine) { $bChanged = $true }
        $aOut += $sEdit
    }

    if ($bChanged) {
        $sText = ($aOut -join "`r`n")
        [System.IO.File]::WriteAllText($sIssPath, $sText, (New-Object System.Text.UTF8Encoding($false)))
    }

    Write-Host "Version: $sOld -> $sNew"
    exit 0
}
catch {
    Write-Host "bumpVersion failed: $($_.Exception.Message)"
    exit 1
}
