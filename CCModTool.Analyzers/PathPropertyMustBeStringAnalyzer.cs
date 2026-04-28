using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CCModTool.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PathPropUsageAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new(
		"CC0003",
		"Invalid use of PathProp",
		"[PathProp] can only be applied to string properties with [ConfigProp]",
		"Usage",
		DiagnosticSeverity.Error,
		true
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
	}

	private static void AnalyzeProperty(SymbolAnalysisContext context)
	{
		var property = (IPropertySymbol)context.Symbol;

		// Must have [PathProp]
		var hasPathProp = property.GetAttributes()
			.Any(a => a.AttributeClass?.Name == "PathProp");

		if (!hasPathProp)
			return;

		// Must have [ConfigProp]
		var hasConfigProp = property.GetAttributes()
			.Any(a => a.AttributeClass?.Name == "ConfigPropAttribute");

		// Must be string
		var isString = property.Type.SpecialType == SpecialType.System_String;

		if (!hasConfigProp || !isString)
		{
			var diagnostic = Diagnostic.Create(
				Rule,
				property.Locations[0],
				property.Name);

			context.ReportDiagnostic(diagnostic);
		}
	}
}