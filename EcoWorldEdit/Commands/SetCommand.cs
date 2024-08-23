using System;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;
using Eco.Shared.Utils;

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

		public SetCommand(User user, Type blockType) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			this.blockType = blockType ?? throw new WorldEditCommandException($"No BlockType with name {blockType.FullName} found!");
		}

		protected override void Execute(WorldRange selection)
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);
			selection = selection.FixXZ(Shared.Voxel.World.VoxelSize);
			void DoAction(Vector3i pos)
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				this.AddBlockChangedEntry(pos);
				blockManager.SetBlock(this.blockType, pos);
				this.BlocksChanged++;
			}
			selection.ForEachInc(DoAction);
		}
	}
}
