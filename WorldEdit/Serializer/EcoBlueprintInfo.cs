using System;
using System.IO;
using Eco.Shared.Math;
using Eco.Shared.Logging;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal struct EcoBlueprintInfo
	{
		[JsonProperty("Version")] public float Version { get; internal set; }
		[JsonProperty("EcoVersion")] public string EcoVersion { get; internal set; }
		[JsonProperty("Author")] public AuthorInformation Author { get; internal set; }
		/// <summary>Dimension in Width, Height, Length. Zero vector if not provided.</summary>
		[JsonProperty("Dimension"), JsonConverter(typeof(JsonVector3iConverter))] public Vector3i Dimension { get; internal set; }
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
		public EcoBlueprintInfo(float version, string ecoVersion, AuthorInformation author, Vector3i dimension) : this()
		{
			this.Version = version;
			this.EcoVersion = ecoVersion;
			this.Author = author ?? AuthorInformation.Unowned();
			this.Dimension = dimension;

			Log.WriteWarningLineLocStr($"Loaded dimension: {this.Dimension}"); //TODO: !Remove debug output
		}
	}
}
