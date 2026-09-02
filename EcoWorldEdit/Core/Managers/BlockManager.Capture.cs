using System.Numerics;
using Eco.Gameplay.Blocks;
using Eco.Gameplay.Components;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Components.Store;
using Eco.Gameplay.Objects;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Model.Components;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.World.Blocks;
using Eco.World.Color;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	using World = Eco.World.World;

	internal sealed partial class BlockManager
	{
		public List<WorldEditBlock> Capture(Vector3i worldPosition, Vector3 origin, BlockCaptureContext context, CancellationToken ct = default)
		{
			ct.ThrowIfCancellationRequested();
			ArgumentNullException.ThrowIfNull(context);
			if (!WorldHeight.IsValid(worldPosition.Y)) throw new WorldEditCommandException($"Cannot capture position {worldPosition}: height is outside world height {WorldHeight.Min}..{WorldHeight.Max}.");

			List<WorldEditBlock> captured = new();
			Block block = World.GetBlock(worldPosition);

			if (block is WorldObjectManyBlock manyBlock)
			{
				foreach (WorldObject worldObject in manyBlock.Objects)
				{
					ct.ThrowIfCancellationRequested();
					this.CaptureWorldObject(worldObject, origin, context, captured, ct);
				}
				return captured;
			}

			if (block is WorldObjectBlock worldObjectBlock)
			{
				this.CaptureWorldObject(worldObjectBlock.WorldObjectHandle.Object, origin, context, captured, ct);
				return captured;
			}

			if (BlockUtils.IsPlantBlock(block))
			{
				Plant? plant = EcoSim.PlantSim.GetPlant(worldPosition);
				if (plant is not null)
				{
					this.CapturePlant(block.GetType(), plant, origin, captured);
					return captured;
				}

				captured.Add(new WorldEditBlock(typeof(EmptyBlock), (Vector3)worldPosition - origin, null, null));
				return captured;
			}

			if (BlockContainerManager.Obj.IsBlockContained(worldPosition))
			{
				foreach (WorldObject worldObject in this.FindWorldObjectsAt(worldPosition, ct))
				{
					ct.ThrowIfCancellationRequested();
					this.CaptureWorldObject(worldObject, origin, context, captured, ct);
				}
				if (captured.Count > 0) return captured;
			}

			captured.Add(new WorldEditBlock(block.GetType(), (Vector3)worldPosition - origin, null, GetBlockColor(worldPosition)));
			return captured;
		}

		private void CaptureWorldObject(WorldObject worldObject, Vector3 origin, BlockCaptureContext context, List<WorldEditBlock> captured, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();
			if (IsIgnoredWorldObject(worldObject)) return;
			if (!context.TryCapture(worldObject)) return;

			WorldObject? parent = null;
			if (worldObject.AttachedTo.IsSet) worldObject.AttachedTo.TryGetObject(out parent);
			if (parent is not null) this.CaptureWorldObject(parent, origin, context, captured, ct);

			Guid objectId = context.GetObjectId(worldObject);
			Guid? parentId = parent is null ? null : context.GetObjectId(parent);
			WorldObjectBlockData objectData = new(worldObject.GetType(), worldObject.Rotation, objectId, parentId, worldObject.Name, this.CaptureComponents(worldObject, ct));

			captured.Add(new WorldEditBlock(typeof(WorldObjectBlock), worldObject.Position - origin, objectData, null));
		}

		private List<IWorldObjectComponentData> CaptureComponents(WorldObject worldObject, CancellationToken ct)
		{
			List<IWorldObjectComponentData> components = new();
			ct.ThrowIfCancellationRequested();

			if (worldObject.GetComponent<StorageComponent>() is { } storage && StorageComponentData.Create(storage) is { } storageData)
				components.Add(storageData);
			if (worldObject.GetComponent<CustomTextComponent>() is { } customText)
				components.Add(new CustomTextComponentData(customText.TextData.Text));
			if (worldObject.GetComponent<MintComponent>() is { } mint && MintComponentData.Create(mint.MintData) is { } mintData)
				components.Add(mintData);
			if (worldObject is TechTree.DoorObject door && DoorComponentData.Create(door) is { } doorData)
				components.Add(doorData);
			if (worldObject.GetComponent<StoreComponent>() is { } store && StoreComponentData.Create(store) is { } storeData)
				components.Add(storeData);
			if (worldObject.GetComponent<PluginModulesComponent>() is { } pluginModules && PluginModulesData.Create(pluginModules) is { } pluginModulesData)
				components.Add(pluginModulesData);
			ct.ThrowIfCancellationRequested();

			return components;
		}

		private void CapturePlant(Type blockType, Plant plant, Vector3 origin, List<WorldEditBlock> captured)
		{
			PlantBlockData plantData = new(
				plant.Species.GetType(),
				plant.YieldPercent,
				plant.Dead,
				(int)plant.DeadType,
				plant.DeathTime,
				plant.GrowthPercent,
				plant.Tended);

			captured.Add(new WorldEditBlock(blockType, plant.Position - origin, plantData, null));
		}

		private static string? GetBlockColor(Vector3i position) => BlockColorManager.Obj.TryGetColorData(position, out ByteColor color) ? color.HexRGBA : null;
	}
}
