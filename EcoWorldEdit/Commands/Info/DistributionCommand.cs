using System.Text;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Info
{
	[ChatCommandHandler]
	internal sealed class DistributionCommand(bool Detailed, string? OutputFile) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "distr will give you a detailed list of all items in your selected area", shortCut: "distr", level: ChatAuthorizationLevel.Admin)]
		public static void Distribution(User user, string type = "brief", string? fileName = null)
		{
			try
			{
				bool detailed = type.Trim().StartsWith("d", StringComparison.OrdinalIgnoreCase);
				CommandResult result = CommandDispatcher.Obj.Execute(user, new DistributionCommand(detailed, fileName));
				if (result.Result.Failed) user.Player.Error(result.Result.Message);
			}
			catch (Exception exception) { Log.WriteException(exception); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!context.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			WorldRange selection = context.Selection.FixToWorldSize();
			Dictionary<Type, long> counts = new();
			long empty = 0;
			BlockCaptureContext captureContext = new();

			foreach (Vector3i position in selection.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				foreach (WorldEditBlock block in context.BlockManager.Capture(position, System.Numerics.Vector3.Zero, captureContext, ct))
				{
					Type type = block.BlockData switch
					{
						PlantBlockData plant => plant.PlantType,
						WorldObjectBlockData worldObject => worldObject.WorldObjectType,
						_ => block.BlockType,
					};
					if (type == typeof(EmptyBlock)) empty++;
					else counts[type] = counts.GetValueOrDefault(type) + 1;
				}
			}

			long total = counts.Values.Sum();
			StringBuilder report = new();
			report.AppendLine("Selection Info").AppendLine($"Region: {selection.min} - {selection.max}");
			report.AppendLine($"Width: {selection.WidthInc}").AppendLine($"Height: {selection.HeightInc}").AppendLine($"Length: {selection.LengthInc}");
			report.AppendLine($"Volume: {selection.VolumeInc}").AppendLine($"Empty blocks: {empty}").AppendLine($"Total blocks: {total}").AppendLine();
			foreach ((Type type, long count) in counts.OrderByDescending(pair => pair.Value))
			{
				double percent = total == 0 ? 0 : count * 100d / total;
				report.Append(type.Name).Append(": ").Append(count).Append(" (").Append(percent.ToString("0.##")).Append("%)");
				if (Detailed) report.Append(" [").Append(type.FullName).Append(']');
				report.AppendLine();
			}

			if (!string.IsNullOrWhiteSpace(OutputFile))
			{
				Directory.CreateDirectory(SchematicUtils.GetSchematicDirectory());
				string safeName = SchematicUtils.SanitizeFileName(OutputFile);
				File.WriteAllText(Path.Combine(SchematicUtils.GetSchematicDirectory(), safeName + ".txt"), report.ToString());
				context.Player.MsgLoc($"Report saved into file with name <{safeName}.txt>");
			}
			context.Player.OpenInfoPanel("WorldEdit Blocks Report", report.ToString(), "WorldEditDistr");
		}
	}
}
