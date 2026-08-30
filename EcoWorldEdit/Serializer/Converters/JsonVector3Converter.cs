using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Converters
{
	internal class JsonVector3Converter : JsonConverter<Vector3>
	{
		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JArray array = JArray.Load(reader);
			if (array.Count != 3) throw new JsonSerializationException($"Vector3 must contain exactly 3 components, but contains {array.Count}.");

			return new Vector3(
				array[0]!.Value<float>(),
				array[1]!.Value<float>(),
				array[2]!.Value<float>());
		}

		public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
		{
			new JArray(value.X, value.Y, value.Z).WriteTo(writer);
		}
	}
}
