using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Model.Components;
using Eco.Shared;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Migrations
{
	/// <summary>Converts legacy BlockData and component payloads to the v3 discriminated DTO contracts.</summary>
	internal sealed class BlueprintMigrationTo1_4 : IBlueprintVersionMigration
	{
		private static readonly IReadOnlyDictionary<string, WorldObjectComponentType> ComponentTypes = new Dictionary<string, WorldObjectComponentType>(StringComparer.Ordinal)
		{
			["Eco.Gameplay.Components.StorageComponent"] = WorldObjectComponentType.Storage,
			["Eco.Gameplay.Components.Storage.StorageComponent"] = WorldObjectComponentType.Storage,
			["Eco.Gameplay.Components.CustomTextComponent"] = WorldObjectComponentType.CustomText,
			["Eco.Gameplay.Components.MintComponent"] = WorldObjectComponentType.Mint,
			["Eco.Mods.TechTree.DoorObject"] = WorldObjectComponentType.Door,
			["Eco.Gameplay.Components.Store.StoreComponent"] = WorldObjectComponentType.Store
		};

		public Version TargetVersion { get; } = new(1, 4);
		public int Order => 0;

		public void Migrate(JObject root, MigrationInfo info)
		{
			BlueprintMigrationJson.RequireSourceVersion(root, new Version(1, 3));
			foreach (JObject block in BlueprintMigrationJson.Blocks(root)) MigrateBlock(block);
			BlueprintMigrationJson.SetVersion(root, this.TargetVersion);
		}

		private static void MigrateBlock(JObject block)
		{
			JToken? dataToken = block["BlockData"];
			if (dataToken is null or { Type: JTokenType.Null }) return;
			JObject data = BlueprintMigrationJson.RequireObject(dataToken, "BlockData", block);

			bool isPlant = data["PlantType"] is not null;
			bool isWorldObject = data["WorldObjectType"] is not null;
			if (isPlant == isWorldObject)
			{
				throw BlueprintMigrationJson.Error(data, "Legacy BlockData must contain exactly one of PlantType or WorldObjectType");
			}

			if (isWorldObject) MigrateWorldObject(block, data);
			block["BlockData"] = new JObject
			{
				["Type"] = (int)(isPlant ? BlockDataType.Plant : BlockDataType.WorldObject),
				["Data"] = data.DeepClone()
			};
		}

		private static void MigrateWorldObject(JObject block, JObject data)
		{
			if (data["Name"] is null or { Type: JTokenType.Null }) data["Name"] = string.Empty;

			JToken? componentsToken = data["Components"];
			if (componentsToken is null or { Type: JTokenType.Null })
			{
				data["Components"] = new JArray();
			}
			else
			{
				JObject components = BlueprintMigrationJson.RequireObject(componentsToken, "Legacy Components", data);
				data["Components"] = MigrateComponents(components);
			}

			if (data["Position"] is not null and not { Type: JTokenType.Null }) MigrateExactPosition(block, data);
		}

		private static JArray MigrateComponents(JObject components)
		{
			JArray result = new JArray();
			foreach (JProperty property in components.Properties())
			{
				string fullName = property.Name.Split(',', 2)[0].Trim();
				if (!ComponentTypes.TryGetValue(fullName, out WorldObjectComponentType componentType))
				{
					throw BlueprintMigrationJson.Error(property, $"Unsupported legacy world-object component '{property.Name}'");
				}

				result.Add(new JObject
				{
					["Type"] = (int)componentType,
					["Data"] = MigrateComponentData(componentType, property.Value)
				});
			}
			return result;
		}

		private static JToken MigrateComponentData(WorldObjectComponentType componentType, JToken value)
		{
			return componentType switch
			{
				WorldObjectComponentType.Storage => new JObject
				{
					["InventoryStacks"] = BlueprintMigrationJson.RequireArray(value, "Legacy StorageComponent data", value).DeepClone()
				},
				WorldObjectComponentType.CustomText => new JObject
				{
					["Text"] = value.Type == JTokenType.Null ? string.Empty : BlueprintMigrationJson.RequireString(value, "Legacy CustomTextComponent data", value)
				},
				WorldObjectComponentType.Mint or WorldObjectComponentType.Door or WorldObjectComponentType.Store =>
					BlueprintMigrationJson.RequireObject(value, $"Legacy {componentType} component data", value).DeepClone(),
				_ => throw BlueprintMigrationJson.Error(value, $"Unsupported component discriminator {componentType}")
			};
		}

		private static void MigrateExactPosition(JObject block, JObject data)
		{
			float[] local = BlueprintMigrationJson.RequireVector3(block["Position"], "World-object block Position", block);
			float[] exact = BlueprintMigrationJson.RequireVector3(data["Position"], "Legacy world-object exact Position", data);
			block["Position"] = new JArray(
				local[0] + exact[0] - Mathf.RoundPositivelyInt(exact[0]),
				local[1] + exact[1] - Mathf.RoundPositivelyInt(exact[1]),
				local[2] + exact[2] - Mathf.RoundPositivelyInt(exact[2]));
			data.Remove("Position");
		}
	}
}
