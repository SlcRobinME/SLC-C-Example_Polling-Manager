namespace Skyline.PollingManager.Interfaces
{
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Factory for <see cref="PollableBase"/> object.
	/// </summary>
	public interface IPollableBaseFactory
	{
		PollableBase CreatePollableBase(SLProtocol protocol, object[] row);
	}
}
