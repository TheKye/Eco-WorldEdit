using Eco.Mods.WorldEdit.Model.Components;
using Eco.Shared.Math;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.BlockData
{
	[BlockDataContract(BlockDataType.WorldObject)]
	internal sealed record WorldObjectBlockData : IBlockData
	{
		public Type WorldObjectType { get; init; }
		public Quaternion Rotation { get; init; }

		/// <summary>Blueprint-local identifier. New captures assign it to every object.</summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Guid? ObjectId { get; init; }
		/// <summary>Blueprint-local identifier of the containing object.</summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Guid? ParentId { get; init; }

		public string Name { get; init; }
		public IReadOnlyList<IWorldObjectComponentData> Components { get; init; }

		[JsonConstructor]
		public WorldObjectBlockData(Type worldObjectType, Quaternion rotation, Guid? objectId, Guid? parentId, string name, List<IWorldObjectComponentData> components)
		{
			this.WorldObjectType = worldObjectType ?? throw new ArgumentNullException(nameof(worldObjectType));
			this.Rotation = rotation;
			this.ObjectId = objectId;
			this.ParentId = parentId;
			this.Name = name;
			this.Components = components;
		}
	}
}
