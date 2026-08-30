using Eco.Gameplay.Components;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	[WorldObjectComponentContract(WorldObjectComponentType.Door)]
	internal sealed record DoorComponentData : IWorldObjectComponentData
	{
		public bool OpensOut { get; init; }

		[JsonConstructor]
		public DoorComponentData(bool opensOut)
		{
			this.OpensOut = opensOut;
		}

		public static DoorComponentData? Create(TechTree.DoorObject door)
		{
			DoorComponent doorComponent = door.GetComponent<DoorComponent>();
			if (doorComponent is null) return null;

			return new DoorComponentData(doorComponent.OpensOutwards);
		}
	}
}
