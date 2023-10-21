﻿namespace Skyline.PollingManager.Interfaces
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;
	using Skyline.PollingManager.Enums;

	public abstract class PollableBase : IPollable
	{
        public PollableBase(SLProtocol protocol, object[] row)
		{
			Protocol = protocol;
			Name = (string)row[1];
			Period = (int)(double)row[2];
			DefaultPeriod = (int)(double)row[3];
			PeriodType = (PeriodType)(double)row[4];
			LastPoll = DateTime.FromOADate((double)row[5]);
			Status = (Status)(double)row[6];
			State = (State)(double)row[8];
		}

        public PollableBase(SLProtocol protocol, string name)
        {
            Protocol = protocol;
            Name = name;
            Period = 60;
            DefaultPeriod = 60;
            PeriodType = PeriodType.Default;
            LastPoll = default;
            Status = Status.NotPolled;
            State = State.Enabled;
        }

        public SLProtocol Protocol { get; set; }

        public string Name { get; set; }

        public int Period { get; set; }

        public int DefaultPeriod { get; set; }

        public PeriodType PeriodType { get; set; }

        public DateTime LastPoll { get; set; }

        public Status Status { get; set; }

        public State State { get; set; }

        public List<IPollable> Parents { get; set; } = new List<IPollable>();

        public List<IPollable> Children { get; set; } = new List<IPollable>();

        public Dictionary<int, Dependency> Dependencies { get; set; } = new Dictionary<int, Dependency>();

        public abstract bool Poll();

        public bool CheckDependencies()
        {
			foreach (var dependency in Dependencies)
            {
                object parameter = Protocol.GetParameter(dependency.Key);

                if (dependency.Value.Type == typeof(double))
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
                else if (dependency.Value.Type == typeof(string))
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

        void IPollable.AddParent(IPollable parent)
        {
			if (Children.Contains(parent))
				throw new InvalidOperationException($"Circular dependency, {parent.Name} is already a child of {Name}!");

			if (Parents.Contains(parent))
                return;

			Parents.Add(parent);
		}

        public void AddParents(params IPollable[] parents)
		{
            foreach (var parent in parents)
            {
                if (Children.Contains(parent))
                    throw new InvalidOperationException($"Circular dependency, {parent.Name} is already a child of {Name}!");

                if (Parents.Contains(parent))
                    return;

                parent.AddChild(this);
                Parents.Add(parent);
            }
		}

        void IPollable.AddChild(IPollable child)
        {
			if (Parents.Contains(child))
				throw new InvalidOperationException($"Circular dependency, {child.Name} is already a parent of {Name}!");

			if (Children.Contains(child))
				return;

			Children.Add(child);
		}

        public void AddChildren(params IPollable[] children)
		{
            foreach (var child in children)
            {
                if (Parents.Contains(child))
                    throw new InvalidOperationException($"Circular dependency, {child.Name} is already a parent of {Name}!");

                if (Children.Contains(child))
                    return;

                child.AddParent(this);
                Children.Add(child);
            }
		}

        private bool CheckDoubleParameter(object parameter, object value)
		{
            return (double)parameter == (double)value;
		}

        private bool CheckStringParameter(object parameter, object value)
		{
            return (string)parameter == (string)value;
		}
	}
}
