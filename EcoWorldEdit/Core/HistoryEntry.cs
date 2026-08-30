using Eco.Mods.WorldEdit.Core.Commands;

namespace Eco.Mods.WorldEdit.Core
{
	/// <summary>A world snapshot which restores the state on the opposite side of a history transition.</summary>
	internal sealed record HistoryEntry(CommandChangeSet Snapshot, bool IsRecovery = false);
}
