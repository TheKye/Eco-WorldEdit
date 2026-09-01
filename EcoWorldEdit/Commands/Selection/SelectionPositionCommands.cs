using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Selection
{
	[ChatCommandHandler]
	internal static class SelectionPositionCommands
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Sets the first selection point to the given coordinates or your position.", shortCut: "setpos1", level: ChatAuthorizationLevel.Admin)]
		public static void SetPos1(User user, string? coordinate = null) => SetPosition(user, coordinate, true);

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Sets the second selection point to the given coordinates or your position.", shortCut: "setpos2", level: ChatAuthorizationLevel.Admin)]
		public static void SetPos2(User user, string? coordinate = null) => SetPosition(user, coordinate, false);

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Clears the current selection.", shortCut: "reset", level: ChatAuthorizationLevel.Admin)]
		public static void Reset(User user)
		{
			try
			{
				UserSession session = WorldEditManager.Obj.GetUserSession(user);
				session.ResetSelection();
				user.Player.MsgLocStr("WorldEdit: Positions reset");
			}
			catch (Exception exception) { Log.WriteException(exception); }
		}

		private static void SetPosition(User user, string? coordinate, bool first)
		{
			try
			{
				Vector3i position = CommandParsing.GetPosition(user, coordinate);
				UserSession session = WorldEditManager.Obj.GetUserSession(user);
				if (first) session.SetFirstPosition(position);
				else session.SetSecondPosition(position);
				user.Player.MsgLoc($"{(first ? "First" : "Second")} Position set to {position}");
			}
			catch (WorldEditCommandException exception) { user.Player.ErrorLocStr(exception.Message); }
			catch (Exception exception) { Log.WriteException(exception); }
		}
	}
}
