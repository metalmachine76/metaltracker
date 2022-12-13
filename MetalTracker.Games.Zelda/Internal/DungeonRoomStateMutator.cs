using MetalTracker.Common.Types;
using MetalTracker.CoOp.Interface;
using MetalTracker.Games.Zelda.Internal.Types;

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
			state.ExitNorth = newDest;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeDestSouth(int w, int x, int y, DungeonRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.ExitSouth = newDest;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeDestWest(int w, int x, int y, DungeonRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.ExitWest = newDest;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeDestEast(int w, int x, int y, DungeonRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.ExitEast = newDest;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeWallNorth(int w, int x, int y, DungeonRoomState state, DungeonWall newWall)
		{
			var oldState = state.Clone();
			state.WallNorth = newWall;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeWallSouth(int w, int x, int y, DungeonRoomState state, DungeonWall newWall)
		{
			var oldState = state.Clone();
			state.WallSouth = newWall;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeWallWest(int w, int x, int y, DungeonRoomState state, DungeonWall newWall)
		{
			var oldState = state.Clone();
			state.WallWest = newWall;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		public void ChangeWallEast(int w, int x, int y, DungeonRoomState state, DungeonWall newWall)
		{
			var oldState = state.Clone();
			state.WallEast = newWall;
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

		public void ChangeTransport(int w, int x, int y, DungeonRoomState state, string transport)
		{
			var oldState = state.Clone();
			state.Transport = transport;
			SendCoOpUpdates(w, x, y, oldState, state);
		}

		private void SendCoOpUpdates(int w, int x, int y, DungeonRoomState oldState, DungeonRoomState newState)
		{
			if (_coOpClient == null) return;

			if (!_coOpClient.IsConnected()) return;

			string map = $"d{w}";

			if (newState.ExitNorth != oldState.ExitNorth)
			{
				_coOpClient.SendLocation("dest", Game, map, x, y, 0, newState.ExitNorth?.GetCode());
			}
			if (newState.ExitSouth != oldState.ExitSouth)
			{
				_coOpClient.SendLocation("dest", Game, map, x, y, 1, newState.ExitSouth?.GetCode());
			}
			if (newState.ExitWest != oldState.ExitWest)
			{
				_coOpClient.SendLocation("dest", Game, map, x, y, 2, newState.ExitWest?.GetCode());
			}
			if (newState.ExitEast != oldState.ExitEast)
			{
				_coOpClient.SendLocation("dest", Game, map, x, y, 3, newState.ExitEast?.GetCode());
			}

			if (newState.WallNorth != oldState.WallNorth)
			{
				_coOpClient.SendLocation("wall", Game, map, x, y, 0, newState.WallNorth?.Code);
			}
			if (newState.WallSouth != oldState.WallSouth)
			{
				_coOpClient.SendLocation("wall", Game, map, x, y, 1, newState.WallSouth?.Code);
			}
			if (newState.WallWest != oldState.WallWest)
			{
				_coOpClient.SendLocation("wall", Game, map, x, y, 2, newState.WallWest?.Code);
			}
			if (newState.WallEast != oldState.WallEast)
			{
				_coOpClient.SendLocation("wall", Game, map, x, y, 3, newState.WallEast?.Code);
			}

			if (newState.Item1 != oldState.Item1)
			{
				_coOpClient.SendLocation("item", Game, map, x, y, 0, newState.Item1?.GetCode());
			}
			if (newState.Item2 != oldState.Item2)
			{
				_coOpClient.SendLocation("item", Game, map, x, y, 1, newState.Item2?.GetCode());
			}

			if (newState.Transport != oldState.Transport)
			{
				_coOpClient.SendLocation("stair", Game, map, x, y, 0, newState.Transport);
			}
		}
	}
}
