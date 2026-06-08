using System;
using System.IO;
using BitemDev.AudioEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BitemDev.AudioEngine.Editor
{
    public static class AudioEngineDemoBuilder
    {
        private const string DemoFolder = "Assets/AudioEngine/Demo";
        private const string ClipsFolder = DemoFolder + "/Clips";
        private const string EventsFolder = DemoFolder + "/Events";
        private const string ConfigPath = "Assets/AudioEngine/AudioEngineConfig.asset";
        private const string LibraryPath = "Assets/AudioEngine/AudioEventLibrary.asset";

        [MenuItem("Tools/BitemDev/Audio Engine/Create No-Code Demo In Current Scene", false, 21)]
        public static void CreateDemo()
        {
            AudioEngineMenu.CreateDefaultAssets();
            AudioEngineMenu.EnsureFolder(DemoFolder);
            AudioEngineMenu.EnsureFolder(ClipsFolder);
            AudioEngineMenu.EnsureFolder(EventsFolder);

            AudioEventLibrary library = AssetDatabase.LoadAssetAtPath<AudioEventLibrary>(LibraryPath);
            AudioEngineConfig config = AssetDatabase.LoadAssetAtPath<AudioEngineConfig>(ConfigPath);

            AudioClip uiClick = CreateToneClip("demo-ui-click", 880f, 0.12f, 0.4f);
            AudioClip worldPing = CreateToneClip("demo-world-ping", 440f, 0.35f, 0.5f);
            AudioClip followLoop = CreateToneClip("demo-follow-loop", 180f, 1.0f, 0.25f);
            AudioClip musicCalm = CreateToneClip("demo-music-calm", 261.63f, 2.0f, 0.25f);
            AudioClip musicIntense = CreateToneClip("demo-music-intense", 329.63f, 2.0f, 0.25f);

            AudioEventDefinition uiEvent = CreateOrUpdateEvent(
                "demo.ui.click",
                "Demo UI Click",
                AudioEventCategory.Ui,
                AudioEventPlaybackMode.OneShot,
                AudioEventSpatialMode.TwoD,
                uiClick,
                1f,
                1f,
                0.02f,
                0f,
                0f,
                0.03f,
                4);

            AudioEventDefinition worldEvent = CreateOrUpdateEvent(
                "demo.sfx.world-ping",
                "Demo World Ping",
                AudioEventCategory.Sfx,
                AudioEventPlaybackMode.OneShot,
                AudioEventSpatialMode.World3D,
                worldPing,
                1f,
                1f,
                0f,
                1f,
                2f,
                0.04f,
                6);

            AudioEventDefinition followEvent = CreateOrUpdateEvent(
                "demo.sfx.follow-loop",
                "Demo Follow Loop",
                AudioEventCategory.Sfx,
                AudioEventPlaybackMode.Loop,
                AudioEventSpatialMode.World3D,
                followLoop,
                0.55f,
                1f,
                0f,
                1f,
                2f,
                0f,
                2,
                0.2f,
                0.3f);

            AudioEventDefinition musicCalmEvent = CreateOrUpdateEvent(
                "demo.music.calm",
                "Demo Music Calm",
                AudioEventCategory.Music,
                AudioEventPlaybackMode.Music,
                AudioEventSpatialMode.TwoD,
                musicCalm,
                0.4f,
                1f,
                0f,
                0f,
                0f,
                0f,
                1,
                0.4f,
                0.4f);

            AudioEventDefinition musicIntenseEvent = CreateOrUpdateEvent(
                "demo.music.intense",
                "Demo Music Intense",
                AudioEventCategory.Music,
                AudioEventPlaybackMode.Music,
                AudioEventSpatialMode.TwoD,
                musicIntense,
                0.4f,
                1f,
                0f,
                0f,
                0f,
                0f,
                1,
                0.4f,
                0.4f);

            AddEventsToLibrary(library, uiEvent, worldEvent, followEvent, musicCalmEvent, musicIntenseEvent);
            CreateSceneObjects(config, library, uiEvent, worldEvent, followEvent, musicCalmEvent, musicIntenseEvent);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Audio Engine Demo Created",
                    "The no-code demo is ready. Press Play, then use the on-screen buttons to test the audio engine.",
                    "OK");
            }
        }

        private static AudioClip CreateToneClip(string fileName, float frequency, float duration, float amplitude)
        {
            string assetPath = ClipsFolder + "/" + fileName + ".wav";
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            WriteSineWave(fullPath, frequency, duration, amplitude);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static AudioEventDefinition CreateOrUpdateEvent(
            string eventId,
            string displayName,
            AudioEventCategory category,
            AudioEventPlaybackMode playbackMode,
            AudioEventSpatialMode spatialMode,
            AudioClip clip,
            float volume,
            float pitch,
            float cooldown,
            float spatialBlend,
            float minDistance,
            float randomPitch,
            int maxVoices,
            float fadeIn = 0f,
            float fadeOut = 0.1f)
        {
            string path = EventsFolder + "/" + eventId.Replace('.', '-') + ".asset";
            AudioEventDefinition definition = AssetDatabase.LoadAssetAtPath<AudioEventDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<AudioEventDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.name = displayName;
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("eventId").stringValue = eventId;
            serialized.FindProperty("category").enumValueIndex = (int)category;
            serialized.FindProperty("playbackMode").enumValueIndex = (int)playbackMode;
            serialized.FindProperty("spatialMode").enumValueIndex = (int)spatialMode;
            serialized.FindProperty("clipSelectionMode").enumValueIndex = (int)AudioClipSelectionMode.Random;
            serialized.FindProperty("volume").floatValue = volume;
            serialized.FindProperty("pitch").floatValue = pitch;
            serialized.FindProperty("randomPitch").floatValue = randomPitch;
            serialized.FindProperty("cooldownSeconds").floatValue = cooldown;
            serialized.FindProperty("spatialBlend").floatValue = spatialBlend;
            serialized.FindProperty("minDistance").floatValue = Mathf.Max(0f, minDistance);
            serialized.FindProperty("maxDistance").floatValue = spatialMode == AudioEventSpatialMode.TwoD ? 40f : 25f;
            serialized.FindProperty("maxVoices").intValue = maxVoices;
            serialized.FindProperty("fadeInSeconds").floatValue = fadeIn;
            serialized.FindProperty("fadeOutSeconds").floatValue = fadeOut;

            SerializedProperty clips = serialized.FindProperty("clips");
            clips.arraySize = 1;
            SerializedProperty entry = clips.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("clip").objectReferenceValue = clip;
            entry.FindPropertyRelative("weight").floatValue = 1f;
            entry.FindPropertyRelative("volume").floatValue = 1f;
            entry.FindPropertyRelative("pitch").floatValue = 1f;

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void AddEventsToLibrary(AudioEventLibrary library, params AudioEventDefinition[] events)
        {
            if (library == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(library);
            SerializedProperty eventList = serialized.FindProperty("events");

            for (int i = 0; i < events.Length; i++)
            {
                AudioEventDefinition definition = events[i];
                if (definition == null || ContainsEvent(eventList, definition))
                {
                    continue;
                }

                eventList.arraySize++;
                eventList.GetArrayElementAtIndex(eventList.arraySize - 1).objectReferenceValue = definition;
            }

            serialized.ApplyModifiedProperties();
            library.RebuildCache();
            EditorUtility.SetDirty(library);
        }

        private static bool ContainsEvent(SerializedProperty eventList, AudioEventDefinition definition)
        {
            for (int i = 0; i < eventList.arraySize; i++)
            {
                if (eventList.GetArrayElementAtIndex(i).objectReferenceValue == definition)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CreateSceneObjects(
            AudioEngineConfig config,
            AudioEventLibrary library,
            AudioEventDefinition uiEvent,
            AudioEventDefinition worldEvent,
            AudioEventDefinition followEvent,
            AudioEventDefinition musicCalmEvent,
            AudioEventDefinition musicIntenseEvent)
        {
            GameObject root = GameObject.Find("BitemDev Audio Engine Demo");
            if (root == null)
            {
                root = new GameObject("BitemDev Audio Engine Demo");
                Undo.RegisterCreatedObjectUndo(root, "Create Audio Engine Demo");
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 5f, -8f);
                cameraObject.transform.rotation = Quaternion.Euler(32f, 0f, 0f);
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Demo Camera");
            }
            else if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }

            GameObject logicReference = GetOrCreateChild(root.transform, "Logic Reference");
            logicReference.transform.position = Vector3.zero;

            AudioManager manager = AudioEngineMenu.FindExistingAudioManager();
            GameObject managerObject = manager != null ? manager.gameObject : null;
            if (managerObject == null)
            {
                managerObject = new GameObject("Audio Manager");
                Undo.RegisterCreatedObjectUndo(managerObject, "Create Audio Manager");
            }

            if (manager == null)
            {
                manager = managerObject.AddComponent<AudioManager>();
            }

            SerializedObject managerSerialized = new SerializedObject(manager);
            managerSerialized.FindProperty("config").objectReferenceValue = config;
            managerSerialized.FindProperty("eventLibrary").objectReferenceValue = library;
            managerSerialized.FindProperty("referenceMode").enumValueIndex = (int)AudioEngineReferenceMode.SeparateListenerAndLogicReference;
            managerSerialized.FindProperty("listenerTransform").objectReferenceValue = camera.transform;
            managerSerialized.FindProperty("logicReferenceTransform").objectReferenceValue = logicReference.transform;
            managerSerialized.ApplyModifiedProperties();

            GameObject rigObject = GetOrCreateChild(root.transform, "Audio Reference Rig");
            AudioReferenceRig rig = rigObject.GetComponent<AudioReferenceRig>();
            if (rig == null)
            {
                rig = rigObject.AddComponent<AudioReferenceRig>();
            }

            SerializedObject rigSerialized = new SerializedObject(rig);
            rigSerialized.FindProperty("referenceMode").enumValueIndex = (int)AudioEngineReferenceMode.SeparateListenerAndLogicReference;
            rigSerialized.FindProperty("listenerCamera").objectReferenceValue = camera;
            rigSerialized.FindProperty("logicReference").objectReferenceValue = logicReference.transform;
            rigSerialized.ApplyModifiedProperties();

            GameObject worldCube = GetOrCreatePrimitive(root.transform, "3D World Ping Position", PrimitiveType.Cube);
            worldCube.transform.position = new Vector3(-2.5f, 0.5f, 0f);
            SetRendererColor(worldCube, Color.green);

            GameObject movingSphere = GetOrCreatePrimitive(root.transform, "Moving Follow Loop Position", PrimitiveType.Sphere);
            movingSphere.transform.position = new Vector3(2.5f, 0.5f, 0f);
            SetRendererColor(movingSphere, Color.cyan);

            GameObject panelObject = GetOrCreateChild(root.transform, "No-Code Demo Panel");
            AudioEngineDemoPanel panel = panelObject.GetComponent<AudioEngineDemoPanel>();
            if (panel == null)
            {
                panel = panelObject.AddComponent<AudioEngineDemoPanel>();
            }

            SerializedObject panelSerialized = new SerializedObject(panel);
            panelSerialized.FindProperty("uiClickEvent").objectReferenceValue = uiEvent;
            panelSerialized.FindProperty("worldPingEvent").objectReferenceValue = worldEvent;
            panelSerialized.FindProperty("followLoopEvent").objectReferenceValue = followEvent;
            panelSerialized.FindProperty("musicCalmEvent").objectReferenceValue = musicCalmEvent;
            panelSerialized.FindProperty("musicIntenseEvent").objectReferenceValue = musicIntenseEvent;
            panelSerialized.FindProperty("worldEmitter").objectReferenceValue = worldCube.transform;
            panelSerialized.FindProperty("movingEmitter").objectReferenceValue = movingSphere.transform;
            panelSerialized.FindProperty("playById").stringValue = "demo.ui.click";
            panelSerialized.ApplyModifiedProperties();

            Selection.activeGameObject = root;
        }

        private static GameObject GetOrCreateChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(child, "Create " + name);
            return child;
        }

        private static GameObject GetOrCreatePrimitive(Transform parent, string name, PrimitiveType primitiveType)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(primitive, "Create " + name);
            return primitive;
        }

        private static void SetRendererColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Standard"));
                renderer.sharedMaterial.color = color;
            }
        }

        private static void WriteSineWave(string path, float frequency, float duration, float amplitude)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            short[] samples = new short[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float envelope = GetEnvelope(i, sampleCount);
                double sample = Math.Sin(2.0 * Math.PI * frequency * i / sampleRate) * amplitude * envelope;
                samples[i] = (short)Mathf.Clamp((float)(sample * short.MaxValue), short.MinValue, short.MaxValue);
            }

            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                int dataLength = samples.Length * sizeof(short);
                writer.Write(new byte[] { 82, 73, 70, 70 });
                writer.Write(36 + dataLength);
                writer.Write(new byte[] { 87, 65, 86, 69 });
                writer.Write(new byte[] { 102, 109, 116, 32 });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * sizeof(short));
                writer.Write((short)sizeof(short));
                writer.Write((short)16);
                writer.Write(new byte[] { 100, 97, 116, 97 });
                writer.Write(dataLength);

                for (int i = 0; i < samples.Length; i++)
                {
                    writer.Write(samples[i]);
                }
            }
        }

        private static float GetEnvelope(int sampleIndex, int sampleCount)
        {
            int fadeSamples = Mathf.Min(sampleCount / 8, 2048);
            if (fadeSamples <= 0)
            {
                return 1f;
            }

            if (sampleIndex < fadeSamples)
            {
                return sampleIndex / (float)fadeSamples;
            }

            int remaining = sampleCount - sampleIndex - 1;
            if (remaining < fadeSamples)
            {
                return remaining / (float)fadeSamples;
            }

            return 1f;
        }
    }
}
