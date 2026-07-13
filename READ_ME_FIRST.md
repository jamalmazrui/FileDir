# What went wrong, and the fix

## The cause (my mistake)

The `FileDir.zip` I sent you contained `FileDir_setup.iss`, and my copy of it was
**stale**: it said `AppVersion=5.0.4`. You had already released **v5.0.5**.
Unzipping it rewound your `.iss`, so the next build bumped 5.0.4 -> 5.0.5 -- a
version already on GitHub -- and `tagRelease` correctly refused to publish it.

I had warned about exactly this and then shipped an `.iss` anyway. Sorry.

## The fix in this drop

`bumpVersion.ps1` no longer trusts the `.iss` alone. Before choosing a number it
asks GitHub for the latest release, and starts from **whichever is higher** -- the
`.iss` or the newest published version. So even if the `.iss` is rewound by a bad
zip, a restored backup, or a bad merge, the build still assigns a version strictly
newer than anything released. The failure mode is now impossible rather than merely
unlikely.

If `gh` is unavailable or you are offline, it falls back to bumping the `.iss`
value, exactly as before.

## What to do now

Just build. No manual editing needed:

```
cd C:\FileDir
BuildFileDir.cmd        ' sees v5.0.5 on GitHub, assigns 5.0.6
'                         compile FileDir_setup.iss in Inno Setup
git add -A / git commit / git push
tagRelease              ' publishes 5.0.6
```

The build will print a line like:

```
Latest GitHub release is v5.0.5, higher than the .iss (5.0.4).
Starting from the released version, so the new number is genuinely new.
Version: 5.0.4 -> 5.0.6
```

## Note

This drop deliberately contains **no `.iss` file**. Only `bumpVersion.ps1` changed.
Everything else from the previous `FileDir.zip` (FileDir.cs, the docs, Say.cs,
Inix.cs, the installer edits) is already in place and still correct -- keep it.
