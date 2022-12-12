using System.Collections.Generic;
using System.Diagnostics;
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
	public class DungeonMap : BaseMap
	{
		const string Game = "zelda";

		string _mapKey = null;

		static SolidBrush ShuffleBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 200));
		static SolidBrush ShadowBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 152));
		static SolidBrush CursorBrush = new SolidBrush(Color.FromArgb(250, 250, 250, 102));
		static Pen CurrentPen = new Pen(Colors.White, 2);

		private readonly DungeonRoomDetail _dungeonRoomDetail;

		private bool _flag_q2;
		private int _flag_level;
		private bool _flag_mirrored;
		private int _flag_shuffle;

		private Image _mapImage;
		private DungeonRoomProps[,] _meta;
		private int _numTransports;

		private List<DungeonWall> _walls = new List<DungeonWall>();

		private DungeonRoomStateMutator _mutator = new DungeonRoomStateMutator();

		private List<GameDest> _dests = new List<GameDest>();
		private List<GameItem> _items = new List<GameItem>();

		private ContextMenu _destsMenu;
		private ContextMenu _wallsMenu;

		private Image _walls_n;
		private Image _walls_s;
		private Image _walls_w;
		private Image _walls_e;

		private bool _menuShowing;

		private DungeonRoomState[,] _roomStates = new DungeonRoomState[8, 8];

		public DungeonMap(Drawable drawable, Panel detailPanel) : base(16, 11, 4, drawable)
		{
			_mw = 8;
			_mh = 8;

			_destsMenu = new ContextMenu();
			_destsMenu.Opening += HandleContextMenuOpening;
			_destsMenu.Closed += HandleContextMenuClosed;

			_wallsMenu = new ContextMenu();
			_wallsMenu.Opening += HandleContextMenuOpening;
			_wallsMenu.Closed += HandleContextMenuClosed;

			foreach (var wall in DungeonResourceClient.GetDungeonWalls())
			{
				Command cmd = new Command();
				cmd.Executed += HandleWallCommand;
				cmd.CommandParameter = wall;
				_wallsMenu.Items.Add(new ButtonMenuItem { Text = wall.Name, Command = cmd });
				_walls.Add(wall);
			}

			_dungeonRoomDetail = new DungeonRoomDetail(detailPanel, _mutator);
			_dungeonRoomDetail.DetailChanged += HandleRoomDetailChanged;

			_walls_n = DungeonResourceClient.GetDungeonWallIcons("n");
			_walls_s = DungeonResourceClient.GetDungeonWallIcons("s");
			_walls_w = DungeonResourceClient.GetDungeonWallIcons("w");
			_walls_e = DungeonResourceClient.GetDungeonWallIcons("e");
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

			_mw = _meta.GetLength(1);
			_mh = 8;

			_mapKey = $"d{level}";

			var stairsCount = 0;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _mw; x++)
				{
					var props = _meta[y, x];
					if (props != null)
					{
						if (props.HasStairs)
						{
							stairsCount = stairsCount + 1;
						}
					}
					if (_roomStates[y, x] == null)
					{
						_roomStates[y, x] = new DungeonRoomState();
					}
				}
			}

			_numTransports = stairsCount / 2;

			_dungeonRoomDetail.SetTransports(_numTransports);
		}

		public void SetGameItems(IEnumerable<GameItem> gameItems)
		{
			_items = gameItems.ToList();
			_dungeonRoomDetail.PopulateItems(_items);
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
				_dungeonRoomDetail.AddDest(dest);
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
				_dungeonRoomDetail.Activate();
			}
		}

		public void ResetState()
		{
			_roomStates = DungeonResourceClient.GetDefaultDungeonState(_flag_q2, _flag_level, _flag_shuffle, _flag_mirrored);
			_drawable.Invalidate();
		}

		public DungeonMapState PersistState()
		{
			DungeonMapState mapState = new DungeonMapState();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _mw; x++)
				{
					var roomState = _roomStates[y, x];

					// dests

					if (roomState.DestNorth != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = roomState.DestNorth.GetCode() };
						mapState.Dests.Add(entry);
					}
					if (roomState.DestSouth != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 1, Code = roomState.DestSouth.GetCode() };
						mapState.Dests.Add(entry);
					}
					if (roomState.DestWest != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 2, Code = roomState.DestWest.GetCode() };
						mapState.Dests.Add(entry);
					}
					if (roomState.DestEast != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 3, Code = roomState.DestEast.GetCode() };
						mapState.Dests.Add(entry);
					}

					// walls

					if (roomState.WallNorth != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = roomState.WallNorth.Code };
						mapState.Walls.Add(entry);
					}
					if (roomState.WallSouth != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 1, Code = roomState.WallSouth.Code };
						mapState.Walls.Add(entry);
					}
					if (roomState.WallWest != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 2, Code = roomState.WallWest.Code };
						mapState.Walls.Add(entry);
					}
					if (roomState.WallEast != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 3, Code = roomState.WallEast.Code };
						mapState.Walls.Add(entry);
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

					// stairs

					if (roomState.Transport != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = roomState.Transport };
						mapState.Stairs.Add(entry);
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

		public void RestoreState(DungeonMapState mapState)
		{
			foreach (var entry in mapState.Dests)
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

			foreach (var entry in mapState.Walls)
			{
				if (entry.Slot == 0)
					_roomStates[entry.Y, entry.X].WallNorth = _walls.Find(i => i.Code == entry.Code);
				else if (entry.Slot == 1)
					_roomStates[entry.Y, entry.X].WallSouth = _walls.Find(i => i.Code == entry.Code);
				else if (entry.Slot == 2)
					_roomStates[entry.Y, entry.X].WallWest = _walls.Find(i => i.Code == entry.Code);
				else if (entry.Slot == 3)
					_roomStates[entry.Y, entry.X].WallEast = _walls.Find(i => i.Code == entry.Code);
			}

			foreach (var entry in mapState.Items)
			{
				if (entry.Slot == 0)
					_roomStates[entry.Y, entry.X].Item1 = _items.Find(i => i.GetCode() == entry.Code);
				else if (entry.Slot == 1)
					_roomStates[entry.Y, entry.X].Item2 = _items.Find(i => i.GetCode() == entry.Code);
			}

			foreach (var entry in mapState.Stairs)
			{
				if (entry.Slot == 0)
					_roomStates[entry.Y, entry.X].Transport = entry.Code;
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
				for (int x = 0; x < _mw; x++)
				{
					var state = _roomStates[y, x];
					if (state.Item1 != null && state.Item1.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item1, $"Dungeon #{_flag_level} (floor)", _mapKey, x, y);
						list.Add(loc);
					}
					if (state.Item2 != null && state.Item2.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item2, $"Dungeon #{_flag_level} (basement)", _mapKey, x, y);
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
				for (int x = 0; x < _mw; x++)
				{
					var state = _roomStates[y, x];
					if (state.DestNorth != null)
					{
						LocationOfDest loc = new LocationOfDest(state.DestNorth, $"Dungeon #{_flag_level} (north door)", _mapKey, x, y);
						list.Add(loc);
					}
					if (state.DestSouth != null)
					{
						LocationOfDest loc = new LocationOfDest(state.DestSouth, $"Dungeon #{_flag_level} (south door)", _mapKey, x, y);
						list.Add(loc);
					}
					if (state.DestWest != null)
					{
						LocationOfDest loc = new LocationOfDest(state.DestWest, $"Dungeon #{_flag_level} (west door)", _mapKey, x, y);
						list.Add(loc);
					}
					if (state.DestEast != null)
					{
						LocationOfDest loc = new LocationOfDest(state.DestEast, $"Dungeon #{_flag_level} (east door)", _mapKey, x, y);
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
			if (_mxClick > -1 && _mxClick < _mw && _myClick > -1 && _myClick < 8 && _nodeClick != '\0')
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

		private void HandleWallCommand(object sender, System.EventArgs e)
		{
			if (_mxClick > -1 && _mxClick < _mw && _myClick > -1 && _myClick < 8 && _nodeClick != '\0')
			{
				var roomProps = GetProps(_mxClick, _myClick);

				if (roomProps == null)
				{
					return;
				}

				var cmd = sender as Command;
				var wall = cmd.CommandParameter as DungeonWall;
				var roomState = _roomStates[_myClick, _mxClick];

				if (_nodeClick == 'N' && !roomProps.DestNorth)
				{
					_mutator.ChangeWallNorth(_flag_level, _mxClick, _myClick, roomState, wall);
				}
				else if (_nodeClick == 'S' && !roomProps.DestSouth)
				{
					_mutator.ChangeWallSouth(_flag_level, _mxClick, _myClick, roomState, wall);
				}
				else if (_nodeClick == 'W' && !roomProps.DestWest)
				{
					_mutator.ChangeWallWest(_flag_level, _mxClick, _myClick, roomState, wall);
				}
				else if (_nodeClick == 'E' && !roomProps.DestEast)
				{
					_mutator.ChangeWallEast(_flag_level, _mxClick, _myClick, roomState, wall);
				}

				_drawable.Invalidate();
				_dungeonRoomDetail.Refresh();
			}
		}

		private void HandleContextMenuOpening(object sender, System.EventArgs e)
		{
			_menuShowing = true;
		}

		private void HandleContextMenuClosed(object sender, System.EventArgs e)
		{
			_menuShowing = false;
		}

		private DungeonRoomProps GetProps(int x, int y)
		{
			return _meta[y, x];
		}

		private List<Point> FindTransportRooms(string transport)
		{
			List<Point> points = new List<Point>();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _mw; x++)
				{
					if (_roomStates[y, x].Transport == transport)
					{
						points.Add(new Point(x, y));
					}
				}
			}

			return points;
		}

		protected override char DetermineNode(float x, float y)
		{
			int qx = (int)(4 * x / _rw);
			int qy = (int)(4 * y / _rh);

			char node = '\0';

			if (qy == 0 && (qx == 1 || qx == 2))
			{
				node = 'N';
			}
			else if (qy == 3 && (qx == 1 || qx == 2))
			{
				node = 'S';
			}
			else if (qx == 0 && (qy == 1 || qy == 2))
			{
				node = 'W';
			}
			else if (qx == 3 && (qy == 1 || qy == 2))
			{
				node = 'E';
			}

			return node;
		}

		protected override void HandleRoomClick(bool altButton)
		{
			if (_mxClick > -1 && _myClick > -1 && _mxClick < _mw && _myClick < 8)
			{
				var roomProps = GetProps(_mxClick, _myClick);
				var roomState = _roomStates[_myClick, _mxClick];
				_dungeonRoomDetail.UpdateDetails(_flag_level, _mxClick, _myClick, roomProps, roomState);

				if (roomProps == null) return;

				if (altButton)
				{
					if (_nodeClick == 'N')
					{
						if (roomProps.DestNorth)
							_destsMenu.Show();
						else
							_wallsMenu.Show();
					}
					else if (_nodeClick == 'S')
					{
						if (roomProps.DestSouth)
							_destsMenu.Show();
						else
							_wallsMenu.Show();
					}
					else if (_nodeClick == 'W')
					{
						if (roomProps.DestWest)
							_destsMenu.Show();
						else
							_wallsMenu.Show();
					}
					else if (_nodeClick == 'E')
					{
						if (roomProps.DestEast)
							_destsMenu.Show();
						else
							_wallsMenu.Show();
					}
				}
			}
		}

		protected override void HandleRoomDoubleClick()
		{
			if (_mx > -1 && _mx < _mw && _my > -1 && _my < 8)
			{
				_roomStates[_my, _mx].Explored = !_roomStates[_my, _mx].Explored;
			}
		}

		protected override void PaintMap(Graphics g, float offx, float offy)
		{
			if (_mapImage != null)
			{
				g.ImageInterpolation = ImageInterpolation.High;

				float w = _mw * _rw;
				float h = _mh * _rh;

				g.DrawImage(_mapImage, offx, offy, w, h);
			}

			// draw room state

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < _mw; x++)
				{
					var props = GetProps(x, y);

					if (props == null)
					{
						continue;
					}

					DungeonRoomState roomState = _roomStates[y, x];

					float x0 = x * _rw + offx;
					float y0 = y * _rh + offy;

					if (props.Shuffled)
					{
						g.FillRectangle(ShuffleBrush, x0, y0, _rw, _rh);
					}
					else
					{
						if (props.Slot1Class != '\0' && roomState.Item1 == null)
						{
							DrawText(g, x0 - 1 * _rw / 4, y0 + _rh / 2 - 10, _rw, props.Slot1Class.ToString(), Brushes.White);
						}
						if (props.Slot2Class != '\0' && roomState.Item2 == null)
						{
							DrawText(g, x0 + 1 * _rw / 4, y0 + _rh / 2 - 10, _rw, props.Slot2Class.ToString(), Brushes.White);
						}
					}

					// walls

					if (_rh > 32)
					{
						if (roomState.WallNorth != null)
						{
							RectangleF sr = new RectangleF(16 * roomState.WallNorth.Ordinal, 0, 16, 16);
							g.DrawImage(_walls_n, sr, new PointF(x0 + _rw / 2 - 8, y0));
						}
						if (roomState.WallSouth != null)
						{
							RectangleF sr = new RectangleF(16 * roomState.WallSouth.Ordinal, 0, 16, 16);
							g.DrawImage(_walls_s, sr, new PointF(x0 + _rw / 2 - 8, y0 + _rh - 16));
						}
						if (roomState.WallWest != null)
						{
							RectangleF sr = new RectangleF(0, 16 * roomState.WallWest.Ordinal, 16, 16);
							g.DrawImage(_walls_w, sr, new PointF(x0, y0 + _rh / 2 - 8));
						}
						if (roomState.WallEast != null)
						{
							// paint east wall type
							RectangleF sr = new RectangleF(0, 16 * roomState.WallEast.Ordinal, 16, 16);
							g.DrawImage(_walls_e, sr, new PointF(x0 + _rw - 16, y0 + _rh / 2 - 8));
						}
					}

					// items

					var sw = _rw / 4f;

					if (roomState.Item1 != null)
					{
						DrawCenteredImage(g, x0 + 1 * sw, y0, sw, _rh, roomState.Item1.Icon);
					}
					if (roomState.Item2 != null)
					{
						DrawCenteredImage(g, x0 + 2 * sw, y0, sw, _rh, roomState.Item2.Icon);
					}

					// explored 

					if (roomState.Explored)
					{
						g.FillRectangle(ShadowBrush, x0, y0, _rw, _rh);
					}

					// transport

					if (roomState.Transport != null)
					{
						DrawText(g, x0 + 1 * _rw / 4, y0 + _rh / 2 - 10, _rw, $"{roomState.Transport}", Brushes.CornflowerBlue);
					}

					// exits

					if (roomState.DestNorth != null)
					{
						DrawExit(g, x0 - _rw, y0 - 11, 3 * _rw, roomState.DestNorth);
					}
					if (roomState.DestSouth != null)
					{
						DrawExit(g, x0 - _rw, y0 + _rh - 10, 3 * _rw, roomState.DestSouth);
					}
					if (roomState.DestWest != null)
					{
						DrawExit(g, x0 - 32, y0 + 11, 64, roomState.DestWest);
					}
					if (roomState.DestEast != null)
					{
						DrawExit(g, x0 + 32, y0 + 11, 64, roomState.DestEast);
					}
				}
			}

			// draw "current room" box

			if (_mxClick > -1 && _myClick > -1 && _mxClick < _mw && _myClick < 8)
			{
				g.DrawRectangle(CurrentPen, _mxClick * _rw + offx, _myClick * _rh + offy, _rw - 1, _rh - 1);
			}

			// draw hover indicators

			if ((_mousePresent || _menuShowing) && _mx > -1 && _mx < _mw && _my > -1 && _my < 8)
			{
				float x0 = _mx * _rw + offx;
				float y0 = _my * _rh + offy;

				g.FillRectangle(CursorBrush, x0, y0, _rw, _rh);

				var props = GetProps(_mx, _my);

				if (props != null)
				{
					if (_node == 'N')
					{
						g.FillRectangle(CursorBrush, x0 + _rw / 4f, y0, _rw / 2f, _rh / 4f);
					}
					else if (_node == 'S')
					{
						g.FillRectangle(CursorBrush, x0 + _rw / 4f, y0 + 3 * _rh / 4, _rw / 2f, _rh / 4f);
					}
					else if (_node == 'W')
					{
						g.FillRectangle(CursorBrush, x0, y0 + _rh / 4, _rw / 4f, _rh / 2f);
					}
					else if (_node == 'E')
					{
						g.FillRectangle(CursorBrush, x0 + 3 * _rw / 4, y0 + _rh / 4, _rw / 4f, _rh / 2f);
					}

					var roomState = _roomStates[_my, _mx];

					if (roomState.Transport != null)
					{
						var rooms = FindTransportRooms(roomState.Transport);
						if (rooms.Count == 2)
						{
							g.AntiAlias = true;
							float tx0 = rooms[0].X * 64 + offx + 32;
							float ty0 = rooms[0].Y * 44 + offy + 22;
							float tx1 = rooms[1].X * 64 + offx + 32;
							float ty1 = rooms[1].Y * 44 + offy + 22;
							g.DrawLine(new Pen(Colors.Black, 3), tx0, ty0, tx1, ty1);
							g.DrawLine(new Pen(Colors.CornflowerBlue, 2), tx0, ty0, tx1, ty1);
						}
					}
				}
			}
		}

		#endregion

		#region CoOp Event Handlers

		private void HandleCoOpClientFound(object sender, FoundEventArgs e)
		{
			Debug.Assert(_mapKey != null && _mapKey.Length == 2);

			if (e.Game == Game && e.Map == _mapKey)
			{
				var roomState = _roomStates[e.Y, e.X];

				if (e.Type == "dest")
				{
					var dest = _dests.Find(d => d.GetCode() == e.Code);

					if (e.Slot == 0)
						roomState.DestNorth = dest;
					else if (e.Slot == 1)
						roomState.DestSouth = dest;
					else if (e.Slot == 2)
						roomState.DestWest = dest;
					else if (e.Slot == 3)
						roomState.DestEast = dest;
				}
				else if (e.Type == "item")
				{
					var item = _items.Find(i => i.GetCode() == e.Code);

					if (e.Slot == 0)
						roomState.Item1 = item;
					else if (e.Slot == 1)
						roomState.Item2 = item;
				}
				else if (e.Type == "wall")
				{
					var wall = _walls.Find(w => w.Code == e.Code);

					if (e.Slot == 0)
						roomState.WallNorth = wall;
					else if (e.Slot == 1)
						roomState.WallSouth = wall;
					else if (e.Slot == 2)
						roomState.WallWest = wall;
					else if (e.Slot == 3)
						roomState.WallEast = wall;
				}
				else if (e.Type == "stair")
				{
					roomState.Transport = e.Code;
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
			int m = _mw / 2;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < m; x++)
				{
					var s0 = _roomStates[y, x];
					var s1 = _roomStates[y, (_mw - 1) - x];

					s0.Mirror();
					s1.Mirror();

					_roomStates[y, x] = s1;
					_roomStates[y, (_mw - 1) - x] = s0;
				}
			}

			_drawable.Invalidate();
		}
	}
}
