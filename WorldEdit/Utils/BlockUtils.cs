using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Gameplay.Blocks;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Gameplay.Systems.Tooltip;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Types;
using Eco.World;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class BlockUtils
	{
		private static readonly Dictionary<Type, Type[]> BlockRotatedVariants = new Dictionary<Type, Type[]>(); //Cache possible variants for speed up search

		public static Type GetBlockType(string pBlockName)
		{
			pBlockName = pBlockName.ToLower();

			if (pBlockName == "air")
				return typeof(EmptyBlock);

			Type blockType = BlockManager.BlockTypes.FirstOrDefault(t => t.Name.ToLower() == pBlockName + "floorblock");

			if (blockType != null)
				return blockType;

			blockType = BlockManager.BlockTypes.FirstOrDefault(t => t.Name.ToLower() == pBlockName + "block");

			if (blockType != null)
				return blockType;

			return BlockManager.BlockTypes.FirstOrDefault(t => t.Name.ToLower() == pBlockName + "block");
		}

		public static bool HasRotatedVariants(Type blockType, out Type[] variants)
		{
			Type[] possibleVariants = new Type[] { blockType };
			//Cached search
			if (BlockRotatedVariants.TryGetValue(blockType, out variants))
			{
				return variants.Length > 1;
			}
			//DeepSearch
			if (BlockFormManager.Data.BlockForms.Any(form =>
				{
					if (form.BlockTypes.Contains(blockType))
					{
						foreach (Type type in form.BlockTypes) //Probably can just check the length, if there no variants it have only one type = self type
						{
							if (Block.Get<RotatedVariants>(type)?.Variants != null)
							{
								possibleVariants = form.BlockTypes;
								return true;
							}
						}
					}
					return false;
				}
			))
			{
				//Cache results for later
				foreach (Type type in possibleVariants)
				{
					BlockRotatedVariants.Add(type, possibleVariants);
				}
				variants = possibleVariants;
				return true;
			}
			BlockRotatedVariants.Add(blockType, possibleVariants); //Cache results for later
			variants = possibleVariants;
			return false;
		}

		public static LocString GetBlockFancyName(Type blockType)
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
				return GetBlockFancyName(blockType.BaseType);
			}
			return Localizer.DoStr(blockType.Name); //Not fancy at all :(
		}
	}
}
