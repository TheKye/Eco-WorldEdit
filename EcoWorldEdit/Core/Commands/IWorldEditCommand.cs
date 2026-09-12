namespace Eco.Mods.WorldEdit.Core.Commands
{
	internal interface IWorldEditCommand
	{
		CommandHistoryPolicy HistoryPolicy => CommandHistoryPolicy.RecordWorldChanges;
		bool PreserveClipboardAnchor => false;
		void Execute(CommandContext context, CancellationToken ct);
	}
}
