using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BitemDev.AudioEngine
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-500)]
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioEngineConfig config;
        [SerializeField] private AudioEventLibrary eventLibrary;
        [SerializeField] private AudioMixer masterMixer;
        [SerializeField] private AudioEngineReferenceMode referenceMode = AudioEngineReferenceMode.SeparateListenerAndLogicReference;
        [SerializeField] private Transform listenerTransform;
        [SerializeField] private Transform logicReferenceTransform;
        [SerializeField, Min(1)] private int initialPoolSize = 32;
        [SerializeField, Min(1)] private int maxPoolSize = 96;
        [SerializeField] private bool allowPoolGrowth = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool useMainCameraFallback = true;

        private readonly List<AudioVoice> voices = new List<AudioVoice>();
        private readonly Dictionary<string, float> nextAllowedPlayTimes = new Dictionary<string, float>();
        private readonly Dictionary<string, int> sequenceIndices = new Dictionary<string, int>();
        private int nextVoiceId = 1;
        private AudioVoice currentMusicVoice;

        public static AudioManager Instance { get; private set; }

        public Transform ListenerTransform => ResolveListenerTransform();
        public Transform LogicReferenceTransform => ResolveLogicReferenceTransform();
        public AudioEngineReferenceMode ReferenceMode => referenceMode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ApplyConfig(config);

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            BuildPool(initialPoolSize);
        }

        private void Update()
        {
            for (int i = 0; i < voices.Count; i++)
            {
                if (voices[i].IsFinished)
                {
                    if (currentMusicVoice == voices[i])
                    {
                        currentMusicVoice = null;
                    }

                    voices[i].Release();
                }
            }
        }

        public void ApplyConfig(AudioEngineConfig newConfig)
        {
            config = newConfig;
            if (config == null)
            {
                return;
            }

            eventLibrary = config.EventLibrary;
            masterMixer = config.MasterMixer;
            referenceMode = config.ReferenceMode;
            initialPoolSize = config.InitialPoolSize;
            maxPoolSize = config.MaxPoolSize;
            allowPoolGrowth = config.AllowPoolGrowth;
            dontDestroyOnLoad = config.DontDestroyOnLoad;
            useMainCameraFallback = config.UseMainCameraFallback;
            ApplyDefaultVolumeBindings();
        }

        public void SetReferences(Transform listener, Transform logicReference, AudioEngineReferenceMode mode)
        {
            listenerTransform = listener;
            logicReferenceTransform = logicReference;
            referenceMode = mode;
        }

        public AudioPlaybackHandle Play(string eventId, float volumeScale = 1f, float pitchScale = 1f)
        {
            return Play(AudioPlaybackRequest.ForEventId(eventId).WithScales(volumeScale, pitchScale));
        }

        public AudioPlaybackHandle Play(AudioEventDefinition definition, float volumeScale = 1f, float pitchScale = 1f)
        {
            return Play(AudioPlaybackRequest.ForEvent(definition).WithScales(volumeScale, pitchScale));
        }

        public AudioPlaybackHandle PlayAt(string eventId, Vector3 position, float volumeScale = 1f, float pitchScale = 1f)
        {
            return Play(AudioPlaybackRequest.ForEventId(eventId).At(position).WithScales(volumeScale, pitchScale));
        }

        public AudioPlaybackHandle PlayAt(AudioEventDefinition definition, Vector3 position, float volumeScale = 1f, float pitchScale = 1f)
        {
            return Play(AudioPlaybackRequest.ForEvent(definition).At(position).WithScales(volumeScale, pitchScale));
        }

        public AudioPlaybackHandle PlayFollow(string eventId, Transform target, float volumeScale = 1f, float pitchScale = 1f)
        {
            return Play(AudioPlaybackRequest.ForEventId(eventId).Follow(target).WithScales(volumeScale, pitchScale));
        }

        public AudioPlaybackHandle PlayFollow(AudioEventDefinition definition, Transform target, float volumeScale = 1f, float pitchScale = 1f)
        {
            return Play(AudioPlaybackRequest.ForEvent(definition).Follow(target).WithScales(volumeScale, pitchScale));
        }

        public AudioPlaybackHandle Play(AudioPlaybackRequest request)
        {
            AudioEventDefinition definition = ResolveEvent(request);
            if (definition == null)
            {
                return default;
            }

            string eventId = definition.EventId;
            if (!CanPlay(definition, eventId))
            {
                return default;
            }

            AudioVoice limitedVoice = ResolveVoiceLimit(definition, eventId);
            if (limitedVoice == null && definition.MaxVoices > 0 && CountActiveVoices(eventId) >= definition.MaxVoices)
            {
                return default;
            }

            int sequenceIndex = sequenceIndices.TryGetValue(eventId, out int currentIndex) ? currentIndex : 0;
            AudioClipEntry clipEntry = definition.SelectClip(ref sequenceIndex);
            sequenceIndices[eventId] = sequenceIndex;

            if (clipEntry == null || clipEntry.Clip == null)
            {
                return default;
            }

            AudioVoice voice = limitedVoice != null ? limitedVoice : GetVoiceFor(definition);
            if (voice == null)
            {
                return default;
            }

            if (definition.PlaybackMode == AudioEventPlaybackMode.Music && currentMusicVoice != null && currentMusicVoice != voice)
            {
                currentMusicVoice.Stop(definition.FadeOutSeconds);
            }

            Vector3 position = ResolvePlaybackPosition(definition, request);
            voice.Play(definition, clipEntry, position, request.FollowTarget, Mathf.Max(0f, request.VolumeScale), request.PitchScale <= 0f ? 1f : request.PitchScale);
            nextAllowedPlayTimes[eventId] = Time.time + definition.CooldownSeconds;

            if (definition.PlaybackMode == AudioEventPlaybackMode.Music)
            {
                currentMusicVoice = voice;
            }

            return new AudioPlaybackHandle(this, voice.VoiceId);
        }

        public void StopAll(float fadeOutSeconds = 0f)
        {
            for (int i = 0; i < voices.Count; i++)
            {
                voices[i].Stop(fadeOutSeconds);
            }

            currentMusicVoice = null;
        }

        public void StopEvent(string eventId, float fadeOutSeconds = -1f)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            for (int i = 0; i < voices.Count; i++)
            {
                AudioVoice voice = voices[i];
                if (voice.IsReserved && voice.CurrentEvent != null && voice.CurrentEvent.EventId == eventId)
                {
                    voice.Stop(ResolveFadeOut(voice.CurrentEvent, fadeOutSeconds));
                }
            }
        }

        public void SetMixerParameter(AudioMixer mixer, string exposedParameter, float value, AudioParameterValueMode mode = AudioParameterValueMode.Raw)
        {
            if (mixer == null)
            {
                mixer = masterMixer;
            }

            if (mixer == null || string.IsNullOrWhiteSpace(exposedParameter))
            {
                return;
            }

            mixer.SetFloat(exposedParameter, ConvertParameterValue(value, mode));
        }

        public bool SetConfiguredVolume(string label, float linearVolume)
        {
            if (config == null || string.IsNullOrWhiteSpace(label))
            {
                return false;
            }

            IReadOnlyList<AudioMixerVolumeBinding> bindings = config.VolumeBindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                AudioMixerVolumeBinding binding = bindings[i];
                if (binding == null || binding.Label != label)
                {
                    continue;
                }

                SetMixerParameter(binding.Mixer, binding.ExposedParameter, Mathf.Clamp01(linearVolume), AudioParameterValueMode.LinearVolumeToDecibel);
                return true;
            }

            return false;
        }

        public void ApplyMixerParameters(IReadOnlyList<AudioMixerParameterValue> values)
        {
            if (values == null)
            {
                return;
            }

            for (int i = 0; i < values.Count; i++)
            {
                AudioMixerParameterValue value = values[i];
                if (value != null)
                {
                    SetMixerParameter(value.Mixer, value.ExposedParameter, value.Value, value.ValueMode);
                }
            }
        }

        public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float transitionSeconds)
        {
            if (snapshot != null)
            {
                snapshot.TransitionTo(Mathf.Max(0f, transitionSeconds));
            }
        }

        internal bool IsHandleValid(int voiceId)
        {
            return FindVoice(voiceId) != null;
        }

        internal void Stop(int voiceId, float fadeOutSeconds)
        {
            AudioVoice voice = FindVoice(voiceId);
            if (voice == null)
            {
                return;
            }

            voice.Stop(ResolveFadeOut(voice.CurrentEvent, fadeOutSeconds));
            if (voice == currentMusicVoice)
            {
                currentMusicVoice = null;
            }
        }

        internal void SetVoiceVolume(int voiceId, float linearVolume)
        {
            AudioVoice voice = FindVoice(voiceId);
            if (voice != null)
            {
                voice.SetVolume(linearVolume);
            }
        }

        private AudioEventDefinition ResolveEvent(AudioPlaybackRequest request)
        {
            if (request.EventDefinition != null)
            {
                return request.EventDefinition;
            }

            if (eventLibrary != null && eventLibrary.TryGetEvent(request.EventId, out AudioEventDefinition definition))
            {
                return definition;
            }

            Debug.LogWarning($"Audio event not found: {request.EventId}", this);
            return null;
        }

        private bool CanPlay(AudioEventDefinition definition, string eventId)
        {
            if (definition.CooldownSeconds <= 0f)
            {
                return true;
            }

            return !nextAllowedPlayTimes.TryGetValue(eventId, out float nextAllowed) || Time.time >= nextAllowed;
        }

        private AudioVoice ResolveVoiceLimit(AudioEventDefinition definition, string eventId)
        {
            if (definition.MaxVoices <= 0 || CountActiveVoices(eventId) < definition.MaxVoices)
            {
                return null;
            }

            if (definition.VoiceLimitBehavior == AudioVoiceLimitBehavior.RejectNew)
            {
                return null;
            }

            AudioVoice oldest = null;
            for (int i = 0; i < voices.Count; i++)
            {
                AudioVoice voice = voices[i];
                if (!voice.IsReserved || voice.CurrentEvent == null || voice.CurrentEvent.EventId != eventId)
                {
                    continue;
                }

                if (oldest == null || voice.StartedAt < oldest.StartedAt)
                {
                    oldest = voice;
                }
            }

            oldest?.Release();
            return oldest;
        }

        private int CountActiveVoices(string eventId)
        {
            int count = 0;
            for (int i = 0; i < voices.Count; i++)
            {
                AudioVoice voice = voices[i];
                if (voice.IsReserved && voice.CurrentEvent != null && voice.CurrentEvent.EventId == eventId)
                {
                    count++;
                }
            }

            return count;
        }

        private AudioVoice GetVoiceFor(AudioEventDefinition definition)
        {
            for (int i = 0; i < voices.Count; i++)
            {
                if (voices[i].IsAvailable)
                {
                    return voices[i];
                }
            }

            if (allowPoolGrowth && voices.Count < maxPoolSize)
            {
                return CreateVoice();
            }

            AudioVoice stealCandidate = null;
            for (int i = 0; i < voices.Count; i++)
            {
                AudioVoice voice = voices[i];
                if (!voice.IsReserved || voice.CurrentEvent == null)
                {
                    continue;
                }

                bool lowerOrEqualPriority = voice.CurrentEvent.Priority >= definition.Priority;
                if (lowerOrEqualPriority && (stealCandidate == null || voice.StartedAt < stealCandidate.StartedAt))
                {
                    stealCandidate = voice;
                }
            }

            if (stealCandidate != null)
            {
                stealCandidate.Release();
            }

            return stealCandidate;
        }

        private Vector3 ResolvePlaybackPosition(AudioEventDefinition definition, AudioPlaybackRequest request)
        {
            if (request.FollowTarget != null)
            {
                return request.FollowTarget.position;
            }

            if (request.HasWorldPosition)
            {
                return request.WorldPosition;
            }

            if (definition.SpatialMode == AudioEventSpatialMode.ListenerRelative3D)
            {
                Transform listener = ResolveListenerTransform();
                if (listener != null)
                {
                    return listener.position;
                }
            }

            Transform logicReference = ResolveLogicReferenceTransform();
            if (logicReference != null)
            {
                return logicReference.position;
            }

            Transform fallbackListener = ResolveListenerTransform();
            return fallbackListener != null ? fallbackListener.position : transform.position;
        }

        private Transform ResolveListenerTransform()
        {
            if (listenerTransform != null)
            {
                return listenerTransform;
            }

            if (!useMainCameraFallback)
            {
                return null;
            }

            Camera mainCamera = Camera.main;
            return mainCamera != null ? mainCamera.transform : null;
        }

        private Transform ResolveLogicReferenceTransform()
        {
            if (referenceMode == AudioEngineReferenceMode.CameraOnly)
            {
                return ResolveListenerTransform();
            }

            if (logicReferenceTransform != null)
            {
                return logicReferenceTransform;
            }

            return ResolveListenerTransform();
        }

        private AudioVoice FindVoice(int voiceId)
        {
            if (voiceId == 0)
            {
                return null;
            }

            for (int i = 0; i < voices.Count; i++)
            {
                if (voices[i].VoiceId == voiceId && voices[i].IsReserved)
                {
                    return voices[i];
                }
            }

            return null;
        }

        private void ApplyDefaultVolumeBindings()
        {
            if (config == null)
            {
                return;
            }

            IReadOnlyList<AudioMixerVolumeBinding> bindings = config.VolumeBindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                AudioMixerVolumeBinding binding = bindings[i];
                if (binding != null)
                {
                    SetMixerParameter(binding.Mixer, binding.ExposedParameter, binding.DefaultLinearVolume, AudioParameterValueMode.LinearVolumeToDecibel);
                }
            }
        }

        private void BuildPool(int count)
        {
            while (voices.Count < count)
            {
                CreateVoice();
            }
        }

        private AudioVoice CreateVoice()
        {
            GameObject voiceObject = new GameObject($"AudioVoice_{voices.Count:00}");
            voiceObject.transform.SetParent(transform, false);
            AudioVoice voice = voiceObject.AddComponent<AudioVoice>();
            voice.Prepare(nextVoiceId++);
            voices.Add(voice);
            return voice;
        }

        private static float ResolveFadeOut(AudioEventDefinition definition, float overrideSeconds)
        {
            if (overrideSeconds >= 0f)
            {
                return overrideSeconds;
            }

            return definition != null ? definition.FadeOutSeconds : 0f;
        }

        private static float ConvertParameterValue(float value, AudioParameterValueMode mode)
        {
            switch (mode)
            {
                case AudioParameterValueMode.LinearVolumeToDecibel:
                    return value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
                case AudioParameterValueMode.Decibel:
                    return value;
                default:
                    return value;
            }
        }
    }
}
