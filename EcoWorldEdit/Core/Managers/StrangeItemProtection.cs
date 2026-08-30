using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.EcoMarketplace;
using Eco.Shared;
using Eco.Shared.Utils;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	internal static class StrangeItemProtection
	{
		public static Result CanCreateBlock(User user, Type blockType, int amount = 1)
		{
			IEnumerable<(Shared.StrangeCloudShared.StrangeItemInWorld PaidItems, int CountCreated)> paidItems = user.StrangeItemManagement.GetPaidAndCreated();
			BlockItem? item = BlockItem.FirstCreatingItem(blockType);
			if (item is null || !item.IsPaidItem()) return Result.Succeeded;

			string itemName = item.Name.TrimEndString("Item");
			(Shared.StrangeCloudShared.StrangeItemInWorld PaidItems, int CountCreated)? paidItem = paidItems.FirstOrNull(x => x.PaidItems.Type == itemName);
			if (paidItem is null) return Result.FailLoc($"You do not own the blueprint needed for block {blockType.Name}.");
			if (paidItem.Value.CountCreated + amount > paidItem.Value.PaidItems.AllowedQuantity) return Result.FailLoc($"You cannot create any more of this block unless you buy more blueprints.");
			return Result.Succeeded;
		}

		public static Result CanCreateItem(User user, Item item, int amount = 1)
		{
			if (!item.IsPaidItem()) return Result.Succeeded;

			IEnumerable<(Shared.StrangeCloudShared.StrangeItemInWorld PaidItems, int CountCreated)> paidItems = user.StrangeItemManagement.GetPaidAndCreated();
			string itemName = item.Name.TrimEndString("Item");
			(Shared.StrangeCloudShared.StrangeItemInWorld PaidItems, int CountCreated)? paidItem = paidItems.FirstOrNull(x => x.PaidItems.Type == itemName);
			if (paidItem is null) return Result.FailLoc($"You do not own the blueprint needed to create item {item.Name}.");
			if (paidItem.Value.CountCreated + amount > paidItem.Value.PaidItems.AllowedQuantity) return Result.FailLoc($"You cannot create {amount} of item {item.Name} unless you buy more blueprints.");
			return Result.Succeeded;
		}

		public static void IncrementUsedBlock(User user, Type blockType)
		{
			if (BlockItem.FirstCreatingItem(blockType) is { } item) IncrementUsedItem(user, item);
		}

		public static void IncrementUsedItem(User user, Item item, int amount = 1)
		{
			if (!item.IsPaidItem()) return;
			lock (user.StrangeItemManagement.TypeToCountCollected)
			{
				int currentAmount = user.StrangeItemManagement.TypeToCountCollected.GetOr(item.Type, 0);
				user.StrangeItemManagement.TypeToCountCollected[item.Type] = currentAmount + amount;
			}
		}

		public static void DecrementUsed(User user, Block block)
		{
			if (block is WorldObjectBlock worldObjectBlock)
			{
				DecrementUsedItem(user, WorldObjectItem.GetCreatingItemTemplateFromType(worldObjectBlock.WorldObjectHandle.Object.GetType()));
				return;
			}

			if (BlockItem.FirstCreatingItem(block.GetType()) is { } item) DecrementUsedItem(user, item);
		}

		public static void DecrementUsedItem(User user, Item item, int amount = 1)
		{
			if (!item.IsPaidItem()) return;
			lock (user.StrangeItemManagement.TypeToCountCollected)
			{
				int currentAmount = user.StrangeItemManagement.TypeToCountCollected.GetOr(item.Type, 0);
				user.StrangeItemManagement.TypeToCountCollected[item.Type] = Mathf.Max(currentAmount - amount, 0);
			}
		}
	}
}
