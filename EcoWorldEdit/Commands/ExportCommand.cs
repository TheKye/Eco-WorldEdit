using System.IO;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class ExportCommand : WorldEditCommand
	{
		private readonly string fileName;

		public ExportCommand(User user, string fileName) : base(user)
		{
			this.fileName = WorldEditManager.GetSchematicFileName(fileName);
			if (!Directory.Exists(WorldEditManager.GetSchematicDirectory())) { Directory.CreateDirectory(WorldEditManager.GetSchematicDirectory()); }
			if (this.UserSession.Clipboard.Count <= 0) throw new WorldEditCommandException($"Please /copy a selection first!");
		}

		protected override void Execute(WorldRange selection)
		{
			WorldEditSerializer serializer = WorldEditSerializer.FromClipboard(this.UserSession.Clipboard);
			using (FileStream stream = File.Create(this.fileName))
			{
				serializer.Serialize(stream);
			}
		}
	}
}
