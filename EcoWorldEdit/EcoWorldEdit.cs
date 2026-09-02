using System.Reflection;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Mods.WorldEdit.Core.Managers;

namespace Eco.Mods.WorldEdit
{
	public class EcoWorldEdit : IModKitPlugin, IServerPlugin, IInitializablePlugin, IModInit
	{
		public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
		public const string SchematicDirectoryName = "Blueprints";
		public const string SchematicDefaultExtension = ".ecobp";

		public EcoWorldEdit() { Logging.Info($"Using WorldEdit version {Version}"); }

		public static ModRegistration Register() => new()
		{
			ModName = "EcoWorldEdit",
			ModDescription = "WorldEdit is the ultimate map editor to get creative, do not engage in routine.",
			ModDisplayName = "WorldEdit",
		};

		void IInitializablePlugin.Initialize(TimedTask timer)
		{
			WorldEditManager.Obj.Initialize();
			MigrationManager.Obj.Initialize();
			WorldEditManager.Obj.UpdateBlueprintList();
		}

		public string GetCategory() => string.Empty;
		public string GetStatus() => string.Empty;
		public override string ToString() => "Eco.Mods.WorldEdit";
	}
}
