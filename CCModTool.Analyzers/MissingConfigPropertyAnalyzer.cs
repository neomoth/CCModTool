using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CCModTool.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingConfigPropertyAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new(
		"CC0004",
		"Missing ConfigProp attribute",
		"Property '{0}' must have [ConfigProp] attribute",
		"Usage",
		DiagnosticSeverity.Error,
		true
	);
    
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var property = (IPropertySymbol)context.Symbol;

        // ignore metadata values from IConfig. TODO: maybe try doing this via reflection instead of specifying the names?
        switch (property.Name)
        {
	        case "SchemaVersion":
	        case "Priority":
		        return;
        }

        var containingType = property.ContainingType;
        if (containingType is null) return;

        var isConfig = containingType.AllInterfaces.Any(i => i.Name == "IConfig");
        if(!isConfig) return;
        
        var hasConfigProp = property
            .GetAttributes()
            .Any(a => a.AttributeClass?.Name == "ConfigPropAttribute");

        if (hasConfigProp) return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, property.Locations[0], property.Name));
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];
}