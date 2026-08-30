using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class CutCommand : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Copy and clean selected area", shortCut: "cut", level: ChatAuthorizationLevel.Admin)]
		public static void Cut(User user)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new CutCommand());
				if (result.Result.Success) user.Player.MsgLoc($"Cut done, {result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.");
				else user.Player.Error(result.Result.Message);
			}
			catch (Exception exception) { Log.WriteException(exception); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			WorldRange selection = context.Selection.FixToWorldSize();
			Vector3i origin = context.User.Position.Round();
			List<WorldEditBlock> blocks = new();
			List<WorldEditBlock> plants = new();
			List<WorldEditBlock> objects = new();
			BlockCaptureContext captureContext = new();

			foreach (Vector3i position in selection.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				foreach (WorldEditBlock block in context.BlockManager.Capture(position, origin, captureContext, ct))
				{
					if (block.BlockData is PlantBlockData) plants.Add(block);
					else if (block.BlockData is WorldObjectBlockData) objects.Add(block);
					else blocks.Add(block);
				}
			}

			context.UserSession.Clipboard = Clipboard.Create(
				blocks,
				plants,
				objects,
				new AuthorInformation(context.User),
				new Vector3i(selection.WidthInc, selection.HeightInc, selection.LengthInc));

			foreach (Vector3i position in selection.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);
			}
			context.BlockManager.CommitBatch(ct);
		}
	}
}
