namespace SmartAPIForge.CLI.Commands;

internal static class NewEntityCommand
{
    public static int Run(string[] args)
    {
        try
        {
            var options = ArgParser.ParseOptions(args);

            if (!options.TryGetValue("name", out var name) || string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("--name is required, e.g. --name Product");
            }

            if (!options.TryGetValue("properties", out var propertiesSpec) || string.IsNullOrWhiteSpace(propertiesSpec))
            {
                throw new ArgumentException("--properties is required, e.g. --properties \"Name:string,Price:decimal\"");
            }

            var projectRoot = options.GetValueOrDefault("project-root", Directory.GetCurrentDirectory());
            var force = options.ContainsKey("force");
            var properties = EntityProperty.ParseAll(propertiesSpec);

            name = char.ToUpperInvariant(name[0]) + name[1..];

            var domainDir = Path.Combine(projectRoot, "src", "SmartAPIForge.Domain");
            var applicationDir = Path.Combine(projectRoot, "src", "SmartAPIForge.Application");
            var apiDir = Path.Combine(projectRoot, "src", "SmartAPIForge.Api");
            var dbContextPath = Path.Combine(projectRoot, "src", "SmartAPIForge.Infrastructure", "Persistence", "AppDbContext.cs");

            foreach (var required in new[] { domainDir, applicationDir, apiDir, dbContextPath })
            {
                if (!Path.Exists(required))
                {
                    throw new InvalidOperationException(
                        $"'{required}' not found. Run this from a SmartAPI Forge repo root, or pass --project-root.");
                }
            }

            var entityPath = Path.Combine(domainDir, "Entities", $"{name}.cs");
            var dtoDir = Path.Combine(applicationDir, $"{name}s", "Dtos");
            var controllerPath = Path.Combine(apiDir, "Controllers", $"{name}sController.cs");

            if (!force && File.Exists(entityPath))
            {
                throw new InvalidOperationException($"'{entityPath}' already exists. Pass --force to overwrite.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(entityPath)!);
            Directory.CreateDirectory(dtoDir);
            Directory.CreateDirectory(Path.GetDirectoryName(controllerPath)!);

            File.WriteAllText(entityPath, EntityTemplates.DomainEntity(name, properties));
            File.WriteAllText(Path.Combine(dtoDir, $"{name}Dto.cs"), EntityTemplates.Dto(name, properties));
            File.WriteAllText(Path.Combine(dtoDir, $"Create{name}Request.cs"), EntityTemplates.CreateRequest(name, properties));
            File.WriteAllText(Path.Combine(dtoDir, $"Update{name}Request.cs"), EntityTemplates.UpdateRequest(name, properties));
            File.WriteAllText(controllerPath, EntityTemplates.Controller(name, properties));

            PatchDbContext(dbContextPath, name);

            Console.WriteLine($"""
                Generated '{name}':
                  {entityPath}
                  {Path.Combine(dtoDir, $"{name}Dto.cs")}
                  {Path.Combine(dtoDir, $"Create{name}Request.cs")}
                  {Path.Combine(dtoDir, $"Update{name}Request.cs")}
                  {controllerPath}
                  (added DbSet<{name}> to AppDbContext)

                Next steps:
                  dotnet ef migrations add Add{name} --project src/SmartAPIForge.Infrastructure --startup-project src/SmartAPIForge.Api
                  dotnet ef database update --project src/SmartAPIForge.Infrastructure --startup-project src/SmartAPIForge.Api
                """);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static void PatchDbContext(string dbContextPath, string name)
    {
        const string anchor = "public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();";
        var content = File.ReadAllText(dbContextPath);

        var newLine = EntityTemplates.DbSetLine(name);
        if (content.Contains(newLine, StringComparison.Ordinal))
        {
            return;
        }

        var anchorIndex = content.IndexOf(anchor, StringComparison.Ordinal);
        if (anchorIndex < 0)
        {
            throw new InvalidOperationException(
                $"Could not find the RefreshTokens DbSet in '{dbContextPath}' to anchor the new DbSet next to. Add manually:\n{newLine}");
        }

        var insertAt = content.IndexOf('\n', anchorIndex) + 1;
        content = content.Insert(insertAt, newLine + Environment.NewLine);
        File.WriteAllText(dbContextPath, content);
    }
}
