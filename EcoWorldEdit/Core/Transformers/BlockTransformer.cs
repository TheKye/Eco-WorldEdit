using System.Numerics;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.World.Blocks;

using EcoQuaternion = Eco.Shared.Math.Quaternion;

namespace Eco.Mods.WorldEdit.Core.Transformers
{
	internal static class BlockTransformer
	{
		public static WorldEditBlock Rotate(WorldEditBlock block, Matrix4x4 structuralTransform, float exactAngle, int quarterTurns)
		{
			ArgumentNullException.ThrowIfNull(block);
			Vector3 localPosition = Vector3.Transform(block.LocalPosition, structuralTransform);
			bool isWorldObject = block.BlockData is WorldObjectBlockData;
			if (!isWorldObject)
			{
				if (!BlockUtils.IsVoxelAligned(block.LocalPosition))
					throw new WorldEditCommandException($"Clipboard block {block.BlockType} at {block.LocalPosition} is not aligned to the voxel grid.");
				localPosition = BlockUtils.RoundToVoxel(localPosition);
			}

			return block with
			{
				LocalPosition = localPosition,
				BlockType = RotateBlockType(block, quarterTurns),
				BlockData = RotateBlockData(block.BlockData, exactAngle)
			};
		}

		private static Type RotateBlockType(WorldEditBlock block, int quarterTurns)
		{
			if (quarterTurns == 0 || block.BlockData is not null || block.BlockType == typeof(EmptyBlock)) return block.BlockType;
			if (BlockUtils.TryGetRotatedVariant(block.BlockType, quarterTurns, out Type rotatedType)) return rotatedType;
			throw new WorldEditCommandException($"Unable to select the rotated variant of block {block.BlockType}.");
		}

		private static IBlockData? RotateBlockData(IBlockData? blockData, float exactAngle)
		{
			if (blockData is not WorldObjectBlockData objectData) return blockData;

			float radians = exactAngle * (MathF.PI / 180f);
			float halfAngle = radians / 2f;
			EcoQuaternion yawRotation = new(0f, MathF.Sin(halfAngle), 0f, MathF.Cos(halfAngle));
			EcoQuaternion rotation = objectData.Rotation * yawRotation;
			if (!EcoQuaternion.IsValid(rotation)) throw new WorldEditCommandException($"WorldObject {objectData.WorldObjectType} has an invalid rotation.");
			return objectData with { Rotation = rotation };
		}
	}
}
