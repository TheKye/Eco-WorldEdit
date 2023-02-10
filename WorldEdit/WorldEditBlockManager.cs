using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Core.Utils;
using Eco.Gameplay.Blocks;
using Eco.Gameplay.Components;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.Components;
using Eco.Shared.IoC;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.Simulation.Types;
using Eco.World.Blocks;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit
{
	using World = Eco.World.World;

	internal class WorldEditBlockManager
	{
		private readonly UserSession _userSession;

		public WorldEditBlockManager(UserSession userSession)
		{
			_userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
		}

		public void SetBlock(Type type, Vector3i position)
		{
			if (IsImpenetrable(position)) return;
			ClearPosition(position);
			SetBlockInternal(type, position);
		}
		public void RestoreBlockOffset(WorldEditBlock block, Vector3i offsetPos)
		{
			RestoreBlock(block, ApplyOffset(block.Position, offsetPos));
		}
		public void RestoreBlock(WorldEditBlock block, Vector3i position)
		{
			if (IsImpenetrable(position)) return;
			ClearPosition(position);
			if (block.IsEmptyBlock())
			{
				SetBlockInternal(typeof(EmptyBlock), position);
			}
			else if (block.IsPlantBlock())
			{
				RestorePlantBlock(block.BlockType, position, block.BlockData);
			}
			else if (block.IsWorldObjectBlock())
			{
				RestoreWorldObjectBlock(block.BlockType, position, block.BlockData);
			}
			else
			{
				SetBlockInternal(block.BlockType, position);
			}
		}

		public static void RestoreBlock(Type type, Vector3i position)
		{
			if (IsImpenetrable(position)) return;
			SetBlockInternal(type, position);
		}

		public static void RestoreEmptyBlock(Vector3i position)
		{
			if (IsImpenetrable(position)) return;
			SetBlockInternal(typeof(EmptyBlock), position);
		}

		public void RestoreWorldObjectBlock(Type type, Vector3i position, IWorldEditBlockData blockData)
		{
			if (blockData == null) { return; }
			WorldEditWorldObjectBlockData worldObjectBlockData = (WorldEditWorldObjectBlockData)blockData;
			ClearWorldObjectPlace(worldObjectBlockData.WorldObjectType, position, worldObjectBlockData.Rotation);

			WorldObject worldObject = null;
			try { worldObject = WorldObjectManager.ForceAdd(worldObjectBlockData.WorldObjectType, _userSession.User, position, worldObjectBlockData.Rotation, true); }
			catch (Exception e)
			{
				Log.WriteException(e);
			}
			if (worldObject == null)
			{
				Log.WriteErrorLineLoc($"Unable spawn WorldObject {worldObjectBlockData.WorldObjectType} at {position}");
				return;
			}
			if(!string.IsNullOrEmpty(worldObjectBlockData.Name)) { worldObject.SetName(worldObjectBlockData.Name); }
			if (worldObject.HasComponent<StorageComponent>() && worldObjectBlockData.Components.ContainsKey(typeof(StorageComponent)))
			{
				StorageComponent storageComponent = worldObject.GetComponent<StorageComponent>();
				List<InventoryStack> inventoryStacks;
				object component = worldObjectBlockData.Components[typeof(StorageComponent)];
				if (component is JArray jArray)
				{
					inventoryStacks = jArray.ToObject<List<InventoryStack>>();
				}
				else
				{
					inventoryStacks = (List<InventoryStack>)component;
				}

				foreach (InventoryStack stack in inventoryStacks)
				{
					if (stack.ItemType == null) continue;
					Result result = storageComponent.Inventory.TryAddItems(stack.ItemType, stack.Quantity);
					if (result.Failed)
					{
						_userSession.Player.ErrorLocStr(result.Message.Trim());
						Log.WriteWarningLineLoc($"Unable restore inventory for WorldObject {worldObjectBlockData.WorldObjectType} at {position}: {result.Message.Trim()}");
						try { storageComponent.Inventory.AddItems(stack.GetItemStack()); } catch (InvalidOperationException) { /*Already show error to user*/ }
					}
				}
			}
			if (worldObject.HasComponent<CustomTextComponent>() && worldObjectBlockData.Components.ContainsKey(typeof(CustomTextComponent)))
			{
				CustomTextComponent textComponent = worldObject.GetComponent<CustomTextComponent>();
				textComponent.TextData.Text = (string)worldObjectBlockData.Components[typeof(CustomTextComponent)];
			}
			if (worldObject.HasComponent<MintComponent>() && worldObjectBlockData.Components.ContainsKey(typeof(MintComponent)))
			{
				MintComponent mintComponent = worldObject.GetComponent<MintComponent>();
				object obj = worldObjectBlockData.Components[typeof(MintComponent)];
				MintDataCurrency mintCurrency;
				if (obj is JObject jobj)
				{
					mintCurrency = jobj.ToObject<MintDataCurrency>();
				}
				else
				{
					mintCurrency = (MintDataCurrency)obj;
				}
				mintComponent.InitializeCurrency(mintCurrency.GetCurrency());
			}
			//Handle door opening
			if(worldObject is TechTree.DoorObject && worldObjectBlockData.Components.ContainsKey(typeof(TechTree.DoorObject)))
			{
				System.Reflection.MethodInfo method = typeof(TechTree.DoorObject).GetMethod("WE_SetOpensOut", new Type[] { typeof(bool) });
				if (method != null)
				{
					object obj = worldObjectBlockData.Components[typeof(TechTree.DoorObject)];
					DoorComponent doorComponent;
					if (obj is JObject jobj)
					{
						doorComponent = jobj.ToObject<DoorComponent>();
					}
					else
					{
						doorComponent = (DoorComponent)obj;
					}
					method.Invoke(worldObject, new object[] { doorComponent.OpensOut });
				}
			}
			if (worldObject.HasComponent<StoreComponent>() && worldObjectBlockData.Components.ContainsKey(typeof(StoreComponent)))
			{
				StoreComponent storeComponent = worldObject.GetComponent<StoreComponent>();
				object obj = worldObjectBlockData.Components[typeof(StoreComponent)];
				StoreComponentData componentData = obj is JObject jobj ? jobj.ToObject<StoreComponentData>() : (StoreComponentData)obj;
				componentData.Buy.ForEach(c => storeComponent.StoreData.BuyCategories.Add(c.GetStoreCategory(storeComponent)));
				componentData.Sell.ForEach(c => storeComponent.StoreData.SellCategories.Add(c.GetStoreCategory(storeComponent)));
			}
		}

		public static void RestorePlantBlock(Type type, Vector3i position, IWorldEditBlockData blockData)
		{
			if (blockData == null) { return; }
			WorldEditPlantBlockData plantBlockData = (WorldEditPlantBlockData)blockData;
			PlantSpecies plantSpecies = null;
			try { plantSpecies = EcoSim.AllSpecies.OfType<PlantSpecies>().First(species => species.GetType() == plantBlockData.PlantType); }
			catch (InvalidOperationException)
			{
				//TODO: Temporary support for the old serialized format! Should be done with migration!
				plantSpecies = EcoSim.AllSpecies.OfType<PlantSpecies>().First(species => species.Name == plantBlockData.PlantType.Name);
			}
			if (plantSpecies == null) return;
			Plant plant = EcoSim.PlantSim.SpawnPlant(plantSpecies, position, true);
			plant.YieldPercent = plantBlockData.YieldPercent;
			plant.Dead = plantBlockData.Dead;
			plant.DeadType = plantBlockData.DeadType;
			plant.GrowthPercent = plantBlockData.GrowthPercent;
			plant.DeathTime = plantBlockData.DeathTime;
			plant.Tended = plantBlockData.Tended;
		}

		/// <summary>Directly delete or set block WITHOUT safety checks.</summary>
		protected static void SetBlockInternal(Type type, Vector3i position)
		{
			if (type == null) { World.DeleteBlock(position); } else { World.SetBlock(type, position); }
		}

		/// <summary>Clears given position from water, plants, trees and objects. Optionally delete block</summary>
		private void ClearPosition(Vector3i position, bool delete = false)
		{
			Block block = World.GetBlock(position);

			if (block is EmptyBlock) return;

			switch (block)
			{
				case IWaterBlock waterBlock:
					//Log.WriteWarningLineLocStr($"Block type: {block.GetType()} {position}");
					//Vector3i above = position + Vector3i.Up;
					//Block aboveBlock = World.GetBlock(above);
					//if (aboveBlock is IWaterBlock) { /*Log.Debug($"Upper block type: {block.GetType()} {above}");*/ ClearPosition(above); }
					//World.SetBlock<EmptyBlock>(position);
					World.DeleteBlock(position, false);
					break;
				case WorldObjectBlock worldObjectBlock:
					worldObjectBlock.WorldObjectHandle.Object.Destroy();
					break;
				case ImpenetrableStoneBlock _:
					return;
				case PlantBlock plantBlock:
				case TreeBlock treeBlock:
					Plant plant = EcoSim.PlantSim.GetPlant(position);
					if (plant != null) { EcoSim.PlantSim.DestroyPlant(plant, DeathType.DivineIntervention, true); }
					break;
				default:
					if (BlockContainerManager.Obj.IsBlockContained(position))
					{
						WorldObject worldObject = ServiceHolder<IWorldObjectManager>.Obj.All.Where(x => x.Position3i.Equals(position)).FirstOrDefault();
						if (worldObject != null) worldObject.Destroy();
					}
					else if (delete) { World.DeleteBlock(position); }
					break;
			}
		}

		private void ClearWorldObjectPlace(Type worldObjectType, Vector3i position, Quaternion rotation)
		{
			List<BlockOccupancy> blockOccupancies = WorldObject.GetOccupancy(worldObjectType);
			if (!blockOccupancies.Any(x => x.BlockType != null)) return;
			foreach (var blockOccupancy in blockOccupancies)
			{
				if (blockOccupancy.BlockType != null)
				{
					Vector3i worldPos = position + rotation.RotateVector(blockOccupancy.Offset).XYZi();
					if (!_userSession.ExecutingCommand.PerformingUndo) _userSession.ExecutingCommand.AddBlockChangedEntry(worldPos); //Do not record changes when doing undo
					ClearPosition(worldPos, true);
				}
			}
		}

		public static bool IsImpenetrable(Vector3i position) => World.GetBlock(position).Is<Impenetrable>() || position.y < 0;
		public static Vector3i ApplyOffset(Vector3i position, Vector3i offset) => position + offset;
	}
}
