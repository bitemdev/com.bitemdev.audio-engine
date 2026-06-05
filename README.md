# BitemDev Audio Engine

Centralized Unity audio middleware for designer-authored sound events.

This package is meant to sit between the sound designer and the programmer:

- The sound designer creates and tunes `AudioEventDefinition` assets.
- The programmer calls the `AudioManager` with an event id.
- Gameplay objects notify the audio system, but they do not own `AudioSource` playback logic.

The package supports both compact and production-scale projects:

- Compact projects: the camera can be both the audio listener and the gameplay audio reference.
- Production projects: the camera can be the listener while a dedicated gameplay transform drives audio logic such as proximity, ambience, combat intensity, or music state.

## Architecture Goals

The package is built around these rules:

| Goal | Package Support |
| --- | --- |
| Centralize playback | `AudioManager` owns pooled `AudioSource` voices, event lookup, playback, mixer parameters, snapshots, and stopping. |
| Keep gameplay code clean | Gameplay code calls the manager with event ids instead of managing `AudioSource` components directly. |
| Give designers control | `AudioEventDefinition` assets hold clips, routing, randomization, spatial settings, cooldowns, voice limits, and fades. |
| Support camera-based listening | `AudioReferenceRig` assigns/registers the listener camera and can ensure it has an `AudioListener`. |
| Support separate gameplay audio logic | `AudioReferenceRig` has a separate `Logic Reference` field used by systems such as `AudioDistanceParameterDriver`. |
| Support compact setups | Set `Reference Mode` to `CameraOnly` when the camera should drive both listening and audio logic. |
| Support larger setups | Set `Reference Mode` to `SeparateListenerAndLogicReference` when listening and gameplay audio logic should use different transforms. |

## Install From Git URL

1. Open Unity.
2. Open `Window > Package Manager`.
3. Press `+`.
4. Choose `Add package from git URL...`.
5. Paste: https://github.com/bitemdev/com.bitemdev.audio-engine.git
6. Wait for Unity to import and compile.

The package appears under `Packages/BitemDev Audio Engine`.

## Fastest No-Code Test

Use this path when someone wants to install the package in an empty project and verify the whole tool without writing code.

1. Install the package from the Git URL.
2. Open any scene, including a brand-new empty scene.
3. Run `Tools > BitemDev > Audio Engine > Create No-Code Demo In Current Scene`.
4. Press Play.
5. Use the on-screen `BitemDev Audio Engine Demo` panel.

The demo creates:

- `AudioEngineConfig.asset`
- `AudioEventLibrary.asset`
- Five demo WAV clips generated inside `Assets/AudioEngine/Demo/Clips`
- Five demo audio events inside `Assets/AudioEngine/Demo/Events`
- One `AudioManager`
- One `AudioReferenceRig`
- One camera listener setup if the scene did not already have one
- One logic reference object
- One green cube for 3D one-shot testing
- One moving cyan sphere for followed loop testing
- One on-screen no-code test panel

The on-screen buttons test:

| Button | What It Tests |
| --- | --- |
| `Play 2D UI Event` | Direct event-asset playback, 2D playback, pooling. |
| `Play Event By Id` | Event library lookup using a string id. |
| `Play 3D Event At Cube` | Positional playback, camera listener, 3D attenuation. |
| `Start Follow Loop` / `Stop Follow Loop` | Looping playback, follow target behavior, handle-based stopping. |
| `Music A` / `Music B` | Music playback mode and fade-out replacement. |
| `Stop All` | Global stop and fade behavior. |
| `Move follow emitter` | Whether the followed sound updates as its transform moves. |

After using the demo, inspect the generated event assets. They are normal `AudioEventDefinition` assets and can be edited, duplicated, renamed, or deleted.

## Empty Project Test

Use these steps when testing the tool manually in a fresh Unity project.

### 1. Create The Default Assets

Run:

`Tools > BitemDev > Audio Engine > Create Default Assets`

Unity creates:

- `Assets/AudioEngine/AudioEngineConfig.asset`
- `Assets/AudioEngine/AudioEventLibrary.asset`

Purpose:

- `AudioEngineConfig` stores global audio settings.
- `AudioEventLibrary` is the list of events the programmer can trigger by id.

### 2. Add The Audio Manager

Run:

`GameObject > BitemDev > Audio Engine > Audio Manager`

This creates a scene object named `Audio Manager`.

Purpose:

- Owns pooled `AudioSource` objects.
- Plays every event.
- Routes events to mixer groups.
- Handles music replacement and fades.
- Handles cooldowns and max voice limits.
- Applies mixer parameters and snapshots.

There should normally be one `AudioManager` in the first scene or bootstrap scene.

### 3. Add The Audio Reference Rig

Run:

`GameObject > BitemDev > Audio Engine > Audio Reference Rig`

Select the new object and assign:

- `Listener Camera`: your Main Camera.
- `Logic Reference`: usually the Player object. In an empty test project, create an empty GameObject named `Player Reference` and assign it.
- `Reference Mode`: choose the setup you want.

Reference modes:

| Mode | Use When | What Happens |
| --- | --- | --- |
| `CameraOnly` | Compact projects, prototypes, simple scenes | Camera position is used for listening and gameplay audio logic. |
| `SeparateListenerAndLogicReference` | Projects where camera movement should not drive gameplay audio logic | Camera hears the sound, logic reference drives proximity and parameters. |
| `Explicit` | Advanced/manual setups | Uses exactly the transforms assigned by code or rig. |

Use `SeparateListenerAndLogicReference` when camera framing, zoom, or cinematic movement should not affect gameplay audio behavior.

Use `CameraOnly` when the camera is the only meaningful listener/reference point in the scene.

### 4. Create A Test Audio Event

Open:

`Tools > BitemDev > Audio Engine > Open Audio Engine Window`

In the window:

1. Assign `AudioEventLibrary.asset` if it is not already assigned.
2. Set `Events Folder` to `Assets/AudioEngine/Events`.
3. Set `Event Id` to `ui.test-click`.
4. Press `Create Event Asset`.
5. Select the created event asset.
6. In `Clips`, set size to `1`.
7. Assign any short `AudioClip`.

Press Play in Unity. Select the event asset. Press:

`Play Through Audio Manager`

If the event has a clip and an `AudioManager` exists in the scene, you should hear it.

### 5. Generate Programmer Constants

Open:

`Tools > BitemDev > Audio Engine > Open Audio Engine Window`

Press:

`Generate AudioEventIds.cs`

This creates:

`Assets/AudioEngine/Generated/AudioEventIds.cs`

Example generated constant:

```csharp
public const string UI_TEST_CLICK = "ui.test-click";
```

The programmer can now use the constant instead of typing strings.

## Main Workflow

### Sound Designer

1. Create event assets.
2. Assign clips.
3. Tune volume, pitch, randomization, spatial settings, cooldowns, and voice limits.
4. Validate the library.
5. Generate constants.
6. Tell the programmer which event ids should be triggered from gameplay.

### Programmer

1. Put one `AudioManager` in the bootstrap scene.
2. Put one `AudioReferenceRig` in the scene.
3. Call the manager from gameplay code.
4. Do not put direct `AudioSource.Play()` logic in gameplay scripts unless there is a very specific reason.

## Event Id Naming

Use stable, readable ids. Recommended format:

```text
category.object.action
```

Examples:

```text
ui.confirm
ui.cancel
sfx.player.footstep
sfx.enemy.hit
sfx.weapon.shot
ambience.forest.loop
music.combat
dialogue.npc.greeting
```

Avoid renaming event ids after programmers start using them. Renaming an id requires updating code or regenerating constants.

## Audio Event Definition

Create from:

`Create > BitemDev > Audio Engine > Audio Event`

Or use the Audio Engine window.

Purpose:

An `AudioEventDefinition` is one playable sound event. It can contain one clip or several variations.

### Top Fields

| Field | Purpose | Example |
| --- | --- | --- |
| `Event Id` | Stable id used by code and generated constants. | `sfx.player.jump` |
| `Category` | Organizational label. Does not change playback by itself. | `Sfx`, `Music`, `Ambience`, `Dialogue`, `Ui` |
| `Playback Mode` | How the event behaves. | `OneShot`, `Loop`, `Music` |
| `Spatial Mode` | Whether the event is 2D or 3D. | UI = `TwoD`, explosion = `World3D` |
| `Output Mixer Group` | Audio Mixer group for routing. | Master/SFX, Master/Music |
| `Clip Selection Mode` | How to pick clips from the list. | `Random` or `Sequential` |
| `Clips` | AudioClip variations. | 5 footstep clips |

### Clip Entries

Each clip entry has:

| Field | Purpose |
| --- | --- |
| `Clip` | The actual `AudioClip`. |
| `Weight` | Random selection weight. Higher means more likely. Only used in `Random` mode. |
| `Volume` | Per-clip volume multiplier. Useful when one variation is louder. |
| `Pitch` | Per-clip pitch multiplier. Useful when one variation needs correction. |

Example:

- Footstep 1 weight `1`
- Footstep 2 weight `1`
- Footstep 3 weight `0.5`

Footstep 3 plays less often.

### Playback Fields

| Field | Purpose | Recommended Use |
| --- | --- | --- |
| `Volume` | Base event volume. | Start at `1`, lower if needed. |
| `Pitch` | Base pitch. | Keep `1` unless designing variation. |
| `Random Volume` | Adds random volume offset. | Footsteps, cloth, impacts. |
| `Random Pitch` | Adds random pitch offset. | Footsteps, impacts, repeated UI sounds. |
| `Priority` | Unity voice priority. Lower number is more important. | Music/dialogue lower number, tiny debris higher number. |
| `Ignore Listener Pause` | Plays even when `AudioListener.pause` is true. | Pause menu UI. |
| `Delay` | Starts playback after seconds. | Delayed stingers. |
| `Fade In Seconds` | Fades in when starting. | Music, ambience loops. |
| `Fade Out Seconds` | Default fade when stopped/replaced. | Music, ambience loops. |

### 3D Fields

Only matter when `Spatial Mode` is not `TwoD`.

| Field | Purpose | Recommended Use |
| --- | --- | --- |
| `Spatial Blend` | `0` = 2D, `1` = full 3D. | Usually `1` for world sounds. |
| `Min Distance` | Full volume range. | `1` to `3` for small sounds. |
| `Max Distance` | Distance where attenuation stops. | Depends on scene scale. |
| `Rolloff Mode` | Distance attenuation curve. | `Logarithmic` is a good default. |
| `Doppler Level` | Pitch shift from movement. | Use carefully. Often `0` or low in stylized games. |
| `Spread` | Stereo spread for 3D source. | Usually `0` for point sounds. |
| `Reverb Zone Mix` | How much the source feeds reverb zones. | `1` default. |
| `Spatialize` | Enables Unity spatializer plugin if configured. | Only if project uses a spatializer. |

### Voice Management Fields

| Field | Purpose | Example |
| --- | --- | --- |
| `Max Voices` | Maximum simultaneous voices for this event. `0` means unlimited until pool limit. | Footsteps = `4`, gunshot = `8` |
| `Voice Limit Behavior` | What to do when max voices is reached. | Reject new or steal oldest. |
| `Cooldown Seconds` | Minimum time between plays for this event. | UI spam prevention, rapid footsteps. |
| `Stop When Follow Target Is Destroyed` | Stops followed loops when their owner disappears. | Engine loop following a vehicle. |

## Examples

### Example 1: UI Click

Use for menu selection, buttons, pause screen.

Event settings:

```text
Event Id: ui.click
Category: Ui
Playback Mode: OneShot
Spatial Mode: TwoD
Clip Selection Mode: Random
Volume: 1
Random Pitch: 0.03
Ignore Listener Pause: true
Max Voices: 4
Cooldown Seconds: 0.02
```

Programmer call:

```csharp
AudioManager.Instance.Play(AudioEventIds.UI_CLICK);
```

### Example 2: 3D Explosion

Use for a world sound at a specific position.

Event settings:

```text
Event Id: sfx.explosion.large
Category: Sfx
Playback Mode: OneShot
Spatial Mode: World3D
Spatial Blend: 1
Min Distance: 2
Max Distance: 80
Rolloff Mode: Logarithmic
Random Pitch: 0.05
Max Voices: 6
Voice Limit Behavior: StealOldest
```

Programmer call:

```csharp
AudioManager.Instance.PlayAt(AudioEventIds.SFX_EXPLOSION_LARGE, explosionPosition);
```

### Example 3: Player Footsteps

Use several clip variations and call from animation events or movement code.

Event settings:

```text
Event Id: sfx.player.footstep
Category: Sfx
Playback Mode: OneShot
Spatial Mode: World3D
Clip Selection Mode: Random
Random Volume: 0.08
Random Pitch: 0.06
Min Distance: 1
Max Distance: 18
Max Voices: 4
Cooldown Seconds: 0.05
```

Programmer call:

```csharp
AudioManager.Instance.PlayAt(AudioEventIds.SFX_PLAYER_FOOTSTEP, transform.position);
```

Designer-only scene test:

1. Add `AudioEmitter` to the player.
2. Assign the footstep event.
3. Leave `Follow Transform` off.
4. Call `AudioEmitter.Play()` from an animation event.

### Example 4: Vehicle Engine Loop

Use for a sound that follows an object and keeps playing.

Event settings:

```text
Event Id: sfx.vehicle.engine-loop
Category: Sfx
Playback Mode: Loop
Spatial Mode: World3D
Fade In Seconds: 0.2
Fade Out Seconds: 0.3
Stop When Follow Target Is Destroyed: true
```

Programmer call:

```csharp
private AudioPlaybackHandle engineSound;

private void OnEnable()
{
    engineSound = AudioManager.Instance.PlayFollow(AudioEventIds.SFX_VEHICLE_ENGINE_LOOP, transform);
}

private void OnDisable()
{
    engineSound.Stop();
}
```

### Example 5: Music Track

Use `Playback Mode: Music` so starting a new music event fades out the current music voice.

Event settings:

```text
Event Id: music.combat
Category: Music
Playback Mode: Music
Spatial Mode: TwoD
Fade In Seconds: 1
Fade Out Seconds: 1
Output Mixer Group: Music
```

Programmer call:

```csharp
AudioManager.Instance.Play(AudioEventIds.MUSIC_COMBAT);
```

### Example 6: Ambience Zone

Use an `AudioZone` to change ambience or mixer state when the player enters a trigger.

Steps:

1. Create a GameObject named `Forest Audio Zone`.
2. Add a `Box Collider`.
3. Enable `Is Trigger`.
4. Add `AudioZone`.
5. Set `Required Tag` to `Player` if your player uses that tag.
6. Assign an `Enter Event`, `Exit Event`, snapshots, or mixer parameter values.

When the matching object enters the trigger, the zone applies enter audio. When the last matching object exits, it applies exit audio.

## Runtime Components

### AudioManager

Add from:

`GameObject > BitemDev > Audio Engine > Audio Manager`

Purpose:

The central playback system. This should be the only object that creates and controls pooled runtime `AudioSource` voices.

Fields:

| Field | Purpose |
| --- | --- |
| `Config` | Optional `AudioEngineConfig` asset. Recommended. |
| `Event Library` | Library used for string/id lookup. Usually assigned from config. |
| `Master Mixer` | Mixer used when setting parameters without a specific mixer. |
| `Reference Mode` | Camera-only or separate listener/logic reference. Usually assigned by config or rig. |
| `Listener Transform` | Transform used as the listener reference. Usually camera. |
| `Logic Reference Transform` | Transform used for gameplay audio logic. Usually player. |
| `Initial Pool Size` | Number of pooled audio voices created at startup. |
| `Max Pool Size` | Hard cap for pooled voices. |
| `Allow Pool Growth` | Allows pool to grow from initial to max. |
| `Dont Destroy On Load` | Keeps the manager across scenes. |
| `Use Main Camera Fallback` | Uses `Camera.main` if no listener was assigned. |

Common programmer methods:

```csharp
AudioManager.Instance.Play("ui.click");
AudioManager.Instance.PlayAt("sfx.explosion.large", position);
AudioManager.Instance.PlayFollow("sfx.vehicle.engine-loop", transform);
AudioManager.Instance.StopEvent("ambience.forest.loop");
AudioManager.Instance.StopAll();
AudioManager.Instance.SetConfiguredVolume("Master", 0.8f);
```

### AudioReferenceRig

Add from:

`GameObject > BitemDev > Audio Engine > Audio Reference Rig`

Purpose:

Connects the scene camera and logic reference to the manager.

Fields:

| Field | Purpose |
| --- | --- |
| `Reference Mode` | Chooses camera-only or dual-reference behavior. |
| `Listener Camera` | Camera that should hold the `AudioListener`. |
| `Logic Reference` | Player/reference transform used for gameplay audio logic. |
| `Ensure Audio Listener On Camera` | Adds an `AudioListener` to the camera if missing. |

Important:

Unity should only have one active `AudioListener` in a scene. If multiple cameras have listeners, disable or remove the extras.

### AudioEmitter

Add manually to a scene object.

Purpose:

A small designer-friendly component that notifies `AudioManager` without custom code. Useful for props, animation events, timeline events, triggers, or quick tests.

Fields:

| Field | Purpose |
| --- | --- |
| `Audio Event` | Direct event asset reference. Preferred for scene objects. |
| `Event Id` | String id fallback if no asset is assigned. |
| `Play On Start` | Plays once when the object starts. |
| `Play On Enable` | Plays when the object is enabled after Start. |
| `Follow Transform` | If true, the sound follows this GameObject. Good for loops. |
| `Volume Scale` | Per-emitter volume multiplier. |
| `Pitch Scale` | Per-emitter pitch multiplier. |

Public methods:

```csharp
Play()
Stop()
```

These can be called from UnityEvents or animation events.

### AudioZone

Purpose:

Trigger area that applies audio changes when matching objects enter or exit.

Fields:

| Field | Purpose |
| --- | --- |
| `Trigger Layers` | Which layers can activate the zone. |
| `Required Tag` | Optional tag filter, such as `Player`. |
| `Enter Event` | Event played when the first matching object enters. |
| `Exit Event` | Event played when the last matching object exits. |
| `Enter Snapshot` | Audio Mixer snapshot transitioned to on enter. |
| `Exit Snapshot` | Audio Mixer snapshot transitioned to on exit. |
| `Snapshot Transition Seconds` | Transition time for snapshots. |
| `Enter Parameters` | Mixer parameters applied on enter. |
| `Exit Parameters` | Mixer parameters applied on exit. |

Works with 3D colliders and 2D colliders.

### AudioDistanceParameterDriver

Purpose:

Writes a distance-based value into an exposed Audio Mixer parameter.

This component supports gameplay-driven audio logic. It measures distance from the manager's logic reference, not necessarily the camera.

Fields:

| Field | Purpose |
| --- | --- |
| `Target` | Object being measured. If empty, uses this transform. |
| `Mixer` | Mixer that owns the exposed parameter. If empty, uses manager master mixer. |
| `Exposed Parameter` | Name of the exposed mixer parameter. |
| `Value Mode` | Raw value, linear volume converted to dB, or direct dB. |
| `Max Distance` | Distance that maps to the end of the 0-1 range. |
| `Invert` | If true, close = `1`, far = `0`. |
| `Update Interval` | Time between parameter updates. |

Example:

- Enemy near player = combat intensity close to `1`.
- Enemy far from player = combat intensity close to `0`.
- In `CameraOnly` mode, distance is measured from the camera.
- In `SeparateListenerAndLogicReference` mode, distance is measured from the assigned logic reference.

## Designer Assets

### AudioEventLibrary

Purpose:

List of all event assets that can be played by id.

The manager uses this to resolve:

```csharp
AudioManager.Instance.Play("sfx.player.jump");
```

If an event is not in the library, string/id playback will fail and log a warning.

### AudioEngineConfig

Purpose:

Global configuration for the manager.

Fields:

| Field | Purpose |
| --- | --- |
| `Event Library` | Main library used by the manager. |
| `Master Mixer` | Default mixer for parameter writes. |
| `Reference Mode` | Default reference mode. |
| `Initial Pool Size` | Startup voice count. |
| `Max Pool Size` | Maximum voice count. |
| `Allow Pool Growth` | Allows voice pool to grow when busy. |
| `Dont Destroy On Load` | Keeps audio manager alive across scenes. |
| `Use Main Camera Fallback` | Uses `Camera.main` if no listener is assigned. |
| `Volume Bindings` | Named mixer volume controls for settings menus. |

Volume binding example:

```text
Label: Master
Mixer: MainAudioMixer
Exposed Parameter: MasterVolume
Default Linear Volume: 1
```

Programmer call:

```csharp
AudioManager.Instance.SetConfiguredVolume("Master", 0.75f);
```

The manager converts linear volume to decibels correctly.

## Programmer Integration

Recommended usage:

```csharp
using BitemDev.AudioEngine;
using BitemDev.AudioEngine.Generated;
using UnityEngine;

public sealed class ExampleAudioCaller : MonoBehaviour
{
    public void Confirm()
    {
        AudioManager.Instance.Play(AudioEventIds.UI_CONFIRM);
    }

    public void Explode(Vector3 position)
    {
        AudioManager.Instance.PlayAt(AudioEventIds.SFX_EXPLOSION_LARGE, position);
    }
}
```

For looping sounds:

```csharp
using BitemDev.AudioEngine;
using BitemDev.AudioEngine.Generated;
using UnityEngine;

public sealed class EngineAudio : MonoBehaviour
{
    private AudioPlaybackHandle handle;

    private void OnEnable()
    {
        handle = AudioManager.Instance.PlayFollow(AudioEventIds.SFX_VEHICLE_ENGINE_LOOP, transform);
    }

    private void OnDisable()
    {
        handle.Stop();
    }
}
```

Avoid this in gameplay code:

```csharp
GetComponent<AudioSource>().Play();
```

Prefer this:

```csharp
AudioManager.Instance.PlayAt(AudioEventIds.SFX_PLAYER_FOOTSTEP, transform.position);
```

## Audio Mixer Setup

The package works without an Audio Mixer, but a production project should use one.

Recommended mixer groups:

```text
Master
Master/SFX
Master/Music
Master/Ambience
Master/Dialogue
Master/UI
```

Recommended exposed volume parameters:

```text
MasterVolume
SfxVolume
MusicVolume
AmbienceVolume
DialogueVolume
UiVolume
```

In Unity's Audio Mixer:

1. Create the groups.
2. Expose each group volume parameter.
3. Assign groups to `AudioEventDefinition.Output Mixer Group`.
4. Add volume bindings in `AudioEngineConfig`.

## Validation Checklist

Before giving events to the programmer:

1. Open `Tools > BitemDev > Audio Engine > Open Audio Engine Window`.
2. Press `Validate Library`.
3. Fix missing clips.
4. Fix duplicate event ids.
5. Generate `AudioEventIds.cs`.
6. Enter Play Mode and use `Play Through Audio Manager` on a few events.

Scene checklist:

1. One active `AudioManager`.
2. One active `AudioListener`.
3. `AudioReferenceRig.Listener Camera` assigned.
4. `AudioReferenceRig.Logic Reference` assigned for dual-reference projects.
5. `AudioManager.Config` assigned.
6. `AudioEngineConfig.Event Library` assigned.

## Troubleshooting

### I press Play Through Audio Manager but hear nothing

Check:

1. Unity is in Play Mode.
2. The scene has an `AudioManager`.
3. The event has at least one clip.
4. The clip volume and event volume are not `0`.
5. The Audio Mixer group is not muted.
6. There is one active `AudioListener`.

### Programmer calls an event id but nothing plays

Check:

1. The event asset is in `AudioEventLibrary`.
2. The id in code matches the event id exactly.
3. `AudioManager.Event Library` or `AudioEngineConfig.Event Library` is assigned.
4. `Validate Library` has no duplicate id warnings.

### 3D sounds play like 2D sounds

Check:

1. Event `Spatial Mode` is `World3D`.
2. `Spatial Blend` is close to `1`.
3. The programmer is using `PlayAt` or `PlayFollow`, not plain `Play`.
4. The listener camera has the active `AudioListener`.

### 3D sounds are too quiet or disappear too fast

Increase:

- `Min Distance`
- `Max Distance`
- Event `Volume`

Also check the assigned Audio Mixer group volume.

### Camera movement changes gameplay audio behavior

For projects where camera movement should not drive gameplay audio logic, set:

```text
AudioReferenceRig.Reference Mode = SeparateListenerAndLogicReference
AudioReferenceRig.Listener Camera = Main Camera
AudioReferenceRig.Logic Reference = Player
```

For compact projects where camera position should drive everything, set:

```text
AudioReferenceRig.Reference Mode = CameraOnly
```

### Unity warns about multiple AudioListeners

Only one camera should have an active `AudioListener`. Remove or disable extra listeners.

## Feedback To Send Back

When testing the package, useful feedback includes:

1. Which Unity version was used.
2. Whether installation from Git URL worked.
3. Whether the default asset creation was clear.
4. Whether creating and previewing events was clear.
5. Whether field names made sense.
6. Whether any setup step felt missing.
7. Whether the camera/player reference modes match the desired workflow.
8. What extra designer controls would save time.

## Current Scope

Included:

- Central audio manager.
- Event assets.
- Event library.
- Runtime pooling.
- 2D and 3D playback.
- Followed loop playback.
- Music replacement with fade-out.
- Cooldowns and per-event max voices.
- Mixer parameter setting.
- Snapshot transitions.
- Camera-only and separate listener/logic reference modes.
- Designer setup window.
- Library validation.
- Generated event id constants.

Not included yet:

- Timeline clips.
- FMOD/Wwise integration.
- Custom waveform preview.
- Import preset automation for audio files.
- Save-file persistence for user volume settings.
