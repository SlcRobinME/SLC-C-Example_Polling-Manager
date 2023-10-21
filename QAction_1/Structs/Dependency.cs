namespace Skyline.PollingManager.Interfaces
{
	using System;
	using System.CodeDom;

	public struct Dependency
	{
		public Dependency(object value, bool shouldEqual, Type type)
		{
			Value = value;
			ShouldEqual = shouldEqual;
			Type = type;
		}

		public object Value { get; set; }

		public bool ShouldEqual { get; set; }

		public Type Type { get; set; }
	}
}
