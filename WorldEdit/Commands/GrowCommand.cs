using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class GrowCommand : WorldEditCommand
	{
		public GrowCommand(User user) : base(user)
		{
			if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) { throw new WorldEditCommandException("Please set both points first!"); }
		}

		protected override void Execute()
		{
			SortedVectorPair vectors = (SortedVectorPair)WorldEditUtils.GetSortedVectors(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value);
			for (int x = vectors.Lower.X; x < vectors.Higher.X; x++)
			{
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					for (int z = vectors.Lower.Z; z < vectors.Higher.Z; z++)
					{
						var pos = new Vector3i(x, y, z);
						var block = Eco.World.World.GetBlock(pos);

						if (block.GetType() == typeof(PlantBlock))
						{
							var pb = PlantBlock.GetPlant(pos);
							pb.GrowthPercent = 1;
							pb.Tended = true;
							pb.Tick();
						}
					}
				}
			}
		}
	}
}
