using Eco.Core.Utils;
using Eco.Gameplay.Blocks;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Gameplay.Systems.EcoMarketplace;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World.Blocks;
using Eco.World.Color;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	using World = Eco.World.World;

	internal sealed partial class BlockManager
	{
		private readonly List<PendingBlockChange> _pendingBlockChanges = new();
		private readonly Dictionary<Vector3i, int> _pendingBlockIndices = new();
		internal bool HasPendingBatch => this._pendingBlockChanges.Count != 0;

		public bool TryScheduleBlock(Type blockType, Vector3i position, CancellationToken ct = default) => this.TryScheduleBlock(blockType, position, null, ct);

		public void CommitBatch(CancellationToken ct = default)
		{
			ct.ThrowIfCancellationRequested();
			if (this._pendingBlockChanges.Count == 0) return;

			// Detach the current batch before applying it. BatchApply is not atomic, so a failed batch must never be retried implicitly against an already partially changed world.
			List<PendingBlockChange> pending = new(this._pendingBlockChanges);
			this._pendingBlockChanges.Clear();
			this._pendingBlockIndices.Clear();

			List<Eco.World.BlockChange> changes = new(pending.Count);
			List<BlockAccountingChange> accountingChanges = new(pending.Count);
			Dictionary<Type, int> paidItemDeltas = new();

			foreach (PendingBlockChange change in pending)
			{
				ct.ThrowIfCancellationRequested();
				if (IsImpenetrable(change.Position))
				{
					Log.WriteWarningLineLoc($"Skipped setting block {change.BlockType} at {change.Position}: position is impenetrable.");
					continue;
				}

				Block existingBlock = World.GetBlock(change.Position);
				bool replaceDirectly = CanReplaceDirectly(existingBlock, change.Position);
				BlockItem? newItem = BlockItem.FirstCreatingItem(change.BlockType);
				int pendingPaidDelta = newItem is not null && newItem.IsPaidItem() ? paidItemDeltas.GetOr(newItem.Type, 0) : 0;
				Result canCreate = StrangeItemProtection.CanCreateBlock(this._userSession.User, change.BlockType, pendingPaidDelta + 1);
				if (canCreate.Failed)
				{
					Log.WriteWarningLineLoc($"Skipped setting block {change.BlockType} at {change.Position}: StrangeItemProtection denied creation. {canCreate.Message.Trim()}");
					try { this._userSession.Player.Error(canCreate.Message); }
					catch (Exception exception) { Log.WriteException(exception); }
					continue;
				}

				Block? replacedOrdinaryBlock = null;
				bool clearRetainedColor = false;
				if (replaceDirectly)
				{
					replacedOrdinaryBlock = existingBlock is EmptyBlock ? null : existingBlock;
					clearRetainedColor = string.IsNullOrEmpty(change.Color) && existingBlock.GetType() == change.BlockType && BlockColorManager.Obj.TryGetColorData(change.Position, out _);
					if (replacedOrdinaryBlock is not null && BlockItem.FirstCreatingItem(replacedOrdinaryBlock.GetType()) is { } oldItem && oldItem.IsPaidItem())
					{
						paidItemDeltas[oldItem.Type] = paidItemDeltas.GetOr(oldItem.Type, 0) - 1;
					}
				}
				else
				{
					this.ClearPosition(change.Position, true, cancellationToken: ct);
				}

				if (newItem is not null && newItem.IsPaidItem()) paidItemDeltas[newItem.Type] = paidItemDeltas.GetOr(newItem.Type, 0) + 1;

				// Register before handing the list to BatchApply. If it throws after applying only
				// part of the list, the command scope still retains an undo/recovery snapshot.
				this._changes.RegisterChangedBlock(change.Position);
				changes.Add(new Eco.World.BlockChange(change.BlockType, change.Position));
				accountingChanges.Add(new(change, replacedOrdinaryBlock, clearRetainedColor));
			}

			if (changes.Count == 0) return;
			World.BatchApply(changes);
			List<Vector3i> retainedColorPositions = accountingChanges.Where(x => x.ClearRetainedColor).Select(x => x.Change.Position).ToList();
			if (retainedColorPositions.Count > 0) BlockColorManager.Obj.ClearColors(retainedColorPositions, true);

			foreach (BlockAccountingChange accounting in accountingChanges)
			{
				if (accounting.ReplacedOrdinaryBlock is not null) StrangeItemProtection.DecrementUsed(this._userSession.User, accounting.ReplacedOrdinaryBlock);
				StrangeItemProtection.IncrementUsedBlock(this._userSession.User, accounting.Change.BlockType);
				if (!string.IsNullOrEmpty(accounting.Change.Color)) BlockColorManager.Obj.SetColor(accounting.Change.Position, ByteColor.FromHex(accounting.Change.Color));
			}
		}

		private bool TryScheduleBlock(Type blockType, Vector3i position, string? color, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();
			ValidateBlockType(blockType);
			if (IsImpenetrable(position))
			{
				Log.WriteWarningLineLoc($"Skipped setting block {blockType} at {position}: position is impenetrable.");
				return false;
			}

			this.CaptureChange(position, ct);
			PendingBlockChange change = new(blockType, position, color);
			if (this._pendingBlockIndices.TryGetValue(position, out int existingIndex))
			{
				this._pendingBlockChanges[existingIndex] = change;
			}
			else
			{
				this._pendingBlockIndices.Add(position, this._pendingBlockChanges.Count);
				this._pendingBlockChanges.Add(change);
			}
			return true;
		}

		private static bool CanReplaceDirectly(Block block, Vector3i position) => block is not WorldObjectManyBlock and not WorldObjectBlock and not PlantBlock and not TreeBlock and not IWaterBlock && !BlockContainerManager.Obj.IsBlockContained(position);

		private readonly record struct PendingBlockChange(Type BlockType, Vector3i Position, string? Color);
		private readonly record struct BlockAccountingChange(PendingBlockChange Change, Block? ReplacedOrdinaryBlock, bool ClearRetainedColor);
	}
}
