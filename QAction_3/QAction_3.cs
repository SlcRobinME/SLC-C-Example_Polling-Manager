using System;

using Skyline.DataMiner.Scripting;
using Skyline.PollingManager.Controllers;
using Skyline.PollingManager.Enums;

public static class QAction
{
	public static void Run(SLProtocolExt protocol)
	{
		try
		{
			protocol.Log("QAction_3.PollingManagerUpdateRow");

			Trigger trigger = (Trigger)protocol.GetTriggerParameter();
			string rowId = protocol.RowKey();

			PollingManagerContainer.GetManager(protocol).UpdateRow(rowId, trigger.ToColumn());
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerInit|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}
