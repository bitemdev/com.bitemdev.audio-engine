using System;
using UnityEngine;
using UnityEngine.Audio;

namespace BitemDev.AudioEngine
{
    public enum AudioEngineReferenceMode
    {
        CameraOnly = 0,
        SeparateListenerAndLogicReference = 1,
        Explicit = 2
    }

    public enum AudioEventCategory
    {
        Sfx = 0,
        Music = 1,
        Ambience = 2,
        Dialogue = 3,
        Ui = 4
    }

    public enum AudioEventBus
    {
        Auto = 0,
        Sfx = 1,
        Music = 2,
        Ambience = 3,
        Dialogue = 4,
        Ui = 5,
        Master = 6,
        Custom = 7
    }

    public enum AudioEventPlaybackMode
    {
        OneShot = 0,
        Loop = 1,
        Music = 2
    }

    public enum AudioEventSpatialMode
    {
        TwoD = 0,
        World3D = 1,
        ListenerRelative3D = 2
    }

    public enum AudioClipSelectionMode
    {
        Random = 0,
        Sequential = 1
    }

    public enum AudioVoiceLimitBehavior
    {
        RejectNew = 0,
        StealOldest = 1
    }

    public enum AudioParameterValueMode
    {
        Raw = 0,
        LinearVolumeToDecibel = 1,
        Decibel = 2
    }

    [Serializable]
    public sealed class AudioClipEntry
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Min(0f)] private float weight = 1f;
        [SerializeField, Range(0f, 2f)] private float volume = 1f;
        [SerializeField, Range(0.01f, 3f)] private float pitch = 1f;

        public AudioClip Clip => clip;
        public float Weight => weight;
        public float Volume => volume;
        public float Pitch => pitch;

        internal void EnsurePlayableDefaults()
        {
            if (weight <= 0f)
            {
                weight = 1f;
            }

            if (volume <= 0f)
            {
                volume = 1f;
            }

            if (pitch <= 0f)
            {
                pitch = 1f;
            }
        }
    }

    [Serializable]
    public sealed class AudioMixerParameterValue
    {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string exposedParameter;
        [SerializeField] private AudioParameterValueMode valueMode;
        [SerializeField] private float value = 1f;

        public AudioMixer Mixer => mixer;
        public string ExposedParameter => exposedParameter;
        public AudioParameterValueMode ValueMode => valueMode;
        public float Value => value;
    }

    [Serializable]
    public sealed class AudioMixerVolumeBinding
    {
        [SerializeField] private string label = "Master";
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string exposedParameter = "MasterVolume";
        [SerializeField, Range(0f, 1f)] private float defaultLinearVolume = 1f;

        public string Label => label;
        public AudioMixer Mixer => mixer;
        public string ExposedParameter => exposedParameter;
        public float DefaultLinearVolume => defaultLinearVolume;
    }

    [Serializable]
    public sealed class AudioMixerBusBinding
    {
        [SerializeField] private AudioEventBus bus = AudioEventBus.Sfx;
        [SerializeField] private AudioMixerGroup mixerGroup;

        public AudioEventBus Bus => bus;
        public AudioMixerGroup MixerGroup => mixerGroup;
    }
}
