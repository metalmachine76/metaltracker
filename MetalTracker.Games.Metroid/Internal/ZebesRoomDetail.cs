using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using MetalTracker.Common.Types;
using MetalTracker.Games.Metroid.Internal.Types;

namespace MetalTracker.Games.Metroid.Internal
{
    internal class ZebesRoomDetail
	{
		private readonly Panel _detailPanel;
		private readonly ZebesRoomStateMutator _mutator;

		StackLayout _mainLayout;
		DropDown _dropDownDestElevUp;
		DropDown _dropDownDestElevDown;
		DropDown _dropDownDestExitLeft;
		DropDown _dropDownDestExitRight;
		DropDown _dropDownItem;

		IReadOnlyList<GameExit> _gameDests;
		IReadOnlyList<GameItem> _gameItems;

		private int _x;
		private int _y;
		private ZebesRoomProps _props;
		private ZebesRoomState _state;

		private bool _refreshing;

		public event EventHandler DetailChanged;

		public ZebesRoomDetail(Panel detailPanel, ZebesRoomStateMutator mutator)
		{
			_detailPanel = detailPanel;
			_mutator = mutator;
		}

		public void Build(IReadOnlyList<GameExit> gameDests, IReadOnlyList<GameItem> gameItems)
		{
			_gameDests = gameDests;
			_gameItems = gameItems;

			_mainLayout = new StackLayout { Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Center };

			#region Exit

			_mainLayout.Items.Add(new Label { Text = "Exit" });

			_dropDownDestElevUp = new DropDown();
			_dropDownDestElevDown = new DropDown();
			_dropDownDestExitLeft = new DropDown();
			_dropDownDestExitRight = new DropDown();

			_dropDownDestElevUp.Items.Add(null);
			_dropDownDestElevDown.Items.Add(null);
			_dropDownDestExitLeft.Items.Add(null);
			_dropDownDestExitRight.Items.Add(null);

			foreach (var gameDest in gameDests)
			{
				ListItem listItem = new ListItem
				{
					Key = gameDest.GetCode(),
					Text = gameDest.LongName,
				};

				_dropDownDestElevUp.Items.Add(listItem);
				_dropDownDestElevDown.Items.Add(listItem);
				_dropDownDestExitLeft.Items.Add(listItem);
				_dropDownDestExitRight.Items.Add(listItem);
			}

			_dropDownDestElevUp.SelectedIndexChanged += HandleSelectedDestUpChanged;
			_dropDownDestElevDown.SelectedIndexChanged += HandleSelectedDestDownChanged;
			_dropDownDestExitLeft.SelectedIndexChanged += HandleSelectedDestLeftChanged;
			_dropDownDestExitRight.SelectedIndexChanged += HandleSelectedDestRightChanged;

			var _exitsLayout = new TableLayout(
				new TableRow(new TableCell(), new TableCell(_dropDownDestElevUp), new TableCell()),
				new TableRow(new TableCell(_dropDownDestExitLeft), new TableCell(), new TableCell(_dropDownDestExitRight)),
				new TableRow(new TableCell(), new TableCell(_dropDownDestElevDown), new TableCell())
			);

			_mainLayout.Items.Add(_exitsLayout);

			#endregion

			#region Items Layout

			_mainLayout.Items.Add(new Label { Text = "Item" });

			var itemsLayout = new StackLayout { Orientation = Orientation.Horizontal, VerticalContentAlignment = VerticalAlignment.Center };

			_dropDownItem = new DropDown { Height = 25 };

			_dropDownItem.SelectedIndexChanged += HandleSelectedItemChanged;

			_dropDownItem.Items.Add(null);

			foreach (var gameItem in gameItems)
			{
				ImageListItem listItem = new ImageListItem
				{
					Key = gameItem.GetCode(),
					Image = gameItem.Icon,
				};

				_dropDownItem.Items.Add(listItem);
			}

			itemsLayout.Items.Add(_dropDownItem);

			_mainLayout.Items.Add(itemsLayout);

			#endregion

			_detailPanel.Content = _mainLayout;
		}

		private void HandleSelectedDestUpChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.FirstOrDefault(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestUp(_x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestDownChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.FirstOrDefault(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestDown(_x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestLeftChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.FirstOrDefault(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestLeft(_x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestRightChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.FirstOrDefault(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestRight(_x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItemChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = _gameItems.FirstOrDefault(d => d.GetCode() == listItem.Key);
			_mutator.ChangeItem(_x, _y, _state, gameItem);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		public void UpdateDetails(int x, int y, ZebesRoomProps props, ZebesRoomState state)
		{
			_x = x;
			_y = y;
			_props = props;
			_state = state;
			Refresh();
		}

		public void Refresh()
		{
			if (_refreshing) return;

			_refreshing = true;

			if (_props.CanHaveDest() || _props.CanHaveItem() || _props.Shuffled)
			{
				_mainLayout.Visible = true;

				_dropDownDestElevUp.SelectedKey = _state.ExitUp?.GetCode();
				_dropDownDestElevDown.SelectedKey = _state.ExitDown?.GetCode();
				_dropDownDestExitLeft.SelectedKey = _state.ExitLeft?.GetCode();
				_dropDownDestExitRight.SelectedKey = _state.ExitRight?.GetCode();

				_dropDownItem.SelectedKey = _state.Item?.GetCode();

				_dropDownDestElevUp.Enabled = _props.ElevatorUp;
				_dropDownDestElevDown.Enabled = _props.ElevatorDown;
				_dropDownDestExitLeft.Enabled = _props.CanExitLeft;
				_dropDownDestExitRight.Enabled = _props.CanExitRight;

				_dropDownItem.Enabled = _props.CanHaveItem() || _props.Shuffled;
			}
			else
			{
				_mainLayout.Visible = false;
			}

			_refreshing = false;
		}
	}
}
