using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class PrintBlueprintListCommand : WorldEditCommand
	{
		public PrintBlueprintListCommand(User user) : base(user) { }

		protected override void Execute(WorldRange selection)
		{
			WorldEditManager.UpdateBlueprintList();

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(this.MakeRow(
				Text.Header(Localizer.DoStr("File name")),
				Text.Header(Localizer.DoStr("Version")),
				Text.Header(Localizer.DoStr("File size")),
				Text.Header(Localizer.DoStr("Date")),
				Text.Header(Localizer.DoStr("Player"))
			));

			//sb.Append(Localizer.DoStr("File name"))
			//.Append(Text.Pos(300, Localizer.DoStr("Version")))
			//.Append(Text.Pos(500, Localizer.DoStr("File size")))
			//.Append(Text.Pos(700, Localizer.DoStr("Date")))
			//.AppendLine(Text.Pos(950, Localizer.DoStr("Player")));

			foreach (EcoBlueprintInfo info in WorldEditManager.BlueprintList.Values.OrderBy(k => k.FileName))
			{
				sb.AppendLine(this.MakeRow(
					info.FileName,
					$"ECO {info.EcoVersion} ({info.Version.ToString("0.00", CultureInfo.InvariantCulture)})",
					(info.FileSize / 1024).ToString() + " KB",
					info.FileChangedDate.ToString(),
					info.Author.Name
				));

				//sb.Append(info.FileName)
				//.Append(Text.Pos(300, $"ECO {info.EcoVersion} ({info.Version.ToString("0.00", CultureInfo.InvariantCulture)})"))
				//.Append(Text.Pos(500, (info.FileSize / 1024).ToString() + " KB"))
				//.Append(Text.Pos(700, info.FileChangedDate.ToString()))
				//.AppendLine(Text.Pos(950, info.Author.Name));
			}

			this.UserSession.Player.OpenInfoPanel(Localizer.Do($"WorldEdit Blueprint List"), sb.ToString(), "WorldEditBpList");
		}

		private string MakeRow(string fileName, string version, string size, string date, string player)
		{
			(string, int)[] row = {
				(fileName, 22),
				(version, 10),
				(size, 8),
				(date, 12),
				(player, 20)
			};
			return Text.Columns(2, 18, row);
		}
	}
}
