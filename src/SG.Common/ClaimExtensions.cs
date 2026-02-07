using System.Globalization;

namespace System.Security.Claims
{
    public static class ClaimExtensions
    {
        public static string GetValue(this IEnumerable<Claim> claims, string claimName, string defaultValue = null)
        {
            return claims.FirstOrDefault(x => x.Type == claimName)?.Value ?? defaultValue;
        }

        public static int GetValueAsInt(this IEnumerable<Claim> claims, string claimName)
        {
            var value = claims.GetValue(claimName);

            return int.TryParse(value, out var result) 
                ? result 
                : default;
        }

        public static DateTime GetValueAsDateTime(this IEnumerable<Claim> claims, string claimName)
        {
            var value = claims.GetValue(claimName);

            return DateTime.TryParse(value, CultureInfo.InvariantCulture.DateTimeFormat, out var result) 
                ? result 
                : default;
        }
    }
}
