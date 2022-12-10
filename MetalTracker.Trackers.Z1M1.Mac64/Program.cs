using System;
using Eto.Forms;

namespace MetalTracker.Trackers.Z1M1.Mac64
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var platform = new Eto.Mac.Platform();
			new Application(platform).Run(new MainForm());
		}
	}
}
