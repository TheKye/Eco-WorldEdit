using System.Collections.Generic;
using Eco.Mods.WorldEdit.Model;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal struct EcoBlueprint
	{
		public float Version { get; private set; }
		public string EcoVersion { get; private set; }
		public List<WorldEditBlock> Blocks { get; private set; }
		public List<WorldEditBlock> Plants { get; private set; }
		public List<WorldEditBlock> Objects { get; private set; }

		public static EcoBlueprint Create(List<WorldEditBlock> blocks, List<WorldEditBlock> plants, List<WorldEditBlock> worldObjects)
		{
			EcoBlueprint schematic = new EcoBlueprint();
			schematic.Version = WorldEditSerializer.currentVersion;
			schematic.EcoVersion = Shared.EcoVersion.VersionNumber;
			schematic.Blocks = blocks;
			schematic.Plants = plants;
			schematic.Objects = worldObjects;
			return schematic;
		}

		[JsonConstructor]
		public EcoBlueprint(float version, string ecoVersion, List<WorldEditBlock> blocks, List<WorldEditBlock> plants, List<WorldEditBlock> objects)
		{
			this.Version = version;
			this.EcoVersion = ecoVersion;
			this.Blocks = blocks;
			this.Plants = plants;
			this.Objects = objects;
		}
	}
}
