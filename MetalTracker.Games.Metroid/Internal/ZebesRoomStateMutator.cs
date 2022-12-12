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

		public void ChangeDestUp(int x, int y, ZebesRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.DestUp = newDest;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeDestDown(int x, int y, ZebesRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.DestDown = newDest;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeDestLeft(int x, int y, ZebesRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.DestLeft = newDest;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeDestRight(int x, int y, ZebesRoomState state, GameDest newDest)
		{
			var oldState = state.Clone();
			state.DestRight = newDest;
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

			if (newState.DestUp != oldState.DestUp)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 0, newState.DestUp?.GetCode());
			}
			if (newState.DestDown != oldState.DestDown)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 1, newState.DestDown?.GetCode());
			}
			if (newState.DestLeft != oldState.DestLeft)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 2, newState.DestLeft?.GetCode());
			}
			if (newState.DestRight != oldState.DestRight)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 3, newState.DestRight?.GetCode());
			}
			if (newState.Item != oldState.Item)
			{
				_coOpClient.SendLocation("item", Game, Map, x, y, 0, newState.Item?.GetCode());
			}
		}
	}
}
