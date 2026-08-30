namespace Eco.Mods.WorldEdit.Core.Commands
{
	internal interface IWorldEditCommand
	{
		CommandHistoryPolicy HistoryPolicy => CommandHistoryPolicy.RecordWorldChanges;
		void Execute(CommandContext context, CancellationToken ct);
	}
}
