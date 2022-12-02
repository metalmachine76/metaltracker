using System.Collections.Generic;
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
	public class OverworldMap : BaseMap
	{
		const string Game = "zelda";
		const string Map = "ow";

		static SolidBrush ShadowBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 153));
		static SolidBrush CursorBrush = new SolidBrush(Color.FromArgb(250, 250, 250, 153));
		static Pen CurrentPen = new Pen(Colors.White, 1) { DashStyle = DashStyles.Dot };

		private readonly Drawable _drawable;
		private readonly OverworldRoomDetail _overworldRoomDetail;

		private bool _flag_q2;
		private bool _flag_mirrored;
		private Image _mapImage;
		private OverworldRoomProps[,] _meta;

		private List<GameDest> _dests = new List<GameDest>();
		private ContextMenu _destsMenu;
		private List<GameItem> _items = new List<GameItem>();
		private UITimer _timer;
		private OverworldRoomStateMutator _mutator = new OverworldRoomStateMutator();

		private bool _active;
		private bool _mousePresent;
		private bool _mouseDown;
		private PointF _mouseDownLoc;
		private PointF _mouseLoc;
		private PointF _offset = new PointF(64, 64);
		private bool _menuShowing;
		private int _my = -1;
		private int _mx = -1;
		private int _myClick = -1;
		private int _mxClick = -1;
		private bool _invalidateMap;
		private bool _invalidateRoom;

		private OverworldRoomState[,] _roomStates = new OverworldRoomState[8, 16];

		#region Public Methods

		public OverworldMap(Drawable drawable, Panel detailPanel)
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

			_overworldRoomDetail = new OverworldRoomDetail(detailPanel, _mutator);
			_overworldRoomDetail.DetailChanged += HandleRoomDetailChanged;

			ResetState();
		}

		public void SetMapFlags(bool q2, bool mirrored)
		{
			if (mirrored != _flag_mirrored)
			{
				MirrorState();
			}

			_flag_q2 = q2;
			_flag_mirrored = mirrored;
			_mapImage = InternalResourceClient.GetOverworldImage(q2, mirrored);
			_meta = InternalResourceClient.GetOverworldMeta(q2, mirrored);
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
				_overworldRoomDetail.Build(_dests, _items);
			}
		}

		public void ResetState()
		{
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					_roomStates[y, x] = new OverworldRoomState();
				}
			}

			_drawable.Invalidate();
		}

		public override List<StateEntry> GetDestStates()
		{
			List<StateEntry> list = new List<StateEntry>();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					var state = _roomStates[y, x];
					if (state.Destination != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 0, Code = state.Destination.GetCode() };
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
				for (int x = 0; x < 16; x++)
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
					if (state.Item3 != null)
					{
						StateEntry entry = new StateEntry { X = x, Y = y, Slot = 2, Code = state.Item3.GetCode() };
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
				_roomStates[entry.Y, entry.X].Destination = _dests.Find(i => i.GetCode() == entry.Code);
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
				else if (entry.Slot == 2)
					_roomStates[entry.Y, entry.X].Item3 = _items.Find(i => i.GetCode() == entry.Code);
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
						LocationOfItem loc = new LocationOfItem(state.Item1, $"Overworld at {y:X1}{x:X1}");
						list.Add(loc);
					}
					if (state.Item2 != null && state.Item2.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item2, $"Overworld at {y:X1}{x:X1}");
						list.Add(loc);
					}
					if (state.Item3 != null && state.Item3.IsImportant())
					{
						LocationOfItem loc = new LocationOfItem(state.Item3, $"Overworld at {y:X1}{x:X1}");
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
						LocationOfDest loc = new LocationOfDest(state.Destination, $"Overworld at {y:X1}{x:X1}");
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
			float dx = _mouseDownLoc.X - _offset.X;
			float dy = _mouseDownLoc.Y - _offset.Y;
			var mdx = dx < 0f ? -1 : 16 * (int)dx / 1024;
			var mdy = dy < 0f ? -1 : 8 * (int)dy / 352;

			if (mdx > -1 && mdx < 16 && mdy > -1 && mdy < 8)
			{
				var cmd = sender as Command;
				var dest = cmd.CommandParameter as GameDest;
				var roomState = _roomStates[mdy, mdx];
				_mutator.ChangeDestination(mdx, mdy, roomState, dest);
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
			_offset.Y = 64;
			_offset.X = _offset.X + _mouseLoc.X - _mouseDownLoc.X;
			if (_offset.X > 64) _offset.X = 64;
			if (_offset.X < -576) _offset.X = -576;
			HandleMouseMove(sender, e);
			_mxClick = _mx;
			_myClick = _my;
			_drawable.Invalidate();
			if (_mxClick > -1 && _myClick > -1 && _mxClick < 16 && _myClick < 8)
			{
				var roomProps = GetMeta(_mxClick, _myClick);
				var roomState = _roomStates[_myClick, _mxClick];
				_overworldRoomDetail.UpdateDetails(_mxClick, _myClick, roomProps, roomState);
			}
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			if (!_active) return;

			_mousePresent = true;

			_mouseLoc = e.Location;

			if (!_mouseDown)
			{
				float dx = _mouseLoc.X - _offset.X;
				float dy = _mouseLoc.Y - _offset.Y;
				_mx = dx < 0f ? -1 : 16 * (int)dx / 1024;
				_my = dy < 0f ? -1 : 8 * (int)dy / 352;
			}

			_drawable.ContextMenu = null;

			if (_mx > -1 && _mx < 16 && _my > -1 && _my < 8)
			{
				if (GetMeta(_mx, _my).DestHere)
				{
					_drawable.ContextMenu = _destsMenu;
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

			if (_mx > -1 && _mx < 16 && _my > -1 && _my < 8)
			{
				_roomStates[_my, _mx].Explored = !_roomStates[_my, _mx].Explored;
				_drawable.Invalidate();
			}
		}

		private void HandlePaint(object sender, PaintEventArgs e)
		{
			if (!_active) return;

			var offx = _offset.X;
			var offy = _offset.Y;

			if (_mouseDown)
			{
				offx = offx + _mouseLoc.X - _mouseDownLoc.X;
			}

			if (_mapImage != null)
			{
				e.Graphics.DrawImage(_mapImage, offx, offy, 1024, 352);
			}

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					var roomState = _roomStates[y, x];

					float x0 = x * 64 + offx;
					float y0 = y * 44 + offy;

					var props = GetMeta(x, y);

					if (!props.DestHere && !props.ItemHere)
					{
						e.Graphics.FillRectangle(ShadowBrush, x0, y0, 64, 44);
						continue;
					}

					if (roomState.Item1 != null)
					{
						e.Graphics.DrawImage(roomState.Item1.Icon, x0 + 3, y0 + 23, 18, 18);
					}

					if (roomState.Item2 != null)
					{
						if (roomState.Destination == null)
						{
							e.Graphics.DrawImage(roomState.Item2.Icon, x0 + 23, y0 + 13, 18, 18);
						}
						else
						{
							e.Graphics.DrawImage(roomState.Item2.Icon, x0 + 23, y0 + 23, 18, 18);
						}
					}

					if (roomState.Item3 != null)
					{
						e.Graphics.DrawImage(roomState.Item3.Icon, x0 + 43, y0 + 23, 18, 18);
					}

					if (roomState.Destination != null && !roomState.Destination.IsExit)
					{
						var dest = roomState.Destination;

						DrawDest(e.Graphics, x0, y0, 64, 44, dest);
					}

					if ((props.DestHere || props.ItemHere) && roomState.Explored)
					{
						e.Graphics.FillRectangle(ShadowBrush, x0, y0, 64, 44);
					}

					if (roomState.Destination != null && roomState.Destination.IsExit)
					{
						var dest = roomState.Destination;

						DrawDest(e.Graphics, x0, y0, 64, 44, dest);
					}
				}
			}

			if ((_mousePresent || _menuShowing) && _mx > -1 && _mx < 16 && _my > -1 && _my < 8)
			{
				e.Graphics.FillRectangle(CursorBrush, _mx * 64 + offx, _my * 44 + offy, 64, 44);
			}

			if (_mxClick > -1 && _myClick > -1 && _mxClick < 16 && _myClick < 8)
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
				_overworldRoomDetail.Refresh();
				_invalidateRoom = false;
			}
		}

		private OverworldRoomProps GetMeta(int x, int y)
		{
			return _meta[y, x];
		}

		#endregion

		#region CoOp Event Handlers

		private void HandleCoOpClientFoundDest(object sender, FoundEventArgs e)
		{
			if (e.Game == Game && e.Map == Map)
			{
				var roomState = _roomStates[e.Y, e.X];
				var dest = _dests.Find(d => d.GetCode() == e.Code);
				roomState.Destination = dest;

				_invalidateMap = true;

				if (e.X == _mxClick && e.Y == _myClick)
				{
					_invalidateRoom = true;
				}
			}
		}

		private void HandleCoOpClientFoundItem(object sender, FoundEventArgs e)
		{
			if (e.Game == Game && e.Map == Map)
			{
				var roomState = _roomStates[e.Y, e.X];
				var item = _items.Find(i => i.GetCode() == e.Code);

				if (e.Slot == 0)
					roomState.Item1 = item;
				if (e.Slot == 1)
					roomState.Item2 = item;
				if (e.Slot == 2)
					roomState.Item3 = item;

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
