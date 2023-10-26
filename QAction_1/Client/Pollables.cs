namespace Skyline.PollingManager.Client
{
    using Skyline.DataMiner.Scripting;

    using Skyline.PollingManager.Enums;
    using Skyline.PollingManager.Pollable;

    public class PollableA : PollableBase
	{
		public PollableA(SLProtocol protocol, string name) : base(protocol, name)
		{
			PeriodType = PeriodType.Custom;
		}

		public PollableA(SLProtocol protocol, object[] row) : base(protocol, row)
		{
		}

		public override bool Poll()
		{
			Protocol.Log($"$$$$$ Logging from [{nameof(PollableA)}] the group [{Name}] $$$$$");

			return true;
		}
	}

    public class PollableB : PollableBase
	{
		public PollableB(SLProtocol protocol, string name, double? period = null) : base(protocol, name)
		{
			if (period != null)
			{
				Period = period.Value;
				PeriodType = PeriodType.Custom;
			}
		}

		public PollableB(SLProtocol protocol, object[] row) : base(protocol, row)
		{
		}

		public override bool Poll()
		{
			Protocol.Log($"##### Logging from [{nameof(PollableB)}] the group [{Name}] #####");

			return true;
		}
	}

    public class PollableC : PollableBase
	{
		public PollableC(SLProtocol protocol, string name) : base(protocol, name)
		{
		}

		public PollableC(SLProtocol protocol, string name, double period, double defaultPeriod, PeriodType periodType) : base(protocol, name)
		{
			Period = period;
			DefaultPeriod = defaultPeriod;
			PeriodType = periodType;
		}

		public PollableC(SLProtocol protocol, object[] row) : base(protocol, row)
		{
		}

		public override bool Poll()
		{
			Protocol.Log($"@@@@@ Logging from [{nameof(PollableC)}] the group [{Name}] @@@@@");

			return true;
		}
	}
}
