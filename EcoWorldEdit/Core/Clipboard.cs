using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Core
{
	internal sealed class Clipboard
	{
		private readonly List<WorldEditBlock> _blocks;
		private readonly List<WorldEditBlock> _plants;
		private readonly List<WorldEditBlock> _worldObjects;

		public IReadOnlyList<WorldEditBlock> Blocks => this._blocks;
		public IReadOnlyList<WorldEditBlock> Plants => this._plants;
		public IReadOnlyList<WorldEditBlock> WorldObjects => this._worldObjects;
		/// <summary>Dimension in Width, Height, Length. Zero vector if not provided.</summary>
		public Vector3i Dimension { get; }
		public AuthorInformation Author { get; }
		public int Count { get; }

		public static Clipboard Empty { get; } = new([], [], [], AuthorInformation.Unowned(), Vector3i.Zero);

		private Clipboard(List<WorldEditBlock> blocks, List<WorldEditBlock> plants, List<WorldEditBlock> worldObjects, AuthorInformation authorInformation, Vector3i dimension)
		{
			this._blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
			this._plants = plants ?? throw new ArgumentNullException(nameof(plants));
			this._worldObjects = worldObjects ?? throw new ArgumentNullException(nameof(worldObjects));
			this.Author = authorInformation ?? throw new ArgumentNullException(nameof(authorInformation));
			this.Dimension = dimension;
			this.Count = this._blocks.Count + this._plants.Count + this._worldObjects.Count;
		}

		/// <summary>
		/// Creates a clipboard that takes ownership of the supplied collections.
		/// The collections must not be modified after this!
		/// </summary>
		public static Clipboard Create(List<WorldEditBlock> blocks, List<WorldEditBlock> plants, List<WorldEditBlock> worldObjects, AuthorInformation author, Vector3i dimension)
		{
			return new Clipboard(
				blocks,
				plants,
				worldObjects,
				author,
				dimension);
		}

		public static Clipboard Create(EcoBlueprint blueprint)
		{
			ArgumentNullException.ThrowIfNull(blueprint);

			return Create(
				blueprint.Blocks,
				blueprint.Plants,
				blueprint.Objects,
				blueprint.Author,
				blueprint.Dimension);
		}

		public EcoBlueprint ToBlueprint()
		{
			return EcoBlueprint.Create(
				this._blocks,
				this._plants,
				this._worldObjects,
				this.Author,
				this.Dimension);
		}
	}
}
