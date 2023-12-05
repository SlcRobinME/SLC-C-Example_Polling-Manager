using System;

using Skyline.DataMiner.Protocol.PollingManager;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	/// <param name="contextMenu">The contextMenu data.</param>
	public static void Run(SLProtocol protocol, object contextMenu)
    {
        try
        {
            // Handles ContextMenu.
            PollingManagerContainer
				.GetManager(protocol, 1)
				.HandleContextMenu(contextMenu);
		}
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Polling Manager Context Menu|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}
