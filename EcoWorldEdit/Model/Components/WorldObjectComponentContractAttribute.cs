namespace Eco.Mods.WorldEdit.Model.Components
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	internal sealed class WorldObjectComponentContractAttribute : Attribute, IDataTypeContractAttribute<WorldObjectComponentType>
	{
		public WorldObjectComponentType Discriminator { get; }

		public WorldObjectComponentContractAttribute(WorldObjectComponentType discriminator)
		{
			this.Discriminator = discriminator;
		}
	}
}
