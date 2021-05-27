using System.Collections.Generic;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class RotateCommand : WorldEditCommand
	{
		private readonly float angleDegrees;

		public RotateCommand(User user, float degrees) : base(user)
		{
			this.angleDegrees = MathUtil.NormalizeAngle0to360(degrees);
			if (this.UserSession.Clipboard.Count <= 0) throw new WorldEditCommandException($"Please /copy a selection first!");
		}

		protected override void Execute()
		{
			AffineTransform transform = new AffineTransform();
			float radians = Mathf.DegToRad(this.angleDegrees);
			transform = transform.RotateY(radians);

			List<WorldEditBlock> blocks = new List<WorldEditBlock>();
			blocks.AddRange(this.UserSession.Clipboard.GetBlocks());
			blocks.AddRange(this.UserSession.Clipboard.GetPlants());
			blocks.AddRange(this.UserSession.Clipboard.GetWorldObjects());

			for (int i = 0; i < blocks.Count; i++)
			{
				WorldEditBlock block = blocks[i];
				block.RotateBlock(transform, this.angleDegrees, radians);
				blocks[i] = block;
			}

			this.UserSession.Clipboard.Clear();
			this.UserSession.Clipboard.Parse(blocks);
		}
	}
}
