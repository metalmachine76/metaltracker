using System.Collections.Generic;
using MetalTracker.Common.Types;

namespace MetalTracker.Trackers.Z1M1.Internal
{
	internal class SessionState
	{
		public List<StateEntry>[] DestStateLists { get; set; } = new List<StateEntry>[11];

		public List<StateEntry>[] ItemStateLists { get; set; } = new List<StateEntry>[11];
	}
}
