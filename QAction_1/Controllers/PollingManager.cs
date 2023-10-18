namespace Skyline.PollingManager.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Scripting;
	using Skyline.PollingManager.Enums;
	using Skyline.PollingManager.Interfaces;
	using Skyline.PollingManager.Providers;

	public class PollingManager
	{
		private static PollingManager _instance;
		private readonly PollingmanagerQActionTable _table;
		private readonly Dictionary<string, PollableBase> _rows = new Dictionary<string, PollableBase>();
		private readonly IPollableBaseFactory _pollableFactory;

		private PollingManager(PollingmanagerQActionTable table, List<PollableBase> rows, IPollableBaseFactory pollableFactory)
		{
			_table = table;
			_pollableFactory = pollableFactory;

			HashSet<string> names = new HashSet<string>();

			for (int i = 0; i < rows.Count; i++)
			{
				if (!names.Add(rows[i].Name))
					throw new ArgumentException($"Duplicate name: {rows[i].Name}");

				_rows.Add((i+1).ToString(), rows[i]);
			}

			FillTable(_rows);
		}

		public static PollingManager Instance => _instance ?? throw new InvalidOperationException("Polling manager is not initialized, call Init!");

		public static PollingManager Init(PollingmanagerQActionTable table, List<PollableBase> rows, IPollableBaseFactory pollableFactory)
		{
			if (_instance == null)
				_instance = new PollingManager(table, rows, pollableFactory);

			return _instance;
		}

		public void CheckForUpdate()
		{
			bool requiresUpdate = false;

			foreach (var row in _rows)
			{
				if (row.Value.State == State.Disabled || row.Value.State == State.DisabledParents)
					continue;

				bool readyToPoll;
				bool pollSucceeded;

				switch (row.Value.PeriodType)
				{
					case PeriodType.Default:
						readyToPoll = CheckLastPollTime(row.Value.DefaultPeriod, row.Value.LastPoll);
						break;

					case PeriodType.Custom:
						readyToPoll = CheckLastPollTime(row.Value.Period, row.Value.LastPoll);
						break;

					default:
						throw new InvalidOperationException($"Unhandled PeriodType: {row.Value.PeriodType}");
				}

				if (readyToPoll && CheckParents(row.Value))
				{
					pollSucceeded = row.Value.Poll();
					row.Value.LastPoll = DateTime.Now;
				}
				else
				{
					continue;
				}

				if (pollSucceeded)
					row.Value.Status = Status.Succeeded;
				else
					row.Value.Status = Status.Failed;

				requiresUpdate = true;
			}

			if (requiresUpdate)
				FillTableNoDelete(_rows);
		}

		public void UpdateRow(string id, Column column)
		{
			PollableBase tableRow = CreateIPollable(_table.GetRow(id));

			switch (column)
			{
				case Column.Period:
					tableRow.PeriodType = PeriodType.Custom;
					break;

				case Column.PeriodType:
					if (tableRow.PeriodType == PeriodType.Custom)
						tableRow.Period = _rows[id].Period;
					break;

				case Column.Poll:
					PollRow(tableRow);
					break;

				case Column.State:
					UpdateState(tableRow);
					break;

				default:
					break;
			}

			UpdateInternalRow(id, tableRow);
			FillTableNoDelete(_rows);
		}

		private void UpdateState(IPollable row)
		{
			if (row.Parents.Where(parent => parent.State == State.Disabled).Any())
			{
				row.State = State.Disabled;
				return;
			}

			switch (row.State)
			{
				case State.Disabled:
					row.Status = Status.Disabled;
					UpdateRelatedStates(row.Children, State.Disabled);
					return;

				case State.Enabled:
					row.Status = Status.NotPolled;
					return;

				case State.DisabledParents:
					row.Status = Status.Disabled;
					UpdateRelatedStates(row.Parents, State.Disabled);
					return;

				case State.EnabledChildren:
					row.Status = Status.NotPolled;
					UpdateRelatedStates(row.Children, State.Enabled);
					return;
			}
		}

		private void UpdateRelatedStates(List<IPollable> collection, State state)
		{
			SLProtocolProvider.Protocol.Log($"collection.Count [{collection.Count}]");

			foreach (var item in collection)
			{
				item.Status = Status.Disabled;
				item.State = state;
				SLProtocolProvider.Protocol.Log($"item.Name [{item.Name}]");
				UpdateState(item);
			}
		}

		private void PollRow(PollableBase row)
		{
			if (row.State == State.Disabled || row.State == State.DisabledParents)
				return;

			bool pollSucceeded = row.Poll();
			row.LastPoll = DateTime.Now;

			if (pollSucceeded)
				row.Status = Status.Succeeded;
			else
				row.Status = Status.Failed;
		}

		private void UpdateInternalRow(string id, PollableBase newValue)
		{
			_rows[id].Name = newValue.Name;
			_rows[id].Period = newValue.Period;
			_rows[id].DefaultPeriod = newValue.DefaultPeriod;
			_rows[id].PeriodType = newValue.PeriodType;
			_rows[id].LastPoll = newValue.LastPoll;
			_rows[id].Status = newValue.Status;
			_rows[id].State = newValue.State;
			_rows[id].Parents = newValue.Parents;
			_rows[id].Children = newValue.Children;
		}

		private bool CheckParents(PollableBase row)
		{
			foreach (var parent in row.Parents)
			{
				if (parent.Status == Status.Disabled)
					return false;
			}

			return true;
		}

		private bool CheckLastPollTime(int period, DateTime lastPoll)
		{
			if ((DateTime.Now - lastPoll).TotalSeconds > period)
				return true;

			return false;
		}

		private void FillTable(Dictionary<string, PollableBase> rows)
		{
			PollingmanagerQActionRow[] tableRows = CreateTableRows(rows);

			_table.FillArray(tableRows);
		}

		private void FillTableNoDelete(string key, PollableBase value)
		{
			FillTableNoDelete(new Dictionary<string, PollableBase>{ { key, value } });
		}

		private void FillTableNoDelete(Dictionary<string, PollableBase> rows)
		{
			PollingmanagerQActionRow[] tableRows = CreateTableRows(rows);

			_table.FillArrayNoDelete(tableRows);
		}

		private PollingmanagerQActionRow CreateTableRow(string key, PollableBase value)
		{
			return new PollingmanagerQActionRow
			{
				Pollingmanagerindex_1001 = key,
				Pollingmanagername_1002 = value.Name,
				Pollingmanagerperiod_1003 = value.PeriodType == PeriodType.Custom ? value.Period : value.DefaultPeriod,
				Pollingmanagerdefaultperiod_1004 = value.DefaultPeriod,
				Pollingmanagerperiodtype_1005 = value.PeriodType,
				Pollingmanagerlastpoll_1006 = value.Status == Status.NotPolled ? (double)Status.NotPolled : value.LastPoll.ToOADate(),
				Pollingmanagerstatus_1007 = value.Status,
				Pollingmanagerstate_1009 = value.State,
			};
		}

		private PollingmanagerQActionRow[] CreateTableRows(Dictionary<string, PollableBase> rows)
		{
			List<PollingmanagerQActionRow> tableRows = new List<PollingmanagerQActionRow>();

			foreach(var row in rows)
			{
				tableRows.Add(CreateTableRow(row.Key, row.Value));
			}

			return tableRows.ToArray();
		}

		private PollableBase CreateIPollable(object[] tableRow)
		{
			string id = (string)tableRow[0];
			PollableBase row = _pollableFactory.CreatePollableBase(tableRow);

			row.Parents = _rows[id].Parents;
			row.Children = _rows[id].Children;

			return row;
		}
	}
}
