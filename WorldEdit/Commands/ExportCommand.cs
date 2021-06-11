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
			if (!Directory.Exists(EcoWorldEdit.SchematicDirectoryPath)) { Directory.CreateDirectory(EcoWorldEdit.SchematicDirectoryPath); }
			if (this.UserSession.Clipboard.Count <= 0) throw new WorldEditCommandException($"Please /copy a selection first!");
		}

		protected override void Execute(WorldRange selection)
		{
			WorldEditSerializer serializer = WorldEditSerializer.FromClipboard(this.UserSession.Clipboard);
			if (this.UserSession.AuthorInfo.IsDirty()) this.UserSession.SetImportedSchematicAuthor(new AuthorInformation(this.UserSession.User));
			serializer.AuthorInformation = this.UserSession.AuthorInfo;
			using (FileStream stream = File.Create(this.fileName))
			{
				serializer.Serialize(stream);
			}
		}
	}
}
