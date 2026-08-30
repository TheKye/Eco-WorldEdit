using Eco.Gameplay.Items;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	internal sealed record InventoryStackData
	{
		public Type ItemType { get; init; }
		public int Quantity { get; init; }

		[JsonConstructor]
		public InventoryStackData(Type itemType, int quantity)
		{
			this.ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
			this.Quantity = quantity;
		}

		public static InventoryStackData? Create(ItemStack itemStack)
		{
			if (itemStack.Empty())
			{
				return null;
			}
			return new InventoryStackData(itemStack.Item.Type, itemStack.Quantity);
		}

		public ItemStack GetItemStack()
		{
			ItemStack itemStack = new ItemStack(this.ItemType, this.Quantity);
			int maxStack = Item.GetMaxStackSize(itemStack.Item?.Type);
			if (itemStack.Quantity > maxStack) itemStack.Quantity = maxStack;
			return itemStack;
		}
	}
}
