namespace SG.Common;

public static class ListExtensions
{
    private const int BatchSize = 1000;

    public static IEnumerable<T> ApplyFilters<T>(this List<T> source, IEnumerable<IListFilter<T>> filters)
    {
        foreach (var filter in filters)
        {
            source = filter.Apply(source).ToList();
        }

        return source;
    }

    public static IEnumerable<List<T>> Batch<T>(this List<T> source)
    {
        for (int i = 0; i < source.Count; i += BatchSize)
        {
            yield return source.GetRange(i, Math.Min(BatchSize, source.Count - i));
        }
    }
}