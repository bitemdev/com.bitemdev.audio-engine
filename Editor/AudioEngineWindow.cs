using System.Collections.Generic;
using System.IO;
using System.Text;
using BitemDev.AudioEngine;
using UnityEditor;
using UnityEngine;

namespace BitemDev.AudioEngine.Editor
{
    public sealed class AudioEngineWindow : EditorWindow
    {
        private const string DefaultFolder = "Assets/AudioEngine";
        private const string DefaultGeneratedFolder = "Assets/AudioEngine/Generated";

        private AudioEventLibrary library;
        private string eventsFolder = DefaultFolder + "/Events";
        private string generatedFolder = DefaultGeneratedFolder;
        private string newEventId = "sfx.new-event";
        private Vector2 scroll;

        [MenuItem("Tools/BitemDev/Audio Engine/Window", false, 2)]
        public static void ShowWindow()
        {
            AudioEngineWindow window = GetWindow<AudioEngineWindow>();
            window.titleContent = new GUIContent("Audio Engine");
            window.minSize = new Vector2(460f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            if (library == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:AudioEventLibrary");
                if (guids.Length > 0)
                {
                    library = AssetDatabase.LoadAssetAtPath<AudioEventLibrary>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
        }

        private void OnGUI()
        {
            DrawHeader();

            if (library == null)
            {
                EditorGUILayout.HelpBox("Create or assign an Audio Event Library to start authoring sound events.", MessageType.Info);
            }
            else
            {
                DrawLibraryTools();
                DrawEventList();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Audio Engine", EditorStyles.boldLabel);
            library = (AudioEventLibrary)EditorGUILayout.ObjectField("Library", library, typeof(AudioEventLibrary), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Default Assets"))
                {
                    AudioEngineMenu.CreateDefaultAssets();
                    library = AssetDatabase.LoadAssetAtPath<AudioEventLibrary>(DefaultFolder + "/AudioEventLibrary.asset");
                }

                if (GUILayout.Button("Select Library") && library != null)
                {
                    Selection.activeObject = library;
                }
            }
        }

        private void DrawLibraryTools()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Create Event", EditorStyles.boldLabel);
            eventsFolder = EditorGUILayout.TextField("Events Folder", eventsFolder);
            newEventId = EditorGUILayout.TextField("Event Id", newEventId);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Event Asset"))
                {
                    CreateEventAsset();
                }

                if (GUILayout.Button("Validate Library"))
                {
                    ValidateLibrary(true);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Programmer Handoff", EditorStyles.boldLabel);
            generatedFolder = EditorGUILayout.TextField("Generated Folder", generatedFolder);
            if (GUILayout.Button("Generate AudioEventIds.cs"))
            {
                GenerateEventIds();
            }
        }

        private void DrawEventList()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            IReadOnlyList<AudioEventDefinition> events = library.Events;
            for (int i = 0; i < events.Count; i++)
            {
                AudioEventDefinition definition = events[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField(definition, typeof(AudioEventDefinition), false);
                    EditorGUILayout.LabelField(definition != null ? definition.EventId : "<missing>", GUILayout.Width(180f));
                    if (GUILayout.Button("Ping", GUILayout.Width(48f)) && definition != null)
                    {
                        EditorGUIUtility.PingObject(definition);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void CreateEventAsset()
        {
            if (library == null)
            {
                return;
            }

            AudioEngineMenu.EnsureFolder(eventsFolder);
            string sanitizedId = AudioEventIdUtility.Sanitize(newEventId);
            string fileName = sanitizedId.Replace('.', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath(eventsFolder + "/" + fileName + ".asset");

            AudioEventDefinition definition = CreateInstance<AudioEventDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("eventId").stringValue = sanitizedId;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(definition, path);
            AddEventToLibrary(definition);
            AssetDatabase.SaveAssets();
            Selection.activeObject = definition;
        }

        private void AddEventToLibrary(AudioEventDefinition definition)
        {
            SerializedObject serializedLibrary = new SerializedObject(library);
            SerializedProperty eventsProperty = serializedLibrary.FindProperty("events");
            eventsProperty.arraySize++;
            eventsProperty.GetArrayElementAtIndex(eventsProperty.arraySize - 1).objectReferenceValue = definition;
            serializedLibrary.ApplyModifiedProperties();
            EditorUtility.SetDirty(library);
        }

        private void ValidateLibrary(bool showDialog)
        {
            List<string> issues = CollectValidationIssues();
            if (!showDialog)
            {
                return;
            }

            string message = issues.Count == 0 ? "No validation issues found." : string.Join("\n", issues);
            EditorUtility.DisplayDialog("Audio Event Library Validation", message, "OK");
        }

        private List<string> CollectValidationIssues()
        {
            List<string> issues = new List<string>();
            Dictionary<string, AudioEventDefinition> seenIds = new Dictionary<string, AudioEventDefinition>();

            IReadOnlyList<AudioEventDefinition> events = library.Events;
            for (int i = 0; i < events.Count; i++)
            {
                AudioEventDefinition definition = events[i];
                if (definition == null)
                {
                    issues.Add($"Entry {i}: missing event asset.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(definition.EventId))
                {
                    issues.Add($"{definition.name}: missing Event Id.");
                }
                else if (seenIds.TryGetValue(definition.EventId, out AudioEventDefinition duplicate))
                {
                    issues.Add($"{definition.name}: duplicate Event Id '{definition.EventId}' also used by {duplicate.name}.");
                }
                else
                {
                    seenIds.Add(definition.EventId, definition);
                }

                bool hasClip = false;
                for (int c = 0; c < definition.Clips.Count; c++)
                {
                    if (definition.Clips[c] != null && definition.Clips[c].Clip != null)
                    {
                        hasClip = true;
                        break;
                    }
                }

                if (!hasClip)
                {
                    issues.Add($"{definition.name}: no AudioClip assigned.");
                }
            }

            return issues;
        }

        private void GenerateEventIds()
        {
            if (library == null)
            {
                return;
            }

            AudioEngineMenu.EnsureFolder(generatedFolder);
            string assetPath = generatedFolder + "/AudioEventIds.cs";
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("namespace BitemDev.AudioEngine.Generated");
            builder.AppendLine("{");
            builder.AppendLine("    public static class AudioEventIds");
            builder.AppendLine("    {");

            HashSet<string> usedNames = new HashSet<string>();
            IReadOnlyList<AudioEventDefinition> events = library.Events;
            for (int i = 0; i < events.Count; i++)
            {
                AudioEventDefinition definition = events[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.EventId))
                {
                    continue;
                }

                string constantName = ToConstantName(definition.EventId);
                if (!usedNames.Add(constantName))
                {
                    constantName = constantName + "_" + i;
                }

                builder.AppendLine($"        public const string {constantName} = \"{definition.EventId}\";");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            File.WriteAllText(fullPath, builder.ToString(), Encoding.UTF8);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath));
        }

        private static string ToConstantName(string eventId)
        {
            StringBuilder builder = new StringBuilder(eventId.Length);
            bool previousUnderscore = false;

            for (int i = 0; i < eventId.Length; i++)
            {
                char c = eventId[i];
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(char.ToUpperInvariant(c));
                    previousUnderscore = false;
                }
                else if (!previousUnderscore)
                {
                    builder.Append('_');
                    previousUnderscore = true;
                }
            }

            string result = builder.ToString().Trim('_');
            if (string.IsNullOrEmpty(result))
            {
                return "AUDIO_EVENT";
            }

            if (char.IsDigit(result[0]))
            {
                result = "_" + result;
            }

            return result;
        }
    }
}
