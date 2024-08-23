using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World.Blocks;
using Eco.WorldGenerator;

namespace Eco.Mods.WorldEdit.Commands
{
	using World = Eco.World.World;

	internal class FixwaterCommand : WorldEditCommand
	{
		private int _height;

		public FixwaterCommand(User user, int height) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			this._height = height == 0 ? WorldGeneratorPlugin.Settings.WaterLevel : height;
		}

		protected override void Execute(WorldRange selection)
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);

			selection = selection.FixXZ(Shared.Voxel.World.VoxelSize);

			void Fixwater(Vector3i pos)
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				Block block = World.GetBlock(pos);

				if(pos.Y <= this._height && BlockUtils.IsNullOrEmptyBlock(block))
				{
					this.AddBlockChangedEntry(pos);
					blockManager.SetBlock(typeof(WaterBlock), pos);
					this.BlocksChanged++;
				}
				else if (pos.Y > this._height && BlockUtils.IsWaterBlock(block))
				{
					this.AddBlockChangedEntry(pos);
					blockManager.SetBlock(typeof(EmptyBlock), pos);
					this.BlocksChanged++;
				}
			}
			selection.ForEachInc(Fixwater);
		}
	}
}
