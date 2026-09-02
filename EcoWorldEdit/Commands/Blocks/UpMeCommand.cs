using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class UpMeCommand(int Count, Vector3i Origin) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Moves you upward and places a support block below you.", shortCut: "upme", level: ChatAuthorizationLevel.Admin)]
		public static void UpMe(User user, int count = 1)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new UpMeCommand(count, CommandParsing.GetPosition(user)));
				if (result.Result.Success) Logging.Success($"Moved up in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				else Logging.Error(result.Result.Message, user.Player);
			}
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			Vector3i support = Origin + Vector3i.Up * Count;
			context.BlockManager.TryScheduleBlock(typeof(StoneBlock), support, ct);
			context.BlockManager.CommitBatch(ct);
			context.Player.SetPosition(support + Vector3i.Up * 2);
		}
	}
}
