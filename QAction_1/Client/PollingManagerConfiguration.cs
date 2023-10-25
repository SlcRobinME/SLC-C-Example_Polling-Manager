namespace Skyline.PollingManager.Client
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.PollingManager.Interfaces;
	using Skyline.PollingManager.Providers;
	using Skyline.PollingManager.Structs;

	public static class PollingManagerConfiguration
	{
		private static readonly Dictionary<string, PollableBase> _rows = new Dictionary<string, PollableBase>()
		{
			{ "Simon", new Pollable(SLProtocolProvider.Protocol, "Simon") },
			{ "Jelle", new Pollable(SLProtocolProvider.Protocol, "Jelle") },
			{ "Edib", new Pollable(SLProtocolProvider.Protocol, "Edib") },
			{ "Ben", new Pollable(SLProtocolProvider.Protocol, "Ben") },
			{ "Bert", new Pollable(SLProtocolProvider.Protocol, "Bert") },
			{ "Robbie", new Pollable(SLProtocolProvider.Protocol, "Robbie") },
			{ "Merima", new Pollable(SLProtocolProvider.Protocol, "Merima") },
			{ "Ibrahim", new Pollable(SLProtocolProvider.Protocol, "Ibrahim") },
			{ "Thomas", new Pollable(SLProtocolProvider.Protocol, "Thomas") },
			{ "Victor", new Pollable(SLProtocolProvider.Protocol, "Victor") },
		};

		private static readonly List<Dependency> _dependencies = new List<Dependency>()
		{
			new Dependency(13.0, true, "Value not 13"),
			new Dependency("SDF", true, "Value not SDF"),
			new Dependency(0.0, false, "Value is 0"),
		};

		static PollingManagerConfiguration()
		{
			SetRelations();
			SetDependencies();
		}

		public static List<PollableBase> Rows => _rows.Select(row => row.Value).ToList();

		private static void SetRelations()
		{
			_rows["Simon"].AddChildren(_rows["Edib"], _rows["Jelle"], _rows["Thomas"]);
			_rows["Jelle"].AddChildren(_rows["Edib"], _rows["Ibrahim"]);
			_rows["Ben"].AddChildren(_rows["Bert"], _rows["Simon"]);
		}

		private static void SetDependencies()
		{
			//_rows["Simon"].Dependencies = new Dictionary<int, Dependency>()
			//{
			//	{ 10,  _dependencies[0] },
			//	{ 12, _dependencies[1] },
			//	{ 14, _dependencies[2] },
			//};
		}
	}
}
