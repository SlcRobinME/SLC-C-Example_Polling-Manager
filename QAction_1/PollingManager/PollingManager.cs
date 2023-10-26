namespace Skyline.PollingManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Scripting;

    using Skyline.PollingManager.Enums;
    using Skyline.PollingManager.Pollable;

    /// <summary>
    /// <see cref="PollingManager"/> container class used to provide singleton on the element level.
    /// </summary>
    public static class PollingManagerContainer
    {
        private static Dictionary<string, PollingManager> _managers = new Dictionary<string, PollingManager>();

		/// <summary>
		/// Creates instance of <see cref="PollingManager"/> and adds it to <see cref="PollingManagerContainer"/>.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="table">Polling manager table instance.</param>
		/// <param name="rows">Rows to add to the <paramref name="table"/>.</param>
		/// <returns>
		/// Newly created instance of <see cref="PollingManager"/>, if it doesn't exist, or existing instance of <see cref="PollingManager"/> with updated <see cref="PollableBase.Protocol"/>.
		/// </returns>
        /// <exception cref="ArgumentException">Throws if creation of <see cref="PollingManager"/> fails.</exception>
        public static PollingManager AddManager(SLProtocol protocol, PollingmanagerQActionTable table, List<PollableBase> rows)
        {
            string key = GetKey(protocol);

            if (!_managers.ContainsKey(key))
            {
                PollingManager manager;
                try
                {
                    manager = new PollingManager(protocol, table, rows);
                }
                catch (ArgumentException ex)
                {
					protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerContainer.AddManager|Exception thrown:{Environment.NewLine}{ex}!", LogType.Error, LogLevel.NoLogging);

					throw new ArgumentException("Failed to create PollingManager!");
                }

                _managers.Add(key, manager);
            }

            _managers[key].Protocol = protocol;

            return _managers[key];
        }

		/// <summary>
		/// Gets <see cref="PollingManager"/> instance for the element.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <returns><see cref="PollingManager"/> instance with updated <see cref="PollableBase.Protocol"/>.</returns>
		/// <exception cref="InvalidOperationException">Throws if <see cref="PollingManager"/> for this element is not initialized.</exception>
        public static PollingManager GetManager(SLProtocol protocol)
        {
            string key = GetKey(protocol);

            if (!_managers.ContainsKey(key))
                throw new InvalidOperationException($"Polling manager for element [{key}] is not initialized, please call AddManager first!");

            _managers[key].Protocol = protocol;

            return _managers[key];
        }

		/// <summary>
		/// Creates unique key based on DataMinerID and ElementID.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <returns>Key in format DataMinerID/ElementID</returns>
        private static string GetKey(SLProtocol protocol)
        {
            return string.Join("/", protocol.DataMinerID, protocol.ElementID);
        }
    }

    /// <summary>
    /// Handler for <see cref="PollingmanagerQActionTable"/>.
    /// </summary>
    public class PollingManager
    {
        private readonly PollingmanagerQActionTable _table;
        private readonly Dictionary<string, PollableBase> _rows = new Dictionary<string, PollableBase>();
        private readonly PollableFactory _pollableFactory;

		/// <summary>
		/// Initializes a new instance of the <see cref="PollingManager"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="table">Polling manager table instance.</param>
		/// <param name="rows">Rows to add to the <paramref name="table"/>.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="rows"/> contains duplicate names.</exception>
		/// <exception cref="ArgumentException">Throws if <paramref name="rows"/> contains null values.</exception>
        public PollingManager(SLProtocol protocol, PollingmanagerQActionTable table, List<PollableBase> rows)
        {
            Protocol = protocol;
            _table = table;
            _pollableFactory = new PollableFactory();

            HashSet<string> names = new HashSet<string>();

            for (int i = 0; i < rows.Count; i++)
            {
                if (!names.Add(rows[i].Name))
                    throw new ArgumentException($"Duplicate name: {rows[i].Name}!");

                _rows.Add((i + 1).ToString(), rows[i] ?? throw new ArgumentException("Rows parameter can't contain null values!"));
            }

            FillTable(_rows);
        }

        public SLProtocol Protocol { get; set; }

		/// <summary>
		/// Checks <see cref="PollingmanagerQActionTable"/> for rows that are ready to be polled and polls them.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Throws if <see cref="PollableBase.PeriodType"/> is not <see cref="PeriodType.Default"/> or <see cref="PeriodType.Custom"/>.
		/// </exception>
        public void CheckForUpdate()
        {
            bool requiresUpdate = false;

            foreach (KeyValuePair<string, PollableBase> row in _rows)
            {
				PollableBase currentRow = row.Value;

				if (currentRow.State == State.Disabled)
                    continue;

				bool readyToPoll;
				bool pollSucceeded;

				switch (currentRow.PeriodType)
                {
                    case PeriodType.Default:
                        readyToPoll = CheckLastPollTime(currentRow.DefaultPeriod, currentRow.LastPoll);
                        break;

                    case PeriodType.Custom:
                        readyToPoll = CheckLastPollTime(currentRow.Period, currentRow.LastPoll);
                        break;

                    default:
                        throw new ArgumentException($"Unhandled PeriodType: {currentRow.PeriodType}!");
                }

				if (readyToPoll && currentRow.CheckDependencies())
                {
                    pollSucceeded = currentRow.Poll();
                    currentRow.LastPoll = DateTime.Now;
                }
                else
                {
                    continue;
                }

				if (pollSucceeded)
					currentRow.Status = Status.Succeeded;
                else
					currentRow.Status = Status.Failed;

				requiresUpdate = true;
            }

            if (requiresUpdate)
                FillTableNoDelete(_rows);
        }

		/// <summary>
		/// Handles sets on the <see cref="PollingmanagerQActionTable"/>.
		/// </summary>
		/// <param name="rowId">Row key.</param>
		/// <param name="column">Column on which set was performed.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="rowId"/> doesn't exist in the table.</exception>
		/// <exception cref="ArgumentException">
		/// Throws if <paramref name="column"/> is not <see cref="Column.Period"/>, <see cref="Column.PeriodType"/> or <see cref="Column.Poll"/>.
		/// </exception>
        public void HandleRowUpdate(string rowId, Column column)
        {
			if (!_rows.ContainsKey(rowId))
				throw new ArgumentException($"Row key [{rowId}] doesn't exist in the Polling Manager table!");

			PollableBase tableRow = CreateIPollable(rowId);

			switch (column)
            {
                case Column.Period:
                    tableRow.PeriodType = PeriodType.Custom;
                    break;

                case Column.PeriodType:
                    if (tableRow.PeriodType == PeriodType.Custom)
                        tableRow.Period = _rows[rowId].Period;

                    break;

                case Column.Poll:
                    PollRow(tableRow);
                    break;

                default:
                    throw new ArgumentException($"Unhandled Column: {column}!");
			}

			UpdateInternalRow(rowId, tableRow);
			FillTableNoDelete(_rows);
        }

		/// <summary>
		/// Handles context menu actions for the <see cref="PollingmanagerQActionTable"/>.
		/// </summary>
		/// <param name="contextMenu">Object that contains information related to the context menu.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="contextMenu"/> is not of type string[].</exception>
		/// <exception cref="ArgumentException">
		/// Throws if second element of converted <paramref name="contextMenu"/> can't be parsed as int.
		/// </exception>
		/// <exception cref="ArgumentException">Throws if <paramref name="contextMenu"/> is missing row keys.</exception>
		/// <exception cref="ArgumentException">Throws if row key doesn't exist in the table.</exception>
        public void HandleContextMenu(object contextMenu)
        {
            var input = contextMenu as string[];

            if (input == null)
                throw new ArgumentException("Parameter can't be converted to string[]!");

            if (!int.TryParse(input[1], out int value))
                throw new ArgumentException("Unable to parse selected option from parameter!");

            var option = (ContextMenuOption)value;

            if (HasRowKeys(option) && input.Length <= 2)
                throw new ArgumentException("Parameter is missing row keys!");

            switch (option)
            {
                case ContextMenuOption.PollAll:
                    PollAll();
                    break;

                case ContextMenuOption.Disable:
                    UpdateState(_rows[input[2]], State.Disabled);
                    break;

                case ContextMenuOption.Enable:
                    UpdateState(_rows[input[2]], State.Enabled);
                    break;

                case ContextMenuOption.ForceDisable:
                    UpdateState(_rows[input[2]], State.ForceDisabled);
                    break;

                case ContextMenuOption.ForceEnable:
                    UpdateState(_rows[input[2]], State.ForceEnabled);
                    break;

                case ContextMenuOption.DisableSelected:
                    foreach (string rowId in input.Skip(2).ToArray())
                    {
                        if (!_rows.ContainsKey(rowId))
                            throw new ArgumentException($"Row key [{rowId}] doesn't exist in the Polling Manager table!");

                        UpdateState(_rows[rowId], State.ForceDisabled);
                    }

                    break;

                case ContextMenuOption.EnableSelected:
                    foreach (string rowId in input.Skip(2).ToArray())
					{
						if (!_rows.ContainsKey(rowId))
							throw new ArgumentException($"Row key [{rowId}] doesn't exist in the Polling Manager table!");

						UpdateState(_rows[rowId], State.ForceEnabled);
					}

                    break;

                case ContextMenuOption.DisableAll:
                    foreach (KeyValuePair<string, PollableBase> row in _rows)
                    {
                        if (row.Value.State != State.Disabled)
                            UpdateState(row.Value, State.ForceDisabled);
                    }

                    break;

                case ContextMenuOption.EnableAll:
                    foreach (KeyValuePair<string, PollableBase> row in _rows)
					{
						if (row.Value.State != State.Enabled)
							UpdateState(row.Value, State.ForceEnabled);
					}

                    break;

                default:
                    throw new ArgumentException($"Unhandled ContextMenuOption: {option}!");
            }

            FillTableNoDelete(_rows);
        }

        /// <summary>
        /// Checks whether option with row keys was selected in context menu.
        /// </summary>
        /// <param name="option">Context menu option.</param>
        /// <returns>True if option with row keys was selected, false otherwise.</returns>
        private bool HasRowKeys(ContextMenuOption option)
		{
			switch (option)
            {
                case ContextMenuOption.PollAll:
                    return false;

                case ContextMenuOption.DisableAll:
                    return false;

                case ContextMenuOption.EnableAll:
                    return false;

                default:
                    return true;
            }
		}

		/// <summary>
		/// Polls all rows by calling <see cref="PollRow"/> for every row.
		/// </summary>
        private void PollAll()
        {
            foreach (KeyValuePair<string, PollableBase> row in _rows)
            {
                PollRow(row.Value);
            }
        }

		/// <summary>
		/// Updates row state.
		/// </summary>
		/// <param name="row">Row to update.</param>
		/// <param name="state">State to update to.</param>
        private void UpdateState(IPollable row, State state)
        {
            switch (state)
            {
                case State.Disabled:
                    if (row.Children.Any(child => child.State == State.Enabled))
                    {
                        ShowChildren(row);
                        return;
                    }

                    row.State = State.Disabled;
                    row.Status = Status.Disabled;
                    return;

                case State.Enabled:
                    if (row.Parents.Any(parent => parent.State == State.Disabled))
                    {
                        ShowParents(row);
                        return;
                    }

                    row.State = State.Enabled;
                    row.Status = Status.NotPolled;
                    return;

                case State.ForceDisabled:
                    row.State = State.Disabled;
                    row.Status = Status.Disabled;
                    UpdateRelatedStates(row.Children, State.ForceDisabled);
                    return;

                case State.ForceEnabled:
                    row.State = State.Enabled;
                    row.Status = Status.NotPolled;
                    UpdateRelatedStates(row.Parents, State.ForceEnabled);
                    return;
            }
        }

        /// <summary>
        /// Shows information message with child rows of the row passed as parameter.
        /// </summary>
        /// <param name="row">Row for which to show children.</param>
        private void ShowChildren(IPollable row)
        {
            string children = string.Join("\n", row.Children.Where(child => child.State == State.Enabled).Select(child => child.Name));

            string message = $"Unable to disable [{row.Name}] because the following rows are dependand on it:\n{children}\nPlease disable them first or use [Force Disable].";

            Protocol.ShowInformationMessage(message);
        }

		/// <summary>
		/// Shows information message with parent rows of the row passed as parameter.
		/// </summary>
		/// <param name="row">Row for which to show parents.</param>
        private void ShowParents(IPollable row)
        {
			string parents = string.Join("\n", row.Parents.Where(parent => parent.State == State.Disabled).Select(parent => parent.Name));

			string message = $"Unable to enable [{row.Name}] because it depends on the following rows:\n{parents}\nPlease enable them first or use [Force Enable].";

			Protocol.ShowInformationMessage(message);
        }

        /// <summary>
        /// Updates states of the related rows.
        /// </summary>
        /// <param name="collection">List of rows to update.</param>
        /// <param name="state">State to update rows to.</param>
        private void UpdateRelatedStates(List<IPollable> collection, State state)
        {
            foreach (IPollable item in collection)
            {
                UpdateState(item, state);
            }
        }

        /// <summary>
        /// Polls a row.
        /// </summary>
        /// <param name="row">Row to poll.</param>
        private void PollRow(PollableBase row)
        {
            if (row.State == State.Disabled)
                return;

            if (!row.CheckDependencies())
                return;

            bool pollSucceeded = row.Poll();
            row.LastPoll = DateTime.Now;

            if (pollSucceeded)
                row.Status = Status.Succeeded;
            else
                row.Status = Status.Failed;
        }

		/// <summary>
		/// Updates internal representation of the row.
		/// </summary>
		/// <param name="rowId">Row key.</param>
		/// <param name="newValue">Values to update the row to.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="rowId"/> doesn't exist in the table.</exception>
        private void UpdateInternalRow(string rowId, PollableBase newValue)
        {
			if (!_rows.ContainsKey(rowId))
				throw new ArgumentException($"Row key [{rowId}] doesn't exist in the Polling Manager table!");

			_rows[rowId].Name = newValue.Name;
			_rows[rowId].Period = newValue.Period;
			_rows[rowId].DefaultPeriod = newValue.DefaultPeriod;
			_rows[rowId].PeriodType = newValue.PeriodType;
			_rows[rowId].LastPoll = newValue.LastPoll;
			_rows[rowId].Status = newValue.Status;
			_rows[rowId].Reason = newValue.Reason;
			_rows[rowId].State = newValue.State;
			_rows[rowId].Parents = newValue.Parents;
			_rows[rowId].Children = newValue.Children;
        }

        /// <summary>
        /// Checks whether poll period has elapsed.
        /// </summary>
        /// <param name="period">Poll period.</param>
        /// <param name="lastPoll">Last poll timestamp.</param>
        /// <returns>True if poll period has elapsed, false otherwise.</returns>
        private bool CheckLastPollTime(double period, DateTime lastPoll)
        {
            if ((DateTime.Now - lastPoll).TotalSeconds > period)
                return true;

            return false;
        }

		/// <summary>
		/// Sets the content of the table to the provided content.
		/// </summary>
		/// <param name="rows">Rows to fill the table with.</param>
        private void FillTable(Dictionary<string, PollableBase> rows)
        {
            PollingmanagerQActionRow[] tableRows = CreateTableRows(rows);

            _table.FillArray(tableRows);
        }

		/// <summary>
		/// Adds the provided row to the table.
		/// </summary>
		/// <param name="key">Row key.</param>
		/// <param name="value">Row to add.</param>
        private void FillTableNoDelete(string key, PollableBase value)
        {
            FillTableNoDelete(new Dictionary<string, PollableBase> { { key, value } });
        }

        /// <summary>
        /// Add the provided rows to the table.
        /// </summary>
        /// <param name="rows">Rows to add to the table.</param>
        private void FillTableNoDelete(Dictionary<string, PollableBase> rows)
        {
            PollingmanagerQActionRow[] tableRows = CreateTableRows(rows);

            _table.FillArrayNoDelete(tableRows);
        }

        /// <summary>
        /// Creates the <see cref="PollingmanagerQActionRow"/>.
        /// </summary>
        /// <param name="key">Row key.</param>
        /// <param name="value">Row to create.</param>
        /// <returns>Instance of <see cref="PollingmanagerQActionRow"/>.</returns>
        private PollingmanagerQActionRow CreateTableRow(string key, PollableBase value)
        {
            return new PollingmanagerQActionRow
            {
                Pollingmanagerindex_1001 = key,
                Pollingmanagername_1002 = value.Name,
                Pollingmanagerperiod_1003 = value.PeriodType == PeriodType.Custom ? value.Period : value.DefaultPeriod,
                Pollingmanagerdefaultperiod_1004 = value.DefaultPeriod,
                Pollingmanagerperiodtype_1005 = value.PeriodType,
                Pollingmanagerlastpoll_1006 = value.LastPoll == default ? Convert.ToDouble(Status.NotPolled) : value.LastPoll.ToOADate(),
                Pollingmanagerstatus_1007 = value.State == State.Disabled ? Status.Disabled : value.Status,
                Pollingmanagerreason_1008 = value.Reason,
                Pollingmanagerstate_1010 = value.State,
            };
        }

        /// <summary>
        /// Creates the array of the <see cref="PollingmanagerQActionRow"/>.
        /// </summary>
        /// <param name="rows">Rows to create.</param>
        /// <returns>Array of the <see cref="PollingmanagerQActionRow"/>.</returns>
        private PollingmanagerQActionRow[] CreateTableRows(Dictionary<string, PollableBase> rows)
        {
            List<PollingmanagerQActionRow> tableRows = new List<PollingmanagerQActionRow>();

            foreach (KeyValuePair<string, PollableBase> row in rows)
            {
                tableRows.Add(CreateTableRow(row.Key, row.Value));
            }

            return tableRows.ToArray();
        }

		/// <summary>
		/// Creates an object that implements <see cref="PollableBase"/> by using <see cref="IPollableBaseFactory"/> while preserving existing relations.
		/// </summary>
		/// <param name="rowId">Row key of the row to serialize.</param>
		/// <returns>Instance of the object that implements <see cref="PollableBase"/>.</returns>
		/// <exception cref="ArgumentException">Throws if <paramref name="rowId"/> doesn't exist in the table.</exception>
        private PollableBase CreateIPollable(string rowId)
        {
			if (!_rows.ContainsKey(rowId))
				throw new ArgumentException($"Row key [{rowId}] doesn't exist in the Polling Manager table!");

			object[] tableRow = _table.GetRow(rowId);

			PollableBase row = _pollableFactory.CreatePollableBase(Protocol, tableRow, _rows[rowId].GetType());

			row.Parents = _rows[rowId].Parents;
			row.Children = _rows[rowId].Children;
			row.Dependencies = _rows[rowId].Dependencies;

			return row;
        }
    }
}
