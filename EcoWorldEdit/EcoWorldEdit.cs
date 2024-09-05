using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Logging;
using System.Reflection;

namespace Eco.Mods.WorldEdit
{
	public class EcoWorldEdit : IModKitPlugin, IServerPlugin, IInitializablePlugin, IModInit
    {
		public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		public const string SchematicDirectoryName = "Blueprints";
		public const string SchematicDefaultExtension = ".ecobp";

		public EcoWorldEdit() { Log.WriteLine(Localizer.Do($"Using WorldEdit version {Version}")); }
		
		public static ModRegistration Register() => new()
        {
            ModName = "EcoWorldEdit",
            ModDescription = "WorldEdit is the ultimate map editor to get creative, do not engage in routine.",
            ModDisplayName = "WorldEdit",
        };

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
