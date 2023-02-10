using System;
using System.Collections.Generic;
using Eco.Gameplay.Components;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Mods.WorldEdit.Model.Components;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Shared.Math;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model
{
	internal struct WorldEditWorldObjectBlockData : IWorldEditBlockData
	{
		public Type WorldObjectType { get; private set; }
		[JsonConverter(typeof(JsonQuaternionConverter))] public Quaternion Rotation { get; private set; }
		public string Name { get; private set; }
		public Dictionary<Type, Object> Components { get; private set; }

		public static WorldEditWorldObjectBlockData From(WorldObject worldObject)
		{
			WorldEditWorldObjectBlockData worldObjectData = new WorldEditWorldObjectBlockData();

			worldObjectData.WorldObjectType = worldObject.GetType();
			worldObjectData.Rotation = worldObject.Rotation;
			worldObjectData.Name = worldObject.Name;

			worldObjectData.Components = new Dictionary<Type, Object>();
			if (worldObject.HasComponent<StorageComponent>())
			{
				StorageComponent storageComponent = worldObject.GetComponent<StorageComponent>();
				List<InventoryStack> inventoryStacks = new List<InventoryStack>();

				foreach (ItemStack stack in storageComponent.Inventory.Stacks)
				{
					if (stack.Empty()) continue;
					inventoryStacks.Add(InventoryStack.Create(stack));
				}

				worldObjectData.Components.Add(typeof(StorageComponent), inventoryStacks);
			}

			if (worldObject.HasComponent<CustomTextComponent>())
			{
				CustomTextComponent textComponent = worldObject.GetComponent<CustomTextComponent>();
				worldObjectData.Components.Add(typeof(CustomTextComponent), textComponent.TextData.Text);
			}

			if (worldObject.HasComponent<MintComponent>())
			{
				MintComponent mintComponent = worldObject.GetComponent<MintComponent>();
				worldObjectData.Components.Add(typeof(MintComponent), MintDataCurrency.Create(mintComponent.MintData));
			}
			if (worldObject is TechTree.DoorObject doorObject)
			{
				worldObjectData.Components.Add(typeof(TechTree.DoorObject), DoorComponent.Create(doorObject));
			}
			if (worldObject.HasComponent<StoreComponent>())
			{
				StoreComponent storeComponent = worldObject.GetComponent<StoreComponent>();
				worldObjectData.Components.Add(typeof(StoreComponent), StoreComponentData.Create(storeComponent));
			}

			return worldObjectData;
		}

		[JsonConstructor]
		public WorldEditWorldObjectBlockData(Type worldObjectType, Quaternion rotation, string name, Dictionary<Type, Object> components)
		{
			this.WorldObjectType = worldObjectType ?? throw new ArgumentNullException(nameof(worldObjectType));
			this.Rotation = rotation;
			this.Components = components;
			this.Name= name;
		}

		public void SetRotation(Quaternion rot)
		{
			this.Rotation = rot;
		}

		public IWorldEditBlockData Clone()
		{
			return new WorldEditWorldObjectBlockData(this.WorldObjectType, this.Rotation, this.Name, new Dictionary<Type, Object>(this.Components));
		}
	}
}
