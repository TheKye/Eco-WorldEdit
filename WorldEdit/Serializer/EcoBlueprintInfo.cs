using System;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal struct EcoBlueprintInfo
	{
		public float Version { get; private set; }
		public string EcoVersion { get; private set; }
		public AuthorInformation Author { get; private set; }
		public DateTime FileCreatedDate { get; private set; }
		public DateTime FileChangedDate { get; private set; }
		public long FileSize { get; private set; }

	}
}
