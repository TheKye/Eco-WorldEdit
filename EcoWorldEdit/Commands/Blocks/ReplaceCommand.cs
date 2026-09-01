using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class ReplaceCommand(Type FindType, Type ReplaceType, bool ReplaceAllNonEmpty) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Replaces one block type with another in the selection.", shortCut: "replace", level: ChatAuthorizationLevel.Admin)]
		public static void Replace(User user, string findType, string replaceType = "")
		{
			try
			{
				Type find = BlockUtils.GetBlockType(findType) ?? throw new WorldEditCommandException($"No BlockType with name {findType} found!");
				bool replaceAll = string.IsNullOrWhiteSpace(replaceType);
				Type replacement = replaceAll ? find : BlockUtils.GetBlockType(replaceType) ?? throw new WorldEditCommandException($"No BlockType with name {replaceType} found!");
				CommandResult result = CommandDispatcher.Obj.Execute(user, new ReplaceCommand(find, replacement, replaceAll));
				if (result.Result.Success) user.Player.MsgLoc($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.");
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
				Type current = Eco.World.World.GetBlock(position).GetType();
				if (ReplaceAllNonEmpty ? current != typeof(EmptyBlock) : current == FindType)
					context.BlockManager.TryScheduleBlock(ReplaceType, position, ct);
			}
			context.BlockManager.CommitBatch(ct);
		}
	}
}
