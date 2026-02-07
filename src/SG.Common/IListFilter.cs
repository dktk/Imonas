namespace SG.Common;

public interface IListFilter<T>
{
    IEnumerable<T> Apply(List<T> source);
}