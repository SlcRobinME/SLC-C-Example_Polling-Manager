namespace Skyline.PollingManager.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Scripting;

    using Skyline.PollingManager.Enums;
    using Skyline.PollingManager.Structs;

	/// <summary>
	/// Base class that implements <see cref="IPollable"/>.
	/// </summary>
    public abstract class PollableBase : IPollable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PollableBase"/> class from table row.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="row">Row loaded from the table via GetRow.</param>
		public PollableBase(SLProtocol protocol, object[] row)
		{
			Protocol = protocol;
			Name = Convert.ToString(row[1]) ?? string.Empty;
			Period = Convert.ToDouble(row[2]);
			DefaultPeriod = Convert.ToDouble(row[3]);
			PeriodType = (PeriodType)Convert.ToDouble(row[4]);
			LastPoll = DateTime.FromOADate(Convert.ToDouble(row[5]));
			Status = (Status)Convert.ToDouble(row[6]);
			State = (State)Convert.ToDouble(row[8]);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PollableBase"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="name">Name of the PollingManager table row.</param>
		public PollableBase(SLProtocol protocol, string name)
        {
            Protocol = protocol;
            Name = name;
            Period = 5;
            DefaultPeriod = 10;
            PeriodType = PeriodType.Default;
            LastPoll = default;
            Status = Status.NotPolled;
            State = State.Enabled;
        }

		public SLProtocol Protocol { get; set; }

		public string Name { get; set; }

		public double Period { get; set; }

		public double DefaultPeriod { get; set; }

		public PeriodType PeriodType { get; set; }

		public DateTime LastPoll { get; set; }

		public Status Status { get; set; }

		public State State { get; set; }

		public List<IPollable> Parents { get; set; } = new List<IPollable>();

		public List<IPollable> Children { get; set; } = new List<IPollable>();

		public Dictionary<int, Dependency> Dependencies { get; set; } = new Dictionary<int, Dependency>();

		/// <summary>
		/// Method to be implemented by extending class. This method gets called by PollingManager.
		/// </summary>
		/// <returns>Returns true for success and false for failed poll.</returns>
		public abstract bool Poll();

		/// <summary>
		/// Gets dependant parameters and compares their values with dependencies.
		/// </summary>
		/// <returns>False if any dependency is not satisfied, otherwise true.</returns>
		public bool CheckDependencies()
        {
			foreach (KeyValuePair<int, Dependency> dependency in Dependencies)
            {
                object parameter = Protocol.GetParameter(dependency.Key);

                if (dependency.Value.Value is double)
                {
                    if (dependency.Value.ShouldEqual)
                    {
                        if (!CheckDoubleParameter(parameter, dependency.Value.Value))
                            return false;
                    }
                    else
                    {
						if (CheckDoubleParameter(parameter, dependency.Value.Value))
							return false;
					}
                }
                else if (dependency.Value.Value is string)
                {
					if (dependency.Value.ShouldEqual)
					{
						if (!CheckStringParameter(parameter, dependency.Value.Value))
							return false;
					}
					else
					{
						if (CheckStringParameter(parameter, dependency.Value.Value))
							return false;
					}
				}
            }

			return true;
        }

		/// <summary>
		/// Adds parent without creating two way relation between elements. This shouldn't be used directly. Use <see cref="AddParents"/> instead.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <exception cref="InvalidOperationException">Throws if <paramref name="parent"/> is already this elements child.</exception>
		void IPollable.AddParent(IPollable parent)
        {
			if (Children.Contains(parent))
				throw new InvalidOperationException($"Circular dependency, {parent.Name} is already a child of {Name}!");

			if (Parents.Contains(parent))
                return;

			Parents.Add(parent);
		}

		/// <summary>
		/// Adds parents to this element, and adds this element as a child of each parent passed as parameter.
		/// </summary>
		/// <param name="parents">Parent elements.</param>
		/// <exception cref="InvalidOperationException">Throws if any <paramref name="parents"/> element is already this elements child.</exception>
		public void AddParents(params IPollable[] parents)
		{
            foreach (IPollable parent in parents)
            {
                if (Children.Contains(parent))
                    throw new InvalidOperationException($"Circular dependency, {parent.Name} is already a child of {Name}!");

                if (Parents.Contains(parent))
                    return;

                parent.AddChild(this);
                Parents.Add(parent);
            }
		}

		/// <summary>
		/// Adds child without creating two way relation between elements. This shouldn't be used directly. Use <see cref="AddChildren"/> instead.
		/// </summary>
		/// <param name="child">Child element.</param>
		/// <exception cref="InvalidOperationException">Throws if <paramref name="child"/> is already this elements parent.</exception>
		void IPollable.AddChild(IPollable child)
        {
			if (Parents.Contains(child))
				throw new InvalidOperationException($"Circular dependency, {child.Name} is already a parent of {Name}!");

			if (Children.Contains(child))
				return;

			Children.Add(child);
		}

		/// <summary>
		/// Adds children to this element, and adds this element as a parent of each child passed as parameter.
		/// </summary>
		/// <param name="children">Child elements.</param>
		/// <exception cref="InvalidOperationException">Throws if any <paramref name="children"/> element is already this elements parent.</exception>
		public void AddChildren(params IPollable[] children)
		{
            foreach (IPollable child in children)
            {
                if (Parents.Contains(child))
                    throw new InvalidOperationException($"Circular dependency, {child.Name} is already a parent of {Name}!");

                if (Children.Contains(child))
                    return;

                child.AddParent(this);
                Children.Add(child);
            }
		}

		/// <summary>
		/// Compares values of boxed double types.
		/// </summary>
		/// <param name="parameter">Parameter object.</param>
		/// <param name="value">Object to compare value against.</param>
		/// <returns>True if the boxed values are the same, otherwise false.</returns>
		/// <exception cref="ArgumentException">Throws if boxed <paramref name="parameter"/> type is not double.</exception>
		private bool CheckDoubleParameter(object parameter, object value)
		{
			return parameter is double ? (double)parameter == (double)value : throw new ArgumentException("Parameter is not of type double!");
		}

		/// <summary>
		/// Compares values of boxed string types.
		/// </summary>
		/// <param name="parameter">Parameter object.</param>
		/// <param name="value">Object to compare value against.</param>
		/// <returns>True if the boxed values are the same, otherwise false.</returns>
		/// <exception cref="ArgumentException">Throws if boxed <paramref name="parameter"/> type is not string.</exception>
		private bool CheckStringParameter(object parameter, object value)
		{
			return parameter is string ? (string)parameter == (string)value : throw new ArgumentException("Parameter is not of type string!");
		}
	}
}
