using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Transformers;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class RotateCommand(float Degrees) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Rotates the contents of your clipboard.", shortCut: "rotate", level: ChatAuthorizationLevel.Admin)]
		public static void Rotate(User user, float degrees = 90f)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new RotateCommand(degrees));
				if (result.Result.Success)
				{
					user.Player.MsgLoc($"Clipboard rotated by {degrees} degrees in {result.Elapsed.TotalMilliseconds}ms.");
				}
				else
				{
					user.Player.Error(result.Result.Message);
				}
			}
			catch (WorldEditCommandException exception)
			{
				user.Player.ErrorLocStr(exception.Message);
			}
			catch (Exception exception)
			{
				Log.WriteException(exception);
			}
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			Clipboard source = context.UserSession.Clipboard;
			if (source.Count <= 0) throw new WorldEditCommandException("Please /copy a selection or /import blueprint first!");
			Clipboard rotated = ClipboardTransformer.Rotate(source, Degrees, ct);
			ct.ThrowIfCancellationRequested();
			context.UserSession.Clipboard = rotated;
		}
	}
}
