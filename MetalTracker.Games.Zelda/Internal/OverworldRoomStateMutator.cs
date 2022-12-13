using MetalTracker.Common.Types;
using MetalTracker.CoOp.Interface;
using MetalTracker.Games.Zelda.Internal.Types;

namespace MetalTracker.Games.Zelda.Internal
{
	internal class OverworldRoomStateMutator
	{
		const string Game = "zelda";
		const string Map = "ow";

		private ICoOpClient _coOpClient;

		public void SetCoOpClient(ICoOpClient coOpClient)
		{
			_coOpClient = coOpClient;
		}

		public void ChangeCave(int x, int y, OverworldRoomState state, OverworldCave newCave)
		{
			if (state.Cave != newCave)
			{
				var oldState = state.Clone();

				state.Cave = newCave;

				state.Item1 = null;
				state.Item2 = null;
				state.Item3 = null;

				if (newCave != null)
				{
					if (newCave.Key == "P")
					{
						state.Item1 = new GameItem("z1", "potion1", "Blue Potion", 'M', ZeldaResourceClient.GetIcon("bluepotion"));
						state.Item3 = new GameItem("z1", "potion2", "Red Potion", 'M', ZeldaResourceClient.GetIcon("redpotion"));
					}
				}

				SendCoOpUpdates(x, y, oldState, state);
			}
		}

		public void ChangeExit(int x, int y, OverworldRoomState state, GameExit newDest)
		{
			if (state.Exit != newDest)
			{
				var oldState = state.Clone();

				state.Exit = newDest;

				state.Item1 = null;
				state.Item2 = null;
				state.Item3 = null;

				SendCoOpUpdates(x, y, oldState, state);
			}
		}

		public void ChangeItem1(int x, int y, OverworldRoomState state, GameItem newItem)
		{
			var oldState = state.Clone();
			state.Item1 = newItem;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeItem2(int x, int y, OverworldRoomState state, GameItem newItem)
		{
			var oldState = state.Clone();
			state.Item2 = newItem;
			SendCoOpUpdates(x, y, oldState, state);
		}

		public void ChangeItem3(int x, int y, OverworldRoomState state, GameItem newItem)
		{
			var oldState = state.Clone();
			state.Item3 = newItem;
			SendCoOpUpdates(x, y, oldState, state);
		}

		private void SendCoOpUpdates(int x, int y, OverworldRoomState oldState, OverworldRoomState newState)
		{
			if (_coOpClient == null) return;

			if (!_coOpClient.IsConnected()) return;

			if (newState.Exit != oldState.Exit)
			{
				_coOpClient.SendLocation("dest", Game, Map, x, y, 0, newState.Exit?.GetCode());
			}
			if (newState.Item1 != oldState.Item1)
			{
				_coOpClient.SendLocation("item", Game, Map, x, y, 0, newState.Item1?.GetCode());
			}
			if (newState.Item2 != oldState.Item2)
			{
				_coOpClient.SendLocation("item", Game, Map, x, y, 1, newState.Item2?.GetCode());
			}
			if (newState.Item3 != oldState.Item3)
			{
				_coOpClient.SendLocation("item", Game, Map, x, y, 2, newState.Item3?.GetCode());
			}
			if (newState.Cave != oldState.Cave)
			{
				_coOpClient.SendLocation("cave", Game, Map, x, y, 0, newState.Cave?.Key);
			}
		}
	}
}
