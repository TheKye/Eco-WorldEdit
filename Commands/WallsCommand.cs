using System;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class WallsCommand : WorldEditCommand
	{
		private Type blockType;

		public WallsCommand(User user, string blockType) : base(user)
		{
			if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) { throw new WorldEditCommandException("Please set both points first!"); }
			this.blockType = BlockUtils.GetBlockType(blockType) ?? throw new WorldEditCommandException($"No BlockType with name {blockType} found!");
		}

		protected override void Execute()
		{
			SortedVectorPair vectors = (SortedVectorPair)WorldEditUtils.GetSortedVectors(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value);
			this.BlocksChanged = 0;

			for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
			{
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					Vector3i pos = new Vector3i(x, y, vectors.Lower.Z);
					this.AddBlockChangedEntry(pos);
					WorldEditBlockManager.SetBlock(this.blockType, pos);
					this.BlocksChanged++;
				}
			}

			for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
			{
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					Vector3i pos = new Vector3i(x, y, vectors.Higher.Z - 1);
					this.AddBlockChangedEntry(pos);
					WorldEditBlockManager.SetBlock(this.blockType, pos);
					this.BlocksChanged++;
				}
			}

			for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
			{
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					Vector3i pos = new Vector3i(vectors.Lower.X, y, z);
					this.AddBlockChangedEntry(pos);
					WorldEditBlockManager.SetBlock(this.blockType, pos);
					this.BlocksChanged++;
				}
			}

			for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
			{
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					Vector3i pos = new Vector3i(vectors.Higher.X - 1, y, z);
					this.AddBlockChangedEntry(pos);
					WorldEditBlockManager.SetBlock(this.blockType, pos);
					this.BlocksChanged++;
				}
			}
			//   int changedBlocks = (((vectors.Higher.X - vectors.Lower.X) * 2 + (vectors.Higher.Z - vectors.Lower.Z) * 2) - 4) * (vectors.Higher.Y - vectors.Lower.Y);
			if (this.BlocksChanged == 0) //maybe better math?
			{
				this.BlocksChanged = 1;
			}
		}
	}
}
