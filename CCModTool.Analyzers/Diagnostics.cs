using Microsoft.CodeAnalysis;

namespace CCModTool.Analyzers;

public static class Diagnostics
{
    public const string IdDependencyFieldAssigned = "CC0001";
    public const string IdDuplicateDependency = "CC0001";
    public const string IdMissingConfigPropertyAttribute = "CC0002";
    public const string IdPathPropertyMustBeString = "CC0003";

    public static SuppressionDescriptor MeansImplicitAssignment =>
        new SuppressionDescriptor("RADC1000", "CS0649", "Marked as implicitly assigned.");
}