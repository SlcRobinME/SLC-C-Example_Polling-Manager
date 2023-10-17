namespace Skyline.PollingManager.Client
{
	using Skyline.PollingManager.Interfaces;

	public class PollableFactory : IPollableBaseFactory
	{
		public PollableBase CreatePollableBase(object[] row)
		{
			return new Pollable(row);
		}
	}
}
