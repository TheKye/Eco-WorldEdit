using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class GrowCommand : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Fully grows plants and trees in the selection.", shortCut: "grow", level: ChatAuthorizationLevel.Admin)]
		public static void Grow(User user)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new GrowCommand());
				if (result.Result.Success) user.Player.MsgLoc($"{result.BlocksChanged} plants grown in {result.Elapsed.TotalMilliseconds}ms.");
				else user.Player.Error(result.Result.Message);
			}
			catch (Exception exception) { Log.WriteException(exception); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			foreach (Vector3i position in context.Selection.FixToWorldSize().XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				context.BlockManager.TryGrowPlant(position, ct);
			}
		}
	}
}
