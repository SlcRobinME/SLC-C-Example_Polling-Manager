namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents base for a row in the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public interface IPollable
	{
        SLProtocol Protocol { get; set; }

        string Name { get; set; }

        double Period { get; set; }

        double DefaultPeriod { get; set; }

        PeriodType PeriodType { get; set; }

        DateTime LastPoll { get; set; }

        Status Status { get; set; }

        string Reason { get; set; }

        State State { get; set; }

        List<IPollable> Parents { get; set; }

        List<IPollable> Children { get; set; }

        Dictionary<int, Dependency> Dependencies { get; set; }

        bool Poll();

        bool CheckDependencies();

        void AddDependency(int paramId, Dependency dependency);

        void AddParent(IPollable parent);

        void AddParents(params IPollable[] parents);

        void AddChild(IPollable child);

        void AddChildren(params IPollable[] children);
    }
}
