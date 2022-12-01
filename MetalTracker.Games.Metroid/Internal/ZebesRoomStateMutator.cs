using MetalTracker.Common.Types;
using MetalTracker.CoOp;
using MetalTracker.Games.Metroid.Types;

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

		public void ChangeDestination(int x, int y, ZebesRoomState state, GameDest newDest)
		{
			if (state.DestElev != newDest)
			{
				var oldState = state.Clone();
				state.DestElev = newDest;
				SendCoOpUpdates(x, y, oldState, state);
			}
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

			if (newState.DestElev != oldState.DestElev)
			{
				_coOpClient.SendDestLocation(Game, Map, x, y, 0, newState.DestElev?.GetCode());
			}
			if (newState.Item != oldState.Item)
			{
				_coOpClient.SendItemLocation(Game, Map, x, y, 0, newState.Item?.GetCode());
			}
		}
	}
}
