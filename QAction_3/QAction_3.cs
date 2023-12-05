using System;

using Skyline.DataMiner.Scripting;

using Skyline.PollingManager;
using Skyline.PollingManager.Enums;

public static class QAction
{
	public static void Run(SLProtocol protocol)
	{
		try
		{
			// Get trigger row key.
			Trigger trigger = (Trigger)protocol.GetTriggerParameter();
			string rowId = protocol.RowKey();

			// Updates row with specific key that was triggered by specific column.
			PollingManagerContainer
				.GetManager(protocol, 1)
				.HandleRowUpdate(rowId, trigger.ToColumn());
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Polling Manager Update|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}
