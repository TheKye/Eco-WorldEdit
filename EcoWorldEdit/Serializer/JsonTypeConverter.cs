using System;
using Eco.Shared.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal class JsonTypeConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (typeof(Type).IsAssignableFrom(value.GetType()))
			{
				Type type = (Type)value;
				writer.WriteValue(type.AssemblyQualifiedName);
			}
			else
			{
				JToken t = JToken.FromObject(value);
				t.WriteTo(writer);
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type type = null;
			if (reader.TokenType != JsonToken.Null)
			{
				JValue value = new JValue(reader.Value);
				if (reader.TokenType == JsonToken.String || reader.TokenType == JsonToken.PropertyName)
				{
					string typeString = (string)value;
					type = Type.GetType(typeString);
					//If null, try use conversion
					if (type is null)
					{
						string[] parsedTypeStr = typeString.ToString().Split(',');
						if (Conversion.TypeConversionDictionary.TryGetValue(parsedTypeStr[0].Trim(), out Type conversionType)) { type = conversionType; }
					}
				}
			}

			if (type is null)
			{
				Log.WriteErrorLineLoc($"Error converting [{reader.Value}] ({reader.TokenType}) to System.Type, fall back to EmptyBlock");
				type = typeof(Eco.World.Blocks.EmptyBlock);
			}
			return type;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(Type).IsAssignableFrom(objectType);
		}
	}
}
