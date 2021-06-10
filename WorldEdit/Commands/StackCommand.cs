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

		public StackCommand(User user, string args) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");

			this.direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out this.amount);
			if (this.direction == Direction.Unknown || this.direction == Direction.None) { throw new WorldEditCommandException("Unable to determine direction"); }
		}

		protected override void Execute()
		{
			WorldRange range = this.UserSession.Selection;
			range.Fix(Shared.Voxel.World.VoxelSize);

			List<WorldEditBlock> blocks = new List<WorldEditBlock>();

			void DoAction(Vector3i pos)
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				WorldEditBlock sourceBlock = WorldEditBlock.Create(Eco.World.World.GetBlock(pos), pos);
				blocks.Add(sourceBlock);
			}
			range.ForEachInc(DoAction);

			Vector3i directionOffset = this.direction.ToVec();
			switch (this.direction)
			{
				case Direction.Up:
				case Direction.Down:
					directionOffset *= range.HeightInc;
					break;
				case Direction.Left:
				case Direction.Right:
					directionOffset *= range.WidthInc;
					break;
				case Direction.Forward:
				case Direction.Back:
					directionOffset *= range.LengthInc;
					break;
			}

			for (int i = 1; i <= amount; i++)
			{
				Vector3i offset = directionOffset * i;

				foreach (WorldEditBlock sourceBlock in blocks)
				{
					this.AddBlockChangedEntry(WorldEditBlockManager.ApplyOffset(sourceBlock.Position, offset));
					WorldEditBlockManager.RestoreBlockOffset(sourceBlock, offset, this.UserSession);
					this.BlocksChanged++;
				}
			}
		}
	}
}
