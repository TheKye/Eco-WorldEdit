using Eco.Gameplay.Components.Storage;
using System;
using System.Collections.Generic;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal static class Conversion
	{
		public static readonly Dictionary<string, Type> TypeConversionDictionary = new Dictionary<string, Type>()
		{
			{ "Eco.Gameplay.Components.StorageComponent", typeof(StorageComponent) },
		};
	}
}
