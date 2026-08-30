using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Logging;

namespace Eco.Mods.WorldEdit.Commands.General
{
	[ChatCommandHandler]
	internal sealed class WandCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Gives the player a Wand for using world edit", shortCut: "wand", level: ChatAuthorizationLevel.Admin)]
		public static void Wand(User user)
		{
			try
			{
				user.Inventory.AddItems(WandToolItem.GetWandItemStack());
			}
			catch (Exception e) { Log.WriteException(e); }
		}

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Removes the wand from the players inventory", shortCut: "rmwand", level: ChatAuthorizationLevel.Admin)]
		public static void RmWand(User user)
		{
			try
			{
				user.Inventory.TryRemoveItems(WandToolItem.GetWandItemStack());
			}
			catch (Exception e) { Log.WriteException(e); }
		}
	}
}
