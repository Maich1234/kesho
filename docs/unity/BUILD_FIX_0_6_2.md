# Build #3 — persistent audio listener, 0.6.2

The full supplied log now identifies the failure. It checked out `1c974a99f1a589f25b5f689771a8d94ef3fcab5d`. On Unity 6000.6.6f1, **18/18 EditMode cases passed and 16/17 PlayMode cases passed**. This confirms that the preceding icon/portrait/audio import repair and the background-save JSON tests passed in Unity on this run.

The sole failed case is `CollegeSceneTests.CampusMeshesAndAllHotspotsSurviveSceneLoad`. Its message is:

> Unhandled log message: '[Log] There are no audio listeners in the scene. Please ensure there is always one audio listener in the scene'.

The log shows repeated missing-listener messages in the runner's initial `Temp/__Backupscenes/0.backup`, before Campus is loaded. The previous implementation attached its only listener to the camera created by `Stage.Awake`; session music could start earlier, and the listener would also disappear whenever its scene camera was destroyed. The test's explicit `LogAssert.NoUnexpectedReceived()` correctly reported that gap. This is an audio lifecycle defect, not an assertion that the campus geometry or paths were broken. The newer editor version was not established as its cause.

## Repair

- `SessionAudioListener` initializes before scene loading, persists independently of scenes and follows the current main camera in LateUpdate after camera easing. It retains its last pose during loading gaps.
- `GameSession` ensures the listener exists before starting music; `Stage` ensures it exists when a scene is opened independently. The creation method is idempotent.
- Stage cameras no longer create their own listeners, avoiding duplicates after a new camera appears.
- The PlayMode runner's RunStarted callback also ensures the same production listener exists in its empty initial scene. The EditMode run is left untouched. No missing-listener logs are ignored and the strict log assertion remains.
- A new PlayMode test checks exactly one startup listener, unloads the gameplay scene while an empty scene is active, loads Room and checks that the same listener survives and matches the replacement camera's position/rotation.
- Runtime 0.6.2 / Android code 8; no save migration.

Local verification: nine Python tests, existing engine-independent story/director/presentation/save/concurrency checks, presentation source probe and parsing all 26 Unity C# sources. The new Unity lifecycle test and an APK build still require the cloud editor. No fresh successful Unity run is claimed.

Build current `main` with the existing pre-export `Kesho.Editor.ProjectSetup.Configure`, project subfolder `unity`, and both test suites enabled. The repository still pins Unity 6000.3.26f1; build #3's 6000.6.6f1 drift is recorded separately from the identified failure. A clean cache was relevant to the prior importer repair; this listener fix requires the new source commit, not repeated cache clearing as a substitute.
