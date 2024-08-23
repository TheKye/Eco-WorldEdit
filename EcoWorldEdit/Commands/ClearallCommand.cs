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
			this.area.min.Y = pos.Value.Y;
			this.area.max.Y = Shared.Voxel.World.VoxelSize.y;
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
