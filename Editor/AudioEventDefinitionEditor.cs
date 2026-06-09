using BitemDev.AudioEngine;
using UnityEditor;
using UnityEngine;

namespace BitemDev.AudioEngine.Editor
{
    [CustomEditor(typeof(AudioEventDefinition))]
    [CanEditMultipleObjects]
    public sealed class AudioEventDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty eventIdProperty;

        private void OnEnable()
        {
            eventIdProperty = serializedObject.FindProperty("eventId");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            if (targets.Length == 1)
            {
                DrawValidation((AudioEventDefinition)target);
                DrawActions((AudioEventDefinition)target);
            }
        }

        private void DrawValidation(AudioEventDefinition definition)
        {
            EditorGUILayout.Space();

            if (string.IsNullOrWhiteSpace(definition.EventId))
            {
                EditorGUILayout.HelpBox("Event Id is required. Programmers use this id to play the event from code.", MessageType.Error);
            }

            bool hasClip = false;
            for (int i = 0; i < definition.Clips.Count; i++)
            {
                if (definition.Clips[i] != null && definition.Clips[i].Clip != null)
                {
                    hasClip = true;
                    break;
                }
            }

            if (!hasClip)
            {
                EditorGUILayout.HelpBox("Add at least one AudioClip before this event can play.", MessageType.Warning);
            }

            if (definition.Bus == AudioEventBus.Custom && definition.OutputMixerGroup == null)
            {
                EditorGUILayout.HelpBox("Custom bus events need an Output Mixer Group because config bus routing only handles named buses.", MessageType.Warning);
            }
            else if (definition.OutputMixerGroup == null)
            {
                EditorGUILayout.HelpBox("This event will use its Bus. Routing works through AudioEngineConfig bus bindings first, then by finding a matching group name in the Master Mixer.", MessageType.Info);
            }

            if (definition.StopEvent == definition)
            {
                EditorGUILayout.HelpBox("Stop Event points back to this event. This is allowed, but usually a short stop/tail event is clearer.", MessageType.Warning);
            }
        }

        private void DrawActions(AudioEventDefinition definition)
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Normalize Id"))
                {
                    serializedObject.Update();
                    eventIdProperty.stringValue = AudioEventIdUtility.Sanitize(string.IsNullOrWhiteSpace(definition.EventId) ? definition.name : definition.EventId);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(definition);
                }

                if (GUILayout.Button("Copy Play Code"))
                {
                    EditorGUIUtility.systemCopyBuffer = $"AudioManager.Instance.Play(\"{definition.EventId}\");";
                }

                if (GUILayout.Button("Copy Stop Code"))
                {
                    EditorGUIUtility.systemCopyBuffer = $"AudioManager.Instance.StopEvent(\"{definition.EventId}\");";
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!Application.isPlaying || AudioManager.Instance == null))
                {
                    if (GUILayout.Button("Play Through Audio Manager"))
                    {
                        AudioManager.Instance.Play(definition);
                    }

                    if (GUILayout.Button("Stop All"))
                    {
                        AudioManager.Instance.StopAll();
                    }

                    if (GUILayout.Button("Stop Event"))
                    {
                        AudioManager.Instance.StopEvent(definition);
                    }
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode with an Audio Manager in the scene to audition this event through the real runtime path.", MessageType.Info);
            }
        }
    }
}
