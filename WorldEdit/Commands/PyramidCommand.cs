using System;
using System.Collections.Generic;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class PyramidCommand : WorldEditCommand
	{
		public enum PyramidStyle { Full, Thin, Thick, Hollow }

		private Vector3i pos;
		private Type blockType;
		private int height;
		private PyramidStyle style;
		private bool clear;

		public PyramidCommand(User user, int height, string blockType, PyramidStyle style, bool clear) : base(user)
		{
			this.pos = ((Vector3i?)(WorldEditUtils.GetPositionForUser(user, null) ?? throw new WorldEditCommandException("Unable to determine position!"))).Value;
			this.blockType = BlockUtils.GetBlockType(blockType) ?? throw new WorldEditCommandException($"No BlockType with name {blockType} found!");
			this.height = height;
			this.style = style;
			this.clear = clear;
		}

		protected override void Execute(WorldRange _)
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);

			void ClearAction(Vector3i pos)
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				this.AddBlockChangedEntry(pos);
				blockManager.SetBlock(null, pos);
				this.BlocksChanged++;
			}

			void SetBlocksAction(Vector3i pos)
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				if (!this.clear) { this.AddBlockChangedEntry(pos); }
				blockManager.SetBlock(this.blockType, pos);
				this.BlocksChanged++;
			}

			if (this.clear)
			{
				WorldRange area = WorldRange.SurroundingSpace(this.pos, this.height);
				area.min.Y = this.pos.Y;
				area = area.FixXZ(Shared.Voxel.World.VoxelSize);
				area.ForEachInc(ClearAction);
			}

			//Vector3i currentPos = this.pos + (Vector3i.Up * height); //Start at the top
			int startHeight = this.pos.Y + this.height - 1;
			for (int currentHeight = startHeight; currentHeight >= this.pos.Y; currentHeight--)
			{
				int heighDiff = startHeight - currentHeight; //Use as offset
				WorldRange area = WorldRange.SurroundingSpace(this.pos, heighDiff);
				area.min.Y = area.max.Y = currentHeight;
				area = area.FixXZ(Shared.Voxel.World.VoxelSize);

				IEnumerable<Vector3i> iterator = area.XYZIterInc();
				switch (this.style)
				{
					case PyramidStyle.Thin:
					case PyramidStyle.Thick:
						iterator = area.SidesIterator();
						break;
				}

				//Main loop for build pyramid
				foreach (Vector3i pos in iterator)
				{
					SetBlocksAction(pos);
				}

				//Additional loop for build inner layer for double walls
				if (this.style == PyramidStyle.Thick && currentHeight != startHeight)
				{
					area = WorldRange.SurroundingSpace(this.pos, heighDiff - 1);
					area.min.Y = area.max.Y = currentHeight;
					area = area.FixXZ(Shared.Voxel.World.VoxelSize);
					foreach (Vector3i pos in area.SidesIterator())
					{
						SetBlocksAction(pos);
					}
				}
			}
		}
	}
}
