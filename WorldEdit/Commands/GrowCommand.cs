using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;
using Eco.World;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class GrowCommand : WorldEditCommand
	{
		public GrowCommand(User user) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
		}

		protected override void Execute()
		{
			WorldRange range = this.UserSession.Selection;
			range.Fix(Shared.Voxel.World.VoxelSize);

			range.ForEachInc(pos =>
			{
				Block block = Eco.World.World.GetBlock(pos);

				if (block.GetType() == typeof(PlantBlock))
				{
					var pb = PlantBlock.GetPlant(pos);
					pb.GrowthPercent = 1;
					pb.Tended = true;
					pb.Tick();
				}
			});
		}
	}
}
