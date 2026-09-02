using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Newtonsoft.Json.Linq;

using EcoBlock = Eco.World.Blocks.Block;
using EcoBlockManager = Eco.World.BlockManager;

namespace Eco.Mods.WorldEdit.Serializer.Migrations
{
	/// <summary>Corrects the rotation of stair and roof blocks saved before Eco 0.10.</summary>
	internal sealed class BlueprintEcoMigrationTo0_10_0_0 : IBlueprintEcoVersionMigration
	{
		private static readonly Version MigrationVersion = new(0, 10, 0, 0);
		public Version TargetVersion => MigrationVersion;
		public int Order => 0;

		private const string BlockSuffix = "Block";

		private static readonly string[] AshlarMaterials =
		[
			"AshlarBasalt",
			"AshlarGneiss",
			"AshlarGranite",
			"AshlarLimestone",
			"AshlarSandstone",
			"AshlarShale"
		];

		private static readonly string[] CompositeLumberMaterials =
		[
			"CompositeBirchLumber",
			"CompositeCedarLumber",
			"CompositeCeibaLumber",
			"CompositeFirLumber",
			"CompositeJoshuaLumber",
			"CompositeLumber",
			"CompositeOakLumber",
			"CompositePalmLumber",
			"CompositeRedwoodLumber",
			"CompositeSaguaroLumber",
			"CompositeSpruceLumber"
		];

		private static readonly IReadOnlySet<string> RoofMaterials = CreateMaterialSet(
			AshlarMaterials,
			CompositeLumberMaterials,
			[
				"Brick",
				"CorrugatedSteel",
				"FlatSteel",
				"FramedGlass",
				"HardwoodHewnLog",
				"HardwoodLumber",
				"HewnLog",
				"Lumber",
				"MortaredGranite",
				"MortaredLimestone",
				"MortaredSandstone",
				"MortaredStone",
				"ReinforcedConcrete",
				"SoftwoodHewnLog",
				"SoftwoodLumber"
			]);

		private static readonly IReadOnlySet<string> StairsMaterials = CreateMaterialSet(
			AshlarMaterials,
			CompositeLumberMaterials,
			[
				"Brick",
				"CorrugatedSteel",
				"FramedGlass",
				"HardwoodHewnLog",
				"HardwoodLumber",
				"HewnLog",
				"Lumber",
				"MortaredGranite",
				"MortaredLimestone",
				"MortaredSandstone",
				"MortaredStone",
				"ReinforcedConcrete",
				"SoftwoodHewnLog",
				"SoftwoodLumber"
			]);

		private static readonly IReadOnlySet<string> UnderStairsMaterials = CreateMaterialSet(
			AshlarMaterials,
			CompositeLumberMaterials,
			["ReinforcedConcrete"]);

		private static readonly IReadOnlySet<string> FloatStairsMaterials = CreateMaterialSet(
			AshlarMaterials,
			CompositeLumberMaterials,
			["CorrugatedSteel", "FlatSteel"]);

		private static readonly IReadOnlySet<string> StairsCornerMaterials = CreateMaterialSet(
			AshlarMaterials,
			CompositeLumberMaterials,
			["CorrugatedSteel", "ReinforcedConcrete"]);

		private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> LegacyMaterialsByForm =
			new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
			{
				["FloatStairsCorner"] = FloatStairsMaterials,
				["FloatStairsTurn"] = FloatStairsMaterials,
				["StairsCorner"] = StairsCornerMaterials,
				["StairsTurn"] = StairsCornerMaterials,
				["FloatStairs"] = FloatStairsMaterials,
				["UnderStairs"] = UnderStairsMaterials,
				["Stairs"] = StairsMaterials,
				["RoofCorner"] = RoofMaterials,
				["RoofPeak"] = RoofMaterials,
				["RoofSide"] = RoofMaterials,
				["RoofTurn"] = RoofMaterials
			};

		// Longer names must be checked first because, for example, FloatStairsCorner also ends with StairsCorner.
		private static readonly string[] FormsByDescendingLength = LegacyMaterialsByForm.Keys.OrderByDescending(form => form.Length).ThenBy(form => form, StringComparer.Ordinal).ToArray();

		public void Migrate(JObject root, MigrationInfo info)
		{
			JArray blocks = BlueprintMigrationJson.RequireArray(root[nameof(EcoBlueprint.Blocks)], "Blueprint Blocks property", root);
			foreach (JToken token in blocks)
			{
				JObject block = BlueprintMigrationJson.RequireObject(token, "Block entry", blocks);
				MigrateBlock(block);
			}
		}

		private static void MigrateBlock(JObject block)
		{
			JToken? typeToken = block[nameof(WorldEditBlock.BlockType)];
			string sourceTypeString = BlueprintMigrationJson.RequireString(typeToken, "BlockType", block);
			if (!TryGetMigratedTypeName(sourceTypeString, out string targetTypeString, out string targetFullName)) return;

			Type? targetType = TypeManager.Obj.GetType(targetTypeString) ?? EcoBlockManager.BlockTypes.FirstOrDefault(type => type.FullName == targetFullName);
			if (targetType is null || !typeof(EcoBlock).IsAssignableFrom(targetType))
			{
				string path = string.IsNullOrEmpty(typeToken!.Path) ? "$" : typeToken.Path;
				Logging.Warning($"Blueprint Eco migration to {MigrationVersion}: rotated variant '{targetFullName}' for '{sourceTypeString}' was not found; kept the original block type. Path: {path}.");
				return;
			}

			block[nameof(WorldEditBlock.BlockType)] = TypeManager.Obj.GetString(targetType);
		}

		private static bool TryGetMigratedTypeName(string sourceTypeString, out string targetTypeString, out string targetFullName)
		{
			targetTypeString = string.Empty;
			targetFullName = string.Empty;

			int assemblySeparator = sourceTypeString.IndexOf(',');
			string sourceFullName = (assemblySeparator >= 0 ? sourceTypeString[..assemblySeparator] : sourceTypeString).Trim();
			int namespaceSeparator = sourceFullName.LastIndexOf('.');
			string sourceTypeName = namespaceSeparator >= 0 ? sourceFullName[(namespaceSeparator + 1)..] : sourceFullName;
			if (!TrySplitVariantTypeName(sourceTypeName, out string familyName, out int angle)) return false;
			if (!IsLegacyAffectedFamily(familyName)) return false;

			int targetAngle = (angle + 270) % 360;
			string targetTypeName = $"{familyName}{(targetAngle == 0 ? string.Empty : targetAngle)}{BlockSuffix}";
			targetFullName = namespaceSeparator >= 0 ? $"{sourceFullName[..(namespaceSeparator + 1)]}{targetTypeName}" : targetTypeName;
			targetTypeString = assemblySeparator >= 0 ? $"{targetFullName}{sourceTypeString[assemblySeparator..]}" : targetFullName;
			return true;
		}

		private static bool TrySplitVariantTypeName(string typeName, out string familyName, out int angle)
		{
			familyName = string.Empty;
			angle = 0;
			if (!typeName.EndsWith(BlockSuffix, StringComparison.Ordinal)) return false;

			string stem = typeName[..^BlockSuffix.Length];
			foreach (int candidateAngle in new[] { 270, 180, 90 })
			{
				string angleSuffix = candidateAngle.ToString(System.Globalization.CultureInfo.InvariantCulture);
				if (!stem.EndsWith(angleSuffix, StringComparison.Ordinal)) continue;
				familyName = stem[..^angleSuffix.Length];
				angle = candidateAngle;
				return familyName.Length > 0;
			}

			familyName = stem;
			return familyName.Length > 0;
		}

		private static bool IsLegacyAffectedFamily(string familyName)
		{
			foreach (string form in FormsByDescendingLength)
			{
				if (!familyName.EndsWith(form, StringComparison.Ordinal)) continue;
				string material = familyName[..^form.Length];
				return LegacyMaterialsByForm[form].Contains(material);
			}
			return false;
		}

		private static HashSet<string> CreateMaterialSet(params IEnumerable<string>[] materialGroups) => new(materialGroups.SelectMany(group => group), StringComparer.Ordinal);
	}
}
