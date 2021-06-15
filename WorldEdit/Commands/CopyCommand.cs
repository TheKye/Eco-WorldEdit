using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class CopyCommand : WorldEditCommand
	{
		public CopyCommand(User user) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
		}

		protected override void Execute(WorldRange selection)
		{
			selection = selection.FixXZ(Shared.Voxel.World.VoxelSize);

			Vector3i playerPos = this.UserSession.Player.Position.Round;

			this.UserSession.Clipboard.Clear();
			void DoAction(Vector3i pos)
			{
				this.UserSession.Clipboard.Add(WorldEditBlock.Create(Eco.World.World.GetBlock(pos), pos, playerPos));
			}
			selection.ForEachInc(DoAction);
			this.UserSession.AuthorInfo.MarkDirty();
		}
	}
}
