namespace Skyline.DataMiner.PollingManager
{
    using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents columns of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
    public enum Column
    {
        Id = 0,
        Name = 1,
        Interval = 2,
        DefaultInterval = 3,
        IntervalType = 4,
        LastPoll = 5,
        Status = 6,
        Reason = 7,
        Poll = 8,
        State = 9,
    }
}
