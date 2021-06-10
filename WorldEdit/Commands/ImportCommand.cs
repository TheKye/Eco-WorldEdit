using System.IO;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Serializer;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class ImportCommand : WorldEditCommand
	{
		private readonly string fileName;

		public ImportCommand(User user, string fileName) : base(user)
		{
			this.fileName = WorldEditManager.GetSchematicFileName(fileName);
		}

		protected override void Execute()
		{
			if (!File.Exists(this.fileName)) throw new WorldEditCommandException($"Schematic file {fileName} not found!");
			using (FileStream stream = File.OpenRead(this.fileName))
			{
				WorldEditSerializer serializer = new WorldEditSerializer();
				serializer.Deserialize(stream);
				this.UserSession.Clipboard.Parse(serializer);
				this.UserSession.SetImportedSchematicAuthor(serializer.AuthorInformation);
			}
		}
	}
}
