using Eco.Mods.WorldEdit.Model;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Migrations
{
	/// <summary>Adds blueprint dimensions. Also repairs early 1.2 files created before Author was added.</summary>
	internal sealed class BlueprintMigrationTo1_3 : IBlueprintVersionMigration
	{
		public Version TargetVersion { get; } = new(1, 3);
		public int Order => 0;

		public void Migrate(JObject root, MigrationInfo info)
		{
			BlueprintMigrationJson.RequireSourceVersion(root, new Version(1, 2));
			JObject[] blocks = BlueprintMigrationJson.Blocks(root).ToArray();
			BlueprintMigrationJson.EnsureAuthor(root);

			JToken? dimensionToken = root[nameof(EcoBlueprint.Dimension)];
			if (dimensionToken is null or { Type: JTokenType.Null } || HasMissingDimension(dimensionToken, root))
			{
				root[nameof(EcoBlueprint.Dimension)] = CalculateDimension(blocks);
			}

			BlueprintMigrationJson.SetVersion(root, this.TargetVersion);
		}

		private static bool HasMissingDimension(JToken dimensionToken, JObject root)
		{
			float[] dimension = BlueprintMigrationJson.RequireVector3(dimensionToken, "Blueprint Dimension", root);
			return dimension.Any(coordinate => coordinate <= 0);
		}

		private static JArray CalculateDimension(IEnumerable<JObject> blocks)
		{
			int[] min = [int.MaxValue, int.MaxValue, int.MaxValue];
			int[] max = [int.MinValue, int.MinValue, int.MinValue];
			bool any = false;
			foreach (JObject block in blocks)
			{
				float[] position = BlueprintMigrationJson.RequireVector3(block["Position"], "Block Position", block);
				for (int i = 0; i < position.Length; i++)
				{
					if (position[i] != MathF.Truncate(position[i]) || position[i] < int.MinValue || position[i] > int.MaxValue)
					{
						throw BlueprintMigrationJson.Error(block["Position"]!, "Version 1.2 block positions must contain integer coordinates");
					}
					int coordinate = (int)position[i];
					min[i] = Math.Min(min[i], coordinate);
					max[i] = Math.Max(max[i], coordinate);
				}
				any = true;
			}

			if (!any) return new JArray(0, 0, 0);
			return new JArray(
				checked(max[0] - min[0] + 1),
				checked(max[1] - min[1] + 1),
				checked(max[2] - min[2] + 1));
		}
	}
}
