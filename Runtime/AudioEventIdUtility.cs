using System.Text;

namespace BitemDev.AudioEngine
{
    public static class AudioEventIdUtility
    {
        public static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "audio.event";
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool previousSeparator = false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = char.ToLowerInvariant(value[i]);
                bool valid = char.IsLetterOrDigit(c);
                if (valid)
                {
                    builder.Append(c);
                    previousSeparator = false;
                }
                else if (!previousSeparator)
                {
                    builder.Append('.');
                    previousSeparator = true;
                }
            }

            return builder.ToString().Trim('.');
        }
    }
}
