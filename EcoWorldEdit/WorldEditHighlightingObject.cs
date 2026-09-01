using Eco.Core.Controller;
using Eco.Gameplay.Objects;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Shared.Math;
using Eco.Shared.Serialization;
using Vector3 = System.Numerics.Vector3;

namespace Eco.Mods.WorldEdit
{
	[Serialized]
	internal sealed class WorldEditHighlightingObject : WorldObject
	{
		public const int MaximumAnimatedSize = 8192;

		static WorldEditHighlightingObject()
		{
			AddOccupancy<WorldEditHighlightingObject>([]);
		}

		public void SetHighlightArea(Vector3i firstPosition, Vector3i secondPosition) => this.SetHighlightArea(new WorldRange(firstPosition, secondPosition));

		public void SetHighlightArea(WorldRange selection)
		{
			if (!selection.IsSet()) throw new ArgumentException("Both selection positions must be set.", nameof(selection));

			WorldRange range = selection.FixToWorldSize();
			Vector3 size = new(range.WidthInc, range.HeightInc, range.LengthInc);
			if (size.X > MaximumAnimatedSize || size.Y > MaximumAnimatedSize || size.Z > MaximumAnimatedSize) throw new ArgumentOutOfRangeException(nameof(selection), selection, $"Highlight dimensions cannot exceed {MaximumAnimatedSize} blocks per axis.");

			// Eco block coordinates point to block centers, while the client mesh grows from its
			// lower corner. Offset by half a block so the visual volume covers both endpoints.
			Vector3 origin = new(range.min.x - 0.5f, range.min.y - 0.5f, range.min.z - 0.5f);
			this.SetHighlightArea(origin, size);
		}

		private void SetHighlightArea(Vector3 origin, Vector3 size)
		{
			this.Position = origin;
			this.Rotation = Quaternion.Identity;

			this.SetAnimatedState("SizeX", size.X);
			this.SetAnimatedState("SizeY", size.Y);
			this.SetAnimatedState("SizeZ", size.Z);

			//Notify and force client sync
			this.Changed(nameof(WorldObject.LastSyncedPosition));
			this.SyncPositionAndRotation();
			this.SetDirty();
		}
	}
}
