namespace MetalTracker.Games.Zelda.Internal.Types
{
	internal class DungeonWall
	{
		public string Code { get; private set; }

		public int Ordinal { get; private set; }

		public string Name { get; private set; }

		public DungeonWall(string code, int ordinal, string name)
		{
			this.Code = code;
			this.Ordinal = ordinal;
			this.Name = name;
		}
	}
}
