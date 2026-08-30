using System.Reflection;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Eco.Simulation.Types;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Migrations
{
	/// <summary>Changes the saved plant runtime type to its PlantSpecies type and adds author metadata.</summary>
	internal sealed class BlueprintMigrationTo1_2 : IBlueprintVersionMigration
	{
		public Version TargetVersion { get; } = new(1, 2);
		public int Order => 0;

		public void Migrate(JObject root, MigrationInfo info)
		{
			BlueprintMigrationJson.RequireSourceVersion(root, new Version(1, 1));
			BlueprintMigrationJson.Blocks(root).ToArray();

			JArray plants = BlueprintMigrationJson.RequireArray(root[nameof(EcoBlueprint.Plants)], "Blueprint Plants property", root);
			foreach (JToken token in plants)
			{
				JObject block = BlueprintMigrationJson.RequireObject(token, "Plant entry", plants);
				JObject data = BlueprintMigrationJson.RequireObject(block["BlockData"], "Plant BlockData", block);
				JToken? plantTypeToken = data["PlantType"];
				string plantTypeName = BlueprintMigrationJson.RequireString(plantTypeToken, "PlantType", data);
				Type plantType = TypeManager.Obj.GetType(plantTypeName) ?? throw BlueprintMigrationJson.Error(plantTypeToken!, $"Unable to resolve old plant type '{plantTypeName}'");
				if (typeof(PlantSpecies).IsAssignableFrom(plantType)) continue;

				Type[] speciesTypes = plantType
					.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
					.Where(type => !type.IsAbstract && typeof(PlantSpecies).IsAssignableFrom(type))
					.ToArray();
				if (speciesTypes.Length != 1)
				{
					throw BlueprintMigrationJson.Error(plantTypeToken!, $"Plant type '{plantTypeName}' has {speciesTypes.Length} nested PlantSpecies types; expected exactly one");
				}
				data["PlantType"] = TypeManager.Obj.GetString(speciesTypes[0]);
			}

			BlueprintMigrationJson.EnsureAuthor(root);
			BlueprintMigrationJson.SetVersion(root, this.TargetVersion);
		}
	}
}
