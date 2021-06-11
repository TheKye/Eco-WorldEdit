using System;
using System.IO;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal struct EcoBlueprintInfo
	{
		[JsonProperty("Version")] public float Version { get; internal set; }
		[JsonProperty("EcoVersion")] public string EcoVersion { get; internal set; }
		[JsonProperty("Author")] public AuthorInformation Author { get; internal set; }
		[JsonIgnore] public DateTime FileCreatedDate { get; private set; }
		[JsonIgnore] public DateTime FileChangedDate { get; private set; }
		[JsonIgnore] public long FileSize { get; private set; }
		[JsonIgnore] public string FileName { get; private set; }

		public static EcoBlueprintInfo FromFile(string file)
		{
			EcoBlueprintInfo blueprintInfo = default;
			FileInfo info = new FileInfo(file);

			if (!info.Exists) { throw new FileNotFoundException("File not found", file); }
			using (FileStream stream = File.OpenRead(file))
			{
				blueprintInfo = WorldEditSerializer.Deserialize<EcoBlueprintInfo>(stream);
				blueprintInfo.FileCreatedDate = info.CreationTime;
				blueprintInfo.FileChangedDate = info.LastWriteTime;
				blueprintInfo.FileSize = info.Length;
				blueprintInfo.FileName = info.Name;
			}
			return blueprintInfo;
		}

		[JsonConstructor]
		public EcoBlueprintInfo(float version, string ecoVersion, AuthorInformation author) : this()
		{
			this.Version = version;
			this.EcoVersion = ecoVersion;
			this.Author = author ?? AuthorInformation.Unowned();
		}
	}
}
