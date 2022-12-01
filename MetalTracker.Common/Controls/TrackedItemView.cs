using Eto.Drawing;
using Eto.Forms;

namespace MetalTracker.Common.Controls
{
	public class TrackedItemView : Drawable
	{
		private bool _mousePresent;

		public string ItemKey { get; set; }
		public Image[] Images { get; set; }
		public int ItemLevel { get; set; }

		public TrackedItemView(string itemKey, Image[] images)
		{
			this.ItemKey = itemKey;
			this.Images = images;
			this.Width = 40;
			this.Height = 36;
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			_mousePresent = true;
			this.Invalidate();
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			_mousePresent = false;
			this.Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Buttons == MouseButtons.Primary)
			{
				int maxLevel = Images.Length - 1;
				if (ItemLevel < maxLevel)
				{
					this.ItemLevel = this.ItemLevel + 1;
				}
			}
			else if (e.Buttons == MouseButtons.Alternate)
			{
				if (ItemLevel > 0)
				{
					this.ItemLevel = this.ItemLevel - 1;
				}
			}

			this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_mousePresent)
				e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 0, 85)), 2, 0, 36, 36);

			var image = this.Images[this.ItemLevel];
			e.Graphics.AntiAlias = false;
			e.Graphics.ImageInterpolation = ImageInterpolation.None;
			e.Graphics.DrawImage(image, 2, 0, 36, 36);
		}
	}
}
