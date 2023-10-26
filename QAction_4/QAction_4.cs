using System;

using Skyline.DataMiner.Scripting;

using Skyline.PollingManager;

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
            PollingManagerContainer.GetManager(protocol).HandleContextMenu(contextMenu);
		}
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Polling Manager Context Menu|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}
