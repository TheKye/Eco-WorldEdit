using Eco.Gameplay.Components;
using Eco.Gameplay.Items;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Eco.Mods.WorldEdit.Model.Components.StoreComponentData;

namespace Eco.Mods.WorldEdit.Model.Components
{
	internal struct StoreComponentData
	{
		public List<Category> Sell { get; private set; }
		public List<Category> Buy { get; private set; }

		public static StoreComponentData Create(StoreComponent storeComponent)
		{
			StoreComponentData data = new StoreComponentData
			{
				Sell = storeComponent.StoreData.SellCategories.Select(c => Category.Create(c)).ToList(),
				Buy = storeComponent.StoreData.BuyCategories.Select(c => Category.Create(c)).ToList()
			};
			return data;
		}

		[JsonConstructor]
		public StoreComponentData(List<Category> sell, List<Category> buy)
		{
			this.Sell = sell;
			this.Buy = buy;
		}

		internal struct Offer
		{
			public InventoryStack Stack { get; private set; }
			public float Price { get; private set; }
			public int Limit { get; private set; }
			public float MinDurability { get; private set; } = -1;
			public float MaxDurability { get; private set; } = -1;
			public bool IsBuying { get; private set; }

			public static Offer Create(TradeOffer tradeOffer)
			{
				Offer offer = new Offer
				{
					Stack = new InventoryStack(tradeOffer.Stack.Item.Type, tradeOffer.Stack.Quantity), //Unable use .Create(tradeOffer) because it have empty check and TradeOffer can have empty stack
					Price = tradeOffer.Price,
					Limit = tradeOffer.Limit,
					MinDurability = tradeOffer.MinDurability,
					MaxDurability = tradeOffer.MaxDurability,
					IsBuying = tradeOffer.Buying
				};
				return offer;
			}

			public TradeOffer GetTradeOffer()
			{
				TradeOffer tradeOffer = new TradeOffer(this.Stack.GetItemStack().Item, this.Price, this.IsBuying)
				{
					Limit= this.Limit,
					MinDurability = this.MinDurability,
					MaxDurability = this.MaxDurability
				};
				return tradeOffer;
			}

			[JsonConstructor]
			public Offer(InventoryStack stack, float price, int limit, float minDurability, float maxDurability, bool isBuying)
			{
				this.Stack = stack;
				this.Price = price;
				this.Limit = limit;
				this.MinDurability = minDurability;
				this.MaxDurability = maxDurability;
				this.IsBuying = isBuying;
			}
		}

		internal struct Category
		{
			public string Name { get; private set; }
			public string GeneratedName { get; private set; }
			public bool IsBuying { get; private set; }
			public List<Offer> Offers { get; private set; }

			public static Category Create(StoreCategory storeCategory)
			{
				Category category = new Category
				{
					Name = storeCategory.Name,
					GeneratedName = storeCategory.GeneratedName,
					IsBuying = storeCategory.IsBuy,
					Offers = storeCategory.Offers.Select(o => Offer.Create(o)).ToList(),
				};
				return category;
			}

			public StoreCategory GetStoreCategory(StoreComponent store)
			{
				StoreCategory storeCategory = new StoreCategory(store)
				{
					Name = this.Name,
					GeneratedName = this.GeneratedName,
					IsBuy = this.IsBuying
				};
				this.Offers.ForEach(o => storeCategory.Offers.Add(o.GetTradeOffer()));
				return storeCategory;
			}

			[JsonConstructor]
			public Category(string name, string generatedName, bool isBuying, List<Offer> offers)
			{
				this.Name = name;
				this.GeneratedName = generatedName;
				this.IsBuying = isBuying;
				this.Offers = offers;
			}
		}
	}
}
