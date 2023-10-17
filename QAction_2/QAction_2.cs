using System;
using System.Collections.Generic;

using Skyline.DataMiner.Scripting;
using Skyline.PollingManager;
using Skyline.PollingManager.Client;
using Skyline.PollingManager.Controllers;
using Skyline.PollingManager.Interfaces;
using Skyline.PollingManager.Providers;

public static class QAction
{
    public static void PollingManagerInit(SLProtocolExt protocol)
    {
        try
        {
            protocol.Log("QAction_2.PollingManagerInit");

            var a = new SLProtocolProvider(protocol);

            var factory = new PollableFactory();

            PollingManager.Init(protocol.pollingmanager, PollingManagerConfiguration.Rows, factory);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerInit|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }

    public static void PollingManagerCheck(SLProtocolExt protocol)
	{
		try
		{
			protocol.Log("QAction_2.PollingManagerCheck");

			PollingManager.Instance.CheckForUpdate();
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerCheck|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}
