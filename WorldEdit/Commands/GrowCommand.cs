using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;
using Eco.Shared.Utils;
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
			selection = this.UserSession.Selection;

			void DoAction(Vector3i pos)
			{
				Block block = Eco.World.World.GetBlock(pos);
				Log.WriteErrorLineLocStr($"{block.GetType().Name}");
				if (block.GetType() == typeof(PlantBlock) || block.GetType() == typeof(TreeBlock) || block.GetType() == typeof(InteractablePlantBlock))
				{
					var pb = PlantBlock.GetPlant(pos);
					var sp = pb.Species;
					var gr = sp.MaxGrowthRate;
					var mad = sp.MaturityAgeDays;
                    sp.MaxGrowthRate = 100000f;
                    sp.MaturityAgeDays = 0.000001f;
					pb.GrowthPercent = 1f;
					pb.Tended = true;
					pb.Tick();
					Log.WriteErrorLineLocStr($"Block Found: {pb.Species.Name}");
                    sp.MaxGrowthRate = gr;
                    sp.MaturityAgeDays = mad;
				}
			}
			selection.ForEachInc(DoAction);
		}
	}
}
