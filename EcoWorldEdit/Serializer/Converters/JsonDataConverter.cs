using System.Globalization;
using Eco.Mods.WorldEdit.Core.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Converters
{
	internal abstract class JsonDataConverter<TDiscriminator, TBase> : JsonConverter<TBase> where TDiscriminator : struct, Enum where TBase : class
	{
		private const string TYPE_PROPERTY = "Type";
		private const string DATA_PROPERTY = "Data";

		private readonly DataTypeRegistry<TDiscriminator, TBase> _registry;

		protected JsonDataConverter(DataTypeRegistry<TDiscriminator, TBase> registry)
		{
			this._registry = registry ?? throw new ArgumentNullException(nameof(registry));
		}

		public override TBase? ReadJson(JsonReader reader, Type objectType, TBase? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;

			JObject wrapper = JObject.Load(reader);
			JToken discriminatorToken = wrapper[TYPE_PROPERTY] ?? throw new JsonSerializationException($"Missing {TYPE_PROPERTY} property for {typeof(TBase).Name}.");
			JToken dataToken = wrapper[DATA_PROPERTY] ?? throw new JsonSerializationException($"Missing {DATA_PROPERTY} property for {typeof(TBase).Name}.");
			TDiscriminator discriminator = ReadDiscriminator(discriminatorToken);
			Type dataType = this._registry.GetType(discriminator);

			object? result = this.WithoutCurrentConverter(serializer, () => dataToken.ToObject(dataType, serializer));
			return result as TBase ?? throw new JsonSerializationException($"Unable to deserialize {dataType.FullName}.");
		}

		public override void WriteJson(JsonWriter writer, TBase? value, JsonSerializer serializer)
		{
			if (value is null)
			{
				writer.WriteNull();
				return;
			}

			Type dataType = value.GetType();
			TDiscriminator discriminator = this._registry.GetDiscriminator(dataType);
			JToken dataToken = this.WithoutCurrentConverter(serializer, () => JToken.FromObject(value, serializer));

			writer.WriteStartObject();

			writer.WritePropertyName(TYPE_PROPERTY);
			writer.WriteValue(ToNumericValue(discriminator));

			writer.WritePropertyName(DATA_PROPERTY);
			dataToken.WriteTo(writer);

			writer.WriteEndObject();
		}

		private TResult WithoutCurrentConverter<TResult>(JsonSerializer serializer, Func<TResult> action)
		{
			JsonConverterCollection converters = serializer.Converters;
			int index = converters.IndexOf(this);

			if (index < 0) return action();
			converters.RemoveAt(index);
			try
			{
				return action();
			}
			finally
			{
				converters.Insert(index, this);
			}
		}

		private static TDiscriminator ReadDiscriminator(JToken token)
		{
			if (token.Type != JTokenType.Integer) throw new JsonSerializationException($"{typeof(TDiscriminator).Name} discriminator must be an integer.");

			Type underlyingType = Enum.GetUnderlyingType(typeof(TDiscriminator));
			object numericValue = token.ToObject(underlyingType) ?? throw new JsonSerializationException($"Unable to read {typeof(TDiscriminator).Name} discriminator.");
			return (TDiscriminator)Enum.ToObject(typeof(TDiscriminator), numericValue);
		}

		private static object ToNumericValue(TDiscriminator discriminator) => Convert.ChangeType(discriminator, Enum.GetUnderlyingType(typeof(TDiscriminator)), CultureInfo.InvariantCulture);
	}
}
