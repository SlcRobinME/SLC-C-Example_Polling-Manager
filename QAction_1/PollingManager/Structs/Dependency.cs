namespace Skyline.PollingManager.Structs
{
    using System;

    /// <summary>
    /// Represents parameter dependancy.
    /// </summary>
    public struct Dependency
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="Dependency"/> struct.
		/// </summary>
		/// <param name="value">Value to check against.</param>
		/// <param name="shouldEqual">Whether <paramref name="value"/> should be equal or different from the value of parameter.</param>
		/// <param name="message">Message to show when condition is not met.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="value"/> is not of type double or string.</exception>
		public Dependency(object value, bool shouldEqual, string message)
        {
            if (value is double || value is string)
                Value = value;
            else if (value is int)
                Value = Convert.ToDouble(value);
            else
                throw new ArgumentException("Value is not of type int, double or string!");

            ShouldEqual = shouldEqual;
            Message = message;
        }

		public object Value { get; set; }

		public bool ShouldEqual { get; set; }

		public string Message { get; set; }
    }
}
