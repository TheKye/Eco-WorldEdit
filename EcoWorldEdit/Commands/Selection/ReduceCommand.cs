using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Selection
{
	[ChatCommandHandler]
	internal static class ReduceCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Reduces the selection by an amount in a direction.", shortCut: "reduce", level: ChatAuthorizationLevel.Admin)]
		public static void Reduce(User user, string directionAndAmount = "1")
		{
			try
			{
				UserSession session = WorldEditManager.Obj.GetUserSession(user);
				if (!session.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
				(Direction direction, int amount) = CommandParsing.ParseDirectionAndAmount(user, directionAndAmount);
				if (direction == Direction.Unknown) throw new WorldEditCommandException("Unable to determine direction.");
				WorldRange selection = session.Selection;

				if (direction == Direction.None)
				{
					Vector3i reduction = new(amount, amount, amount);
					selection.min += reduction;
					selection.max -= reduction;
				}
				else
				{
					Vector3i offset = direction.ToVec() * -amount;
					if (direction is Direction.Left or Direction.Back or Direction.Down) selection.min += offset;
					else selection.max += offset;
				}

				if (selection.min.x > selection.max.x || selection.min.y > selection.max.y || selection.min.z > selection.max.z)
					throw new WorldEditCommandException("The selection cannot be reduced past zero size.");
				session.SetSelection(selection);
				Logging.Success($"Reduced selection {amount} {direction}", user.Player);
			}
			catch (WorldEditCommandException exception) { Logging.ErrorLocStr(exception.Message, user.Player); }
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}
	}
}
