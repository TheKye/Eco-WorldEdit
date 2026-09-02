using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;
using Eco.World.Blocks;
using Eco.WorldGenerator;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class FixWaterCommand(int Height) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Restores the specified water level within the selection.", shortCut: "fixwater", level: ChatAuthorizationLevel.Admin)]
		public static void FixWater(User user, int height = 0)
		{
			try
			{
				int waterLevel = height == 0 ? WorldGeneratorPlugin.Settings.WaterLevel : height;
				CommandResult result = CommandDispatcher.Obj.Execute(user, new FixWaterCommand(waterLevel));
				if (result.Result.Success) Logging.Success($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				else Logging.Error(result.Result.Message, user.Player);
			}
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			foreach (Vector3i position in context.Selection.FixToWorldSize().XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				Block block = Eco.World.World.GetBlock(position);
				if (position.y <= Height && block is EmptyBlock) context.BlockManager.TryScheduleBlock(typeof(WaterBlock), position, ct);
				else if (position.y > Height && block is IWaterBlock) context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);
			}
			context.BlockManager.CommitBatch(ct);
		}
	}
}
