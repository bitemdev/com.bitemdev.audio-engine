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

            if (definition.SpatialMode != AudioEventSpatialMode.TwoD && definition.OutputMixerGroup == null)
            {
                EditorGUILayout.HelpBox("3D events work without a mixer group, but routing them through an Audio Mixer is recommended.", MessageType.Info);
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
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode with an Audio Manager in the scene to audition this event through the real runtime path.", MessageType.Info);
            }
        }
    }
}
