using System.IO;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Serializer;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class ExportCommand : WorldEditCommand
	{
		private readonly string fileName;

		public ExportCommand(User user, string fileName) : base(user)
		{
			this.fileName = WorldEditManager.GetSchematicFileName(fileName);
			if (!Directory.Exists(EcoWorldEdit.SchematicDirectoryPath)) { Directory.CreateDirectory(EcoWorldEdit.SchematicDirectoryPath); }
			if (this.UserSession.Clipboard.Count <= 0) throw new WorldEditCommandException($"Please /copy a selection first!");
		}

		protected override void Execute()
		{
			WorldEditSerializer serializer = WorldEditSerializer.FromClipboard(this.UserSession.Clipboard);
			using (MemoryStream stream = serializer.Serialize())
			{
				File.WriteAllBytes(this.fileName, stream.ToArray());
			}
		}
	}
}
