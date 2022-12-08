namespace MetalTracker.Games.Zelda.Internal.Types
{
	internal class DungeonWall
	{
		public string Code { get; private set; }

		public string Name { get; private set; }

		public DungeonWall(string code, string name)
		{
			this.Code = code;
			this.Name = name;
		}
	}
}
