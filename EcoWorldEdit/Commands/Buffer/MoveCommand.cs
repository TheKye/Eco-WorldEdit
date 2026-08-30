using System.Numerics;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class MoveCommand(Direction Direction, int Amount) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Move blocks in the Selected Area to a direction", shortCut: "move", level: ChatAuthorizationLevel.Admin)]
		public static void Move(User user, string directionAndAmount = "1")
		{
			try
			{
				(Direction direction, int amount) = CommandParsing.ParseDirectionAndAmount(user, directionAndAmount);
				if (direction is Direction.Unknown or Direction.None) throw new WorldEditCommandException("Unable to determine direction.");
				CommandResult result = CommandDispatcher.Obj.Execute(user, new MoveCommand(direction, amount));
				if (result.Result.Success) user.Player.MsgLoc($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.");
				else user.Player.Error(result.Result.Message);
			}
			catch (WorldEditCommandException exception) { user.Player.ErrorLocStr(exception.Message); }
			catch (Exception exception) { Log.WriteException(exception); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			WorldRange selection = context.Selection.FixToWorldSize();
			List<WorldEditBlock> snapshot = new();
			BlockCaptureContext captureContext = new();
			foreach (Vector3i position in selection.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				snapshot.AddRange(context.BlockManager.Capture(position, Vector3.Zero, captureContext, ct));
			}

			foreach (Vector3i position in selection.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);
			}
			context.BlockManager.CommitBatch(ct);

			Vector3i offset = Direction.ToVec() * Amount;
			context.BlockManager.Restore(snapshot, (Vector3)offset, ct);
			context.UserSession.SetSelection(new WorldRange(selection.min + offset, selection.max + offset));
		}
	}
}
