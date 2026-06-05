using UnityEngine;

namespace BitemDev.AudioEngine
{
    public sealed class AudioEngineDemoPanel : MonoBehaviour
    {
        [SerializeField] private AudioEventDefinition uiClickEvent;
        [SerializeField] private AudioEventDefinition worldPingEvent;
        [SerializeField] private AudioEventDefinition followLoopEvent;
        [SerializeField] private AudioEventDefinition musicCalmEvent;
        [SerializeField] private AudioEventDefinition musicIntenseEvent;
        [SerializeField] private Transform worldEmitter;
        [SerializeField] private Transform movingEmitter;
        [SerializeField] private string playById = "demo.ui.click";
        [SerializeField] private bool moveEmitter = true;
        [SerializeField, Min(0.5f)] private float moveRadius = 3f;
        [SerializeField, Min(0.1f)] private float moveSpeed = 1f;

        private AudioPlaybackHandle followHandle;
        private Vector3 movingStartPosition;

        private void Start()
        {
            if (movingEmitter != null)
            {
                movingStartPosition = movingEmitter.position;
            }
        }

        private void Update()
        {
            if (!moveEmitter || movingEmitter == null)
            {
                return;
            }

            float angle = Time.time * moveSpeed;
            movingEmitter.position = movingStartPosition + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * moveRadius;
        }

        private void OnGUI()
        {
            const int width = 360;
            GUILayout.BeginArea(new Rect(16, 16, width, Screen.height - 32), GUI.skin.box);
            GUILayout.Label("BitemDev Audio Engine Demo");
            GUILayout.Space(6);

            if (AudioManager.Instance == null)
            {
                GUILayout.Label("No AudioManager found in the scene.");
                GUILayout.EndArea();
                return;
            }

            if (GUILayout.Button("Play 2D UI Event"))
            {
                AudioManager.Instance.Play(uiClickEvent);
            }

            if (GUILayout.Button("Play Event By Id"))
            {
                AudioManager.Instance.Play(playById);
            }

            if (GUILayout.Button("Play 3D Event At Cube"))
            {
                AudioManager.Instance.PlayAt(worldPingEvent, worldEmitter != null ? worldEmitter.position : transform.position);
            }

            string followButton = followHandle.IsValid ? "Stop Follow Loop" : "Start Follow Loop";
            if (GUILayout.Button(followButton))
            {
                if (followHandle.IsValid)
                {
                    followHandle.Stop();
                }
                else
                {
                    followHandle = AudioManager.Instance.PlayFollow(followLoopEvent, movingEmitter != null ? movingEmitter : transform);
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Music A"))
            {
                AudioManager.Instance.Play(musicCalmEvent);
            }

            if (GUILayout.Button("Music B"))
            {
                AudioManager.Instance.Play(musicIntenseEvent);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Stop All"))
            {
                followHandle = default;
                AudioManager.Instance.StopAll(0.25f);
            }

            moveEmitter = GUILayout.Toggle(moveEmitter, "Move follow emitter");
            GUILayout.Space(6);
            GUILayout.Label("Tests: 2D, 3D position, event-id lookup, followed loop, music replacement, pooling, fades.");
            GUILayout.EndArea();
        }
    }
}
