using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using MetalTracker.Common.Bases;
using MetalTracker.Common.Types;
using MetalTracker.CoOp.Interface;
using MetalTracker.Games.Zelda.Internal;
using MetalTracker.Games.Zelda.Types;

namespace MetalTracker.Games.Zelda.Proxies
{
	public class DungeonMap : BaseMap
	{
		const string Game = "zelda";

		string _map = null;

		static SolidBrush ShuffleBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 200));
		static SolidBrush ShadowBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 152));
		static SolidBrush CursorBrush = new SolidBrush(Color.FromArgb(250, 250, 250, 102));
		static Pen CurrentPen = new Pen(Colors.White, 2);

		private readonly Drawable _drawable;
		private readonly DungeonRoomDetail _dungeonRoomDetail;

		private bool _flag_q2;
		private int _flag_level;
		private bool _flag_mirrored;
		private int _flag_shuffle;

		private Image _mapImage;
		private DungeonRoomProps[,] _meta;
		private int _width;
		private int _numTransports;

		private List<GameDest> _dests = new List<GameDest>();
		private ContextMenu _destsMenu;
		private List<GameItem> _items = new List<GameItem>();
		private UITimer _timer;
		private DungeonRoomStateMutator _mutator = new DungeonRoomStateMutator();

		private bool _active;
		private bool _mousePresent;
		private bool _mouseDown;
		private PointF _mouseDownLoc;
		private PointF _mouseLoc;
		//private PointF _offset = new PointF(64, 64);
		private bool _menuShowing;
		private int _my = -1;
		private int _mx = -1;
		private char _node = '\0';
		private int _myClick = -1;
		private int _mxClick = -1;
		private char _nodeClick = '\0';
		private bool _invalidateMap;
		private bool _invalidateRoom;

		private DungeonRoomState[,] _roomStates = new DungeonRoomState[8, 8];

		public DungeonMap(Drawable drawable, Panel detailPanel)
		{
			_drawable = drawable;
			_drawable.MouseLeave += HandleMouseLeave;
			_drawable.MouseMove += HandleMouseMove;
			_drawable.MouseDown += HandleMouseDown;
			_drawable.MouseUp += HandleMouseUp;
			_drawable.MouseDoubleClick += HandleMouseDoubleClick;
			_drawable.Paint += HandlePaint;

			_destsMenu = new ContextMenu();
			_destsMenu.Opening += HandleDestsMenuOpening;
			_destsMenu.Closed += HandleDestsMenuClosed;

			_dungeonRoomDetail = new DungeonRoomDetail(detailPanel, _mutator);
			_dungeonRoomDetail.DetailChanged += HandleRoomDetailChanged;

			//ResetState();
		}

		public void SetMapFlags(bool q2, int level, int shuffleMode, bool mirrored)
		{
			if (mirrored != _flag_mirrored)
			{
				MirrorState();
			}

			_flag_q2 = q2;
			_flag_level = level;
			_flag_shuffle = shuffleMode;
			_flag_mirrored = mirrored;

			_mapImage = DungeonResourceClient.GetDungeonImage(q2, level, mirrored);
			_meta = DungeonResourceClient.GetDungeonMeta(q2, level, shuffleMode, mirrored);
			_width = _meta.GetLength(1);
			_map = $"d{level}";

			var stairsCount = 0;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					var props = _meta[y, x];
					if (props != null)
					{
						if (props.HasTransports)
						{
							stairsCount = stairsCount + 1;
						}
					}
				}
			}

			_numTransports = stairsCount / 2;
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
			coOpClient.FoundDest += HandleCoOpClientFoundDest;
			coOpClient.FoundItem += HandleCoOpClientFoundItem;
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
				_dungeonRoomDetail.Build(_dests, _items, _numTransports);
			}
		}

		public void ResetState()
		{
			_roomStates = DungeonResourceClient.GetDefaultDungeonState(_flag_q2, _flag_level, _flag_shuffle, _flag_mirrored);
			_drawable.Invalidate();
		}

		public void LocateRoom(int x, int y)
		{
			_mxClick = x;
			_myClick = y;
			_drawable.Invalidate();
			var roomProps = GetProps(_mxClick, _myClick);
			var roomState = _roomStates[_myClick, _mxClick];
			_dungeonRoomDetail.UpdateDetails(_flag_level, _mxClick, _myClick, roomProps, roomState);
		}

		public override string GetMapKey()
		{
			return _map;
		}

		public override List<StateEntry> GetDestStates()
		{
			List<StateEntry> list = new List<StateEntry>();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					var state = _roomStates[y, x];
					if (state.DestNorth != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = state.DestNorth.GetCode() };
						list.Add(entry);
					}
					if (state.DestSouth != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 1, Code = state.DestSouth.GetCode() };
						list.Add(entry);
					}
					if (state.DestWest != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 2, Code = state.DestWest.GetCode() };
						list.Add(entry);
					}
					if (state.DestEast != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 3, Code = state.DestEast.GetCode() };
						list.Add(entry);
					}
				}
			}

			return list;
		}

		public override List<StateEntry> GetItemStates()
		{
			List<StateEntry> list = new List<StateEntry>();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					var state = _roomStates[y, x];
					if (state.Item1 != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = state.Item1.GetCode() };
						list.Add(entry);
					}
					if (state.Item2 != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 1, Code = state.Item2.GetCode() };
						list.Add(entry);
					}
				}
			}

			return list;
		}

		public override void SetDestStates(List<StateEntry> entries)
		{
			foreach (var entry in entries)
			{
				if (entry.Slot == 0)
					_roomStates[entry.Y, entry.X].DestNorth = _dests.Find(i => i.GetCode() == entry.Code);
				else if (entry.Slot == 1)
					_roomStates[entry.Y, entry.X].DestSouth = _dests.Find(i => i.GetCode() == entry.Code);
				else if (entry.Slot == 2)
					_roomStates[entry.Y, entry.X].DestWest = _dests.Find(i => i.GetCode() == entry.Code);
				else if (entry.Slot == 3)
					_roomStates[entry.Y, entry.X].DestEast = _dests.Find(i => i.GetCode() == entry.Code);
			}
		}

		public override void SetItemStates(List<StateEntry> entries)
		{
			foreach (var entry in entries)
			{
				if (entry.Slot == 0)
					_roomStates[entry.Y, entry.X].Item1 = _items.Find(i => i.GetCode() == entry.Code);
				else if (entry.Slot == 1)
					_roomStates[entry.Y, entry.X].Item2 = _items.Find(i => i.GetCode() == entry.Code);
			}
		}

		public override List<LocationOfItem> LogItemLocations()
		{
			List<LocationOfItem> list = new List<LocationOfItem>();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					var state = _roomStates[y, x];
					if (state.Item1 != null && state.Item1.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item1, $"Dungeon #{_flag_level} (floor)", _map, x, y);
						list.Add(loc);
					}
					if (state.Item2 != null && state.Item2.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item2, $"Dungeon #{_flag_level} (basement)", _map, x, y);
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
				for (int x = 0; x < _width; x++)
				{
					var state = _roomStates[y, x];
					if (state.DestNorth != null)
					{
						LocationOfDest loc = new LocationOfDest(state.DestNorth, $"Dungeon #{_flag_level}", _map, x, y);
						list.Add(loc);
					}
					if (state.DestSouth != null)
					{
						LocationOfDest loc = new LocationOfDest(state.DestSouth, $"Dungeon #{_flag_level}", _map, x, y);
						list.Add(loc);
					}
					if (state.DestWest != null)
					{
						LocationOfDest loc = new LocationOfDest(state.DestWest, $"Dungeon #{_flag_level}", _map, x, y);
						list.Add(loc);
					}
					if (state.DestEast != null)
					{
						LocationOfDest loc = new LocationOfDest(state.DestEast, $"Dungeon #{_flag_level}", _map, x, y);
						list.Add(loc);
					}
				}
			}

			return list;
		}

		#region Event Handlers

		private void HandleRoomDetailChanged(object sender, System.EventArgs e)
		{
			_drawable.Invalidate();
		}

		private void HandleDestCommand(object sender, System.EventArgs e)
		{
			if (_mxClick > -1 && _mxClick < _width && _myClick > -1 && _myClick < 8 && _nodeClick != '\0')
			{
				var roomProps = GetProps(_mxClick, _myClick);

				if (roomProps == null)
				{
					return;
				}

				var cmd = sender as Command;
				var dest = cmd.CommandParameter as GameDest;
				var roomState = _roomStates[_myClick, _mxClick];

				if (_nodeClick == 'N' && roomProps.DestNorth)
				{
					_mutator.ChangeDestNorth(_flag_level, _mxClick, _myClick, roomState, dest);
				}
				else if (_nodeClick == 'S' && roomProps.DestSouth)
				{
					_mutator.ChangeDestSouth(_flag_level, _mxClick, _myClick, roomState, dest);
				}
				else if (_nodeClick == 'W' && roomProps.DestWest)
				{
					_mutator.ChangeDestWest(_flag_level, _mxClick, _myClick, roomState, dest);
				}
				else if (_nodeClick == 'E' && roomProps.DestEast)
				{
					_mutator.ChangeDestEast(_flag_level, _mxClick, _myClick, roomState, dest);
				}

				_drawable.Invalidate();
				_dungeonRoomDetail.Refresh();
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

		private void HandleMouseDown(object sender, MouseEventArgs e)
		{
			if (!_active) return;

			_mouseDown = true;
			_mouseDownLoc = e.Location;
		}

		private void HandleMouseUp(object sender, MouseEventArgs e)
		{
			if (!_active) return;

			_mouseDown = false;
			HandleMouseMove(sender, e);
			_mxClick = _mx;
			_myClick = _my;
			_nodeClick = _node;
			_drawable.Invalidate();
			if (_mxClick > -1 && _myClick > -1 && _mxClick < _width && _myClick < 8)
			{
				var roomProps = GetProps(_mxClick, _myClick);
				var roomState = _roomStates[_myClick, _mxClick];
				_dungeonRoomDetail.UpdateDetails(_flag_level, _mxClick, _myClick, roomProps, roomState);
				if (e.Buttons == MouseButtons.Alternate)
				{
					if (roomProps.DestNorth && _nodeClick == 'N')
					{
						_destsMenu.Show();
					}
					else if (roomProps.DestSouth && _nodeClick == 'S')
					{
						_destsMenu.Show();
					}
					else if (roomProps.DestWest && _nodeClick == 'W')
					{
						_destsMenu.Show();
					}
					else if (roomProps.DestEast && _nodeClick == 'E')
					{
						_destsMenu.Show();
					}
				}
			}
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			if (!_active) return;

			_mousePresent = true;

			_mouseLoc = e.Location;

			if (!_mouseDown)
			{
				var offx = (512 - _width * 64) / 2;
				var offy = 64;

				float dx = _mouseLoc.X - offx;
				float dy = _mouseLoc.Y - offy;
				_mx = dx < 0f ? -1 : (int)dx / 64;
				_my = dy < 0f ? -1 : (int)dy / 44;

				int qx = ((int)dx / 16) % 4;
				int qy = ((int)dy / 11) % 4;

				_node = '\0';

				if (qx == 0 && (qy == 1 || qy == 2))
				{
					_node = 'W';
				}
				else if (qx == 3 && (qy == 1 || qy == 2))
				{
					_node = 'E';
				}
				else if (qy == 0 && (qx == 1 || qx == 2))
				{
					_node = 'N';
				}
				else if (qy == 3 && (qx == 1 || qx == 2))
				{
					_node = 'S';
				}
			}

			_drawable.Invalidate();
		}

		private void HandleMouseLeave(object sender, MouseEventArgs e)
		{
			if (!_active) return;

			_mousePresent = false;

			_drawable.Invalidate();
		}

		private void HandleMouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (!_active) return;

			if (_mx > -1 && _mx < _width && _my > -1 && _my < 8)
			{
				_roomStates[_my, _mx].Explored = !_roomStates[_my, _mx].Explored;
				_drawable.Invalidate();
			}
		}

		private void HandlePaint(object sender, PaintEventArgs e)
		{
			if (!_active) return;

			var offx = (512 - _width * 64) / 2;
			var offy = 64;

			if (_mapImage != null)
			{
				e.Graphics.DrawImage(_mapImage, offx, offy, _width * 64, 352);
			}

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					var props = GetProps(x, y);

					if (props == null)
					{
						continue;
					}

					var roomState = _roomStates[y, x];

					float x0 = x * 64 + offx;
					float y0 = y * 44 + offy;

					if (props.Shuffled)
					{
						e.Graphics.FillRectangle(ShuffleBrush, x0, y0, 64, 44);
					}
					else
					{
						if (props.Slot1Class != '\0' && roomState.Item1 == null)
						{
							DrawText(e.Graphics, x0 - 6, y0 + 5, 64, 44, props.Slot1Class.ToString(), Fonts.Sans(12), Brushes.White);
						}
						if (props.Slot2Class != '\0' && roomState.Item2 == null)
						{
							DrawText(e.Graphics, x0 + 6, y0 + 17, 64, 44, props.Slot2Class.ToString(), Fonts.Sans(12), Brushes.White);
						}
					}

					if (roomState.Item1 != null)
					{
						e.Graphics.DrawImage(roomState.Item1.Icon, x0 + 17, y0 + 7, 18, 18);
					}

					if (roomState.Item2 != null)
					{
						e.Graphics.DrawImage(roomState.Item2.Icon, x0 + 29, y0 + 16, 18, 18);
					}

					if (roomState.Explored)
					{
						e.Graphics.FillRectangle(ShadowBrush, x0, y0, 64, 44);
					}

					if (roomState.DestNorth != null)
					{
						DrawDest(e.Graphics, x0, y0 - 11, 64, 44, roomState.DestNorth);
					}

					if (roomState.DestSouth != null)
					{
						DrawDest(e.Graphics, x0, y0 + 33, 64, 44, roomState.DestSouth);
					}

					if (roomState.DestWest != null)
					{
						DrawDest(e.Graphics, x0 - 32, y0 + 11, 64, 44, roomState.DestWest);
					}

					if (roomState.DestEast != null)
					{
						DrawDest(e.Graphics, x0 + 32, y0 + 11, 64, 44, roomState.DestEast);
					}
				}
			}

			if ((_mousePresent || _menuShowing) && _mx > -1 && _mx < _width && _my > -1 && _my < 8)
			{
				float x0 = _mx * 64 + offx;
				float y0 = _my * 44 + offy;

				e.Graphics.FillRectangle(CursorBrush, x0, y0, 64, 44);

				if (_node == 'N')
				{
					e.Graphics.FillRectangle(CursorBrush, x0 + 16, y0, 32, 11);
				}
				else if (_node == 'S')
				{
					e.Graphics.FillRectangle(CursorBrush, x0 + 16, y0 + 33, 32, 11);
				}
				else if (_node == 'W')
				{
					e.Graphics.FillRectangle(CursorBrush, x0, y0 + 11, 16, 22);
				}
				else if (_node == 'E')
				{
					e.Graphics.FillRectangle(CursorBrush, x0 + 48, y0 + 11, 16, 22);
				}
			}

			if (_mxClick > -1 && _myClick > -1 && _mxClick < _width && _myClick < 8)
			{
				e.Graphics.DrawRectangle(CurrentPen, _mxClick * 64 + offx, _myClick * 44 + offy, 63, 43);
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
				_dungeonRoomDetail.Refresh();
				_invalidateRoom = false;
			}
		}

		private DungeonRoomProps GetProps(int x, int y)
		{
			return _meta[y, x];
		}

		#endregion

		#region CoOp Event Handlers

		private void HandleCoOpClientFoundDest(object sender, FoundEventArgs e)
		{
			Debug.Assert(_map != null && _map.Length == 2);

			if (e.Game == Game && e.Map == _map)
			{
				var roomState = _roomStates[e.Y, e.X];
				var dest = _dests.Find(d => d.GetCode() == e.Code);

				if (e.Slot == 0)
					roomState.DestNorth = dest;
				else if (e.Slot == 1)
					roomState.DestSouth = dest;
				else if (e.Slot == 2)
					roomState.DestWest = dest;
				else if (e.Slot == 3)
					roomState.DestEast = dest;

				_invalidateMap = true;

				if (e.X == _mxClick && e.Y == _myClick)
				{
					_invalidateRoom = true;
				}
			}
		}

		private void HandleCoOpClientFoundItem(object sender, FoundEventArgs e)
		{
			Debug.Assert(_map != null && _map.Length == 2);

			if (e.Game == Game && e.Map == _map)
			{
				var roomState = _roomStates[e.Y, e.X];
				var item = _items.Find(i => i.GetCode() == e.Code);

				if (e.Slot == 0)
					roomState.Item1 = item;
				else if (e.Slot == 1)
					roomState.Item2 = item;

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
			int m = _width / 2;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < m; x++)
				{
					var s0 = _roomStates[y, x];
					var s1 = _roomStates[y, (_width - 1) - x];

					s0.Mirror();
					s1.Mirror();

					_roomStates[y, x] = s1;
					_roomStates[y, (_width - 1) - x] = s0;
				}
			}

			_drawable.Invalidate();
		}
	}
}
