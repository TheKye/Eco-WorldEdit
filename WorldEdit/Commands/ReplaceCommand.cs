using System;
using Eco.Gameplay.Players;
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
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			this.blockTypeToFind = BlockUtils.GetBlockType(findType) ?? throw new WorldEditCommandException($"No BlockType with name {findType} found!");
			this.blockTypeToReplace = null;
			if (replaceType != string.Empty)
			{
				this.blockTypeToReplace = BlockUtils.GetBlockType(replaceType) ?? throw new WorldEditCommandException($"No BlockType with name {replaceType} found!");
			}
		}

		protected override void Execute(WorldRange selection)
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);

			selection = selection.FixXZ(Shared.Voxel.World.VoxelSize);

			void ReplaceBlock(Vector3i pos)
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				Block block = Eco.World.World.GetBlock(pos);

				if (block == null) return;
				if (this.blockTypeToReplace != null)
				{
					if (block.GetType() == this.blockTypeToFind)
					{
						this.AddBlockChangedEntry(pos);
						blockManager.SetBlock(this.blockTypeToReplace, pos);
						this.BlocksChanged++;
					}
				}
				else
				{
					if (block.GetType() != typeof(EmptyBlock))
					{
						this.AddBlockChangedEntry(pos);
						blockManager.SetBlock(this.blockTypeToFind, pos);
						this.BlocksChanged++;
					}
				}
			}
			selection.ForEachInc(ReplaceBlock);
		}
	}
}
