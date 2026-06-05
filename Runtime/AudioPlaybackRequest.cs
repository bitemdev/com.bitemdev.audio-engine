using UnityEngine;

namespace BitemDev.AudioEngine
{
    public struct AudioPlaybackRequest
    {
        public AudioEventDefinition EventDefinition;
        public string EventId;
        public bool HasWorldPosition;
        public Vector3 WorldPosition;
        public Transform FollowTarget;
        public float VolumeScale;
        public float PitchScale;

        public static AudioPlaybackRequest ForEvent(AudioEventDefinition definition)
        {
            return new AudioPlaybackRequest
            {
                EventDefinition = definition,
                VolumeScale = 1f,
                PitchScale = 1f
            };
        }

        public static AudioPlaybackRequest ForEventId(string eventId)
        {
            return new AudioPlaybackRequest
            {
                EventId = eventId,
                VolumeScale = 1f,
                PitchScale = 1f
            };
        }

        public AudioPlaybackRequest At(Vector3 position)
        {
            HasWorldPosition = true;
            WorldPosition = position;
            return this;
        }

        public AudioPlaybackRequest Follow(Transform target)
        {
            FollowTarget = target;
            if (target != null)
            {
                HasWorldPosition = true;
                WorldPosition = target.position;
            }

            return this;
        }

        public AudioPlaybackRequest WithScales(float volumeScale, float pitchScale = 1f)
        {
            VolumeScale = volumeScale;
            PitchScale = pitchScale;
            return this;
        }
    }
}
