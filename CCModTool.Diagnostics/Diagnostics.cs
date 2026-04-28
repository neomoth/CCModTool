using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

#nullable enable

namespace CCModTool.Diagnostics;

public static class Diagnostics
{
	public static readonly HashSet<DiagnosticDescriptor> Rules = [
		,
		,
		,
		,
		
	];

	public static DiagnosticDescriptor Get(string id) =>
		Rules.First(r => r.Id == id);
}