using Eco.Core.Plugins.Interfaces;

namespace Eco.Mods.WorldEdit
{
	public class EcoWorldEdit : IModKitPlugin, IServerPlugin
	{
		public const string SchematicDirectoryPath = "./Schematics/";

		public string GetStatus()
		{
			return "";
		}

		public override string ToString()
		{
			return "Eco.Mods.WorldEdit";
		}
	}
}
