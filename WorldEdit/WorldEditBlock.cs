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
                xPos = pPosition.X,
                yPos = pPosition.Y,
                zPos = pPosition.Z,
                Type = pBlock.GetType(),
                Position = new Vector3i(xPos, yPos, zPos)
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
        [Serialized] internal int xPos { get; set; }
        [Serialized] internal int yPos { get; set; }
        [Serialized] internal int zPos { get; set; }

        public WorldEditBlock(Type pType, Vector3i pPosition, byte[] data) : this()
        {
            Type = pType;
            Data = data;
            xPos = pPosition.X;
            yPos = pPosition.Y;
            zPos = pPosition.Z;
            Position = pPosition;
        }

        public WorldEditBlock Clone()
        {
            return new WorldEditBlock(Type, new Vector3i(xPos, yPos, zPos), Data);
        }
    }
}
