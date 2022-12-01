using System;
using System.Threading;
using Eto.Forms;

namespace MetalTracker.Trackers.Z1M1
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
#if GTK
			var platform = new Eto.GtkSharp.Platform();
			SynchronizationContext.SetSynchronizationContext(new GLib.GLibSynchronizationContext());
			new Application(platform).Run(new MainForm());
#endif
#if WPF
			new Application(Eto.Platform.Detect).Run(new MainForm());
#endif
		}
	}
}
