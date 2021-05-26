using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class DistrCommand : WorldEditCommand
	{
		public DistrCommand(User user) : base(user)
		{
			if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) { throw new WorldEditCommandException("Please set both points first!"); }
		}

		protected override void Execute()
		{
			SortedVectorPair vectors = (SortedVectorPair)WorldEditUtils.GetSortedVectors(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value);
			Dictionary<string, long> mBlocks = new Dictionary<string, long>();

			for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
			{
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
					{
						//                 Console.WriteLine($"{x} {y} {z}");
						var pos = new Vector3i(x, y, z);
						var block = Eco.World.World.GetBlock(pos).GetType().ToString();

						long count;
						mBlocks.TryGetValue(block, out count);
						mBlocks[block] = count + 1;
					}
				}
			}

			double amountBlocks = mBlocks.Values.Sum(); // (vectors.Higher.X - vectors.Lower.X) * (vectors.Higher.Y - vectors.Lower.Y) * (vectors.Higher.Z - vectors.Lower.Z);

			this.UserSession.Player.MsgLoc($"total blocks: {amountBlocks}");

			foreach (var entry in mBlocks)
			{
				string percent = (Math.Round((entry.Value / amountBlocks) * 100, 2)).ToString() + "%";
				string nameOfBlock = entry.Key.Substring(entry.Key.LastIndexOf(".") + 1);
				this.UserSession.Player.MsgLoc($"{entry.Value.ToString().PadRight(6)} {percent.PadRight(6)} {nameOfBlock}");
			}
		}
	}
}
