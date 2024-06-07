namespace Skyline.Protocol.PollingManager
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class Pollable : PollableBase
	{
		public Pollable(SLProtocol protocol, string name) : this(protocol, name, TimeSpan.FromSeconds(10))
		{
		}

		public Pollable(SLProtocol protocol, string name, TimeSpan interval) : base(protocol, name, interval)
		{
		}

		public override void Disable()
		{
			Protocol.ClearAllKeys(2000 /*example tablePid - REPLACE ME*/); // Use the ClearAllKeys to clear all related tables. Default behavior of a disabled polling item should be empty tables

			// Update all stand-alone parameters related to this poll item to an exception value. Default behavior of a disabled polling item should indicate that stand-alone parameters are not retrieved.
			var paramsToSet = new Dictionary<int, object>
			{
				{ 2000 /*example ParameterID - REPLACE ME*/, "-1" },
				{ 2001 /*example ParameterID - REPLACE ME*/, "-1" },
			};
			Protocol.SetParameters(paramsToSet.Keys.ToArray(), paramsToSet.Values.ToArray());
		}

		public override bool Poll()
		{
			Protocol.Log($"Polling '{Name}'.");

			return true;
		}
	}
}