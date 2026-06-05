using UnityEngine;

namespace BitemDev.AudioEngine
{
    [DisallowMultipleComponent]
    public sealed class AudioReferenceRig : MonoBehaviour
    {
        [SerializeField] private AudioEngineReferenceMode referenceMode = AudioEngineReferenceMode.SeparateListenerAndLogicReference;
        [SerializeField] private Camera listenerCamera;
        [SerializeField] private Transform logicReference;
        [SerializeField] private bool ensureAudioListenerOnCamera = true;

        private void Reset()
        {
            listenerCamera = Camera.main;
        }

        private void OnEnable()
        {
            Register();
        }

        private void Start()
        {
            Register();
        }

        public void Register()
        {
            if (listenerCamera == null)
            {
                listenerCamera = Camera.main;
            }

            if (ensureAudioListenerOnCamera && listenerCamera != null && listenerCamera.GetComponent<AudioListener>() == null)
            {
                listenerCamera.gameObject.AddComponent<AudioListener>();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetReferences(
                    listenerCamera != null ? listenerCamera.transform : null,
                    logicReference,
                    referenceMode);
            }
        }
    }
}
