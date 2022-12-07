using System.Collections.Generic;
using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Types
{
	public class DungeonMapState
	{
		public List<StateEntry> Dests { get; set; } = new List<StateEntry>();

		public List<StateEntry> Items { get; set; } = new List<StateEntry>();

		public List<StateEntry> Stairs { get; set; } = new List<StateEntry>();
	}
}
