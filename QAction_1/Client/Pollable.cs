namespace Skyline.PollingManager.Client
{
	using Skyline.DataMiner.Scripting;
	using Skyline.PollingManager.Pollable;

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
