namespace Skyline.DataMiner.PollingManager
{
	using System;

	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents triggers of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public enum Trigger
    {
        Interval = 1053,
        IntervalType = 1055,
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
                case Trigger.Interval:
                    return Column.Interval;

                case Trigger.IntervalType:
                    return Column.IntervalType;

                case Trigger.Poll:
                    return Column.Poll;

                default:
                    throw new ArgumentException($"Unsupported IntervalType '{trigger}'.");
            }
        }
    }
}
