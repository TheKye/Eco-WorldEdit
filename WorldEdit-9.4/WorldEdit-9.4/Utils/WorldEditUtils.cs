using System;
using System.Collections.Generic;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class WorldEditUtils
	{
		public static Direction ParseDirectionAndAmountArgs(User user, string args, out int amount)
		{
			Direction direction = Direction.Unknown;
			amount = 1;
			string[] splitted = args.Split(' ', ',');

			switch (splitted.Length)
			{
				case 1:
					if (!int.TryParse(splitted[0], out amount))
					{
						direction = DirectionUtils.GetDirectionOrLooking(user, splitted[0]);
					}
					else
					{
						direction = DirectionUtils.GetDirectionOrLooking(user, null);
					}
					break;
				case 2:
					if (!int.TryParse(splitted[0], out amount))
					{
						direction = DirectionUtils.GetDirectionOrLooking(user, splitted[0]);
						if (!int.TryParse(splitted[1], out amount))
						{
							throw new ArgumentException("Expects direction and amount");
						}
					}
					else
					{
						direction = DirectionUtils.GetDirectionOrLooking(user, splitted[1]);
					}
					break;
				default:
					direction = DirectionUtils.GetDirectionOrLooking(user, null);
					break;
			}

			return direction;

			//if (!int.TryParse(splitted[0], out amount))
			//{
			//	user.Player.ErrorLocStr($"Please provide an amount first");
			//	return Direction.Unknown;
			//}

			//return DirectionUtils.GetDirectionOrLooking(user, splitted.Length >= 2 ? splitted[1] : null);
		}

		// Is there a correct name for this operation?
		public static int SumAllAxis(Vector3i pVector)
		{
			return pVector.X + pVector.Y + pVector.Z;
		}

		public static SortedVectorPair? GetSortedVectors(Vector3i? firstPos, Vector3i? secondPos)
		{
			if (!firstPos.HasValue || !secondPos.HasValue) return null;

			Vector3i pos1 = firstPos.Value;
			Vector3i pos2 = secondPos.Value;

			int typeOfVolume = FindTypeOfVolume(ref pos1, ref pos2, out Vector3i lower, out Vector3i higher);

			if (typeOfVolume == 1)
			{
				//swap
				Vector3i tmp = higher;
				higher = lower;
				lower = tmp;

				lower.Y = Math.Min(pos1.Y, pos2.Y);
				higher.Y = Math.Max(pos1.Y, pos2.Y) + 1;
			}
			if (typeOfVolume == 3)
			{
				int tmp = higher.X;
				higher.X = lower.X;
				lower.X = tmp;
			}
			else if (typeOfVolume == 2)
			{
				int tmp = higher.Z;
				higher.Z = lower.Z;
				lower.Z = tmp;
			}

			higher.X = (higher.X + 1) % Shared.Voxel.World.VoxelSize.X;
			higher.Z = (higher.Z + 1) % Shared.Voxel.World.VoxelSize.Z;

			return new SortedVectorPair(lower, higher);
		}

		public static int FindTypeOfVolume(ref Vector3i pos1, ref Vector3i pos2, out Vector3i lower, out Vector3i higher)
		{
			lower = new Vector3i();
			higher = new Vector3i();

			pos1.X = pos1.X % Shared.Voxel.World.VoxelSize.X;
			pos1.Z = pos1.Z % Shared.Voxel.World.VoxelSize.Z;

			pos2.X = pos2.X % Shared.Voxel.World.VoxelSize.X;
			pos2.Z = pos2.Z % Shared.Voxel.World.VoxelSize.Z;

			lower.X = Math.Min(pos1.X, pos2.X);
			lower.Y = Math.Min(pos1.Y, pos2.Y);
			lower.Z = Math.Min(pos1.Z, pos2.Z);

			higher.X = Math.Max(pos1.X, pos2.X);
			higher.Y = Math.Max(pos1.Y, pos2.Y) + 1;
			higher.Z = Math.Max(pos1.Z, pos2.Z);

			KeyValuePair<int, int>[] volumes = new KeyValuePair<int, int>[4];

			//sometimes a delta is 0 - we add one so the volume is not 0. The value does not matter here!
			volumes[0] = new KeyValuePair<int, int>(0, (((higher.X - lower.X) + 1) * ((higher.Z - lower.Z) + 1)));
			volumes[1] = new KeyValuePair<int, int>(1, (lower.X + (Shared.Voxel.World.VoxelSize.X - higher.X) + 1) * (lower.Z + (Shared.Voxel.World.VoxelSize.Z - higher.Z) + 1));
			volumes[2] = new KeyValuePair<int, int>(2, ((higher.X - lower.X) + 1) * (lower.Z + (Shared.Voxel.World.VoxelSize.Z - higher.Z) + 1));
			volumes[3] = new KeyValuePair<int, int>(3, (lower.X + (Shared.Voxel.World.VoxelSize.X - higher.X) + 1) * ((higher.Z - lower.Z) + 1));

			KeyValuePair<int, int> min = volumes.MinObj(kv => kv.Value);

			return min.Key;
		}
	}
}
