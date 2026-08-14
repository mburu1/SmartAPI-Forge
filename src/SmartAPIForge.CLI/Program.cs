using SmartAPIForge.CLI.Commands;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    PrintUsage();
    return 0;
}

return args switch
{
    ["new", "entity", ..] => NewEntityCommand.Run(args[2..]),
    _ => Unknown(args)
};

static int Unknown(string[] args)
{
    Console.Error.WriteLine($"Unknown command: {string.Join(' ', args)}");
    PrintUsage();
    return 1;
}

static void PrintUsage()
{
    Console.WriteLine("""
        SmartAPI Forge CLI — scaffolds code into a SmartAPI Forge-layout .NET project.

        Usage:
          smartapiforge new entity --name <Name> --properties "Prop1:type,Prop2:type" [--project-root <path>]

        Example:
          smartapiforge new entity --name Product --properties "Name:string,Price:decimal,InStock:bool"

        Generates:
          src/SmartAPIForge.Domain/Entities/<Name>.cs
          src/SmartAPIForge.Application/<Name>s/Dtos/{<Name>Dto,Create<Name>Request,Update<Name>Request}.cs
          src/SmartAPIForge.Api/Controllers/<Name>sController.cs (EF Core-backed CRUD)
          + adds a DbSet<<Name>> to AppDbContext

        Supported property types: string, int, long, decimal, double, bool, DateTime, Guid
        (append '?' for nullable, e.g. "Notes:string?").

        Options:
          --project-root <path>   Repo root containing src/SmartAPIForge.*  (default: current directory)
        """);
}
