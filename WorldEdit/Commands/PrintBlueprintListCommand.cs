using System.Globalization;
using System.Linq;
using System.Text;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class PrintBlueprintListCommand : WorldEditCommand
	{
		public PrintBlueprintListCommand(User user) : base(user) { }

		protected override void Execute()
		{
			WorldEditManager.UpdateBlueprintList();

			StringBuilder sb = new StringBuilder();

			sb.Append(Localizer.DoStr("File name"))
			.Append(Text.Pos(300, Localizer.DoStr("Version")))
			.Append(Text.Pos(500, Localizer.DoStr("File size")))
			.Append(Text.Pos(700, Localizer.DoStr("Date")))
			.AppendLine(Text.Pos(950, Localizer.DoStr("Player")));

			foreach (EcoBlueprintInfo info in WorldEditManager.BlueprintList.Values.OrderBy(k => k.FileName))
			{
				sb.Append(info.FileName)
				.Append(Text.Pos(300, $"ECO {info.EcoVersion} ({info.Version.ToString("0.00", CultureInfo.InvariantCulture)})"))
				.Append(Text.Pos(500, (info.FileSize / 1024).ToString() + " KB"))
				.Append(Text.Pos(700, info.FileCreatedDate.ToString()))
				.AppendLine(Text.Pos(950, info.Author.Name));
			}

			this.UserSession.Player.OpenInfoPanel(Localizer.Do($"WorldEdit Blueprint List"), sb.ToString(), "WorldEditBpList");
		}
	}
}
