using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;

namespace Eco.Mods.WorldEdit.Commands.General
{
	[ChatCommandHandler]
	internal class WorldEditCommand
	{
		[ChatCommand(helpText: "Lists available WorldEdit commands.", shortCut: "we", level: ChatAuthorizationLevel.Admin)] public static void WorldEdit(User user) { }
	}
}
