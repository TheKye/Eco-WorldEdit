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
	internal static class ShiftCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "shift will move the selected area by the specified amount in the looking direction", shortCut: "shift", level: ChatAuthorizationLevel.Admin)]
		public static void Shift(User user, string directionAndAmount = "1")
		{
			try
			{
				UserSession session = WorldEditManager.Obj.GetUserSession(user);
				if (!session.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
				(Direction direction, int amount) = CommandParsing.ParseDirectionAndAmount(user, directionAndAmount);
				if (direction is Direction.Unknown or Direction.None) throw new WorldEditCommandException("Unable to determine direction.");
				Vector3i offset = direction.ToVec() * amount;
				WorldRange selection = session.Selection;
				selection.min += offset;
				selection.max += offset;
				session.SetSelection(selection);
				user.Player.MsgLoc($"Shifted selection {amount} {direction}");
			}
			catch (WorldEditCommandException exception) { user.Player.ErrorLocStr(exception.Message); }
			catch (Exception exception) { Log.WriteException(exception); }
		}
	}
}
