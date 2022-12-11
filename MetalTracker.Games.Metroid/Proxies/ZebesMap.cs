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

		private readonly Drawable _drawable;
		private readonly ZebesRoomDetail _zebesRoomDetail;

		private int _flag_shuffled;
		private bool _flag_mirrored;
		private Image _mapImage;
		private ZebesRoomProps[,] _meta;

		private List<GameDest> _dests = new List<GameDest>();
		private ContextMenu _destsMenu;
		private List<GameItem> _items = new List<GameItem>();
		private UITimer _timer;
		private ZebesRoomStateMutator _mutator = new ZebesRoomStateMutator();

		private bool _menuShowing;
		private bool _invalidateMap;
		private bool _invalidateRoom;

		private ZebesRoomState[,] _roomStates = new ZebesRoomState[32, 32];

		#region Public Methods

		public ZebesMap(Drawable drawable, Panel detailPanel) : base(32, 30, drawable)
		{
			_drawable = drawable;
			_drawable.MouseUp += HandleMouseUp;
			_drawable.MouseDoubleClick += HandleMouseDoubleClick;
			_drawable.Paint += HandlePaint;

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
			_mapImage = InternalResourceClient.GetZebesImage(mirrored);
			_meta = InternalResourceClient.GetZebesMeta(shuffleMode, mirrored);
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

		public void LocateRoom(int x, int y)
		{
			_offset.X = 256 - 32 * x - 16;
			_offset.Y = 240 - 30 * y - 15;
			_mxClick = x;
			_myClick = y;
			_drawable.Invalidate();
			var roomProps = GetProps(_mxClick, _myClick);
			var roomState = _roomStates[_myClick, _mxClick];
			_zebesRoomDetail.UpdateDetails(_mxClick, _myClick, roomProps, roomState);
		}

		public override string GetMapKey()
		{
			return Map;
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

					if (roomState.DestElev != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = roomState.DestElev.GetCode() };
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
				_roomStates[entry.Y, entry.X].DestElev = _dests.Find(i => i.GetCode() == entry.Code);
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
						LocationOfItem loc = new LocationOfItem(state.Item, $"Zebes at Y={y:X2} X={x:X2}", Map, x, y);
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
					if (state.DestElev != null && state.DestElev.IsExit)
					{
						LocationOfDest loc = new LocationOfDest(state.DestElev, $"Zebes at Y={y:X2} X={x:X2}", Map, x, y);
						list.Add(loc);
					}
				}
			}

			return list;
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
				_mutator.ChangeDestination(_mxClick, _myClick, roomState, dest);
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

		private void HandleMouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (!_active) return;

			if (_mx > -1 && _mx < 32 && _my > -1 && _my < 32)
			{
				_roomStates[_my, _mx].Explored = !_roomStates[_my, _mx].Explored;
				_drawable.Invalidate();
			}
		}

		private void HandlePaint(object sender, PaintEventArgs e)
		{
			if (!_active) return;

			var origin = CalcPaintOrigin();

			var offx = origin.X;
			var offy = origin.Y;

			if (_mapImage != null)
			{
				e.Graphics.DrawImage(_mapImage, offx, offy, 1024, 960);
			}

			// draw room states

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					var roomState = _roomStates[y, x];

					float x0 = x * 32 + offx;
					float y0 = y * 30 + offy;

					var props = GetProps(x, y);

					if (props.Shuffled)
					{
						e.Graphics.FillRectangle(ShuffleBrush, x0, y0, 32, 30);
					}
					else
					{
						if (props.SlotClass != '\0' && roomState.Item == null)
						{
							DrawText(e.Graphics, x0, y0, 32, 30, props.SlotClass.ToString(), Fonts.Sans(12), Brushes.White);
						}
					}

					if (roomState.Item != null)
					{
						e.Graphics.DrawImage(roomState.Item.Icon, x0 + 7, y0 + 6, 18, 18);
					}

					if (roomState.Explored)
					{
						e.Graphics.FillRectangle(ShadowBrush, x0, y0, 32, 30);
					}

					if (roomState.DestElev != null)
					{
						DrawDest(e.Graphics, x0 - 16, y0, 64, 30, roomState.DestElev);
					}
				}
			}

			// draw "current room" box

			if (_mxClick > -1 && _myClick > -1 && _mxClick < 32 && _myClick < 32)
			{
				e.Graphics.DrawRectangle(CurrentPen, _mxClick * 32 + offx, _myClick * 30 + offy, 31, 29);
			}

			// draw hover indicators

			if ((_mousePresent || _menuShowing) && _mx > -1 && _mx < 32 && _my > -1 && _my < 32)
			{
				e.Graphics.FillRectangle(CursorBrush, _mx * 32 + offx, _my * 30 + offy, 32, 30);
			}
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
				_zebesRoomDetail.Refresh();
				_invalidateRoom = false;
			}
		}

		private ZebesRoomProps GetProps(int x, int y)
		{
			return _meta[y, x];
		}

		protected override void HandleRoomClick(bool altButton)
		{
			if (_mxClick > -1 && _myClick > -1 && _mxClick < 32 && _myClick < 32)
			{
				var roomProps = GetProps(_mxClick, _myClick);
				var roomState = _roomStates[_myClick, _mxClick];
				_zebesRoomDetail.UpdateDetails(_mxClick, _myClick, roomProps, roomState);
				if (roomProps.CanHaveDest() && altButton)
				{
					_destsMenu.Show();
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
					roomState.DestElev = dest;
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
					_roomStates[y, x] = s1;
					_roomStates[y, 31 - x] = s0;
				}
			}

			_drawable.Invalidate();
		}

	}
}
