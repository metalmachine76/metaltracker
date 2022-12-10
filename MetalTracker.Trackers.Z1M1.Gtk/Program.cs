using System;
using System.Threading;
using Eto.Forms;

namespace MetalTracker.Trackers.Z1M1.Gtk
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var platform = new Eto.GtkSharp.Platform();
			SynchronizationContext.SetSynchronizationContext(new GLib.GLibSynchronizationContext());
			new Application(platform).Run(new MainForm());
		}
	}
}
