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
		private DropDown _dropDownStatus;

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

		public void Build(IReadOnlyList<GameExit> gameExits, IReadOnlyList<GameItem> gameItems)
		{
			_mainLayout = new StackLayout { Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Center };

			#region Destination

			_mainLayout.Items.Add(new Label { Text = "Destination" });

			_dropDownDest = new DropDown();

			_dropDownDest.Items.Add(null);

			foreach (var cave in OverworldResourceClient.GetCaves())
			{
				ListItem listItem = new ListItem
				{
					Key = $"cave|{cave.Key}",
					Text = cave.LongName,
					Tag = cave,
				};

				_dropDownDest.Items.Add(listItem);
			}

			foreach (var gameExit in gameExits)
			{
				ListItem listItem = new ListItem
				{
					Key = $"exit|{gameExit.GetCode()}",
					Text = gameExit.LongName,
					Tag = gameExit,
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
					Tag = gameItem,
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

			#region Actions

			_mainLayout.Items.Add(new Label { Text = "Room Status" });

			_dropDownStatus = new DropDown();

			_dropDownStatus.Items.Add("");
			_dropDownStatus.Items.Add("Explored");
			_dropDownStatus.Items.Add("Ignored");

			_dropDownStatus.SelectedIndexChanged += HandleRoomStatusChanged;

			_mainLayout.Items.Add(_dropDownStatus);

			#endregion

			_mainLayout.Visible = false;

			_detailPanel.Content = _mainLayout;
		}

		private void HandleSelectedDestChanged(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;

			if (listItem.Key == null)
			{
				_mutator.ChangeCave(_x, _y, _state, null);
				_mutator.ChangeExit(_x, _y, _state, null);
			}
			else if (listItem.Tag is GameExit gameExit)
			{
				_mutator.ChangeCave(_x, _y, _state, null);
				_mutator.ChangeExit(_x, _y, _state, gameExit);
			}
			else if (listItem.Tag is OverworldCave cave)
			{
				_mutator.ChangeExit(_x, _y, _state, null);
				_mutator.ChangeCave(_x, _y, _state, cave);
			}

			Refresh();
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItem1Changed(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = listItem.Tag as GameItem;
			_mutator.ChangeItem1(_x, _y, _state, gameItem);
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItem2Changed(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = listItem.Tag as GameItem;
			_mutator.ChangeItem2(_x, _y, _state, gameItem);
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleSelectedItem3Changed(object sender, EventArgs e)
		{
			var listItem = (sender as DropDown).SelectedValue as ListItem;
			var gameItem = listItem.Tag as GameItem;
			_mutator.ChangeItem3(_x, _y, _state, gameItem);
			DetailChanged?.Invoke(this, EventArgs.Empty);
		}

		private void HandleRoomStatusChanged(object sender, EventArgs e)
		{
			_state.Status = _dropDownStatus.SelectedIndex;
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

			if (_state.Cave != null)
			{
				_dropDownDest.SelectedKey = $"cave|{_state.Cave.Key}";
			}
			else if (_state.Exit != null)
			{
				_dropDownDest.SelectedKey = $"exit|{_state.Exit.GetCode()}";
			}
			else
			{
				_dropDownDest.SelectedIndex = 0;
			}

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

				if (_state.Cave == null)
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
					if (_state.Cave.ItemSlots == 0)
					{
						_dropDownItem1.Enabled = false;
						_dropDownItem2.Enabled = false;
						_dropDownItem3.Enabled = false;
					}
					else if (_state.Cave.ItemSlots == 1)
					{
						_dropDownItem1.Enabled = false;
						_dropDownItem2.Enabled = true;
						_dropDownItem3.Enabled = false;
					}
					else if (_state.Cave.ItemSlots == 2)
					{
						_dropDownItem1.Enabled = true;
						_dropDownItem2.Enabled = false;
						_dropDownItem3.Enabled = true;

					}
					else if (_state.Cave.ItemSlots == 3)
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

			_dropDownStatus.SelectedIndex = _state.Status;

			_refreshing = false;
		}
	}
}
