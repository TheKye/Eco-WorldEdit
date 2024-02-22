using System.Collections.Generic;
using System.Linq;
using Eco.Gameplay.Players;
using Eco.Gameplay.Rooms;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class PasteCommand : WorldEditCommand
	{
		private Vector3i playerPos;
		private bool skipEmpty;
		private WorldEditClipboard clipboard;

		public PasteCommand(User user, bool skipEmpty = false) : base(user)
		{
			if (this.UserSession.Clipboard.Count <= 0) throw new WorldEditCommandException($"Please /copy a selection first!");
			this.skipEmpty = skipEmpty;
			this.playerPos = this.UserSession.Player.User.Position.Round();
			this.clipboard = this.UserSession.Clipboard.Copy();
		}

		protected override void Execute(WorldRange selection)
		{
			this.Restore(this.clipboard.GetBlocks());
			this.Restore(this.clipboard.GetPlants());
			this.Restore(this.clipboard.GetWorldObjects());
			this.ScheduleRoomRecalculation();
		}

		private void Restore(List<WorldEditBlock> list)
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);
			foreach (WorldEditBlock entry in list)
			{
				if(this.skipEmpty && entry.IsEmptyBlock()) continue;
				Vector3i pos = WorldEditBlockManager.ApplyOffset(entry.Position, this.playerPos);
				if (WorldEditBlockManager.IsImpenetrable(pos)) continue;
				this.AddBlockChangedEntry(pos);
				blockManager.RestoreBlockOffset(entry, this.playerPos);
			}
		}

		private void ScheduleRoomRecalculation()
		{
			if(this.AffectedBlocks.Count == 0) return;
			IEnumerable<Vector3i> positions = this.AffectedBlocks.Select(b => b.Position);
			RoomData.QueuePositionsTest(positions);
		}
	}
}
