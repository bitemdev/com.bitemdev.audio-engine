using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace BitemDev.AudioEngine
{
    [DisallowMultipleComponent]
    public sealed class AudioVoice : MonoBehaviour
    {
        private AudioSource source;
        private AudioEventDefinition currentEvent;
        private Transform followTarget;
        private Coroutine fadeRoutine;
        private float expectedStartTime;
        private bool stopIfFollowTargetDestroyed;
        private bool reserved;
        private bool stopping;

        internal int VoiceId { get; private set; }
        internal float StartedAt { get; private set; }
        internal AudioEventDefinition CurrentEvent => currentEvent;
        internal bool IsReserved => reserved;

        internal bool IsFinished
        {
            get
            {
                return reserved &&
                    !stopping &&
                    source != null &&
                    !source.loop &&
                    !source.isPlaying &&
                    Time.time >= expectedStartTime + 0.05f;
            }
        }

        internal bool IsAvailable => !reserved;

        internal void Prepare(int id)
        {
            VoiceId = id;
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = false;
            source.clip = null;
            gameObject.SetActive(false);
        }

        internal void Play(
            AudioEventDefinition definition,
            AudioClipEntry clipEntry,
            AudioMixerGroup outputMixerGroup,
            Vector3 position,
            Transform target,
            float volumeScale,
            float pitchScale)
        {
            if (definition == null || clipEntry == null || clipEntry.Clip == null)
            {
                Release();
                return;
            }

            if (source == null)
            {
                Prepare(VoiceId);
            }

            gameObject.SetActive(true);
            reserved = true;
            stopping = false;
            currentEvent = definition;
            followTarget = target;
            transform.position = target != null ? target.position : position;
            StartedAt = Time.time;
            expectedStartTime = Time.time + definition.Delay;
            stopIfFollowTargetDestroyed = target != null && definition.StopWhenFollowTargetIsDestroyed;

            source.Stop();
            source.clip = clipEntry.Clip;
            source.outputAudioMixerGroup = outputMixerGroup;
            source.loop = definition.IsLooping;
            source.priority = definition.Priority;
            source.ignoreListenerPause = definition.IgnoreListenerPause;
            source.spatialBlend = definition.SpatialMode == AudioEventSpatialMode.TwoD ? 0f : definition.SpatialBlend;
            source.minDistance = definition.MinDistance;
            source.maxDistance = definition.MaxDistance;
            source.rolloffMode = definition.RolloffMode;
            source.dopplerLevel = definition.DopplerLevel;
            source.spread = definition.Spread;
            source.reverbZoneMix = definition.ReverbZoneMix;
            source.spatialize = definition.Spatialize;

            float volumeJitter = definition.RandomVolume > 0f ? Random.Range(-definition.RandomVolume, definition.RandomVolume) : 0f;
            float pitchJitter = definition.RandomPitch > 0f ? Random.Range(-definition.RandomPitch, definition.RandomPitch) : 0f;
            float targetVolume = Mathf.Max(0f, definition.Volume * clipEntry.Volume * Mathf.Max(0f, volumeScale) + volumeJitter);
            source.volume = definition.FadeInSeconds > 0f ? 0f : targetVolume;
            source.pitch = Mathf.Max(0.01f, definition.Pitch * clipEntry.Pitch * Mathf.Max(0.01f, pitchScale) + pitchJitter);

            if (definition.Delay > 0f)
            {
                source.PlayDelayed(definition.Delay);
            }
            else
            {
                source.Play();
            }

            if (definition.FadeInSeconds > 0f)
            {
                if (definition.Delay > 0f)
                {
                    FadeInAfterDelay(targetVolume, definition.FadeInSeconds, definition.Delay);
                }
                else
                {
                    FadeTo(targetVolume, definition.FadeInSeconds, false);
                }
            }
        }

        internal void Stop(float fadeOutSeconds)
        {
            if (!reserved)
            {
                return;
            }

            if (fadeOutSeconds > 0f && source != null && source.isPlaying)
            {
                FadeTo(0f, fadeOutSeconds, true);
                return;
            }

            Release();
        }

        internal void SetVolume(float linearVolume)
        {
            if (source != null && fadeRoutine == null)
            {
                source.volume = Mathf.Max(0f, linearVolume);
            }
        }

        internal void Release()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            if (source != null)
            {
                source.Stop();
                source.clip = null;
                source.loop = false;
            }

            currentEvent = null;
            followTarget = null;
            stopIfFollowTargetDestroyed = false;
            reserved = false;
            stopping = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!reserved)
            {
                return;
            }

            if (followTarget != null)
            {
                transform.position = followTarget.position;
            }
            else if (stopIfFollowTargetDestroyed)
            {
                Release();
            }
        }

        private void FadeTo(float targetVolume, float duration, bool releaseWhenDone)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeCoroutine(targetVolume, duration, releaseWhenDone));
        }

        private void FadeInAfterDelay(float targetVolume, float duration, float delay)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(DelayedFadeCoroutine(targetVolume, duration, delay));
        }

        private IEnumerator DelayedFadeCoroutine(float targetVolume, float duration, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            yield return FadeCoroutine(targetVolume, duration, false);
        }

        private IEnumerator FadeCoroutine(float targetVolume, float duration, bool releaseWhenDone)
        {
            stopping = releaseWhenDone;
            float startVolume = source != null ? source.volume : 0f;
            float elapsed = 0f;

            while (elapsed < duration && source != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            if (source != null)
            {
                source.volume = targetVolume;
            }

            fadeRoutine = null;
            if (releaseWhenDone)
            {
                Release();
            }
            else
            {
                stopping = false;
            }
        }
    }
}
