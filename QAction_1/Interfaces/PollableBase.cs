namespace Skyline.PollingManager.Interfaces
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.DMSState.Agents;
	using Skyline.PollingManager.Enums;

	public abstract class PollableBase : IPollable
	{
        private List<IPollable> _parents = new List<IPollable>();
        private List<IPollable> _children = new List<IPollable>();

        public PollableBase(object[] row)
        {
            Name = (string)row[1];
            Period = (int)(double)row[2];
            DefaultPeriod = (int)(double)row[3];
            PeriodType = (PeriodType)(double)row[4];
            LastPoll = DateTime.FromOADate((double)row[5]);
            Status = (Status)(double)row[6];
        }

        public PollableBase(string name)
        {
            Name = name;
            Period = 60;
            DefaultPeriod = 60;
            PeriodType = PeriodType.Default;
            LastPoll = default;
            Status = Status.NotPolled;
        }

        public string Name { get; set; }

        public int Period { get; set; }

        public int DefaultPeriod { get; set; }

        public PeriodType PeriodType { get; set; }

        public DateTime LastPoll { get; set; }

        public Status Status { get; set; }

        public List<IPollable> Parents => _parents;

        public List<IPollable> Children => _children;

        public abstract bool Poll();

        void IPollable.AddParent(IPollable parent)
        {
			if (_children.Contains(parent))
				throw new InvalidOperationException($"Circular dependency, {parent.Name} is already a child of {Name}!");

			if (_parents.Contains(parent))
                return;

			_parents.Add(parent);
		}

        public void AddParents(params IPollable[] parents)
		{
            foreach (var parent in parents)
            {
                if (_children.Contains(parent))
                    throw new InvalidOperationException($"Circular dependency, {parent.Name} is already a child of {Name}!");

                if (_parents.Contains(parent))
                    return;

                parent.AddChild(this);
                _parents.Add(parent);
            }
		}

        void IPollable.AddChild(IPollable child)
        {
			if (_parents.Contains(child))
				throw new InvalidOperationException($"Circular dependency, {child.Name} is already a parent of {Name}!");

			if (_children.Contains(child))
				return;

			_children.Add(child);
		}

        public void AddChildren(params IPollable[] children)
		{
            foreach (var child in children)
            {
                if (_parents.Contains(child))
                    throw new InvalidOperationException($"Circular dependency, {child.Name} is already a parent of {Name}!");

                if (_children.Contains(child))
                    return;

                child.AddParent(this);
                _children.Add(child);
            }
		}
	}
}
