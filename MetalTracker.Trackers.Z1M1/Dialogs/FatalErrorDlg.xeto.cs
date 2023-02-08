using System;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace MetalTracker.Trackers.Z1M1.Dialogs
{
	public class FatalErrorDlg : Dialog
	{
		private Exception _ex;

		public FatalErrorDlg()
		{
			XamlReader.Load(this);
		}

		public void SetException(Exception ex)
		{
			_ex = ex;
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			if (_ex != null)
			{
				this.FindChild<TextArea>("textAreaDetails").Text = _ex.ToString() + "\r\n";
			}
		}
	}
}
