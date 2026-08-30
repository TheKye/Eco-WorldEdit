using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class PasteCommand(bool SkipEmpty) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "paste will paste the copied selection or imported schematic from where the player is standing", shortCut: "paste", level: ChatAuthorizationLevel.Admin)]
		public static void Paste(User user, bool skipEmpty = false)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new PasteCommand(skipEmpty));

				if (result.Result.Success)
				{
					user.Player.MsgLoc($"Paste done, {result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.");
					return;
				}
				else
				{
					user.Player.Error(result.Result.Message);
				}
			}
			catch (WorldEditCommandException e)
			{
				user.Player.ErrorLocStr(e.Message);
			}
			catch (Exception e) { Log.WriteException(e); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			Clipboard clipboard = context.UserSession.Clipboard;
			if (clipboard.Count <= 0) throw new WorldEditCommandException($"Please /copy a selection or /import blueprint first!");
			Vector3i playerPos = context.User.Position.Round();
			BlockManager blockManager = context.BlockManager;
			blockManager.Restore(clipboard.Blocks, clipboard.Plants, clipboard.WorldObjects, playerPos, SkipEmpty, ct);
		}
	}
}
