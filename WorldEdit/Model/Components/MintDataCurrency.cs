using System;
using System.Linq;
using Eco.Gameplay.Economy;
using Eco.Gameplay.Items.PersistentData;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	internal struct MintDataCurrency
	{
		public Type BackingItem { get; private set; }
		public float CoinsPerItem { get; private set; }
		public string Name { get; private set; }

		public static MintDataCurrency Create(MintItemData mintItemData)
		{
			MintDataCurrency mintDataCurrency = new MintDataCurrency();
			if (mintItemData.Currency != null && mintItemData.Currency.BackingItem != null)
			{
				Currency currency = mintItemData.Currency;
				mintDataCurrency.BackingItem = currency.BackingItem.GetType();
				mintDataCurrency.CoinsPerItem = currency.CoinsPerItem;
				mintDataCurrency.Name = currency.Name;
			}
			return mintDataCurrency;
		}

		public Currency GetCurrency()
		{
			Currency currency = null;
			Type type = this.BackingItem;
			currency = CurrencyManager.Currencies.FirstOrDefault(c => c.BackingItem != null && c.BackingItem.Type.Equals(type));
			return currency;
		}

		[JsonConstructor]
		public MintDataCurrency(Type backingItem, float coinsPerItem, string name)
		{
			this.BackingItem = backingItem;
			this.CoinsPerItem = coinsPerItem;
			this.Name = name;
		}
	}
}
