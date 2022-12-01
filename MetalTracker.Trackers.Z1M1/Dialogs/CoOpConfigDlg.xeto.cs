using System;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using MetalTracker.Trackers.Z1M1.Internal;

namespace MetalTracker.Trackers.Z1M1.Dialogs
{
	internal class CoOpConfigDlg : Dialog<bool>
	{
		public CoOpConfig Config { get; set; }

		public CoOpConfigDlg()
		{
			XamlReader.Load(this);
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			if (this.Config != null)
			{
				this.FindChild<TextBox>("textBoxPlayerName").Text = this.Config.PlayerName;
				this.FindChild<ColorPicker>("colorPickerColor").Value = FromHex(this.Config.PlayerColor);
			}
		}

		protected void HandleNameChanged(object sender, EventArgs e)
		{
			this.FindChild<Button>("buttonAccept").Enabled = this.FindChild<TextBox>("textBoxPlayerName").Text.Length > 0;
		}

		protected void HandleAcceptClick(object sender, EventArgs e)
		{
			if (this.Config == null)
			{
				this.Config = new CoOpConfig();
			}

			this.Config.PlayerName = this.FindChild<TextBox>("textBoxPlayerName").Text;
			this.Config.PlayerColor = this.FindChild<ColorPicker>("colorPickerColor").Value.ToHex(false).Substring(1);

			this.Result = true;

			this.Close();
		}

		private Color FromHex(string hex)
		{
			int argb = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
			Color c = Color.FromArgb(argb);
			c.Ab = 255;
			return c;
		}
	}
}
