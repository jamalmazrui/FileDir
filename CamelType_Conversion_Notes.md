# FileDir — Camel Type Conversion Notes (C# method pass)

Date: June 17, 2026
Applies to: FileDir.cs (5.0 beta)

## What changed

All **240** FileDir-defined methods that were PascalCase are now lowerCamel.
Each rename moved together as a unit: the definition, every call site (bare
`name(` and qualified `obj.name(`), every delegate/method-group reference (e.g.
`menu_Helper("Elevate Version", "F11", menuHelpElevateVersion_Click)`), and the
`} // name method` end-of-method comments.

Examples: `MenuHelpElevateVersion_Click` to `menuHelpElevateVersion_Click`,
`FetchLatestReleaseTag` to `fetchLatestReleaseTag`, `GetActiveChild` to
`getActiveChild`, `ClickOrDescribe` to `clickOrDescribe`, `Convert2Text` to
`convert2Text`, `GetPortableExecutableKind` to `getPortableExecutableKind`.

## What deliberately stayed PascalCase

- **Class names** — `App`, `Frame`, `MdiChild`, `SingleInstanceApplication`.
  Camel Type keeps classes (and namespaces) PascalCase.
- **BCL / framework method calls** — `.Contains(`, `.Sort(`, `.IndexOf(`,
  `.AddRange(`, `.ToLower(`, `.TrimEnd(`, etc. These are not FileDir's methods;
  Camel Type uses third-party/API names verbatim. (FileDir defines no methods of
  its own with these names — confirmed there are no such definitions.)

## How it was done safely (no compiler available here)

The rename was scripted and gated, then statically verified:

- Only names with a `} // name method` definition marker were considered.
- Excluded any name matching a common BCL method, any name whose lowercase form
  already existed (avoids collisions — this is what protected the `Frame` class),
  and any name appearing inside a string literal.
- Verified after applying: brace open/close delta unchanged (15); zero PascalCase
  residue for renamed names; renamed lowercase counts absorbed the originals; BCL
  `.Name(` counts byte-for-byte unchanged; no renamed target is a C# keyword; no
  reflection-by-name (`GetMethod`/`Invoke` with a string) is used.

## Still open for "complete" Camel Type (recommended next passes)

- **Fields** not yet Hungarian: e.g. `hashDirectory` to `dDirectory`,
  `listRecentDirs` to `lsRecentDirs`, `TempFileList` to `lsTempFile`,
  `KeyDescriber` to `bKeyDescriber`. Same global-rename discipline applies.
- **Declaration grouping/alphabetization** within scopes (cosmetic, low risk).
- **Other languages**: the JScript .NET helper (`LbcJS.js`) and the JAWS script
  family (`.jss`/`.jkm`) per `CamelType_JAWSScript.md`, when those are tackled.

## Minor cleanup noted (not done)

Some blocks carry stray legacy `} // <BclName> method` comments (e.g.
`} // AddRange method`) that do not actually close a method. They are harmless
comments; left as-is to avoid unnecessary churn.

## Fields pass (App static fields)

Renamed (safe — accessed via `App.`, not in any string literal, no collision):

- `KeyDescriber` to `bKeyDescriber`
- `hashDirectory` to `dDirectory`
- `TempFileList` to `lsTempFile`
- `listRecentDirs` to `lsRecentDirs`

Config-backed bools (names double as persisted `.ini` keys, so the string key is
always preserved):

- `ExtraSpeech` to `bExtraSpeech` and `ZipOpener` to `bZipOpener` — renamed (field
  identifier only; the `"ExtraSpeech"`/`"ZipOpener"` config keys are unchanged).
- `Recycle` and `DirsBeforeFiles` — kept as a **principled exception**. `bRecycle`
  is already a parameter throughout the delete/copy/move methods meaning "recycle
  *this* operation", and `bDirsBeforeFiles` is already a local snapshot beside
  `App.DirsBeforeFiles`. Renaming these fields to those same names would turn a
  global setting and a per-operation flag/local into homonyms — which works
  against the screen-reader readability Camel Type exists to serve. Left as-is by
  design (the `b`-less field name reads as the setting; the `b`-prefixed local
  reads as the flag).

Instance fields in `Frame`, `MdiChild`, and the custom list classes are a later
sweep; most are already Hungarian-prefixed.
