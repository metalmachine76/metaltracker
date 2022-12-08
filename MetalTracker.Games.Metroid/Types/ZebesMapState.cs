using System.Collections.Generic;
using MetalTracker.Common.Types;

namespace MetalTracker.Games.Metroid.Types
{
	public class ZebesMapState
	{
		public List<StateEntry> Dests { get; set; } = new List<StateEntry>();

		public List<StateEntry> Items { get; set; } = new List<StateEntry>();
	}
}
