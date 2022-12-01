using System;
using System.Collections.Generic;
using Eto.Forms;
using MetalTracker.Common.Types;
using MetalTracker.Games.Metroid.Types;

namespace MetalTracker.Games.Metroid.Internal
{
	internal class ZebesRoomDetail
	{
		private readonly Panel _detailPanel;
		private readonly ZebesRoomStateMutator _mutator;

		StackLayout _mainLayout;
		DropDown _dropDownDestElev;
		DropDown _dropDownItem;

		List<GameDest> _gameDests;
		List<GameItem> _gameItems;

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

		public void Build(List<GameDest> gameDests, List<GameItem> gameItems)
		{
			_gameDests = gameDests;
			_gameItems = gameItems;

			_mainLayout = new StackLayout { Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Center };

			#region Exit

			_mainLayout.Items.Add(new Label { Text = "Exit" });

			_dropDownDestElev = new DropDown();

			_dropDownDestElev.Items.Add(null);

			foreach (var gameDest in gameDests)
			{
				ListItem listItem = new ListItem
				{
					Key = gameDest.GetCode(),
					Text = gameDest.LongName,
				};

				_dropDownDestElev.Items.Add(listItem);
			}

			_dropDownDestElev.SelectedIndexChanged += HandleSelectedDestElevChanged;

			_mainLayout.Items.Add(_dropDownDestElev);

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
					Text = gameItem.Name,
				};

				_dropDownItem.Items.Add(listItem);
			}

			itemsLayout.Items.Add(_dropDownItem);

			_mainLayout.Items.Add(itemsLayout);

			#endregion

			//_mainLayout.Visible = false;

			_detailPanel.Content = _mainLayout;
		}

		private void HandleSelectedDestElevChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestination(_x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItemChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = _gameItems.Find(d => d.GetCode() == listItem.Key);
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

			if (_props.CanHaveDest())
			{
				_dropDownDestElev.Enabled = true;
				_dropDownDestElev.SelectedKey = _state.DestElev?.GetCode();
			}
			else
			{
				_dropDownDestElev.Enabled = false;
				_dropDownDestElev.SelectedKey = null;
			}

			if (_props.CanHaveItem())
			{
				_dropDownItem.Enabled = true;
				_dropDownItem.SelectedKey = _state.Item?.GetCode();
			}
			else
			{
				_dropDownItem.Enabled = false;
				_dropDownItem.SelectedKey = null;
			}

			_refreshing = false;
		}
	}
}
