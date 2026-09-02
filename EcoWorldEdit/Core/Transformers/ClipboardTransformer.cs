using System.Numerics;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Occupancy;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Core.Transformers
{
	internal static class ClipboardTransformer
	{
		public static Clipboard Rotate(Clipboard source, float degrees, CancellationToken ct = default)
		{
			ArgumentNullException.ThrowIfNull(source);
			ct.ThrowIfCancellationRequested();
			if (source.Count <= 0) throw new WorldEditCommandException("Please /copy a selection or /import blueprint first!");
			if (!float.IsFinite(degrees)) throw new WorldEditCommandException("Rotation angle must be a finite number.");

			float exactAngle = NormalizeAngle0To360(degrees);
			int quarterTurns = ((int)MathF.Round(exactAngle / 90f, MidpointRounding.AwayFromZero)) % 4;
			float gridAngle = quarterTurns * 90f;
			ValidateDimension(source.Dimension);

			Matrix4x4 rotation = Matrix4x4.CreateRotationY(gridAngle * (MathF.PI / 180f));
			(Vector3 shift, Vector3i dimension) = CalculateBounds(source.Dimension, rotation);
			Matrix4x4 structuralTransform = rotation * Matrix4x4.CreateTranslation(shift);

			List<WorldEditBlock> blocks = Transform(source.Blocks, structuralTransform, exactAngle, quarterTurns, ct);
			List<WorldEditBlock> plants = Transform(source.Plants, structuralTransform, exactAngle, quarterTurns, ct);
			List<WorldEditBlock> worldObjects = Transform(source.WorldObjects, structuralTransform, exactAngle, quarterTurns, ct);

			ct.ThrowIfCancellationRequested();
			ValidateCollisions(blocks, plants, worldObjects, ct);
			ct.ThrowIfCancellationRequested();
			return Clipboard.Create(blocks, plants, worldObjects, source.Author, dimension);
		}

		private static List<WorldEditBlock> Transform(IReadOnlyList<WorldEditBlock> source, Matrix4x4 transform, float exactAngle, int quarterTurns, CancellationToken ct)
		{
			List<WorldEditBlock> transformed = new(source.Count);
			foreach (WorldEditBlock block in source)
			{
				ct.ThrowIfCancellationRequested();
				transformed.Add(BlockTransformer.Rotate(block, transform, exactAngle, quarterTurns));
			}
			return transformed;
		}

		private static (Vector3 Shift, Vector3i Dimension) CalculateBounds(Vector3i dimension, Matrix4x4 rotation)
		{
			float maxSourceX = dimension.X - 1;
			float maxSourceZ = dimension.Z - 1;
			Vector3[] corners =
			[
				Vector3.Transform(new Vector3(0, 0, 0), rotation),
				Vector3.Transform(new Vector3(maxSourceX, 0, 0), rotation),
				Vector3.Transform(new Vector3(0, 0, maxSourceZ), rotation),
				Vector3.Transform(new Vector3(maxSourceX, 0, maxSourceZ), rotation)
			];

			float minX = corners.Min(corner => MathF.Round(corner.X, MidpointRounding.AwayFromZero));
			float maxX = corners.Max(corner => MathF.Round(corner.X, MidpointRounding.AwayFromZero));
			float minZ = corners.Min(corner => MathF.Round(corner.Z, MidpointRounding.AwayFromZero));
			float maxZ = corners.Max(corner => MathF.Round(corner.Z, MidpointRounding.AwayFromZero));
			Vector3 shift = new(-minX, 0, -minZ);
			Vector3i newDimension = new((int)(maxX - minX + 1), dimension.Y, (int)(maxZ - minZ + 1));
			return (shift, newDimension);
		}

		private static void ValidateCollisions(IReadOnlyList<WorldEditBlock> blocks, IReadOnlyList<WorldEditBlock> plants, IReadOnlyList<WorldEditBlock> worldObjects, CancellationToken ct)
		{
			Dictionary<Vector3i, List<Occupant>> occupied = new();
			foreach (WorldEditBlock block in blocks)
			{
				ct.ThrowIfCancellationRequested();
				if (block.IsEmptyBlock()) continue;
				AddOccupant(occupied, ToBlockPosition(block.LocalPosition), new Occupant(block, block.BlockType, null, false), ct);
			}
			foreach (WorldEditBlock plant in plants)
			{
				ct.ThrowIfCancellationRequested();
				AddOccupant(occupied, ToBlockPosition(plant.LocalPosition), new Occupant(plant, plant.BlockType, null, false), ct);
			}

			Dictionary<Guid, Guid?> parents = ValidateObjectHierarchy(worldObjects, ct);
			foreach (WorldEditBlock block in worldObjects)
			{
				ct.ThrowIfCancellationRequested();
				if (block.BlockData is not WorldObjectBlockData objectData) throw new WorldEditCommandException($"Clipboard WorldObject entry at {block.LocalPosition} has invalid block data.");
				if (!objectData.WorldObjectType.DerivesFrom<WorldObject>()) throw new WorldEditCommandException($"Type {objectData.WorldObjectType} is not a WorldObject type.");
				List<BlockOccupancy> occupancies;
				try { occupancies = WorldObject.GetOccupancy(objectData.WorldObjectType).ToList(); }
				catch (Exception exception) { throw new WorldEditCommandException($"Unable to determine occupancy for WorldObject {objectData.WorldObjectType}: {exception.Message}", exception); }

				Vector3i occupancyOrigin = block.LocalPosition.XYZi();
				foreach (BlockOccupancy occupancy in occupancies)
				{
					ct.ThrowIfCancellationRequested();
					if (occupancy.BlockType is null) continue;
					Vector3i occupiedPosition = occupancyOrigin + objectData.Rotation.RotateVector(occupancy.Offset).XYZi();
					AddOccupant(occupied, occupiedPosition, new Occupant(block, occupancy.BlockType, objectData.ObjectId, true), ct, parents);
				}
			}
		}

		private static Dictionary<Guid, Guid?> ValidateObjectHierarchy(IReadOnlyList<WorldEditBlock> worldObjects, CancellationToken ct)
		{
			Dictionary<Guid, Guid?> parents = new();
			foreach (WorldEditBlock block in worldObjects)
			{
				ct.ThrowIfCancellationRequested();
				if (block.BlockData is not WorldObjectBlockData data) throw new WorldEditCommandException($"Clipboard WorldObject entry at {block.LocalPosition} has invalid block data.");
				if (data.ObjectId is Guid objectId && !parents.TryAdd(objectId, data.ParentId)) throw new WorldEditCommandException($"WorldObject identifier {objectId} is duplicated.");
			}
			foreach (WorldEditBlock block in worldObjects)
			{
				ct.ThrowIfCancellationRequested();
				WorldObjectBlockData data = (WorldObjectBlockData)block.BlockData!;
				if (data.ParentId is Guid parentId && !parents.ContainsKey(parentId)) throw new WorldEditCommandException($"WorldObject references missing parent {parentId}.");
			}

			foreach ((Guid objectId, Guid? parentId) in parents)
			{
				ct.ThrowIfCancellationRequested();
				if (parentId is Guid parent && !parents.ContainsKey(parent)) throw new WorldEditCommandException($"WorldObject {objectId} references missing parent {parent}.");
				HashSet<Guid> path = [];
				Guid? current = objectId;
				while (current is Guid currentId)
				{
					ct.ThrowIfCancellationRequested();
					if (!path.Add(currentId)) throw new WorldEditCommandException($"Cycle detected in WorldObject hierarchy at {currentId}.");
					current = parents[currentId];
				}
			}
			return parents;
		}

		private static void AddOccupant(Dictionary<Vector3i, List<Occupant>> occupied, Vector3i position, Occupant candidate, CancellationToken ct, IReadOnlyDictionary<Guid, Guid?>? parents = null)
		{
			if (!occupied.TryGetValue(position, out List<Occupant>? existing))
			{
				occupied.Add(position, [candidate]);
				return;
			}

			foreach (Occupant occupant in existing)
			{
				ct.ThrowIfCancellationRequested();
				if (ReferenceEquals(occupant.Block, candidate.Block)) continue;
				if (parents is not null && AreRelated(occupant.ObjectId, candidate.ObjectId, parents)) continue;
				if (occupant.IsWorldObject && candidate.IsWorldObject &&
					typeof(WorldObjectManyBlock).IsAssignableFrom(occupant.OccupancyBlockType) &&
					typeof(WorldObjectManyBlock).IsAssignableFrom(candidate.OccupancyBlockType)) continue;

				throw new WorldEditCommandException($"Rotated clipboard has incompatible occupancy at {position} between {occupant.Block.BlockType} and {candidate.Block.BlockType}.");
			}
			existing.Add(candidate);
		}

		private static bool AreRelated(Guid? first, Guid? second, IReadOnlyDictionary<Guid, Guid?> parents)
		{
			if (first is not Guid firstId || second is not Guid secondId) return false;
			return parents.TryGetValue(firstId, out Guid? firstParent) && firstParent == secondId || parents.TryGetValue(secondId, out Guid? secondParent) && secondParent == firstId;
		}

		private static Vector3i ToBlockPosition(Vector3 position)
		{
			if (!BlockUtils.TryGetBlockPosition(position, out Vector3i blockPosition)) throw new WorldEditCommandException($"Rotated WorldObject occupancy position {position} is not aligned to the block grid.");
			return blockPosition;
		}

		private static float NormalizeAngle0To360(float degrees)
		{
			float normalized = degrees % 360f;
			return normalized < 0f ? normalized + 360f : normalized;
		}

		private static void ValidateDimension(Vector3i dimension)
		{
			if (dimension.X <= 0 || dimension.Y <= 0 || dimension.Z <= 0) throw new WorldEditCommandException($"Clipboard dimension {dimension} is invalid; all dimensions must be positive.");
		}

		private sealed record Occupant(WorldEditBlock Block, Type OccupancyBlockType, Guid? ObjectId, bool IsWorldObject);
	}
}
