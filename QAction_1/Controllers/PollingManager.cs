namespace Skyline.PollingManager.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Scripting;
	using Skyline.PollingManager.Enums;
	using Skyline.PollingManager.Interfaces;

	public static class PollingManagerContainer
	{
		private static Dictionary<string, PollingManager> _managers = new Dictionary<string, PollingManager>();

		public static PollingManager AddManager(SLProtocol protocol, PollingmanagerQActionTable table, List<PollableBase> rows, IPollableBaseFactory pollableFactory)
		{
			string key = GetKey(protocol);

			if (!_managers.ContainsKey(key))
			{
				var manager = new PollingManager(protocol, table, rows, pollableFactory);

				_managers.Add(key, manager);
			}

			return _managers[key];
		}

		public static PollingManager GetManager(SLProtocol protocol)
		{
			string key = GetKey(protocol);

			if (!_managers.ContainsKey(key))
				throw new InvalidOperationException("Polling manager for this element is not initialized, please call AddManager first.");

			_managers[key].Protocol = protocol;

			return _managers[key];
		}

		private static string GetKey(SLProtocol protocol)
		{
			return string.Join("/", protocol.DataMinerID, protocol.ElementID);
		}
	}

	public class PollingManager
	{
		private readonly PollingmanagerQActionTable _table;
		private readonly Dictionary<string, PollableBase> _rows = new Dictionary<string, PollableBase>();
		private readonly IPollableBaseFactory _pollableFactory;

		internal PollingManager(SLProtocol protocol, PollingmanagerQActionTable table, List<PollableBase> rows, IPollableBaseFactory pollableFactory)
		{
			Protocol = protocol;
			_table = table;
			_pollableFactory = pollableFactory;

			HashSet<string> names = new HashSet<string>();

			for (int i = 0; i < rows.Count; i++)
			{
				if (!names.Add(rows[i].Name))
					throw new ArgumentException($"Duplicate name: {rows[i].Name}");

				_rows.Add((i + 1).ToString(), rows[i]);
			}

			FillTable(_rows);
		}

		public SLProtocol Protocol { get; set; }

		public void CheckForUpdate()
		{
			bool requiresUpdate = false;

			foreach (KeyValuePair<string, PollableBase> row in _rows)
			{
				if (row.Value.State == State.Disabled)
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

				if (readyToPoll && CheckParents(row.Value) && row.Value.CheckDependencies())
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

		public void HandleContextMenu(string[] contextMenu)
		{
			int.TryParse(contextMenu[1], out int selectedOption);

			switch ((ContextMenuOption)selectedOption)
			{
				case ContextMenuOption.PollAll:
					PollAll();
					break;

				case ContextMenuOption.DisableAll:
					DisableAll();
					break;

				case ContextMenuOption.EnableAll:
					EnableAll();
					break;

				case ContextMenuOption.DisableSelected:
					DisableSelected(contextMenu.Skip(2).ToArray());
					break;

				case ContextMenuOption.EnableSelected:
					EnableSelected(contextMenu.Skip(2).ToArray());
					break;

				default:
					break;
			}

			FillTableNoDelete(_rows);
		}

		private void PollAll()
		{
			foreach (KeyValuePair<string, PollableBase> row in _rows)
			{
				PollRow(row.Value);
			}
		}

		private void DisableAll()
		{
			foreach (KeyValuePair<string, PollableBase> row in _rows)
			{
				row.Value.State = State.Disabled;
			}
		}

		private void EnableAll()
		{
			foreach (KeyValuePair<string, PollableBase> row in _rows)
			{
				row.Value.State = State.Enabled;
			}
		}

		private void DisableSelected(string[] rows)
		{
			foreach(string row in rows)
			{
				_rows[row].State = State.Disabled;
			}
		}

		private void EnableSelected(string[] rows)
		{
			foreach (string row in rows)
			{
				_rows[row].State = State.Enabled;
			}
		}

		private void UpdateState(IPollable row)
		{
			switch (row.State)
			{
				case State.Disabled:
					if (row.Children.Any(child => child.State == State.Enabled))
					{
						ShowChildren(row);
						row.State = State.Enabled;
						return;
					}

					row.Status = Status.Disabled;
					UpdateRelatedStates(row.Children, State.Disabled);
					return;

				case State.Enabled:
					if (row.Parents.Any(parent => parent.State == State.Disabled))
					{
						ShowParents(row);
						row.State = State.Disabled;
					}

					return;

				case State.ForceDisabled:
					row.State = State.Disabled;
					row.Status = Status.Disabled;
					UpdateRelatedStates(row.Children, State.ForceDisabled);
					return;

				case State.ForceEnabled:
					row.State = State.Enabled;
					UpdateRelatedStates(row.Parents, State.ForceEnabled);
					return;
			}
		}

		private void ShowChildren(IPollable row)
		{
			string children = string.Join("\n", row.Children.Select(child => child.Name));

			string message = $"Unable to disable [{row.Name}] because the following rows are dependand on it:\n{children}\nPlease disable them first or use [Force Disable].";

			Protocol.ShowInformationMessage(message);
		}

		private void ShowParents(IPollable row)
		{
			string parents = string.Join("\n", row.Parents.Select(parent => parent.Name));

			string message = $"Unable to enable [{row.Name}] because it depends on the following rows:\n{parents}\nPlease enable them first or use [Force Enable].";

			Protocol.ShowInformationMessage(message);
		}

		private void UpdateRelatedStates(List<IPollable> collection, State state)
		{
			foreach (IPollable item in collection)
			{
				item.Status = Status.Disabled;
				item.State = state;
				UpdateState(item);
			}
		}

		private void PollRow(PollableBase row)
		{
			if (row.State == State.Disabled)
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
			foreach (IPollable parent in row.Parents)
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
				Pollingmanagerperiod_1003 = value.State == State.Disabled ? -1 : value.PeriodType == PeriodType.Custom ? value.Period : value.DefaultPeriod,
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

			foreach(KeyValuePair<string, PollableBase> row in rows)
			{
				tableRows.Add(CreateTableRow(row.Key, row.Value));
			}

			return tableRows.ToArray();
		}

		private PollableBase CreateIPollable(object[] tableRow)
		{
			string id = (string)tableRow[0];
			PollableBase row = _pollableFactory.CreatePollableBase(Protocol, tableRow);

			row.Parents = _rows[id].Parents;
			row.Children = _rows[id].Children;

			return row;
		}
	}

}
