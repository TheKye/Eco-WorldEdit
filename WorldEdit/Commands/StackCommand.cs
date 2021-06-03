using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class StackCommand : WorldEditCommand
	{
		public readonly Direction direction;
		public readonly int amount;

		public StackCommand(User user, string args) : base(user)
		{
			if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) { throw new WorldEditCommandException("Please set both points first!"); }

			this.direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out this.amount);
			if (this.direction == Direction.Unknown) { throw new WorldEditCommandException("Unable to determine direction"); }
		}

		protected override void Execute()
		{
			SortedVectorPair vectors = (SortedVectorPair)WorldEditUtils.GetSortedVectors(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value);
			this.BlocksChanged = 0;

			for (int i = 1; i <= amount; i++)
			{
				Vector3i offset = this.direction.ToVec() * (vectors.Higher - vectors.Lower) * i;

				for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
				{
					for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
					{
						for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
						{
							Vector3i pos = new Vector3i(x, y, z);
							if (WorldEditBlockManager.IsImpenetrable(pos)) continue;
							AddBlockChangedEntry(pos + offset);
							WorldEditBlock sourceBlock = WorldEditBlock.Create(Eco.World.World.GetBlock(pos), pos);
							WorldEditBlockManager.RestoreBlockOffset(sourceBlock, offset, this.UserSession);
							this.BlocksChanged++;
						}
					}
				}
			}
			//   int changedBlocks = (int)((vectors.Higher.X - vectors.Lower.X) * (vectors.Higher.Y - vectors.Lower.Y) * (vectors.Higher.Z - vectors.Lower.Z)) * amount;
		}
	}
}
