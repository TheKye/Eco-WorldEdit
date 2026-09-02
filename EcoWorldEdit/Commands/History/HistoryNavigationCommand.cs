using System.Numerics;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils.Exceptions;

namespace Eco.Mods.WorldEdit.Commands.History
{
	internal abstract class HistoryNavigationCommand(int ActionCount) : IWorldEditCommand
	{
		public CommandHistoryPolicy HistoryPolicy => CommandHistoryPolicy.ManageHistory;

		protected abstract string ActionName { get; }
		protected abstract HistoryDirection Direction { get; }
		protected abstract LimitedStack<HistoryEntry> GetSource(UserSession session);
		protected abstract LimitedStack<HistoryEntry> GetDestination(UserSession session);

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (ActionCount <= 0) throw new WorldEditCommandException($"{this.ActionName} count must be greater than zero.");

			UserSession session = context.UserSession;
			HistoryDirection? deviatedFrom = session.PendingRecoveryDirection is HistoryDirection pendingDirection && pendingDirection != this.Direction ? pendingDirection : null;
			SuspendedHistoryRecovery? suspendedRecovery = deviatedFrom is null ? null : session.SuspendPendingRecoveries();

			try
			{
				LimitedStack<HistoryEntry> source = this.GetSource(session);
				LimitedStack<HistoryEntry> destination = this.GetDestination(session);
				bool applyingRecovery = source.TryPeek(out HistoryEntry? firstEntry) && firstEntry.IsRecovery;
				if (session.PendingRecoveryDirection == this.Direction && !applyingRecovery) throw new InvalidOperationException($"Pending {this.ActionName} recovery is not at the top of its history stack.");
				if (applyingRecovery && session.PendingRecoveryDirection is null) session.MarkPendingRecovery(this.Direction);

				int actualCount = applyingRecovery ? 1 : Math.Min(ActionCount, source.Count);
				if (actualCount == 0) throw new WorldEditCommandException($"Nothing to {this.ActionName.ToLowerInvariant()}.");

				for (int i = 1; i <= actualCount; i++)
				{
					ct.ThrowIfCancellationRequested();
					if (!source.TryPeek(out HistoryEntry? entry)) throw new WorldEditCommandException($"Nothing to {this.ActionName.ToLowerInvariant()}.");

					CommandScope reverseScope = context.CreateScope();
					CommandChangeSet reverseChangeSet = reverseScope.Changes;
					BlockManager blockManager = reverseScope.BlockManager;
					try
					{
						blockManager.Restore(entry.Snapshot.AffectedBlocks, Vector3.Zero, ct);
					}
					catch
					{
						// Keep the source entry until its restore succeeds. Only actual mutations
						// invalidate a deliberately bypassed recovery or create a new recovery.
						if (reverseChangeSet.ChangedBlocks > 0)
						{
							this.DiscardSuspendedRecovery(context, ref suspendedRecovery, deviatedFrom);
							destination.PushTransient(new HistoryEntry(reverseChangeSet, IsRecovery: true));
							HistoryDirection recoveryDirection = Opposite(this.Direction);
							session.MarkPendingRecovery(recoveryDirection);
							Notify(context, $"{this.ActionName} was only partially completed. The world may be in an intermediate state. Run {recoveryDirection.ToString().ToLowerInvariant()} to revert the changes made by this {this.ActionName.ToLowerInvariant()} attempt. Executing another world-changing command will discard this recovery snapshot.");
						}
						throw;
					}

					if (reverseChangeSet.ChangedBlocks == 0 && suspendedRecovery is not null) return;
					this.DiscardSuspendedRecovery(context, ref suspendedRecovery, deviatedFrom);

					HistoryEntry completedEntry = source.Pop();
					if (!ReferenceEquals(completedEntry, entry)) throw new InvalidOperationException("History changed while a command was executing.");
					if (entry.IsRecovery)
					{
						session.CompleteRecovery(this.Direction);
						if (session.PendingRecoveryDirection is HistoryDirection previousRecovery)
							Notify(context, $"{this.ActionName} recovery completed. A previous recovery is now pending; run {previousRecovery.ToString().ToLowerInvariant()} to continue returning to the state before the failed history operation.");
						else
							Notify(context, $"{this.ActionName} recovery completed. The partial attempt was reverted; you can retry {Opposite(this.Direction).ToString().ToLowerInvariant()}.");
						return;
					}

					if (reverseChangeSet.ChangedBlocks > 0) destination.Push(new HistoryEntry(reverseChangeSet));
					if (actualCount > 1) Notify(context, $"{this.ActionName} {i}/{actualCount} done.");
				}
			}
			finally
			{
				if (suspendedRecovery is not null) session.RestorePendingRecoveries(suspendedRecovery);
			}
		}

		private static HistoryDirection Opposite(HistoryDirection direction) => direction == HistoryDirection.Undo ? HistoryDirection.Redo : HistoryDirection.Undo;

		private void DiscardSuspendedRecovery(CommandContext context, ref SuspendedHistoryRecovery? suspendedRecovery, HistoryDirection? deviatedFrom)
		{
			if (suspendedRecovery is null) return;
			suspendedRecovery = null;
			Notify(context, $"The pending history recovery was discarded because {this.ActionName.ToLowerInvariant()} was executed instead of the recommended {deviatedFrom!.Value.ToString().ToLowerInvariant()}. Guaranteed restoration to the state before the failed history operation is no longer available.");
		}

		private static void Notify(CommandContext context, FormattableString message)
		{
			Logging.Success(message, context.Player);
		}
	}
}
