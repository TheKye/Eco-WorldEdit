using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Shared.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Converters
{
	internal class JsonTypeConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(Type).IsAssignableFrom(objectType);
		}

		public override Type? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			try
			{
				JValue value = new JValue(reader.Value);
				switch (reader.TokenType)
				{
					case JsonToken.String:
					case JsonToken.PropertyName:
						{
							string? typeString = value.Value<string>();
							return TypeManager.Obj.GetType(typeString) ?? throw new JsonSerializationException($"TypeManager return NULL.");
						}
					default: throw new JsonSerializationException($"Unsupported TokenType.");
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLineLoc($"Error while converting [{reader.Value}] ({reader.TokenType}) to System.Type: {ex.Message}");
			}
			return null;
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is null)
			{
				writer.WriteNull();
				return;
			}
			writer.WriteValue(TypeManager.Obj.GetString((Type)value));
		}
	}
}
