using Eco.Mods.WorldEdit.Model;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Migrations
{
	/// <summary>Normalizes legacy player-relative positions to a minimum-corner clipboard origin.</summary>
	internal sealed class BlueprintMigrationTo1_5 : IBlueprintVersionMigration
	{
		public Version TargetVersion { get; } = new(1, 5);
		public int Order => 0;

		public void Migrate(JObject root, MigrationInfo info)
		{
			BlueprintMigrationJson.RequireSourceVersion(root, new Version(1, 4));
			JObject[] blocks = BlueprintMigrationJson.Blocks(root).ToArray();
			if (blocks.Length == 0)
			{
				BlueprintMigrationJson.SetVersion(root, this.TargetVersion);
				return;
			}

			float[] legacyDimension = BlueprintMigrationJson.RequireVector3(root[nameof(EcoBlueprint.Dimension)], "Blueprint Dimension", root);
			int[] dimension = new int[3];
			for (int axis = 0; axis < dimension.Length; axis++)
			{
				if (legacyDimension[axis] != MathF.Truncate(legacyDimension[axis]) || legacyDimension[axis] < 0 || legacyDimension[axis] > int.MaxValue) throw BlueprintMigrationJson.Error(root[nameof(EcoBlueprint.Dimension)]!, "Blueprint Dimension coordinates must be non-negative integers");
				dimension[axis] = (int)legacyDimension[axis];
			}

			float[][] positions = new float[blocks.Length][];
			int[] minimum = [int.MaxValue, int.MaxValue, int.MaxValue];
			for (int blockIndex = 0; blockIndex < blocks.Length; blockIndex++)
			{
				float[] position = BlueprintMigrationJson.RequireVector3(blocks[blockIndex]["Position"], "Block Position", blocks[blockIndex]);
				positions[blockIndex] = position;
				for (int axis = 0; axis < minimum.Length; axis++) minimum[axis] = Math.Min(minimum[axis], checked((int)MathF.Floor(position[axis])));
			}

			// Some legacy files were stamped as 1.3 with an explicitly zero Dimension,
			// so derive any missing axes from their normalized block positions.
			int[] maximum = [dimension[0] - 1, dimension[1] - 1, dimension[2] - 1];
			for (int blockIndex = 0; blockIndex < blocks.Length; blockIndex++)
			{
				float[] position = positions[blockIndex];
				JArray normalized = new();
				for (int axis = 0; axis < minimum.Length; axis++)
				{
					float coordinate = position[axis] - minimum[axis];
					normalized.Add(coordinate);
					maximum[axis] = Math.Max(maximum[axis], checked((int)MathF.Floor(coordinate)));
				}
				blocks[blockIndex]["Position"] = normalized;
			}

			root[nameof(EcoBlueprint.Dimension)] = new JArray(
				checked(maximum[0] + 1),
				checked(maximum[1] + 1),
				checked(maximum[2] + 1));
			BlueprintMigrationJson.SetVersion(root, this.TargetVersion);
		}
	}
}
