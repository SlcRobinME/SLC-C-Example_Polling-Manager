using System;

using Skyline.DataMiner.Scripting;

using Skyline.PollingManager;
using Skyline.PollingManager.Client;

public static class QAction
{
	/// <summary>
	/// Runs after startup in order to add/initialize polling manager.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void PollingManagerInit(SLProtocol protocol)
    {
        try
        {
			var configuration = new PollingManagerConfiguration(protocol);

			// Creates PollingManager instance and adds it to PollingManagerContainer.
			PollingManagerContainer.AddManager(protocol, configuration);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerInit|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }

	/// <summary>
	/// Gets called by timer to check for necessary polls.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void PollingManagerCheck(SLProtocol protocol)
	{
		try
		{
			// Checks PollingManager table for rows that need to be polled.
			PollingManagerContainer.GetManager(protocol, 1).CheckForUpdate();
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerCheck|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}
