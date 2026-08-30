namespace Eco.Mods.WorldEdit.Model
{
	internal interface IDataTypeContractAttribute<TDiscriminator> where TDiscriminator : struct, Enum
	{
		TDiscriminator Discriminator { get; }
	}
}
