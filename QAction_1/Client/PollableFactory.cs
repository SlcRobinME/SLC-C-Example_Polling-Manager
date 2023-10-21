namespace Skyline.PollingManager.Client
{
	using Skyline.DataMiner.Scripting;
	using Skyline.PollingManager.Interfaces;

	public class PollableFactory : IPollableBaseFactory
	{
		public PollableBase CreatePollableBase(SLProtocol protocol, object[] row)
		{
			return new Pollable(protocol, row);
		}
	}
}
