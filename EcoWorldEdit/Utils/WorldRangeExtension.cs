using System.Collections.Generic;
using System.Linq;
using Eco.Shared.Math;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class WorldRangeExtension
	{
		public static bool IsSet(this WorldRange range) => !range.min.Equals(Vector3i.MaxValue) && !range.max.Equals(Vector3i.MaxValue);

		public static IEnumerable<int> AxisIterator(this WorldRange range, Axis a)
		{
			int pos = 0;
			switch (a)
			{
				case Axis.X: for (pos = range.min.x; pos <= range.max.x; pos++) { yield return pos; } break;
				case Axis.Y: for (pos = range.min.y; pos <= range.max.y; pos++) { yield return pos; } break;
				case Axis.Z: for (pos = range.min.z; pos <= range.max.z; pos++) { yield return pos; } break;
			}
		}
		public static IEnumerable<Vector3i> SidesIterator(this WorldRange range)
		{
			return range.XYZIterInc().Where(locPos => (
								locPos.x == range.min.x || locPos.z == range.min.z ||
								locPos.x == range.max.x || locPos.z == range.max.z
							 ));
		}

		public static WorldRange FixXZ(this WorldRange range, Vector3i worldSize = default)
		{
			Vector3i start = range.min;
			Vector3i end = range.max;
			range.Fix(worldSize);
			if (worldSize != default)
			{
				if (MathUtil.Min(start.y, end.y) < 0) range.min.y = 0;
				if (MathUtil.Max(start.y, end.y) > Shared.Voxel.World.VoxelSize.y) range.max.y = Shared.Voxel.World.VoxelSize.y;
			}
			return range;
		}
	}
}
