using System.Collections.Generic;
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
		private readonly int offset;

		public StackCommand(User user, string args, int offset) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");

			this.direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out this.amount);
			if (this.direction == Direction.Unknown || this.direction == Direction.None) { throw new WorldEditCommandException("Unable to determine direction"); }
			this.offset = offset;
		}

		protected override void Execute(WorldRange selection)
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);
			selection = selection.FixXZ(Shared.Voxel.World.VoxelSize);

			List<WorldEditBlock> blocks = new List<WorldEditBlock>();

			void DoAction(Vector3i pos)
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				IEnumerable<WorldEditBlock> sourceBlocks = WorldEditBlock.Create(Eco.World.World.GetBlock(pos), pos);
				foreach (WorldEditBlock sourceBlock in sourceBlocks)
				{
					blocks.Add(sourceBlock);
				}
			}
			selection.ForEachInc(DoAction);

			Vector3i directionOffset = this.direction.ToVec();
			switch (this.direction)
			{
				case Direction.Up:
				case Direction.Down:
					directionOffset *= selection.HeightInc + this.offset;
					break;
				case Direction.Left:
				case Direction.Right:
					directionOffset *= selection.WidthInc + this.offset;
					break;
				case Direction.Forward:
				case Direction.Back:
					directionOffset *= selection.LengthInc + this.offset;
					break;
			}

			for (int i = 1; i <= this.amount; i++)
			{
				Vector3i offset = directionOffset * i;

				foreach (WorldEditBlock sourceBlock in blocks)
				{
					Vector3i finalPos = WorldEditBlockManager.ApplyOffset(sourceBlock.Position, offset);
					if (finalPos.Y < 0 || finalPos.Y > Shared.Voxel.World.VoxelSize.Y) continue;
					this.AddBlockChangedEntry(finalPos);
					blockManager.RestoreBlockOffset(sourceBlock, offset);
					this.BlocksChanged++;
				}
			}
		}
	}
}
