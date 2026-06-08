using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BitemDev.AudioEngine
{
    [CreateAssetMenu(fileName = "AudioEngineConfig", menuName = "BitemDev/Audio Engine/Audio Engine Config", order = 12)]
    public sealed class AudioEngineConfig : ScriptableObject
    {
        [SerializeField] private AudioEventLibrary eventLibrary;
        [SerializeField] private AudioMixer masterMixer;
        [SerializeField] private AudioEngineReferenceMode referenceMode = AudioEngineReferenceMode.SeparateListenerAndLogicReference;
        [SerializeField, Min(1)] private int initialPoolSize = 32;
        [SerializeField, Min(1)] private int maxPoolSize = 96;
        [SerializeField] private bool allowPoolGrowth = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool useMainCameraFallback = true;
        [SerializeField] private List<AudioMixerBusBinding> busBindings = new List<AudioMixerBusBinding>();
        [SerializeField] private List<AudioMixerVolumeBinding> volumeBindings = new List<AudioMixerVolumeBinding>();

        public AudioEventLibrary EventLibrary => eventLibrary;
        public AudioMixer MasterMixer => masterMixer;
        public AudioEngineReferenceMode ReferenceMode => referenceMode;
        public int InitialPoolSize => Mathf.Max(1, initialPoolSize);
        public int MaxPoolSize => Mathf.Max(InitialPoolSize, maxPoolSize);
        public bool AllowPoolGrowth => allowPoolGrowth;
        public new bool DontDestroyOnLoad => dontDestroyOnLoad;
        public bool UseMainCameraFallback => useMainCameraFallback;
        public IReadOnlyList<AudioMixerBusBinding> BusBindings => busBindings;
        public IReadOnlyList<AudioMixerVolumeBinding> VolumeBindings => volumeBindings;

        private void OnValidate()
        {
            if (maxPoolSize < initialPoolSize)
            {
                maxPoolSize = initialPoolSize;
            }
        }
    }
}
