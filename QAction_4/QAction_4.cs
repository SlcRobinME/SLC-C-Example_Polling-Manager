using System;

using Skyline.DataMiner.Scripting;
using Skyline.PollingManager.Controllers;

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
            PollingManagerContainer.GetManager(protocol).HandleContextMenu((string[])contextMenu);
		}
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}
