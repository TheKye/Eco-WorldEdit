using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.StrangeCloudGameplay;
using Eco.Gameplay.Systems.EcoMarketplace;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eco.Mods.WorldEdit
{
	internal class StrangeItemProtection : Singleton<StrangeItemProtection>
	{

		public void Initialize() { }

		public static Result CanCreateBlock(User user, Type blockType)
		{
			IEnumerable<(Shared.StrangeCloudShared.StrangeItemInWorld PaidItems, int CountCreated)> paidItemsForUser = user.StrangeItemManagement.GetPaidAndCreated();
			if (BlockItem.FirstCreatingItem(blockType) is { } item && item.IsPaidItem())
			{
				string itemName = item.Name.TrimEndString("Item");
				(Shared.StrangeCloudShared.StrangeItemInWorld PaidItems, int CountCreated)? paidItem = paidItemsForUser.FirstOrNull(x => x.PaidItems.Type == itemName);
				if (paidItem is null) { return Result.FailLoc($"You do not own the blueprint needed for block {blockType.Name}."); }
				if (paidItem.Value.CountCreated >= paidItem.Value.PaidItems.AllowedQuantity) { return Result.FailLoc($"You cannot create anymore of this block unless you buy more blueprints."); }
			}

			return Result.Succeeded;
		}

		public static Result CanCreateItem(User user, Item item) => CanCreateItem(user, item, 1);
		public static Result CanCreateItem(User user, Item item, int amount)
		{
			IEnumerable<(Shared.StrangeCloudShared.StrangeItemInWorld PaidItems, int CountCreated)> paidItemsForUser = user.StrangeItemManagement.GetPaidAndCreated();
			if (item.IsPaidItem())
			{
				string itemName = item.Name.TrimEndString("Item");
				(Shared.StrangeCloudShared.StrangeItemInWorld PaidItems, int CountCreated)? paidItem = paidItemsForUser.FirstOrNull(x => x.PaidItems.Type == itemName);
				if (paidItem is null) { return Result.FailLoc($"You do not own the blueprint needed for create item {item.Name}."); }
				if (paidItem.Value.CountCreated >= paidItem.Value.PaidItems.AllowedQuantity) { return Result.FailLoc($"You cannot create anymore of item {item.Name} unless you buy more blueprints."); }
				if (paidItem.Value.CountCreated + amount > paidItem.Value.PaidItems.AllowedQuantity) { return Result.FailLoc($"You cannot create {amount} of item {item.Name} unless you buy more blueprints."); }
			}

			return Result.Succeeded;
		}

		public static void IncrementUsedBlock(User user, Type blockType)
		{
			if (BlockItem.FirstCreatingItem(blockType) is { } item && item.IsPaidItem())
			{
				IncrementUsedItem(user, item);
			}
		}
		public static void IncrementUsedItem(User user, Item item) => IncrementUsedItem(user, item, 1);
		public static void IncrementUsedItem(User user, Item item, int amount)
		{
			if (!item.IsPaidItem()) return;
			string itemName = item.Name.TrimEndString("Item");
			//TODO: THIS IS NOT THREADSAFE!!!
			int currenAmount = user.StrangeItemManagement.NameToCountCollected.GetOr(itemName, 0);
			currenAmount += amount;
			user.StrangeItemManagement.NameToCountCollected[itemName] = currenAmount;
		}
	}
}
