using System.Text;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Commands.Info
{
	internal static class BlueprintListCommand
	{
		public static void Print(User user)
		{
			WorldEditManager.Obj.UpdateBlueprintList();
			IEnumerable<EcoBlueprintInfo> blueprintList = WorldEditManager.Obj.BlueprintList.Values;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(MakeRow(
				Text.Header(Localizer.DoStr("File name")),
				Text.Header(Localizer.DoStr("Version")),
				Text.Header(Localizer.DoStr("File size")),
				Text.Header(Localizer.DoStr("Date")),
				Text.Header(Localizer.DoStr("Player"))
			));

			foreach (EcoBlueprintInfo info in blueprintList.OrderBy(x => x.FileName))
			{
				sb.AppendLine(MakeRow(
					Path.GetFileNameWithoutExtension(info.FileName),
					$"ECO {info.EcoVersion} ({info.Version.ToString(2)})",
					(info.FileSize / 1024).ToString() + " KB",
					info.FileChangedDate.ToString(),
					info.Author.Name
				));
			}

			user.Player.OpenInfoPanel(Localizer.Do($"WorldEdit Blueprint List"), sb.ToString(), "WorldEditBpList");
		}

		private static string MakeRow(string fileName, string version, string size, string date, string player)
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
