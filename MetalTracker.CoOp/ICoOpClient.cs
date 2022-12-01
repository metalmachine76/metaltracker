using MetalTracker.CoOp.EventArgs;

namespace MetalTracker.CoOp
{
	public interface ICoOpClient
	{
		event EventHandler<FoundEventArgs> FoundItem;

		event EventHandler<FoundEventArgs> FoundDest;

		bool IsConnected();

		Task SendItemLocation(string game, string map, int x, int y, int slot, string code);

		Task SendDestLocation(string game, string map, int x, int y, int slot, string code);
	}
}
