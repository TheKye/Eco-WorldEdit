using System;
using Eco.Shared.Utils;
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
				if (reader.TokenType == JsonToken.String)
				{
					string typeString = (string)value;
					type = Type.GetType(typeString);
				}
			}

			if (type == null)
			{
				Log.WriteErrorLineLoc($"Error converting [{reader.Value}] to System.Type, fall back to EmptyBlock");
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
