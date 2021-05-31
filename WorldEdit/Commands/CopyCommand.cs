using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class CopyCommand : WorldEditCommand
	{
		private readonly SortedVectorPair vectors;

		public CopyCommand(User user) : base(user)
		{
			//if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) throw new WorldEditCommandException($"Please set both points first!");
			SortedVectorPair? svp = WorldEditUtils.GetSortedVectors(this.UserSession.FirstPos, this.UserSession.SecondPos);
			if (svp == null) throw new WorldEditCommandException($"Please set both points first!");
			this.vectors = (SortedVectorPair)svp;
		}

		protected override void Execute()
		{
			Vector3i playerPos = this.UserSession.Player.Position.Round;

			this.UserSession.Clipboard.Clear();
			for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
			{
				//Console.WriteLine($"Copy {vectors.Lower.X} {x} {vectors.Higher.X}"); //TODO Remove debug! Check for vector problems
				for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
				{
					for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
					{
						Vector3i pos = new Vector3i(x, y, z);
						this.UserSession.Clipboard.Add(WorldEditBlock.Create(Eco.World.World.GetBlock(pos), pos, playerPos));
					}
				}
			}
			this.UserSession.AuthorInfo.MarkDirty();
		}
	}
}
