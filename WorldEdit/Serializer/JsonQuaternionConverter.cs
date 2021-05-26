using System;
using System.Diagnostics.CodeAnalysis;
using Eco.Shared.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal class JsonQuaternionConverter : JsonConverter<Quaternion>
	{
		public override Quaternion ReadJson(JsonReader reader, Type objectType, [AllowNull] Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JArray array = JArray.Load(reader);
			return new Quaternion(
				(float)array[0],
				(float)array[1],
				(float)array[2],
				(float)array[3]);
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] Quaternion value, JsonSerializer serializer)
		{
			JArray array = new JArray { value.x, value.y, value.z, value.w };
			array.WriteTo(writer);
		}
	}
}
