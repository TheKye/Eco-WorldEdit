using System.Collections.Concurrent;
using System.Numerics;
using Eco.Gameplay.Blocks;
using Eco.Gameplay.Plants;
using Eco.Shared.Math;
using Eco.World;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class BlockUtils
	{
		private const float BlockPositionTolerance = 0.0001f;
		private static readonly ConcurrentDictionary<Type, Type[]> RotatedVariantCache = new();

		public static bool IsPlantBlock(Block block)
		{
			ArgumentNullException.ThrowIfNull(block);
			return block is PlantBlock or TreeBlock;
		}

		public static bool TryGetBlockPosition(Vector3 position, out Vector3i blockPosition)
		{
			blockPosition = (Vector3i)position;
			return IsBlockPositionAligned(position, blockPosition);
		}

		public static bool IsVoxelAligned(Vector3 position) => IsBlockPositionAligned(position, RoundToVoxel(position));

		public static Vector3 RoundToVoxel(Vector3 position) => new(
			MathF.Round(position.X, MidpointRounding.AwayFromZero),
			MathF.Round(position.Y, MidpointRounding.AwayFromZero),
			MathF.Round(position.Z, MidpointRounding.AwayFromZero));

		public static Type? GetBlockType(string blockName)
		{
			Type? blockType = null;
			if (string.IsNullOrWhiteSpace(blockName)) return blockType;
			blockName = blockName.Replace(" ", "");

			if (blockName.Equals("air", StringComparison.InvariantCultureIgnoreCase) || blockName.Equals("empty", StringComparison.InvariantCultureIgnoreCase)) return typeof(EmptyBlock);

			if (TryGetBlockType(blockName + "floorblock", out blockType)) return blockType;
			if (TryGetBlockType(blockName + "block", out blockType)) return blockType;
			_ = TryGetBlockType(blockName, out blockType); //Last we check for correct full name given
			return blockType;
		}

		public static bool TryGetBlockType(string blockName, out Type? type)
		{
			type = null;
			if (string.IsNullOrWhiteSpace(blockName)) return false;
			type = BlockManager.BlockTypes.FirstOrDefault(t => t.Name.Equals(blockName, StringComparison.InvariantCultureIgnoreCase));
			return type is not null;
		}

		public static bool TryGetRotatedVariant(Type blockType, int quarterTurns, out Type rotatedType)
		{
			ArgumentNullException.ThrowIfNull(blockType);
			quarterTurns = ((quarterTurns % 4) + 4) % 4;
			if (quarterTurns == 0)
			{
				rotatedType = blockType;
				return true;
			}

			Type[] variants = RotatedVariantCache.GetOrAdd(blockType, FindRotatedVariants);
			if (variants.Length <= 1)
			{
				rotatedType = blockType;
				return true;
			}

			if (TryGetNamedRotation(blockType, out string baseName, out int currentAngle))
			{
				int wantedAngle = (currentAngle + quarterTurns * 90) % 360;
				Type? namedVariant = variants.FirstOrDefault(candidate =>
					TryGetNamedRotation(candidate, out string candidateBase, out int candidateAngle) &&
					candidateBase.Equals(baseName, StringComparison.Ordinal) &&
					candidateAngle == wantedAngle);
				if (namedVariant is not null)
				{
					rotatedType = namedVariant;
					return true;
				}
			}

			// Eco exposes the variants as an ordered array. Keep this as a fallback for
			// block families whose generated type names do not include their angle.
			int currentIndex = Array.IndexOf(variants, blockType);
			if (currentIndex >= 0 && variants.Length == 4)
			{
				rotatedType = variants[(currentIndex + quarterTurns) % variants.Length];
				return true;
			}

			rotatedType = blockType;
			return false;
		}

		private static Type[] FindRotatedVariants(Type blockType)
		{
			Type[]? directVariants = Block.Get<RotatedVariants>(blockType)?.Variants;
			if (directVariants is { Length: > 0 })
			{
				Type[] variants = directVariants.Contains(blockType) ? directVariants : [blockType, .. directVariants];
				foreach (Type variant in variants) RotatedVariantCache.TryAdd(variant, variants);
				return variants;
			}

			foreach (BlockForm? form in BlockFormManager.Data.BlockForms)
			{
				if (!form.BlockTypes.Contains(blockType)) continue;
				Type[] variants = form.BlockTypes;
				if (!variants.Any(type => Block.Get<RotatedVariants>(type)?.Variants is { Length: > 0 })) break;
				foreach (Type variant in variants) RotatedVariantCache.TryAdd(variant, variants);
				return variants;
			}

			return [blockType];
		}

		private static bool TryGetNamedRotation(Type blockType, out string baseName, out int angle)
		{
			const string blockSuffix = "Block";
			string name = blockType.Name;
			string stem = name.EndsWith(blockSuffix, StringComparison.Ordinal) ? name[..^blockSuffix.Length] : name;
			int digitStart = stem.Length;
			while (digitStart > 0 && char.IsAsciiDigit(stem[digitStart - 1])) digitStart--;

			baseName = stem[..digitStart];
			if (digitStart == stem.Length)
			{
				angle = 0;
				return true;
			}

			return int.TryParse(stem.AsSpan(digitStart), out angle) && angle is >= 0 and < 360 && angle % 90 == 0;
		}

		private static bool IsBlockPositionAligned(Vector3 position, Vector3 blockPosition) =>
			Vector3.DistanceSquared(position, blockPosition) <= BlockPositionTolerance * BlockPositionTolerance;
	}
}
