using System;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class WallsCommand : WorldEditCommand
	{
		private Type blockType;

		public WallsCommand(User user, string blockType) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			this.blockType = BlockUtils.GetBlockType(blockType) ?? throw new WorldEditCommandException($"No BlockType with name {blockType} found!");
		}

		protected override void Execute(WorldRange selection)
		{
			selection = selection.FixXZ(Shared.Voxel.World.VoxelSize);

			foreach (Vector3i pos in selection.SidesIterator())
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) continue;
				this.AddBlockChangedEntry(pos);
				WorldEditBlockManager.SetBlock(blockType, pos);
				this.BlocksChanged++;
			}
		}
	}
}
