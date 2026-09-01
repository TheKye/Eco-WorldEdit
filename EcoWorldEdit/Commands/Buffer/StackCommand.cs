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

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class StackCommand(Direction Direction, int Amount, int Gap) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Repeats the selected area in a direction.", shortCut: "stack", level: ChatAuthorizationLevel.Admin)]
		public static void Stack(User user, string directionAndAmount = "1", int offset = 0)
		{
			try
			{
				(Direction direction, int amount) = CommandParsing.ParseDirectionAndAmount(user, directionAndAmount);
				if (direction is Direction.Unknown or Direction.None) throw new WorldEditCommandException("Unable to determine direction.");

				CommandResult result = CommandDispatcher.Obj.Execute(user, new StackCommand(direction, amount, offset));
				if (result.Result.Success)
				{
					user.Player.MsgLoc($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.");
				}
				else
				{
					user.Player.Error(result.Result.Message);
				}
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

			int selectionSize = Direction switch
			{
				Direction.Up or Direction.Down => selection.HeightInc,
				Direction.Left or Direction.Right => selection.WidthInc,
				_ => selection.LengthInc,
			};
			Vector3 step = (Vector3)(Direction.ToVec() * (selectionSize + Gap));
			if (Amount > 0 && context.BlockManager.GetPlacementHeightRange(snapshot, false, ct) is { } sourceRange)
			{
				int firstOffsetY = checked((int)step.Y);
				int lastOffsetY = checked(firstOffsetY * Amount);
				PlacementHeightRange targetRange = sourceRange.Offset(firstOffsetY).Union(sourceRange.Offset(lastOffsetY));
				if (!targetRange.FitsWorld()) throw new WorldEditCommandException($"Cannot stack selection at height {targetRange.MinY}..{targetRange.MaxY}. Valid world height is {WorldHeight.Min}..{WorldHeight.Max}.");
			}
			for (int copy = 1; copy <= Amount; copy++)
			{
				ct.ThrowIfCancellationRequested();
				context.BlockManager.Restore(snapshot, step * copy, ct);
			}
		}
	}
}
