using System.Text;

namespace SG.Common.Security
{
    public class PasswordGenerator
    {
        private const string AlphaChars = "abcdefghijklmnopqrstuvwxyz";
        private const string NonAlphaNumericChars = "-!@$^&*";
        private const string NumericChars = "0123456789";
        private const int DefaultPasswordLength = 16;
        private const int DefaultNumberOfNumericChars = 2;
        private const int DefaultNumberOfSpecialChars = 2;

        public static string GeneratePassword(int passwordLength = DefaultPasswordLength, bool includeUppercase = true,
            int numberOfNumericChars = DefaultNumberOfNumericChars, int numberOfSpecialChars = DefaultNumberOfSpecialChars)
        {
            var characterPool = new StringBuilder(AlphaChars);

            if (includeUppercase)
            {
                characterPool.Append(AlphaChars.ToUpper());
            }

            var rng = new Random();
            var passwordBuilder = new StringBuilder();

            AppendRandomCharacters(passwordBuilder, NonAlphaNumericChars, numberOfSpecialChars, rng);
            AppendRandomCharacters(passwordBuilder, NumericChars, numberOfNumericChars, rng);
            AppendRandomCharacters(passwordBuilder, characterPool.ToString(), passwordLength - numberOfSpecialChars - numberOfNumericChars, rng);

            return Shuffle(passwordBuilder.ToString(), rng);
        }

        private static void AppendRandomCharacters(StringBuilder builder, string charactersSource, int length, Random random)
        {
            for (int i = 0; i < length; i++)
            {
                builder.Append(charactersSource[random.Next(charactersSource.Length)]);
            }
        }

        private static string Shuffle(string input, Random random)
        {
            char[] array = input.ToCharArray();
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            return new string(array);
        }
    }
}
