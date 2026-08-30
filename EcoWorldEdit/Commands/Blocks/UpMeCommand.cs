using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Logging;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class UpMeCommand(int Count, Vector3i Origin) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Upme will move the player upwards and place a block under the player", shortCut: "upme", level: ChatAuthorizationLevel.Admin)]
		public static void UpMe(User user, int count = 1)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new UpMeCommand(count, CommandParsing.GetPosition(user)));
				if (result.Result.Success) user.Player.MsgLoc($"Moved up in {result.Elapsed.TotalMilliseconds}ms.");
				else user.Player.Error(result.Result.Message);
			}
			catch (Exception exception) { Log.WriteException(exception); }
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
