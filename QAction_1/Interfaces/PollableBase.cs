namespace Skyline.PollingManager.Interfaces
{
	using System;
	using System.Collections.Generic;

	using Skyline.PollingManager.Enums;

	public abstract class PollableBase : IPollable
	{
        public PollableBase(object[] row)
        {
            Name = (string)row[1];
            Period = (int)(double)row[2];
            DefaultPeriod = (int)(double)row[3];
            PeriodType = (PeriodType)(double)row[4];
            LastPoll = DateTime.FromOADate((double)row[5]);
            Status = (Status)(double)row[6];
            State = (State)(double)row[8];
        }

        public PollableBase(string name)
        {
            Name = name;
            Period = 60;
            DefaultPeriod = 60;
            PeriodType = PeriodType.Default;
            LastPoll = default;
            Status = Status.NotPolled;
            State = State.Enabled;
        }

        public string Name { get; set; }

        public int Period { get; set; }

        public int DefaultPeriod { get; set; }

        public PeriodType PeriodType { get; set; }

        public DateTime LastPoll { get; set; }

        public Status Status { get; set; }

        public State State { get; set; }

        public List<IPollable> Parents { get; set; } = new List<IPollable>();

        public List<IPollable> Children { get; set; } = new List<IPollable>();

        public abstract bool Poll();

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
	}
}
