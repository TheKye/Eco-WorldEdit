using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class GrowCommand : WorldEditCommand
	{
		public GrowCommand(User user) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
		}

		protected override void Execute(WorldRange selection)
		{
			selection = selection.FixXZ(Shared.Voxel.World.VoxelSize);

			void DoAction(Vector3i pos)
			{
				Block block = Eco.World.World.GetBlock(pos);

				if (block.GetType() == typeof(PlantBlock) || block.GetType() == typeof(TreeBlock))
				{
					var pb = PlantBlock.GetPlant(pos);
					pb.GrowthPercent = 1;
					pb.Tended = true;
					pb.Tick();
				}
			}
			selection.ForEachInc(DoAction);
		}
	}
}
