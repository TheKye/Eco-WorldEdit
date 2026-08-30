using System.Text;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Info
{
	[ChatCommandHandler]
	internal static class BlueprintInfoCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), "BInfo will give you information about blueprint", "binfo", ChatAuthorizationLevel.Admin)]
		public static void BlueprintInfo(User user, string fileName, string? outFileName = null)
		{
			try
			{
				string path = SchematicUtils.GetSchematicFilePath(fileName);
				if (!File.Exists(path)) throw new WorldEditCommandException($"Schematic file {path} not found!");
				EcoBlueprint blueprint = new WorldEditSerializer().Deserialize(path);
				Dictionary<Type, long> counts = new();
				long empty = 0;
				foreach (WorldEditBlock block in blueprint.Blocks.Concat(blueprint.Plants).Concat(blueprint.Objects))
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

				Vector3i dimension = blueprint.Dimension;
				WorldRange range = new(
					Vector3i.Zero,
					new Vector3i(Math.Max(0, dimension.x - 1), Math.Max(0, dimension.y - 1), Math.Max(0, dimension.z - 1)));
				long total = counts.Values.Sum();
				StringBuilder report = new();
				report.AppendLine($"Blueprint info: {fileName}").AppendLine($"Region: {range.min} - {range.max}");
				report.AppendLine($"Width: {range.WidthInc}").AppendLine($"Height: {range.HeightInc}").AppendLine($"Length: {range.LengthInc}");
				report.AppendLine($"Volume: {range.VolumeInc}").AppendLine($"Empty blocks: {empty}").AppendLine($"Total blocks: {total}").AppendLine();
				foreach ((Type type, long count) in counts.OrderByDescending(pair => pair.Value))
				{
					double percent = total == 0 ? 0 : count * 100d / total;
					report.Append(type.Name).Append(": ").Append(count).Append(" (").Append(percent.ToString("0.##")).AppendLine("%)");
				}

				if (!string.IsNullOrWhiteSpace(outFileName))
				{
					Directory.CreateDirectory(SchematicUtils.GetSchematicDirectory());
					string safeName = SchematicUtils.SanitizeFileName(outFileName);
					File.WriteAllText(Path.Combine(SchematicUtils.GetSchematicDirectory(), safeName + ".txt"), report.ToString());
					user.Player.MsgLoc($"Report saved into file with name <{safeName}.txt>");
				}
				user.Player.OpenInfoPanel("WorldEdit Blueprint Report", report.ToString(), "WorldEditBInfo");
			}
			catch (WorldEditCommandException exception) { user.Player.ErrorLocStr(exception.Message); }
			catch (Exception exception) { Log.WriteException(exception); }
		}
	}
}
