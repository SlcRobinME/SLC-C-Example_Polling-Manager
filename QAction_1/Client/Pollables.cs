namespace Skyline.PollingManager.Client
{
	using System;

	using Skyline.DataMiner.Scripting;

	using Skyline.PollingManager.Enums;
	using Skyline.PollingManager.Pollable;

	public class PollableA : PollableBase
	{
		private static int _seed = 0;

		public PollableA(SLProtocol protocol, string name) : base(protocol, name)
		{
			PeriodType = PeriodType.Custom;
		}

		public PollableA(SLProtocol protocol, object[] row) : base(protocol, row)
		{
		}

		public override bool Poll()
		{
			var random = new Random(_seed++);
			int randomNumber = random.Next(1, 101);

			bool poll = false;

			if (randomNumber < 80)
				poll = true;

			if (poll)
			{
				Protocol.Log($"$$$$$ Logging from [{nameof(PollableA)}] the group [{Name}] $$$$$");
				return poll;
			}
			else
			{
				Reason = $"Poll failed!";
				return poll;
			}
		}
	}

	public class PollableB : PollableBase
	{
		private static int _seed = 999999999;

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
			var random = new Random(_seed++);
			int randomNumber = random.Next(1, 101);

			bool poll = false;

			if (randomNumber < 80)
				poll = true;

			if (poll)
			{
				Protocol.Log($"##### Logging from [{nameof(PollableB)}] the group [{Name}] #####");
				return poll;
			}
			else
			{
				Reason = $"Poll failed!";
				return poll;
			}
		}
	}

	public class PollableC : PollableBase
	{
		private static int _seed = 123123123;

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
			var random = new Random(_seed++);
			int randomNumber = random.Next(1, 101);

			bool poll = false;

			if (randomNumber < 80)
				poll = true;

			if (poll)
			{
				Protocol.Log($"@@@@@ Logging from [{nameof(PollableC)}] the group [{Name}] @@@@@");
				return poll;
			}
			else
			{
				Reason = $"Poll failed!";
				return poll;
			}
		}
	}
}
