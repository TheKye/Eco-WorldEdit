using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model.Components;

namespace Eco.Mods.WorldEdit.Serializer.Converters
{
	internal sealed class JsonWorldObjectComponentDataConverter : JsonDataConverter<WorldObjectComponentType, IWorldObjectComponentData>
	{
		public JsonWorldObjectComponentDataConverter(DataTypeRegistry<WorldObjectComponentType, IWorldObjectComponentData> registry) : base(registry)
		{
		}
	}
}
