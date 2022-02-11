using System;
using System.Linq;
using Eco.Gameplay.Economy;
using Eco.Gameplay.Items.PersistentData;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	internal struct DoorComponent
	{
		public bool OpensOut { get; private set; }

		public static DoorComponent Create(TechTree.DoorObject door)
		{
			DoorComponent component = new DoorComponent();
			if (door != null)
			{
				component.OpensOut = door.OpensOut;
			}
			return component;
		}

		[JsonConstructor]
		public DoorComponent(bool opensOut)
		{
			this.OpensOut = opensOut;
		}
	}
}
