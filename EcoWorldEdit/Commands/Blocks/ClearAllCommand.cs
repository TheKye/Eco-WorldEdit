using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class ClearAllCommand(int Radius, Vector3i Origin) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Clear the place for construction from everything", shortCut: "clearall", level: ChatAuthorizationLevel.Admin)]
		public static void ClearAll(User user, int square = 20)
		{
			try
			{
				ClearAllCommand command = new(Math.Max(0, square), CommandParsing.GetPosition(user));
				CommandResult result = CommandDispatcher.Obj.Execute(user, command);
				if (result.Result.Success) user.Player.MsgLoc($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.");
				else user.Player.Error(result.Result.Message);
			}
			catch (Exception exception) { Log.WriteException(exception); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			WorldRange area = WorldRange.SurroundingSpace(Origin, Radius);
			area.min.y = Origin.y;
			area.max.y = WorldHeight.Max;
			area = area.FixToWorldSize();
			foreach (Vector3i position in area.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);
			}
			context.BlockManager.CommitBatch(ct);
		}
	}
}
