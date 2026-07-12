# postPage.ps1 (generic)
# Publishes the first *.md file in the script's folder as a public GitHub
# repository with GitHub Pages enabled and a tagged release.
#
# Generic behavior: drop this script (and postPage.cmd) into any project
# folder that contains a Pandoc-Markdown source file. The script:
#   - Picks the first *.md file alphabetically (README.md is skipped).
#   - Treats the folder name as the GitHub repository name.
#   - Reads "title", "subtitle", and "version" from the file's YAML
#     frontmatter to set the Jekyll site title, repo description, and
#     release tag. Falls back gracefully if any field is missing.
#
# Configuration is one line near the top: set $sGitHubUser to the GitHub
# account that owns the repos.
#
# Output is captured to postPage.log alongside the script, written via a
# single StreamWriter (no file-lock race) as UTF-8 without BOM.
#
# All staging files (README.md, _config.yml, .gitignore, pagesEnable.json)
# are also written as UTF-8 without BOM, since Windows PowerShell 5.1's
# Set-Content -Encoding UTF8 writes a BOM that breaks GitHub's JSON parser.
#
# Native command output (gh, git) is read as UTF-8 by setting the console
# output encoding at the top of the script; this keeps the gh check mark
# and other non-ASCII characters from being mojibake'd into the log.

[CmdletBinding()]
param()

# Force UTF-8 for native-command output, so the log captures gh's UTF-8
# bytes correctly (without this, Windows PowerShell 5.1 reads through the
# OEM code page and mangles characters like the gh status check mark).
$OutputEncoding = [System.Text.UTF8Encoding]::new()
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()

# ============================================================
# Configuration (the only line that may need editing per user)
# ============================================================
$sGitHubUser = "JamalMazrui"
$sBranchName = "main"
$sJekyllTheme = "jekyll-theme-cayman"

# ============================================================
# Project context derived from the script's own folder
# ============================================================
$sProjectDir = $PSScriptRoot
$sLogFile = Join-Path $sProjectDir "postPage.log"
$sRepoName = Split-Path -Leaf $sProjectDir

# Variables and shared state
$oLog = $null
$iResultCode = 0
$sStageDir = $null
$oUtf8NoBom = [System.Text.UTF8Encoding]::new($false)

try {
    $oLog = [System.IO.StreamWriter]::new($sLogFile, $false, $oUtf8NoBom)
    $oLog.AutoFlush = $true
} catch {
    Write-Host "FATAL: cannot open log file $sLogFile. $_"
    exit 1
}

function writeBoth {
    param([string]$sMessage)
    Write-Host $sMessage
    if ($null -ne $script:oLog) {
        $script:oLog.WriteLine($sMessage)
    }
}

function writeUtf8NoBom {
    param([string]$sPath, [string]$sContent)
    [System.IO.File]::WriteAllText($sPath, $sContent, $script:oUtf8NoBom)
}

function getYamlField {
    param([string]$sMarkdownPath, [string]$sFieldName)
    if (-not (Test-Path -LiteralPath $sMarkdownPath)) { return $null }
    $sContent = [System.IO.File]::ReadAllText($sMarkdownPath)
    # Strip a leading UTF-8 BOM (U+FEFF) if present, so the \A anchor in
    # the next regex still matches against "---" at the very start.
    if ($sContent.Length -gt 0 -and $sContent[0] -eq [char]0xFEFF) {
        $sContent = $sContent.Substring(1)
    }
    if ($sContent -notmatch '(?s)\A---\s*\r?\n(.*?)\r?\n---\s*\r?\n') { return $null }
    $sYaml = $Matches[1]
    $sPattern = '(?m)^' + [regex]::Escape($sFieldName) + ':\s*"?([^"\r\n]+?)"?\s*$'
    if ($sYaml -match $sPattern) { return $Matches[1].Trim() }
    return $null
}

function runAndLog {
    param([string]$sDescription, [scriptblock]$oScriptBlock)
    writeBoth ""
    writeBoth "=== $sDescription ==="
    $iLocalCode = 0
    try {
        & $oScriptBlock 2>&1 | ForEach-Object {
            $sLine = "$_"
            Write-Host $sLine
            $script:oLog.WriteLine($sLine)
        }
        $iLocalCode = $LASTEXITCODE
    } catch {
        writeBoth "ERROR running '$sDescription': $_"
        $iLocalCode = 1
    }
    return $iLocalCode
}

try {
    writeBoth "postPage.ps1 (generic) started $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    writeBoth "Project folder: $sProjectDir"
    writeBoth "PowerShell version: $($PSVersionTable.PSVersion)"

    # ============================================================
    # Step 1: Discover the source Markdown file
    # ============================================================
    writeBoth ""
    writeBoth "=== Step 1: Discover source file ==="
    $aMdFiles = Get-ChildItem -Path $sProjectDir -Filter "*.md" -File | Where-Object {
        $_.Name -ne "README.md" -and $_.Name -ne "index.md"
    } | Sort-Object Name

    if ($aMdFiles.Count -eq 0) {
        writeBoth "ERROR: No *.md source file found in $sProjectDir."
        writeBoth "Place a Markdown file (anything except README.md or index.md) in this folder and retry."
        $iResultCode = 5
        return
    }
    if ($aMdFiles.Count -gt 1) {
        writeBoth "Multiple *.md files found. Using the first alphabetically:"
        foreach ($oFile in $aMdFiles) { writeBoth "  - $($oFile.Name)" }
    }
    $sSourceFile = $aMdFiles[0].Name
    $sSourcePath = $aMdFiles[0].FullName
    writeBoth "Source file: $sSourceFile"
    writeBoth "Source path: $sSourcePath"

    # ============================================================
    # Step 2: Read YAML metadata
    # ============================================================
    writeBoth ""
    writeBoth "=== Step 2: Read YAML metadata ==="
    $sTitle = getYamlField -sMarkdownPath $sSourcePath -sFieldName "title"
    $sSubtitle = getYamlField -sMarkdownPath $sSourcePath -sFieldName "subtitle"
    $sVersion = getYamlField -sMarkdownPath $sSourcePath -sFieldName "version"
    $sAuthor = getYamlField -sMarkdownPath $sSourcePath -sFieldName "author"

    if (-not $sTitle) { $sTitle = $sRepoName }
    if ($sSubtitle) {
        $sRepoDescription = "${sTitle}: ${sSubtitle}"
    } else {
        $sRepoDescription = $sTitle
    }
    if ($sVersion) {
        $sReleaseTag = $sVersion
    } else {
        $sReleaseTag = "v1.0.0"
        writeBoth "No version field in YAML. Defaulting to $sReleaseTag."
        writeBoth "Tip: add   version: ""v1.0.0""   to YAML to control the release tag."
    }
    $sReleaseTitle = "${sTitle} ${sReleaseTag}"

    writeBoth "Target repo:    $sGitHubUser/$sRepoName  (derived from folder name)"
    writeBoth "Page title:     $sTitle  (from YAML title)"
    if ($sSubtitle) {
        writeBoth "Page subtitle:  $sSubtitle  (from YAML subtitle)"
    }
    if ($sAuthor) {
        writeBoth "Page author:    $sAuthor  (from YAML author)"
    }
    writeBoth "Release tag:    $sReleaseTag  (from YAML version)"
    writeBoth "Branch:         $sBranchName"

    # ============================================================
    # Step 3: Verify prerequisites
    # ============================================================
    writeBoth ""
    writeBoth "=== Step 3: Verify prerequisites ==="
    $oGh = Get-Command gh -ErrorAction SilentlyContinue
    if ($null -eq $oGh) {
        writeBoth "ERROR: gh CLI is not on PATH. Install from https://cli.github.com/"
        $iResultCode = 2
        return
    }
    writeBoth "gh:  $($oGh.Source)"
    $oGit = Get-Command git -ErrorAction SilentlyContinue
    if ($null -eq $oGit) {
        writeBoth "ERROR: git is not on PATH."
        $iResultCode = 3
        return
    }
    writeBoth "git: $($oGit.Source)"

    $iRC = runAndLog "gh auth status" { gh auth status }
    if ($iRC -ne 0) {
        writeBoth "ERROR: gh is not authenticated. Run 'gh auth login' first."
        $iResultCode = 4
        return
    }

    # ============================================================
    # Step 4: Build staging directory (UTF-8 no BOM throughout)
    # ============================================================
    writeBoth ""
    writeBoth "=== Step 4: Build staging directory ==="
    $sStageDir = Join-Path $env:TEMP "${sRepoName}_stage"
    if (Test-Path -LiteralPath $sStageDir) {
        Remove-Item -LiteralPath $sStageDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $sStageDir | Out-Null
    writeBoth "Staging dir: $sStageDir"

    Copy-Item -LiteralPath $sSourcePath -Destination (Join-Path $sStageDir "index.md") -Force
    Copy-Item -LiteralPath $sSourcePath -Destination (Join-Path $sStageDir $sSourceFile) -Force

    $sConfigDescription = if ($sSubtitle) { $sSubtitle } else { $sTitle }
    $sConfigAuthor = if ($sAuthor) { $sAuthor } else { $sGitHubUser }
    $sJekyllConfig = @"
title: $sTitle
description: $sConfigDescription
author: $sConfigAuthor
theme: $sJekyllTheme
markdown: kramdown
kramdown:
  input: GFM
  toc_levels: 2..3
"@
    writeUtf8NoBom -sPath (Join-Path $sStageDir "_config.yml") -sContent $sJekyllConfig

    $sReadme = @"
# $sTitle

$sConfigDescription

Read the full guide as a web page at https://$sGitHubUser.github.io/$sRepoName/

See $sSourceFile in this repository for the Markdown source.
"@
    writeUtf8NoBom -sPath (Join-Path $sStageDir "README.md") -sContent $sReadme

    writeUtf8NoBom -sPath (Join-Path $sStageDir ".gitignore") -sContent "_site/`n.jekyll-cache/`n"

    # Declare line-ending handling explicitly so git stops warning
    # "LF will be replaced by CRLF the next time Git touches it" on every
    # add. With * text=auto, git knows files are text and how to handle
    # line endings on its own.
    writeUtf8NoBom -sPath (Join-Path $sStageDir ".gitattributes") -sContent "* text=auto`n"

    writeBoth "Staging files written (UTF-8, no BOM)."

    # ============================================================
    # Step 5: git init and commit
    # ============================================================
    Set-Location -LiteralPath $sStageDir
    $iRC = runAndLog "git init and commit" {
        git init -b $sBranchName
        git add .
        git commit -m "Publish $sTitle $sReleaseTag"
    }

    # ============================================================
    # Step 6: Create the repo (if new) or push updates (if existing)
    # ============================================================
    writeBoth ""
    writeBoth "=== Step 6: Check if remote repo exists ==="
    $iRepoView = runAndLog "gh repo view (existence check)" {
        gh repo view "$sGitHubUser/$sRepoName"
    }

    if ($iRepoView -ne 0) {
        writeBoth "Repo does not exist on GitHub. Creating new repo."
        $iRC = runAndLog "gh repo create" {
            gh repo create "$sGitHubUser/$sRepoName" --public --description "$sRepoDescription" --source . --remote origin --push
        }
        if ($iRC -ne 0) {
            writeBoth "ERROR: gh repo create failed."
            $iResultCode = 6
            return
        }
        $iVerify = runAndLog "gh repo view (post-create verify)" {
            gh repo view "$sGitHubUser/$sRepoName"
        }
        if ($iVerify -ne 0) {
            writeBoth "ERROR: repo create returned success but verify failed."
            $iResultCode = 6
            return
        }
        writeBoth "Repo created and verified."
    } else {
        writeBoth "Repo already exists. Pushing updates."
        $iRC = runAndLog "git remote and push" {
            git remote add origin "https://github.com/$sGitHubUser/$sRepoName.git" 2>$null
            git push -u origin $sBranchName --force
        }
        if ($iRC -ne 0) {
            writeBoth "ERROR: git push failed."
            $iResultCode = 7
            return
        }
        writeBoth "Push complete."
    }

    # ============================================================
    # Step 7: Enable GitHub Pages (idempotent)
    # ============================================================
    writeBoth ""
    writeBoth "=== Step 7: Enable GitHub Pages ==="
    $oPagesBody = @{ source = @{ branch = $sBranchName; path = "/" } }
    $sPagesJson = $oPagesBody | ConvertTo-Json -Compress -Depth 5
    $sPagesJsonFile = Join-Path $sStageDir "pagesEnable.json"
    writeUtf8NoBom -sPath $sPagesJsonFile -sContent $sPagesJson
    writeBoth "Pages body: $sPagesJson"

    $aFirstBytes = [System.IO.File]::ReadAllBytes($sPagesJsonFile) | Select-Object -First 3
    $sFirstBytesHex = ($aFirstBytes | ForEach-Object { "{0:X2}" -f $_ }) -join " "
    writeBoth "First 3 bytes of JSON file: $sFirstBytesHex  (must NOT be EF BB BF)"
    if ($sFirstBytesHex -eq "EF BB BF") {
        writeBoth "ERROR: BOM detected in JSON file. Aborting."
        $iResultCode = 8
        return
    }

    $iPagesGet = runAndLog "Check current Pages status" {
        gh api "repos/$sGitHubUser/$sRepoName/pages"
    }
    if ($iPagesGet -ne 0) {
        writeBoth "Pages not yet enabled. Enabling via POST."
        $iRC = runAndLog "gh api POST pages" {
            gh api -X POST "repos/$sGitHubUser/$sRepoName/pages" --input $sPagesJsonFile
        }
        if ($iRC -ne 0) {
            writeBoth "ERROR: Pages enable POST failed."
            writeBoth "Manual fallback: in github.com Settings > Pages, set Source = Deploy from branch, branch = $sBranchName, folder = /, Save."
            $iResultCode = 8
            return
        }
        writeBoth "Pages enabled."
    } else {
        writeBoth "Pages already enabled. Updating source via PUT."
        $iRC = runAndLog "gh api PUT pages" {
            gh api -X PUT "repos/$sGitHubUser/$sRepoName/pages" --input $sPagesJsonFile
        }
    }
    $null = runAndLog "Final Pages status" {
        gh api "repos/$sGitHubUser/$sRepoName/pages"
    }

    # ============================================================
    # Step 8: Create the tagged release if it doesn't exist yet
    # ============================================================
    writeBoth ""
    writeBoth "=== Step 8: Create tagged release ==="
    $iRelView = runAndLog "Check release existence" {
        gh release view $sReleaseTag --repo "$sGitHubUser/$sRepoName"
    }
    if ($iRelView -ne 0) {
        writeBoth "Release $sReleaseTag does not exist. Creating now."
        $sReleaseNotes = "Release $sReleaseTag of $sTitle. See $sSourceFile attached to this release for the Markdown source. Read the rendered guide at https://$sGitHubUser.github.io/$sRepoName/"
        $iRC = runAndLog "gh release create" {
            gh release create $sReleaseTag --repo "$sGitHubUser/$sRepoName" --title "$sReleaseTitle" --notes "$sReleaseNotes" $sSourceFile
        }
        if ($iRC -ne 0) {
            writeBoth "ERROR: gh release create failed."
            $iResultCode = 9
            return
        }
        writeBoth "Release created."
    } else {
        writeBoth "Release $sReleaseTag already exists. Skipping creation."
        writeBoth "Tip: bump the version field in the YAML frontmatter of $sSourceFile before re-running to publish a new release."
    }

    # ============================================================
    # Success
    # ============================================================
    writeBoth ""
    writeBoth "============================================================"
    writeBoth "SUCCESS"
    writeBoth "Repo URL (github.com source):    https://github.com/$sGitHubUser/$sRepoName"
    writeBoth "Pages URL (github.io rendered):  https://$sGitHubUser.github.io/$sRepoName/"
    writeBoth "Release URL:                     https://github.com/$sGitHubUser/$sRepoName/releases/tag/$sReleaseTag"
    writeBoth "Allow 1-2 minutes for Pages to rebuild on update."
    writeBoth "============================================================"

} catch {
    writeBoth "FATAL ERROR: $_"
    writeBoth ($_.ScriptStackTrace | Out-String)
    if ($iResultCode -eq 0) { $iResultCode = 99 }
} finally {
    Set-Location -LiteralPath $sProjectDir
    if ($null -ne $oLog) {
        $oLog.WriteLine("")
        $oLog.WriteLine("postPage.ps1 finished $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') with exit code $iResultCode")
        $oLog.Flush()
        $oLog.Close()
        $oLog.Dispose()
    }
    Write-Host ""
    Write-Host "Log saved to: $sLogFile"
    Write-Host "Exit code: $iResultCode"
    exit $iResultCode
}
