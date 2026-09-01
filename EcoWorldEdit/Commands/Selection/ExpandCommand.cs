using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Selection
{
	[ChatCommandHandler]
	internal static class ExpandCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Expands the selection by an amount in a direction.", shortCut: "expand", level: ChatAuthorizationLevel.Admin)]
		public static void Expand(User user, string directionAndAmount = "1")
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
					Vector3i expansion = new(amount, amount, amount);
					selection.min -= expansion;
					selection.max += expansion;
				}
				else
				{
					Vector3i offset = direction.ToVec() * amount;
					if (direction is Direction.Left or Direction.Back or Direction.Down) selection.min += offset;
					else selection.max += offset;
				}

				session.SetSelection(selection);
				user.Player.MsgLoc($"Expanded selection {amount} {direction}");
			}
			catch (WorldEditCommandException exception) { user.Player.ErrorLocStr(exception.Message); }
			catch (Exception exception) { Log.WriteException(exception); }
		}
	}
}
