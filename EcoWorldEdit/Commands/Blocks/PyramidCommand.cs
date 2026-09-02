using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class PyramidCommand(int Height, Type BlockType, PyramidCommand.PyramidStyle Style, bool Clear, Vector3i Origin) : IWorldEditCommand
	{
		public enum PyramidStyle { Full, Thin, Thick, Hollow }

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Builds a pyramid at your position.", shortCut: "pyramid", level: ChatAuthorizationLevel.Admin)]
		public static void Pyramid(User user, int height, string? blockType = null, string styleStr = "full", bool clear = true)
		{
			try
			{
				string requestedType = blockType ?? nameof(DirtBlock);
				Type type = BlockUtils.GetBlockType(requestedType) ?? throw new WorldEditCommandException($"No BlockType with name {requestedType} found!");
				if (!Enum.TryParse(styleStr, true, out PyramidStyle style)) style = PyramidStyle.Full;
				PyramidCommand command = new(Math.Max(1, height), type, style, clear, CommandParsing.GetPosition(user));
				CommandResult result = CommandDispatcher.Obj.Execute(user, command);
				if (result.Result.Success) Logging.Success($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				else Logging.Error(result.Result.Message, user.Player);
			}
			catch (WorldEditCommandException exception) { Logging.ErrorLocStr(exception.Message, user.Player); }
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (Clear)
			{
				WorldRange clearArea = WorldRange.SurroundingSpace(Origin, Height).FixToWorldSize();
				clearArea.min.y = Origin.y;
				foreach (Vector3i position in clearArea.XYZIterInc())
				{
					ct.ThrowIfCancellationRequested();
					context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);
				}
			}

			int top = Origin.y + Height - 1;
			for (int y = top; y >= Origin.y; y--)
			{
				ct.ThrowIfCancellationRequested();
				int radius = top - y;
				WorldRange layer = WorldRange.SurroundingSpace(Origin, radius).FixToWorldSize();
				layer.min.y = layer.max.y = y;
				IEnumerable<Vector3i> positions = Style is PyramidStyle.Thin or PyramidStyle.Thick or PyramidStyle.Hollow ? layer.SidesIterator() : layer.XYZIterInc();
				foreach (Vector3i position in positions) context.BlockManager.TryScheduleBlock(BlockType, position, ct);

				if (Style == PyramidStyle.Thick && radius > 0)
				{
					WorldRange inner = WorldRange.SurroundingSpace(Origin, radius - 1).FixToWorldSize();
					inner.min.y = inner.max.y = y;
					foreach (Vector3i position in inner.SidesIterator()) context.BlockManager.TryScheduleBlock(BlockType, position, ct);
				}
			}
			context.BlockManager.CommitBatch(ct);
		}
	}
}
