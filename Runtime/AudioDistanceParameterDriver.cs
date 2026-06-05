using UnityEngine;
using UnityEngine.Audio;

namespace BitemDev.AudioEngine
{
    public sealed class AudioDistanceParameterDriver : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string exposedParameter;
        [SerializeField] private AudioParameterValueMode valueMode = AudioParameterValueMode.Raw;
        [SerializeField, Min(0.01f)] private float maxDistance = 20f;
        [SerializeField] private bool invert = true;
        [SerializeField, Min(0f)] private float updateInterval = 0.1f;

        private float nextUpdateTime;

        private void Reset()
        {
            target = transform;
        }

        private void Update()
        {
            if (Time.time < nextUpdateTime)
            {
                return;
            }

            nextUpdateTime = Time.time + updateInterval;
            UpdateParameter();
        }

        public void UpdateParameter()
        {
            if (AudioManager.Instance == null || string.IsNullOrWhiteSpace(exposedParameter))
            {
                return;
            }

            Transform reference = AudioManager.Instance.LogicReferenceTransform;
            Transform measuredTarget = target != null ? target : transform;
            if (reference == null || measuredTarget == null)
            {
                return;
            }

            float normalized = Mathf.Clamp01(Vector3.Distance(reference.position, measuredTarget.position) / maxDistance);
            float value = invert ? 1f - normalized : normalized;
            AudioManager.Instance.SetMixerParameter(mixer, exposedParameter, value, valueMode);
        }
    }
}
