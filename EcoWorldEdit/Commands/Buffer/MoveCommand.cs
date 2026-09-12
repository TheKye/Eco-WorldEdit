using System.Numerics;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class MoveCommand(Direction Direction, int Amount) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Moves the selected area in a direction.", shortCut: "move", level: ChatAuthorizationLevel.Admin)]
		public static void Move(User user, string directionAndAmount = "1")
		{
			try
			{
				(Direction direction, int amount) = CommandParsing.ParseDirectionAndAmount(user, directionAndAmount);
				if (direction is Direction.Unknown or Direction.None) throw new WorldEditCommandException("Unable to determine direction.");
				CommandResult result = CommandDispatcher.Obj.Execute(user, new MoveCommand(direction, amount));
				if (result.Result.Success) Logging.Success($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				else Logging.CommandFailed("Move", result, user.Player);
			}
			catch (WorldEditCommandException exception) { Logging.ErrorLocStr(exception.Message, user.Player); }
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			WorldRange selection = context.Selection.FixToWorldSize();
			Vector3i offset = Direction.ToVec() * Amount;
			List<WorldEditBlock> snapshot = new();
			BlockCaptureContext captureContext = new();
			foreach (Vector3i position in selection.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				snapshot.AddRange(context.BlockManager.Capture(position, Vector3.Zero, captureContext, ct));
			}
			if (context.BlockManager.GetPlacementHeightRange(snapshot, false, ct) is { } sourceRange)
			{
				PlacementHeightRange targetRange = sourceRange.Offset(offset.Y);
				if (!targetRange.FitsWorld()) throw new WorldEditCommandException($"Cannot move selection to height {targetRange.MinY}..{targetRange.MaxY}. Valid world height is {WorldHeight.Min}..{WorldHeight.Max}.");
			}

			foreach (Vector3i position in selection.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);
			}
			context.BlockManager.CommitBatch(ct);

			context.BlockManager.Restore(snapshot, (Vector3)offset, ct);
			context.UserSession.SetSelection(new WorldRange(selection.min + offset, selection.max + offset));
		}
	}
}
