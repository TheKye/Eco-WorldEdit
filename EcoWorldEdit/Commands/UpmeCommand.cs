using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using Eco.Shared.Math;
using System.Numerics;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class UpmeCommand : WorldEditCommand
	{
		private readonly int count;

		public UpmeCommand(User user, int count) : base(user)
		{
			this.count = count;
		}

		protected override void Execute(WorldRange selection)
		{
			Vector3 pos = this.UserSession.Player.User.Position;
			var newpos = new Vector3i((int)pos.X, (int)pos.Y + this.count, (int)pos.Z);
			WorldEditBlockManager.RestoreBlock(typeof(StoneBlock), newpos);
			newpos.Y += 2;
			this.UserSession.Player.SetPosition(newpos);
		}
	}
}
