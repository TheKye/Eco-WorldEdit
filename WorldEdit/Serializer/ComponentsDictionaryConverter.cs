/// Using code from https://github.com/VinzzF/JsonNetCustomKeyDictionaryObjectConverter

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal class ComponentsDictionaryConverter : JsonConverter
	{
		public override bool CanWrite => false;

		private static void GetDictionaryGenericTypes(Type objectType, out Type[] dictionaryTypes, out bool isStringKey)
		{
			dictionaryTypes = objectType.GetGenericArguments();
			if ((dictionaryTypes?.Length ?? 0) < 2) { throw new InvalidOperationException($"Deserializing Json dictionary with less than two types {objectType.Name}"); }
			isStringKey = dictionaryTypes[0] == typeof(string);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			GetDictionaryGenericTypes(objectType, out Type[] dictionaryTypes, out bool isStringKey);
			IDictionary res = Activator.CreateInstance(objectType) as IDictionary;
			object key = null;
			object value = null;

			if (reader.TokenType != JsonToken.StartObject) { throw new JsonException("Json Dictionary is not represented as object"); }
			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndObject)
				{
					if (key != null || value != null) throw new JsonException($"Json Dictionary ended while still expecting key or value {key}, {value}");
					break;
				}

				if (reader.TokenType == JsonToken.PropertyName)
				{
					if (key != null) throw new JsonException($"Json Dictionary key {key} followed by another key, not value");
					if (reader.ValueType != typeof(string)) throw new JsonException($"Json Dictionary key {reader.Value} is no string type");
					key = isStringKey ? reader.Value : serializer.Deserialize(reader, dictionaryTypes[0]);
					if (value != null) throw new JsonException($"Json Dictionary key {key} read while value present");
				}
				else
				{
					if (key == null) throw new JsonException($"Json Dictionary value read {reader.ReadAsString()} but no read before");
					if (value != null) throw new JsonException($"Json Dictionary value read {reader.ReadAsString()} but already has value {value}");
					value = serializer.Deserialize(reader, dictionaryTypes[1]);

					res.Add(key, value);
					key = null;
					value = null;
				}
			}
			return res;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType.Equals(typeof(Dictionary<Type, Object>));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
