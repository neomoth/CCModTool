namespace CCModTool.Logging;

public interface ILogManager
{
	/// The "root" sawmill every other sawmill is parented to.
	ISawmill RootSawmill { get; }

	/// <summary>
	/// Gets the sawmill with the specified name in order to better describe where the log message originates.
	/// Will create a new <see cref="ISawmill"/> if one with the given name does not exist.
	/// </summary>
	/// <param name="name">The desired name of the sawmill you wish to fetch.</param>
	/// <returns>An instance of <see cref="ISawmill"/></returns>
	ISawmill GetSawmill(string name);
	
	/// Gets a list of all existing sawmills as an <see cref="IEnumerable{ISawmill}"/>.
	IEnumerable<ISawmill> AllSawmills { get; }
}