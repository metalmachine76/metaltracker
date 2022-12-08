using System;
using System.Collections.Generic;
using Eto.Forms;
using MetalTracker.Common.Types;
using MetalTracker.Games.Zelda.Internal.Types;

namespace MetalTracker.Games.Zelda.Internal
{
	internal class OverworldRoomDetail
	{
		private readonly Panel _detailPanel;
		private readonly OverworldRoomStateMutator _mutator;

		private StackLayout _mainLayout;
		private DropDown _dropDownDest;
		private DropDown _dropDownItem1;
		private DropDown _dropDownItem2;
		private DropDown _dropDownItem3;

		private List<GameDest> _gameDests;
		private List<GameItem> _gameItems;

		private int _x;
		private int _y;
		private OverworldRoomProps _props;
		private OverworldRoomState _state;

		private bool _refreshing;

		public event EventHandler DetailChanged;

		public OverworldRoomDetail(Panel detailPanel, OverworldRoomStateMutator mutator)
		{
			_detailPanel = detailPanel;
			_mutator = mutator;
		}

		public void Build(List<GameDest> gameDests, List<GameItem> gameItems)
		{
			_gameDests = gameDests;
			_gameItems = gameItems;

			_mainLayout = new StackLayout { Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Center };

			#region Destination

			_mainLayout.Items.Add(new Label { Text = "Destination" });

			_dropDownDest = new DropDown();

			_dropDownDest.Items.Add(null);

			foreach (var gameDest in gameDests)
			{
				ListItem listItem = new ListItem
				{
					Key = gameDest.GetCode(),
					Text = gameDest.LongName,
				};

				_dropDownDest.Items.Add(listItem);
			}

			_dropDownDest.SelectedIndexChanged += HandleSelectedDestChanged;

			_mainLayout.Items.Add(_dropDownDest);

			#endregion

			#region Items Layout

			_mainLayout.Items.Add(new Label { Text = "Items" });

			var itemsLayout = new StackLayout { Orientation = Orientation.Horizontal, VerticalContentAlignment = VerticalAlignment.Center };

			_dropDownItem1 = new DropDown { Height = 25 };
			_dropDownItem2 = new DropDown { Height = 25 };
			_dropDownItem3 = new DropDown { Height = 25 };

			_dropDownItem1.SelectedIndexChanged += HandleSelectedItem1Changed;
			_dropDownItem2.SelectedIndexChanged += HandleSelectedItem2Changed;
			_dropDownItem3.SelectedIndexChanged += HandleSelectedItem3Changed;

			_dropDownItem1.Items.Add(null);
			_dropDownItem2.Items.Add(null);
			_dropDownItem3.Items.Add(null);

			foreach (var gameItem in gameItems)
			{
				ImageListItem listItem = new ImageListItem
				{
					Key = gameItem.GetCode(),
					Image = gameItem.Icon,
				};

				_dropDownItem1.Items.Add(listItem);
				_dropDownItem2.Items.Add(listItem);
				_dropDownItem3.Items.Add(listItem);
			}

			itemsLayout.Items.Add(_dropDownItem1);
			itemsLayout.Items.Add(_dropDownItem2);
			itemsLayout.Items.Add(_dropDownItem3);

			_mainLayout.Items.Add(itemsLayout);

			#endregion

			_mainLayout.Visible = false;

			_detailPanel.Content = _mainLayout;
		}

		private void HandleSelectedDestChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameDest = _gameDests.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeDestination(_x, _y, _state, gameDest);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItem1Changed(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = _gameItems.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeItem1(_x, _y, _state, gameItem);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItem2Changed(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = _gameItems.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeItem2(_x, _y, _state, gameItem);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItem3Changed(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = _gameItems.Find(d => d.GetCode() == listItem.Key);
			_mutator.ChangeItem3(_x, _y, _state, gameItem);
			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		public void UpdateDetails(int x, int y, OverworldRoomProps props, OverworldRoomState state)
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

			_dropDownDest.SelectedKey = _state.Destination?.GetCode();
			_dropDownItem1.SelectedKey = _state.Item1?.GetCode();
			_dropDownItem2.SelectedKey = _state.Item2?.GetCode();
			_dropDownItem3.SelectedKey = _state.Item3?.GetCode();

			if (_props.ItemHere)
			{
				_mainLayout.Visible = true;

				_dropDownDest.Enabled = false;

				_dropDownItem1.Visible = false;
				_dropDownItem2.Visible = true;
				_dropDownItem3.Visible = false;

				_dropDownItem1.Enabled = false;
				_dropDownItem2.Enabled = true;
				_dropDownItem3.Enabled = false;

			}
			else if (_props.DestHere)
			{
				_mainLayout.Visible = true;

				_dropDownDest.Enabled = true;

				_dropDownItem1.Visible = true;
				_dropDownItem2.Visible = true;
				_dropDownItem3.Visible = true;

				if (_state.Destination == null)
				{
					_dropDownItem1.Enabled = false;
					_dropDownItem2.Enabled = false;
					_dropDownItem3.Enabled = false;

					_dropDownItem1.SelectedKey = null;
					_dropDownItem2.SelectedKey = null;
					_dropDownItem3.SelectedKey = null;
				}
				else
				{
					if (_state.Destination.ItemSlots == 0)
					{
						_dropDownItem1.Enabled = false;
						_dropDownItem2.Enabled = false;
						_dropDownItem3.Enabled = false;
					}
					else if (_state.Destination.ItemSlots == 1)
					{
						_dropDownItem1.Enabled = false;
						_dropDownItem2.Enabled = true;
						_dropDownItem3.Enabled = false;
					}
					else if (_state.Destination.ItemSlots == 2)
					{
						_dropDownItem1.Enabled = true;
						_dropDownItem2.Enabled = false;
						_dropDownItem3.Enabled = true;

					}
					else if (_state.Destination.ItemSlots == 3)
					{
						_dropDownItem1.Enabled = true;
						_dropDownItem2.Enabled = true;
						_dropDownItem3.Enabled = true;
					}
				}
			}
			else
			{
				_mainLayout.Visible = false;
			}

			_refreshing = false;
		}
	}
}
