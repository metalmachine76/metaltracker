using MetalTracker.Common.Types;
using MetalTracker.CoOp.Interface;
using MetalTracker.Games.Zelda.Types;

namespace MetalTracker.Games.Zelda.Internal
{
	internal class DungeonRoomStateMutator
	{
		const string Game = "zelda";

		private ICoOpClient _coOpClient;

		public void SetCoOpClient(ICoOpClient coOpClient)
		{
			_coOpClient = coOpClient;
		}

		public void ChangeDestNorth(int w, int x, int y, DungeonRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.DestNorth = newDest;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeDestSouth(int w, int x, int y, DungeonRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.DestSouth = newDest;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeDestWest(int w, int x, int y, DungeonRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.DestWest = newDest;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeDestEast(int w, int x, int y, DungeonRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.DestEast = newDest;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeItem1(int w, int x, int y, DungeonRoomState state, GameItem newItem)
		{
			var oldState = state.Clone();
			state.Item1 = newItem;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeItem2(int w, int x, int y, DungeonRoomState state, GameItem newItem)
		{
			var oldState = state.Clone();
			state.Item2 = newItem;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		private void SendCoOpUpdates(int w, int x, int y, DungeonRoomState oldState, DungeonRoomState newState)
		{
			if (_coOpClient == null) return;

			if (!_coOpClient.IsConnected()) return;

			string map = $"d{w}";

			if (newState.DestNorth != oldState.DestNorth)
			{
				_coOpClient.SendDestLocation(Game, map, x, y, 0, newState.DestNorth?.GetCode());
			}
			if (newState.DestSouth != oldState.DestSouth)
			{
				_coOpClient.SendDestLocation(Game, map, x, y, 1, newState.DestSouth?.GetCode());
			}
			if (newState.DestWest != oldState.DestWest)
			{
				_coOpClient.SendDestLocation(Game, map, x, y, 2, newState.DestWest?.GetCode());
			}
			if (newState.DestEast != oldState.DestEast)
			{
				_coOpClient.SendDestLocation(Game, map, x, y, 3, newState.DestEast?.GetCode());
			}
			if (newState.Item1 != oldState.Item1)
			{
				_coOpClient.SendItemLocation(Game, map, x, y, 0, newState.Item1?.GetCode());
			}
			if (newState.Item2 != oldState.Item2)
			{
				_coOpClient.SendItemLocation(Game, map, x, y, 1, newState.Item2?.GetCode());
			}
		}
	}
}
