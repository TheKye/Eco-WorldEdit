using System.Numerics;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Occupancy;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Utils.Eco;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	internal sealed partial class BlockManager
	{
		public PlacementHeightRange? GetPlacementHeightRange(IReadOnlyList<WorldEditBlock> snapshot, bool skipEmpty, CancellationToken ct)
		{
			ArgumentNullException.ThrowIfNull(snapshot);
			return CalculatePlacementHeightRange(snapshot, skipEmpty, ct);
		}

		public PlacementHeightRange? GetPlacementHeightRange(IReadOnlyList<WorldEditBlock> blocks, IReadOnlyList<WorldEditBlock> plants, IReadOnlyList<WorldEditBlock> worldObjects, bool skipEmpty, CancellationToken ct)
		{
			ArgumentNullException.ThrowIfNull(blocks);
			ArgumentNullException.ThrowIfNull(plants);
			ArgumentNullException.ThrowIfNull(worldObjects);

			PlacementHeightRange? range = CalculatePlacementHeightRange(blocks, skipEmpty, ct);
			range = Union(range, CalculatePlacementHeightRange(plants, skipEmpty, ct));
			range = Union(range, CalculatePlacementHeightRange(worldObjects, skipEmpty, ct));
			return range;
		}

		private static PlacementHeightRange? CalculatePlacementHeightRange(IReadOnlyList<WorldEditBlock> blocks, bool skipEmpty, CancellationToken ct)
		{
			PlacementHeightRange? range = null;
			foreach (WorldEditBlock block in blocks)
			{
				ct.ThrowIfCancellationRequested();
				if (skipEmpty && block.IsEmptyBlock()) continue;

				if (block.BlockData is WorldObjectBlockData objectData && typeof(WorldObject).IsAssignableFrom(objectData.WorldObjectType))
				{
					if (IsIgnoredWorldObjectType(objectData.WorldObjectType)) continue;
					// The anchor may be fractional, but it must still remain inside the vertical world bounds.
					range = Include(range, (int)MathF.Floor(block.LocalPosition.Y));
					foreach (BlockOccupancy occupancy in WorldObject.GetOccupancy(objectData.WorldObjectType))
					{
						ct.ThrowIfCancellationRequested();
						if (occupancy.BlockType is null) continue;
						Vector3 occupiedPosition = block.LocalPosition + objectData.Rotation.RotateVector(occupancy.Offset);
						range = Include(range, ToBlockPosition(occupiedPosition).Y);
					}
					continue;
				}

				range = Include(range, ToBlockPosition(block.LocalPosition).Y);
			}
			return range;
		}

		private static PlacementHeightRange Include(PlacementHeightRange? range, int height)
		{
			return range is { } current
				? new PlacementHeightRange(Math.Min(current.MinY, height), Math.Max(current.MaxY, height))
				: new PlacementHeightRange(height, height);
		}

		private static PlacementHeightRange? Union(PlacementHeightRange? left, PlacementHeightRange? right)
		{
			if (left is null) return right;
			if (right is null) return left;
			return left.Value.Union(right.Value);
		}
	}
}
