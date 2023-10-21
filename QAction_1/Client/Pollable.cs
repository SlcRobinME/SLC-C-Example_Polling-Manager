namespace Skyline.PollingManager.Client
{
	using Skyline.DataMiner.Scripting;

	using Skyline.PollingManager.Interfaces;

	public class Pollable : PollableBase
	{
		public Pollable(SLProtocol protocol, string name) : base(protocol, name)
		{
		}

		public Pollable(SLProtocol protocol, object[] row) : base(protocol, row)
		{
		}

		public override bool Poll()
		{
			Protocol.Log($"Name [{Name}]");

			return true;
		}
	}
}
