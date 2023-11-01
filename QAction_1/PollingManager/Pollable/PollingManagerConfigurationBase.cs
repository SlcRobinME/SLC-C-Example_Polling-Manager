namespace Skyline.PollingManager.Pollable
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Scripting;
	using Skyline.PollingManager.Structs;

	public abstract class PollingManagerConfigurationBase
	{
		public PollingManagerConfigurationBase(SLProtocol protocol) => Protocol = protocol;

		public SLProtocol Protocol { get; set; }

		public List<PollableBase> ListRows => Rows.Select(row => row.Value).ToList();

		protected abstract Dictionary<string, PollableBase> Rows { get; set; }

		protected abstract List<Dependency> Dependencies { get; set; }

		public void Create()
		{
			CreateRelations();
			CreateDependencies();
		}

		protected abstract void CreateRelations();

		protected abstract void CreateDependencies();
	}
}
