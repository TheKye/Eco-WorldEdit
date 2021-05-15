using Eco.Core.Serialization;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Shared.Math;
using Eco.Shared.Serialization;
using Eco.World;
using System;

namespace Eco.Mods.WorldEdit
{
    [Serialized]
    public struct WorldEditBlock
    {
        public static WorldEditBlock API { get; set; }

        public WorldEditBlock CreateNew(Block pBlock, Vector3i pPosition, Vector3i? pSourcePosition)
        {
            WorldEditBlock web = new WorldEditBlock
            {
                Type = pBlock.GetType(),
                Position = pPosition,
            };

            var constuctor = web.Type.GetConstructor(Type.EmptyTypes);

            if (constuctor == null)
            {
                if (pBlock is PlantBlock)
                    web.Data = EcoSerializer.Serialize(PlantBlock.GetPlant(web.Position)).ToArray();

                if (pBlock is WorldObjectBlock)
                {
                    WorldObject wob = (WorldObjectBlock)pBlock;

                    if (wob.Position3i == pSourcePosition)
                    {
                        web.Data = EcoSerializer.Serialize(wob).ToArray();
                    }
                }
            }

            return web;
        }

        [Serialized] public Vector3i Position { get; set; }
        [Serialized] public byte[] Data { get; set; }
        [Serialized] public Type Type;

        public WorldEditBlock(Type pType, Vector3i pPosition, byte[] data) : this()
        {
            Type = pType;
            Position = pPosition;
            Data = data;
        }

        public WorldEditBlock Clone()
        {
            return new WorldEditBlock(Type, Position, Data);
        }
    }
}
