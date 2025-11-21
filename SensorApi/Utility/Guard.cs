namespace SensorApi.Utility;

public class Guard
{
    public static T AgainstNull<T>(T parameter, string? name = null) where T : class
    {
        return parameter ?? throw new ArgumentNullException(name ?? "", $"guarded argument '{name ?? ""}' was null");
    }

    public static void AgainstNullOrEmpty<T>(IEnumerable<T> enumerable, string? name = null)
    {
        if (enumerable == null || !enumerable.Any())
        {
            throw new ArgumentException($"expected at least one element in '{name ?? ""}' but it was null or empty");
        }
    }

    public static void AgainstNullOrEmptyWithNullOrWhitespaceValues(IEnumerable<string> strings, string? name = null)
    {
        if (strings == null || !strings.Any())
        {
            throw new ArgumentException($"expected at least one element in '{name ?? ""}' but it was null or empty");
        }

        if (strings.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException($"'{name ?? ""}' contains null or whitespace values");
        }
    }
}
