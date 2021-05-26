using System;
using System.Diagnostics.CodeAnalysis;
using Eco.Shared.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal class JsonVector3iConverter : JsonConverter<Vector3i>
	{
		public override Vector3i ReadJson(JsonReader reader, Type objectType, [AllowNull] Vector3i existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JArray array = JArray.Load(reader);
			return new Vector3i(
				(int)array[0],
				(int)array[1],
				(int)array[2]);
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] Vector3i value, JsonSerializer serializer)
		{
			JArray array = new JArray { value.x, value.y, value.z, };
			array.WriteTo(writer);
		}
	}
}
