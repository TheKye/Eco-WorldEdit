using Eco.Core.Utils;
using Eco.Gameplay.Components;
using Eco.Gameplay.Items;
using Eco.Gameplay.Modules;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Shared.Logging;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	[WorldObjectComponentContract(WorldObjectComponentType.PluginModules)]
	internal sealed record PluginModulesData : IWorldObjectComponentData
	{
		public IReadOnlyList<PluginModuleSlotData> Modules { get; init; }

		[JsonConstructor]
		public PluginModulesData(List<PluginModuleSlotData> modules)
		{
			this.Modules = modules ?? throw new ArgumentNullException(nameof(modules));
		}

		public static PluginModulesData? Create(PluginModulesComponent component)
		{
			if (component.Inventory is null) return null;

			List<PluginModuleSlotData> modules = component.Inventory.AllSlotsByTag
				.OrderBy(pair => pair.Key, StringComparer.Ordinal)
				.Select(pair => pair.Value.Stacks.FirstOrDefault()?.Item is PluginModule module
					? new PluginModuleSlotData(pair.Key, module.Type)
					: null)
				.OfType<PluginModuleSlotData>()
				.ToList();

			return modules.Count == 0 ? null : new PluginModulesData(modules);
		}

		public void Restore(PluginModulesComponent component, User user, CancellationToken ct)
		{
			if (component.Inventory is not { } inventory)
			{
				Log.WriteWarningLineLoc($"Skipped restoring plugin modules to {component.Parent.MarkedUpName}: the component has no initialized plugin module inventory.");
				return;
			}

			bool changed = false;
			try
			{
				foreach (PluginModuleSlotData moduleData in this.Modules)
				{
					ct.ThrowIfCancellationRequested();
					Inventory? slot = inventory.GetSlot(moduleData.SlotTag);
					if (slot is null)
					{
						Log.WriteWarningLineLoc($"Skipped restoring {moduleData.ModuleType.Name} to {component.Parent.MarkedUpName}: plugin module slot '{moduleData.SlotTag}' is not available.");
						continue;
					}

					Item module = Item.Get(moduleData.ModuleType);
					Result canCreate = StrangeItemProtection.CanCreateItem(user, module);
					Result result = canCreate.Success ? slot.TryAddItemsNonUnique(moduleData.ModuleType, 1) : canCreate;
					if (result.Failed)
					{
						user.Player.Error(result.Message);
						continue;
					}

					StrangeItemProtection.IncrementUsedItem(user, module);
					changed = true;
				}
			}
			finally
			{
				if (changed) component.RefreshAfterExternalSlotWrite();
			}
		}
	}

	internal sealed record PluginModuleSlotData
	{
		public string SlotTag { get; init; }
		public Type ModuleType { get; init; }

		[JsonConstructor]
		public PluginModuleSlotData(string slotTag, Type moduleType)
		{
			if (string.IsNullOrWhiteSpace(slotTag)) throw new ArgumentException("Plugin module slot tag cannot be empty.", nameof(slotTag));
			ArgumentNullException.ThrowIfNull(moduleType);
			if (!typeof(PluginModule).IsAssignableFrom(moduleType)) throw new ArgumentException($"Type {moduleType} is not a {nameof(PluginModule)}.", nameof(moduleType));

			this.SlotTag = slotTag;
			this.ModuleType = moduleType;
		}
	}
}
