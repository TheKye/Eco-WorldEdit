﻿using System;
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
			if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) { throw new WorldEditCommandException("Please set both points first!"); }

			this.direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out this.amount);
			if (this.direction == Direction.Unknown) { throw new WorldEditCommandException("Unable to determine direction"); }
		}

		protected override void Execute()
		{
			SortedVectorPair vectors = (SortedVectorPair)WorldEditUtils.GetSortedVectors(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value);
			Vector3i offset = this.direction.ToVec() * amount;

			this.BlocksChanged = 0;

			Action<int, int, int> action = (int x, int y, int z) =>
			{
				Vector3i pos = new Vector3i(x, y, z);
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				AddBlockChangedEntry(pos);
				AddBlockChangedEntry(pos + offset);

				WorldEditBlock sourceBlock = WorldEditBlock.Create(Eco.World.World.GetBlock(pos), pos);
				WorldEditBlockManager.RestoreBlockOffset(sourceBlock, offset, this.UserSession.Player);
				WorldEditBlockManager.SetBlock(typeof(EmptyBlock), pos);
				this.BlocksChanged++;
			};


			if (this.direction == Direction.Left || this.direction == Direction.Back || this.direction == Direction.Down)
			{
				for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
				{
					for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
					{
						for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
						{
							action.Invoke(x, y, z);
						}
					}
				}
			}
			else
			{
				/*                for (int x = vectors.Higher.X - 1; x >= vectors.Lower.X; x--)
									for (int y = vectors.Higher.Y - 1; y >= vectors.Lower.Y; y--)
										for (int z = vectors.Higher.Z - 1; z >= vectors.Lower.Z; z--)*/

				int x = vectors.Higher.X - 1;
				if (x < 0)
				{
					x = x + Shared.Voxel.World.VoxelSize.X;
				}

				int startZ = vectors.Higher.Z - 1;
				if (startZ < 0)
				{
					startZ = startZ + Shared.Voxel.World.VoxelSize.Z;
				}

				//           Console.WriteLine("--------------");
				//           Console.WriteLine(vectors.Lower);
				//            Console.WriteLine(vectors.Higher);

				for (; x != (vectors.Lower.X - 1); x--)
				{
					if (x < 0)
					{
						x = x + Shared.Voxel.World.VoxelSize.X;
					}

					for (int y = vectors.Higher.Y - 1; y >= vectors.Lower.Y; y--)
					{
						for (int z = startZ; z != (vectors.Lower.Z - 1); z--)
						{
							if (z < 0)
							{
								z = z + Shared.Voxel.World.VoxelSize.Z;
							}

							//               Console.WriteLine($"{x} {y} {z}");
							action.Invoke(x, y, z);
						}
					}
				}
			}
		}
	}
}
