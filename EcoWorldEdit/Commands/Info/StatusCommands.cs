using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Utils;

namespace Eco.Mods.WorldEdit.Commands.Info
{
	[ChatCommandHandler]
	internal static class StatusCommands
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Shows the installed WorldEdit version.", shortCut: "weversion", level: ChatAuthorizationLevel.Admin)]
		public static void Version(User user) => user.Player.MsgLocStr($"World Edit Version: {EcoWorldEdit.Version}");

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Shows your current WorldEdit direction.", shortCut: "welooking", level: ChatAuthorizationLevel.Admin)]
		public static void Looking(User user) => user.Player.MsgLocStr($"Looking direction: {CommandParsing.GetLookingDirection(user)}");
	}
}
