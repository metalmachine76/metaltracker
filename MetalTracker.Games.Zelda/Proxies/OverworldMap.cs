using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using MetalTracker.Common.Bases;
using MetalTracker.Common.Types;
using MetalTracker.CoOp.Interface;
using MetalTracker.Games.Zelda.Internal;
using MetalTracker.Games.Zelda.Internal.Types;
using MetalTracker.Games.Zelda.Types;

namespace MetalTracker.Games.Zelda.Proxies
{
    public class OverworldMap : BaseMap
	{
		const string Game = "zelda";
		const string Map = "ow";

		static SolidBrush ShadowBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 153));
		static SolidBrush CursorBrush = new SolidBrush(Color.FromArgb(250, 250, 250, 153));
		static Pen CurrentPen = new Pen(Colors.White, 2);

		private readonly OverworldRoomDetail _overworldRoomDetail;

		private bool _flag_q2;
		private bool _flag_shuffle_exits;
		private bool _flag_shuffle_caves;
		private bool _flag_mirrored;
		private Image _mapImage;
		private OverworldRoomProps[,] _meta;

		private List<GameDest> _dests = new List<GameDest>();
		private ContextMenu _destsMenu;
		private List<GameItem> _items = new List<GameItem>();
		private UITimer _timer;
		private OverworldRoomStateMutator _mutator = new OverworldRoomStateMutator();

		private bool _menuShowing;
		private bool _invalidateMap;
		private bool _invalidateRoom;

		private OverworldRoomState[,] _roomStates = new OverworldRoomState[8, 16];

		#region Public Methods

		public OverworldMap(Drawable drawable, Panel detailPanel) : base(64, 44, drawable)
		{
			_mw = 16;
			_mh = 8;

			_destsMenu = new ContextMenu();
			_destsMenu.Opening += HandleDestsMenuOpening;
			_destsMenu.Closed += HandleDestsMenuClosed;

			_overworldRoomDetail = new OverworldRoomDetail(detailPanel, _mutator);
			_overworldRoomDetail.DetailChanged += HandleRoomDetailChanged;
		}

		public void SetMapFlags(bool q2, bool shuffleExits, bool shuffleCaves, bool mirrored)
		{
			if (mirrored != _flag_mirrored)
			{
				MirrorState();
			}

			_flag_q2 = q2;
			_flag_mirrored = mirrored;
			_flag_shuffle_exits = shuffleExits;
			_flag_shuffle_caves = shuffleCaves;
			_mapImage = OverworldResourceClient.GetOverworldImage(q2, mirrored);
			_meta = OverworldResourceClient.GetOverworldMeta(q2, mirrored);
		}

		public void SetGameItems(IEnumerable<GameItem> gameItems)
		{
			_items = gameItems.ToList();
		}

		public void AddDestinations(IEnumerable<GameDest> destinations)
		{
			if (_destsMenu.Items.Count > 0)
			{
				_destsMenu.Items.AddSeparator();
			}

			foreach (var dest in destinations)
			{
				Command cmd = new Command();
				cmd.Executed += HandleDestCommand;
				cmd.CommandParameter = dest;
				_destsMenu.Items.Add(new ButtonMenuItem { Text = dest.LongName, Command = cmd });
				_dests.Add(dest);
			}
		}

		public void SetCoOpClient(ICoOpClient coOpClient)
		{
			coOpClient.Found += HandleCoOpClientFound;
			_mutator.SetCoOpClient(coOpClient);
			_timer = new UITimer();
			_timer.Interval = 0.5;
			_timer.Elapsed += HandleTimerElapsed;
			_timer.Start();
		}

		public void Activate(bool active)
		{
			_active = active;
			_drawable.Invalidate();
			if (active)
			{
				_overworldRoomDetail.Build(_dests, _items);
			}
		}

		public void ResetState()
		{
			var defaultState = OverworldResourceClient.GetDefaultOverworldState(
				_flag_q2,
				!_flag_shuffle_exits,
				!_flag_shuffle_caves,
				_flag_mirrored);

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					_roomStates[y, x] = defaultState[y, x];
				}
			}

			_drawable.Invalidate();
		}

		public override string GetMapKey()
		{
			return Map;
		}

		public OverworldMapState PersistState()
		{
			OverworldMapState mapState = new OverworldMapState();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					var roomState = _roomStates[y, x];

					// destination

					if (roomState.Destination != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = roomState.Destination.GetCode() };
						mapState.Dests.Add(entry);
					}

					// items

					if (roomState.Item1 != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = roomState.Item1.GetCode() };
						mapState.Items.Add(entry);
					}
					if (roomState.Item2 != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 1, Code = roomState.Item2.GetCode() };
						mapState.Items.Add(entry);
					}
					if (roomState.Item3 != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 2, Code = roomState.Item3.GetCode() };
						mapState.Items.Add(entry);
					}

					// explored

					if (roomState.Explored)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = "1" };
						mapState.Explored.Add(entry);
					}
				}
			}

			return mapState;
		}

		public void RestoreState(OverworldMapState mapState)
		{
			foreach (var entry in mapState.Dests)
			{
				_roomStates[entry.Y, entry.X].Destination = _dests.Find(i => i.GetCode() == entry.Code);
			}

			foreach (var entry in mapState.Items)
			{
				if (entry.Slot == 0)
					_roomStates[entry.Y, entry.X].Item1 = _items.Find(i => i.GetCode() == entry.Code);
				else if (entry.Slot == 1)
					_roomStates[entry.Y, entry.X].Item2 = _items.Find(i => i.GetCode() == entry.Code);
				else if (entry.Slot == 2)
					_roomStates[entry.Y, entry.X].Item3 = _items.Find(i => i.GetCode() == entry.Code);
			}

			foreach (var entry in mapState.Explored)
			{
				_roomStates[entry.Y, entry.X].Explored = entry.Code == "1";
			}
		}

		public override List<LocationOfItem> LogItemLocations()
		{
			List<LocationOfItem> list = new List<LocationOfItem>();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					var state = _roomStates[y, x];
					if (state.Item1 != null && state.Item1.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item1, $"Overworld at {y:X1}{x:X1}", Map, x, y);
						list.Add(loc);
					}
					if (state.Item2 != null && state.Item2.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item2, $"Overworld at {y:X1}{x:X1}", Map, x, y);
						list.Add(loc);
					}
					if (state.Item3 != null && state.Item3.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item3, $"Overworld at {y:X1}{x:X1}", Map, x, y);
						list.Add(loc);
					}
				}
			}

			return list;
		}

		public override List<LocationOfDest> LogExitLocations()
		{
			List<LocationOfDest> list = new List<LocationOfDest>();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					var state = _roomStates[y, x];
					if (state.Destination != null && state.Destination.IsExit)
					{
						LocationOfDest loc = new LocationOfDest(state.Destination, $"Overworld at {y:X1}{x:X1}", Map, x, y);
						list.Add(loc);
					}
				}
			}

			return list;
		}

		public void SetDestination(int x, int y, string destCode)
		{
			var dest = _dests.Find(d => d.GetCode() == destCode);
			_roomStates[y, x].Destination = dest;
		}

		#endregion

		#region Event Handlers

		private void HandleRoomDetailChanged(object sender, System.EventArgs e)
		{
			_drawable.Invalidate();
		}

		private void HandleDestCommand(object sender, System.EventArgs e)
		{
			if (_mxClick > -1 && _mxClick < 16 && _myClick > -1 && _myClick < 8)
			{
				var cmd = sender as Command;
				var dest = cmd.CommandParameter as GameDest;
				var roomState = _roomStates[_myClick, _mxClick];
				_mutator.ChangeDestination(_mxClick, _myClick, roomState, dest);
				_drawable.Invalidate();
				_overworldRoomDetail.Refresh();
			}
		}

		private void HandleDestsMenuOpening(object sender, System.EventArgs e)
		{
			_menuShowing = true;
		}

		private void HandleDestsMenuClosed(object sender, System.EventArgs e)
		{
			_menuShowing = false;
		}

		private void HandleTimerElapsed(object sender, System.EventArgs e)
		{
			if (_invalidateMap)
			{
				_drawable.Invalidate();
				_invalidateMap = false;
			}
			if (_invalidateRoom)
			{
				_overworldRoomDetail.Refresh();
				_invalidateRoom = false;
			}
		}

		private OverworldRoomProps GetProps(int x, int y)
		{
			return _meta[y, x];
		}

		protected override void HandleRoomClick(bool altButton)
		{
			if (_mxClick > -1 && _myClick > -1 && _mxClick < 16 && _myClick < 8)
			{
				var roomProps = GetProps(_mxClick, _myClick);
				var roomState = _roomStates[_myClick, _mxClick];
				_overworldRoomDetail.UpdateDetails(_mxClick, _myClick, roomProps, roomState);
				if (roomProps.DestHere && altButton)
				{
					_destsMenu.Show();
				}
			}
		}

		protected override void HandleRoomDoubleClick()
		{
			if (_mx > -1 && _mx < 16 && _my > -1 && _my < 8)
			{
				_roomStates[_my, _mx].Explored = !_roomStates[_my, _mx].Explored;
			}
		}

		protected override void PaintMap(Graphics g, float offx, float offy)
		{
			if (_mapImage != null)
			{
				g.DrawImage(_mapImage, offx, offy, 1024, 352);
			}

			// draw room state

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					var roomState = _roomStates[y, x];

					float x0 = x * 64 + offx;
					float y0 = y * 44 + offy;

					var props = GetProps(x, y);

					if (!props.DestHere && !props.ItemHere)
					{
						g.FillRectangle(ShadowBrush, x0, y0, 64, 44);
						continue;
					}

					if (roomState.Item1 != null)
					{
						g.DrawImage(roomState.Item1.Icon, x0 + 3, y0 + 23, 18, 18);
					}

					if (roomState.Item2 != null)
					{
						if (roomState.Destination == null)
						{
							g.DrawImage(roomState.Item2.Icon, x0 + 23, y0 + 13, 18, 18);
						}
						else
						{
							g.DrawImage(roomState.Item2.Icon, x0 + 23, y0 + 23, 18, 18);
						}
					}

					if (roomState.Item3 != null)
					{
						g.DrawImage(roomState.Item3.Icon, x0 + 43, y0 + 23, 18, 18);
					}

					if (roomState.Destination != null && !roomState.Destination.IsExit)
					{
						var dest = roomState.Destination;

						DrawDest(g, x0, y0, 64, 44, dest);
					}

					if ((props.DestHere || props.ItemHere) && roomState.Explored)
					{
						g.FillRectangle(ShadowBrush, x0, y0, 64, 44);
					}

					if (roomState.Destination != null && roomState.Destination.IsExit)
					{
						var dest = roomState.Destination;

						DrawDest(g, x0, y0, 64, 44, dest);
					}
				}
			}

			// draw "current room" box

			if (_mxClick > -1 && _myClick > -1 && _mxClick < 16 && _myClick < 8)
			{
				g.DrawRectangle(CurrentPen, _mxClick * 64 + offx, _myClick * 44 + offy, 63, 43);
			}

			// draw hover indicators

			if ((_mousePresent || _menuShowing) && _mx > -1 && _mx < 16 && _my > -1 && _my < 8)
			{
				g.FillRectangle(CursorBrush, _mx * 64 + offx, _my * 44 + offy, 64, 44);
			}
		}

		#endregion

		#region CoOp Event Handlers

		private void HandleCoOpClientFound(object sender, FoundEventArgs e)
		{
			if (e.Game == Game && e.Map == Map)
			{
				var roomState = _roomStates[e.Y, e.X];

				if (e.Type == "dest")
				{
					var dest = _dests.Find(d => d.GetCode() == e.Code);
					roomState.Destination = dest;
				}
				else if (e.Type == "item")
				{
					var item = _items.Find(i => i.GetCode() == e.Code);
					if (e.Slot == 0)
						roomState.Item1 = item;
					if (e.Slot == 1)
						roomState.Item2 = item;
					if (e.Slot == 2)
						roomState.Item3 = item;
				}

				_invalidateMap = true;

				if (e.X == _mxClick && e.Y == _myClick)
				{
					_invalidateRoom = true;
				}
			}
		}

		#endregion

		public void MirrorState()
		{
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					var s0 = _roomStates[y, x];
					var s1 = _roomStates[y, 15 - x];
					_roomStates[y, x] = s1;
					_roomStates[y, 15 - x] = s0;
				}
			}

			_drawable.Invalidate();
		}
	}
}
