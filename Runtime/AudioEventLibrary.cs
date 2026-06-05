using System.Collections.Generic;
using UnityEngine;

namespace BitemDev.AudioEngine
{
    [CreateAssetMenu(fileName = "AudioEventLibrary", menuName = "BitemDev/Audio Engine/Audio Event Library", order = 11)]
    public sealed class AudioEventLibrary : ScriptableObject
    {
        [SerializeField] private List<AudioEventDefinition> events = new List<AudioEventDefinition>();

        private Dictionary<string, AudioEventDefinition> cache;

        public IReadOnlyList<AudioEventDefinition> Events => events;

        public bool TryGetEvent(string eventId, out AudioEventDefinition definition)
        {
            RebuildCacheIfNeeded();
            if (string.IsNullOrWhiteSpace(eventId))
            {
                definition = null;
                return false;
            }

            return cache.TryGetValue(eventId, out definition);
        }

        public void RebuildCache()
        {
            if (cache == null)
            {
                cache = new Dictionary<string, AudioEventDefinition>();
            }
            else
            {
                cache.Clear();
            }

            for (int i = 0; i < events.Count; i++)
            {
                AudioEventDefinition definition = events[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.EventId))
                {
                    continue;
                }

                if (!cache.ContainsKey(definition.EventId))
                {
                    cache.Add(definition.EventId, definition);
                }
            }
        }

        private void RebuildCacheIfNeeded()
        {
            if (cache == null)
            {
                RebuildCache();
            }
        }

        private void OnValidate()
        {
            cache = null;
        }
    }
}
