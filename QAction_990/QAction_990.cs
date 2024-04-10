using System;

using Skyline.DataMiner.PollingManager;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: Polling Manager - Process.
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
			PollingManagerContainer
				.GetManager(protocol, initTrigger: 1)
				.CheckForUpdate();
		}
		catch (Exception ex)
		{
			protocol.Log(
				$"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Polling Manager - Process|Exception thrown:{Environment.NewLine}{ex}",
				LogType.Error,
				LogLevel.NoLogging);
		}
	}
}