using System;
using System.Collections.Generic;
using Eto.Forms;
using MetalTracker.Common.Types;
using MetalTracker.Games.Zelda.Types;

namespace MetalTracker.Games.Zelda.Internal
{
	internal class DungeonRoomDetail
	{
		private readonly Panel _detailPanel;
		private readonly DungeonRoomStateMutator _mutator;

		private StackLayout _mainLayout;
		private DropDown _dropDownDestNorth;
		private DropDown _dropDownDestSouth;
		private DropDown _dropDownDestWest;
		private DropDown _dropDownDestEast;
		private DropDown _dropDownItem;

		private List<GameDest> _gameDests;
		private List<GameItem> _gameItems;

		private int _w;
		private int _x;
		private int _y;
		private DungeonRoomProps _props;
		private DungeonRoomState _state;

		private bool _refreshing;

		public event EventHandler DetailChanged;

		public DungeonRoomDetail(Panel detailPanel, DungeonRoomStateMutator mutator)
		{
			_detailPanel = detailPanel;
			_mutator = mutator;
		}

		public void Build(List<GameDest> gameDests, List<GameItem> gameItems)
		{
			_gameDests = gameDests;
			_gameItems = gameItems;

			_mainLayout = new StackLayout { Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Center };

			#region Destinations 

			_mainLayout.Items.Add(new Label { Text = "Destinations" });

			_dropDownDestNorth = new DropDown();
			_dropDownDestSouth = new DropDown();
			_dropDownDestWest = new DropDown();
			_dropDownDestEast = new DropDown();

			_dropDownDestNorth.Items.Add(null);
			_dropDownDestSouth.Items.Add(null);
			_dropDownDestWest.Items.Add(null);
			_dropDownDestEast.Items.Add(null);

			foreach (var gameDest in gameDests)
			{
				ListItem listItem = new ListItem
				{
					Key = gameDest.GetCode(),
					Text = gameDest.LongName,
				};

				_dropDownDestNorth.Items.Add(listItem);
				_dropDownDestSouth.Items.Add(listItem);
				_dropDownDestWest.Items.Add(listItem);
				_dropDownDestEast.Items.Add(listItem);
			}

			_dropDownDestNorth.SelectedIndexChanged += HandleSelectedDestNorthChanged;
			_dropDownDestSouth.SelectedIndexChanged += HandleSelectedDestSouthChanged;
			_dropDownDestWest.SelectedIndexChanged += HandleSelectedDestWestChanged;
			_dropDownDestEast.SelectedIndexChanged += HandleSelectedDestEastChanged;

			TableLayout destsLayout = new TableLayout(
				new TableRow(new TableCell(), new TableCell(_dropDownDestNorth), new TableCell()),
				new TableRow(new TableCell(_dropDownDestWest), new TableCell(), new TableCell(_dropDownDestEast)),
				new TableRow(new TableCell(), new TableCell(_dropDownDestSouth), new TableCell())
			);

			_mainLayout.Items.Add(destsLayout);

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

			_mainLayout.Visible = false;

			_detailPanel.Content = _mainLayout;
		}

		private void HandleSelectedDestNorthChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestNorth(_w, _x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestSouthChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestSouth(_w, _x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestWestChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestWest(_w, _x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestEastChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestEast(_w, _x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItemChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = _gameItems.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeItem(_w, _x, _y, _state, gameItem);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		public void UpdateDetails(int w, int x, int y, DungeonRoomProps props, DungeonRoomState state)
		{
			_w = w;
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

			if (_props.CanHaveItem() || _props.CanHaveDest())
			{
				_mainLayout.Visible = true;

				_dropDownDestNorth.SelectedKey = _state.DestNorth?.GetCode();
				_dropDownDestSouth.SelectedKey = _state.DestSouth?.GetCode();
				_dropDownDestWest.SelectedKey = _state.DestWest?.GetCode();
				_dropDownDestEast.SelectedKey = _state.DestEast?.GetCode();
				_dropDownItem.SelectedKey = _state.Item?.GetCode();

				_dropDownDestNorth.Enabled = _props.DestNorth;
				_dropDownDestSouth.Enabled = _props.DestSouth;
				_dropDownDestWest.Enabled = _props.DestWest;
				_dropDownDestEast.Enabled = _props.DestEast;
				_dropDownItem.Enabled = _props.CanHaveItem();
			}
			else
			{
				_mainLayout.Visible = false;
			}

			_refreshing = false;
		}
	}
}
