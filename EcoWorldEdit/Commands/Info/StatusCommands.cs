using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;

namespace Eco.Mods.WorldEdit.Commands.Info
{
	[ChatCommandHandler]
	internal static class StatusCommands
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Shows the installed WorldEdit version.", shortCut: "weversion", level: ChatAuthorizationLevel.Admin)]
		public static void Version(User user) => Logging.SuccessLocStr($"World Edit Version: {EcoWorldEdit.Version}", user.Player);

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Shows your current WorldEdit direction.", shortCut: "welooking", level: ChatAuthorizationLevel.Admin)]
		public static void Looking(User user) => Logging.SuccessLocStr($"Looking direction: {CommandParsing.GetLookingDirection(user)}", user.Player);
	}
}
