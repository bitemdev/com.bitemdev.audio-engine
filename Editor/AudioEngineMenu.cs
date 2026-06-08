using BitemDev.AudioEngine;
using UnityEditor;
using UnityEngine;

namespace BitemDev.AudioEngine.Editor
{
    public static class AudioEngineMenu
    {
        private const string DefaultFolder = "Assets/AudioEngine";
        private const string ConfigPath = DefaultFolder + "/AudioEngineConfig.asset";
        private const string LibraryPath = DefaultFolder + "/AudioEventLibrary.asset";

        [MenuItem("Tools/BitemDev/Audio Engine/Open Audio Engine Window", false, 1)]
        public static void OpenWindow()
        {
            AudioEngineWindow.ShowWindow();
        }

        [MenuItem("Tools/BitemDev/Audio Engine/Create Default Assets", false, 20)]
        public static void CreateDefaultAssets()
        {
            EnsureFolder(DefaultFolder);
            AudioEventLibrary library = LoadOrCreate<AudioEventLibrary>(LibraryPath);
            AudioEngineConfig config = LoadOrCreate<AudioEngineConfig>(ConfigPath);

            SerializedObject configObject = new SerializedObject(config);
            bool changed = false;
            SerializedProperty libraryProperty = configObject.FindProperty("eventLibrary");
            if (libraryProperty.objectReferenceValue == null)
            {
                libraryProperty.objectReferenceValue = library;
                changed = true;
            }

            EnsureDefaultBusBindings(configObject, ref changed);
            if (changed)
            {
                configObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = config;
        }

        [MenuItem("GameObject/BitemDev/Audio Engine/Audio Manager", false, 10)]
        public static void CreateAudioManager(MenuCommand menuCommand)
        {
            CreateDefaultAssets();

            AudioManager existingManager = FindExistingAudioManager();
            if (existingManager != null)
            {
                Selection.activeGameObject = existingManager.gameObject;
                EditorUtility.DisplayDialog(
                    "Audio Manager Already Exists",
                    "Only one Audio Manager should exist in a project. The existing Audio Manager has been selected.",
                    "OK");
                return;
            }

            GameObject managerObject = new GameObject("Audio Manager");
            GameObjectUtility.SetParentAndAlign(managerObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(managerObject, "Create Audio Manager");

            AudioManager manager = managerObject.AddComponent<AudioManager>();
            SerializedObject managerSerialized = new SerializedObject(manager);
            managerSerialized.FindProperty("config").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioEngineConfig>(ConfigPath);
            managerSerialized.FindProperty("eventLibrary").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioEventLibrary>(LibraryPath);
            managerSerialized.ApplyModifiedProperties();

            Selection.activeGameObject = managerObject;
        }

        internal static AudioManager FindExistingAudioManager()
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            return Object.FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
#else
            return Object.FindObjectOfType<AudioManager>(true);
#endif
        }

        private static void EnsureDefaultBusBindings(SerializedObject configObject, ref bool changed)
        {
            SerializedProperty busBindings = configObject.FindProperty("busBindings");
            if (busBindings == null || busBindings.arraySize > 0)
            {
                return;
            }

            AudioEventBus[] defaultBuses =
            {
                AudioEventBus.Sfx,
                AudioEventBus.Music,
                AudioEventBus.Ambience,
                AudioEventBus.Dialogue,
                AudioEventBus.Ui,
                AudioEventBus.Master
            };

            busBindings.arraySize = defaultBuses.Length;
            for (int i = 0; i < defaultBuses.Length; i++)
            {
                SerializedProperty element = busBindings.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("bus").enumValueIndex = (int)defaultBuses[i];
                element.FindPropertyRelative("mixerGroup").objectReferenceValue = null;
            }

            changed = true;
        }

        [MenuItem("GameObject/BitemDev/Audio Engine/Audio Reference Rig", false, 11)]
        public static void CreateAudioReferenceRig(MenuCommand menuCommand)
        {
            GameObject rigObject = new GameObject("Audio Reference Rig");
            GameObjectUtility.SetParentAndAlign(rigObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(rigObject, "Create Audio Reference Rig");
            rigObject.AddComponent<AudioReferenceRig>();
            Selection.activeGameObject = rigObject;
        }

        internal static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        internal static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
