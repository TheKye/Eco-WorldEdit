﻿using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;

namespace Eco.Mods.WorldEdit
{
	public class EcoWorldEdit : IModKitPlugin, IServerPlugin, IInitializablePlugin
	{
		public const string Version = "2.0.2";
		public const string SchematicDirectoryName = "Schematics";
		public const string SchematicDefaultExtension = ".ecobp";

		public string GetStatus()
		{
			return string.Empty;
		}

		public void Initialize(TimedTask timer)
		{
			WorldEditManager.UpdateBlueprintList();
		}

		public override string ToString()
		{
			return "Eco.Mods.WorldEdit";
		}
	}
}
