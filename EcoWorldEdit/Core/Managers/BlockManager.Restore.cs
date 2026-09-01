using System.Numerics;
using Eco.Core.Utils;
using Eco.Gameplay.Components;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Components.Store;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Occupancy;
using Eco.Gameplay.Settlements.ClaimStakes.Internal;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Model.Components;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.Simulation.Types;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	using EcoQuaternion = Eco.Shared.Math.Quaternion;

	internal sealed partial class BlockManager
	{
		public void Restore(IReadOnlyList<WorldEditBlock> snapshot, Vector3 origin, CancellationToken ct)
		{
			ArgumentNullException.ThrowIfNull(snapshot);
			ct.ThrowIfCancellationRequested();

			foreach (WorldEditBlock block in snapshot)
			{
				ct.ThrowIfCancellationRequested();
				ValidateRestore(block, origin);
			}

			BlockRestoreContext context = new(snapshot, ct);

			// Restore blocks
			foreach (WorldEditBlock block in snapshot)
			{
				ct.ThrowIfCancellationRequested();
				if (block.IsBlockInternally()) this.ScheduleRestoreBlock(block, origin, ct);
			}
			this.CommitBatch(ct);

			// Restore plants
			foreach (WorldEditBlock block in snapshot)
			{
				ct.ThrowIfCancellationRequested();
				if (block.IsPlantInternally()) this.RestorePlant(block, (PlantBlockData)block.BlockData!, origin, ct);
			}

			// Restore WorldObjects
			foreach (WorldEditBlock block in snapshot)
			{
				ct.ThrowIfCancellationRequested();
				if (block.IsWorldObjectInternally()) this.RestoreWorldObject(block, (WorldObjectBlockData)block.BlockData!, origin, context, ct);
			}
		}

		public void Restore(IReadOnlyList<WorldEditBlock> blocks, IReadOnlyList<WorldEditBlock> plants, IReadOnlyList<WorldEditBlock> worldObjects, Vector3 origin, bool skipEmptyBlocks, CancellationToken ct)
		{
			ArgumentNullException.ThrowIfNull(blocks);
			ArgumentNullException.ThrowIfNull(plants);
			ArgumentNullException.ThrowIfNull(worldObjects);
			ct.ThrowIfCancellationRequested();

			foreach (WorldEditBlock block in blocks) { ct.ThrowIfCancellationRequested(); ValidateRestore(block, origin, InternalRestoreKind.Block); }
			foreach (WorldEditBlock plant in plants) { ct.ThrowIfCancellationRequested(); ValidateRestore(plant, origin, InternalRestoreKind.Plant); }
			foreach (WorldEditBlock worldObject in worldObjects) { ct.ThrowIfCancellationRequested(); ValidateRestore(worldObject, origin, InternalRestoreKind.WorldObject); }

			BlockRestoreContext context = new(worldObjects, ct) { SkipEmpty = skipEmptyBlocks };

			this.Restore(blocks, origin, context, ct);
			this.Restore(plants, origin, context, ct);
			this.Restore(worldObjects, origin, context, ct);
		}

		private void Restore(IReadOnlyList<WorldEditBlock> blocks, Vector3 origin, BlockRestoreContext context, CancellationToken ct)
		{
			foreach (WorldEditBlock block in blocks)
			{
				ct.ThrowIfCancellationRequested();
				if (context.SkipEmpty && block.IsEmptyBlock()) continue;
				if (block.IsWorldObjectInternally())
				{
					this.RestoreWorldObject(block, (WorldObjectBlockData)block.BlockData!, origin, context, ct);
				}
				else if (block.IsPlantInternally())
				{
					this.RestorePlant(block, (PlantBlockData)block.BlockData!, origin, ct);
				}
				else if (block.IsBlockInternally())
				{
					this.ScheduleRestoreBlock(block, origin, ct);
				}
				else
				{
					throw UnsupportedBlockData(block);
				}
			}
			this.CommitBatch(ct);
		}

		private void ScheduleRestoreBlock(WorldEditBlock block, Vector3 origin, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();
			Vector3i position = ToBlockPosition(origin + block.LocalPosition);
			this.TryScheduleBlock(block.BlockType, position, block.Color, ct);
		}

		private void RestorePlant(WorldEditBlock block, PlantBlockData plantData, Vector3 origin, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();
			Vector3i position = ToBlockPosition(origin + block.LocalPosition);
			if (!WorldHeight.IsValid(position.Y))
			{
				Log.WriteWarningLineLoc($"Skipped restoring plant {plantData.PlantType} at {position}: height is outside the world.");
				return;
			}
			if (IsImpenetrable(position))
			{
				Log.WriteWarningLineLoc($"Skipped restoring plant {plantData.PlantType} at {position}: position is impenetrable.");
				return;
			}

			PlantSpecies? species = FindPlantSpecies(plantData.PlantType, ct);
			if (species is null)
			{
				Log.WriteWarningLineLoc($"Skipped restoring plant {plantData.PlantType} at {position}: matching PlantSpecies was not found.");
				return;
			}

			this.CaptureChange(position, ct);
			this.ClearPosition(position, true, cancellationToken: ct);
			ct.ThrowIfCancellationRequested();
			Plant plant = EcoSim.PlantSim.SpawnPlant(species, position.WorldPosition3iOrInvalid(), true);
			this._changes.RegisterChangedBlock(position);
			plant.YieldPercent = plantData.YieldPercent;
			plant.Dead = plantData.Dead;
			plant.DeadType = (DeathType)plantData.DeadType;
			plant.DeathTime = plantData.DeathTime;
			plant.GrowthPercent = plantData.GrowthPercent;
			plant.Tended = plantData.Tended;
		}

		private WorldObject? RestoreWorldObject(WorldEditBlock block, WorldObjectBlockData objectData, Vector3 origin, BlockRestoreContext context, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();
			if (IsIgnoredWorldObjectType(objectData.WorldObjectType)) return null;
			if (objectData.ObjectId is Guid existingId && context.TryGetRestoredObject(existingId, out WorldObject existing)) return existing;

			Guid? objectId = objectData.ObjectId;
			if (objectId is Guid restoringId) context.BeginRestore(restoringId);
			try
			{
				WorldObject? parent = null;
				if (objectData.ParentId is Guid parentId)
				{
					if (!context.TryGetRestoredObject(parentId, out parent))
					{
						if (!context.TryGetBlock(parentId, out WorldEditBlock parentBlock) || !parentBlock.IsWorldObjectInternally()) throw new WorldEditCommandException($"WorldObject references missing parent {parentId}.");
						WorldObjectBlockData parentData = (WorldObjectBlockData)parentBlock.BlockData!;
						parent = this.RestoreWorldObject(parentBlock, parentData, origin, context, ct);
					}
				}

				if (objectData.WorldObjectType.DerivesFrom<ClaimStakeObjectBase>()) throw new WorldEditCommandException($"Claim stake {objectData.WorldObjectType} is ignored for safety reasons.");

				Item creatingItem = WorldObjectItem.GetCreatingItemTemplateFromType(objectData.WorldObjectType);
				Result canCreate = StrangeItemProtection.CanCreateItem(this._userSession.User, creatingItem);
				if (canCreate.Failed) throw new WorldEditCommandException(canCreate.Message.Trim());

				Vector3 position = origin + block.LocalPosition;
				this.ClearWorldObjectPlace(objectData.WorldObjectType, position, objectData.Rotation, parent, ct);
				ct.ThrowIfCancellationRequested();
				WorldObject? worldObject = WorldObjectManager.ForceAdd(
					objectData.WorldObjectType,
					this._userSession.User,
					position,
					objectData.Rotation,
					true,
					attachedToWorldObject: parent);

				if (worldObject is null) throw new WorldEditCommandException($"Unable to create WorldObject {objectData.WorldObjectType} at {position}.");
				this._changes.RegisterChangedBlock(position);
				StrangeItemProtection.IncrementUsedItem(this._userSession.User, creatingItem);
				if (!string.IsNullOrEmpty(objectData.Name)) worldObject.SetName(objectData.Name);
				this.RestoreComponents(worldObject, objectData.Components, ct);

				if (objectId is Guid completedId) context.CompleteRestore(completedId, worldObject);
				return worldObject;
			}
			catch
			{
				if (objectId is Guid failedId) context.CancelRestore(failedId);
				throw;
			}
		}

		private void RestoreComponents(WorldObject worldObject, IReadOnlyList<IWorldObjectComponentData> components, CancellationToken ct)
		{
			foreach (IWorldObjectComponentData componentData in components)
			{
				ct.ThrowIfCancellationRequested();
				switch (componentData)
				{
					case StorageComponentData storageData when worldObject.GetComponent<StorageComponent>() is { Inventory: { } inventory }:
						foreach (InventoryStackData stack in storageData.InventoryStacks)
						{
							ct.ThrowIfCancellationRequested();
							Item item = Item.Get(stack.ItemType);
							Result canCreate = StrangeItemProtection.CanCreateItem(this._userSession.User, item, stack.Quantity);
							Result result = canCreate.Success ? inventory.TryAddItemsNonUnique(stack.ItemType, stack.Quantity, this._userSession.User) : canCreate;
							if (result.Failed)
							{
								this._userSession.Player.Error(result.Message);
								continue;
							}
							StrangeItemProtection.IncrementUsedItem(this._userSession.User, item, stack.Quantity);
						}
						break;

					case CustomTextComponentData textData when worldObject.GetComponent<CustomTextComponent>() is { } text:
						text.TextData.Text = textData.Text;
						break;

					case MintComponentData mintData when worldObject.GetComponent<MintComponent>() is { } mint:
						if (mintData.GetCurrency() is { } currency) mint.InitializeCurrency(currency);
						break;

					case StoreComponentData storeData when worldObject.GetComponent<StoreComponent>() is { } store:
						store.StoreData.BuyCategories.Clear();
						store.StoreData.SellCategories.Clear();
						foreach (StoreCategoryData category in storeData.Buy) { ct.ThrowIfCancellationRequested(); store.StoreData.BuyCategories.Add(category.GetStoreCategory(store, true, ct)); }
						foreach (StoreCategoryData category in storeData.Sell) { ct.ThrowIfCancellationRequested(); store.StoreData.SellCategories.Add(category.GetStoreCategory(store, false, ct)); }
						break;

					case DoorComponentData:
						// Eco exposes OpensOutwards as read-only; preserve the DTO until the game API supports restoring it.
						break;
				}
			}
		}

		private static void ValidateRestore(WorldEditBlock block, Vector3 origin, InternalRestoreKind? expectedKind = null)
		{
			ArgumentNullException.ThrowIfNull(block);
			if (!typeof(Block).IsAssignableFrom(block.BlockType)) throw new WorldEditCommandException($"Type {block.BlockType} is not a block type.");
			if (!block.IsSupportedInternally()) throw UnsupportedBlockData(block);
			if (expectedKind is InternalRestoreKind.Block && !block.IsBlockInternally()) throw UnexpectedBlockData(block, expectedKind.Value);
			if (expectedKind is InternalRestoreKind.Plant && !block.IsPlantInternally()) throw UnexpectedBlockData(block, expectedKind.Value);
			if (expectedKind is InternalRestoreKind.WorldObject && !block.IsWorldObjectInternally()) throw UnexpectedBlockData(block, expectedKind.Value);

			if (block.IsWorldObjectInternally())
			{
				WorldObjectBlockData objectData = (WorldObjectBlockData)block.BlockData!;
				if (!typeof(WorldObject).IsAssignableFrom(objectData.WorldObjectType)) throw new WorldEditCommandException($"Type {objectData.WorldObjectType} is not a WorldObject type.");
			}
			else
			{
				ToBlockPosition(origin + block.LocalPosition);
			}
		}

		private static WorldEditCommandException UnsupportedBlockData(WorldEditBlock block) => new($"Unsupported internal BlockData type {block.BlockData?.GetType().FullName ?? "<null>"} at {block.LocalPosition}.");
		private static WorldEditCommandException UnexpectedBlockData(WorldEditBlock block, InternalRestoreKind expectedKind) => new($"Clipboard {expectedKind} entry at {block.LocalPosition} contains {block.BlockData?.GetType().FullName ?? "ordinary block data"}.");

		private static PlantSpecies? FindPlantSpecies(Type plantType, CancellationToken ct)
		{
			PlantSpecies? matchingName = null;
			foreach (PlantSpecies species in EcoSim.AllSpecies.OfType<PlantSpecies>())
			{
				ct.ThrowIfCancellationRequested();
				if (species.GetType() == plantType) return species;
				if (matchingName is null && species.Name == plantType.Name) matchingName = species;
			}
			return matchingName;
		}

		private void ClearWorldObjectPlace(Type worldObjectType, Vector3 position, EcoQuaternion rotation, WorldObject? protectedParent, CancellationToken ct)
		{
			List<Vector3i> occupiedPositions = new();
			foreach (BlockOccupancy occupancy in WorldObject.GetOccupancy(worldObjectType))
			{
				ct.ThrowIfCancellationRequested();
				if (occupancy.BlockType is null) continue;
				Vector3i occupiedPosition = ToBlockPosition(position + rotation.RotateVector(occupancy.Offset));
				if (!WorldHeight.IsValid(occupiedPosition.Y)) throw new WorldEditCommandException($"Cannot restore WorldObject {worldObjectType}: occupied height {occupiedPosition.Y} is outside world height {WorldHeight.Min}..{WorldHeight.Max}.");
				occupiedPositions.Add(occupiedPosition);
			}

			foreach (Vector3i occupiedPosition in occupiedPositions)
			{
				ct.ThrowIfCancellationRequested();
				this.CaptureChange(occupiedPosition, ct);
				this.ClearPosition(occupiedPosition, true, protectedParent, ct);
			}
		}

		private enum InternalRestoreKind
		{
			Block,
			Plant,
			WorldObject
		}
	}
}
