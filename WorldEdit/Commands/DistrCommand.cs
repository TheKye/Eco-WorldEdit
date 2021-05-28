using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Gameplay.Systems.Tooltip;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.Simulation.Types;
using Eco.World;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class DistrCommand : WorldEditCommand
	{
		private string outputType;

		public DistrCommand(User user, string type) : base(user)
		{
			if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) { throw new WorldEditCommandException("Please set both points first!"); }
			this.outputType = type;
		}

		protected override void Execute()
		{
			SortedVectorPair vectors = (SortedVectorPair)WorldEditUtils.GetSortedVectors(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value);
			Dictionary<object, long> blocks = new Dictionary<object, long>();

			for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
			{
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
					{
						//                 Console.WriteLine($"{x} {y} {z}");
						Vector3i pos = new Vector3i(x, y, z);
						Block block = Eco.World.World.GetBlock(pos);
						Type blockType = null;
						switch (block)
						{
							case PlantBlock _:
							case TreeBlock _:
								Plant plant = EcoSim.PlantSim.GetPlant(pos);
								if (plant != null && plant.Position.Equals(pos)) blockType = plant.Species.GetType();
								break;
							case WorldObjectBlock worldObjectBlock:
								WorldObject worldObject = worldObjectBlock.WorldObjectHandle.Object;
								if (worldObject.Position3i.Equals(pos)) blockType = worldObject.GetType();
								break;
							default:
								blockType = block.GetType();
								break;
						}
						if (blockType != null)
						{
							if (this.outputType.Equals("brief"))
							{
								string name = this.GetBlockFancyName(blockType);
								blocks.TryGetValue(name, out long count);
								blocks[name] = count + 1;
							}
							else
							{
								blocks.TryGetValue(blockType, out long count);
								blocks[blockType] = count + 1;
							}
						}
					}
				}
			}

			decimal totalBlocks = blocks.Values.Sum(); // (vectors.Higher.X - vectors.Lower.X) * (vectors.Higher.Y - vectors.Lower.Y) * (vectors.Higher.Z - vectors.Lower.Z);

			StringBuilder sb = new StringBuilder();
			sb.AppendLineLoc($"Total blocks: {totalBlocks}");
			foreach (KeyValuePair<object, long> entry in blocks)
			{
				decimal percent = Math.Round((entry.Value / totalBlocks) * 100, 2);

				sb.Append(this.outputType.Equals("detail") ? this.GetBlockFancyName((Type)entry.Key) : (string)entry.Key);
				sb.Append(Text.Pos(300, Text.Info(Text.Int(entry.Value))));
				sb.Append($"({percent}%)".PadLeft(8));
				if (this.outputType.Equals("detail")) sb.Append(Text.Pos(500, $"[{Localizer.DoStr(((Type)entry.Key).Name)}]"));
				sb.AppendLine();
				//string percent = Math.Round((entry.Value / totalBlocks) * 100, 2).ToString() + "%";
				//string nameOfBlock = entry.Key[(entry.Key.LastIndexOf(".") + 1)..];
				//msg += $"<ecoicon name='{nameOfBlock}'></ecoicon>{entry.Value,-6} {percent,-6} {Localizer.DoStr(nameOfBlock)} \n";
			}
			this.UserSession.Player.OpenInfoPanel(Localizer.Do($"WorldEdit Blocks Report"), sb.ToString(), "WorldEditDistr");
		}

		private LocString GetBlockFancyName(Type blockType)
		{
			if (blockType.DerivesFrom<PlantSpecies>())
			{
				Species species = EcoSim.AllSpecies.OfType<PlantSpecies>().First(species => species.GetType() == blockType);
				if (species != null) return species.UILink();
			}
			Item item = blockType.TryGetAttribute<Ramp>(false, out var rampAttr) ? Item.Get(rampAttr.RampType) : BlockItem.GetBlockItem(blockType) ?? BlockItem.CreatingItem(blockType);
			if (item == null && blockType.DerivesFrom<WorldObject>())
			{
				item = WorldObjectItem.GetCreatingItemTemplateFromType(blockType);
			}
			if (item != null) return item.UILink();
			if (blockType.BaseType != null && blockType.BaseType != typeof(Block))
			{
				return this.GetBlockFancyName(blockType.BaseType);
			}
			return Localizer.DoStr(blockType.Name); //Not fancy at all :(
		}
	}
}
