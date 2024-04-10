namespace Skyline.DataMiner.PollingManager
{
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents statuses of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public enum Status
    {
        NotPolled = -2,
        Disabled = -1,
        Failed = 0,
        Succeeded = 1,
    }
}
