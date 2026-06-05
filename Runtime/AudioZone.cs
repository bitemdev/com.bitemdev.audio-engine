using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BitemDev.AudioEngine
{
    [DisallowMultipleComponent]
    public sealed class AudioZone : MonoBehaviour
    {
        [SerializeField] private LayerMask triggerLayers = ~0;
        [SerializeField] private string requiredTag;
        [SerializeField] private AudioEventDefinition enterEvent;
        [SerializeField] private AudioEventDefinition exitEvent;
        [SerializeField] private AudioMixerSnapshot enterSnapshot;
        [SerializeField] private AudioMixerSnapshot exitSnapshot;
        [SerializeField, Min(0f)] private float snapshotTransitionSeconds = 0.5f;
        [SerializeField] private List<AudioMixerParameterValue> enterParameters = new List<AudioMixerParameterValue>();
        [SerializeField] private List<AudioMixerParameterValue> exitParameters = new List<AudioMixerParameterValue>();

        private int occupants;

        private void OnTriggerEnter(Collider other)
        {
            if (!Matches(other.gameObject))
            {
                return;
            }

            occupants++;
            if (occupants == 1)
            {
                ApplyEnter();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Matches(other.gameObject))
            {
                return;
            }

            occupants = Mathf.Max(0, occupants - 1);
            if (occupants == 0)
            {
                ApplyExit();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!Matches(other.gameObject))
            {
                return;
            }

            occupants++;
            if (occupants == 1)
            {
                ApplyEnter();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!Matches(other.gameObject))
            {
                return;
            }

            occupants = Mathf.Max(0, occupants - 1);
            if (occupants == 0)
            {
                ApplyExit();
            }
        }

        public void ApplyEnter()
        {
            if (AudioManager.Instance == null)
            {
                return;
            }

            if (enterEvent != null)
            {
                AudioManager.Instance.Play(enterEvent);
            }

            AudioManager.Instance.TransitionToSnapshot(enterSnapshot, snapshotTransitionSeconds);
            AudioManager.Instance.ApplyMixerParameters(enterParameters);
        }

        public void ApplyExit()
        {
            if (AudioManager.Instance == null)
            {
                return;
            }

            if (exitEvent != null)
            {
                AudioManager.Instance.Play(exitEvent);
            }

            AudioManager.Instance.TransitionToSnapshot(exitSnapshot, snapshotTransitionSeconds);
            AudioManager.Instance.ApplyMixerParameters(exitParameters);
        }

        private bool Matches(GameObject candidate)
        {
            bool layerMatches = (triggerLayers.value & (1 << candidate.layer)) != 0;
            if (!layerMatches)
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(requiredTag) || candidate.CompareTag(requiredTag);
        }
    }
}
