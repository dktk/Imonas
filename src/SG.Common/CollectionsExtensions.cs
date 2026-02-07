namespace SG.Common
{
    public static class CollectionsExtensions
    {
        public static (IEnumerable<T>, IEnumerable<T>) Split<T>(this IEnumerable<T> source, Func<T, bool> filter)
        {
            var oks = new List<T>();
            var notOks = new List<T>();

            foreach (var item in source)
            {
                var isOk = filter(item);

                if (isOk)
                {
                    oks.Add(item);
                }
                else
                {
                    notOks.Add(item);
                }
            }

            return (oks, notOks);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
    }
}