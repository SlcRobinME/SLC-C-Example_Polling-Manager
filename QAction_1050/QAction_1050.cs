using System;

using Skyline.DataMiner.PollingManager;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: Polling Manager - Sets.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			// Get trigger row key.
			Trigger trigger = (Trigger)protocol.GetTriggerParameter();
			string rowId = protocol.RowKey();

			// Updates row with specific key that was triggered by specific column.
			PollingManagerContainer
				.GetManager(protocol, initTrigger: 1)
				.HandleRowUpdate(rowId, trigger.ToColumn());
		}
		catch (Exception ex)
		{
			protocol.Log(
				$"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Polling Manager - Sets|Exception thrown:{Environment.NewLine}{ex}",
				LogType.Error,
				LogLevel.NoLogging);
		}
	}
}