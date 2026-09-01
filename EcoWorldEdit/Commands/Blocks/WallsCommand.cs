using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class WallsCommand(Type BlockType) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Builds outer walls around the selection.", shortCut: "walls", level: ChatAuthorizationLevel.Admin)]
		public static void Walls(User user, string typeName)
		{
			try
			{
				Type blockType = BlockUtils.GetBlockType(typeName) ?? throw new WorldEditCommandException($"No BlockType with name {typeName} found!");
				CommandResult result = CommandDispatcher.Obj.Execute(user, new WallsCommand(blockType));
				if (result.Result.Success) user.Player.MsgLoc($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.");
				else user.Player.Error(result.Result.Message);
			}
			catch (WorldEditCommandException exception) { user.Player.ErrorLocStr(exception.Message); }
			catch (Exception exception) { Log.WriteException(exception); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			foreach (Vector3i position in context.Selection.FixToWorldSize().SidesIterator())
			{
				ct.ThrowIfCancellationRequested();
				context.BlockManager.TryScheduleBlock(BlockType, position, ct);
			}
			context.BlockManager.CommitBatch(ct);
		}
	}
}
