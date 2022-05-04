using System.Collections.Generic;
using System.Linq;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit
{
	internal class WorldEditClipboard
	{
		private Dictionary<Vector3i, WorldEditBlock> Blocks { get; }
		private Dictionary<Vector3i, WorldEditBlock> Plants { get; }
		private Dictionary<Vector3i, WorldEditBlock> WorldObjects { get; }

		public int Count => this.Blocks.Count + this.Plants.Count + this.WorldObjects.Count;
		/// <summary>Dimension in Width, Height, Length. Zero vector if not provided.</summary>
		public Vector3i Dimension { get; internal set; }
		public AuthorInformation AuthorInfo { get; private set; }

		public List<WorldEditBlock> GetBlocks() => this.Blocks.Values.ToList();
		public List<WorldEditBlock> GetPlants() => this.Plants.Values.ToList();
		public List<WorldEditBlock> GetWorldObjects() => this.WorldObjects.Values.ToList();

		public WorldEditClipboard()
		{
			this.Blocks = new Dictionary<Vector3i, WorldEditBlock>();
			this.Plants = new Dictionary<Vector3i, WorldEditBlock>();
			this.WorldObjects = new Dictionary<Vector3i, WorldEditBlock>();
		}

		public WorldEditClipboard(Dictionary<Vector3i, WorldEditBlock> blocks, Dictionary<Vector3i, WorldEditBlock> plants, Dictionary<Vector3i, WorldEditBlock> objects)
		{
			this.Blocks = new Dictionary<Vector3i, WorldEditBlock>(blocks);
			this.Plants = new Dictionary<Vector3i, WorldEditBlock>(plants);
			this.WorldObjects = new Dictionary<Vector3i, WorldEditBlock>(objects);
		}

		public void Add(WorldEditBlock worldEditBlock)
		{
			if (worldEditBlock.IsPlantBlock())
			{
				if (this.Plants.ContainsKey(worldEditBlock.Position)) return;
				this.Add(this.Plants, worldEditBlock);
			}
			else if (worldEditBlock.IsWorldObjectBlock())
			{
				if (this.WorldObjects.ContainsKey(worldEditBlock.Position)) return;
				this.Add(this.WorldObjects, worldEditBlock);
			}
			else
			{
				if (this.Blocks.ContainsKey(worldEditBlock.Position)) return;
				this.Add(this.Blocks, worldEditBlock);
			}
		}

		private void Add(Dictionary<Vector3i, WorldEditBlock> list, WorldEditBlock worldEditBlock)
		{
			if (!list.TryAdd(worldEditBlock.Position, worldEditBlock))
			{
				Log.WriteLine(Localizer.Do($"Unable add {worldEditBlock.Position} to {list}"));
			}
		}

		[System.Obsolete]
		public void Rotate(float degrees)
		{
			AffineTransform transform = new AffineTransform();
			float radians = Mathf.DegToRad(degrees);
			transform = transform.RotateY(radians);

			List<WorldEditBlock> blocks = new List<WorldEditBlock>();
			blocks.AddRange(this.GetBlocks());
			blocks.AddRange(this.GetPlants());
			blocks.AddRange(this.GetWorldObjects());

			for (int i = 0; i < blocks.Count; i++)
			{
				WorldEditBlock block = blocks[i];
				block.RotateBlock(transform, degrees, radians);
				blocks[i] = block;
			}

			this.Clear();
			this.Parse(blocks);
		}

		public void Parse(WorldEditSerializer serializer)
		{
			this.Clear();
			this.Dimension = serializer.Dimension;
			this.AuthorInfo = serializer.AuthorInformation;
			this.Parse(serializer.BlockList);
			this.Parse(serializer.PlantList);
			this.Parse(serializer.WorldObjectList);
		}

		public void Parse(List<WorldEditBlock> blocks)
		{
			foreach (WorldEditBlock block in blocks)
			{
				this.Add(block);
			}
		}

		public void Clear()
		{
			this.Blocks.Clear();
			this.Plants.Clear();
			this.WorldObjects.Clear();
		}

		public void SetAuthor(AuthorInformation information)
		{
			this.AuthorInfo = information;
		}

		public WorldEditClipboard Copy()
		{
			return new WorldEditClipboard(this.Blocks, this.Plants, this.WorldObjects);
		}
	}
}
