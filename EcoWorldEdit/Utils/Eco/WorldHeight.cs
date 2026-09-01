namespace Eco.Mods.WorldEdit.Utils.Eco
{
	internal static class WorldHeight
	{
		public const int Min = 0;

		public static int Max => Shared.Voxel.World.VoxelSize.y - 1;

		public static bool IsValid(int height) => height >= Min && height <= Max;

		public static int Clamp(int height) => Math.Clamp(height, Min, Max);
	}
}
