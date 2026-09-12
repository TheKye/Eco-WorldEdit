using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Transformers;
using Eco.Mods.WorldEdit.Utils.Exceptions;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class RotateCommand(float Degrees) : IWorldEditCommand
	{
		public bool PreserveClipboardAnchor => true;

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Rotates the contents of your clipboard.", shortCut: "rotate", level: ChatAuthorizationLevel.Admin)]
		public static void Rotate(User user, float degrees = 90f)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new RotateCommand(degrees));
				if (result.Result.Success)
				{
					Logging.Success($"Clipboard rotated by {degrees} degrees in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				}
				else
				{
					Logging.CommandFailed("Rotate", result, user.Player);
				}
			}
			catch (WorldEditCommandException exception)
			{
				Logging.ErrorLocStr(exception.Message, user.Player);
			}
			catch (Exception exception)
			{
				Logging.Exception(exception, user.Player);
			}
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			Clipboard source = context.UserSession.Clipboard;
			if (source.Count <= 0) throw new WorldEditCommandException("Please /copy a selection or /import blueprint first!");
			Clipboard rotated = ClipboardTransformer.Rotate(source, Degrees, ct);
			ct.ThrowIfCancellationRequested();
			context.UserSession.SetClipboard(rotated, refreshAnchor: true);
		}
	}
}
