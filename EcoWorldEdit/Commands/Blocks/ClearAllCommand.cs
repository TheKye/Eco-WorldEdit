using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class ClearAllCommand(int Radius, Vector3i Origin) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Clears everything above your position within the specified radius.", shortCut: "clearall", level: ChatAuthorizationLevel.Admin)]
		public static void ClearAll(User user, int radius = 20)
		{
			try
			{
				ClearAllCommand command = new(Math.Max(0, radius), CommandParsing.GetPosition(user));
				CommandResult result = CommandDispatcher.Obj.Execute(user, command);
				if (result.Result.Success) Logging.Success($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				else Logging.Error(result.Result.Message, user.Player);
			}
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
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
