using System;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Eco.Gameplay.Blocks;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared;
using Eco.Shared.IoC;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Agents;
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
				case TreeBlock treeBlock:
					//Log.Debug($"{worldEditBlock.BlockType.ToString()} at {originalPosition} is a PlantBlock or TreeBlock");
					Plant plant = EcoSim.PlantSim.GetPlant(originalPosition);
					if (plant != null)
					{
						worldEditBlock.Position = plant.Position.XYZi() - offsetPosition;
						worldEditBlock.BlockData = WorldEditPlantBlockData.From(plant);
					}
					else { worldEditBlock.BlockType = typeof(EmptyBlock); }
					break;
				case WorldObjectBlock objectBlock:
					//Log.Debug($"{worldEditBlock.BlockType.ToString()} at {originalPosition} is a WorldObjectBlock");
					WorldObject worldObject = objectBlock.WorldObjectHandle.Object;
					worldEditBlock.BlockData = WorldEditWorldObjectBlockData.From(worldObject);
					relativePosition = worldObject.Position3i - offsetPosition;
					worldEditBlock.Position = relativePosition;
					break;
				case EmptyBlock emptyBlock:
					//Log.Debug($"{worldEditBlock.BlockType.ToString()} at {originalPosition} is a EmptyBlock");
					break;
				default:
					//Log.Debug($"{worldEditBlock.BlockType.ToString()} at {originalPosition} is a Block");
					System.Reflection.ConstructorInfo constuctor = worldEditBlock.BlockType.GetConstructor(Type.EmptyTypes);
					if (constuctor == null) { throw new ArgumentOutOfRangeException(message: "Block type is not supported", paramName: worldEditBlock.BlockType.FullName); }
					if (BlockContainerManager.Obj.IsBlockContained(originalPosition))
					{
						worldEditBlock.BlockType = typeof(EmptyBlock);
						WorldObject obj = ServiceHolder<IWorldObjectManager>.Obj.All.Where(x => x.Position3i.Equals(originalPosition)).FirstOrDefault();
						if (obj != null)
						{
							worldEditBlock.BlockType = typeof(WorldObjectBlock);
							worldEditBlock.BlockData = WorldEditWorldObjectBlockData.From(obj);
							relativePosition = obj.Position3i - offsetPosition;
							worldEditBlock.Position = relativePosition;
						}
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
				worldObjectBlockData.SetRotation(worldObjectBlockData.Rotation * QuaternionUtils.FromAxisAngle(Vector3.UnitY, radians));
				this.BlockData = worldObjectBlockData;
			}
			else if (!this.IsPlantBlock() && !this.BlockType.Equals(typeof(EmptyBlock)))
			{
				if (BlockUtils.HasRotatedVariants(this.BlockType, out Type[] variants))
				{
					int currentAngle = 0;
					string angleStr = Regex.Match(this.BlockType.Name, @"\d+").Value;
					if (!string.IsNullOrEmpty(angleStr)) { currentAngle = int.Parse(angleStr); }

					float newAngle = MathF.Round((currentAngle + degrees) / 90) * 90;
					newAngle = MathUtil.NormalizeAngle0to360(newAngle);

					Type type = variants.FirstOrDefault(variant =>
					{
						string angleStr = Regex.Match(variant.Name, @"\d+").Value;
						if (string.IsNullOrEmpty(angleStr) && newAngle == 0) return true;
						if (!string.IsNullOrEmpty(angleStr) && newAngle == int.Parse(angleStr)) return true;
						return false;
					});

					if(type is null) //Default number search not working, trying new name extraction based
					{
						string baseBlockName = variants[0].Name;
						string baseName = baseBlockName.Substring(0, baseBlockName.LastIndexOf("Block"));
						string plainCurName = this.BlockType.Name.Substring(0, this.BlockType.Name.LastIndexOf("Block"));
						//Determine angle again
						currentAngle = 0;
						angleStr = plainCurName.Substring(baseName.Length);
						if (!string.IsNullOrEmpty(angleStr)) { currentAngle = int.Parse(angleStr); }
						newAngle = MathF.Round((currentAngle + degrees) / 90) * 90;
						newAngle = MathUtil.NormalizeAngle0to360(newAngle);
						//
						string constructedName = $"{baseName}{(int)newAngle}Block";
						type = variants.FirstOrDefault(v => v.Name == $"{baseName}{(int)newAngle}Block" || (newAngle == 0 && v.Name == baseBlockName));
					}
					if (type == null) { Log.WriteWarningLineLoc($"{this.BlockType.Name} unable to rotate this block!"); return; }
					this.BlockType = type;
				}
			}
		}

		public bool IsPlantBlock() => this.BlockType.DerivesFrom<PlantBlock>() || this.BlockType.DerivesFrom<TreeBlock>();
		public bool IsWorldObjectBlock() => this.BlockType.DerivesFrom<WorldObjectBlock>();
		public bool IsEmptyBlock() => this.BlockType.Equals(typeof(EmptyBlock));

		public WorldEditBlock Clone()
		{
			return new WorldEditBlock(this.BlockType, this.Position, this.OriginalPosition, this.BlockData.Clone());
		}
	}
}
