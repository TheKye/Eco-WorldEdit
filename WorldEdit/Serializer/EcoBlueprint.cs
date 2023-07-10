using System.Collections.Generic;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Serializer
{
    internal struct EcoBlueprint
	{
		public float Version { get; private set; }
		public string EcoVersion { get; private set; }
		public AuthorInformation Author { get; private set; }
		/// <summary>Dimension in Width, Height, Length. Zero vector if not provided.</summary>
		[JsonConverter(typeof(JsonVector3iConverter))] public Vector3i Dimension { get; private set; }
		public List<WorldEditBlock> Blocks { get; private set; }
		public List<WorldEditBlock> Plants { get; private set; }
		public List<WorldEditBlock> Objects { get; private set; }

		public static EcoBlueprint Create(List<WorldEditBlock> blocks, List<WorldEditBlock> plants, List<WorldEditBlock> worldObjects, AuthorInformation author, Vector3i dim)
		{
			EcoBlueprint schematic = new EcoBlueprint();
			schematic.Version = WorldEditSerializer.CurrentVersion;
			schematic.EcoVersion = Shared.EcoVersion.VersionNumber;
			schematic.Blocks = blocks;
			schematic.Plants = plants;
			schematic.Objects = worldObjects;
			schematic.Author = author;
			schematic.Dimension = dim;
			return schematic;
		}

		[JsonConstructor]
		public EcoBlueprint(float version, string ecoVersion, AuthorInformation author, List<WorldEditBlock> blocks, List<WorldEditBlock> plants, List<WorldEditBlock> objects, Vector3i dimension)
		{
			this.Version = version;
			this.EcoVersion = ecoVersion;
			this.Blocks = blocks;
			this.Plants = plants;
			this.Objects = objects;
			this.Author = author ?? AuthorInformation.Unowned();
			this.Dimension = dimension;
		}
	}
}
