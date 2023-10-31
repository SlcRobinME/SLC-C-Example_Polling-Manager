namespace Skyline.PollingManager.Client
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Scripting;
	using Skyline.PollingManager.Pollable;
	using Skyline.PollingManager.Structs;

	public class PollingManagerConfiguration
	{
		private readonly Dictionary<string, PollableBase> _rows = new Dictionary<string, PollableBase>()
		{
			// Parent of CEO, CFO, CTO
			// Child of -
			{ "Owner", new Pollable(Protocol, "Owner") },

			// Parent of CFO, CTO, Expert Hub Lead
			// Child of Owner
			{ "CEO", new Pollable(Protocol, "CEO") },

			// Parent of -
			// Child of Owner, CEO
			{ "CFO", new Pollable(Protocol, "CFO") },

			// Parent of Expert Hub Lead
			// Child of Owner, CEO
			{ "CTO", new Pollable(Protocol, "CTO") },

			// Parent of Principal 1, Principal 2, Senior 1
			// Child of CEO, CTO
			{ "Expert Hub Lead", new Pollable(Protocol, "Expert Hub Lead") },

			// Parent of Senior 2, Senior 3
			// Child of CTO, Expert Hub Lead
			{ "Principal 1", new Pollable(Protocol, "Principal 1") },

			// Parent of Senior 1
			// Child of CTO, Expert Hub Lead
			{ "Principal 2", new Pollable(Protocol, "Principal 2") },

			// Parent of -
			// Child of Expert Hub Lead, Principal 2
			{ "Senior 1", new Pollable(Protocol, "Senior 1") },

			// Parent of -
			// Child of Principal 1
			{ "Senior 2", new Pollable(Protocol, "Senior 2") },

			// Parent of -
			// Child of Principal 1
			{ "Senior 3", new Pollable(Protocol, "Senior 3") },
		};

		private readonly List<Dependency> _dependencies = new List<Dependency>()
		{
		};

		public PollingManagerConfiguration(SLProtocol protocol)
		{
			Protocol = protocol;
			SetRelations();
			SetDependencies();
		}

		public static SLProtocol Protocol { get; set; }

		public List<PollableBase> Rows => _rows.Select(row => row.Value).ToList();

		private void SetRelations()
		{
			_rows["Owner"].AddChildren(_rows["CEO"], _rows["CFO"], _rows["CTO"]);

			_rows["CEO"].AddChildren(_rows["CFO"], _rows["CTO"], _rows["Expert Hub Lead"]);

			_rows["CTO"].AddChildren(_rows["Expert Hub Lead"]);

			_rows["Principal 1"].AddParents(_rows["CTO"], _rows["Expert Hub Lead"]);
			_rows["Principal 1"].AddChildren(_rows["Senior 2"], _rows["Senior 3"]);

			_rows["Principal 2"].AddParents(_rows["CTO"], _rows["Expert Hub Lead"]);
			_rows["Principal 2"].AddChildren(_rows["Senior 1"]);

			_rows["Senior 1"].AddParents(_rows["Expert Hub Lead"]);
		}

		private void SetDependencies()
		{
		}
	}
}
