using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CCModTool.Diagnostics;

public static class Diagnostics
{
	public static readonly Dictionary<string, DiagnosticDescriptor> Rules = [];

	/// <summary>
	/// Define rules here, they will be propagated into <see cref="Rules"/> by the static constructor as <see cref="DiagnosticDescriptor"/>.
	/// It is done like this so they can be looked up easier by ID. 
	/// </summary>
	private static readonly HashSet<DiagnosticDescriptorDefinition> RuleDefinitions = [
		new(
			"CC0001",
			"Assignment to dependency field",
			"Tried to assign to [Dependency] field '{0}'. Remove [Dependency] or inject it via field injection instead.",
			"Usage",
			DiagnosticSeverity.Warning
		),
		new(
			"CC0002",
			"Duplicate dependency field",
			"Another [Dependency] field of type '{0}' already exists in this type with field '{1}'",
			"Usage",
			DiagnosticSeverity.Warning
		),
		new(
			"CC0003",
			"Invalid use of PathProp",
			"[PathProp] can only be applied to string properties with [ConfigProp]",
			"Usage",
			DiagnosticSeverity.Error
		),
		new(
			"CC0004",
			"Missing ConfigProp attribute",
			"Property '{0}' must have [ConfigProp] attribute",
			"Usage",
			DiagnosticSeverity.Error
		),
	];

	static Diagnostics()
	{
		foreach (var rule in RuleDefinitions)
			Rules.Add(rule.Id,
				new DiagnosticDescriptor(rule.Id, rule.Rule, rule.Description, rule.Category, rule.Severity,
					rule.EnabledByDefault));
	}
	
	private record DiagnosticDescriptorDefinition(
	    string Id,
	    string Rule,
	    string Description,
	    string Category,
	    DiagnosticSeverity Severity,
	    bool EnabledByDefault = true);
}