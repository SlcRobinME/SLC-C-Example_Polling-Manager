namespace Skyline.PollingManager.Client
{
	using Skyline.DataMiner.Scripting;

	using Skyline.PollingManager.Interfaces;
	using Skyline.PollingManager.Providers;

	public class Pollable : PollableBase
	{
		public Pollable(string name) : base(name)
		{
		}

		public Pollable(object[] row) : base(row)
		{
		}

		public override bool Poll()
		{
			SLProtocolProvider.Protocol.Log($"Name [{Name}]");

			return true;
		}
	}
}
