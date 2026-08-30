using Eco.Shared.Math;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model
{
	internal record EcoBlueprintInfo
	{
		[JsonProperty("Version")] public Version Version { get; internal set; }
		[JsonProperty("EcoVersion")] public string EcoVersion { get; internal set; }
		[JsonProperty("Author")] public AuthorInformation Author { get; internal set; }
		/// <summary>Dimension in Width, Height, Length. Zero vector if not provided.</summary>
		[JsonProperty("Dimension")] public Vector3i Dimension { get; internal set; }
		[JsonIgnore] public DateTime FileCreatedDate { get; private set; }
		[JsonIgnore] public DateTime FileChangedDate { get; private set; }
		[JsonIgnore] public long FileSize { get; private set; }
		[JsonIgnore] public string FileName { get; private set; } = string.Empty;

		[JsonConstructor]
		public EcoBlueprintInfo(Version version, string ecoVersion, AuthorInformation author, Vector3i dimension)
		{
			this.Version = version;
			this.EcoVersion = ecoVersion;
			this.Author = author ?? AuthorInformation.Unowned();
			this.Dimension = dimension;
		}

		public void SetFileInformation(FileInfo info)
		{
			this.FileCreatedDate = info.CreationTime;
			this.FileChangedDate = info.LastWriteTime;
			this.FileSize = info.Length;
			this.FileName = info.Name;
		}
	}
}
