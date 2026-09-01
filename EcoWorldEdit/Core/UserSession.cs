using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Core
{
	internal class UserSession
	{
		public User User { get; private init; }
		public Player Player => this.User.Player;

		public WorldRange Selection { get; private set; } = WorldRange.Invalid;
		public WorldObjectHandle HighlightingObject { get; private set; }

		public Clipboard Clipboard { get; set; } = Clipboard.Empty;

		public LimitedStack<HistoryEntry> UndoHistory { get; } = new LimitedStack<HistoryEntry>(10);
		public LimitedStack<HistoryEntry> RedoHistory { get; } = new LimitedStack<HistoryEntry>(10);
		public HistoryDirection? PendingRecoveryDirection { get; private set; }

		public UserSession(User user)
		{
			this.User = user ?? throw new ArgumentNullException(nameof(user));
		}

		public void SetFirstPosition(Vector3i pos)
		{
			WorldRange range = this.Selection;
			range.min = pos;
			this.SetSelection(range);
		}
		public void SetSecondPosition(Vector3i pos)
		{
			WorldRange range = this.Selection;
			range.max = pos;
			this.SetSelection(range);
		}

		public void ResetSelection()
		{
			this.SetSelection(WorldRange.Invalid);
		}

		public void SetSelection(WorldRange range)
		{
			this.Selection = range;
			this.UpdateHighlightingObject();
		}

		public void DestroyHighlightingObject()
		{
			WorldObjectHandle handle = this.HighlightingObject;
			this.HighlightingObject = default;
			if (handle.TryGetObject(out WorldObject worldObject) && worldObject is WorldEditHighlightingObject) worldObject.Destroy();
		}

		private void UpdateHighlightingObject()
		{
			if (!this.Selection.IsSet())
			{
				this.DestroyHighlightingObject();
				return;
			}

			WorldEditHighlightingObject? highlightingObject = null;
			bool created = false;
			if (this.HighlightingObject.TryGetObject(out WorldObject trackedObject)) highlightingObject = trackedObject as WorldEditHighlightingObject;

			if (highlightingObject is null)
			{
				highlightingObject = WorldObjectManager.ForceAdd(
					typeof(WorldEditHighlightingObject),
					this.User,
					this.Selection.min,
					Quaternion.Identity,
					validatePlacement: false) as WorldEditHighlightingObject ?? throw new InvalidOperationException($"Unable to create {nameof(WorldEditHighlightingObject)}.");
				this.HighlightingObject = new WorldObjectHandle(highlightingObject);
				created = true;
			}

			try
			{
				highlightingObject.SetHighlightArea(this.Selection);
			}
			catch
			{
				if (created) this.DestroyHighlightingObject();
				throw;
			}
		}

		public LimitedStack<HistoryEntry> GetHistory(HistoryDirection direction) => direction == HistoryDirection.Undo ? this.UndoHistory : this.RedoHistory;

		public void MarkPendingRecovery(HistoryDirection direction)
		{
			LimitedStack<HistoryEntry> history = this.GetHistory(direction);
			if (!history.TryPeek(out HistoryEntry entry) || !entry.IsRecovery) throw new InvalidOperationException($"{direction} recovery must be at the top of its history stack.");
			this.PendingRecoveryDirection = direction;
		}

		public void CompleteRecovery(HistoryDirection completedDirection)
		{
			HistoryDirection previousDirection = completedDirection == HistoryDirection.Undo ? HistoryDirection.Redo : HistoryDirection.Undo;
			LimitedStack<HistoryEntry> previousHistory = this.GetHistory(previousDirection);
			this.PendingRecoveryDirection = previousHistory.TryPeek(out HistoryEntry previous) && previous.IsRecovery ? previousDirection : null;
		}

		public bool DiscardPendingRecoveries()
		{
			bool discarded = DiscardRecoveryEntries(this.UndoHistory);
			discarded |= DiscardRecoveryEntries(this.RedoHistory);
			this.PendingRecoveryDirection = null;
			return discarded;
		}

		public SuspendedHistoryRecovery SuspendPendingRecoveries()
		{
			HistoryDirection pendingDirection = this.PendingRecoveryDirection ?? throw new InvalidOperationException("No history recovery is pending.");
			List<HistoryEntry> undoEntries = DetachRecoveryEntries(this.UndoHistory);
			List<HistoryEntry> redoEntries = DetachRecoveryEntries(this.RedoHistory);
			if (undoEntries.Count == 0 && redoEntries.Count == 0) throw new InvalidOperationException("Pending history recovery is not present on either history stack.");
			this.PendingRecoveryDirection = null;
			return new(undoEntries, redoEntries, pendingDirection);
		}

		public void RestorePendingRecoveries(SuspendedHistoryRecovery recovery)
		{
			ArgumentNullException.ThrowIfNull(recovery);
			if (this.PendingRecoveryDirection is not null) throw new InvalidOperationException("A new history recovery is already pending.");
			RestoreRecoveryEntries(this.UndoHistory, recovery.UndoEntries);
			RestoreRecoveryEntries(this.RedoHistory, recovery.RedoEntries);
			this.PendingRecoveryDirection = recovery.PendingDirection;
		}

		private static bool DiscardRecoveryEntries(LimitedStack<HistoryEntry> history)
		{
			bool discarded = false;
			while (history.TryPeek(out HistoryEntry entry) && entry.IsRecovery)
			{
				history.Pop();
				discarded = true;
			}
			return discarded;
		}

		private static List<HistoryEntry> DetachRecoveryEntries(LimitedStack<HistoryEntry> history)
		{
			List<HistoryEntry> entries = new();
			while (history.TryPeek(out HistoryEntry entry) && entry.IsRecovery)
			{
				entries.Add(history.Pop());
			}
			return entries;
		}

		private static void RestoreRecoveryEntries(LimitedStack<HistoryEntry> history, IReadOnlyList<HistoryEntry> entries)
		{
			for (int i = entries.Count - 1; i >= 0; i--)
			{
				history.PushTransient(entries[i]);
			}
		}
	}

	internal sealed record SuspendedHistoryRecovery(IReadOnlyList<HistoryEntry> UndoEntries, IReadOnlyList<HistoryEntry> RedoEntries, HistoryDirection PendingDirection);
}
