using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BitemDev.AudioEngine
{
    [CreateAssetMenu(fileName = "NewAudioEvent", menuName = "BitemDev/Audio Engine/Audio Event", order = 10)]
    public sealed class AudioEventDefinition : ScriptableObject
    {
        [SerializeField] private string eventId;
        [SerializeField] private AudioEventCategory category;
        [SerializeField] private AudioEventBus bus = AudioEventBus.Auto;
        [SerializeField] private AudioEventPlaybackMode playbackMode;
        [SerializeField] private AudioEventSpatialMode spatialMode;
        [SerializeField] private AudioMixerGroup outputMixerGroup;
        [SerializeField] private AudioClipSelectionMode clipSelectionMode;
        [SerializeField] private List<AudioClipEntry> clips = new List<AudioClipEntry>();

        [Header("Playback")]
        [SerializeField, Range(0f, 2f)] private float volume = 1f;
        [SerializeField, Range(0.01f, 3f)] private float pitch = 1f;
        [SerializeField, Range(0f, 1f)] private float randomVolume = 0f;
        [SerializeField, Range(0f, 1f)] private float randomPitch = 0f;
        [SerializeField, Range(0, 256)] private int priority = 128;
        [SerializeField] private bool ignoreListenerPause;
        [SerializeField, Min(0f)] private float delay;
        [SerializeField, Min(0f)] private float fadeInSeconds;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.1f;

        [Header("3D")]
        [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
        [SerializeField, Min(0f)] private float minDistance = 1f;
        [SerializeField, Min(0.01f)] private float maxDistance = 40f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField, Range(0f, 5f)] private float dopplerLevel = 1f;
        [SerializeField, Range(0f, 360f)] private float spread = 0f;
        [SerializeField, Range(0f, 1.1f)] private float reverbZoneMix = 1f;
        [SerializeField] private bool spatialize;

        [Header("Voice Management")]
        [SerializeField, Min(0)] private int maxVoices = 0;
        [SerializeField] private AudioVoiceLimitBehavior voiceLimitBehavior = AudioVoiceLimitBehavior.StealOldest;
        [SerializeField, Min(0f)] private float cooldownSeconds;
        [SerializeField] private bool stopWhenFollowTargetIsDestroyed = true;

        public string EventId => eventId;
        public AudioEventCategory Category => category;
        public AudioEventBus Bus => bus;
        public AudioEventPlaybackMode PlaybackMode => playbackMode;
        public AudioEventSpatialMode SpatialMode => spatialMode;
        public AudioMixerGroup OutputMixerGroup => outputMixerGroup;
        public AudioClipSelectionMode ClipSelectionMode => clipSelectionMode;
        public IReadOnlyList<AudioClipEntry> Clips => clips;
        public float Volume => volume;
        public float Pitch => pitch;
        public float RandomVolume => randomVolume;
        public float RandomPitch => randomPitch;
        public int Priority => priority;
        public bool IgnoreListenerPause => ignoreListenerPause;
        public float Delay => delay;
        public float FadeInSeconds => fadeInSeconds;
        public float FadeOutSeconds => fadeOutSeconds;
        public float SpatialBlend => spatialBlend;
        public float MinDistance => minDistance;
        public float MaxDistance => Mathf.Max(minDistance, maxDistance);
        public AudioRolloffMode RolloffMode => rolloffMode;
        public float DopplerLevel => dopplerLevel;
        public float Spread => spread;
        public float ReverbZoneMix => reverbZoneMix;
        public bool Spatialize => spatialize;
        public int MaxVoices => maxVoices;
        public AudioVoiceLimitBehavior VoiceLimitBehavior => voiceLimitBehavior;
        public float CooldownSeconds => cooldownSeconds;
        public bool StopWhenFollowTargetIsDestroyed => stopWhenFollowTargetIsDestroyed;
        public bool IsLooping => playbackMode == AudioEventPlaybackMode.Loop || playbackMode == AudioEventPlaybackMode.Music;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                eventId = AudioEventIdUtility.Sanitize(name);
            }

            if (maxDistance < minDistance)
            {
                maxDistance = minDistance;
            }
        }

        internal AudioClipEntry SelectClip(ref int sequenceIndex)
        {
            if (clips == null || clips.Count == 0)
            {
                return null;
            }

            if (clipSelectionMode == AudioClipSelectionMode.Sequential)
            {
                int start = Mathf.Abs(sequenceIndex);
                for (int i = 0; i < clips.Count; i++)
                {
                    AudioClipEntry entry = clips[(start + i) % clips.Count];
                    sequenceIndex = (start + i + 1) % clips.Count;
                    if (entry != null && entry.Clip != null)
                    {
                        return entry;
                    }
                }

                return null;
            }

            float totalWeight = 0f;
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i] != null && clips[i].Clip != null)
                {
                    totalWeight += Mathf.Max(0f, clips[i].Weight);
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float roll = Random.value * totalWeight;
            for (int i = 0; i < clips.Count; i++)
            {
                AudioClipEntry entry = clips[i];
                if (entry == null || entry.Clip == null)
                {
                    continue;
                }

                roll -= Mathf.Max(0f, entry.Weight);
                if (roll <= 0f)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
