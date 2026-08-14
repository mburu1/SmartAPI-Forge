namespace SmartAPIForge.CLI.Commands;

/// <summary>Minimal `--flag value` parser — no external dependency, just enough for this tool's handful of options.</summary>
internal static class ArgParser
{
    public static Dictionary<string, string> ParseOptions(ReadOnlySpan<string> args)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var key = args[i][2..];
            var value = i + 1 < args.Length ? args[i + 1] : throw new ArgumentException($"Missing value for --{key}");
            options[key] = value;
            i++;
        }

        return options;
    }
}
