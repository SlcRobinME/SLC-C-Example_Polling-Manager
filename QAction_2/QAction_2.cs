using System;

using Skyline.DataMiner.Scripting;

using Skyline.PollingManager;
using Skyline.PollingManager.Client;
using Skyline.PollingManager.Providers;

public static class QAction
{
	/// <summary>
	/// Runs after stratup in order to add/initialize polling manager.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void PollingManagerInit(SLProtocolExt protocol)
    {
        try
        {
			// Used by PollingManagerConfiguration, not related directly to PollingManager
			SLProtocolProvider.Protocol = protocol;

			// Instance of class that implements IPollableFactory, defined by user.
			var factory = new PollableFactory();

			// Creates PollingManager instance and adds it to PollingManagerContainer.
			PollingManagerContainer.AddManager(protocol, protocol.pollingmanager, PollingManagerConfiguration.Rows, factory);
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
			protocol.Log("QAction_2.PollingManagerCheck");

			// Checks PollingManager table for rows that need to be polled.
			PollingManagerContainer.GetManager(protocol).CheckForUpdate();
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerCheck|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}
