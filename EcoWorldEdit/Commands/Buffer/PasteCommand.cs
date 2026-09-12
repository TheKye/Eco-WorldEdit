using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class PasteCommand(bool SkipEmpty) : IWorldEditCommand
	{
		public bool PreserveClipboardAnchor => true;

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Pastes the clipboard at its active anchor or your position.", shortCut: "paste", level: ChatAuthorizationLevel.Admin)]
		public static void Paste(User user, bool skipEmpty = false)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new PasteCommand(skipEmpty));

				if (result.Result.Success)
				{
					Logging.Success($"Paste done, {result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
					return;
				}
				else
				{
					Logging.CommandFailed("Paste", result, user.Player);
				}
			}
			catch (WorldEditCommandException e)
			{
				Logging.ErrorLocStr(e.Message, user.Player);
			}
			catch (Exception e) { Logging.Exception(e, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			Clipboard clipboard = context.UserSession.Clipboard;
			if (clipboard.Count <= 0) throw new WorldEditCommandException($"Please /copy a selection or /import blueprint first!");
			Vector3i pasteOrigin = context.UserSession.IsClipboardAnchored ? context.Selection.min : context.User.Position.Round();
			BlockManager blockManager = context.BlockManager;
			if (blockManager.GetPlacementHeightRange(clipboard.Blocks, clipboard.Plants, clipboard.WorldObjects, SkipEmpty, ct) is { } localRange)
			{
				PlacementHeightRange targetRange = localRange.Offset(pasteOrigin.Y);
				if (!targetRange.FitsWorld()) throw new WorldEditCommandException($"Cannot paste clipboard at height {targetRange.MinY}..{targetRange.MaxY}. Valid world height is {WorldHeight.Min}..{WorldHeight.Max}.");
			}
			blockManager.Restore(clipboard.Blocks, clipboard.Plants, clipboard.WorldObjects, pasteOrigin, SkipEmpty, ct);
		}
	}
}
