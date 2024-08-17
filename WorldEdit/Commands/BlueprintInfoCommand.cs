using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Shared.Logging;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands
{
    internal class BlueprintInfoCommand : WorldEditCommand
	{
		private string blueprintName;
		private string outFileName;

		public BlueprintInfoCommand(User user, string blueprint, string outFileName) : base(user)
		{
			this.blueprintName = WorldEditManager.GetSchematicFileName(blueprint);
			this.outFileName = outFileName;
		}

		protected override void Execute(WorldRange selection)
		{
			EcoBlueprint blueprint = default;
			if (!File.Exists(this.blueprintName)) throw new WorldEditCommandException($"Schematic file {this.blueprintName} not found!");
			using (FileStream stream = File.OpenRead(this.blueprintName))
			{
				blueprint = WorldEditSerializer.Deserialize<EcoBlueprint>(stream);
			}
			// Scan blueprint
			WorldRange range = blueprint.Dimension.Equals(Vector3i.Zero) ? WorldRange.Invalid : new WorldRange(Vector3i.One, blueprint.Dimension);
			Dictionary<object, long> blockTypes = new Dictionary<object, long>();
			long emptyBlocks = 0;

			List<WorldEditBlock> blocks = new List<WorldEditBlock>();
			blocks.AddRange(blueprint.Blocks);
			blocks.AddRange(blueprint.Plants);
			blocks.AddRange(blueprint.Objects);
			foreach (WorldEditBlock block in blocks)
			{
				if(blueprint.Dimension.Equals(Vector3i.Zero)) { range.ExtendToInclude(block.Position); }
				if (block.BlockType != null)
				{
					if (block.BlockType == typeof(EmptyBlock)) { emptyBlocks++; continue; }
					string name;
					if(block.IsPlantBlock())
					{
						WorldEditPlantBlockData plantData =  (WorldEditPlantBlockData)block.BlockData;
						name = BlockUtils.GetBlockFancyName(plantData.PlantType);
					}
					else if(block.IsWorldObjectBlock())
					{
						WorldEditWorldObjectBlockData objectData = (WorldEditWorldObjectBlockData)block.BlockData;
						name = BlockUtils.GetBlockFancyName(objectData.WorldObjectType);
					}
					else
					{
						name = BlockUtils.GetBlockFancyName(block.BlockType);
					}
					blockTypes.TryGetValue(name, out long count);
					blockTypes[name] = count + 1;
				}
			}
			decimal totalBlocks = blocks.Count;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(TextLoc.Header(Localizer.DoStr($"Blueprint info: {this.blueprintName}")));
			sb.Append(Localizer.DoStr("Width:").ToString().PadRight(8)).AppendLine(Text.PluralLocStr("block", "blocks", range.WidthInc));
			sb.Append(Localizer.DoStr("Height:").ToString().PadRight(8)).AppendLine(Text.PluralLocStr("block", "blocks", range.HeightInc));
			sb.Append(Localizer.DoStr("Length:").ToString().PadRight(8)).AppendLine(Text.PluralLocStr("block", "blocks", range.LengthInc));
			sb.Append(Localizer.DoStr("Volume:").ToString().PadRight(8)).AppendLine(Text.PluralLocStr("block", "blocks", range.VolumeInc));
			sb.Append(Localizer.DoStr("Area:").ToString().PadRight(8)).AppendLine(Text.PluralLocStr("block", "blocks", range.WidthInc * range.LengthInc));
			sb.AppendLocStr("Empty blocks:"); sb.AppendLine($" {emptyBlocks,8}");
			sb.AppendLocStr("Total blocks:"); sb.AppendLine($" {totalBlocks,8}");

			sb.AppendLine().AppendLine(TextLoc.Header(Localizer.DoStr("Block List")));
			foreach (KeyValuePair<object, long> entry in blockTypes)
			{
				decimal percent = Math.Round((entry.Value / totalBlocks) * 100, 2);

				sb.Append((string)entry.Key);
				sb.Append(Text.Pos(400, Text.Info(Text.Int(entry.Value))));
				sb.Append($"({percent}%)".PadLeft(10));
				sb.AppendLine();
			}
			if (!string.IsNullOrEmpty(this.outFileName))
			{
				WorldEditUtils.OutputToTxtFile(sb.ToString(), this.outFileName);
				this.UserSession.Player.MsgLoc($"Report saved into file with name <{WorldEditManager.SanitizeFileName(this.outFileName)}.txt>");
			}
			this.UserSession.Player.OpenInfoPanel(Localizer.Do($"WorldEdit Blueprint Report"), sb.ToString(), "WorldEditBInfo");

			
			Log.WriteWarningLineLoc($"Loaded dimension: {blueprint.Dimension} Scanned: {new Vector3i(range.WidthInc, range.HeightInc, range.LengthInc)}"); //TODO: !Remove debug output
		}
	}
}
