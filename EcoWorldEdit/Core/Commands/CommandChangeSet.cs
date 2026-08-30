using System.Numerics;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Core.Commands
{
	internal sealed class CommandChangeSet
	{
		private readonly List<WorldEditBlock> _affectedBlocks = new();
		private readonly HashSet<Vector3i> _capturedPositions = new();
		private readonly HashSet<Vector3> _changedPositions = new();

		public IReadOnlyList<WorldEditBlock> AffectedBlocks => this._affectedBlocks;
		public int ChangedBlocks => this._changedPositions.Count;
		public IEnumerable<Vector3i> AffectedPositions => this._capturedPositions;

		/// <summary>Returns true only for the first capture of a world position.</summary>
		public bool TryBeginCapture(Vector3i position) => this._capturedPositions.Add(position);

		public void AddCapturedBlocks(IEnumerable<WorldEditBlock> blocks)
		{
			ArgumentNullException.ThrowIfNull(blocks);
			this._affectedBlocks.AddRange(blocks);
		}

		public void RegisterChangedBlock(Vector3 position) => this._changedPositions.Add(position);
	}
}
