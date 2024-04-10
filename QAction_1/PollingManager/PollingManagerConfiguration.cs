namespace Skyline.Protocol.PollingManager
{
	using System.Collections.Generic;

	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class PollingManagerConfiguration : PollingManagerConfigurationBase
	{
		public PollingManagerConfiguration(SLProtocol protocol) : base(protocol)
		{
			Rows = new Dictionary<string, PollableBase>()
			{
				// Parent of CEO, CFO, CTO
				// Child of -
				{ "Owner", new Pollable(Protocol, "Owner - A") },

				// Parent of CFO, CTO, Expert Hub Lead
				// Child of Owner
				{ "CEO", new Pollable(Protocol, "CEO - A") },

				// Parent of -
				// Child of Owner, CEO
				{ "CFO", new Pollable(Protocol, "CFO - B") },

				// Parent of Expert Hub Lead
				// Child of Owner, CEO
				{ "CTO", new Pollable(Protocol, "CTO - B") },

				// Parent of Principal 1, Principal 2, Senior 1
				// Child of CEO, CTO
				{ "Expert Hub Lead", new Pollable(Protocol, "Expert Hub Lead - B") },

				// Parent of Senior 2, Senior 3
				// Child of CTO, Expert Hub Lead
				{ "Principal 1", new Pollable(Protocol, "Principal 1 - B") },

				// Parent of Senior 1
				// Child of CTO, Expert Hub Lead
				{ "Principal 2", new Pollable(Protocol, "Principal 2 - C") },

				// Parent of -
				// Child of Expert Hub Lead, Principal 2
				{ "Senior 1", new Pollable(Protocol, "Senior 1 - A") },

				// Parent of -
				// Child of Principal 1
				{ "Senior 2", new Pollable(Protocol, "Senior 2 - B") },

				// Parent of -
				// Child of Principal 1
				{ "Senior 3", new Pollable(Protocol, "Senior 3 - C") },
			};

			Dependencies = new List<Dependency>()
			{
			};
		}

		protected override Dictionary<string, PollableBase> Rows { get; set; }

		protected override List<Dependency> Dependencies { get; set; }

		protected override void CreateDependencies()
		{
		}

		protected override void CreateRelations()
		{
			Rows["Owner"].AddChildren(Rows["CEO"], Rows["CFO"], Rows["CTO"]);

			Rows["CEO"].AddChildren(Rows["CFO"], Rows["CTO"], Rows["Expert Hub Lead"]);

			Rows["CTO"].AddChildren(Rows["Expert Hub Lead"]);

			Rows["Principal 1"].AddParents(Rows["CTO"], Rows["Expert Hub Lead"]);
			Rows["Principal 1"].AddChildren(Rows["Senior 2"], Rows["Senior 3"]);

			Rows["Principal 2"].AddParents(Rows["CTO"], Rows["Expert Hub Lead"]);
			Rows["Principal 2"].AddChildren(Rows["Senior 1"]);

			Rows["Senior 1"].AddParents(Rows["Expert Hub Lead"]);
		}
	}
}
