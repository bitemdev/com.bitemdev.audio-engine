# BitemDev Audio Engine

Centralized Unity audio middleware for designer-authored sound events, pooled playback, mixer routing, listener/reference configuration, ambience zones, and runtime audio parameters.

## Install

Use Unity Package Manager:

1. Open `Window > Package Manager`.
2. Press `+`.
3. Choose `Add package from git URL...`.
4. Enter the GitHub URL for this package.

## Core Idea

Gameplay code does not own audio playback. Gameplay objects notify `AudioManager`, and sound designers author `AudioEventDefinition` assets that describe clips, routing, randomization, spatial behavior, cooldowns, voice limits, and fades.

The package supports both common setups:

- Small/gamejam: `AudioEngineReferenceMode.CameraOnly`, where the camera is both the listener and gameplay audio reference.
- Medium/big projects: `SeparateListenerAndLogicReference`, where the camera owns spatial listening and the player or controlled actor drives gameplay audio logic such as proximity, ambience, and intensity.

## Quick Setup

1. Run `Tools > BitemDev > Audio Engine > Create Default Assets`.
2. Add `GameObject > BitemDev > Audio Engine > Audio Manager` to the bootstrap scene.
3. Add `GameObject > BitemDev > Audio Engine > Audio Reference Rig`.
4. Assign the listener camera and optional logic reference, usually the player.
5. Open `Tools > BitemDev > Audio Engine > Open Audio Engine Window`.
6. Create audio event assets and add clips.
7. Generate `AudioEventIds.cs` for programmer-friendly constants.

## Programmer API

```csharp
using BitemDev.AudioEngine;
using BitemDev.AudioEngine.Generated;

AudioManager.Instance.Play(AudioEventIds.UI_CONFIRM);
AudioManager.Instance.PlayAt(AudioEventIds.SFX_EXPLOSION, transform.position);
AudioManager.Instance.PlayFollow(AudioEventIds.SFX_ENGINE_LOOP, vehicle.transform);
AudioManager.Instance.SetConfiguredVolume("Master", 0.8f);
```

`PlayFollow` returns an `AudioPlaybackHandle` for stopping loops or changing per-voice volume:

```csharp
AudioPlaybackHandle handle = AudioManager.Instance.PlayFollow(AudioEventIds.SFX_ENGINE_LOOP, transform);
handle.Stop();
```

## Runtime Components

- `AudioManager`: central playback, event lookup, pooling, voice stealing, cooldowns, mixer parameters, snapshots, and music crossfades.
- `AudioReferenceRig`: registers camera/listener and gameplay logic reference.
- `AudioEmitter`: scene component that notifies the manager from UnityEvents, animation events, or inspector-driven hooks.
- `AudioZone`: trigger volume for ambience, snapshots, enter/exit events, and mixer parameters.
- `AudioDistanceParameterDriver`: writes a normalized distance value between the logic reference and a target into a mixer parameter.

## Designer Assets

- `AudioEventDefinition`: one sound event with clips, mixer group, spatial mode, random volume/pitch, delay, fade, cooldown, priority, and max voices.
- `AudioEventLibrary`: central lookup table from event id to event asset.
- `AudioEngineConfig`: manager defaults, pool sizing, reference mode, mixer, library, and volume bindings.

## Validation

Use `Tools > BitemDev > Audio Engine > Open Audio Engine Window > Validate Library` to catch missing clips, duplicate ids, and missing event assets.

This repository cannot run Unity compilation by itself. After importing into a Unity project, validate with:

1. Open a test scene with one `AudioManager`.
2. Create one 2D UI event and one 3D positional event.
3. Call `Play`, `PlayAt`, and `PlayFollow`.
4. Switch `AudioReferenceRig` between `CameraOnly` and `SeparateListenerAndLogicReference`.
5. Confirm only one `AudioListener` is active in the scene.
