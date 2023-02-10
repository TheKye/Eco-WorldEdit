using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;

namespace Eco.Mods.WorldEdit
{
	public class EcoWorldEdit : IModKitPlugin, IServerPlugin, IInitializablePlugin
	{
		public const string Version = "2.5.2";
		public const string SchematicDirectoryName = "Blueprints";
		public const string SchematicDefaultExtension = ".ecobp";

		public void Initialize(TimedTask timer)
		{
			WorldEditManager.UpdateBlueprintList();
		}

		public string GetCategory()
		{
			return string.Empty;
		}

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
