using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Model.BlockData;

namespace Eco.Mods.WorldEdit.Serializer.Converters
{
	internal sealed class JsonBlockDataConverter : JsonDataConverter<BlockDataType, IBlockData>
	{
		public JsonBlockDataConverter(DataTypeRegistry<BlockDataType, IBlockData> registry) : base(registry)
		{
		}
	}
}
