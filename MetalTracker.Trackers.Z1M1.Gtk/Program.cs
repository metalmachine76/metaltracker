using System;
using System.Threading;
using Eto.Forms;
using MetalTracker.Trackers.Z1M1.Dialogs;

namespace MetalTracker.Trackers.Z1M1.Gtk
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				var platform = new Eto.GtkSharp.Platform();
				SynchronizationContext.SetSynchronizationContext(new GLib.GLibSynchronizationContext());
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
