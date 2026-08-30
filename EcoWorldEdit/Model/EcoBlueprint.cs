using System.Text;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Shared.Math;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model
{
	internal record EcoBlueprint : EcoBlueprintInfo
	{
		public List<WorldEditBlock> Blocks { get; private set; }
		public List<WorldEditBlock> Plants { get; private set; }
		public List<WorldEditBlock> Objects { get; private set; }

		public static EcoBlueprint Create(List<WorldEditBlock> blocks, List<WorldEditBlock> plants, List<WorldEditBlock> worldObjects, AuthorInformation author, Vector3i dimension)
		{
			return new EcoBlueprint(
				WorldEditSerializer.CurrentVersion,
				WorldEditSerializer.CurrentEcoVersion,
				author,
				blocks,
				plants,
				worldObjects,
				dimension
			);
		}

		[JsonConstructor]
		public EcoBlueprint(Version version, string ecoVersion, AuthorInformation? author, List<WorldEditBlock> blocks, List<WorldEditBlock> plants, List<WorldEditBlock> objects, Vector3i dimension) :
		base(version, ecoVersion, author ?? AuthorInformation.Unowned(), dimension)
		{
			this.Blocks = blocks;
			this.Plants = plants;
			this.Objects = objects;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Version: {this.Version}");
			sb.AppendLine($"EcoVersion: {this.EcoVersion}");
			sb.AppendLine($"Dimension: {this.Dimension}");
			sb.AppendLine("Author:");
			sb.AppendLine(this.Author.ToString());
			sb.AppendLine($"Blocks: {this.Blocks.Count}");
			foreach (WorldEditBlock worldEditBlock in this.Blocks)
			{
				sb.AppendLine(worldEditBlock.ToString());
			}
			sb.AppendLine($"Plants: {this.Plants.Count}");
			foreach (WorldEditBlock worldEditBlock in this.Plants)
			{
				sb.AppendLine(worldEditBlock.ToString());
			}
			sb.AppendLine($"Objects: {this.Objects.Count}");
			foreach (WorldEditBlock worldEditBlock in this.Objects)
			{
				sb.AppendLine(worldEditBlock.ToString());
			}
			return sb.ToString();
		}
	}
}
