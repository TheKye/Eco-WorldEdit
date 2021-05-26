using System;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;
using Eco.World;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class ReplaceCommand : WorldEditCommand
	{
		private Type blockTypeToFind;
		private Type blockTypeToReplace;

		public ReplaceCommand(User user, string findType, string replaceType) : base(user)
		{
			if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) { throw new WorldEditCommandException("Please set both points first!"); }
			this.blockTypeToFind = BlockUtils.GetBlockType(findType) ?? throw new WorldEditCommandException($"No BlockType with name {findType} found!");
			this.blockTypeToReplace = null;
			if (replaceType != string.Empty)
			{
				this.blockTypeToReplace = BlockUtils.GetBlockType(replaceType) ?? throw new WorldEditCommandException($"No BlockType with name {replaceType} found!");
			}
		}

		protected override void Execute()
		{
			SortedVectorPair vectors = (SortedVectorPair)WorldEditUtils.GetSortedVectors(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value);
			this.BlocksChanged = 0;
			for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
			{
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
					{
						Vector3i pos = new Vector3i(x, y, z);
						if (WorldEditBlockManager.IsImpenetrable(pos)) continue;
						ReplaceBlock(pos);
					}
				}
			}
		}

		private void ReplaceBlock(Vector3i pos)
		{
			Block block = Eco.World.World.GetBlock(pos);

			if (block == null) return;
			if (this.blockTypeToReplace != null)
			{
				if (block.GetType() == this.blockTypeToFind)
				{
					this.AddBlockChangedEntry(pos);
					WorldEditBlockManager.SetBlock(this.blockTypeToReplace, pos);
					this.BlocksChanged++;
				}
			}
			else
			{
				if (block.GetType() != typeof(EmptyBlock))
				{
					this.AddBlockChangedEntry(pos);
					WorldEditBlockManager.SetBlock(this.blockTypeToFind, pos);
					this.BlocksChanged++;
				}
			}
		}
	}
}
