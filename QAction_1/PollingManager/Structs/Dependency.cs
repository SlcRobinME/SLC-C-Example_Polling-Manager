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
		/// <exception cref="ArgumentException">Throws if <paramref name="value"/> is not of type double or string.</exception>
		public Dependency(object value, bool shouldEqual)
        {
            if (value is double || value is string)
                Value = value;
            else
                throw new ArgumentException("Values underlying type is not double or string!");

            ShouldEqual = shouldEqual;
        }

		public object Value { get; set; }

		public bool ShouldEqual { get; set; }
    }
}
