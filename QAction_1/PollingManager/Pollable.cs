namespace Skyline.Protocol.PollingManager
{
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class Pollable : PollableBase
	{
		public Pollable(SLProtocol protocol, string name) : base(protocol, name)
		{
		}

		public override bool Poll()
		{
			Protocol.Log($"Polling '{Name}'.");

			return true;
		}
	}
}
