using Eto.Drawing;
using MetalTracker.Common.Types;

namespace MetalTracker.Common.Bases
{
	public abstract class BaseMap
	{
		public abstract string GetMapKey();

		public abstract List<LocationOfDest> LogExitLocations();

		public abstract List<LocationOfItem> LogItemLocations();

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
	}
}
