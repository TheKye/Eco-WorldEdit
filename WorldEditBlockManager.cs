using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Gameplay.Components;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.Simulation.Types;
using Eco.World;
using Eco.World.Blocks;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit
{
	internal static class WorldEditBlockManager
	{
		public static void SetBlock(Type type, Vector3i position)
		{
			ClearPosition(position);
			RestoreBlock(type, position);
		}
		public static void RestoreBlockOffset(WorldEditBlock block, Vector3i offsetPos, Player player)
		{
			RestoreBlock(block, ApplyOffset(block.Position, offsetPos), player);
		}
		public static void RestoreBlock(WorldEditBlock block, Vector3i position, Player player)
		{
			ClearPosition(position);
			if (block.BlockType.Equals(typeof(EmptyBlock)))
			{
				RestoreEmptyBlock(position);
			}
			else if (block.BlockType.DerivesFrom<PlantBlock>())
			{
				RestorePlantBlock(block.BlockType, position, block.BlockData);
			}
			else if (block.BlockType.DerivesFrom<WorldObjectBlock>())
			{
				RestoreWorldObjectBlock(block.BlockType, position, block.BlockData, player);
			}
			else
			{
				RestoreBlock(block.BlockType, position);
			}
		}

		public static void RestoreEmptyBlock(Vector3i position)
		{
			Eco.World.World.DeleteBlock(position);
		}

		public static void RestoreBlock(Type type, Vector3i position)
		{
			Eco.World.World.SetBlock(type, position);
		}

		public static void RestoreWorldObjectBlock(Type type, Vector3i position, IWorldEditBlockData blockData, Player player)
		{
			WorldEditWorldObjectBlockData worldObjectBlockData = (WorldEditWorldObjectBlockData)blockData;
			ClearWorldObjectPlace(worldObjectBlockData.WorldObjectType, position, worldObjectBlockData.Rotation);

			WorldObject worldObject = null;
			try { worldObject = WorldObjectManager.ForceAdd(worldObjectBlockData.WorldObjectType, player.User, position, worldObjectBlockData.Rotation, true); }
			catch (Exception e)
			{
				Log.WriteLine(new LocString(e.ToString()));
			}
			if (worldObject == null)
			{
				Log.WriteLine(Localizer.Do($"Unable spawn WorldObject {worldObjectBlockData.WorldObjectType} at {position}"));
				return;
			}
			if (worldObject.HasComponent<StorageComponent>() && worldObjectBlockData.Components.ContainsKey(typeof(StorageComponent)))
			{
				StorageComponent storageComponent = worldObject.GetComponent<StorageComponent>();
				List<InventoryStack> inventoryStacks;
				object component = worldObjectBlockData.Components[typeof(StorageComponent)];
				if (component is JArray)
				{
					JArray jArray = (JArray)component;
					inventoryStacks = jArray.ToObject<List<InventoryStack>>();
				}
				else
				{
					inventoryStacks = (List<InventoryStack>)component;
				}

				foreach (InventoryStack stack in inventoryStacks)
				{
					if (stack.ItemType == null) continue;
					storageComponent.Inventory.AddItems(stack.ItemType, stack.Quantity);
				}
			}
			if (worldObject.HasComponent<CustomTextComponent>() && worldObjectBlockData.Components.ContainsKey(typeof(CustomTextComponent)))
			{
				CustomTextComponent textComponent = worldObject.GetComponent<CustomTextComponent>();
				textComponent.TextData.Text = (string)worldObjectBlockData.Components[typeof(CustomTextComponent)];
			}
		}

		public static void RestorePlantBlock(Type type, Vector3i position, IWorldEditBlockData blockData)
		{
			WorldEditPlantBlockData plantBlockData = (WorldEditPlantBlockData)blockData;
			PlantSpecies plantSpecies = EcoSim.AllSpecies.OfType<PlantSpecies>().First(species => species.BlockType == type);
			Plant plant = EcoSim.PlantSim.SpawnPlant(plantSpecies, position);
			plant.YieldPercent = plantBlockData.YieldPercent;
			plant.Dead = plantBlockData.Dead;
			plant.DeadType = plantBlockData.DeadType;
			plant.GrowthPercent = plantBlockData.GrowthPercent;
			plant.DeathTime = plantBlockData.DeathTime;
			plant.Tended = plantBlockData.Tended;
		}

		private static void ClearPosition(Vector3i position)
		{
			Block block = World.World.GetBlock(position);

			if (block is EmptyBlock)
				return;

			switch (block)
			{
				case WorldObjectBlock worldObjectBlock:
					worldObjectBlock.WorldObjectHandle.Object.Destroy();
					break;
				default:
					Eco.World.World.DeleteBlock(position);
					break;
			}
		}

		private static void ClearWorldObjectPlace(Type worldObjectType, Vector3i position, Quaternion rotation)
		{
			List<BlockOccupancy> blockOccupancies = WorldObject.GetOccupancy(worldObjectType);
			if (!blockOccupancies.Any(x => x.BlockType != null)) return;
			foreach (var blockOccupancy in blockOccupancies)
			{
				if (blockOccupancy.BlockType != null)
				{
					Vector3i worldPos = position + rotation.RotateVector(blockOccupancy.Offset).XYZi;
					ClearPosition(worldPos);
				}
			}
		}

		public static Vector3i ApplyOffset(Vector3i position, Vector3i offset)
		{
			return position + offset;
		}
	}
}
