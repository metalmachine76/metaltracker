using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using MetalTracker.Common.Types;

namespace MetalTracker.Common.Bases
{
	public abstract class BaseMap
	{
		protected readonly float[] _roomWidths;
		protected readonly float[] _roomHeights;
		protected readonly Drawable _drawable;

		protected int _zoom;

		protected float _rw;
		protected float _rh;

		protected int _mw;
		protected int _mh;

		protected bool _active;
		protected bool _mousePresent;
		protected bool _mouseDown;
		protected PointF _mouseDownLoc;
		protected PointF _mouseLoc;
		protected PointF _offset = new PointF(64, 64);

		protected int _my = -1;
		protected int _mx = -1;
		protected char _node = '\0';

		protected int _myClick = -1;
		protected int _mxClick = -1;
		protected char _nodeClick = '\0';

		protected UITimer _timer;
		protected bool _invalidateMap;
		protected bool _invalidateRoom;

		protected BaseMap(float rw0, float rh0, int zoom, Drawable drawable)
		{
			_roomWidths = new float[7];

			_roomWidths[0] = 1.0f * rw0;
			_roomWidths[1] = 1.5f * rw0;
			_roomWidths[2] = 2.0f * rw0;
			_roomWidths[3] = 3.0f * rw0;
			_roomWidths[4] = 4.0f * rw0;
			_roomWidths[5] = 6.0f * rw0;
			_roomWidths[6] = 8.0f * rw0;

			_roomHeights = new float[7];

			_roomHeights[0] = 1.0f * rh0;
			_roomHeights[1] = 1.5f * rh0;
			_roomHeights[2] = 2.0f * rh0;
			_roomHeights[3] = 3.0f * rh0;
			_roomHeights[4] = 4.0f * rh0;
			_roomHeights[5] = 6.0f * rh0;
			_roomHeights[6] = 8.0f * rh0;

			_zoom = zoom;

			_rw = _roomWidths[zoom];
			_rh = _roomHeights[zoom];

			_drawable = drawable;

			_drawable.MouseDown += HandleMouseDown;
			_drawable.MouseUp += HandleMouseUp;
			_drawable.MouseLeave += HandleMouseLeave;
			_drawable.MouseMove += HandleMouseMove;
			_drawable.MouseDoubleClick += HandleMouseDoubleClick;
			_drawable.Paint += HandlePaint;

			_timer = new UITimer();
			_timer.Interval = 0.5;
			_timer.Elapsed += HandleTimerElapsed;
			_timer.Start();
		}

		public void LocateRoom(int x, int y)
		{
			_mxClick = x;
			_myClick = y;
			_offset.X = 256 - _rw * x - _rw / 2;
			_offset.Y = 240 - _rh * y - _rh / 2;
			_drawable.Invalidate();
			HandleRoomClick(false);
		}

		public void SetZoom(int zoom)
		{
			var dcx = _offset.X - 256;
			var dcy = _offset.Y - 240;

			var rw0 = _rw;
			var rh0 = _rh;

			_zoom = zoom;
			_rw = _roomWidths[zoom];
			_rh = _roomHeights[zoom];

			var ratiox = _rw / rw0;
			var ratioy = _rh / rh0;

			dcx = dcx * ratiox;
			dcy = dcy * ratioy;

			_offset.X = (int)(256 + dcx);
			_offset.Y = (int)(240 + dcy);

			_drawable.Invalidate();
		}

		public int GetZoom()
		{
			return _zoom;
		}

		public abstract void Activate(bool active);

		public abstract List<LocationOfDest> LogExitLocations();

		public abstract List<LocationOfItem> LogItemLocations();

		protected void DrawExit(Graphics g, float x0, float y0, float rw, float rh, GameDest dest)
		{
			Brush textBrush = Brushes.Lime;
			Font textFont = Fonts.Sans(14);
			DrawText(g, x0, y0, rw, rh, dest.ShortName, textFont, textBrush);
		}

		protected void DrawText(Graphics g, float x0, float y0, float rw, float rh, string text, Font font, Brush brush)
		{
			RectangleF rect = new RectangleF(0, 0, rw, rh);

			rect.X = x0 - 1;
			rect.Y = y0;
			g.DrawText(font, Brushes.Black, rect, text, alignment: FormattedTextAlignment.Center);

			rect.X = x0 + 1;
			rect.Y = y0;
			g.DrawText(font, Brushes.Black, rect, text, alignment: FormattedTextAlignment.Center);

			rect.X = x0;
			rect.Y = y0 - 1;
			g.DrawText(font, Brushes.Black, rect, text, alignment: FormattedTextAlignment.Center);

			rect.X = x0;
			rect.Y = y0 + 1;
			g.DrawText(font, Brushes.Black, rect, text, alignment: FormattedTextAlignment.Center);

			rect.X = x0;
			rect.Y = y0;
			g.DrawText(font, brush, rect, text, alignment: FormattedTextAlignment.Center);
		}

		protected void DrawCenteredImage(Graphics g, float x0, float y0, float w, float h, Image image)
		{
			var x = x0 + w / 2f - image.Width / 2f;
			var y = y0 + h / 2f - image.Height / 2f;
			g.DrawImage(image, x, y);
		}

		#region Event Handlers

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

			var maxx = 256 - _rw / 2;
			var maxy = 240 - _rh / 2;

			if (_offset.X > maxx) _offset.X = maxx;
			if (_offset.Y > maxy) _offset.Y = maxy;

			var minx = 256 - _rw * (_mw - 1) - _rw / 2;
			var miny = 240 - _rh * (_mh - 1) - _rh / 2;

			if (_offset.X < minx) _offset.X = minx;
			if (_offset.Y < miny) _offset.Y = miny;

			HandleMouseMove(sender, e);
			_mxClick = _mx;
			_myClick = _my;
			_nodeClick = _node;
			_drawable.Invalidate();
			HandleRoomClick(e.Buttons == MouseButtons.Alternate);
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			if (!_active) return;

			_mousePresent = true;

			_mouseLoc = e.Location;

			if (!_mouseDown)
			{
				float dx = (_mouseLoc.X - _offset.X);
				float dy = (_mouseLoc.Y - _offset.Y);

				_mx = dx < 0f ? -1 : (int)(dx / _rw);
				_my = dy < 0f ? -1 : (int)(dy / _rh);

				_node = DetermineNode(dx % _rw, dy % _rh);
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

			HandleRoomDoubleClick();

			_drawable.Invalidate();
		}

		private void HandlePaint(object? sender, PaintEventArgs e)
		{
			if (!_active) return;

			float offx = _offset.X;
			float offy = _offset.Y;

			if (_mouseDown)
			{
				offx = offx + _mouseLoc.X - _mouseDownLoc.X;
				offy = offy + _mouseLoc.Y - _mouseDownLoc.Y;
			}

			PaintMap(e.Graphics, (int)offx, (int)offy);
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
				HandleRoomClick(false);
				_invalidateRoom = false;
			}
		}

		#endregion

		protected abstract void HandleRoomClick(bool altButton);

		protected abstract void HandleRoomDoubleClick();

		protected abstract void PaintMap(Graphics g, float offx, float offy);

		protected virtual char DetermineNode(float x, float y)
		{
			return '\0';
		}
	}
}
