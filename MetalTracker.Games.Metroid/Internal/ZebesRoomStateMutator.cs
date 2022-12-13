using MetalTracker.Common.Types;
using MetalTracker.CoOp.Interface;
using MetalTracker.Games.Metroid.Internal.Types;

namespace MetalTracker.Games.Metroid.Internal
{
    internal class ZebesRoomStateMutator
	{
		const string Game = "metroid";
		const string Map = "zebes";

		private ICoOpClient _coOpClient;

		public void SetCoOpClient(ICoOpClient coOpClient)
		{
			_coOpClient = coOpClient;
		}

		public void ChangeDestUp(int x, int y, ZebesRoomState state, GameExit newDest)
		{
			var oldState = state.Clone();
			state.ExitUp = newDest;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeDestDown(int x, int y, ZebesRoomState state, GameExit newDest)
		{
			var oldState = state.Clone();
			state.ExitDown = newDest;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeDestLeft(int x, int y, ZebesRoomState state, GameExit newDest)
		{
			var oldState = state.Clone();
			state.ExitLeft = newDest;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeDestRight(int x, int y, ZebesRoomState state, GameExit newDest)
		{
			var oldState = state.Clone();
			state.ExitRight = newDest;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeItem(int x, int y, ZebesRoomState state, GameItem newItem)
		{
			var oldState = state.Clone();
			state.Item = newItem;
			SendCoOpUpdates(x, y, oldState, state);
		}

		private void SendCoOpUpdates(int x, int y, ZebesRoomState oldState, ZebesRoomState newState)
		{
			if (_coOpClient == null) return;

			if (!_coOpClient.IsConnected()) return;

			if (newState.ExitUp != oldState.ExitUp)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 0, newState.ExitUp?.GetCode());
			}
			if (newState.ExitDown != oldState.ExitDown)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 1, newState.ExitDown?.GetCode());
			}
			if (newState.ExitLeft != oldState.ExitLeft)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 2, newState.ExitLeft?.GetCode());
			}
			if (newState.ExitRight != oldState.ExitRight)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 3, newState.ExitRight?.GetCode());
			}
			if (newState.Item != oldState.Item)
			{
				_coOpClient.SendLocation("item", Game, Map, x, y, 0, newState.Item?.GetCode());
			}
		}
	}
}
