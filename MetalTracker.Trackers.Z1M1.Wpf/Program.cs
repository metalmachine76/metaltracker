using Eto.Forms;

namespace MetalTracker.Trackers.Z1M1.Wpf
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var platform = new Eto.Wpf.Platform();
			new Application(platform).Run(new MainForm());
		}
	}
}
