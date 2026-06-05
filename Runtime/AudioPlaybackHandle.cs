namespace BitemDev.AudioEngine
{
    public readonly struct AudioPlaybackHandle
    {
        private readonly AudioManager manager;
        private readonly int voiceId;

        public bool IsValid => manager != null && manager.IsHandleValid(voiceId);

        internal AudioPlaybackHandle(AudioManager manager, int voiceId)
        {
            this.manager = manager;
            this.voiceId = voiceId;
        }

        public void Stop(float fadeOutSeconds = -1f)
        {
            if (manager != null)
            {
                manager.Stop(voiceId, fadeOutSeconds);
            }
        }

        public void SetVolume(float linearVolume)
        {
            if (manager != null)
            {
                manager.SetVoiceVolume(voiceId, linearVolume);
            }
        }
    }
}
