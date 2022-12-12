using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using MetalTracker.Common.Bases;
using MetalTracker.Common.Types;
using MetalTracker.CoOp.Interface;
using MetalTracker.Games.Metroid.Internal;
using MetalTracker.Games.Metroid.Internal.Types;
using MetalTracker.Games.Metroid.Types;

namespace MetalTracker.Games.Metroid.Proxies
{
	public class ZebesMap : BaseMap
	{
		const string Game = "metroid";
		const string Map = "zebes";

		static SolidBrush ShuffleBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 200));
		static SolidBrush ShadowBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 152));
		static SolidBrush CursorBrush = new SolidBrush(Color.FromArgb(250, 250, 250, 102));
		static Pen CurrentPen = new Pen(Colors.White, 2);

		private readonly ZebesRoomDetail _zebesRoomDetail;

		private int _flag_shuffled;
		private bool _flag_mirrored;
		private Image _mapImage;
		private ZebesRoomProps[,] _meta;

		private List<GameDest> _dests = new List<GameDest>();
		private List<GameItem> _items = new List<GameItem>();

		private ContextMenu _destsMenu;
		private ZebesRoomStateMutator _mutator = new ZebesRoomStateMutator();

		private bool _menuShowing;

		private ZebesRoomState[,] _roomStates = new ZebesRoomState[32, 32];

		#region Public Methods

		public ZebesMap(Drawable drawable, Panel detailPanel) : base(16, 15, 2, drawable)
		{
			_mw = 32;
			_mh = 32;

			_destsMenu = new ContextMenu();
			_destsMenu.Opening += HandleDestsMenuOpening;
			_destsMenu.Closed += HandleDestsMenuClosed;

			_zebesRoomDetail = new ZebesRoomDetail(detailPanel, _mutator);
			_zebesRoomDetail.DetailChanged += HandleRoomDetailChanged;
		}

		public void SetMapFlags(int shuffleMode, bool mirrored)
		{
			if (mirrored != _flag_mirrored)
			{
				MirrorState();
			}

			_flag_shuffled = shuffleMode;
			_flag_mirrored = mirrored;
			_mapImage = ZebesResourceClient.GetZebesImage(mirrored);
			_meta = ZebesResourceClient.GetZebesMeta(shuffleMode, mirrored);
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
		}

		public override void Activate(bool active)
		{
			_active = active;
			if (active)
			{
				_drawable.Invalidate();
				_zebesRoomDetail.Build(_dests, _items);
			}
		}

		public void ResetState()
		{
			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					_roomStates[y, x] = new ZebesRoomState();
				}
			}

			_drawable.Invalidate();
		}

		public ZebesMapState PersistState()
		{
			ZebesMapState mapState = new ZebesMapState();

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					var roomState = _roomStates[y, x];

					// exits

					if (roomState.DestUp != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = roomState.DestUp.GetCode() };
						mapState.Dests.Add(entry);
					}
					if (roomState.DestDown != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 1, Code = roomState.DestDown.GetCode() };
						mapState.Dests.Add(entry);
					}
					if (roomState.DestLeft != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 2, Code = roomState.DestLeft.GetCode() };
						mapState.Dests.Add(entry);
					}
					if (roomState.DestRight != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 3, Code = roomState.DestRight.GetCode() };
						mapState.Dests.Add(entry);
					}

					// items

					if (roomState.Item != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = roomState.Item.GetCode() };
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

		public void RestoreState(ZebesMapState mapState)
		{
			foreach (var entry in mapState.Dests)
			{
				var dest = _dests.Find(i => i.GetCode() == entry.Code);
				if (entry.Slot == 0)
					_roomStates[entry.Y, entry.X].DestUp = dest;
				else if (entry.Slot == 1)
					_roomStates[entry.Y, entry.X].DestDown = dest;
				else if (entry.Slot == 2)
					_roomStates[entry.Y, entry.X].DestLeft = dest;
				else if (entry.Slot == 3)
					_roomStates[entry.Y, entry.X].DestRight = dest;
			}
			foreach (var entry in mapState.Items)
			{
				_roomStates[entry.Y, entry.X].Item = _items.Find(i => i.GetCode() == entry.Code);
			}
			foreach (var entry in mapState.Explored)
			{
				_roomStates[entry.Y, entry.X].Explored = entry.Code == "1";
			}
		}

		public override List<LocationOfItem> LogItemLocations()
		{
			List<LocationOfItem> list = new List<LocationOfItem>();

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					var state = _roomStates[y, x];
					if (state.Item != null && state.Item.IsImportant())
					{
						string areaName = GetAreaName(x, y);
						LocationOfItem loc = new LocationOfItem(state.Item, $"Zebes at Y={y:X2} X={x:X2} ({areaName})", Map, x, y);
						list.Add(loc);
					}
				}
			}

			return list;
		}

		public override List<LocationOfDest> LogExitLocations()
		{
			List<LocationOfDest> list = new List<LocationOfDest>();

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					var state = _roomStates[y, x];
					if (state.DestUp != null && state.DestUp.IsExit)
					{
						string areaName = GetAreaName(x, y);
						LocationOfDest loc = new LocationOfDest(state.DestUp, $"Zebes at Y={y:X2} X={x:X2} (elevator in {areaName})", Map, x, y);
						list.Add(loc);
					}
					if (state.DestDown != null && state.DestDown.IsExit)
					{
						string areaName = GetAreaName(x, y);
						LocationOfDest loc = new LocationOfDest(state.DestDown, $"Zebes at Y={y:X2} X={x:X2} (elevator in {areaName})", Map, x, y);
						list.Add(loc);
					}
					if (state.DestLeft != null && state.DestLeft.IsExit)
					{
						string areaName = GetAreaName(x, y);
						LocationOfDest loc = new LocationOfDest(state.DestLeft, $"Zebes at Y={y:X2} X={x:X2} (door in {areaName})", Map, x, y);
						list.Add(loc);
					}
					if (state.DestRight != null && state.DestRight.IsExit)
					{
						string areaName = GetAreaName(x, y);
						LocationOfDest loc = new LocationOfDest(state.DestRight, $"Zebes at Y={y:X2} X={x:X2} (door in {areaName})", Map, x, y);
						list.Add(loc);
					}
				}
			}

			return list;
		}

		private string GetAreaName(int x, int y)
		{
			var props = _meta[y, x];

			if (props == null) return "???";

			switch (props.AreaCode)
			{
				case 'B':
					return "Brinstar";
				case 'N':
					return "Norfair";
				case 'K':
					return "Kraid";
				case 'R':
					return "Ridley";
			}

			return "???";
		}

		#endregion

		#region Event Handlers

		private void HandleRoomDetailChanged(object sender, System.EventArgs e)
		{
			_drawable.Invalidate();
		}

		private void HandleDestCommand(object sender, System.EventArgs e)
		{
			if (_mxClick > -1 && _mxClick < 32 && _myClick > -1 && _myClick < 32)
			{
				var cmd = sender as Command;
				var dest = cmd.CommandParameter as GameDest;
				var roomState = _roomStates[_myClick, _mxClick];

				if (_nodeClick == 'U')
					_mutator.ChangeDestUp(_mxClick, _myClick, roomState, dest);
				else if (_nodeClick == 'D')
					_mutator.ChangeDestDown(_mxClick, _myClick, roomState, dest);
				else if (_nodeClick == 'L')
					_mutator.ChangeDestLeft(_mxClick, _myClick, roomState, dest);
				else if (_nodeClick == 'R')
					_mutator.ChangeDestRight(_mxClick, _myClick, roomState, dest);

				_drawable.Invalidate();
				_zebesRoomDetail.Refresh();
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

		private ZebesRoomProps GetProps(int x, int y)
		{
			return _meta[y, x];
		}

		protected override char DetermineNode(float x, float y)
		{
			int qx = (int)(4 * x / _rw);
			int qy = (int)(4 * y / _rh);

			char node = '\0';

			if (qy == 0 && (qx == 1 || qx == 2))
			{
				node = 'U';
			}
			else if (qy == 3 && (qx == 1 || qx == 2))
			{
				node = 'D';
			}
			else if (qx == 0 && (qy == 1 || qy == 2))
			{
				node = 'L';
			}
			else if (qx == 3 && (qy == 1 || qy == 2))
			{
				node = 'R';
			}

			return node;
		}

		protected override void HandleRoomClick(bool altButton)
		{
			if (_mxClick > -1 && _myClick > -1 && _mxClick < 32 && _myClick < 32)
			{
				var roomProps = GetProps(_mxClick, _myClick);
				var roomState = _roomStates[_myClick, _mxClick];
				_zebesRoomDetail.UpdateDetails(_mxClick, _myClick, roomProps, roomState);
				if (altButton && _nodeClick != '\0')
				{
					if (_nodeClick == 'U' && roomProps.ElevatorUp)
						_destsMenu.Show();
					else if (_nodeClick == 'D' && roomProps.ElevatorDown)
						_destsMenu.Show();
					else if (_nodeClick == 'L' && roomProps.CanExitLeft)
						_destsMenu.Show();
					else if (_nodeClick == 'R' && roomProps.CanExitRight)
						_destsMenu.Show();
				}
			}
		}

		protected override void HandleRoomDoubleClick()
		{
			if (_mx > -1 && _mx < 32 && _my > -1 && _my < 32)
			{
				_roomStates[_my, _mx].Explored = !_roomStates[_my, _mx].Explored;
			}
		}

		protected override void PaintMap(Graphics g, float offx, float offy)
		{
			if (_mapImage != null)
			{
				if (_zoom >= 2)
					g.ImageInterpolation = ImageInterpolation.None;
				else
					g.ImageInterpolation = ImageInterpolation.High;

				float w = _mw * _rw;
				float h = _mh * _rh;

				g.DrawImage(_mapImage, offx, offy, w, h);
			}

			// draw room states

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					var roomState = _roomStates[y, x];

					float x0 = x * _rw + offx;
					float y0 = y * _rh + offy;

					var props = GetProps(x, y);

					if (props.Shuffled)
					{
						g.FillRectangle(ShuffleBrush, x0, y0, _rw, _rh);
					}
					else
					{
						if (props.SlotClass != '\0' && roomState.Item == null)
						{
							DrawText(g, x0, y0 + _rh / 2 - 10, _rw, props.SlotClass.ToString(), Brushes.White);
						}
					}

					if (roomState.Item != null)
					{
						DrawCenteredImage(g, x0, y0, _rw, _rh, roomState.Item.Icon);
					}

					if (roomState.Explored)
					{
						g.FillRectangle(ShadowBrush, x0, y0, _rw, _rh);
					}

					if (roomState.DestUp != null)
					{
						DrawExit(g, x0 - _rw, y0, 3 * _rw, roomState.DestUp);
					}
					if (roomState.DestDown != null)
					{
						DrawExit(g, x0 - _rw, y0 + _rh / 2, 3 * _rw, roomState.DestDown);
					}
					if (roomState.DestLeft != null)
					{
						DrawExit(g, x0 - _rw - _rw / 3, y0 + _rh / 2 - 15, 3 * _rw, roomState.DestLeft);
					}
					if (roomState.DestRight != null)
					{
						DrawExit(g, x0 - _rw + _rw / 3, y0 + _rh / 2 - 15, 3 * _rw, roomState.DestRight);
					}
				}
			}

			// draw "current room" box

			if (_mxClick > -1 && _myClick > -1 && _mxClick < 32 && _myClick < 32)
			{
				g.DrawRectangle(CurrentPen, _mxClick * _rw + offx, _myClick * _rh + offy, _rw - 1, _rh - 1);
			}

			// draw hover indicators

			if ((_mousePresent || _menuShowing) && _mx > -1 && _mx < 32 && _my > -1 && _my < 32)
			{
				float x0 = _mx * _rw + offx;
				float y0 = _my * _rh + offy;

				g.FillRectangle(CursorBrush, x0, y0, _rw, _rh);

				var props = GetProps(_mx, _my);

				if (props != null)
				{
					if (props.ElevatorUp && _node == 'U')
					{
						g.FillRectangle(CursorBrush, x0 + _rw / 4f, y0, _rw / 2f, _rh / 4f);
					}
					else if (props.ElevatorDown && _node == 'D')
					{
						g.FillRectangle(CursorBrush, x0 + _rw / 4f, y0 + 3 * _rh / 4, _rw / 2f, _rh / 4f);
					}
					else if (props.CanExitLeft && _node == 'L')
					{
						g.FillRectangle(CursorBrush, x0, y0 + _rh / 4, _rw / 4f, _rh / 2f);
					}
					else if (props.CanExitRight && _node == 'R')
					{
						g.FillRectangle(CursorBrush, x0 + 3 * _rw / 4, y0 + _rh / 4, _rw / 4f, _rh / 2f);
					}
				}
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

					if (e.Slot == 0)
						roomState.DestUp = dest;
					else if (e.Slot == 1)
						roomState.DestDown = dest;
					else if (e.Slot == 2)
						roomState.DestLeft = dest;
					else if (e.Slot == 3)
						roomState.DestRight = dest;
				}
				else if (e.Type == "item")
				{
					var item = _items.Find(i => i.GetCode() == e.Code);
					roomState.Item = item;
				}

				_invalidateMap = true;

				if (e.X == _mxClick && e.Y == _myClick)
				{
					_invalidateRoom = true;
				}
			}
		}

		#endregion

		private void MirrorState()
		{
			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					var s0 = _roomStates[y, x];
					var s1 = _roomStates[y, 31 - x];

					s0?.Mirror();
					s1?.Mirror();

					_roomStates[y, x] = s1;
					_roomStates[y, 31 - x] = s0;
				}
			}

			_drawable.Invalidate();
		}
	}
}
