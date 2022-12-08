using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using MetalTracker.Common.Types;
using MetalTracker.Games.Zelda.Internal.Types;

namespace MetalTracker.Games.Zelda.Internal
{
	internal class DungeonRoomDetail
	{
		private readonly Panel _detailPanel;
		private readonly DungeonRoomStateMutator _mutator;

		private StackLayout _mainLayout;

		private Panel _panelNorth;
		private Panel _panelSouth;
		private Panel _panelWest;
		private Panel _panelEast;

		private DropDown _dropDownDestNorth;
		private DropDown _dropDownDestSouth;
		private DropDown _dropDownDestWest;
		private DropDown _dropDownDestEast;

		private DropDown _dropDownWallNorth;
		private DropDown _dropDownWallSouth;
		private DropDown _dropDownWallWest;
		private DropDown _dropDownWallEast;

		private TableLayout _dirsLayout;

		private DropDown _dropDownItem1;
		private DropDown _dropDownItem2;
		private DropDown _dropDownTransports;

		private List<GameDest> _gameDests = new List<GameDest>();
		private List<GameItem> _gameItems;
		private List<DungeonWall> _walls;

		private int _level;
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

			_walls = DungeonResourceClient.GetDungeonWalls();

			_mainLayout = new StackLayout { Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Center };

			#region Walls / Exits

			_mainLayout.Items.Add(new Label { Text = "Walls / Exits" });

			_panelNorth = new Panel();
			_panelSouth = new Panel();
			_panelWest = new Panel();
			_panelEast = new Panel();

			_dropDownWallNorth = new DropDown { Width = 120 };
			_dropDownWallSouth = new DropDown { Width = 120 };
			_dropDownWallWest = new DropDown { Width = 120 };
			_dropDownWallEast = new DropDown { Width = 120 };

			_dropDownWallNorth.Items.Add(null);
			_dropDownWallSouth.Items.Add(null);
			_dropDownWallWest.Items.Add(null);
			_dropDownWallEast.Items.Add(null);

			_dropDownWallNorth.SelectedIndexChanged += HandleSelectedWallNorthChanged;
			_dropDownWallSouth.SelectedIndexChanged += HandleSelectedWallSouthChanged;
			_dropDownWallWest.SelectedIndexChanged += HandleSelectedWallWestChanged;
			_dropDownWallEast.SelectedIndexChanged += HandleSelectedWallEastChanged;

			foreach (var wall in _walls)
			{
				ListItem listItem = new ListItem
				{
					Key = wall.Code,
					Text = wall.Name
				};

				_dropDownWallNorth.Items.Add(listItem);
				_dropDownWallSouth.Items.Add(listItem);
				_dropDownWallWest.Items.Add(listItem);
				_dropDownWallEast.Items.Add(listItem);
			}

			_dropDownDestNorth = new DropDown { Width = 120 };
			_dropDownDestSouth = new DropDown { Width = 120 };
			_dropDownDestWest = new DropDown { Width = 120 };
			_dropDownDestEast = new DropDown { Width = 120 };

			_dropDownDestNorth.Items.Add(null);
			_dropDownDestSouth.Items.Add(null);
			_dropDownDestWest.Items.Add(null);
			_dropDownDestEast.Items.Add(null);

			_dropDownDestNorth.SelectedIndexChanged += HandleSelectedDestNorthChanged;
			_dropDownDestSouth.SelectedIndexChanged += HandleSelectedDestSouthChanged;
			_dropDownDestWest.SelectedIndexChanged += HandleSelectedDestWestChanged;
			_dropDownDestEast.SelectedIndexChanged += HandleSelectedDestEastChanged;

			_dirsLayout = new TableLayout(
				new TableRow(new TableCell(), new TableCell(_panelNorth), new TableCell()),
				new TableRow(new TableCell(_panelWest), new TableCell(), new TableCell(_panelEast)),
				new TableRow(new TableCell(), new TableCell(_panelSouth), new TableCell())
			);

			_mainLayout.Items.Add(_dirsLayout);

			#endregion

			#region Items Layout

			_mainLayout.Items.Add(new Label { Text = "Items" });

			var itemsLayout = new StackLayout { Orientation = Orientation.Horizontal, VerticalContentAlignment = VerticalAlignment.Center };

			_dropDownItem1 = new DropDown { Height = 25 };
			_dropDownItem2 = new DropDown { Height = 25 };

			_dropDownItem1.SelectedIndexChanged += HandleSelectedItem1Changed;
			_dropDownItem2.SelectedIndexChanged += HandleSelectedItem2Changed;

			itemsLayout.Items.Add(_dropDownItem1);
			itemsLayout.Items.Add(_dropDownItem2);

			_mainLayout.Items.Add(itemsLayout);

			#endregion

			#region Transports

			_mainLayout.Items.Add(new Label { Text = "Transport" });

			_dropDownTransports = new DropDown { Height = 25 };

			_dropDownTransports.SelectedIndexChanged += HandleSelectedStairChanged;

			_mainLayout.Items.Add(_dropDownTransports);

			#endregion

			_mainLayout.Visible = false;
		}

		public void PopulateItems(List<GameItem> gameItems)
		{
			_gameItems = gameItems;

			_dropDownItem1.Items.Add(null);
			_dropDownItem2.Items.Add(null);

			foreach (var gameItem in gameItems)
			{
				ImageListItem listItem = new ImageListItem
				{
					Key = gameItem.GetCode(),
					Image = gameItem.Icon,
				};

				_dropDownItem1.Items.Add(listItem);
				_dropDownItem2.Items.Add(listItem);
			}
		}

		public void AddDest(GameDest gameDest)
		{
			_gameDests.Add(gameDest);

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

		public void SetTransports(int numTransports)
		{
			_dropDownTransports.Items.Clear();

			_dropDownTransports.Items.Add(new ListItem { Key = null, Text = null });

			for (int i = 0; i < numTransports; i++)
			{
				string k = ((char)('A' + i)).ToString();

				ListItem listItem = new ListItem
				{
					Key = k,
					Text = k,
				};

				_dropDownTransports.Items.Add(listItem);
			}
		}

		public void Activate()
		{
			_detailPanel.Content = _mainLayout;
		}

		public void UpdateDetails(int level, int x, int y, DungeonRoomProps props, DungeonRoomState state)
		{
			_level = level;
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

			if (_props != null)
			{
				_mainLayout.Visible = true;

				_dropDownDestNorth.SelectedKey = _state.DestNorth?.GetCode();
				_dropDownDestSouth.SelectedKey = _state.DestSouth?.GetCode();
				_dropDownDestWest.SelectedKey = _state.DestWest?.GetCode();
				_dropDownDestEast.SelectedKey = _state.DestEast?.GetCode();

				_dropDownWallNorth.SelectedKey = _state.WallNorth?.Code;
				_dropDownWallSouth.SelectedKey = _state.WallSouth?.Code;
				_dropDownWallWest.SelectedKey = _state.WallWest?.Code;
				_dropDownWallEast.SelectedKey = _state.WallEast?.Code;

				_dropDownItem1.SelectedKey = _state.Item1?.GetCode();
				_dropDownItem2.SelectedKey = _state.Item2?.GetCode();
				_dropDownTransports.SelectedKey = _state.Transport;

				var dropNorth = _props.DestNorth ? _dropDownDestNorth : _dropDownWallNorth;
				var dropSouth = _props.DestSouth ? _dropDownDestSouth : _dropDownWallSouth;
				var dropWest = _props.DestWest ? _dropDownDestWest : _dropDownWallWest;
				var dropEast = _props.DestEast ? _dropDownDestEast : _dropDownWallEast;

				_panelNorth.Content = dropNorth;
				_panelSouth.Content = dropSouth;
				_panelWest.Content = dropWest;
				_panelEast.Content = dropEast;

				_dropDownItem1.Enabled = _props.Shuffled || _props.CanHaveItem1();
				_dropDownItem2.Enabled = _props.Shuffled || _props.CanHaveItem2();
				_dropDownTransports.Enabled = _props.Shuffled || _props.HasStairs;
			}
			else
			{
				_mainLayout.Visible = false;
			}

			_refreshing = false;
		}

		private void HandleSelectedDestNorthChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestNorth(_level, _x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestSouthChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestSouth(_level, _x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestWestChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestWest(_level, _x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedDestEastChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestEast(_level, _x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedWallNorthChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var wall = _walls.Find(d => d.Code == listItem.Key);
			_mutator.ChangeWallNorth(_level, _x, _y, _state, wall);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedWallSouthChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var wall = _walls.Find(d => d.Code == listItem.Key);
			_mutator.ChangeWallSouth(_level, _x, _y, _state, wall);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedWallWestChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var wall = _walls.Find(d => d.Code == listItem.Key);
			_mutator.ChangeWallWest(_level, _x, _y, _state, wall);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedWallEastChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var wall = _walls.Find(d => d.Code == listItem.Key);
			_mutator.ChangeWallEast(_level, _x, _y, _state, wall);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItem1Changed(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = _gameItems.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeItem1(_level, _x, _y, _state, gameItem);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItem2Changed(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = _gameItems.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeItem2(_level, _x, _y, _state, gameItem);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedStairChanged(object sender, EventArgs e)
		{
			if (_refreshing) return;
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			string transport = listItem?.Key;
			_mutator.ChangeTransport(_level, _x, _y, _state, transport);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
