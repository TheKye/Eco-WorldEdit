using Eco.Gameplay.Objects;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	/// <summary>State shared by all block captures that belong to one operation.</summary>
	internal sealed class BlockCaptureContext
	{
		private readonly Dictionary<WorldObject, Guid> _objectIds = new(ReferenceEqualityComparer.Instance);
		private readonly HashSet<WorldObject> _capturedObjects = new(ReferenceEqualityComparer.Instance);
		private readonly HashSet<WorldObject> _ignoredObjects = new(ReferenceEqualityComparer.Instance);

		public Guid GetObjectId(WorldObject worldObject)
		{
			ArgumentNullException.ThrowIfNull(worldObject);
			if (!this._objectIds.TryGetValue(worldObject, out Guid objectId))
			{
				objectId = Guid.NewGuid();
				this._objectIds.Add(worldObject, objectId);
			}
			return objectId;
		}

		public bool TryCapture(WorldObject worldObject)
		{
			ArgumentNullException.ThrowIfNull(worldObject);
			return this._capturedObjects.Add(worldObject);
		}

		public bool IsIgnored(WorldObject worldObject)
		{
			ArgumentNullException.ThrowIfNull(worldObject);
			return this._ignoredObjects.Contains(worldObject);
		}

		public void Ignore(WorldObject worldObject)
		{
			ArgumentNullException.ThrowIfNull(worldObject);
			this._ignoredObjects.Add(worldObject);
		}
	}
}
