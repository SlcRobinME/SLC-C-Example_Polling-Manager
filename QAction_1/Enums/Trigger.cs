namespace Skyline.PollingManager.Enums
{
	using System;

	public enum Trigger
	{
		Period = 1103,
		PeriodType = 1105,
		Poll = 1108,
		State = 1109,
	}

	public static class TriggerExtensions
	{
		public static Column ToColumn(this Trigger trigger)
		{
			switch (trigger)
			{
				case Trigger.Period:
					return Column.Period;

				case Trigger.PeriodType:
					return Column.PeriodType;

				case Trigger.Poll:
					return Column.Poll;

				case Trigger.State:
					return Column.State;

				default:
					throw new InvalidOperationException($"Unhandled PeriodType: {trigger}");
			}
		}
	}
}
