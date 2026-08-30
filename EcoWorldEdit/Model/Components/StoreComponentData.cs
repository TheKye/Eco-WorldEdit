using Eco.Gameplay.Components;
using Eco.Gameplay.Components.Store;
using Eco.Gameplay.Components.Store.Internal;
using Eco.Shared.Utils;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	[WorldObjectComponentContract(WorldObjectComponentType.Store)]
	internal sealed record StoreComponentData : IWorldObjectComponentData
	{
		public IReadOnlyList<StoreCategoryData> Sell { get; init; }
		public IReadOnlyList<StoreCategoryData> Buy { get; init; }

		[JsonConstructor]
		public StoreComponentData(List<StoreCategoryData> sell, List<StoreCategoryData> buy)
		{
			this.Sell = sell;
			this.Buy = buy;
		}

		public static StoreComponentData? Create(StoreComponent storeComponent)
		{
			List<StoreCategoryData> sell = storeComponent.StoreData.SellCategories.Select(c => StoreCategoryData.Create(c)).NonNull().ToList();
			List<StoreCategoryData> buy = storeComponent.StoreData.BuyCategories.Select(c => StoreCategoryData.Create(c)).NonNull().ToList();

			return new StoreComponentData(sell, buy);
		}
	}

	internal sealed record StoreCategoryData
	{
		public string Name { get; init; }
		public string GeneratedName { get; init; }
		public bool IsBuying { get; init; }
		public IReadOnlyList<StoreOfferData> Offers { get; init; }

		[JsonConstructor]
		public StoreCategoryData(string name, string generatedName, bool isBuying, List<StoreOfferData> offers)
		{
			this.Name = name;
			this.GeneratedName = generatedName;
			this.IsBuying = isBuying;
			this.Offers = offers;
		}

		public static StoreCategoryData? Create(StoreCategory storeCategory)
		{
			List<StoreOfferData> offers = storeCategory.Offers.Select(o => StoreOfferData.Create(o)).NonNull().ToList();
			return new StoreCategoryData(storeCategory.Name, storeCategory.GeneratedName, storeCategory.IsBuy, offers);
		}

		public StoreCategory GetStoreCategory(StoreComponent store, bool isBuy, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();
			StoreCategory storeCategory = new StoreCategory(store, isBuy)
			{
				Name = this.Name,
				GeneratedName = this.GeneratedName,
				IsBuy = this.IsBuying
			};
			foreach (StoreOfferData offer in this.Offers)
			{
				ct.ThrowIfCancellationRequested();
				storeCategory.Offers.Add(offer.GetTradeOffer());
			}
			return storeCategory;
		}
	}

	internal sealed record StoreOfferData
	{
		public InventoryStackData Stack { get; init; }
		public float Price { get; init; }
		public int Limit { get; init; }
		public float MinDurability { get; init; }
		public float MaxDurability { get; init; }
		public bool IsBuying { get; init; }

		[JsonConstructor]
		public StoreOfferData(InventoryStackData stack, float price, int limit, float minDurability, float maxDurability, bool isBuying)
		{
			this.Stack = stack;
			this.Price = price;
			this.Limit = limit;
			this.MinDurability = minDurability;
			this.MaxDurability = maxDurability;
			this.IsBuying = isBuying;
		}

		public static StoreOfferData? Create(TradeOffer tradeOffer)
		{
			InventoryStackData stack = new InventoryStackData(tradeOffer.Stack.Item.Type, tradeOffer.Stack.Quantity); //Unable use .Create(tradeOffer.Stack) because it have empty check and TradeOffer can have empty stack
			return new StoreOfferData(stack, tradeOffer.Price, tradeOffer.Limit, tradeOffer.MinDurability, tradeOffer.MaxDurability, tradeOffer.Buying);
		}

		public TradeOffer GetTradeOffer()
		{
			TradeOffer tradeOffer = new TradeOffer(this.Stack.GetItemStack().Item, this.Price, this.IsBuying)
			{
				Limit = this.Limit,
				MinDurability = this.MinDurability,
				MaxDurability = this.MaxDurability
			};
			return tradeOffer;
		}
	}
}
