using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CCModTool.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingConfigPropertyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new (
        Diagnostics.IdMissingConfigPropertyAttribute,
        "Missing ConfigProp attribute",
        "Property '{0}' must have [ConfigProp] attribute",
        "Usage",
        DiagnosticSeverity.Error,
        true);
    
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var property = (IPropertySymbol)context.Symbol;

        if (property.Name == "SchemaVersion") return;
        if (property.Name == "Priority") return;

        var containingType = property.ContainingType;
        if (containingType is null) return;

        var isConfig = containingType.AllInterfaces.Any(i => i.Name == "IConfig");
        if(!isConfig) return;
        
        var hasConfigProp = property
            .GetAttributes()
            .Any(a => a.AttributeClass?.Name == "ConfigPropAttribute");

        if (!hasConfigProp)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, property.Locations[0], property.Name));
            return;
        }
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];
}

// Just doing this here out of laziness.
[Generator]
public sealed class ConfigGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var properties = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is PropertyDeclarationSyntax p &&
                    p.AttributeLists.Count > 0,

                transform: static (ctx, _) =>
                {
                    if (ctx.Node is not PropertyDeclarationSyntax prop)
                        return null;

                    return ctx.SemanticModel.GetDeclaredSymbol(prop) as IPropertySymbol;
                })
            .Where(static p => p is not null)!;

        context.RegisterSourceOutput(properties.Collect(), Execute);
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<IPropertySymbol?> symbols)
    {
        var grouped = symbols
            .Where(p => p is not null)
            .Cast<IPropertySymbol>()
            .Where(IsConfigProp)
            .GroupBy(p => p.ContainingType, SymbolEqualityComparer.Default);

        foreach (var group in grouped)
        {
            var type = group.Key;
            var ns = type.ContainingNamespace.ToDisplayString();
            var className = type.Name;

            var assignments = new StringBuilder();
            var schema = new StringBuilder();

            foreach (var prop in group.OrderBy(p => p.Name))
            {
                var attr = prop.GetAttributes()
                    .First(a => a.AttributeClass?.Name == "ConfigPropAttribute");

                var named = attr.NamedArguments;

                var hasHashDefault = named.Any(x =>
                    x.Key == "HashDefault" &&
                    x.Value.Value is bool b &&
                    b);

                // Get default value - now can be any type
                var defaultArg = named.FirstOrDefault(x => x.Key == "Default");
                object? defaultValue = null;
                
                if (defaultArg.Value.Value != null)
                {
                    defaultValue = defaultArg.Value.Value;
                }

                // Format the value for code generation
                var formattedValue = FormatDefaultValue(defaultValue, prop.Type, out var isEnumValue);
                
                // IMPORTANT RULE:
                // Only include default in schema if HashDefault = true
                var schemaDefaultPart = hasHashDefault
                    ? (formattedValue != null ? formattedValue : GetDefaultValueString(prop.Type))
                    : ""; // ignored in hash

                assignments.AppendLine(
                    $"            {prop.Name} = {formattedValue ?? GetDefaultValueString(prop.Type)};");

                schema.Append(prop.Name)
                    .Append(':')
                    .Append(prop.Type.ToDisplayString())
                    .Append(':')
                    .Append(schemaDefaultPart) // conditional inclusion
                    .AppendLine();
            }

            var hash = StableHash(schema.ToString());

            var source = $$"""
namespace {{ns}}
{
    public partial class {{className}}
    {
        public {{className}}()
        {
            SchemaVersion = {{hash}};
{{assignments}}
        }
    }
}
""";

            context.AddSource($"{className}.Config.g.cs", source);
        }
    }

    private static bool IsConfigProp(IPropertySymbol prop)
    {
        if (!prop.ContainingType.AllInterfaces.Any(i => i.Name == "IConfig"))
            return false;

        return prop.GetAttributes()
            .Any(a => a.AttributeClass?.Name == "ConfigPropAttribute");
    }

    private static string? FormatDefaultValue(object? value, ITypeSymbol type, out bool isEnumValue)
    {
        isEnumValue = false;
        
        if (value == null)
            return null;

        // Handle enum types
        if (type.TypeKind == TypeKind.Enum)
        {
            isEnumValue = true;
            var enumType = (INamedTypeSymbol)type;
            var enumName = enumType.ToDisplayString();
            
            // If value is already the enum member name string
            if (value is string enumMemberName)
            {
                return $"{enumName}.{enumMemberName}";
            }
            
            // If value is the numeric value
            if (value is int intValue)
            {
                var member = enumType.GetMembers()
                    .OfType<IFieldSymbol>()
                    .FirstOrDefault(f => f.HasConstantValue && Equals(f.ConstantValue, intValue));
                
                if (member != null)
                    return $"{enumName}.{member.Name}";
                else
                    return $"({enumName}){intValue}";
            }
        }

        // Handle other types
        return type.SpecialType switch
        {
            SpecialType.System_String when value is string str => $"\"{Escape(str)}\"",
            SpecialType.System_Boolean => value.ToString()?.ToLowerInvariant(),
            SpecialType.System_Int32 => value.ToString(),
            SpecialType.System_Int64 => value.ToString(),
            SpecialType.System_Double => value.ToString()?.Replace(",", "."),
            SpecialType.System_Single => value.ToString()?.Replace(",", "."),
            _ when type.Name == "Version" && value is string version => $"Version.Parse(\"{Escape(version)}\")",
            _ => value.ToString()
        };
    }

    private static string GetDefaultValueString(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Enum)
        {
            var enumType = (INamedTypeSymbol)type;
            var members = enumType.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue).ToList();
            
            if (members.Any())
            {
                var firstMember = members.First();
                return $"{enumType.ToDisplayString()}.{firstMember.Name}";
            }
            
            return $"default({enumType.ToDisplayString()})";
        }
        
        return type.SpecialType switch
        {
            SpecialType.System_String => "\"\"",
            SpecialType.System_Int32 => "0",
            SpecialType.System_Int64 => "0",
            SpecialType.System_Boolean => "false",
            SpecialType.System_Double => "0",
            SpecialType.System_Single => "0",
            _ when type.Name == "Version" => "Version.Parse(\"0.0.0\")",
            _ => "default!"
        };
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    private static long StableHash(string text)
    {
        using var sha = SHA256.Create();
        var data = Encoding.UTF8.GetBytes(text);
        var bytes = sha.ComputeHash(data);
        return Math.Abs(BitConverter.ToInt64(bytes, 0));
    }
}