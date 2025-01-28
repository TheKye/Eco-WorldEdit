using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World.Color;
using System.Collections.Generic;

namespace Eco.Mods.WorldEdit.Commands
{
	using Eco.World;

	internal class ColorCommand : WorldEditCommand
	{
		private ByteColor _color;

		public ColorCommand(User user, ByteColor color) : base(user)
		{
			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			this._color = color;
		}

		protected override void Execute(WorldRange selection)
		{
			selection = selection.FixXZ(Shared.Voxel.World.VoxelSize);
			List<WrappedWorldPosition3i> paintedPositions = new();

			void PaintBlocks(Vector3i pos)
			{
				if (WrappedWorldPosition3i.TryCreate(pos, out WrappedWorldPosition3i wrappedWorldPos))
				{
					BlockColorManager.Obj.SetColor(pos, this._color);
					paintedPositions.Add(wrappedWorldPos);
				}
			}
			if (this._color == ByteColor.Clear)
			{
				BlockColorManager.Obj.ClearColors(selection.XYZIterInc(), true);
			}
			else
			{
				selection.ForEachInc(PaintBlocks);
				World.ForceUpdateBatch(paintedPositions);
			}
		}
	}
}
