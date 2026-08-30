using System.Numerics;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Shared.Utils;
using Eco.World.Blocks;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model
{
	internal sealed record WorldEditBlock
	{
		public Type BlockType { get; init; }
		[JsonProperty("Position")] public Vector3 LocalPosition { get; init; }
		public IBlockData? BlockData { get; init; }
		public string? Color { get; init; }

		[JsonConstructor]
		public WorldEditBlock(Type blockType, Vector3 localPosition, IBlockData? blockData, string? color)
		{
			this.BlockType = blockType ?? throw new ArgumentNullException(nameof(blockType));
			this.LocalPosition = localPosition;
			this.BlockData = blockData;
			this.Color = color;
		}

		public bool IsPlantBlock() => this.BlockType.DerivesFrom<PlantBlock>() || this.BlockType.DerivesFrom<TreeBlock>();
		public bool IsWorldObjectBlock() => this.BlockType.DerivesFrom<WorldObjectBlock>();

		/// <summary>Classifies the serialized payload, independently of the external game block type.</summary>
		public bool IsBlockInternally() => this.BlockData is null;
		public bool IsPlantInternally() => this.BlockData is PlantBlockData;
		public bool IsWorldObjectInternally() => this.BlockData is WorldObjectBlockData;
		public bool IsSupportedInternally() => this.IsBlockInternally() || this.IsPlantInternally() || this.IsWorldObjectInternally();

		public bool IsEmptyBlock()
		{
			if (this.BlockType.Equals(typeof(EmptyBlock))) { return true; }
			else if (this.IsWorldObjectInternally() && this.BlockData is WorldObjectBlockData objectData && objectData.WorldObjectType.Equals(typeof(EmptyBlock))) { return true; }
			return false;
		}
	}
}
