using System;
using Eto.Forms;
using MetalTracker.Trackers.Z1M1.Dialogs;

namespace MetalTracker.Trackers.Z1M1.Wpf
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				var platform = new Eto.Wpf.Platform();
				new Application(platform).Run(new MainForm());
			}
			catch (Exception ex)
			{
				FatalErrorDlg dlg = new FatalErrorDlg();
				dlg.SetException(ex);
				dlg.ShowModal();
			}
		}
	}
}
