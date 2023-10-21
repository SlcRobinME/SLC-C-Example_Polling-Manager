namespace Skyline.PollingManager.Interfaces
{
	using Skyline.DataMiner.Scripting;

	public interface IPollableBaseFactory
	{
		PollableBase CreatePollableBase(SLProtocol protocol, object[] row);
	}
}
