using System.Globalization;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Serializer.Migrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Converters
{
	internal class MigrationConverter : JsonConverter
	{
		public override bool CanWrite => false;

		public override bool CanConvert(Type objectType)
		{
			return typeof(EcoBlueprintInfo).IsAssignableFrom(objectType); //Apply for EcoBlueprintInfo and EcoBlueprint
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			JToken token = JToken.Load(reader);
			MigrationInfo info = ReadMigrationInfo(token);
			this.ApplyMigrations(token, info);
			if (token is JObject root && typeof(EcoBlueprint).IsAssignableFrom(objectType)) BlueprintTypeCompatibility.Normalize(root);
			JsonConverterCollection converters = serializer.Converters;
			int index = converters.IndexOf(this);
			if (index >= 0) converters.RemoveAt(index);
			try
			{
				using (JsonReader subReader = token.CreateReader())
				{
					return serializer.Deserialize(subReader, objectType);
				}
			}
			finally
			{
				if (index >= 0) converters.Insert(index, this);
			}
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

		private void ApplyMigrations(JToken token, MigrationInfo info)
		{
			if (token is not JObject root) return;

			foreach (IBlueprintMigration migration in MigrationManager.Obj.GetMigrations(info))
			{
				migration.Migrate(root, info);
			}
		}

		private static MigrationInfo ReadMigrationInfo(JToken token)
		{
			if (token is not JObject obj) throw new JsonSerializationException("Blueprint root must be a JSON object.");

			Version version = ReadVersion(obj[nameof(EcoBlueprintInfo.Version)]);
			Version ecoVersion = ReadVersion(obj[nameof(EcoBlueprintInfo.EcoVersion)]);

			return new MigrationInfo(version, ecoVersion);
		}

		private static Version ReadVersion(JToken? token)
		{
			if (token == null) throw new JsonSerializationException("Blueprint version is missing.");

			string? value = token.Type switch
			{
				JTokenType.Integer or JTokenType.Float => Convert.ToString(((JValue)token).Value, CultureInfo.InvariantCulture),
				JTokenType.String => token.Value<string>(),
				_ => null
			};

			if (!Version.TryParse(value, out Version? version)) throw new JsonSerializationException($"Invalid blueprint version: {token}");

			return version;
		}
	}
}
