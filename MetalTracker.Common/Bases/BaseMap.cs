using Eto.Drawing;
using Eto.Forms;
using MetalTracker.Common.Types;

namespace MetalTracker.Common.Bases
{
	public abstract class BaseMap
	{
		protected readonly int _rw;
		protected readonly int _rh;
		protected readonly Drawable _drawable;

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

		protected BaseMap(int roomWidth, int roomHeight, Drawable drawable)
		{
			_rw = roomWidth;
			_rh = roomHeight;
			_drawable = drawable;

			_drawable.MouseDown += HandleMouseDown;
			_drawable.MouseUp += HandleMouseUp;
			_drawable.MouseLeave += HandleMouseLeave;
			_drawable.MouseMove += HandleMouseMove;
			_drawable.MouseDoubleClick += HandleMouseDoubleClick;
			_drawable.Paint += HandlePaint;
		}

		public abstract string GetMapKey();

		public abstract List<LocationOfDest> LogExitLocations();

		public abstract List<LocationOfItem> LogItemLocations();

		protected PointF CalcPaintOrigin()
		{
			float offx = _offset.X;
			float offy = _offset.Y;

			if (_mouseDown)
			{
				offx = offx + _mouseLoc.X - _mouseDownLoc.X;
				offy = offy + _mouseLoc.Y - _mouseDownLoc.Y;
			}

			return new PointF(offx, offy);
		}

		protected void DrawDest(Graphics g, float x0, float y0, float rw, float rh, GameDest dest)
		{
			Brush textBrush = dest.IsExit ? Brushes.Lime : Brushes.White;
			Font textFont = dest.IsExit ? Fonts.Sans(14) : Fonts.Sans(12);
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
			//if (_offset.X > 64) _offset.X = 64;
			//if (_offset.X < -576) _offset.X = -576;
			//if (_offset.Y > 64) _offset.Y = 64;
			//if (_offset.Y < -544) _offset.Y = -544;
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
				int dx = (int)(_mouseLoc.X - _offset.X);
				int dy = (int)(_mouseLoc.Y - _offset.Y);

				var qrx = Math.DivRem(dx, _rw);
				var qry = Math.DivRem(dy, _rh);

				_mx = dx < 0f ? -1 : qrx.Quotient;
				_my = dy < 0f ? -1 : qry.Quotient;

				_node = DetermineNode(qrx.Remainder, qry.Remainder);
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

			PaintMap(e.Graphics, offx, offy);
		}

		protected abstract void HandleRoomClick(bool altButton);

		protected abstract void HandleRoomDoubleClick();

		protected abstract void PaintMap(Graphics g, float offx, float offy);

		protected virtual char DetermineNode(int x, int y)
		{
			return '\0';
		}
	}
}
