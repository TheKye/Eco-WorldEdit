using Eco.Gameplay.Objects;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Utils.Exceptions;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	/// <summary>State shared by all block restores that belong to one operation.</summary>
	internal sealed class BlockRestoreContext
	{
		private readonly Dictionary<Guid, WorldEditBlock> _objectBlocks = new();
		private readonly Dictionary<Guid, WorldObject> _restoredObjects = new();
		private readonly HashSet<Guid> _restoringObjects = new();

		public bool SkipEmpty { get; init; }

		public BlockRestoreContext(IReadOnlyList<WorldEditBlock> worldObjects, CancellationToken ct)
		{
			ArgumentNullException.ThrowIfNull(worldObjects);
			ct.ThrowIfCancellationRequested();

			foreach (WorldEditBlock block in worldObjects)
			{
				ct.ThrowIfCancellationRequested();
				if (!block.IsWorldObjectInternally()) continue;
				if (block.BlockData is not WorldObjectBlockData { ObjectId: Guid objectId }) continue;
				if (!this._objectBlocks.TryAdd(objectId, block)) throw new WorldEditCommandException($"WorldObject identifier {objectId} is duplicated.");
			}

			foreach (WorldEditBlock block in worldObjects)
			{
				ct.ThrowIfCancellationRequested();
				if (!block.IsWorldObjectInternally()) continue;
				if (block.BlockData is not WorldObjectBlockData { ParentId: Guid parentId }) continue;
				if (!this._objectBlocks.ContainsKey(parentId)) throw new WorldEditCommandException($"WorldObject references missing parent {parentId}.");
			}

			Dictionary<Guid, byte> states = new();
			foreach (Guid objectId in this._objectBlocks.Keys)
			{
				ct.ThrowIfCancellationRequested();
				this.ValidateHierarchy(objectId, states, ct);
			}
		}

		public bool TryGetBlock(Guid objectId, out WorldEditBlock block) => this._objectBlocks.TryGetValue(objectId, out block!);
		public bool TryGetRestoredObject(Guid objectId, out WorldObject worldObject) => this._restoredObjects.TryGetValue(objectId, out worldObject!);

		public void BeginRestore(Guid objectId)
		{
			if (!this._restoringObjects.Add(objectId)) throw new WorldEditCommandException($"Cycle detected in WorldObject hierarchy at {objectId}.");
		}

		public void CompleteRestore(Guid objectId, WorldObject worldObject)
		{
			ArgumentNullException.ThrowIfNull(worldObject);
			this._restoringObjects.Remove(objectId);
			this._restoredObjects.Add(objectId, worldObject);
		}

		public void CancelRestore(Guid objectId) => this._restoringObjects.Remove(objectId);

		private void ValidateHierarchy(Guid objectId, Dictionary<Guid, byte> states, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();
			if (states.TryGetValue(objectId, out byte state))
			{
				if (state == 1) throw new WorldEditCommandException($"Cycle detected in WorldObject hierarchy at {objectId}.");
				return;
			}

			states.Add(objectId, 1);
			WorldObjectBlockData objectData = (WorldObjectBlockData)this._objectBlocks[objectId].BlockData!;
			if (objectData.ParentId is Guid parentId) this.ValidateHierarchy(parentId, states, ct);
			states[objectId] = 2;
		}
	}
}
