namespace Skyline.DataMiner.Protocol.PollingManager.Enums
{
	using System;

	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents triggers of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public enum Trigger
    {
        Period = 1053,
        PeriodType = 1055,
        Poll = 1059,
    }

	/// <summary>
	/// Extension class for <see cref="Trigger"/>.
	/// </summary>
	public static class TriggerExtensions
    {
		/// <summary>
		/// Converts <see cref="Trigger"/> to its corresponding <see cref="Column"/> value.
		/// </summary>
		/// <param name="trigger">Trigger to be converted.</param>
		/// <returns>Column that corresponds to the trigger.</returns>
		/// <exception cref="InvalidOperationException">Throws if trigger has no corresponding column.</exception>
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

                default:
                    throw new ArgumentException($"Unsupported PeriodType '{trigger}'.");
            }
        }
    }
}
