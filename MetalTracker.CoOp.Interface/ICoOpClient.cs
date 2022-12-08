namespace MetalTracker.CoOp.Interface
{
	public interface ICoOpClient
	{
		event EventHandler<FoundEventArgs> Found;

		bool IsConnected();

		Task SendLocation(string type, string game, string map, int x, int y, int slot, string code);
	}
}
