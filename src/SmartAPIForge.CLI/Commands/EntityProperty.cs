namespace SmartAPIForge.CLI.Commands;

internal readonly record struct EntityProperty(string Name, string ClrType)
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.Ordinal)
    {
        "string", "int", "long", "decimal", "double", "bool", "DateTime", "Guid"
    };

    public bool IsNullable => ClrType.EndsWith('?');

    public static IReadOnlyList<EntityProperty> ParseAll(string spec)
    {
        var properties = new List<EntityProperty>();
        foreach (var part in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pieces = part.Split(':', 2);
            if (pieces.Length != 2)
            {
                throw new ArgumentException($"Malformed property '{part}'. Expected Name:type, e.g. \"Price:decimal\".");
            }

            var name = pieces[0].Trim();
            var type = pieces[1].Trim();
            var baseType = type.TrimEnd('?');

            if (!AllowedTypes.Contains(baseType))
            {
                throw new ArgumentException(
                    $"Unsupported type '{baseType}' for property '{name}'. Supported: {string.Join(", ", AllowedTypes)}.");
            }

            properties.Add(new EntityProperty(name, type));
        }

        if (properties.Count == 0)
        {
            throw new ArgumentException("--properties must list at least one Name:type pair.");
        }

        return properties;
    }
}
