using Eco.Gameplay.Economy;
using Eco.Gameplay.Items.PersistentData;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	[WorldObjectComponentContract(WorldObjectComponentType.Mint)]
	internal sealed record MintComponentData : IWorldObjectComponentData
	{
		public Type BackingItem { get; init; }
		public float CoinsPerItem { get; init; }
		public string Name { get; init; }

		[JsonConstructor]
		public MintComponentData(Type backingItem, float coinsPerItem, string name)
		{
			this.BackingItem = backingItem;
			this.CoinsPerItem = coinsPerItem;
			this.Name = name;
		}

		public static MintComponentData? Create(MintItemData mintItemData)
		{
			if (mintItemData.Currency is null || mintItemData.Currency.BackingItem is null) return null;

			Currency currency = mintItemData.Currency;
			Type backingItemType = currency.BackingItem.GetType();
			float coinsPerItem = currency.CoinsPerItem;
			string name = currency.Name;
			return new MintComponentData(backingItemType, coinsPerItem, name);
		}

		public Currency? GetCurrency()
		{
			Type type = this.BackingItem;
			Currency? currency = CurrencyManager.Currencies.FirstOrDefault(c => c.BackingItem != null && c.BackingItem.Type.Equals(type));
			return currency;
		}
	}
}
