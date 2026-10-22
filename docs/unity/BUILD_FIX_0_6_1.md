# Android build #2 — asset-import repair 0.6.1

## Evidence from the supplied log

The cloud builder checked out `473d6bfc6141ebb2d17b4d47ca0c24bc250e6fec`. It ran **Unity 6000.6.6f1**; `unity/ProjectSettings/ProjectVersion.txt` pins **6000.3.26f1**. This difference is observed configuration drift, not proof that the newer editor caused the failure.

The log explicitly reports that icon and portrait PNG metadata contain a TextureImporter at version 1, below the supported minimum 10. It reports the same issue for the three newly baked sounds: AudioImporter version 1, minimum 6. The branding SVG also has invalid inferred importer metadata. The source assets are in the checkout; this is not a missing-file or Git LFS-download diagnosis.

At initial configuration, `AssetDatabase.LoadAssetAtPath<Texture2D>` returned no icon and `ProjectSetup.FirstImport` threw `BuildFailedException: Kesho app icon is missing`. The old method attempted to load/check the icon before repairing its importer and set the render pipeline before that failure. A cached pipeline could subsequently skip incomplete initial configuration.

The fatal stop is **“one or more unit tests failed”** after a PlayMode run. The supplied log saves `TestResults.xml` in the cloud process but does not print failed test names/assertions. The invalid portrait import could affect the existing portrait-presence test, but that is an inference; without the test XML we cannot identify every failed test or claim all were caused by asset import.

The D3D11 dedicated video decoder warning explicitly falls back to software video decoding. It is not the reported fatal build reason.

## Changes

- Explicit supported TextureImporter metadata (version 13, Default type / 2D shape, no mipmaps, uncompressed import) for icon and portrait. Preserve their GUIDs.
- Explicit AudioImporter metadata (version 7) for all seven OGG files, preserving existing channels and GUIDs. Baked clips and original media are unchanged.
- Use the existing root `icon.svg` as the vector master; remove its unnecessary duplicate from Unity Branding so no SVG importer plugin is required. Browser template SVG remains unchanged and is copied by the WebGL template system.
- Synchronously import and normalize required images **before** loading them. Distinguish missing checkout files, missing importers and failed Texture2D imports. Validate each required audio clip too.
- Configure once per editor session even with a cached pipeline; only mark session setup complete after it succeeds. Pre-export Configure remains available and explicit.
- Print test status and failed names/messages/stacks through Unity's supported `TestRunCallback` without logging artificial errors, ignoring exceptions, weakening assertions or disabling test gates.
- Add two texture and seven audio EditMode import cases. Add Python metadata-contract regressions that reject the old incomplete importer data.
- Version 0.6.1, Android code 7. No save-format change.

## Verification and retry

Local Python tests, engine-independent story/save tests and C# parsing pass. The new Unity import tests/callback/editor code still require an activated Unity run. No repaired APK or successful cloud rebuild is claimed.

In Unity Build Automation, use current `main`, project subfolder `unity`, editor **6000.3.26f1**, pre-export method **`Kesho.Editor.ProjectSetup.Configure`**. Leave pre-build/post-build shell script fields empty and keep unit-test failure blocking enabled. Run EditMode and PlayMode suites.

For this retry, use a clean build/cache reset once to remove the prior invalid imported artifacts. The source fix also forces required texture/audio reimports; cache clearing alone would not fix the old metadata. After import succeeds, subsequent builds can reuse caches.

If tests still fail, provide the build's **`TestResults.xml`** (or the next full log, which now includes `[Kesho Tests]` failure details). Diagnose the named assertion before changing unrelated gameplay or disabling the gate.
