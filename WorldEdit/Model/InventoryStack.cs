using System;
using Eco.Gameplay.Items;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model
{
	internal struct InventoryStack
	{
		public Type ItemType { get; private set; }
		public int Quantity { get; private set; }

		public static InventoryStack Create(ItemStack itemStack)
		{
			InventoryStack inventoryStack = new InventoryStack();
			if (!itemStack.Empty())
			{
				inventoryStack.ItemType = itemStack.Item.Type;
				inventoryStack.Quantity = itemStack.Quantity;
			}
			return inventoryStack;
		}

		public ItemStack GetItemStack()
		{
			ItemStack itemStack = new ItemStack(this.ItemType, this.Quantity);
			int maxStack = Item.GetMaxStackSize(itemStack.Item?.Type);
			if (itemStack.Quantity > maxStack) itemStack.Quantity = maxStack;
			return itemStack;
		}

		[JsonConstructor]
		public InventoryStack(Type itemType, int quantity)
		{
			this.ItemType = itemType;
			this.Quantity = quantity;
		}
	}
}
