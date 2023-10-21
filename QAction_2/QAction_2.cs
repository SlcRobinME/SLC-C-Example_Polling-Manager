using System;

using Skyline.DataMiner.Scripting;
using Skyline.PollingManager;
using Skyline.PollingManager.Client;
using Skyline.PollingManager.Controllers;
using Skyline.PollingManager.Providers;

public static class QAction
{
    public static void PollingManagerInit(SLProtocolExt protocol)
    {
        try
        {
            protocol.Log("QAction_2.PollingManagerInit");

            SLProtocolProvider.Protocol = protocol;

            var factory = new PollableFactory();

            PollingManagerContainer.AddManager(protocol, protocol.pollingmanager, PollingManagerConfiguration.Rows, factory);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerInit|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }

    public static void PollingManagerCheck(SLProtocol protocol)
	{
		try
		{
			protocol.Log("QAction_2.PollingManagerCheck");

			PollingManagerContainer.GetManager(protocol).CheckForUpdate();
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerCheck|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}
