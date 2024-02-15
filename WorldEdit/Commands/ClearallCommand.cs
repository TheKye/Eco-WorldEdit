using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class ClearallCommand : WorldEditCommand
	{
		private WorldRange area;

		public ClearallCommand(User user, int square) : base(user)
		{
			Vector3i? pos = WorldEditUtils.GetPositionForUser(user, null) ?? throw new WorldEditCommandException("Unable to determine position!");
			this.area = WorldRange.SurroundingSpace(pos.Value, square);
			this.area.min = new Vector3i(this.area.min.X, pos.Value.Y, this.area.min.Z);
			this.area.max = new Vector3i(this.area.max.X, Shared.Voxel.World.VoxelSize.y, this.area.max.Z);
		}

		protected override void Execute(WorldRange selection)
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);
			selection = this.area.FixXZ(Shared.Voxel.World.VoxelSize);
			void DoAction(Vector3i pos)
			{
				if (WorldEditBlockManager.IsImpenetrable(pos)) return;
				this.AddBlockChangedEntry(pos);
				blockManager.SetBlock(null, pos);
				this.BlocksChanged++;
			}
			selection.ForEachInc(DoAction);
		}
	}
}
