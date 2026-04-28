using System.Text;

namespace CCModTool.Logging;

public static class EncodingHelper
{
	/// <summary>
	/// Custom version of <see cref="Encoding.UTF8"/> that DOESN'T do 80Ms.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Encoding UTF8 = new UTF8Encoding();
}