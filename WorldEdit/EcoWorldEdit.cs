using Eco.Core.Plugins.Interfaces;

namespace Eco.Mods.WorldEdit
{
	public class EcoWorldEdit : IModKitPlugin, IServerPlugin
	{
		public const string Version = "2.0.2";
		public const string SchematicDirectoryPath = "./Mods/UserCode/WorldEdit/Schematics/";

		public string GetStatus()
		{
			return string.Empty;
		}

		public override string ToString()
		{
			return "Eco.Mods.WorldEdit";
		}
	}
}
