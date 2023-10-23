namespace Skyline.PollingManager.Enums
{
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents states of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public enum State
    {
        Disabled = 1,
        Enabled = 2,
        ForceDisabled = 3,
        ForceEnabled = 4,
    }
}
