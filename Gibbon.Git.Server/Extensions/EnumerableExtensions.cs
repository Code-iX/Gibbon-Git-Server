namespace Gibbon.Git.Server.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        return ascending ? source.OrderBy(selector) : source.OrderByDescending(selector);
    }
}
