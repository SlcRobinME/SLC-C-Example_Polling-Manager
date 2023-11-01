using System;

using Skyline.DataMiner.Scripting;

using Skyline.PollingManager;
using Skyline.PollingManager.Client;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocol protocol, object contextMenu)
    {
        try
        {
            // Handles ContextMenu.
            PollingManagerContainer.GetManager(protocol, (int)Trigger.Init).HandleContextMenu(contextMenu);
		}
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Polling Manager Context Menu|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}
