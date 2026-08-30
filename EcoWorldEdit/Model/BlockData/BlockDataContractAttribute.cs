namespace Eco.Mods.WorldEdit.Model.BlockData
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	internal sealed class BlockDataContractAttribute : Attribute, IDataTypeContractAttribute<BlockDataType>
	{
		public BlockDataType Discriminator { get; }

		public BlockDataContractAttribute(BlockDataType discriminator)
		{
			this.Discriminator = discriminator;
		}
	}
}
