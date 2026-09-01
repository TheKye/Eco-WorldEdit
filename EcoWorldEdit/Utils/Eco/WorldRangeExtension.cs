using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Utils.Eco
{
	internal static class WorldRangeExtension
	{
		public static bool IsSet(this WorldRange range) => !range.min.Equals(Vector3i.MaxValue) && !range.max.Equals(Vector3i.MinValue);

		public static IEnumerable<int> AxisIterator(this WorldRange range, Axis a)
		{
			int pos = 0;
			switch (a)
			{
				case Axis.X:
					for (pos = range.min.x; pos <= range.max.x; pos++)
					{ yield return pos; }
					break;
				case Axis.Y:
					for (pos = range.min.y; pos <= range.max.y; pos++)
					{ yield return pos; }
					break;
				case Axis.Z:
					for (pos = range.min.z; pos <= range.max.z; pos++)
					{ yield return pos; }
					break;
			}
		}
		public static IEnumerable<Vector3i> SidesIterator(this WorldRange range)
		{
			return range.XYZIterInc().Where(
				locPos => (
					locPos.x == range.min.x || locPos.z == range.min.z ||
					locPos.x == range.max.x || locPos.z == range.max.z
				)
			);
		}

		public static WorldRange FixToWorldSize(this WorldRange range)
		{
			Vector3i worldSize = Shared.Voxel.World.VoxelSize;
			Vector3i start = range.min;
			Vector3i end = range.max;
			range.Fix(worldSize);
			if (worldSize != default)
			{
				// Unlike X and Z, the vertical axis does not wrap. Restore the original vertical ordering after WorldRange.Fix and clamp it independently.
				range.min.y = WorldHeight.Clamp(Math.Min(start.y, end.y));
				range.max.y = WorldHeight.Clamp(Math.Max(start.y, end.y));
			}
			return range;
		}
	}
}
