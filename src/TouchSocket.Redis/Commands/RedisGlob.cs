namespace TouchSocket.Redis;

internal static class RedisGlob
{
    public static bool IsMatch(string text, string pattern)
    {
        return IsMatch(text, 0, pattern, 0);
    }

    private static bool IsMatch(string text, int textIndex, string pattern, int patternIndex)
    {
        while (patternIndex < pattern.Length)
        {
            var p = pattern[patternIndex];
            if (p == '*')
            {
                patternIndex++;
                if (patternIndex == pattern.Length)
                {
                    return true;
                }

                while (textIndex <= text.Length)
                {
                    if (IsMatch(text, textIndex, pattern, patternIndex))
                    {
                        return true;
                    }
                    textIndex++;
                }

                return false;
            }

            if (textIndex >= text.Length)
            {
                return false;
            }

            if (p != '?' && p != text[textIndex])
            {
                return false;
            }

            textIndex++;
            patternIndex++;
        }

        return textIndex == text.Length;
    }
}
