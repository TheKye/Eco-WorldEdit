﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Eco.Gameplay.Blocks;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation.Agents;
using Eco.World;
using Eco.World.Blocks;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model
{
	internal struct WorldEditBlock
	{
		public Type BlockType { get; private set; }
		[JsonConverter(typeof(JsonVector3iConverter))] public Vector3i Position { get; private set; }
		[JsonIgnore] public Vector3i OriginalPosition { get; private set; }
		[JsonIgnore] public Vector3i OffsetPosition { get; private set; }
		[JsonConverter(typeof(JsonWorldEditBlockDataConverter))] public IWorldEditBlockData BlockData { get; private set; }

		public static WorldEditBlock Create(Block block, Vector3i originalPosition, Vector3i offsetPosition)
		{
			Vector3i relativePosition = originalPosition - offsetPosition;

			WorldEditBlock worldEditBlock = new WorldEditBlock();
			worldEditBlock.Position = relativePosition;
			worldEditBlock.OffsetPosition = offsetPosition;
			worldEditBlock.OriginalPosition = originalPosition;
			worldEditBlock.BlockType = block.GetType();


			switch (block)
			{
				case PlantBlock plantBlock:
					//Log.Debug($"{worldEditBlock.BlockType.ToString()} is a PlantBlock");
					Plant plant = PlantBlock.GetPlant(worldEditBlock.OriginalPosition);
					worldEditBlock.BlockData = WorldEditPlantBlockData.From(plant);
					break;
				case WorldObjectBlock objectBlock:
					//Log.Debug($"{worldEditBlock.BlockType.ToString()} is a WorldObjectBlock");
					WorldObject worldObject = objectBlock.WorldObjectHandle.Object;
					worldEditBlock.BlockData = WorldEditWorldObjectBlockData.From(worldObject);
					relativePosition = worldObject.Position3i - offsetPosition;
					worldEditBlock.Position = relativePosition;
					break;
				case EmptyBlock emptyBlock:
					//Log.Debug($"{worldEditBlock.BlockType.ToString()} is a EmptyBlock");
					break;
				default:
					//Log.Debug($"{worldEditBlock.BlockType.ToString()} is a Block");
					System.Reflection.ConstructorInfo constuctor = worldEditBlock.BlockType.GetConstructor(Type.EmptyTypes);
					if (constuctor == null) { throw new ArgumentOutOfRangeException(message: "Block type is not supported", paramName: worldEditBlock.BlockType.ToString()); }
					if (BlockContainerManager.Obj.IsBlockContained(originalPosition))
					{
						worldEditBlock.BlockType = typeof(EmptyBlock);
					}
					break;
			}

			return worldEditBlock;
		}

		public static WorldEditBlock CreateEmpty(Vector3i originalPosition, Vector3i offsetPosition)
		{
			Vector3i relativePosition = originalPosition - offsetPosition;
			WorldEditBlock worldEditBlock = new WorldEditBlock();
			worldEditBlock.Position = relativePosition;
			worldEditBlock.OffsetPosition = offsetPosition;
			worldEditBlock.OriginalPosition = originalPosition;
			worldEditBlock.BlockType = typeof(EmptyBlock);
			return worldEditBlock;
		}

		public static WorldEditBlock Create(Block block, Vector3i originalPosition)
		{
			return Create(block, originalPosition, Vector3i.Zero);
		}

		[JsonConstructor]
		public WorldEditBlock(Type blockType, Vector3i position, IWorldEditBlockData blockData) : this()
		{
			this.BlockType = blockType ?? throw new ArgumentNullException(nameof(blockType));
			this.Position = position;
			this.BlockData = blockData;
		}

		public WorldEditBlock(Type blockType, Vector3i position, Vector3i originalPosition, IWorldEditBlockData blockData) : this()
		{
			this.BlockType = blockType ?? throw new ArgumentNullException(nameof(blockType));
			this.Position = position;
			this.OriginalPosition = originalPosition;
			this.BlockData = blockData;
		}

		public void RotateBlock(AffineTransform transform, float degrees, float radians)
		{
			this.Position = transform.Apply(this.Position);
			if (this.IsWorldObjectBlock())
			{
				WorldEditWorldObjectBlockData worldObjectBlockData = (WorldEditWorldObjectBlockData)this.BlockData;
				worldObjectBlockData.SetRotation(worldObjectBlockData.Rotation * QuaternionUtils.FromAxisAngle(Vector3.Up, radians));
				this.BlockData = worldObjectBlockData;
			}
			else if (!this.IsPlantBlock() && !this.BlockType.Equals(typeof(EmptyBlock)))
			{
				if (BlockUtils.HasRotatedVariants(this.BlockType, out Type[] variants))
				{
					int currentAngle = 0;
					string angleStr = Regex.Match(this.BlockType.Name, @"\d+").Value;
					if (!string.IsNullOrEmpty(angleStr)) { currentAngle = int.Parse(angleStr); }

					float newAngle = Mathf.Round((currentAngle + degrees) / 90) * 90;
					newAngle = MathUtil.NormalizeAngle0to360(newAngle);

					Type type = variants.Single(variant =>
					{
						string angleStr = Regex.Match(variant.Name, @"\d+").Value;
						if (string.IsNullOrEmpty(angleStr) && newAngle == 0) return true;
						if (!string.IsNullOrEmpty(angleStr) && newAngle == int.Parse(angleStr)) return true;
						return false;
					});

					if (type == null) { Log.Debug($"{this.BlockType.Name} new form: {type.Name}"); return; }
					this.BlockType = type;
				}
			}
		}

		public bool IsPlantBlock()
		{
			return this.BlockType.DerivesFrom<PlantBlock>();
		}

		public bool IsWorldObjectBlock()
		{
			return this.BlockType.DerivesFrom<WorldObjectBlock>();
		}

		public WorldEditBlock Clone()
		{
			return new WorldEditBlock(this.BlockType, this.Position, this.OriginalPosition, this.BlockData.Clone());
		}
	}
}