using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using MetalTracker.Common.Bases;
using MetalTracker.Common.Types;
using MetalTracker.CoOp;
using MetalTracker.CoOp.EventArgs;
using MetalTracker.Games.Metroid.Internal;
using MetalTracker.Games.Metroid.Types;

namespace MetalTracker.Games.Metroid.Proxies
{
	public class ZebesMap : BaseMap
	{
		const string Game = "metroid";
		const string Map = "zebes";

		static SolidBrush ShadowBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 152));
		static SolidBrush CursorBrush = new SolidBrush(Color.FromArgb(250, 250, 250, 102));
		static Pen CurrentPen = new Pen(Colors.White, 1) { DashStyle = DashStyles.Dot };

		private readonly Drawable _drawable;
		private readonly ZebesRoomDetail _zebesRoomDetail;

		private bool _flag_mirrored;
		private Image _mapImage;
		private ZebesRoomProps[,] _meta;

		private List<GameDest> _dests = new List<GameDest>();
		private ContextMenu _destsMenu;
		private List<GameItem> _items = new List<GameItem>();
		private UITimer _timer;
		private ZebesRoomStateMutator _mutator = new ZebesRoomStateMutator();

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

		private ZebesRoomState[,] _roomStates = new ZebesRoomState[32, 32];

		#region Public Methods

		public ZebesMap(Drawable drawable, Panel detailPanel)
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

			_zebesRoomDetail = new ZebesRoomDetail(detailPanel, _mutator);
			_zebesRoomDetail.DetailChanged += HandleRoomDetailChanged;

			ResetState();
		}

		public void SetMapFlags(bool mirrored)
		{
			_flag_mirrored = mirrored;
			_mapImage = InternalResourceClient.GetZebesImage(mirrored);
			_meta = InternalResourceClient.GetZebesMeta(mirrored);
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
			var mdx = dx < 0f ? -1 : 32 * (int)dx / 1024;
			var mdy = dy < 0f ? -1 : 32 * (int)dy / 960;

			if (mdx > -1 && mdx < 32 && mdy > -1 && mdy < 32)
			{
				var cmd = sender as Command;
				var dest = cmd.CommandParameter as GameDest;
				var roomState = _roomStates[mdy, mdx];
				_mutator.ChangeDestination(mdx, mdy, roomState, dest);
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
			_offset.Y = _offset.Y + _mouseLoc.Y - _mouseDownLoc.Y;
			_offset.X = _offset.X + _mouseLoc.X - _mouseDownLoc.X;
			if (_offset.X > 64) _offset.X = 64;
			if (_offset.X < -576) _offset.X = -576;
			if (_offset.Y > 64) _offset.Y = 64;
			if (_offset.Y < -544) _offset.Y = -544;
			HandleMouseMove(sender, e);
			_mxClick = _mx;
			_myClick = _my;
			_drawable.Invalidate();
			if (_mxClick > -1 && _myClick > -1 && _mxClick < 32 && _myClick < 32)
			{
				var roomProps = GetMeta(_mxClick, _myClick);
				var roomState = _roomStates[_myClick, _mxClick];
				_zebesRoomDetail.UpdateDetails(_mxClick, _myClick, roomProps, roomState);
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
				_mx = dx < 0f ? -1 : 32 * (int)dx / 1024;
				_my = dy < 0f ? -1 : 32 * (int)dy / 960;
			}

			_drawable.ContextMenu = null;

			if (_mx > -1 && _mx < 32 && _my > -1 && _my < 32)
			{
				var props = GetMeta(_mx, _my);

				if (props.CanHaveDest())
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

			if (_mx > -1 && _mx < 32 && _my > -1 && _my < 32)
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
				offy = offy + _mouseLoc.Y - _mouseDownLoc.Y;
			}

			if (_mapImage != null)
			{
				e.Graphics.DrawImage(_mapImage, offx, offy, 1024, 960);
			}

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					var roomState = _roomStates[y, x];

					float x0 = x * 32 + offx;
					float y0 = y * 30 + offy;

					var props = GetMeta(x, y);

					if (props.SlotClass != '\0' && roomState.Item == null)
					{
						DrawText(e.Graphics, x0, y0, 32, 30, props.SlotClass.ToString(), Fonts.Sans(12), Brushes.White);
					}

					if (roomState.DestElev != null)
					{
						DrawDest(e.Graphics, x0 - 16, y0, 64, 30, roomState.DestElev);
					}

					if (roomState.Item != null)
					{
						e.Graphics.DrawImage(roomState.Item.Icon, x0 + 7, y0 + 6, 18, 18);
					}

					if (roomState.Explored)
					{
						e.Graphics.FillRectangle(ShadowBrush, x0, y0, 32, 30);
					}
				}
			}

			if ((_mousePresent || _menuShowing) && _mx > -1 && _mx < 32 && _my > -1 && _my < 32)
			{
				e.Graphics.FillRectangle(CursorBrush, _mx * 32 + offx, _my * 30 + offy, 32, 30);
			}

			if (_mxClick > -1 && _myClick > -1 && _mxClick < 32 && _myClick < 32)
			{
				e.Graphics.DrawRectangle(CurrentPen, _mxClick * 32 + offx, _myClick * 30 + offy, 31, 29);
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

		private ZebesRoomProps GetMeta(int x, int y)
		{
			return _meta[y, x];
		}

		#endregion

		#region CoOp Send/Recieve

		private void HandleCoOpClientFoundDest(object sender, FoundEventArgs e)
		{
			if (e.Game == Game && e.Map == Map)
			{
				var roomState = _roomStates[e.Y, e.X];
				var dest = _dests.Find(d => d.GetCode() == e.Code);

				roomState.DestElev = dest;

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

				roomState.Item = item;

				_invalidateMap = true;

				if (e.X == _mxClick && e.Y == _myClick)
				{
					_invalidateRoom = true;
				}
			}
		}

		#endregion
	}
}
