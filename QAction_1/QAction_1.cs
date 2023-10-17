namespace Skyline.PollingManager
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.PollingManager.Client;
	using Skyline.PollingManager.Interfaces;

	public static class PollingManagerConfiguration
	{
		private static readonly Dictionary<string, PollableBase> _rows = new Dictionary<string, PollableBase>()
		{
			{ "Simon", new Pollable("Simon") },
			{ "Jelle", new Pollable("Jelle") },
			{ "Edib", new Pollable("Edib") },
		};

		static PollingManagerConfiguration()
		{
			SetDependencies();
		}

		public static List<PollableBase> Rows => _rows.Select(row => row.Value).ToList();

		private static void SetDependencies()
		{
			_rows["Simon"].AddChildren(_rows["Edib"], _rows["Jelle"]);
		}
	}
}
