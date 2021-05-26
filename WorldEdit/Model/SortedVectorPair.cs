using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Model
{
	internal struct SortedVectorPair
	{
		public Vector3i Lower { get; private set; }
		public Vector3i Higher { get; private set; }

		public SortedVectorPair(Vector3i lower, Vector3i higher)
		{
			this.Lower = lower;
			this.Higher = higher;
		}
	}
}
