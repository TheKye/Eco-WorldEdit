using Eco.Gameplay.Components.Storage;
using Eco.Shared.Utils;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	[WorldObjectComponentContract(WorldObjectComponentType.Storage)]
	internal sealed record StorageComponentData : IWorldObjectComponentData
	{
		public IReadOnlyList<InventoryStackData> InventoryStacks { get; }

		[JsonConstructor]
		public StorageComponentData(List<InventoryStackData> inventoryStacks)
		{
			this.InventoryStacks = inventoryStacks;
		}

		public static StorageComponentData? Create(StorageComponent storageComponent)
		{
			List<InventoryStackData> inventoryStacks = storageComponent.Inventory.Stacks.Select(x => InventoryStackData.Create(x)).NonNull().ToList();
			return new StorageComponentData(inventoryStacks);
		}
	}
}
