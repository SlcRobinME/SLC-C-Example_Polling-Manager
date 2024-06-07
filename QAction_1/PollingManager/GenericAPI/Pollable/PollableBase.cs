﻿namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Base class that implements <see cref="IPollable"/>.
	/// </summary>
	public abstract class PollableBase : IPollable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PollableBase"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="name">Name of the PollingManager table row.</param>
		/// <param name="interval">Default polling interval.</param>
		public PollableBase(SLProtocol protocol, string name, TimeSpan interval)
		{
			Protocol = protocol;
			Name = name;
			Interval = interval.TotalSeconds;
			DefaultInterval = interval.TotalSeconds;
			IntervalType = IntervalType.Default;
			LastPoll = default;
			Status = Status.NotPolled;
			Reason = string.Empty;
			State = State.Enabled;
		}

		public SLProtocol Protocol { get; set; }

		public string Name { get; set; }

		public double Interval { get; set; }

		public double DefaultInterval { get; set; }

		public IntervalType IntervalType { get; set; }

		public DateTime LastPoll { get; set; }

		public Status Status { get; set; }

		public string Reason { get; set; }

		public State State { get; set; }

		public List<IPollable> Parents { get; set; } = new List<IPollable>();

		public List<IPollable> Children { get; set; } = new List<IPollable>();

		public Dictionary<int, Dependency> Dependencies { get; set; } = new Dictionary<int, Dependency>();

		/// <summary>
		/// Method to be implemented by extending class. This method gets called by <see cref="PollingManager"/>.
		/// </summary>
		/// <returns>Returns true for success and false for failed poll.</returns>
		/// <remarks>Implementation of <see cref="Poll"/> should never throw.</remarks>
		public abstract bool Poll();

		/// <summary>
		/// Method to be implemented by extending class. This method gets called by <see cref="PollingManager"/> context menu when disabling or force disabling the item.
		/// </summary>
		public abstract void Disable();

		/// <summary>
		/// Updates current state of <see cref="PollableBase"/>.
		/// </summary>
		/// <param name="row">Row on which to base the update.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="row"/> has length less then 9.</exception>
		public void Update(object[] row)
		{
			if (row.Length < 9)
			{
				throw new ArgumentException($"Parameter '{nameof(row)}' must have at least 9 elements, but has '{row.Length}'.");
			}

			var name = Convert.ToString(row[(int)Column.Name]);
			if (String.IsNullOrEmpty(name))
			{
				// BugFix: when a new poll item is added, the LoadRows() method call this Update()-method to overwrite the in memory row (newly added) with a not yet existing row -> null values.
				// This stops the element from polling completely and forces the user to recreate the element.
				// Checking if this value is not empty prevents unwanted behavior by overwriting.
				return;
			}

			Name = name;
			Interval = Convert.ToDouble(row[(int)Column.Interval]);
			DefaultInterval = Convert.ToDouble(row[(int)Column.DefaultInterval]);
			IntervalType = (IntervalType)Convert.ToDouble(row[(int)Column.IntervalType]);
			LastPoll = DateTime.FromOADate(Convert.ToDouble(row[(int)Column.LastPoll]));
			Status = (Status)Convert.ToDouble(row[(int)Column.Status]);
			Reason = Convert.ToString(row[(int)Column.Reason]) ?? string.Empty;
			State = (State)Convert.ToDouble(row[(int)Column.State]);
		}

		/// <summary>
		/// Gets dependent parameters and compares their values with dependencies. Sets <see cref="Reason"/> to first condition not satisfied.
		/// </summary>
		/// <returns>False if any condition is not satisfied, otherwise true.</returns>
		public bool CheckDependencies()
		{
			try
			{
				foreach (KeyValuePair<int, Dependency> dependency in Dependencies)
				{
					object parameter = Protocol.GetParameter(dependency.Key)
						?? throw new InvalidOperationException($"Parameter with ID '{dependency.Key}' doesn't exist.");

					if (dependency.Value.Value is double)
					{
						if (dependency.Value.ShouldEqual)
						{
							if (!CheckDoubleParameter(parameter, dependency.Value.Value))
							{
								Reason = dependency.Value.Message;
								return false;
							}
						}
						else
						{
							if (CheckDoubleParameter(parameter, dependency.Value.Value))
							{
								Reason = dependency.Value.Message;
								return false;
							}
						}
					}
					else if (dependency.Value.Value is string)
					{
						if (dependency.Value.ShouldEqual)
						{
							if (!CheckStringParameter(parameter, dependency.Value.Value))
							{
								Reason = dependency.Value.Message;
								return false;
							}
						}
						else
						{
							if (CheckStringParameter(parameter, dependency.Value.Value))
							{
								Reason = dependency.Value.Message;
								return false;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Protocol.Log($"QA{Protocol.QActionID}|{Protocol.GetTriggerParameter()}|PollableBase.CheckDependencies|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);

				Reason = "Something went wrong. Please check logs.";
				return false;
			}

			Reason = string.Empty;
			return true;
		}

		/// <summary>
		/// Adds a dependency.
		/// </summary>
		/// <param name="paramId">Id of parameter on which this row is depending.</param>
		/// <param name="dependency">Dependency object.</param>
		public void AddDependency(int paramId, Dependency dependency)
		{
			Dependencies.Add(paramId, dependency);
		}

		/// <summary>
		/// Adds parent without creating two way relation between elements. This shouldn't be used directly. Use <see cref="AddParents"/> instead.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <exception cref="InvalidOperationException">Throws if <paramref name="parent"/> is already this elements child.</exception>
		void IPollable.AddParent(IPollable parent)
		{
			if (Children.Contains(parent))
			{
				throw new InvalidOperationException($"Circular dependency, '{parent.Name}' is already a child of '{Name}'.");
			}

			if (Parents.Contains(parent))
			{
				return;
			}

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
				{
					throw new InvalidOperationException($"Circular dependency, '{parent.Name}' is already a child of '{Name}'.");
				}

				if (Parents.Contains(parent))
				{
					return;
				}

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
			{
				throw new InvalidOperationException($"Circular dependency, '{child.Name}' is already a parent of '{Name}'.");
			}

			if (Children.Contains(child))
			{
				return;
			}

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
				{
					throw new InvalidOperationException($"Circular dependency, '{child.Name}' is already a parent of '{Name}'.");
				}

				if (Children.Contains(child))
				{
					return;
				}

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
		private static bool CheckDoubleParameter(object parameter, object value)
		{
			return parameter is double d
				? d == (double)value
				: throw new ArgumentException($"{nameof(parameter)} is not of type double.");
		}

		/// <summary>
		/// Compares values of boxed string types.
		/// </summary>
		/// <param name="parameter">Parameter object.</param>
		/// <param name="value">Object to compare value against.</param>
		/// <returns>True if the boxed values are the same, otherwise false.</returns>
		/// <exception cref="ArgumentException">Throws if boxed <paramref name="parameter"/> type is not string.</exception>
		private static bool CheckStringParameter(object parameter, object value)
		{
			return parameter is string s
				? s == (string)value
				: throw new ArgumentException($"{nameof(parameter)} is not of type string.");
		}
	}
}
