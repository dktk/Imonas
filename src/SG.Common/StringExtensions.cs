using System.Collections.Concurrent;

namespace System
{
    public static class StringExtensions
    {
        public static int Hash(this string value) => Math.Abs(Hasher.Hash(value));

        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static bool Is(this string value) => !string.IsNullOrWhiteSpace(value);

        // todo: use spans for this
        public static string ToUpperFirstLetter(this string source)
        {
            if (source.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            char[] letters = source.ToCharArray();

            letters[0] = char.ToUpper(letters[0]);

            return new string(letters);
        }

        public static string ToLowerFirstLetter(this string source)
        {
            if (source.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            char[] letters = source.ToCharArray();

            letters[0] = char.ToLower(letters[0]);

            return new string(letters);
        }
    }
}