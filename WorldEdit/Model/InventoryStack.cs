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

		[JsonConstructor]
		public InventoryStack(Type itemType, int quantity)
		{
			this.ItemType = itemType;
			this.Quantity = quantity;
		}
	}
}
