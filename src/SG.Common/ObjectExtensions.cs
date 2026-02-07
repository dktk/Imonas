using System.Reflection;
using System.Text.Json.Serialization;
using System.Web;
using SG.Common;

namespace System
{
    public static class ObjectExtensions
    {
        public static Result<TResult> CreateFailure<TResult>(this string error) => Result<TResult>.BuildFailure(error);
        public static Result<TResult> CreateSuccess<TResult>(this TResult value) => Result<TResult>.BuildSuccess(value);
        
        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found on object of type '{obj.GetType().Name}'");
            }

            return (T)property.GetValue(obj);
        }

        public static string ToQueryParams<T>(this T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var queryParams = new List<string>();

            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                if (value == null)
                    continue;

                var stringValue = value.ToString();
                if (string.IsNullOrWhiteSpace(stringValue))
                    continue;

                var attribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                var paramName = attribute?.Name ?? property.Name.ToLowerInvariant();
                queryParams.Add($"{HttpUtility.UrlEncode(paramName)}={HttpUtility.UrlEncode(stringValue)}");
            }

            return string.Join("&", queryParams);
        }

        public static string ToJson<T>(this T value) => System.Text.Json.JsonSerializer.Serialize(value, typeof(T));
    }
}
