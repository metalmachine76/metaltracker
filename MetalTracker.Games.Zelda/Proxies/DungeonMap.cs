using Eto.Forms;
using MetalTracker.CoOp;
using MetalTracker.Games.Zelda.Types;

namespace MetalTracker.Games.Zelda.Proxies
{
	public class DungeonMap
	{
		private readonly Drawable _drawable;
		private readonly Panel _detailPanel;

		private bool _active;

		private int _w;

		private int _my;
		private int _mx;

		private DungeonRoomState[,] _roomStates = new DungeonRoomState[8, 12];

		public DungeonMap(Drawable drawable, Panel detailPanel)
		{
			_drawable = drawable;
			_detailPanel = detailPanel;
		}

		public void SetCoOpClient(CoOpClient coOpClient)
		{
			//coOpClient.FoundDest += HandleCoOpClientFoundDest;
			//coOpClient.FoundItem += HandleCoOpClientFoundItem;
			//_mutator.SetCoOpClient(coOpClient);
			//_timer = new UITimer();
			//_timer.Interval = 0.5;
			//_timer.Elapsed += HandleTimerElapsed;
			//_timer.Start();
		}

		public void Activate(bool active)
		{
			_active = active;
			_drawable.Invalidate();
			_detailPanel.Content = new Label { Text = "DUNGEON ROOM DETAIL" };
		}
	}
}
