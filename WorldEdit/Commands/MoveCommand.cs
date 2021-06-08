using System.Collections.Generic;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class MoveCommand : WorldEditCommand
	{
		public readonly Direction direction;
		public readonly int amount;

		public MoveCommand(User user, string args) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");

			this.direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out this.amount);
			if (this.direction == Direction.Unknown || this.direction == Direction.None) { throw new WorldEditCommandException("Unable to determine direction"); }
		}

		protected override void Execute()
		{
			WorldRange range = this.UserSession.Selection;
			range.Fix(Shared.Voxel.World.VoxelSize);

			Vector3i offset = this.direction.ToVec() * this.amount;
			Stack<WorldEditBlock> blocks = new Stack<WorldEditBlock>();

			range.ForEachInc(pos =>
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				WorldEditBlock sourceBlock = WorldEditBlock.Create(Eco.World.World.GetBlock(pos), pos);
				blocks.Push(sourceBlock);
				this.AddBlockChangedEntry(pos);
				WorldEditBlockManager.SetBlock(typeof(EmptyBlock), pos);
			});

			while (blocks.TryPop(out WorldEditBlock sourceBlock))
			{
				this.AddBlockChangedEntry(WorldEditBlockManager.ApplyOffset(sourceBlock.Position, offset));
				WorldEditBlockManager.RestoreBlockOffset(sourceBlock, offset, this.UserSession);
				this.BlocksChanged++;
			}


			//Action<int, int, int> action = (int x, int y, int z) =>
			//{
			//	Vector3i pos = new Vector3i(x, y, z);
			//	if (WorldEditBlockManager.IsImpenetrable(pos)) return;
			//	AddBlockChangedEntry(pos);
			//	AddBlockChangedEntry(pos + offset);

			//	WorldEditBlock sourceBlock = WorldEditBlock.Create(Eco.World.World.GetBlock(pos), pos);
			//	WorldEditBlockManager.RestoreBlockOffset(sourceBlock, offset, this.UserSession);
			//	WorldEditBlockManager.SetBlock(typeof(EmptyBlock), pos);
			//	this.BlocksChanged++;
			//};


			//if (this.direction == Direction.Left || this.direction == Direction.Back || this.direction == Direction.Down)
			//{
			//	for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
			//	{
			//		for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
			//		{
			//			for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
			//			{
			//				action.Invoke(x, y, z);
			//			}
			//		}
			//	}
			//}
			//else
			//{
			//	/*                for (int x = vectors.Higher.X - 1; x >= vectors.Lower.X; x--)
			//						for (int y = vectors.Higher.Y - 1; y >= vectors.Lower.Y; y--)
			//							for (int z = vectors.Higher.Z - 1; z >= vectors.Lower.Z; z--)*/

			//	int x = vectors.Higher.X - 1;
			//	if (x < 0)
			//	{
			//		x = x + Shared.Voxel.World.VoxelSize.X;
			//	}

			//	int startZ = vectors.Higher.Z - 1;
			//	if (startZ < 0)
			//	{
			//		startZ = startZ + Shared.Voxel.World.VoxelSize.Z;
			//	}

			//	//           Console.WriteLine("--------------");
			//	//           Console.WriteLine(vectors.Lower);
			//	//            Console.WriteLine(vectors.Higher);

			//	for (; x != (vectors.Lower.X - 1); x--)
			//	{
			//		if (x < 0)
			//		{
			//			x = x + Shared.Voxel.World.VoxelSize.X;
			//		}

			//		for (int y = vectors.Higher.Y - 1; y >= vectors.Lower.Y; y--)
			//		{
			//			for (int z = startZ; z != (vectors.Lower.Z - 1); z--)
			//			{
			//				if (z < 0)
			//				{
			//					z = z + Shared.Voxel.World.VoxelSize.Z;
			//				}

			//				//               Console.WriteLine($"{x} {y} {z}");
			//				action.Invoke(x, y, z);
			//			}
			//		}
			//	}
			//}
		}
	}
}
