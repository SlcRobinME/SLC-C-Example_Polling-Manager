namespace Skyline.PollingManager.Interfaces
{
	public interface IPollableBaseFactory
	{
		PollableBase CreatePollableBase(object[] row);
	}
}
