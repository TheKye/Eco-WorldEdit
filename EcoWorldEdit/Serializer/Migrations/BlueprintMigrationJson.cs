using System.Globalization;
using Eco.Mods.WorldEdit.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Migrations
{
	internal static class BlueprintMigrationJson
	{
		private static readonly string[] LayerNames = [nameof(EcoBlueprint.Blocks), nameof(EcoBlueprint.Plants), nameof(EcoBlueprint.Objects)];

		public static IEnumerable<JObject> Blocks(JObject root)
		{
			foreach (string layerName in LayerNames)
			{
				if (root[layerName] is not JArray layer) throw Error(root[layerName] ?? root, $"Blueprint property '{layerName}' must be an array.");
				foreach (JToken token in layer)
				{
					if (token is not JObject block) throw Error(token, $"Every item in '{layerName}' must be an object.");
					yield return block;
				}
			}
		}

		public static JArray RequireArray(JToken? token, string description, JToken parent)
		{
			return token as JArray ?? throw Error(token ?? parent, $"{description} must be an array.");
		}

		public static JObject RequireObject(JToken? token, string description, JToken parent)
		{
			return token as JObject ?? throw Error(token ?? parent, $"{description} must be an object.");
		}

		public static string RequireString(JToken? token, string description, JToken parent)
		{
			if (token?.Type != JTokenType.String) throw Error(token ?? parent, $"{description} must be a string.");
			return token.Value<string>()!;
		}

		public static float[] RequireVector3(JToken? token, string description, JToken parent)
		{
			JArray array = RequireArray(token, description, parent);
			if (array.Count != 3) throw Error(array, $"{description} must contain exactly three coordinates.");

			float[] result = new float[3];
			for (int i = 0; i < result.Length; i++)
			{
				if (array[i].Type is not (JTokenType.Integer or JTokenType.Float)) throw Error(array[i], $"{description} coordinates must be numbers.");
				result[i] = array[i].Value<float>();
				if (!float.IsFinite(result[i])) throw Error(array[i], $"{description} coordinates must be finite numbers.");
			}
			return result;
		}

		public static Version RequireVersion(JObject root)
		{
			JToken? token = root[nameof(EcoBlueprintInfo.Version)];
			string? value = token?.Type switch
			{
				JTokenType.Integer or JTokenType.Float => Convert.ToString(((JValue)token).Value, CultureInfo.InvariantCulture),
				JTokenType.String => token.Value<string>(),
				_ => null
			};
			if (!Version.TryParse(value, out Version? version)) throw Error(token ?? root, "Blueprint version is missing or invalid.");
			return version;
		}

		public static void RequireSourceVersion(JObject root, Version expected)
		{
			Version actual = RequireVersion(root);
			if (actual < new Version(1, 1))
			{
				throw Error(root[nameof(EcoBlueprintInfo.Version)] ?? root, "Blueprint format 1.0 is not supported: its EcoSerializer payload cannot be migrated reliably.");
			}
			if (actual != expected) throw Error(root[nameof(EcoBlueprintInfo.Version)] ?? root, $"Migration expected blueprint version {expected}, but found {actual}.");
		}

		public static void SetVersion(JObject root, Version version) => root[nameof(EcoBlueprintInfo.Version)] = version.ToString(2);

		public static void EnsureAuthor(JObject root)
		{
			if (root[nameof(EcoBlueprintInfo.Author)] is not null and not { Type: JTokenType.Null }) return;
			root[nameof(EcoBlueprintInfo.Author)] = new JObject
			{
				[nameof(AuthorInformation.Name)] = "Unowned",
				[nameof(AuthorInformation.SlgID)] = string.Empty,
				[nameof(AuthorInformation.SteamID)] = string.Empty
			};
		}

		public static JsonSerializationException Error(JToken token, string message)
		{
			string path = string.IsNullOrEmpty(token.Path) ? "$" : token.Path;
			return new JsonSerializationException($"{message} Path: {path}.");
		}
	}
}
