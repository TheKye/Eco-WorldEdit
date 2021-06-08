using System;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class SetCommand : WorldEditCommand
	{
		private Type blockType;

		public SetCommand(User user, string blockType) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			this.blockType = BlockUtils.GetBlockType(blockType) ?? throw new WorldEditCommandException($"No BlockType with name {blockType} found!");
		}


		protected override void Execute()
		{
			WorldRange range = this.UserSession.Selection;
			range.Fix(Shared.Voxel.World.VoxelSize);

			range.ForEachInc(pos =>
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				this.AddBlockChangedEntry(pos);
				WorldEditBlockManager.SetBlock(blockType, pos);
				this.BlocksChanged++;
			});
		}
	}
}
