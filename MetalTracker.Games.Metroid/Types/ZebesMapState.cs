using System.Collections.Generic;
using MetalTracker.Common.Types;

namespace MetalTracker.Games.Metroid.Types
{
	public class ZebesMapState
	{
		public List<StateEntry> Exits { get; set; } = new List<StateEntry>();

		public List<StateEntry> Items { get; set; } = new List<StateEntry>();

		public List<StateEntry> Status { get; set; } = new List<StateEntry>();
	}
}
