using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class SetCommand(Type BlockType) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Fills the selection with the specified block.", shortCut: "set", level: ChatAuthorizationLevel.Admin)]
		public static void Set(User user, string blockName)
		{
			try
			{
				Type blockType = BlockUtils.GetBlockType(blockName) ?? throw new WorldEditCommandException($"No Type of Block for name {blockName} found!");

				CommandResult result = CommandDispatcher.Obj.Execute(user, new SetCommand(blockType));

				if (result.Result.Success)
				{
					Logging.Success($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
					return;
				}
				else
				{
					Logging.CommandFailed("Set", result, user.Player);
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
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			BlockManager blockManager = context.BlockManager;
			WorldRange selection = context.Selection.FixToWorldSize();
			foreach (Vector3i position in selection.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				blockManager.TryScheduleBlock(BlockType, position, ct);
			}
			blockManager.CommitBatch(ct);
		}
	}
}
