using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CCModTool.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExplicitVirtualAnalyzer : DiagnosticAnalyzer
{
	private const string Attribute = "CCModTool.Abstractions.Analyzers.VirtualAttribute";
	
	private static readonly DiagnosticDescriptor Rule = new(
		"CC0005",
		"Class must be explicitly marked as [Virtual], abstract, or sealed",
		"Class must be explicitly marked as [Virtual], abstract, or sealed",
		"Usage",
		DiagnosticSeverity.Error,
		true
	);
	
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];
	
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
	}

	private static bool HasAttribute(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol attrSymbol) =>
		namedTypeSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol));

	private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
	{
		var attrSymbol = context.Compilation.GetTypeByMetadataName(Attribute);
		var classDeclaration = (ClassDeclarationSyntax)context.Node;
		var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
		if (classSymbol is null)
			return;

		if (classSymbol.IsSealed || classSymbol.IsAbstract || classSymbol.IsStatic)
			return;
		if (HasAttribute(classSymbol, attrSymbol))
			return;
		
		context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Keyword.GetLocation()));
	}
}