namespace CCModTool.Abstractions.Analyzers;

/// <summary>
/// Specify that this class is allowed to be inherited.
/// </summary>
/// <remarks>
///	This is to prevent accidental inheritance of non-sealed classes.
/// Classes must be marked [Virtual], abstract, or sealed.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class VirtualAttribute : Attribute
{
}