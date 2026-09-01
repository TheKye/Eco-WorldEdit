using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Graphics;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class ColorCommand(ByteColor Color) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Paints blocks in the selection with the specified color.", shortCut: "color", level: ChatAuthorizationLevel.Admin)]
		public static void ColorBlocks(User user, string color)
		{
			try
			{
				string normalized = color.Trim().Replace(" ", ",");
				string[] parts = normalized.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				ByteColor parsedColor;
				if (normalized.Equals(nameof(ByteColor.Clear), StringComparison.OrdinalIgnoreCase)) parsedColor = ByteColor.Clear;
				else if (parts.Length == 1 && parts[0].StartsWith('#')) parsedColor = ByteColor.FromHex(parts[0]);
				else if (parts.Length is 1 or 2 && Enum.TryParse(parts[0], true, out NamedColors named))
				{
					parsedColor = named.GetNamedColor();
					if (parts.Length == 2)
					{
						if (!byte.TryParse(parts[1], out byte alpha)) throw new WorldEditCommandException("Incorrect alpha value.");
						parsedColor = parsedColor.WithAlpha(alpha);
					}
				}
				else if (parts.Length is 3 or 4 && byte.TryParse(parts[0], out byte r) && byte.TryParse(parts[1], out byte g) && byte.TryParse(parts[2], out byte b))
				{
					byte alpha = 255;
					if (parts.Length == 4 && !byte.TryParse(parts[3], out alpha)) throw new WorldEditCommandException("Incorrect alpha value.");
					parsedColor = new ByteColor(r, g, b, alpha);
				}
				else throw new WorldEditCommandException("Incorrect color name, HEX or RGB/RGBA value.");

				CommandResult result = CommandDispatcher.Obj.Execute(user, new ColorCommand(parsedColor));
				if (result.Result.Success) user.Player.MsgLoc($"{result.BlocksChanged} blocks painted in {result.Elapsed.TotalMilliseconds}ms.");
				else user.Player.Error(result.Result.Message);
			}
			catch (WorldEditCommandException exception) { user.Player.ErrorLocStr(exception.Message); }
			catch (Exception exception) { Log.WriteException(exception); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			foreach (Vector3i position in context.Selection.FixToWorldSize().XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				context.BlockManager.TrySetColor(position, Color, ct);
			}
		}
	}
}
