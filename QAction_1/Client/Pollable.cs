namespace Skyline.DataMiner.Protocol.PollingManager.Client
{
	using Skyline.DataMiner.Protocol.PollingManager.Pollable;
	using Skyline.DataMiner.Scripting;

	public class Pollable : PollableBase
	{
		public Pollable(SLProtocol protocol, string name) : base(protocol, name)
		{
		}

		public override bool Poll()
		{
			Protocol.Log($"Polling [{Name}]");

			return true;
		}
	}
}
