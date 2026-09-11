using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Shared.Localization;

namespace Eco.Mods.WorldEdit.Commands.General
{
	[ChatCommandHandler]
	internal sealed class CancelCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Cancels your currently executing WorldEdit command.", shortCut: "cancel", level: ChatAuthorizationLevel.Admin)]
		public static void Cancel(User user)
		{
			try
			{
				if (CommandDispatcher.Obj.TryCancelCommand(user))
				{
					Logging.SuccessLocStr("WorldEdit command cancellation requested.", user.Player);
				}
				else
				{
					Logging.Warning(Localizer.DoStr("No cancellable WorldEdit command is currently running."), user.Player);
				}
			}
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}
	}
}
