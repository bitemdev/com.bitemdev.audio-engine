using UnityEngine;

namespace BitemDev.AudioEngine
{
    [DisallowMultipleComponent]
    public sealed class AudioEmitter : MonoBehaviour
    {
        [SerializeField] private AudioEventDefinition audioEvent;
        [SerializeField] private string eventId;
        [SerializeField] private bool playOnStart;
        [SerializeField] private bool playOnEnable;
        [SerializeField] private bool followTransform;
        [SerializeField, Range(0f, 2f)] private float volumeScale = 1f;
        [SerializeField, Range(0.01f, 3f)] private float pitchScale = 1f;

        private AudioPlaybackHandle activeHandle;
        private bool started;

        private void OnEnable()
        {
            if (playOnEnable && started)
            {
                Play();
            }
        }

        private void Start()
        {
            started = true;
            if (playOnStart)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            if (activeHandle.IsValid)
            {
                activeHandle.Stop();
            }
        }

        public void Play()
        {
            if (AudioManager.Instance == null)
            {
                return;
            }

            AudioPlaybackRequest request = audioEvent != null
                ? AudioPlaybackRequest.ForEvent(audioEvent)
                : AudioPlaybackRequest.ForEventId(eventId);

            request = followTransform
                ? request.Follow(transform).WithScales(volumeScale, pitchScale)
                : request.At(transform.position).WithScales(volumeScale, pitchScale);

            activeHandle = AudioManager.Instance.Play(request);
        }

        public void Stop()
        {
            activeHandle.Stop();
        }

        public void Stop(float fadeOutSeconds)
        {
            activeHandle.Stop(fadeOutSeconds);
        }
    }
}
