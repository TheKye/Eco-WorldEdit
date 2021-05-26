using System.Collections.Generic;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class PasteCommand : WorldEditCommand
	{
		private Vector3i playerPos;

		public PasteCommand(User user) : base(user)
		{
			if (this.UserSession.Clipboard.Count <= 0) throw new WorldEditCommandException($"Please /copy a selection first!");
		}

		protected override void Execute()
		{
			this.playerPos = this.UserSession.Player.Position.Round;

			this.Restore(this.UserSession.Clipboard.GetBlocks());
			this.Restore(this.UserSession.Clipboard.GetPlants());
			this.Restore(this.UserSession.Clipboard.GetWorldObjects());
		}

		private void Restore(List<WorldEditBlock> list)
		{
			foreach (WorldEditBlock entry in list)
			{
				Vector3i pos = WorldEditBlockManager.ApplyOffset(entry.Position, this.playerPos);
				if (WorldEditBlockManager.IsImpenetrable(pos)) continue;
				AddBlockChangedEntry(pos);
				WorldEditBlockManager.RestoreBlockOffset(entry, this.playerPos, this.UserSession.Player);
			}
		}
	}
}
