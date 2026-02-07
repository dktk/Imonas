using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace System
{
    public class EnumExtensions<T>
    {
        private static string[] _enumNames = Enum.GetNames(typeof(T));
        private static T[] _enums = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        private static Dictionary<string, T> _mapping =
            _enumNames
                .Select((name, i) => new { name, i })
                .ToDictionary(c => c.name, c => _enums[c.i]);


        public T Get(string value, string enumTypeName)
        {
            if (_enumNames.Contains(value))
            {
                return _mapping[value];
            }

            throw new Exception($"Unable to map '{value}' to {enumTypeName} enum.");
        }
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enu)
        {
            var attr = GetDisplayAttribute(enu);
            return attr?.Name ?? enu.ToString();
        }

        public static string GetDescription(this Enum enu)
        {
            var attr = GetDisplayAttribute(enu);
            return attr?.Description ?? enu.ToString();
        }

        public static IEnumerable<KeyValuePair<int, string>> Values<TEnum>() where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
            {
                throw new ArgumentException($"Type {type} is not an enum");
            }

            return Enum.GetNames(type).Select(name => new KeyValuePair<int, string>((int)Enum.Parse(type, name), name)).ToList();
        }

        public static IEnumerable<KeyValuePair<int, string>> DisplayValues<TEnum>() where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"Type {typeof(TEnum)} is not an enum");
            }

            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<Enum>();

            return enumValues
                .Select(enumValue => new KeyValuePair<int, string>(Convert.ToInt32(enumValue), enumValue.GetDisplayName()))
                .ToList();
        }

        public static TEnum GetEnumValue<TEnum>(this string displayName) where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList().FirstOrDefault(e => e.GetDisplayName() == displayName);
        }

        private static DisplayAttribute GetDisplayAttribute(object value)
        {
            // todo: this should be cached
            var type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"Type {type} is not an enum");
            }

            var field = type.GetField(value.ToString() ?? string.Empty);
            return field == null ? null : field.GetCustomAttribute<DisplayAttribute>();
        }
    }
}
