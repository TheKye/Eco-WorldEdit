using System.Text;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Serializer.Converters;
using K4os.Compression.LZ4.Streams;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal class WorldEditSerializer
	{
		// Headers
		private const string LZ4_HEADER = "LZ4";

		// Versions
		public static readonly Version CurrentVersion = new(1, 5);
		/* Version History:
		 * 1.0 - Old and unused format, supported blocks only.
		 * 1.1 - Support plants, objects, blocks at separate layers
		 * 1.2 - Changed WorldEditPlantBlockData.PlantType form plant.GetType() to plant.Species.GetType()
		 *	added AuthorInformation
		 * 1.3 - Added Dimension information
		 * 1.4 - Support for v3.
		 * 1.5 - Normalized block positions to the clipboard minimum corner.
		 * */
		public static string CurrentEcoVersion => Shared.EcoVersion.VersionNumber;

		private readonly JsonSerializer _serializer;

		public WorldEditSerializer()
		{
			this._serializer = JsonSerializer.CreateDefault(DefaultSerializerSettings);
		}

		public static JsonSerializerSettings DefaultSerializerSettings
		{
			get
			{
				JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
				serializerSettings.Culture = System.Globalization.CultureInfo.InvariantCulture;
				serializerSettings.Converters.Add(new MigrationConverter());
				serializerSettings.Converters.Add(new JsonBlockDataConverter(WorldEditManager.Obj.BlockDataRegistry));
				serializerSettings.Converters.Add(new JsonWorldObjectComponentDataConverter(WorldEditManager.Obj.ComponentRegistry));
				serializerSettings.Converters.Add(new JsonTypeConverter());
				serializerSettings.Converters.Add(new JsonQuaternionConverter());
				serializerSettings.Converters.Add(new JsonVector3iConverter());
				serializerSettings.Converters.Add(new JsonVector3Converter());
				return serializerSettings;
			}
		}

		public void Serialize(Stream stream, EcoBlueprint schematic)
		{
			this.SerializeJSON(stream, schematic);
		}

		private void SerializeJSON(Stream stream, object obj)
		{
			byte[] header = new byte[8];
			Array.Copy(Encoding.ASCII.GetBytes(LZ4_HEADER), header, Math.Min(8, LZ4_HEADER.Length));
			stream.Write(header, 0, 8);

			using (LZ4EncoderStream lZ4EncoderStream = LZ4Stream.Encode(stream, null, true))
			using (StreamWriter sw = new StreamWriter(lZ4EncoderStream, Encoding.UTF8, 1024, true))
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				writer.Formatting = Formatting.None;
				this._serializer.Serialize(writer, obj);
			}
			stream.Seek(0, SeekOrigin.Begin);
		}

		public EcoBlueprint Deserialize(string file)
		{
			FileInfo info = new FileInfo(file);
			if (!info.Exists) throw new FileNotFoundException("File not found", file);
			using (FileStream stream = File.OpenRead(file))
			{
				EcoBlueprint schematic = this.Deserialize<EcoBlueprint>(stream) ?? throw new NullReferenceException($"Unable deserialize EcoBlueprint from {file}");
				schematic.SetFileInformation(info);
				return schematic;
			}
		}

		public EcoBlueprintInfo DeserializeInfo(string file)
		{
			FileInfo info = new FileInfo(file);
			if (!info.Exists) throw new FileNotFoundException("File not found", file);
			using (FileStream stream = File.OpenRead(file))
			{
				EcoBlueprintInfo blueprintInfo = this.Deserialize<EcoBlueprintInfo>(stream) ?? throw new NullReferenceException($"Unable deserialize EcoBlueprintInfo from {file}");
				blueprintInfo.SetFileInformation(info);
				return blueprintInfo;
			}
		}

		private T? Deserialize<T>(Stream stream)
		{
			if (IsLZ4Stream(stream))
			{
				using (LZ4DecoderStream lZ4DecoderStream = LZ4Stream.Decode(stream, null, true))
				{
					return this.DeserializeJSON<T>(lZ4DecoderStream);
				}
			}
			else
			{
				return this.DeserializeJSON<T>(stream);
			}
		}

		private T? DeserializeJSON<T>(Stream stream)
		{
			using StreamReader sr = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
			using JsonReader reader = new JsonTextReader(sr);
			return this._serializer.Deserialize<T>(reader);
		}

		private static bool IsLZ4Stream(Stream stream)
		{
			long startPosition = stream.Position;
			byte[] buff = new byte[8];
			int bytesRead = stream.ReadAtLeast(buff, buff.Length, false);
			if (bytesRead == buff.Length && Encoding.ASCII.GetString(buff, 0, LZ4_HEADER.Length).Equals(LZ4_HEADER, StringComparison.Ordinal))
			{
				return true;
			}
			else
			{
				stream.Seek(startPosition, SeekOrigin.Begin);
				return false;
			}
		}
	}
}
