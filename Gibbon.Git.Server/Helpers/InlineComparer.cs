namespace Gibbon.Git.Server.Helpers;

public sealed class InlineComparer<T> : IEqualityComparer<T>
{
    public static InlineComparer<T> Create(Func<T, T, bool> equals, Func<T, int> hashCode) => new(equals, hashCode);

    private readonly Func<T, T, bool> _getEquals;
    private readonly Func<T, int> _getHashCode;

    private InlineComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
    {
        _getEquals = equals;
        _getHashCode = hashCode;
    }

    public bool Equals(T x, T y) => _getEquals(x, y);

    public int GetHashCode(T obj) => _getHashCode(obj);
}