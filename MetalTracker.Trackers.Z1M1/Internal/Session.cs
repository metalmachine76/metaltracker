using System.Collections.Generic;

namespace MetalTracker.Trackers.Z1M1.Internal
{
	internal class Session
	{
		public SessionFlags Flags { get; set; }

		public SessionState State { get; set; }

		public List<InventoryEntry> Inventory { get; set; }
	}
}
