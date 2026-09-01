namespace Eco.Mods.WorldEdit.Utils.Eco
{
	internal readonly record struct PlacementHeightRange(int MinY, int MaxY)
	{
		public PlacementHeightRange Offset(int offsetY) => new(checked(this.MinY + offsetY), checked(this.MaxY + offsetY));

		public PlacementHeightRange Union(PlacementHeightRange other) => new(Math.Min(this.MinY, other.MinY), Math.Max(this.MaxY, other.MaxY));

		public bool FitsWorld() => WorldHeight.IsValid(this.MinY) && WorldHeight.IsValid(this.MaxY);
	}
}
