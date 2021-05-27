using System;
using System.Diagnostics.CodeAnalysis;
using Eco.Mods.WorldEdit.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal class JsonWorldEditBlockDataConverter : JsonConverter<IWorldEditBlockData>
	{
		public override IWorldEditBlockData ReadJson(JsonReader reader, Type objectType, [AllowNull] IWorldEditBlockData existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;
			JObject jObject = JObject.Load(reader);

			using (JsonReader jObjectReader = this.CopyReaderForObject(reader, jObject))
			{
				if (this.IsFieldExists("PlantType", jObject))
				{
					return (WorldEditPlantBlockData)serializer.Deserialize(jObjectReader, typeof(WorldEditPlantBlockData));
				}
				else if (this.IsFieldExists("WorldObjectType", jObject))
				{
					return (WorldEditWorldObjectBlockData)serializer.Deserialize(jObjectReader, typeof(WorldEditWorldObjectBlockData));
				}
				else
				{
					throw new TypeLoadException();
				}
			}
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] IWorldEditBlockData value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		private bool IsFieldExists(string fieldName, JObject jObject)
		{
			return jObject[fieldName] != null;
		}

		public JsonReader CopyReaderForObject(JsonReader reader, JToken jToken)
		{
			JsonReader jTokenReader = jToken.CreateReader();
			jTokenReader.Culture = reader.Culture;
			jTokenReader.DateFormatString = reader.DateFormatString;
			jTokenReader.DateParseHandling = reader.DateParseHandling;
			jTokenReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
			jTokenReader.FloatParseHandling = reader.FloatParseHandling;
			jTokenReader.MaxDepth = reader.MaxDepth;
			jTokenReader.SupportMultipleContent = reader.SupportMultipleContent;
			return jTokenReader;
		}
	}
}
