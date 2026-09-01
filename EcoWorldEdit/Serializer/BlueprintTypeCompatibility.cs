using System.Diagnostics.CodeAnalysis;
using Eco.Gameplay.Items;
using Eco.Gameplay.Modules;
using Eco.Gameplay.Objects;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Model.Components;
using Eco.Shared.Logging;
using Eco.Simulation.Types;
using Eco.World.Blocks;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer
{
	/// <summary>
	/// Removes data that depends on unavailable game or mod types after migrations have run,
	/// but before the JSON is converted to the strict DTO model.
	/// </summary>
	internal static class BlueprintTypeCompatibility
	{
		public static void Normalize(JObject root)
		{
			if (root["Blocks"] is JArray blocks) NormalizeBlocks(blocks);
			if (root["Plants"] is JArray plants) NormalizePlants(plants);
			if (root["Objects"] is JArray objects) NormalizeWorldObjects(objects);
		}

		private static void NormalizeBlocks(JArray blocks)
		{
			foreach (JObject block in blocks.OfType<JObject>())
			{
				JToken? typeToken = block["BlockType"];
				if (TryResolveExpectedType(typeToken, typeof(Block), out _)) continue;

				LogUnavailable(typeToken, block, "replaced block with EmptyBlock");
				block["BlockType"] = TypeManager.Obj.GetString(typeof(EmptyBlock));
				block["BlockData"] = JValue.CreateNull();
				block["Color"] = JValue.CreateNull();
			}
		}

		private static void NormalizePlants(JArray plants)
		{
			foreach (JObject plant in plants.OfType<JObject>().ToArray())
			{
				JToken? blockTypeToken = plant["BlockType"];
				if (!TryResolveExpectedType(blockTypeToken, typeof(Block), out _))
				{
					LogUnavailable(blockTypeToken, plant, "ignored plant with unavailable block type");
					plant.Remove();
					continue;
				}

				if (!TryGetBlockData(plant, BlockDataType.Plant, out JObject? data)) continue;
				JToken? plantTypeToken = data["PlantType"];
				if (TryResolveExpectedType(plantTypeToken, typeof(PlantSpecies), out _)) continue;

				LogUnavailable(plantTypeToken, plant, "ignored plant with unavailable PlantSpecies type");
				plant.Remove();
			}
		}

		private static void NormalizeWorldObjects(JArray objects)
		{
			HashSet<Guid> removedObjectIds = [];
			foreach (JObject worldObject in objects.OfType<JObject>().ToArray())
			{
				JToken? blockTypeToken = worldObject["BlockType"];
				if (!TryResolveExpectedType(blockTypeToken, typeof(Block), out _))
				{
					LogUnavailable(blockTypeToken, worldObject, "ignored world object with unavailable block type");
					RemoveWorldObject(worldObject, null, removedObjectIds);
					continue;
				}

				if (!TryGetBlockData(worldObject, BlockDataType.WorldObject, out JObject? data)) continue;
				JToken? worldObjectTypeToken = data["WorldObjectType"];
				if (!TryResolveExpectedType(worldObjectTypeToken, typeof(WorldObject), out Type? worldObjectType))
				{
					LogUnavailable(worldObjectTypeToken, worldObject, "ignored world object with unavailable WorldObject type");
					RemoveWorldObject(worldObject, data, removedObjectIds);
					continue;
				}

				if (WorldObjectItem.GetCreatingItemTemplateFromType(worldObjectType) is null)
				{
					LogUnavailable(worldObjectTypeToken, worldObject, "ignored world object with unavailable CreatingItem");
					RemoveWorldObject(worldObject, data, removedObjectIds);
					continue;
				}

				NormalizeComponents(data);
			}

			RemoveChildrenOfUnavailableObjects(objects, removedObjectIds);
		}

		private static void NormalizeComponents(JObject worldObjectData)
		{
			if (worldObjectData["Components"] is not JArray components) return;
			foreach (JObject component in components.OfType<JObject>().ToArray())
			{
				if (!TryGetDiscriminatedData(component, out int discriminator, out JObject? data)) continue;
				switch ((WorldObjectComponentType)discriminator)
				{
					case WorldObjectComponentType.Storage:
						NormalizeInventoryStacks(data["InventoryStacks"] as JArray);
						break;
					case WorldObjectComponentType.Mint:
						JToken? backingItemToken = data["BackingItem"];
						if (!TryResolveExpectedType(backingItemToken, typeof(Item), out _))
						{
							LogUnavailable(backingItemToken, component, "ignored Mint component with unavailable backing item type");
							component.Remove();
						}
						break;
					case WorldObjectComponentType.Store:
						NormalizeStoreCategories(data["Buy"] as JArray);
						NormalizeStoreCategories(data["Sell"] as JArray);
						break;
					case WorldObjectComponentType.PluginModules:
						NormalizePluginModules(data["Modules"] as JArray);
						break;
				}
			}
		}

		private static void NormalizePluginModules(JArray? modules)
		{
			if (modules is null) return;

			HashSet<string> slotTags = new(StringComparer.Ordinal);
			foreach (JObject module in modules.OfType<JObject>().ToArray())
			{
				JToken? slotTagToken = module["SlotTag"];
				string? slotTag = slotTagToken?.Type == JTokenType.String ? slotTagToken.Value<string>() : null;
				if (string.IsNullOrWhiteSpace(slotTag))
				{
					LogIgnored(slotTagToken, module, "ignored plugin module with missing or empty slot tag");
					module.Remove();
					continue;
				}

				JToken? moduleTypeToken = module["ModuleType"];
				if (!TryResolveExpectedType(moduleTypeToken, typeof(PluginModule), out _))
				{
					LogUnavailable(moduleTypeToken, module, "ignored plugin module with unavailable or invalid PluginModule type");
					module.Remove();
					continue;
				}

				if (slotTags.Add(slotTag)) continue;

				LogIgnored(slotTagToken, module, $"ignored duplicate plugin module slot '{slotTag}'");
				module.Remove();
			}
		}

		private static void NormalizeInventoryStacks(JArray? stacks)
		{
			if (stacks is null) return;
			foreach (JObject stack in stacks.OfType<JObject>().ToArray())
			{
				JToken? itemTypeToken = stack["ItemType"];
				if (TryResolveExpectedType(itemTypeToken, typeof(Item), out _)) continue;

				LogUnavailable(itemTypeToken, stack, "ignored inventory stack with unavailable item type");
				stack.Remove();
			}
		}

		private static void NormalizeStoreCategories(JArray? categories)
		{
			if (categories is null) return;
			foreach (JObject category in categories.OfType<JObject>())
			{
				if (category["Offers"] is not JArray offers) continue;
				foreach (JObject offer in offers.OfType<JObject>().ToArray())
				{
					if (offer["Stack"] is not JObject stack) continue;
					JToken? itemTypeToken = stack["ItemType"];
					if (TryResolveExpectedType(itemTypeToken, typeof(Item), out _)) continue;

					LogUnavailable(itemTypeToken, offer, "ignored store offer with unavailable item type");
					offer.Remove();
				}
			}
		}

		private static void RemoveChildrenOfUnavailableObjects(JArray objects, HashSet<Guid> removedObjectIds)
		{
			if (removedObjectIds.Count == 0) return;

			bool removed;
			do
			{
				removed = false;
				foreach (JObject worldObject in objects.OfType<JObject>().ToArray())
				{
					if (!TryGetBlockData(worldObject, BlockDataType.WorldObject, out JObject? data)) continue;
					if (!TryReadGuid(data["ParentId"], out Guid parentId) || !removedObjectIds.Contains(parentId)) continue;

					LogIgnored(data["ParentId"], worldObject, $"ignored child world object because parent {parentId} was unavailable");
					RemoveWorldObject(worldObject, data, removedObjectIds);
					removed = true;
				}
			}
			while (removed);
		}

		private static void RemoveWorldObject(JObject worldObject, JObject? data, HashSet<Guid> removedObjectIds)
		{
			if (data is null) TryGetBlockData(worldObject, BlockDataType.WorldObject, out data);
			if (data is not null && TryReadGuid(data["ObjectId"], out Guid objectId)) removedObjectIds.Add(objectId);
			worldObject.Remove();
		}

		private static bool TryGetBlockData(JObject block, BlockDataType expectedType, [NotNullWhen(true)] out JObject? data)
		{
			data = null;
			if (block["BlockData"] is not JObject wrapper) return false;
			if (!TryGetDiscriminatedData(wrapper, out int discriminator, out data)) return false;
			return discriminator == (int)expectedType;
		}

		private static bool TryGetDiscriminatedData(JObject wrapper, out int discriminator, [NotNullWhen(true)] out JObject? data)
		{
			discriminator = default;
			data = wrapper["Data"] as JObject;
			return wrapper["Type"]?.Type == JTokenType.Integer && data is not null && int.TryParse(wrapper["Type"]!.ToString(), out discriminator);
		}

		private static bool TryResolveExpectedType(JToken? token, Type expectedBaseType, out Type? type)
		{
			type = null;
			if (token?.Type != JTokenType.String) return false;
			type = TypeManager.Obj.GetType(token.Value<string>());
			return type is not null && expectedBaseType.IsAssignableFrom(type);
		}

		private static bool TryReadGuid(JToken? token, out Guid value)
		{
			value = default;
			return token?.Type == JTokenType.String && Guid.TryParse(token.Value<string>(), out value);
		}

		private static void LogUnavailable(JToken? typeToken, JToken owner, string action)
		{
			string typeName = typeToken?.Type == JTokenType.String ? typeToken.Value<string>()! : typeToken?.ToString() ?? "<missing>";
			LogIgnored(typeToken, owner, $"{action}: '{typeName}'");
		}

		private static void LogIgnored(JToken? token, JToken owner, string message)
		{
			string path = token?.Path ?? owner.Path;
			if (string.IsNullOrEmpty(path)) path = "$";
			Log.WriteWarningLineLoc($"Blueprint compatibility: {message}. Path: {path}.");
		}
	}
}
