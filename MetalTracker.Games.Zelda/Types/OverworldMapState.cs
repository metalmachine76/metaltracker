using System.Collections.Generic;
using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Types
{
	public class OverworldMapState
	{
		public List<StateEntry> Exits { get; set; } = new List<StateEntry>();

		public List<StateEntry> Items { get; set; } = new List<StateEntry>();

		public List<StateEntry> Caves { get; set; } = new List<StateEntry>();

		public List<StateEntry> Explored { get; set; } = new List<StateEntry>();
	}
}
