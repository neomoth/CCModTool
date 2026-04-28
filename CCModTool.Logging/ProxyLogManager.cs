namespace CCModTool.Logging;

public sealed class ProxyLogManager : ILogManager
{
	private readonly ILogManager _impl;

	public ProxyLogManager(ILogManager impl) =>
		_impl = impl;

	public ISawmill RootSawmill => _impl.RootSawmill;

	public ISawmill GetSawmill(string name) =>
		_impl.GetSawmill(name);

	public IEnumerable<ISawmill> AllSawmills => _impl.AllSawmills;
}