using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#nullable enable

namespace CCModTool.Generators;

[Generator]
public sealed class ConfigGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var properties = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is PropertyDeclarationSyntax { AttributeLists.Count: > 0 },

                transform: static (ctx, _) =>
                {
                    if (ctx.Node is not PropertyDeclarationSyntax prop)
                        return null;

                    return ctx.SemanticModel.GetDeclaredSymbol(prop) as IPropertySymbol;
                })
            .Where(static p => p is not null);

        context.RegisterSourceOutput(properties.Collect(), Execute!);
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<IPropertySymbol> symbols)
    {
        var grouped = symbols
	        .Where(p => p is not null)
            .Where(IsConfigProp)
            .GroupBy(p => p.ContainingType, SymbolEqualityComparer.Default);

        foreach (var group in grouped)
        {
            var type = group.Key;
            var ns = type?.ContainingNamespace.ToDisplayString();
            var className = type?.Name;

            var assignments = new StringBuilder();
            var schema = new StringBuilder();

            foreach (var prop in group.OrderBy(p => p.Name))
            {
                var attr = prop.GetAttributes()
                    .First(a => a.AttributeClass?.Name == "ConfigPropAttribute");

                var named = attr.NamedArguments;

                var hasHashDefault = named.Any(x =>
                    x is { Key: "HashDefault", Value.Value: true });

                var defaultArg = named.FirstOrDefault(x => x.Key == "Default");
                object? defaultValue = null;
                
                if (defaultArg.Value.Value != null)
                {
                    defaultValue = defaultArg.Value.Value;
                }

                var formattedValue = FormatDefaultValue(defaultValue, prop.Type);
                
                var schemaDefaultPart = hasHashDefault
                    ? formattedValue ?? GetDefaultValueString(prop.Type)
                    : "";

                assignments.AppendLine(
                    $"            {prop.Name} = {formattedValue ?? GetDefaultValueString(prop.Type)};");

                schema.Append(prop.Name)
                    .Append(':')
                    .Append(prop.Type.ToDisplayString())
                    .Append(':')
                    .Append(schemaDefaultPart)
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

    private static string? FormatDefaultValue(object? value, ITypeSymbol type)
    {
	    if (value == null)
            return null;

        if (type.TypeKind != TypeKind.Enum)
	        return type.SpecialType switch
	        {
		        SpecialType.System_String when value is string str => $"\"{Escape(str)}\"",
		        SpecialType.System_Boolean => value.ToString()?.ToLowerInvariant(),
		        SpecialType.System_Int32 => value.ToString(),
		        SpecialType.System_Int64 => value.ToString(),
		        SpecialType.System_Double or SpecialType.System_Single => value.ToString()?.Replace(",", "."),
		        _ when type.Name == "Version" && value is string version => $"Version.Parse(\"{Escape(version)}\")",
		        _ => value.ToString()
	        };

        var enumType = (INamedTypeSymbol)type;
        var enumName = enumType.ToDisplayString();
            
        switch (value)
        {
	        case string enumMemberName:
		        return $"{enumName}.{enumMemberName}";
	        case int intValue:
	        {
		        var member = enumType.GetMembers()
			        .OfType<IFieldSymbol>()
			        .FirstOrDefault(f => f.HasConstantValue && Equals(f.ConstantValue, intValue));

		        return member != null ? $"{enumName}.{member.Name}" : $"({enumName}){intValue}";
	        }
        }

        return value.ToString(); // Fallback and pray
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