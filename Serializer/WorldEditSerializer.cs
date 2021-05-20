using System.Collections.Generic;
using System.IO;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Utils;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal class WorldEditSerializer
	{
		public const float currentVersion = 1.1f;
		public string CurrentEcoVersion => Shared.EcoVersion.VersionNumber;
		private readonly List<WorldEditBlock> blockList = new List<WorldEditBlock>();
		private readonly List<WorldEditBlock> plantList = new List<WorldEditBlock>();
		private readonly List<WorldEditBlock> worldObjectList = new List<WorldEditBlock>();

		public List<WorldEditBlock> BlockList
		{
			get
			{
				return new List<WorldEditBlock>(this.blockList);
			}
			set
			{
				this.blockList.Clear();
				this.blockList.AddRange(value);
			}
		}

		public List<WorldEditBlock> PlantList
		{
			get
			{
				return new List<WorldEditBlock>(this.plantList);
			}
			set
			{
				this.plantList.Clear();
				this.plantList.AddRange(value);
			}
		}

		public List<WorldEditBlock> WorldObjectList
		{
			get
			{
				return new List<WorldEditBlock>(this.worldObjectList);
			}
			set
			{
				this.worldObjectList.Clear();
				this.worldObjectList.AddRange(value);
			}
		}

		public WorldEditSerializer()
		{

		}

		public static JsonSerializerSettings SerializerSettings
		{
			get
			{
				JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
				serializerSettings.Converters.Add(new JsonQuaternionConverter());
				serializerSettings.Converters.Add(new JsonVector3iConverter());
				return serializerSettings;
			}
		}

		public static WorldEditSerializer FromClipboard(WorldEditClipboard clipboard)
		{
			WorldEditSerializer serializer = new WorldEditSerializer();
			serializer.BlockList = clipboard.GetBlocks();
			serializer.PlantList = clipboard.GetPlants();
			serializer.WorldObjectList = clipboard.GetWorldObjects();
			return serializer;
		}

		public MemoryStream Serialize()
		{
			EcoBlueprint schematic = EcoBlueprint.Create(this.blockList, this.plantList, this.worldObjectList);
			return Serialize(schematic);
		}

		public static MemoryStream Serialize(object obj)
		{
			MemoryStream stream = StreamPool.GetStream();
			using (StreamWriter sw = new StreamWriter(stream, System.Text.Encoding.UTF8, 1024, true))
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				writer.Formatting = Newtonsoft.Json.Formatting.Indented;
				JsonSerializer serializer = JsonSerializer.CreateDefault(SerializerSettings);
				serializer.Serialize(writer, obj);
			}
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public void Deserialize(Stream stream)
		{
			EcoBlueprint schematic = Deserialize<EcoBlueprint>(stream);

			if (!currentVersion.Equals(schematic.Version))
			{
				//TODO: Handle serialization version changes and support previous versions
				//throw new FileLoadException(message: $"EcoBlueprint file version missmatch [file version: {schematic.Version}, current version: {currentVersion}]");
			}

			if (!string.IsNullOrEmpty(schematic.EcoVersion) && !this.CurrentEcoVersion.Equals(schematic.EcoVersion))
			{
				//TODO: Handle ECO version changes and migrate from previous versions
				//throw new FileLoadException(message: $"EcoBlueprint file created in different Eco version [file version: {schematic.EcoVersion}, current version: {this.CurrentEcoVersion}]");
			}

			this.BlockList = schematic.Blocks;
			this.PlantList = schematic.Plants;
			this.WorldObjectList = schematic.Objects;
		}

		public static T Deserialize<T>(Stream stream)
		{
			using (StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8, false, 1024, true))
			using (JsonReader reader = new JsonTextReader(sr))
			{
				JsonSerializer serializer = JsonSerializer.CreateDefault(SerializerSettings);
				return serializer.Deserialize<T>(reader);
			}
		}
	}
}
