using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal static class AnchorCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Shows the clipboard paste area at your position.", shortCut: "anchor", level: ChatAuthorizationLevel.Admin)]
		public static void Anchor(User user, bool enabled = true)
		{
			try
			{
				UserSession session = WorldEditManager.Obj.GetUserSession(user);
				if (enabled)
				{
					session.EnableClipboardAnchor(user.Position.Round());
					Logging.Success($"Clipboard anchor set to {session.Selection.min}; paste area is {session.Selection.min} - {session.Selection.max}.", user.Player);
				}
				else
				{
					session.DisableClipboardAnchor(resetSelection: true);
					Logging.SuccessLocStr("Clipboard anchor disabled and selection reset.", user.Player);
				}
			}
			catch (WorldEditCommandException exception) { Logging.ErrorLocStr(exception.Message, user.Player); }
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}
	}
}
