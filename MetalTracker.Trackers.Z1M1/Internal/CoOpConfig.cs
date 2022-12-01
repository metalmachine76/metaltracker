using System;
using System.IO;
using MetalTracker.CoOp;

namespace MetalTracker.Trackers.Z1M1.Internal
{
	internal class CoOpConfig
	{
		public string PlayerId { get; private set; }

		public string PlayerName { get; set; }

		public string PlayerColor { get; set; }

		public CoOpConfig()
		{
			this.PlayerId = Guid.NewGuid().ToString("N");
			this.PlayerName = "";
			this.PlayerColor = "FFFFFF";
		}

		public void RestoreFrom(string filename)
		{
			string[] lines = File.ReadAllLines(filename);
			if (lines.Length == 3)
			{
				this.PlayerId = lines[0];
				this.PlayerName = lines[1];
				this.PlayerColor = lines[2];
			}
		}

		public void PersistTo(string filename)
		{
			string[] lines = new string[3];
			lines[0] = this.PlayerId;
			lines[1] = this.PlayerName;
			lines[2] = this.PlayerColor;
			File.WriteAllLines(filename, lines);
		}
	}
}
