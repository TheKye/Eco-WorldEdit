using System.Numerics;
using Eco.Core.Utils;
using Eco.Gameplay.Blocks;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.IoC;
using Eco.Shared.Logging;
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
		private const float BlockPositionTolerance = 0.0001f;

		private readonly UserSession _userSession;
		private readonly CommandChangeSet _changes;
		private readonly BlockCaptureContext _undoCaptureContext = new();

		public BlockManager(UserSession userSession, CommandChangeSet changes)
		{
			this._userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
			this._changes = changes ?? throw new ArgumentNullException(nameof(changes));
		}

		public bool TrySetBlock(Type blockType, Vector3i position, CancellationToken ct = default)
		{
			ct.ThrowIfCancellationRequested();
			if (this.HasPendingBatch) throw new InvalidOperationException("Commit the pending block batch before applying an immediate block change.");
			ValidateBlockType(blockType);
			if (!WorldHeight.IsValid(position.Y))
			{
				Log.WriteWarningLineLoc($"Skipped setting block {blockType} at {position}: height is outside the world.");
				return false;
			}
			if (IsImpenetrable(position))
			{
				Log.WriteWarningLineLoc($"Skipped setting block {blockType} at {position}: position is impenetrable.");
				return false;
			}

			Result canCreate = StrangeItemProtection.CanCreateBlock(this._userSession.User, blockType);
			if (canCreate.Failed)
			{
				Log.WriteWarningLineLoc($"Skipped setting block {blockType} at {position}: StrangeItemProtection denied creation. {canCreate.Message.Trim()}");
				try { this._userSession.Player.Error(canCreate.Message); }
				catch (Exception exception) { Log.WriteException(exception); }
				return false;
			}

			this.CaptureChange(position, ct);
			this.ClearPosition(position, true, cancellationToken: ct);
			ct.ThrowIfCancellationRequested();
			World.SetBlock(blockType, position);
			this._changes.RegisterChangedBlock(position);
			StrangeItemProtection.IncrementUsedBlock(this._userSession.User, blockType);
			return true;
		}

		public bool TrySetColor(Vector3i position, ByteColor color, CancellationToken ct = default)
		{
			ct.ThrowIfCancellationRequested();
			if (!WorldHeight.IsValid(position.Y)) return false;
			if (IsImpenetrable(position)) return false;
			this.CaptureChange(position, ct);
			if (color == ByteColor.Clear) BlockColorManager.Obj.ClearColors([position], true);
			else BlockColorManager.Obj.SetColor(position, color);
			this._changes.RegisterChangedBlock(position);
			return true;
		}

		public bool TryGrowPlant(Vector3i position, CancellationToken ct = default)
		{
			ct.ThrowIfCancellationRequested();
			if (!WorldHeight.IsValid(position.Y)) return false;
			Plant? plant = EcoSim.PlantSim.GetPlant(position);
			if (plant is null || plant.Position != position) return false;
			this.CaptureChange(position, ct);
			plant.GrowthPercent = 1f;
			plant.Tended = true;
			plant.Tick();
			this._changes.RegisterChangedBlock(position);
			return true;
		}

		private bool ClearPosition(Vector3i position, bool deleteBlock, WorldObject? protectedObject = null, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!WorldHeight.IsValid(position.Y)) return false;
			if (IsImpenetrable(position))
			{
				Log.WriteWarningLineLoc($"Skipped clearing position {position}: position is impenetrable.");
				return false;
			}
			Block block = World.GetBlock(position);
			if (block is EmptyBlock) return false;

			if (block is WorldObjectManyBlock manyBlock)
			{
				bool destroyed = false;
				List<WorldObject> objectsToDestroy = manyBlock.Objects.Where(x => !ReferenceEquals(x, protectedObject) && !IsIgnoredWorldObject(x)).ToList();
				foreach (WorldObject worldObject in objectsToDestroy)
				{
					Item creatingItem = WorldObjectItem.GetCreatingItemTemplateFromType(worldObject.GetType());
					if (!worldObject.Destroy()) continue;
					this._changes.RegisterChangedBlock(position);
					StrangeItemProtection.DecrementUsedItem(this._userSession.User, creatingItem);
					destroyed = true;
				}
				return destroyed;
			}

			if (block is WorldObjectBlock worldObjectBlock)
			{
				WorldObject worldObject = worldObjectBlock.WorldObjectHandle.Object;
				if (ReferenceEquals(worldObject, protectedObject) || IsIgnoredWorldObject(worldObject)) return false;
				Item creatingItem = WorldObjectItem.GetCreatingItemTemplateFromType(worldObject.GetType());
				if (!worldObject.Destroy()) return false;
				this._changes.RegisterChangedBlock(position);
				StrangeItemProtection.DecrementUsedItem(this._userSession.User, creatingItem);
				return true;
			}

			if (block is PlantBlock or TreeBlock)
			{
				Plant? plant = EcoSim.PlantSim.GetPlant(position);
				if (plant is null) return false;
				EcoSim.PlantSim.DestroyPlant(plant, DeathType.DivineIntervention, true);
				this._changes.RegisterChangedBlock(position);
				return true;
			}

			if (BlockContainerManager.Obj.IsBlockContained(position))
			{
				WorldObject? contained = this.FindWorldObjectsAt(position, cancellationToken).FirstOrDefault(x => !ReferenceEquals(x, protectedObject));
				if (contained is null) return false;
				Item creatingItem = WorldObjectItem.GetCreatingItemTemplateFromType(contained.GetType());
				if (!contained.Destroy()) return false;
				this._changes.RegisterChangedBlock(position);
				StrangeItemProtection.DecrementUsedItem(this._userSession.User, creatingItem);
				return true;
			}

			if (block is IWaterBlock)
			{
				World.DeleteBlock(position, false);
				this._changes.RegisterChangedBlock(position);
				return true;
			}

			if (!deleteBlock) return false;
			World.DeleteBlock(position);
			this._changes.RegisterChangedBlock(position);
			StrangeItemProtection.DecrementUsed(this._userSession.User, block);
			return true;
		}

		private void CaptureChange(Vector3i position, CancellationToken ct = default)
		{
			ct.ThrowIfCancellationRequested();
			if (!this._changes.TryBeginCapture(position)) return;
			this._changes.AddCapturedBlocks(this.Capture(position, Vector3.Zero, this._undoCaptureContext, ct));
		}

		private IEnumerable<WorldObject> FindWorldObjectsAt(Vector3i position, CancellationToken ct = default)
		{
			foreach (WorldObject worldObject in ServiceHolder<IWorldObjectManager>.Obj.All)
			{
				ct.ThrowIfCancellationRequested();
				if (!IsIgnoredWorldObject(worldObject) && worldObject.WorldOccupancy.Contains(position)) yield return worldObject;
			}
		}

		private static bool IsIgnoredWorldObject(WorldObject? worldObject) => worldObject is WorldEditHighlightingObject;
		private static bool IsIgnoredWorldObjectType(Type worldObjectType) => worldObjectType == typeof(WorldEditHighlightingObject);

		private static Vector3i ToBlockPosition(Vector3 position)
		{
			Vector3i blockPosition = (Vector3i)position;
			if (Vector3.DistanceSquared(position, blockPosition) > BlockPositionTolerance * BlockPositionTolerance) throw new WorldEditCommandException($"Position {position} is not aligned to the block grid.");
			return blockPosition;
		}

		public static bool IsImpenetrable(Vector3i position) => position.Y < 0 || World.GetBlock(position).Is<Impenetrable>();

		private static void ValidateBlockType(Type blockType)
		{
			ArgumentNullException.ThrowIfNull(blockType);
			if (!typeof(Block).IsAssignableFrom(blockType)) throw new ArgumentOutOfRangeException(nameof(blockType), blockType, "Type must derive from Block.");
		}
	}
}
