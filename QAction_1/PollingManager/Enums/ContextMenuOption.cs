namespace Skyline.PollingManager.Enums
{
    using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents context menu options of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
    public enum ContextMenuOption
    {
        PollAll = 1,
        Disable = 2,
        Enable = 3,
        ForceDisable = 4,
        ForceEnable = 5,
        DisableSelected = 6,
        EnableSelected = 7,
        DisableAll = 8,
        EnableAll = 9,
    }
}
